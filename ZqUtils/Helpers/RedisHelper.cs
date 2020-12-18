#region License
/***
 * Copyright © 2018-2020, 张强 (943620963@qq.com).
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * without warranties or conditions of any kind, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#endregion

using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ZqUtils.Extensions;
/****************************
* [Author] 张强
* [Date] 2018-03-21
* [Describe] Redis工具类
* **************************/
namespace ZqUtils.Helpers
{
    /// <summary>
    /// Redis帮助工具类
    /// </summary>
    public class RedisHelper
    {
        #region 私有字段
        /// <summary>
        /// 线程锁
        /// </summary>
        private static readonly SemaphoreSlim locker = new SemaphoreSlim(1, 1);

        /// <summary>
        /// redis连接对象
        /// </summary>
        private static IConnectionMultiplexer connectionMultiplexer;
        #endregion

        #region 公有属性
        /// <summary>
        /// 静态单例，注意单例对象慎重修改对象公有属性，建议不进行修改操作
        /// </summary>
        public static RedisHelper Instance => SingletonHelper<RedisHelper>.GetInstance();

        /// <summary>
        /// IConnectionMultiplexer对象
        /// </summary>
        public IConnectionMultiplexer IConnectionMultiplexer => connectionMultiplexer;

        /// <summary>
        /// 数据库，注意单例对象不建议修改
        /// </summary>
        public IDatabase Database { get; set; }

        /// <summary>
        /// RedisKey的前缀，注意单例对象不建议修改
        /// </summary>
        public string KeyPrefix { get; set; } = ConfigHelper.GetAppSettings<string>("Redis.KeyPrefix");
        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        public RedisHelper()
        {
            Database = GetConnectionRedisMultiplexer().GetDatabase();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="defaultDatabase">数据库索引</param>
        public RedisHelper(int defaultDatabase)
        {
            Database = GetConnectionRedisMultiplexer().GetDatabase(defaultDatabase);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="redisConnectionString">redis连接字符串</param>
        public RedisHelper(string redisConnectionString)
        {
            Database = GetConnectionRedisMultiplexer(redisConnectionString).GetDatabase();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="redisConnectionString">redis连接字符串</param>
        ///  <param name="defaultDatabase">数据库索引</param>
        public RedisHelper(string redisConnectionString, int defaultDatabase)
        {
            Database = GetConnectionRedisMultiplexer(redisConnectionString).GetDatabase(defaultDatabase);
        }

        /// <summary>
        /// 构造函数
        /// </summary>        
        /// <param name="configurationOptions">连接配置</param>
        public RedisHelper(ConfigurationOptions configurationOptions)
        {
            Database = GetConnectionRedisMultiplexer(configurationOptions).GetDatabase();
        }
        #endregion 构造函数

        #region 连接对象
        #region GetConnectionRedisMultiplexer
        /// <summary>
        /// 获取redis连接对象
        /// </summary>
        /// <returns>返回IConnectionMultiplexer</returns>
        public static IConnectionMultiplexer GetConnectionRedisMultiplexer()
        {
            if (connectionMultiplexer == null || !connectionMultiplexer.IsConnected)
            {
                try
                {
                    locker.Wait();

                    if (connectionMultiplexer == null || !connectionMultiplexer.IsConnected)
                    {
                        var connectionStr = ConfigHelper.GetConnectionString("RedisConnectionStrings");

                        if (string.IsNullOrEmpty(connectionStr))
                            connectionStr = ConfigHelper.GetAppSettings<string>("RedisConnectionStrings");

                        if (!string.IsNullOrEmpty(connectionStr))
                            connectionMultiplexer = ConnectionMultiplexer.Connect(connectionStr);

                        if (string.IsNullOrEmpty(connectionStr))
                            throw new ArgumentNullException("Redis连接字符串配置为null");

                        AddRegisterEvent();
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    locker.Release();
                }
            }
            return connectionMultiplexer;
        }

        /// <summary>
        /// 获取redis连接对象
        /// </summary>
        /// <param name="redisConnectionString">redis连接字符串</param>
        /// <returns>返回IConnectionMultiplexer</returns>
        public static IConnectionMultiplexer GetConnectionRedisMultiplexer(string redisConnectionString)
        {
            if (connectionMultiplexer == null || !connectionMultiplexer.IsConnected)
            {
                try
                {
                    locker.Wait();

                    if (connectionMultiplexer == null || !connectionMultiplexer.IsConnected)
                    {
                        connectionMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString);

                        AddRegisterEvent();
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    locker.Release();
                }
            }
            return connectionMultiplexer;
        }

        /// <summary>
        /// 获取redis连接对象
        /// </summary>
        /// <param name="configurationOptions">连接配置</param>
        /// <returns>返回IConnectionMultiplexer</returns>
        public static IConnectionMultiplexer GetConnectionRedisMultiplexer(ConfigurationOptions configurationOptions)
        {
            if (connectionMultiplexer == null || !connectionMultiplexer.IsConnected)
            {
                try
                {
                    locker.Wait();

                    if (connectionMultiplexer == null || !connectionMultiplexer.IsConnected)
                    {
                        connectionMultiplexer = ConnectionMultiplexer.Connect(configurationOptions);

                        AddRegisterEvent();
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    locker.Release();
                }
            }
            return connectionMultiplexer;
        }
        #endregion

        #region GetConnectionRedisMultiplexerAsync
        /// <summary>
        /// 获取redis连接对象
        /// </summary>
        /// <returns>返回IConnectionMultiplexer</returns>
        public static async Task<IConnectionMultiplexer> GetConnectionRedisMultiplexerAsync()
        {
            if (connectionMultiplexer == null || !connectionMultiplexer.IsConnected)
            {
                try
                {
                    await locker.WaitAsync().ConfigureAwait(false);

                    if (connectionMultiplexer == null || !connectionMultiplexer.IsConnected)
                    {
                        var connectionStr = ConfigHelper.GetConnectionString("RedisConnectionStrings");

                        if (string.IsNullOrEmpty(connectionStr))
                            connectionStr = ConfigHelper.GetAppSettings<string>("RedisConnectionStrings");

                        if (!string.IsNullOrEmpty(connectionStr))
                            connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(connectionStr);

                        if (string.IsNullOrEmpty(connectionStr))
                            throw new ArgumentNullException("Redis连接字符串配置为null");

                        AddRegisterEvent();
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    locker.Release();
                }
            }

            return connectionMultiplexer;
        }

        /// <summary>
        /// 获取redis连接对象
        /// </summary>
        /// <param name="redisConnectionString">redis连接字符串</param>
        /// <returns>返回IConnectionMultiplexer</returns>
        public static async Task<IConnectionMultiplexer> GetConnectionRedisMultiplexerAsync(string redisConnectionString)
        {
            if (connectionMultiplexer == null || !connectionMultiplexer.IsConnected)
            {
                try
                {
                    await locker.WaitAsync().ConfigureAwait(false);

                    if (connectionMultiplexer == null || !connectionMultiplexer.IsConnected)
                    {
                        connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(redisConnectionString);

                        AddRegisterEvent();
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    locker.Release();
                }
            }

            return connectionMultiplexer;
        }

        /// <summary>
        /// 获取redis连接对象
        /// </summary>
        /// <param name="configurationOptions">连接配置</param>
        /// <returns>返回IConnectionMultiplexer</returns>
        public static async Task<IConnectionMultiplexer> GetConnectionRedisMultiplexerAsync(ConfigurationOptions configurationOptions)
        {
            if (connectionMultiplexer == null || !connectionMultiplexer.IsConnected)
            {
                try
                {
                    await locker.WaitAsync().ConfigureAwait(false);

                    if (connectionMultiplexer == null || !connectionMultiplexer.IsConnected)
                    {
                        connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(configurationOptions);

                        AddRegisterEvent();
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    locker.Release();
                }
            }

            return connectionMultiplexer;
        }
        #endregion

        #region SetConnectionRedisMultiplexerAsync
        /// <summary>
        /// 初始化IConnectionMultiplexer
        /// </summary>
        /// <returns></returns>
        public static async Task SetConnectionRedisMultiplexerAsync()
        {
            connectionMultiplexer = await GetConnectionRedisMultiplexerAsync();
        }

        /// <summary>
        /// 初始化IConnectionMultiplexer
        /// </summary>
        /// <param name="redisConnectionString"></param>
        /// <returns></returns>
        public static async Task SetConnectionRedisMultiplexerAsync(string redisConnectionString)
        {
            connectionMultiplexer = await GetConnectionRedisMultiplexerAsync(redisConnectionString);
        }

        /// <summary>
        /// 初始化IConnectionMultiplexer
        /// </summary>
        /// <param name="configurationOptions"></param>
        /// <returns></returns>
        public static async Task SetConnectionRedisMultiplexerAsync(ConfigurationOptions configurationOptions)
        {
            connectionMultiplexer = await GetConnectionRedisMultiplexerAsync(configurationOptions);
        }
        #endregion
        #endregion

        #region Redis事务
        /// <summary>
        /// redis事务
        /// </summary>
        /// <returns>返回ITransaction</returns>
        public ITransaction GetTransaction()
        {
            return Database.CreateTransaction();
        }
        #endregion

        #region 注册事件
        /// <summary>
        /// 添加注册事件
        /// </summary>
        private static void AddRegisterEvent()
        {
            if (ConfigHelper.GetAppSettings<bool>("Redis.RegisterEvent"))
            {
                connectionMultiplexer.ConnectionRestored += ConnMultiplexer_ConnectionRestored;
                connectionMultiplexer.ConnectionFailed += ConnMultiplexer_ConnectionFailed;
                connectionMultiplexer.ErrorMessage += ConnMultiplexer_ErrorMessage;
                connectionMultiplexer.ConfigurationChanged += ConnMultiplexer_ConfigurationChanged;
                connectionMultiplexer.HashSlotMoved += ConnMultiplexer_HashSlotMoved;
                connectionMultiplexer.InternalError += ConnMultiplexer_InternalError;
                connectionMultiplexer.ConfigurationChangedBroadcast += ConnMultiplexer_ConfigurationChangedBroadcast;
            }
        }

        /// <summary>
        /// 重新配置广播时（通常意味着主从同步更改）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void ConnMultiplexer_ConfigurationChangedBroadcast(object sender, EndPointEventArgs e)
        {
            LogHelper.Info($"{nameof(ConnMultiplexer_ConfigurationChangedBroadcast)}: {e.EndPoint}");
        }

        /// <summary>
        /// 发生内部错误时（主要用于调试）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void ConnMultiplexer_InternalError(object sender, InternalErrorEventArgs e)
        {
            LogHelper.Error(e.Exception, $"{nameof(ConnMultiplexer_InternalError)}: {e.Exception?.Message}");
        }

        /// <summary>
        /// 更改集群时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void ConnMultiplexer_HashSlotMoved(object sender, HashSlotMovedEventArgs e)
        {
            LogHelper.Info($"{nameof(ConnMultiplexer_HashSlotMoved)}: {nameof(e.OldEndPoint)}-{e.OldEndPoint} To {nameof(e.NewEndPoint)}-{e.NewEndPoint}, ");
        }

        /// <summary>
        /// 配置更改时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void ConnMultiplexer_ConfigurationChanged(object sender, EndPointEventArgs e)
        {
            LogHelper.Info($"{nameof(ConnMultiplexer_ConfigurationChanged)}: {e.EndPoint}");
        }

        /// <summary>
        /// 发生错误时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void ConnMultiplexer_ErrorMessage(object sender, RedisErrorEventArgs e)
        {
            LogHelper.Error($"{nameof(ConnMultiplexer_ErrorMessage)}: {e.Message}");
        }

        /// <summary>
        /// 物理连接失败时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void ConnMultiplexer_ConnectionFailed(object sender, ConnectionFailedEventArgs e)
        {
            LogHelper.Error(e.Exception, $"{nameof(ConnMultiplexer_ConnectionFailed)}: {e.Exception?.Message}");
        }

        /// <summary>
        /// 建立物理连接时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void ConnMultiplexer_ConnectionRestored(object sender, ConnectionFailedEventArgs e)
        {
            LogHelper.Error(e.Exception, $"{nameof(ConnMultiplexer_ConnectionRestored)}: {e.Exception?.Message}");
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 添加key的前缀
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>返回添加前缀后的key</returns>
        private string AddKeyPrefix(string key)
        {
            return string.IsNullOrEmpty(KeyPrefix) ? key : $"{KeyPrefix}:{key}";
        }
        #endregion

        #region 公有方法
        #region 切换IDatabase
        /// <summary>
        /// 切换数据库，注意单例对象慎用
        /// </summary>
        /// <param name="database">数据库索引</param>
        /// <returns></returns>
        public RedisHelper UseDatabase(int database)
        {
            if (connectionMultiplexer != null && connectionMultiplexer.IsConnected)
                Database = connectionMultiplexer.GetDatabase(database);

            return this;
        }
        #endregion

        #region 重置RedisKey前缀
        /// <summary>
        /// 重置RedisKey前缀，注意单例对象慎用
        /// </summary>
        /// <param name="keyPrefix"></param>
        /// <returns></returns>
        public RedisHelper ResetKeyPrefix(string keyPrefix = null)
        {
            KeyPrefix = keyPrefix;

            return this;
        }
        #endregion

        #region String操作
        #region 同步方法
        #region StringSet
        /// <summary>
        /// 保存字符串（若key已存在，则覆盖值）
        /// </summary>
        /// <param name="redisKey">字符串key</param>
        /// <param name="redisValue">字符串value</param>
        /// <param name="expiry">过期时间</param>
        /// <returns>返回是否保存成功</returns>
        public bool StringSet(string redisKey, string redisValue, TimeSpan? expiry = null)
        {
            redisKey = AddKeyPrefix(redisKey);
            return Database.StringSet(redisKey, redisValue, expiry);
        }

        /// <summary>
        /// 保存一组字符串
        /// </summary>
        /// <param name="keyValuePairs">字符串集合</param>
        /// <returns>返回是否保存成功</returns>
        public bool StringSet(IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            var pairs = keyValuePairs.Select(x => new KeyValuePair<RedisKey, RedisValue>(AddKeyPrefix(x.Key), x.Value));
            return Database.StringSet(pairs.ToArray());
        }

        /// <summary>
        /// 保存对象为字符串（若key已存在，则覆盖，该对象会被序列化）
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">字符串key</param>
        /// <param name="redisValue">字符串value</param>
        /// <param name="expiry">过期时间</param>
        /// <returns>返回是否保存成功</returns>
        public bool StringSet<T>(string redisKey, T redisValue, TimeSpan? expiry = null)
        {
            return StringSet(redisKey, redisValue.ToJson(), expiry);
        }
        #endregion

        #region StringGet
        /// <summary>
        /// 获取字符串值
        /// </summary>
        /// <param name="redisKey">字符串key</param>
        /// <returns>返回字符串值</returns>
        public string StringGet(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return Database.StringGet(redisKey);
        }

        /// <summary>
        /// 获取序反列化后的对象
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">字符串key</param>
        /// <returns>返回反序列化后的对象</returns>
        public T StringGet<T>(string redisKey)
        {
            return StringGet(redisKey).ToObject<T>();
        }
        #endregion
        #endregion

        #region 异步方法
        #region StringSetAsync
        /// <summary>
        /// 保存字符串（若key已存在，则覆盖）
        /// </summary>
        /// <param name="redisKey">字符串key</param>
        /// <param name="redisValue">字符串value</param>
        /// <param name="expiry">过期时间</param>
        /// <returns>返回是否保存成功</returns>
        public async Task<bool> StringSetAsync(string redisKey, string redisValue, TimeSpan? expiry = null)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await Database.StringSetAsync(redisKey, redisValue, expiry);
        }

        /// <summary>
        /// 保存一组字符串
        /// </summary>
        /// <param name="keyValuePairs">字符串集合</param>
        /// <returns>返回是否保存成功</returns>
        public async Task<bool> StringSetAsync(IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            var pairs = keyValuePairs.Select(x => new KeyValuePair<RedisKey, RedisValue>(AddKeyPrefix(x.Key), x.Value));
            return await Database.StringSetAsync(pairs.ToArray());
        }

        /// <summary>
        /// 保存对象为字符串（若key已存在，则覆盖，该对象会被序列化）
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">字符串key</param>
        /// <param name="redisValue">字符串value</param>
        /// <param name="expiry">过期时间</param>
        /// <returns>返回是否保存成功</returns>
        public async Task<bool> StringSetAsync<T>(string redisKey, T redisValue, TimeSpan? expiry = null)
        {
            return await StringSetAsync(redisKey, redisValue.ToJson(), expiry);
        }
        #endregion

        #region StringGetAsync
        /// <summary>
        /// 获取字符串值
        /// </summary>
        /// <param name="redisKey">字符串key</param>
        /// <returns>返回字符串值</returns>
        public async Task<string> StringGetAsync(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await Database.StringGetAsync(redisKey);
        }

        /// <summary>
        /// 获取序反列化后的对象
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">字符串key</param>
        /// <returns>返回反序列化后的对象</returns>
        public async Task<T> StringGetAsync<T>(string redisKey)
        {
            return (await StringGetAsync(redisKey)).ToObject<T>();
        }
        #endregion
        #endregion
        #endregion

        #region Hash操作
        #region 同步方法
        #region HashSet
        /// <summary>
        /// 保存hash字段
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="hashField">hash字段key</param>
        /// <param name="fieldValue">hash字段value</param>
        /// <returns>返回是否保存成功</returns>
        public bool HashSet(string redisKey, string hashField, string fieldValue)
        {
            redisKey = AddKeyPrefix(redisKey);
            return Database.HashSet(redisKey, hashField, fieldValue);
        }

        /// <summary>
        /// 保存hash字段
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="hashFields">hash字段key-value集合</param>
        public void HashSet(string redisKey, IEnumerable<KeyValuePair<string, string>> hashFields)
        {
            redisKey = AddKeyPrefix(redisKey);
            var entries = hashFields.Select(x => new HashEntry(x.Key, x.Value));
            Database.HashSet(redisKey, entries.ToArray());
        }

        /// <summary>
        /// 保存对象到hash中
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="hashField">hash字段key</param>
        /// <param name="fieldValue">hash字段value</param>
        /// <returns>返回是否保存成功</returns>
        public bool HashSet<T>(string redisKey, string hashField, T fieldValue)
        {
            return HashSet(redisKey, hashField, fieldValue.ToJson());
        }
        #endregion

        #region HashGet
        /// <summary>
        /// 获取hash字段值
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="hashField">hash字段key</param>
        /// <returns>返回hash字段值</returns>
        public string HashGet(string redisKey, string hashField)
        {
            redisKey = AddKeyPrefix(redisKey);
            return Database.HashGet(redisKey, hashField);
        }

        /// <summary>
        /// 获取hash中反序列化后的对象
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="hashField">hash字段key</param>
        /// <returns>返回反序列化后的对象</returns>
        public T HashGet<T>(string redisKey, string hashField)
        {
            return HashGet(redisKey, hashField).ToObject<T>();
        }

        /// <summary>
        /// 获取hash字段值反序列化后的对象集合
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">redis存储key</param>
        /// <returns>返回反序列化后的对象集合</returns>
        public IEnumerable<T> HashGet<T>(string redisKey)
        {
            var hashFields = HashKeys(redisKey);
            return HashGet<T>(redisKey, hashFields);
        }

        /// <summary>
        /// 获取hash字段值反序列化后的对象集合
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="hashFields">hash字段key集合</param>
        /// <returns>返回反序列化后的对象集合</returns>
        public IEnumerable<T> HashGet<T>(string redisKey, IEnumerable<string> hashFields)
        {
            redisKey = AddKeyPrefix(redisKey);
            var fields = hashFields.Select(x => (RedisValue)x);
            return Database.HashGet(redisKey, fields.ToArray()).Select(o => o.ToString().ToObject<T>());
        }
        #endregion

        #region HashDelete
        /// <summary>
        /// 从hash中移除指定字段
        /// </summary>
        /// <param name="redisKey">redis存储key</param>        
        /// <returns>返回是否删除成功</returns>
        public long HashDelete(string redisKey)
        {
            var hashFields = HashKeys(redisKey);
            return HashDelete(redisKey, hashFields);
        }

        /// <summary>
        /// 从hash中移除指定字段
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="hashField">hash字段key</param>
        /// <returns>返回是否删除成功</returns>
        public bool HashDelete(string redisKey, string hashField)
        {
            redisKey = AddKeyPrefix(redisKey);
            return Database.HashDelete(redisKey, hashField);
        }

        /// <summary>
        /// 从hash中移除指定字段
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="hashFields">hash字段key集合</param>
        /// <returns>返回是否删除成功</returns>
        public long HashDelete(string redisKey, IEnumerable<string> hashFields)
        {
            redisKey = AddKeyPrefix(redisKey);
            var fields = hashFields.Select(x => (RedisValue)x);
            return Database.HashDelete(redisKey, fields.ToArray());
        }
        #endregion

        #region HashExists
        /// <summary>
        /// 判断键值是否在hash中
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="hashField">hash字段key</param>
        /// <returns>返回是否存在</returns>
        public bool HashExists(string redisKey, string hashField)
        {
            redisKey = AddKeyPrefix(redisKey);
            return Database.HashExists(redisKey, hashField);
        }
        #endregion

        #region HashKeys
        /// <summary>
        /// 获取hash中指定key的所有字段key
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <returns>返回hash字段key集合</returns>
        public IEnumerable<string> HashKeys(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return Database.HashKeys(redisKey).Select(o => o.ToString());
        }
        #endregion

        #region HashValues
        /// <summary>
        /// 获取hash中指定key的所有字段value
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <returns>返回hash字段value集合</returns>
        public IEnumerable<string> HashValues(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return Database.HashValues(redisKey).Select(o => o.ToString());
        }
        #endregion

        #region HashLength
        /// <summary>
        /// 获取hash长度
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="flags">操作命令标识</param>
        /// <returns></returns>
        public long HashLength(string key, CommandFlags flags = CommandFlags.None)
        {
            key = AddKeyPrefix(key);
            return Database.HashLength(key, flags);
        }
        #endregion

        #region HashScan
        /// <summary>
        /// hash扫描
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="pattern">模式匹配key</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="flags">操作命令标识</param>
        /// <returns></returns>
        public IEnumerable<HashEntry> HashScan(string key, string pattern, int pageSize, CommandFlags flags)
        {
            key = AddKeyPrefix(key);
            return Database.HashScan(key, pattern, pageSize, flags);
        }

        /// <summary>
        /// hash扫描
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="pattern">模式匹配key</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="cursor">起始位置</param>
        /// <param name="pageOffset">起始偏移量</param>
        /// <param name="flags">操作命令标识</param>
        /// <returns></returns>
        public IEnumerable<HashEntry> HashScan(string key, string pattern, int pageSize = 250, long cursor = 0L, int pageOffset = 0, CommandFlags flags = CommandFlags.None)
        {
            key = AddKeyPrefix(key);
            return Database.HashScan(key, pattern, pageSize, cursor, pageOffset, flags);
        }
        #endregion
        #endregion

        #region 异步方法
        #region HashSetAsync
        /// <summary>
        /// 保存hash字段
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="hashField">hash字段key</param>
        /// <param name="fieldValue">hash字段value</param>
        /// <returns>返回是否保存成功</returns>
        public async Task<bool> HashSetAsync(string redisKey, string hashField, string fieldValue)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await Database.HashSetAsync(redisKey, hashField, fieldValue);
        }

        /// <summary>
        /// 保存hash字段
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="hashFields">hash字段key-value集合</param>
        public async Task HashSetAsync(string redisKey, IEnumerable<KeyValuePair<string, string>> hashFields)
        {
            redisKey = AddKeyPrefix(redisKey);
            var entries = hashFields.Select(x => new HashEntry(AddKeyPrefix(x.Key), x.Value));
            await Database.HashSetAsync(redisKey, entries.ToArray());
        }

        /// <summary>
        /// 保存对象到hash中
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="hashField">hash字段key</param>
        /// <param name="fieldValue">hash字段value</param>
        /// <returns>返回是否保存成功</returns>
        public async Task<bool> HashSetAsync<T>(string redisKey, string hashField, T fieldValue)
        {
            return await HashSetAsync(redisKey, hashField, fieldValue.ToJson());
        }
        #endregion

        #region HashGetAsync
        /// <summary>
        /// 获取hash字段值
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="hashField">hash字段key</param>
        /// <returns>返回hash字段值</returns>
        public async Task<string> HashGetAsync(string redisKey, string hashField)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await Database.HashGetAsync(redisKey, hashField);
        }

        /// <summary>
        /// 获取hash中反序列化后的对象
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="hashField">hash字段key</param>
        /// <returns>返回反序列化后的对象</returns>
        public async Task<T> HashGetAsync<T>(string redisKey, string hashField)
        {
            return (await HashGetAsync(redisKey, hashField)).ToObject<T>();
        }

        /// <summary>
        /// 获取hash字段值反序列化后的对象集合
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">redis存储key</param>
        /// <returns>返回反序列化后的对象集合</returns>
        public async Task<IEnumerable<T>> HashGetAsync<T>(string redisKey)
        {
            var hashFields = await HashKeysAsync(redisKey);
            return await HashGetAsync<T>(redisKey, hashFields);
        }

        /// <summary>
        /// 获取hash字段值反序列化后的对象集合
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="hashFields">hash字段key集合</param>
        /// <returns>返回反序列化后的对象集合</returns>
        public async Task<IEnumerable<T>> HashGetAsync<T>(string redisKey, IEnumerable<string> hashFields)
        {
            redisKey = AddKeyPrefix(redisKey);
            var fields = hashFields.Select(x => (RedisValue)x);
            var result = (await Database.HashGetAsync(redisKey, fields.ToArray())).Select(o => o.ToString());
            return result.Select(o => o.ToString().ToObject<T>());
        }
        #endregion

        #region HashDeleteAsync
        /// <summary>
        /// 从hash中移除指定字段
        /// </summary>
        /// <param name="redisKey">redis存储key</param>        
        /// <returns>返回是否删除成功</returns>
        public async Task<long> HashDeleteAsync(string redisKey)
        {
            var hashFields = await HashKeysAsync(redisKey);
            return await HashDeleteAsync(redisKey, hashFields);
        }

        /// <summary>
        /// 从hash中移除指定字段
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="hashField">hash字段key</param>
        /// <returns>返回是否删除成功</returns>
        public async Task<bool> HashDeleteAsync(string redisKey, string hashField)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await Database.HashDeleteAsync(redisKey, hashField);
        }

        /// <summary>
        /// 从hash中移除指定字段
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="hashFields">hash字段key集合</param>
        /// <returns>返回是否删除成功</returns>
        public async Task<long> HashDeleteAsync(string redisKey, IEnumerable<string> hashFields)
        {
            redisKey = AddKeyPrefix(redisKey);
            var fields = hashFields.Select(x => (RedisValue)x);
            return await Database.HashDeleteAsync(redisKey, fields.ToArray());
        }
        #endregion

        #region HashExistsAsync
        /// <summary>
        /// 判断键值是否在hash中
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="hashField">hash字段key</param>
        /// <returns>返回是否存在</returns>
        public async Task<bool> HashExistsAsync(string redisKey, string hashField)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await Database.HashExistsAsync(redisKey, hashField);
        }
        #endregion

        #region HashKeysAsync
        /// <summary>
        /// 获取hash中指定key的所有字段key
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <returns>返回hash字段key集合</returns>
        public async Task<IEnumerable<string>> HashKeysAsync(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return (await Database.HashKeysAsync(redisKey)).Select(o => o.ToString());
        }
        #endregion

        #region HashValuesAsync
        /// <summary>
        /// 获取hash中指定key的所有字段value
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <returns>返回hash字段value集合</returns>
        public async Task<IEnumerable<string>> HashValuesAsync(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return (await Database.HashValuesAsync(redisKey)).Select(o => o.ToString());
        }
        #endregion

        #region HashLengthAsync
        /// <summary>
        /// 获取hash长度
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="flags">操作命令标识</param>
        /// <returns></returns>
        public async Task<long> HashLengthAsync(string key, CommandFlags flags = CommandFlags.None)
        {
            key = AddKeyPrefix(key);
            return await Database.HashLengthAsync(key, flags);
        }
        #endregion
        #endregion
        #endregion

        #region List操作
        #region 同步方法
        #region ListLeft
        /// <summary>
        /// 在列表头部插入值，如果键不存在，先创建再插入值
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="redisValue">redis存储value</param>
        /// <returns>返回插入后列表的长度</returns>
        public long ListLeftPush(string redisKey, string redisValue)
        {
            redisKey = AddKeyPrefix(redisKey);
            return Database.ListLeftPush(redisKey, redisValue);
        }

        /// <summary>
        /// 在列表头部插入值，如果键不存在，先创建再插入值
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="redisValue">redis存储value</param>
        /// <returns>返回插入后列表的长度</returns>
        public long ListLeftPush<T>(string redisKey, T redisValue)
        {
            return ListLeftPush(redisKey, redisValue.ToJson());
        }

        /// <summary>
        /// 移除并返回存储在该键列表的第一个元素
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <returns>返回移除元素的值</returns>
        public string ListLeftPop(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return Database.ListLeftPop(redisKey);
        }

        /// <summary>
        /// 移除并返回存储在该键列表的第一个元素
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">redis存储key</param>
        /// <returns>返回移除元素的值</returns>
        public T ListLeftPop<T>(string redisKey)
        {
            return ListLeftPop(redisKey).ToObject<T>();
        }
        #endregion

        #region ListRight
        /// <summary>
        /// 在列表尾部插入值，如果键不存在，先创建再插入值
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="redisValue">redis存储value</param>
        /// <returns>返回插入后列表的长度</returns>
        public long ListRightPush(string redisKey, string redisValue)
        {
            redisKey = AddKeyPrefix(redisKey);
            return Database.ListRightPush(redisKey, redisValue);
        }

        /// <summary>
        /// 在列表尾部插入值，如果键不存在，先创建再插入值
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="redisValue">redis存储value</param>
        /// <returns>返回插入后列表的长度</returns>
        public long ListRightPush<T>(string redisKey, T redisValue)
        {
            return ListRightPush(redisKey, redisValue.ToJson());
        }

        /// <summary>
        /// 移除并返回存储在该键列表的最后一个元素
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <returns>返回移除元素的值</returns>
        public string ListRightPop(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return Database.ListRightPop(redisKey);
        }

        /// <summary>
        /// 移除并返回存储在该键列表的最后一个元素
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">redis存储key</param>
        /// <returns>返回移除元素的值</returns>
        public T ListRightPop<T>(string redisKey)
        {
            return ListRightPop(redisKey).ToObject<T>();
        }
        #endregion

        #region ListRemove
        /// <summary>
        /// 移除列表指定键上与该值相同的元素
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="redisValue">redis存储value</param>
        /// <returns>返回移除元素的数量</returns>
        public long ListRemove(string redisKey, string redisValue)
        {
            redisKey = AddKeyPrefix(redisKey);
            return Database.ListRemove(redisKey, redisValue);
        }
        #endregion

        #region ListLength
        /// <summary>
        /// 返回列表上该键的长度，如果不存在，返回 0
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <returns>返回列表的长度</returns>
        public long ListLength(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return Database.ListLength(redisKey);
        }
        #endregion

        #region ListRange
        /// <summary>
        /// 返回在该列表上键所对应的元素
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="start">开始索引位置</param>
        /// <param name="stop">结束索引位置</param>
        /// <returns>返回指定范围内的元素集合</returns>
        public IEnumerable<string> ListRange(string redisKey, long start = 0, long stop = -1)
        {
            redisKey = AddKeyPrefix(redisKey);
            return Database.ListRange(redisKey, start, stop).Select(o => o.ToString());
        }
        #endregion

        #region Queue
        /// <summary>
        /// 队列入队
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="redisValue">redis存储value</param>
        /// <returns>返回入队后队列的长度</returns>
        public long EnqueueItemOnList(string redisKey, string redisValue)
        {
            return ListRightPush(redisKey, redisValue);
        }

        /// <summary>
        /// 队列入队
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="redisValue">redis存储value</param>
        /// <returns>返回入队后队列的长度</returns>
        public long EnqueueItemOnList<T>(string redisKey, T redisValue)
        {
            return ListRightPush(redisKey, redisValue);
        }

        /// <summary>
        /// 队列出队
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <returns>返回出队元素的值</returns>
        public string DequeueItemFromList(string redisKey)
        {
            return ListLeftPop(redisKey);
        }

        /// <summary>
        /// 队列出队
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">redis存储key</param>
        /// <returns>返回出队元素的值</returns>
        public T DequeueItemFromList<T>(string redisKey)
        {
            return ListLeftPop<T>(redisKey);
        }

        /// <summary>
        /// 获取队列长度
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <returns>返回队列的长度</returns>
        public long GetQueueLength(string redisKey)
        {
            return ListLength(redisKey);
        }
        #endregion
        #endregion

        #region 异步方法
        #region ListLeftAsync
        /// <summary>
        /// 在列表头部插入值。如果键不存在，先创建再插入值
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="redisValue">redis存储value</param>
        /// <returns>返回插入后列表的长度</returns>
        public async Task<long> ListLeftPushAsync(string redisKey, string redisValue)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await Database.ListLeftPushAsync(redisKey, redisValue);
        }

        /// <summary>
        /// 在列表头部插入值。如果键不存在，先创建再插入值
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="redisValue">redis存储value</param>
        /// <returns>返回插入后列表的长度</returns>
        public async Task<long> ListLeftPushAsync<T>(string redisKey, T redisValue)
        {
            return await ListLeftPushAsync(redisKey, redisValue.ToJson());
        }

        /// <summary>
        /// 移除并返回存储在该键列表的第一个元素
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <returns>返回移除元素的值</returns>
        public async Task<string> ListLeftPopAsync(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await Database.ListLeftPopAsync(redisKey);
        }

        /// <summary>
        /// 移除并返回存储在该键列表的第一个元素
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">redis存储key</param>
        /// <returns>返回移除元素的值</returns>
        public async Task<T> ListLeftPopAsync<T>(string redisKey)
        {
            return (await ListLeftPopAsync(redisKey)).ToObject<T>();
        }
        #endregion

        #region ListRightAsync
        /// <summary>
        /// 在列表尾部插入值。如果键不存在，先创建再插入值
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="redisValue">redis存储value</param>
        /// <returns>返回插入后列表的长度</returns>
        public async Task<long> ListRightPushAsync(string redisKey, string redisValue)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await Database.ListRightPushAsync(redisKey, redisValue);
        }

        /// <summary>
        /// 在列表尾部插入值。如果键不存在，先创建再插入值
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="redisValue">redis存储value</param>
        /// <returns>返回插入后列表的长度</returns>
        public async Task<long> ListRightPushAsync<T>(string redisKey, T redisValue)
        {
            return await ListRightPushAsync(redisKey, redisValue.ToJson());
        }

        /// <summary>
        /// 移除并返回存储在该键列表的最后一个元素
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <returns>返回移除元素的值</returns>
        public async Task<string> ListRightPopAsync(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await Database.ListRightPopAsync(redisKey);
        }

        /// <summary>
        /// 移除并返回存储在该键列表的最后一个元素
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">redis存储key</param>
        /// <returns>返回移除元素的值</returns>
        public async Task<T> ListRightPopAsync<T>(string redisKey)
        {
            return (await ListRightPopAsync(redisKey)).ToObject<T>();
        }
        #endregion

        #region ListRemoveAsync
        /// <summary>
        /// 移除列表指定键上与该值相同的元素
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="redisValue">redis存储value</param>
        /// <returns>返回移除元素的数量</returns>
        public async Task<long> ListRemoveAsync(string redisKey, string redisValue)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await Database.ListRemoveAsync(redisKey, redisValue);
        }
        #endregion

        #region ListLengthAsync
        /// <summary>
        /// 返回列表上该键的长度，如果不存在，返回 0
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <returns>返回列表的长度</returns>
        public async Task<long> ListLengthAsync(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await Database.ListLengthAsync(redisKey);
        }
        #endregion

        #region ListRangeAsync
        /// <summary>
        /// 返回在该列表上键所对应的元素
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="start">开始索引位置</param>
        /// <param name="stop">结束索引位置</param>
        /// <returns>返回指定范围内的元素集合</returns>
        public async Task<IEnumerable<string>> ListRangeAsync(string redisKey, long start = 0, long stop = -1)
        {
            redisKey = AddKeyPrefix(redisKey);
            var query = await Database.ListRangeAsync(redisKey, start, stop);
            return query.Select(x => x.ToString());
        }
        #endregion

        #region QueueAsync
        /// <summary>
        /// 队列入队
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="redisValue">redis存储value</param>
        /// <returns>返回入队后队列的长度</returns>
        public async Task<long> EnqueueItemOnListAsync(string redisKey, string redisValue)
        {
            return await ListRightPushAsync(redisKey, redisValue);
        }

        /// <summary>
        /// 队列入队
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="redisValue">redis存储value</param>
        /// <returns>返回入队后队列的长度</returns>
        public async Task<long> EnqueueItemOnListAsync<T>(string redisKey, T redisValue)
        {
            return await ListRightPushAsync(redisKey, redisValue);
        }

        /// <summary>
        /// 队列出队
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <returns>返回出队元素的值</returns>
        public async Task<string> DequeueItemFromListAsync(string redisKey)
        {
            return await ListLeftPopAsync(redisKey);
        }

        /// <summary>
        /// 队列出队
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">redis存储key</param>
        /// <returns>返回出队元素的值</returns>
        public async Task<T> DequeueItemFromListAsync<T>(string redisKey)
        {
            return await ListLeftPopAsync<T>(redisKey);
        }

        /// <summary>
        /// 获取队列长度
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <returns>返回队列的长度</returns>
        public async Task<long> GetQueueLengthAsync(string redisKey)
        {
            return await ListLengthAsync(redisKey);
        }
        #endregion
        #endregion
        #endregion

        #region SortedSet操作
        #region 同步方法
        #region SortedSetAdd
        /// <summary>
        /// 新增元素到有序集合
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="member">redis存储value</param>
        /// <param name="score">score</param>
        /// <returns>若值已存在则返回false且score被更新，否则添加成功返回true</returns>
        public bool SortedSetAdd(string redisKey, string member, double score)
        {
            redisKey = AddKeyPrefix(redisKey);
            return Database.SortedSetAdd(redisKey, member, score);
        }

        /// <summary>
        /// 新增元素到有序集合
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="member">redis存储value</param>
        /// <param name="score">score</param>
        /// <returns>若值已存在则返回false且score被更新，否则添加成功返回true</returns>
        public bool SortedSetAdd<T>(string redisKey, T member, double score)
        {
            return SortedSetAdd(redisKey, member.ToJson(), score);
        }
        #endregion

        #region SortedSetRemove
        /// <summary>
        /// 移除有序集合中指定key-value的元素
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="member">redis存储value</param>
        /// <returns>返回是否移除成功</returns>
        public bool SortedSetRemove(string redisKey, string member)
        {
            redisKey = AddKeyPrefix(redisKey);
            return Database.SortedSetRemove(redisKey, member);
        }

        /// <summary>
        /// 移除有序集合中指定key-value的元素
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="member">redis存储value</param>
        /// <returns>返回是否移除成功</returns>
        public bool SortedSetRemove<T>(string redisKey, T member)
        {
            return SortedSetRemove(redisKey, member.ToJson());
        }

        /// <summary>
        /// 根据起始索引位置移除有序集合中的指定范围的元素
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="start">开始索引位置</param>
        /// <param name="stop">结束索引位置</param>
        /// <returns>返回移除元素数量</returns>
        public long SortedSetRemoveRangeByRank(string redisKey, long start, long stop)
        {
            redisKey = AddKeyPrefix(redisKey);
            return Database.SortedSetRemoveRangeByRank(redisKey, start, stop);
        }

        /// <summary>
        /// 根据score起始值移除有序集合中的指定score范围的元素
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="start">开始score</param>
        /// <param name="stop">结束score</param>
        /// <returns>返回移除元素数量</returns>
        public long SortedSetRemoveRangeByScore(string redisKey, double start, double stop)
        {
            redisKey = AddKeyPrefix(redisKey);
            return Database.SortedSetRemoveRangeByScore(redisKey, start, stop);
        }

        /// <summary>
        /// 根据value最大和最小值移除有序集合中的指定value范围的元素
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="min">value最小值</param>
        /// <param name="max">value最大值</param>
        /// <returns>返回移除元素数量</returns>
        public long SortedSetRemoveRangeByValue(string redisKey, string min, string max)
        {
            redisKey = AddKeyPrefix(redisKey);
            return Database.SortedSetRemoveRangeByValue(redisKey, min, max);
        }
        #endregion

        #region SortedSetIncrement
        /// <summary>
        /// 按增量增加按键存储的有序集合中成员的score
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="member">redis存储value</param>
        /// <param name="value">增量值</param>
        /// <returns>返回新的score</returns>
        public double SortedSetIncrement(string redisKey, string member, double value = 1)
        {
            redisKey = AddKeyPrefix(redisKey);
            return Database.SortedSetIncrement(redisKey, member, value);
        }

        /// <summary>
        /// 按增量增加按键存储的有序集合中成员的score
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="member">redis存储value</param>
        /// <param name="value">增量值</param>
        /// <returns>返回新的score</returns>
        public double SortedSetIncrement<T>(string redisKey, T member, double value = 1)
        {
            redisKey = AddKeyPrefix(redisKey);
            return Database.SortedSetIncrement(redisKey, member.ToJson(), value);
        }
        #endregion

        #region SortedSetDecrement
        /// <summary>
        /// 通过递减递减存储在键处的有序集中的成员的score
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="member">redis存储value</param>
        /// <param name="value">递减值</param>
        /// <returns>返回新的score</returns>
        public double SortedSetDecrement(string redisKey, string member, double value)
        {
            redisKey = AddKeyPrefix(redisKey);
            return Database.SortedSetDecrement(redisKey, member, value);
        }

        /// <summary>
        /// 通过递减递减存储在键处的有序集中的成员的score
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="member">redis存储value</param>
        /// <param name="value">递减值</param>
        /// <returns>返回新的score</returns>
        public double SortedSetDecrement<T>(string redisKey, T member, double value)
        {
            redisKey = AddKeyPrefix(redisKey);
            return Database.SortedSetDecrement(redisKey, member.ToJson(), value);
        }
        #endregion

        #region SortedSetLength
        /// <summary>
        /// 获取有序集合的长度
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <returns>返回有序集合的长度</returns>
        public long SortedSetLength(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return Database.SortedSetLength(redisKey);
        }
        #endregion

        #region SortedSetRank
        /// <summary>
        /// 获取集合中的索引位置，从0开始
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="member">redis存储value</param>
        /// <param name="order">排序方式</param>
        /// <returns>返回索引位置</returns>
        public long? SortedSetRank(string redisKey, string member, Order order = Order.Ascending)
        {
            redisKey = AddKeyPrefix(redisKey);
            return Database.SortedSetRank(redisKey, member, order);
        }

        /// <summary>
        /// 获取集合中的索引位置，从0开始
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="member">redis存储value</param>
        /// <param name="order">排序方式</param>
        /// <returns>返回索引位置</returns>
        public long? SortedSetRank<T>(string redisKey, T member, Order order = Order.Ascending)
        {
            redisKey = AddKeyPrefix(redisKey);
            return Database.SortedSetRank(redisKey, member.ToJson(), order);
        }
        #endregion

        #region SortedSetScore
        /// <summary>
        /// 获取有序集合中指定元素的score
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="memebr">redis存储value</param>
        /// <returns>返回指定元素的score</returns>
        public double? SortedSetScore(string redisKey, string memebr)
        {
            redisKey = AddKeyPrefix(redisKey);
            return Database.SortedSetScore(redisKey, memebr);
        }

        /// <summary>
        /// 获取有序集合中指定元素的score
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="memebr">redis存储value</param>
        /// <returns>返回指定元素的score</returns>
        public double? SortedSetScore<T>(string redisKey, T memebr)
        {
            redisKey = AddKeyPrefix(redisKey);
            return Database.SortedSetScore(redisKey, memebr.ToJson());
        }
        #endregion

        #region SortedSetRange
        /// <summary>
        /// 获取有序集合中指定索引范围的元素value，默认情况下从低到高
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="start">开始索引</param>
        /// <param name="stop">结束索引</param>
        /// <param name="order">排序方式</param>        
        /// <returns>返回指定范围的元素value</returns>
        public IEnumerable<string> SortedSetRangeByRank(string redisKey, long start = 0, long stop = -1, Order order = Order.Ascending)
        {
            redisKey = AddKeyPrefix(redisKey);
            return Database.SortedSetRangeByRank(redisKey, start, stop, order).Select(x => x.ToString());
        }

        /// <summary>
        /// 获取有序集合中指定索引范围的元素value，默认情况下从低到高
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="start">开始索引</param>
        /// <param name="stop">结束索引</param>
        /// <param name="order">排序方式</param>        
        /// <returns>返回指定范围的元素value</returns>
        public IEnumerable<T> SortedSetRangeByRank<T>(string redisKey, long start = 0, long stop = -1, Order order = Order.Ascending)
        {
            return SortedSetRangeByRank(redisKey, start, stop, order).Select(o => o.ToObject<T>());
        }

        /// <summary>
        /// 获取有序集合中指定索引范围的元素value-score，默认情况下从低到高
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="start">开始索引</param>
        /// <param name="stop">结束索引</param>
        /// <param name="order">排序方式</param>        
        /// <returns>返回指定范围的元素value-score</returns>
        public Dictionary<string, double> SortedSetRangeByRankWithScores(string redisKey, long start = 0, long stop = -1, Order order = Order.Ascending)
        {
            redisKey = AddKeyPrefix(redisKey);
            var result = Database.SortedSetRangeByRankWithScores(redisKey, start, stop, order);
            return result.Select(x => new { x.Score, Value = x.Element.ToString() }).ToDictionary(x => x.Value, x => x.Score);
        }

        /// <summary>
        /// 获取有序集合中指定索引范围的元素value-score，默认情况下从低到高
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="start">开始索引</param>
        /// <param name="stop">结束索引</param>
        /// <param name="order">排序方式</param>        
        /// <returns>返回指定范围的元素value-score</returns>
        public Dictionary<T, double> SortedSetRangeByRankWithScores<T>(string redisKey, long start = 0, long stop = -1, Order order = Order.Ascending)
        {
            return SortedSetRangeByRankWithScores(redisKey, start, stop, order).ToDictionary(o => o.Key.ToObject<T>(), o => o.Value);
        }

        /// <summary>
        /// 获取有序集合中指定score起始范围的元素value，默认情况下从低到高
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="start">开始score</param>
        /// <param name="stop">结束score</param>
        /// <param name="skip">跳过元素数量</param>
        /// <param name="take">拿取元素数量</param>
        /// <param name="order">排序方式</param>
        /// <returns>返回指定范围的元素value</returns>
        public IEnumerable<string> SortedSetRangeByScore(string redisKey, double start = double.NegativeInfinity, double stop = double.PositiveInfinity, long skip = 0, long take = -1, Order order = Order.Ascending)
        {
            redisKey = AddKeyPrefix(redisKey);
            return Database.SortedSetRangeByScore(redisKey, start, stop, order: order, skip: skip, take: take).Select(o => o.ToString());
        }

        /// <summary>
        /// 获取有序集合中指定score起始范围的元素value，默认情况下从低到高
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="start">开始score</param>
        /// <param name="stop">结束score</param>
        /// <param name="skip">跳过元素数量</param>
        /// <param name="take">拿取元素数量</param>
        /// <param name="order">排序方式</param>
        /// <returns>返回指定范围的元素value</returns>
        public IEnumerable<T> SortedSetRangeByScore<T>(string redisKey, double start = double.NegativeInfinity, double stop = double.PositiveInfinity, long skip = 0, long take = -1, Order order = Order.Ascending)
        {
            return SortedSetRangeByScore(redisKey, start, stop, skip, take, order).Select(o => o.ToObject<T>());
        }

        /// <summary>
        /// 获取有序集合中指定score起始范围的元素value-score，默认情况下从低到高
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="start">开始score</param>
        /// <param name="stop">结束score</param>
        /// <param name="skip">跳过元素数量</param>
        /// <param name="take">拿取元素数量</param>
        /// <param name="order">排序方式</param>
        /// <returns>返回指定范围的元素value-score</returns>
        public Dictionary<string, double> SortedSetRangeByScoreWithScores(string redisKey, double start = double.NegativeInfinity, double stop = double.PositiveInfinity, long skip = 0, long take = -1, Order order = Order.Ascending)
        {
            redisKey = AddKeyPrefix(redisKey);
            var result = Database.SortedSetRangeByScoreWithScores(redisKey, start, stop, order: order, skip: skip, take: take);
            return result.Select(x => new { x.Score, Value = x.Element.ToString() }).ToDictionary(x => x.Value, x => x.Score);
        }

        /// <summary>
        /// 获取有序集合中指定score起始范围的元素value-score，默认情况下从低到高
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="start">开始score</param>
        /// <param name="stop">结束score</param>
        /// <param name="skip">跳过元素数量</param>
        /// <param name="take">拿取元素数量</param>
        /// <param name="order">排序方式</param>
        /// <returns>返回指定范围的元素value-score</returns>
        public Dictionary<T, double> SortedSetRangeByScoreWithScores<T>(string redisKey, double start = double.NegativeInfinity, double stop = double.PositiveInfinity, long skip = 0, long take = -1, Order order = Order.Ascending)
        {
            return SortedSetRangeByScoreWithScores(redisKey, start, stop, skip, take, order).ToDictionary(o => o.Key.ToObject<T>(), o => o.Value);
        }

        /// <summary>
        /// 获取有序集合中指定value最大最小范围的元素value，当有序集合中的score相同时，按照value从小到大进行排序
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="min">value最小值</param>
        /// <param name="max">value最大值</param>
        /// <param name="skip">跳过元素数量</param>
        /// <param name="take">拿取元素数量</param>        
        /// <returns>返回指定范围的元素value</returns>
        public IEnumerable<string> SortedSetRangeByValue(string redisKey, RedisValue min = default(RedisValue), RedisValue max = default(RedisValue), long skip = 0, long take = -1)
        {
            redisKey = AddKeyPrefix(redisKey);
            return Database.SortedSetRangeByValue(redisKey, min, max, skip: skip, take: take).Select(o => o.ToString());
        }

        /// <summary>
        /// 获取有序集合中指定value最大最小范围的元素value，当有序集合中的score相同时，按照value从小到大进行排序
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="min">value最小值</param>
        /// <param name="max">value最大值</param>
        /// <param name="skip">跳过元素数量</param>
        /// <param name="take">拿取元素数量</param>        
        /// <returns>返回指定范围的元素value</returns>
        public IEnumerable<T> SortedSetRangeByValue<T>(string redisKey, RedisValue min = default(RedisValue), RedisValue max = default(RedisValue), long skip = 0, long take = -1)
        {
            return SortedSetRangeByValue(redisKey, min, max, skip, take).Select(o => o.ToObject<T>());
        }
        #endregion
        #endregion

        #region 异步方法
        #region SortedSetAddAsync
        /// <summary>
        /// 新增元素到有序集合
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="member">redis存储value</param>
        /// <param name="score">score</param>
        /// <returns>若值已存在则返回false且score被更新，否则添加成功返回true</returns>
        public async Task<bool> SortedSetAddAsync(string redisKey, string member, double score)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await Database.SortedSetAddAsync(redisKey, member, score);
        }

        /// <summary>
        /// 新增元素到有序集合
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="member">redis存储value</param>
        /// <param name="score">score</param>
        /// <returns>若值已存在则返回false且score被更新，否则添加成功返回true</returns>
        public async Task<bool> SortedSetAddAsync<T>(string redisKey, T member, double score)
        {
            return await SortedSetAddAsync(redisKey, member.ToJson(), score);
        }
        #endregion

        #region SortedSetRemoveAsync
        /// <summary>
        /// 移除有序集合中指定key-value的元素
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="member">redis存储value</param>
        /// <returns>返回是否移除成功</returns>
        public async Task<bool> SortedSetRemoveAsync(string redisKey, string member)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await Database.SortedSetRemoveAsync(redisKey, member);
        }

        /// <summary>
        /// 移除有序集合中指定key-value的元素
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="member">redis存储value</param>
        /// <returns>返回是否移除成功</returns>
        public async Task<bool> SortedSetRemoveAsync<T>(string redisKey, T member)
        {
            return await SortedSetRemoveAsync(redisKey, member.ToJson());
        }

        /// <summary>
        /// 根据起始索引位置移除有序集合中的指定范围的元素
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="start">开始索引位置</param>
        /// <param name="stop">结束索引位置</param>
        /// <returns>返回移除元素数量</returns>
        public async Task<long> SortedSetRemoveRangeByRankAsync(string redisKey, long start, long stop)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await Database.SortedSetRemoveRangeByRankAsync(redisKey, start, stop);
        }

        /// <summary>
        /// 根据score起始值移除有序集合中的指定score范围的元素
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="start">开始score</param>
        /// <param name="stop">结束score</param>
        /// <returns>返回移除元素数量</returns>
        public async Task<long> SortedSetRemoveRangeByScoreAsync(string redisKey, double start, double stop)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await Database.SortedSetRemoveRangeByScoreAsync(redisKey, start, stop);
        }

        /// <summary>
        /// 根据value最大和最小值移除有序集合中的指定value范围的元素
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="min">value最小值</param>
        /// <param name="max">value最大值</param>
        /// <returns>返回移除元素数量</returns>
        public async Task<long> SortedSetRemoveRangeByValueAsync(string redisKey, string min, string max)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await Database.SortedSetRemoveRangeByValueAsync(redisKey, min, max);
        }
        #endregion

        #region SortedSetIncrementAsync
        /// <summary>
        /// 按增量增加按键存储的有序集合中成员的score
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="member">redis存储value</param>
        /// <param name="value">增量值</param>
        /// <returns>返回新的score</returns>
        public async Task<double> SortedSetIncrementAsync(string redisKey, string member, double value = 1)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await Database.SortedSetIncrementAsync(redisKey, member, value);
        }

        /// <summary>
        /// 按增量增加按键存储的有序集合中成员的score
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="member">redis存储value</param>
        /// <param name="value">增量值</param>
        /// <returns>返回新的score</returns>
        public async Task<double> SortedSetIncrementAsync<T>(string redisKey, T member, double value = 1)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await Database.SortedSetIncrementAsync(redisKey, member.ToJson(), value);
        }
        #endregion

        #region SortedSetDecrementAsync
        /// <summary>
        /// 通过递减递减存储在键处的有序集中的成员的score
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="member">redis存储value</param>
        /// <param name="value">递减值</param>
        /// <returns>返回新的score</returns>
        public async Task<double> SortedSetDecrementAsync(string redisKey, string member, double value)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await Database.SortedSetDecrementAsync(redisKey, member, value);
        }

        /// <summary>
        /// 通过递减递减存储在键处的有序集中的成员的score
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="member">redis存储value</param>
        /// <param name="value">递减值</param>
        /// <returns>返回新的score</returns>
        public async Task<double> SortedSetDecrementAsync<T>(string redisKey, T member, double value)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await Database.SortedSetDecrementAsync(redisKey, member.ToJson(), value);
        }
        #endregion

        #region SortedSetLengthAsync
        /// <summary>
        /// 获取有序集合的长度
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <returns>返回有序集合的长度</returns>
        public async Task<long> SortedSetLengthAsync(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await Database.SortedSetLengthAsync(redisKey);
        }
        #endregion

        #region SortedSetRankAsync
        /// <summary>
        /// 获取集合中的索引位置，从0开始
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="member">redis存储value</param>
        /// <param name="order">排序方式</param>
        /// <returns>返回索引位置</returns>
        public async Task<long?> SortedSetRankAsync(string redisKey, string member, Order order = Order.Ascending)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await Database.SortedSetRankAsync(redisKey, member, order);
        }

        /// <summary>
        /// 获取集合中的索引位置，从0开始
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="member">redis存储value</param>
        /// <param name="order">排序方式</param>
        /// <returns>返回索引位置</returns>
        public async Task<long?> SortedSetRankAsync<T>(string redisKey, T member, Order order = Order.Ascending)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await Database.SortedSetRankAsync(redisKey, member.ToJson(), order);
        }
        #endregion

        #region SortedSetScoreAsync
        /// <summary>
        /// 获取有序集合中指定元素的score
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="memebr">redis存储value</param>
        /// <returns>返回指定元素的score</returns>
        public async Task<double?> SortedSetScoreAsync(string redisKey, string memebr)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await Database.SortedSetScoreAsync(redisKey, memebr);
        }

        /// <summary>
        /// 获取有序集合中指定元素的score
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="memebr">redis存储value</param>
        /// <returns>返回指定元素的score</returns>
        public async Task<double?> SortedSetScoreAsync<T>(string redisKey, T memebr)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await Database.SortedSetScoreAsync(redisKey, memebr.ToJson());
        }
        #endregion

        #region SortedSetRangeAsync
        /// <summary>
        /// 获取有序集合中指定索引范围的元素value，默认情况下从低到高
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="start">开始索引</param>
        /// <param name="stop">结束索引</param>
        /// <param name="order">排序方式</param>
        /// <returns>返回指定范围的元素value</returns>
        public async Task<IEnumerable<string>> SortedSetRangeByRankAsync(string redisKey, long start = 0, long stop = -1, Order order = Order.Ascending)
        {
            redisKey = AddKeyPrefix(redisKey);
            return (await Database.SortedSetRangeByRankAsync(redisKey, start, stop, order)).Select(o => o.ToString());
        }

        /// <summary>
        /// 获取有序集合中指定索引范围的元素value，默认情况下从低到高
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="start">开始索引</param>
        /// <param name="stop">结束索引</param>
        /// <param name="order">排序方式</param>
        /// <returns>返回指定范围的元素value</returns>
        public async Task<IEnumerable<T>> SortedSetRangeByRankAsync<T>(string redisKey, long start = 0, long stop = -1, Order order = Order.Ascending)
        {
            return (await SortedSetRangeByRankAsync(redisKey, start, stop, order)).Select(o => o.ToObject<T>());
        }

        /// <summary>
        /// 获取有序集合中指定索引范围的元素value-score，默认情况下从低到高
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="start">开始索引</param>
        /// <param name="stop">结束索引</param>
        /// <param name="order">排序方式</param>        
        /// <returns>返回指定范围的元素value-score</returns>
        public async Task<Dictionary<string, double>> SortedSetRangeByRankWithScoresAsync(string redisKey, long start = 0, long stop = -1, Order order = Order.Ascending)
        {
            redisKey = AddKeyPrefix(redisKey);
            var result = await Database.SortedSetRangeByRankWithScoresAsync(redisKey, start, stop, order);
            return result.Select(x => new { x.Score, Value = x.Element.ToString() }).ToDictionary(x => x.Value, x => x.Score);
        }

        /// <summary>
        /// 获取有序集合中指定索引范围的元素value-score，默认情况下从低到高
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="start">开始索引</param>
        /// <param name="stop">结束索引</param>
        /// <param name="order">排序方式</param>        
        /// <returns>返回指定范围的元素value-score</returns>
        public async Task<Dictionary<T, double>> SortedSetRangeByRankWithScoresAsync<T>(string redisKey, long start = 0, long stop = -1, Order order = Order.Ascending)
        {
            return (await SortedSetRangeByRankWithScoresAsync(redisKey, start, stop, order)).ToDictionary(o => o.Key.ToObject<T>(), o => o.Value);
        }

        /// <summary>
        /// 获取有序集合中指定score起始范围的元素value，默认情况下从低到高
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="start">开始score</param>
        /// <param name="stop">结束score</param>
        /// <param name="skip">跳过元素数量</param>
        /// <param name="take">拿取元素数量</param>
        /// <param name="order">排序方式</param>
        /// <returns>返回指定范围的元素value</returns>
        public async Task<IEnumerable<string>> SortedSetRangeByScoreAsync(string redisKey, double start = double.NegativeInfinity, double stop = double.PositiveInfinity, long skip = 0, long take = -1, Order order = Order.Ascending)
        {
            redisKey = AddKeyPrefix(redisKey);
            return (await Database.SortedSetRangeByScoreAsync(redisKey, start, stop, order: order, skip: skip, take: take)).Select(o => o.ToString());
        }

        /// <summary>
        /// 获取有序集合中指定score起始范围的元素value，默认情况下从低到高
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="start">开始score</param>
        /// <param name="stop">结束score</param>
        /// <param name="skip">跳过元素数量</param>
        /// <param name="take">拿取元素数量</param>
        /// <param name="order">排序方式</param>
        /// <returns>返回指定范围的元素value</returns>
        public async Task<IEnumerable<T>> SortedSetRangeByScoreAsync<T>(string redisKey, double start = double.NegativeInfinity, double stop = double.PositiveInfinity, long skip = 0, long take = -1, Order order = Order.Ascending)
        {
            return (await SortedSetRangeByScoreAsync(redisKey, start, stop, skip, take, order)).Select(o => o.ToObject<T>());
        }

        /// <summary>
        /// 获取有序集合中指定score起始范围的元素value-score，默认情况下从低到高
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="start">开始score</param>
        /// <param name="stop">结束score</param>
        /// <param name="skip">跳过元素数量</param>
        /// <param name="take">拿取元素数量</param>
        /// <param name="order">排序方式</param>
        /// <returns>返回指定范围的元素value-score</returns>
        public async Task<Dictionary<string, double>> SortedSetRangeByScoreWithScoresAsync(string redisKey, double start = double.NegativeInfinity, double stop = double.PositiveInfinity, long skip = 0, long take = -1, Order order = Order.Ascending)
        {
            redisKey = AddKeyPrefix(redisKey);
            var result = await Database.SortedSetRangeByScoreWithScoresAsync(redisKey, start, stop, order: order, skip: skip, take: take);
            return result.Select(x => new { x.Score, Value = x.Element.ToString() }).ToDictionary(x => x.Value, x => x.Score);
        }

        /// <summary>
        /// 获取有序集合中指定score起始范围的元素value-score，默认情况下从低到高
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="start">开始score</param>
        /// <param name="stop">结束score</param>
        /// <param name="skip">跳过元素数量</param>
        /// <param name="take">拿取元素数量</param>
        /// <param name="order">排序方式</param>
        /// <returns>返回指定范围的元素value-score</returns>
        public async Task<Dictionary<T, double>> SortedSetRangeByScoreWithScoresAsync<T>(string redisKey, double start = double.NegativeInfinity, double stop = double.PositiveInfinity, long skip = 0, long take = -1, Order order = Order.Ascending)
        {
            return (await SortedSetRangeByScoreWithScoresAsync(redisKey, start, stop, skip, take, order)).ToDictionary(o => o.Key.ToObject<T>(), o => o.Value);
        }

        /// <summary>
        /// 获取有序集合中指定value最大最小范围的元素value，当有序集合中的score相同时，按照value从小到大进行排序
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="min">value最小值</param>
        /// <param name="max">value最大值</param>
        /// <param name="skip">跳过元素数量</param>
        /// <param name="take">拿取元素数量</param>
        /// <returns>返回指定范围的元素value</returns>
        public async Task<IEnumerable<string>> SortedSetRangeByValueAsync(string redisKey, RedisValue min = default(RedisValue), RedisValue max = default(RedisValue), long skip = 0, long take = -1)
        {
            redisKey = AddKeyPrefix(redisKey);
            return (await Database.SortedSetRangeByValueAsync(redisKey, min, max, skip: skip, take: take)).Select(o => o.ToString());
        }

        /// <summary>
        /// 获取有序集合中指定value最大最小范围的元素value，当有序集合中的score相同时，按照value从小到大进行排序
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="min">value最小值</param>
        /// <param name="max">value最大值</param>
        /// <param name="skip">跳过元素数量</param>
        /// <param name="take">拿取元素数量</param>
        /// <returns>返回指定范围的元素value</returns>
        public async Task<IEnumerable<T>> SortedSetRangeByValueAsync<T>(string redisKey, RedisValue min = default(RedisValue), RedisValue max = default(RedisValue), long skip = 0, long take = -1)
        {
            return (await SortedSetRangeByValueAsync(redisKey, min, max, skip, take)).Select(o => o.ToObject<T>());
        }
        #endregion
        #endregion
        #endregion

        #region Key操作
        #region 同步方法
        #region Keys
        /// <summary>
        /// 模式匹配获取key
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="database"></param>
        /// <param name="configuredOnly"></param>
        /// <returns></returns>
        public List<string> Keys(string pattern, int database = 0, bool configuredOnly = false)
        {
            var result = new List<string>();
            var points = connectionMultiplexer.GetEndPoints(configuredOnly);
            if (points?.Length > 0)
            {
                foreach (var point in points)
                {
                    var server = connectionMultiplexer.GetServer(point);
                    var keys = server.Keys(database: database, pattern: pattern);
                    result.AddRange(keys.Select(x => (string)x));
                }
            }

            return result;
        }
        #endregion

        #region KeyDelete
        /// <summary>
        /// 移除指定key
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <returns>返回是否移除成功</returns>
        public bool KeyDelete(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return Database.KeyDelete(redisKey);
        }

        /// <summary>
        /// 移除指定key
        /// </summary>
        /// <param name="redisKeys">redis存储key集合</param>
        /// <returns>返回是否移除成功</returns>
        public long KeyDelete(IEnumerable<string> redisKeys)
        {
            var keys = redisKeys.Select(x => (RedisKey)AddKeyPrefix(x));
            return Database.KeyDelete(keys.ToArray());
        }

        /// <summary>
        /// 根据通配符*移除key
        /// </summary>
        /// <param name="pattern">匹配模式</param>
        /// <param name="database">数据库</param>
        /// <param name="configuredOnly">配置</param>
        /// <returns>返回是否移除成功</returns>
        public long KeyDeleteByPattern(string pattern, int database = 0, bool configuredOnly = false)
        {
            var result = 0L;
            var points = connectionMultiplexer.GetEndPoints(configuredOnly);
            if (points?.Length > 0)
            {
                foreach (var point in points)
                {
                    var server = connectionMultiplexer.GetServer(point);
                    var keys = server.Keys(database: database, pattern: pattern);

                    result += KeyDelete(keys.Select(x => (string)x));
                }
            }
            return result;
        }
        #endregion

        #region KeyExists
        /// <summary>
        /// 判断key是否存在
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <returns>返回是否存在</returns>
        public bool KeyExists(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return Database.KeyExists(redisKey);
        }
        #endregion

        #region KeyRename
        /// <summary>
        /// 重命名key
        /// </summary>
        /// <param name="redisKey">redis存储旧key</param>
        /// <param name="redisNewKey">redis存储新key</param>
        /// <returns>返回重命名是否成功</returns>
        public bool KeyRename(string redisKey, string redisNewKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return Database.KeyRename(redisKey, redisNewKey);
        }
        #endregion

        #region KeyExpire
        /// <summary>
        /// 设置key过期时间
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="expiry">过期时间</param>
        /// <returns>返回是否设置成功</returns>
        public bool KeyExpire(string redisKey, TimeSpan? expiry)
        {
            redisKey = AddKeyPrefix(redisKey);
            return Database.KeyExpire(redisKey, expiry);
        }
        #endregion
        #endregion

        #region 异步方法
        #region Keys
        /// <summary>
        /// 模式匹配获取key
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="database"></param>
        /// <param name="configuredOnly"></param>
        /// <returns></returns>
        public async Task<List<string>> KeysAsync(string pattern, int database = 0, bool configuredOnly = false)
        {
            var result = new List<string>();
            var points = connectionMultiplexer.GetEndPoints(configuredOnly);
            if (points?.Length > 0)
            {
                foreach (var point in points)
                {
                    var server = connectionMultiplexer.GetServer(point);
                    var keys = server.Keys(database: database, pattern: pattern);
                    result.AddRange(keys.Select(x => (string)x));
                }
            }

            return await Task.FromResult(result);
        }
        #endregion

        #region KeyDeleteAsync
        /// <summary>
        /// 移除指定key
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <returns>返回是否移除成功</returns>
        public async Task<bool> KeyDeleteAsync(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await Database.KeyDeleteAsync(redisKey);
        }

        /// <summary>
        /// 移除指定key
        /// </summary>
        /// <param name="redisKeys">redis存储key集合</param>
        /// <returns>返回是否移除成功</returns>
        public async Task<long> KeyDeleteAsync(IEnumerable<string> redisKeys)
        {
            var keys = redisKeys.Select(x => (RedisKey)AddKeyPrefix(x));
            return await Database.KeyDeleteAsync(keys.ToArray());
        }

        /// <summary>
        /// 根据通配符*移除key
        /// </summary>
        /// <param name="pattern">匹配模式</param>
        /// <param name="database">数据库</param>
        /// <param name="configuredOnly">配置</param>
        /// <returns>返回是否移除成功</returns>
        public async Task<long> KeyDeleteByPatternAsync(string pattern, int database = 0, bool configuredOnly = false)
        {
            var result = 0L;
            var points = connectionMultiplexer.GetEndPoints(configuredOnly);
            if (points?.Length > 0)
            {
                foreach (var point in points)
                {
                    var server = connectionMultiplexer.GetServer(point);
                    var keys = server.Keys(database: database, pattern: pattern);
                    var keyDeletes = new List<RedisKey>();
                    foreach (var key in keys)
                    {
                        keyDeletes.Add(key);
                    }

                    result += await Database.KeyDeleteAsync(keyDeletes.ToArray());
                }
            }

            return result;
        }
        #endregion

        #region KeyExistsAsync
        /// <summary>
        /// 判断key是否存在
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <returns>返回是否存在</returns>
        public async Task<bool> KeyExistsAsync(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await Database.KeyExistsAsync(redisKey);
        }
        #endregion

        #region KeyRenameAsync
        /// <summary>
        /// 重命名key
        /// </summary>
        /// <param name="redisKey">redis存储旧key</param>
        /// <param name="redisNewKey">redis存储新key</param>
        /// <returns>返回重命名是否成功</returns>
        public async Task<bool> KeyRenameAsync(string redisKey, string redisNewKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await Database.KeyRenameAsync(redisKey, redisNewKey);
        }
        #endregion

        #region KeyExpireAsync
        /// <summary>
        /// 设置key过期时间
        /// </summary>
        /// <param name="redisKey">redis存储key</param>
        /// <param name="expiry">过期时间</param>
        /// <returns>返回是否设置成功</returns>
        public async Task<bool> KeyExpireAsync(string redisKey, TimeSpan? expiry)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await Database.KeyExpireAsync(redisKey, expiry);
        }
        #endregion
        #endregion
        #endregion

        #region 清空缓存
        #region 同步方法
        /// <summary>
        /// 清空缓存
        /// </summary>
        /// <param name="configuredOnly">默认：false</param>
        public void Clear(bool configuredOnly = false)
        {
            var points = connectionMultiplexer.GetEndPoints(configuredOnly);
            foreach (var point in points)
            {
                var server = connectionMultiplexer.GetServer(point);
                server.FlushAllDatabases();
            }
        }

        /// <summary>
        /// 清空缓存
        /// </summary>
        /// <param name="host">主机地址</param>
        /// <param name="port">端口号</param>
        public void Clear(string host, int port)
        {
            var server = connectionMultiplexer.GetServer(host, port);
            server.FlushAllDatabases();
        }

        /// <summary>
        /// 清空缓存
        /// </summary>
        /// <param name="host">主机地址</param>
        /// <param name="port">端口号</param>
        /// <param name="database">数据库</param>
        public void Clear(string host, int port, int database)
        {
            var server = connectionMultiplexer.GetServer(host, port);
            server.FlushDatabase(database);
        }
        #endregion

        #region 异步方法
        /// <summary>
        /// 清空缓存
        /// </summary>
        /// <param name="configuredOnly">默认：false</param>
        public async Task ClearAsync(bool configuredOnly = false)
        {
            var points = connectionMultiplexer.GetEndPoints(configuredOnly);
            foreach (var point in points)
            {
                var server = connectionMultiplexer.GetServer(point);
                await server.FlushAllDatabasesAsync();
            }
        }

        /// <summary>
        /// 清空缓存
        /// </summary>
        /// <param name="host">主机地址</param>
        /// <param name="port">端口号</param>
        public async Task ClearAsync(string host, int port)
        {
            var server = connectionMultiplexer.GetServer(host, port);
            await server.FlushAllDatabasesAsync();
        }

        /// <summary>
        /// 清空缓存
        /// </summary>
        /// <param name="host">主机地址</param>
        /// <param name="port">端口号</param>
        /// <param name="database">数据库</param>
        public async Task ClearAsync(string host, int port, int database)
        {
            var server = connectionMultiplexer.GetServer(host, port);
            await server.FlushDatabaseAsync(database);
        }
        #endregion
        #endregion

        #region 分布式锁
        #region 同步方法
        /// <summary>
        /// 获取redis分布式锁
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="expiry">过期时间</param>
        /// <param name="flags">标识</param>
        /// <returns></returns>
        public bool LockTake(string key, string value, TimeSpan expiry, CommandFlags flags = CommandFlags.None)
        {
            key = AddKeyPrefix(key);
            return Database.LockTake(key, value, expiry, flags);
        }

        /// <summary>
        /// 释放redis分布式锁
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="flags">标识</param>
        /// <returns></returns>
        public bool LockRelease(string key, string value, CommandFlags flags = CommandFlags.None)
        {
            key = AddKeyPrefix(key);
            return Database.LockRelease(key, value, flags);
        }
        #endregion

        #region 异步方法
        /// <summary>
        /// 获取redis分布式锁
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="expiry">过期时间</param>
        /// <param name="flags">标识</param>
        /// <returns></returns>
        public async Task<bool> LockTakeAsync(string key, string value, TimeSpan expiry, CommandFlags flags = CommandFlags.None)
        {
            key = AddKeyPrefix(key);
            return await Database.LockTakeAsync(key, value, expiry, flags);
        }

        /// <summary>
        /// 释放redis分布式锁
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="flags">标识</param>
        /// <returns></returns>
        public async Task<bool> LockReleaseAsync(string key, string value, CommandFlags flags = CommandFlags.None)
        {
            key = AddKeyPrefix(key);
            return await Database.LockReleaseAsync(key, value, flags);
        }
        #endregion
        #endregion

        #region  发布/订阅[当作消息代理中间件使用 一般使用更专业的消息队列来处理这种业务场景]
        /// <summary>
        /// 当作消息代理中间件使用
        /// 消息组建中,重要的概念便是生产者,消费者,消息中间件
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="message">消息</param>
        /// <returns>返回收到消息的客户端数量</returns>
        public long Publish(string channel, string message)
        {
            var sub = GetConnectionRedisMultiplexer().GetSubscriber();
            return sub.Publish(channel, message);
        }

        /// <summary>
        /// 在消费者端得到该消息并输出
        /// </summary>
        /// <param name="channelFrom">通道来源</param>
        /// <param name="subscribeFn">订阅处理委托</param>
        public void Subscribe(string channelFrom, Action<RedisValue> subscribeFn)
        {
            var sub = GetConnectionRedisMultiplexer().GetSubscriber();
            sub.Subscribe(channelFrom, (channel, message) => subscribeFn?.Invoke(message));
        }
        #endregion
        #endregion
    }
}
