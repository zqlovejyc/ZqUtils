#region License
/***
 * Copyright © 2018, 张强 (943620963@qq.com).
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

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using StackExchange.Redis;
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
        /// 默认的 Key 值（用来当作 RedisKey 的前缀）
        /// </summary>
        private static readonly string defaultKey = ConfigHelper.GetAppSettings<string>("Redis.DefaultKey") ?? "Redis.DefaultKey";

        /// <summary>
        /// 线程对象，线程锁使用
        /// </summary>
        private static readonly object locker = new object();

        /// <summary>
        /// redis 连接对象
        /// </summary>
        private static IConnectionMultiplexer connMultiplexer;

        /// <summary>
        /// 数据库，不能为静态字段，多个实例情况下会被覆盖
        /// </summary>
        private IDatabase db;
        #endregion

        #region 单例实例
        /// <summary>
        /// 单例实例
        /// </summary>
        public static RedisHelper Instance => SingletonHelper<RedisHelper>.GetInstance();
        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        public RedisHelper()
        {
            try
            {
                db = GetConnectionRedisMultiplexer().GetDatabase();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="defaultDatabase">数据库索引</param>
        public RedisHelper(int defaultDatabase)
        {
            try
            {
                db = GetConnectionRedisMultiplexer().GetDatabase(defaultDatabase);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>        
        /// <param name="configurationOptions">连接配置</param>
        public RedisHelper(ConfigurationOptions configurationOptions)
        {
            try
            {
                db = GetConnectionRedisMultiplexer(configurationOptions).GetDatabase();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion 构造函数

        #region 连接对象
        /// <summary>
        /// 获取 Redis 连接对象
        /// </summary>
        /// <param name="configurationOptions">连接配置</param>
        /// <returns></returns>
        public static IConnectionMultiplexer GetConnectionRedisMultiplexer(ConfigurationOptions configurationOptions = null)
        {
            if (connMultiplexer == null || !connMultiplexer.IsConnected)
            {
                lock (locker)
                {
                    if (connMultiplexer == null || !connMultiplexer.IsConnected)
                    {
                        if (configurationOptions != null)
                        {
                            connMultiplexer = ConnectionMultiplexer.Connect(configurationOptions);
                        }
                        else
                        {
                            var connectionStr = ConfigHelper.GetConnectionString("RedisConnectionString");
                            if (string.IsNullOrEmpty(connectionStr))
                            {
                                connectionStr = ConfigHelper.GetAppSettings<string>("RedisConnectionString");
                            }
                            if (!string.IsNullOrEmpty(connectionStr))
                            {
                                connMultiplexer = ConnectionMultiplexer.Connect(connectionStr);
                            }
                            else
                            {
                                throw new ArgumentNullException("RedisConnectionString和configOptions不能同时为null");
                            }
                        }
                        AddRegisterEvent();
                    }
                }
            }
            return connMultiplexer;
        }
        #endregion

        #region redis事务
        /// <summary>
        /// redis事务
        /// </summary>
        /// <returns></returns>
        public ITransaction GetTransaction()
        {
            return db.CreateTransaction();
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 添加 Key 的前缀
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private string AddKeyPrefix(string key)
        {
            return string.IsNullOrEmpty(defaultKey) ? key : $"{defaultKey}:{key}";
        }
        #endregion

        #region 注册事件
        /// <summary>
        /// 添加注册事件
        /// </summary>
        private static void AddRegisterEvent()
        {
            connMultiplexer.ConnectionRestored += ConnMultiplexer_ConnectionRestored;
            connMultiplexer.ConnectionFailed += ConnMultiplexer_ConnectionFailed;
            connMultiplexer.ErrorMessage += ConnMultiplexer_ErrorMessage;
            connMultiplexer.ConfigurationChanged += ConnMultiplexer_ConfigurationChanged;
            connMultiplexer.HashSlotMoved += ConnMultiplexer_HashSlotMoved;
            connMultiplexer.InternalError += ConnMultiplexer_InternalError;
            connMultiplexer.ConfigurationChangedBroadcast += ConnMultiplexer_ConfigurationChangedBroadcast;
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

        #region 公有方法
        #region string 操作
        #region string-同步
        /// <summary>
        /// 设置 key 并保存字符串（如果 key 已存在，则覆盖值）
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="redisValue"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public bool StringSet(string redisKey, string redisValue, TimeSpan? expiry = null)
        {
            redisKey = AddKeyPrefix(redisKey);
            return db.StringSet(redisKey, redisValue, expiry);
        }

        /// <summary>
        /// 保存多个 Key-value
        /// </summary>
        /// <param name="keyValuePairs"></param>
        /// <returns></returns>
        public bool StringSet(IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            var pairs = keyValuePairs.Select(x => new KeyValuePair<RedisKey, RedisValue>(AddKeyPrefix(x.Key), x.Value));
            return db.StringSet(pairs.ToArray());
        }

        /// <summary>
        /// 获取字符串
        /// </summary>
        /// <param name="redisKey"></param>
        /// <returns></returns>
        public string StringGet(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return db.StringGet(redisKey);
        }

        /// <summary>
        /// 存储一个对象（该对象会被序列化保存）
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="redisValue"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public bool StringSet<T>(string redisKey, T redisValue, TimeSpan? expiry = null)
        {
            redisKey = AddKeyPrefix(redisKey);
            return db.StringSet(redisKey, redisValue.ToJson(), expiry);
        }

        /// <summary>
        /// 获取一个对象（会进行反序列化）
        /// </summary>
        /// <param name="redisKey"></param>
        /// <returns></returns>
        public T StringGet<T>(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return db.StringGet(redisKey).ToString().ToObject<T>();
        }
        #endregion

        #region string-异步
        /// <summary>
        /// 保存一个字符串值
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="redisValue"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public async Task<bool> StringSetAsync(string redisKey, string redisValue, TimeSpan? expiry = null)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await db.StringSetAsync(redisKey, redisValue, expiry);
        }

        /// <summary>
        /// 保存一组字符串值
        /// </summary>
        /// <param name="keyValuePairs"></param>
        /// <returns></returns>
        public async Task<bool> StringSetAsync(IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            var pairs = keyValuePairs.Select(x => new KeyValuePair<RedisKey, RedisValue>(AddKeyPrefix(x.Key), x.Value));
            return await db.StringSetAsync(pairs.ToArray());
        }

        /// <summary>
        /// 获取单个值
        /// </summary>
        /// <param name="redisKey"></param>
        /// <returns></returns>
        public async Task<string> StringGetAsync(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await db.StringGetAsync(redisKey);
        }

        /// <summary>
        /// 存储一个对象（该对象会被序列化保存）
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="redisValue"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public async Task<bool> StringSetAsync<T>(string redisKey, T redisValue, TimeSpan? expiry = null)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await db.StringSetAsync(redisKey, redisValue.ToJson(), expiry);
        }

        /// <summary>
        /// 获取一个对象（会进行反序列化）
        /// </summary>
        /// <param name="redisKey"></param>
        /// <returns></returns>
        public async Task<T> StringGetAsync<T>(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return (await db.StringGetAsync(redisKey)).ToString().ToObject<T>();
        }
        #endregion
        #endregion

        #region Hash 操作
        #region Hash-同步
        /// <summary>
        /// 判断该字段是否存在 hash 中
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="hashField"></param>
        /// <returns></returns>
        public bool HashExists(string redisKey, string hashField)
        {
            redisKey = AddKeyPrefix(redisKey);
            return db.HashExists(redisKey, hashField);
        }

        /// <summary>
        /// 从 hash 中移除指定字段
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="hashField"></param>
        /// <returns></returns>
        public bool HashDelete(string redisKey, string hashField)
        {
            redisKey = AddKeyPrefix(redisKey);
            return db.HashDelete(redisKey, hashField);
        }

        /// <summary>
        /// 从 hash 中移除指定字段
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="hashFields"></param>
        /// <returns></returns>
        public long HashDelete(string redisKey, IEnumerable<string> hashFields)
        {
            redisKey = AddKeyPrefix(redisKey);
            var fields = hashFields.Select(x => (RedisValue)x);
            return db.HashDelete(redisKey, fields.ToArray());
        }

        /// <summary>
        /// 从 hash 中移除指定字段
        /// </summary>
        /// <param name="redisKey"></param>        
        /// <returns></returns>
        public long HashDelete(string redisKey)
        {
            var hashFileds = HashKeys(redisKey);
            return HashDelete(redisKey, hashFileds);
        }

        /// <summary>
        /// 在 hash 设定值
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="hashField"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool HashSet(string redisKey, string hashField, string value)
        {
            redisKey = AddKeyPrefix(redisKey);
            return db.HashSet(redisKey, hashField, value);
        }

        /// <summary>
        /// 在 hash 中设定值
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="hashFields"></param>
        public void HashSet(string redisKey, IEnumerable<KeyValuePair<string, string>> hashFields)
        {
            redisKey = AddKeyPrefix(redisKey);
            var entries = hashFields.Select(x => new HashEntry(x.Key, x.Value));
            db.HashSet(redisKey, entries.ToArray());
        }

        /// <summary>
        /// 在 hash 中获取值
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="hashField"></param>
        /// <returns></returns>
        public string HashGet(string redisKey, string hashField)
        {
            redisKey = AddKeyPrefix(redisKey);
            return db.HashGet(redisKey, hashField);
        }

        /// <summary>
        /// 在 hash 中获取值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="redisKey"></param>
        /// <param name="hashFields"></param>
        /// <returns></returns>
        public IEnumerable<T> HashGet<T>(string redisKey, IEnumerable<string> hashFields)
        {
            redisKey = AddKeyPrefix(redisKey);
            var fields = hashFields.Select(x => (RedisValue)x);
            var result = db.HashGet(redisKey, fields.ToArray()).Select(o => o.ToString());
            return result.Select(o => o.ToString().ToObject<T>());
        }

        /// <summary>
        /// 在 hash 中获取值
        /// </summary>
        /// <param name="redisKey"></param>
        /// <returns></returns>
        public IEnumerable<T> HashGet<T>(string redisKey)
        {
            var hashFileds = HashKeys(redisKey);
            return HashGet<T>(redisKey, hashFileds);
        }

        /// <summary>
        /// 从 hash 返回所有的字段值
        /// </summary>
        /// <param name="redisKey"></param>
        /// <returns></returns>
        public IEnumerable<string> HashKeys(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return db.HashKeys(redisKey).Select(o => o.ToString());
        }

        /// <summary>
        /// 返回 hash 中的所有值
        /// </summary>
        /// <param name="redisKey"></param>
        /// <returns></returns>
        public IEnumerable<string> HashValues(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return db.HashValues(redisKey).Select(o => o.ToString());
        }

        /// <summary>
        /// 在 hash 设定值（序列化）
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="hashField"></param>
        /// <param name="redisValue"></param>
        /// <returns></returns>
        public bool HashSet<T>(string redisKey, string hashField, T redisValue)
        {
            redisKey = AddKeyPrefix(redisKey);
            return db.HashSet(redisKey, hashField, redisValue.ToJson());
        }

        /// <summary>
        /// 在 hash 中获取值（反序列化）
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="hashField"></param>
        /// <returns></returns>
        public T HashGet<T>(string redisKey, string hashField)
        {
            redisKey = AddKeyPrefix(redisKey);
            return db.HashGet(redisKey, hashField).ToString().ToObject<T>();
        }
        #endregion

        #region Hash-异步
        /// <summary>
        /// 判断该字段是否存在 hash 中
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="hashField"></param>
        /// <returns></returns>
        public async Task<bool> HashExistsAsync(string redisKey, string hashField)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await db.HashExistsAsync(redisKey, hashField);
        }

        /// <summary>
        /// 从 hash 中移除指定字段
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="hashField"></param>
        /// <returns></returns>
        public async Task<bool> HashDeleteAsync(string redisKey, string hashField)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await db.HashDeleteAsync(redisKey, hashField);
        }

        /// <summary>
        /// 从 hash 中移除指定字段
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="hashFields"></param>
        /// <returns></returns>
        public async Task<long> HashDeleteAsync(string redisKey, IEnumerable<string> hashFields)
        {
            redisKey = AddKeyPrefix(redisKey);
            var fields = hashFields.Select(x => (RedisValue)x);
            return await db.HashDeleteAsync(redisKey, fields.ToArray());
        }

        /// <summary>
        /// 从 hash 中移除指定字段
        /// </summary>
        /// <param name="redisKey"></param>        
        /// <returns></returns>
        public async Task<long> HashDeleteAsync(string redisKey)
        {
            var hashFileds = await HashKeysAsync(redisKey);
            return await HashDeleteAsync(redisKey, hashFileds);
        }

        /// <summary>
        /// 在 hash 设定值
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="hashField"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<bool> HashSetAsync(string redisKey, string hashField, string value)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await db.HashSetAsync(redisKey, hashField, value);
        }

        /// <summary>
        /// 在 hash 中设定值
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="hashFields"></param>
        public async Task HashSetAsync(string redisKey, IEnumerable<KeyValuePair<string, string>> hashFields)
        {
            redisKey = AddKeyPrefix(redisKey);
            var entries = hashFields.Select(x => new HashEntry(AddKeyPrefix(x.Key), x.Value));
            await db.HashSetAsync(redisKey, entries.ToArray());
        }

        /// <summary>
        /// 在 hash 中获取值
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="hashField"></param>
        /// <returns></returns>
        public async Task<string> HashGetAsync(string redisKey, string hashField)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await db.HashGetAsync(redisKey, hashField);
        }

        /// <summary>
        /// 在 hash 中获取值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="redisKey"></param>
        /// <param name="hashFields"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> HashGetAsync<T>(string redisKey, IEnumerable<string> hashFields)
        {
            redisKey = AddKeyPrefix(redisKey);
            var fields = hashFields.Select(x => (RedisValue)x);
            var result = (await db.HashGetAsync(redisKey, fields.ToArray())).Select(o => o.ToString());
            return result.Select(o => o.ToString().ToObject<T>());
        }

        /// <summary>
        /// 在 hash 中获取值
        /// </summary>
        /// <param name="redisKey"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> HashGetAsync<T>(string redisKey)
        {
            var hashFileds = await HashKeysAsync(redisKey);
            return await HashGetAsync<T>(redisKey, hashFileds);
        }

        /// <summary>
        /// 从 hash 返回所有的字段值
        /// </summary>
        /// <param name="redisKey"></param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> HashKeysAsync(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return (await db.HashKeysAsync(redisKey)).Select(o => o.ToString());
        }

        /// <summary>
        /// 返回 hash 中的所有值
        /// </summary>
        /// <param name="redisKey"></param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> HashValuesAsync(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return (await db.HashValuesAsync(redisKey)).Select(o => o.ToString());
        }

        /// <summary>
        /// 在 hash 设定值（序列化）
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="hashField"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<bool> HashSetAsync<T>(string redisKey, string hashField, T value)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await db.HashSetAsync(redisKey, hashField, value.ToJson());
        }

        /// <summary>
        /// 在 hash 中获取值（反序列化）
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="hashField"></param>
        /// <returns></returns>
        public async Task<T> HashGetAsync<T>(string redisKey, string hashField)
        {
            redisKey = AddKeyPrefix(redisKey);
            return (await db.HashGetAsync(redisKey, hashField)).ToString().ToObject<T>();
        }
        #endregion
        #endregion

        #region List 操作
        #region List-同步
        /// <summary>
        /// 移除并返回存储在该键列表的第一个元素
        /// </summary>
        /// <param name="redisKey"></param>
        /// <returns></returns>
        public string ListLeftPop(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return db.ListLeftPop(redisKey);
        }

        /// <summary>
        /// 移除并返回存储在该键列表的最后一个元素
        /// </summary>
        /// <param name="redisKey"></param>
        /// <returns></returns>
        public string ListRightPop(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return db.ListRightPop(redisKey);
        }

        /// <summary>
        /// 移除列表指定键上与该值相同的元素
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="redisValue"></param>
        /// <returns></returns>
        public long ListRemove(string redisKey, string redisValue)
        {
            redisKey = AddKeyPrefix(redisKey);
            return db.ListRemove(redisKey, redisValue);
        }

        /// <summary>
        /// 在列表尾部插入值。如果键不存在，先创建再插入值
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="redisValue"></param>
        /// <returns></returns>
        public long ListRightPush(string redisKey, string redisValue)
        {
            redisKey = AddKeyPrefix(redisKey);
            return db.ListRightPush(redisKey, redisValue);
        }

        /// <summary>
        /// 在列表头部插入值。如果键不存在，先创建再插入值
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="redisValue"></param>
        /// <returns></returns>
        public long ListLeftPush(string redisKey, string redisValue)
        {
            redisKey = AddKeyPrefix(redisKey);
            return db.ListLeftPush(redisKey, redisValue);
        }

        /// <summary>
        /// 返回列表上该键的长度，如果不存在，返回 0
        /// </summary>
        /// <param name="redisKey"></param>
        /// <returns></returns>
        public long ListLength(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return db.ListLength(redisKey);
        }

        /// <summary>
        /// 返回在该列表上键所对应的元素
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns></returns>
        public IEnumerable<string> ListRange(string redisKey, long start = 0L, long stop = -1L)
        {
            redisKey = AddKeyPrefix(redisKey);
            return db.ListRange(redisKey, start, stop).Select(o => o.ToString());
        }

        /// <summary>
        /// 移除并返回存储在该键列表的第一个元素
        /// </summary>
        /// <param name="redisKey"></param>
        /// <returns></returns>
        public T ListLeftPop<T>(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return db.ListLeftPop(redisKey).ToString().ToObject<T>();
        }

        /// <summary>
        /// 移除并返回存储在该键列表的最后一个元素
        /// </summary>
        /// <param name="redisKey"></param>
        /// <returns></returns>
        public T ListRightPop<T>(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return db.ListRightPop(redisKey).ToString().ToObject<T>();
        }

        /// <summary>
        /// 在列表尾部插入值。如果键不存在，先创建再插入值
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="redisValue"></param>
        /// <returns></returns>
        public long ListRightPush<T>(string redisKey, T redisValue)
        {
            redisKey = AddKeyPrefix(redisKey);
            return db.ListRightPush(redisKey, redisValue.ToJson());
        }

        /// <summary>
        /// 在列表头部插入值。如果键不存在，先创建再插入值
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="redisValue"></param>
        /// <returns></returns>
        public long ListLeftPush<T>(string redisKey, T redisValue)
        {
            redisKey = AddKeyPrefix(redisKey);
            return db.ListLeftPush(redisKey, redisValue.ToJson());
        }

        /// <summary>
        /// 队列入队
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="redisKey"></param>
        /// <param name="redisValue"></param>
        /// <returns></returns>
        public long EnqueueItemOnList<T>(string redisKey, T redisValue)
        {
            return ListRightPush(redisKey, redisValue);
        }

        /// <summary>
        /// 队列出队
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="redisKey"></param>
        /// <returns></returns>
        public T DequeueItemFromList<T>(string redisKey)
        {
            return ListLeftPop<T>(redisKey);
        }

        /// <summary>
        /// 获取队列长度
        /// </summary>
        /// <param name="redisKey"></param>
        /// <returns></returns>
        public long GetQueueLength(string redisKey)
        {
            return ListLength(redisKey);
        }
        #endregion

        #region List-异步
        /// <summary>
        /// 移除并返回存储在该键列表的第一个元素
        /// </summary>
        /// <param name="redisKey"></param>
        /// <returns></returns>
        public async Task<string> ListLeftPopAsync(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await db.ListLeftPopAsync(redisKey);
        }

        /// <summary>
        /// 移除并返回存储在该键列表的最后一个元素
        /// </summary>
        /// <param name="redisKey"></param>
        /// <returns></returns>
        public async Task<string> ListRightPopAsync(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await db.ListRightPopAsync(redisKey);
        }

        /// <summary>
        /// 移除列表指定键上与该值相同的元素
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="redisValue"></param>
        /// <returns></returns>
        public async Task<long> ListRemoveAsync(string redisKey, string redisValue)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await db.ListRemoveAsync(redisKey, redisValue);
        }

        /// <summary>
        /// 在列表尾部插入值。如果键不存在，先创建再插入值
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="redisValue"></param>
        /// <returns></returns>
        public async Task<long> ListRightPushAsync(string redisKey, string redisValue)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await db.ListRightPushAsync(redisKey, redisValue);
        }

        /// <summary>
        /// 在列表头部插入值。如果键不存在，先创建再插入值
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="redisValue"></param>
        /// <returns></returns>
        public async Task<long> ListLeftPushAsync(string redisKey, string redisValue)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await db.ListLeftPushAsync(redisKey, redisValue);
        }

        /// <summary>
        /// 返回列表上该键的长度，如果不存在，返回 0
        /// </summary>
        /// <param name="redisKey"></param>
        /// <returns></returns>
        public async Task<long> ListLengthAsync(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await db.ListLengthAsync(redisKey);
        }

        /// <summary>
        /// 返回在该列表上键所对应的元素
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> ListRangeAsync(string redisKey, long start = 0L, long stop = -1L)
        {
            redisKey = AddKeyPrefix(redisKey);
            var query = await db.ListRangeAsync(redisKey, start, stop);
            return query.Select(x => x.ToString());
        }

        /// <summary>
        /// 移除并返回存储在该键列表的第一个元素
        /// </summary>
        /// <param name="redisKey"></param>
        /// <returns></returns>
        public async Task<T> ListLeftPopAsync<T>(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return (await db.ListLeftPopAsync(redisKey)).ToString().ToObject<T>();
        }

        /// <summary>
        /// 移除并返回存储在该键列表的最后一个元素
        /// </summary>
        /// <param name="redisKey"></param>
        /// <returns></returns>
        public async Task<T> ListRightPopAsync<T>(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return (await db.ListRightPopAsync(redisKey)).ToString().ToObject<T>();
        }

        /// <summary>
        /// 在列表尾部插入值。如果键不存在，先创建再插入值
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="redisValue"></param>
        /// <returns></returns>
        public async Task<long> ListRightPushAsync<T>(string redisKey, T redisValue)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await db.ListRightPushAsync(redisKey, redisValue.ToJson());
        }

        /// <summary>
        /// 在列表头部插入值。如果键不存在，先创建再插入值
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="redisValue"></param>
        /// <returns></returns>
        public async Task<long> ListLeftPushAsync<T>(string redisKey, T redisValue)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await db.ListLeftPushAsync(redisKey, redisValue.ToJson());
        }

        /// <summary>
        /// 队列入队
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="redisKey"></param>
        /// <param name="redisValue"></param>
        /// <returns></returns>
        public async Task<long> EnqueueItemOnListAsync<T>(string redisKey, T redisValue)
        {
            return await ListRightPushAsync(redisKey, redisValue);
        }

        /// <summary>
        /// 队列出队
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="redisKey"></param>
        /// <returns></returns>
        public async Task<T> DequeueItemFromListAsync<T>(string redisKey)
        {
            return await ListLeftPopAsync<T>(redisKey);
        }

        /// <summary>
        /// 获取队列长度
        /// </summary>
        /// <param name="redisKey"></param>
        /// <returns></returns>
        public async Task<long> GetQueueLengthAsync(string redisKey)
        {
            return await ListLengthAsync(redisKey);
        }
        #endregion
        #endregion

        #region SortedSet 操作
        #region SortedSet-同步
        /// <summary>
        /// SortedSet 新增
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="member"></param>
        /// <param name="score"></param>
        /// <returns></returns>
        public bool SortedSetAdd(string redisKey, string member, double score)
        {
            redisKey = AddKeyPrefix(redisKey);
            return db.SortedSetAdd(redisKey, member, score);
        }

        /// <summary>
        /// SortedSet 新增
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="member"></param>
        /// <param name="score"></param>
        /// <returns></returns>
        public bool SortedSetAdd<T>(string redisKey, T member, double score)
        {
            redisKey = AddKeyPrefix(redisKey);
            return SortedSetAdd(redisKey, member.ToJson(), score);
        }

        /// <summary>
        /// SortedSet 移除
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="memebr"></param>
        /// <returns></returns>
        public bool SortedSetRemove(string redisKey, string memebr)
        {
            redisKey = AddKeyPrefix(redisKey);
            return db.SortedSetRemove(redisKey, memebr);
        }

        /// <summary>
        /// SortedSet 移除
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns></returns>
        public long SortedSetRemoveRangeByRank(string redisKey, long start, long stop)
        {
            redisKey = AddKeyPrefix(redisKey);
            return db.SortedSetRemoveRangeByRank(redisKey, start, stop);
        }

        /// <summary>
        /// SortedSet 移除
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns></returns>
        public long SortedSetRemoveRangeByScore(string redisKey, double start, double stop)
        {
            redisKey = AddKeyPrefix(redisKey);
            return db.SortedSetRemoveRangeByScore(redisKey, start, stop);
        }

        /// <summary>
        /// SortedSet 移除
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public long SortedSetRemoveRangeByValue(string redisKey, string min, string max)
        {
            redisKey = AddKeyPrefix(redisKey);
            return db.SortedSetRemoveRangeByValue(redisKey, min, max);
        }

        /// <summary>
        /// 增量的得分排序的集合中的成员存储键值键按增量
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="member"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public double SortedSetIncrement(string redisKey, string member, double value = 1)
        {
            redisKey = AddKeyPrefix(redisKey);
            return db.SortedSetIncrement(redisKey, member, value);
        }

        /// <summary>
        /// 减量的得分排序的集合中的成员存储键值键按减量
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="member"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public double SortedSetDecrement(string redisKey, string member, double value)
        {
            redisKey = AddKeyPrefix(redisKey);
            return db.SortedSetDecrement(redisKey, member, value);
        }

        /// <summary>
        /// 返回有序集合的元素个数
        /// </summary>
        /// <param name="redisKey"></param>
        /// <returns></returns>
        public long SortedSetLength(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return db.SortedSetLength(redisKey);
        }

        /// <summary>
        /// 获取集合中的索引位置，从0开始
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="member"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public long? SortedSetRank(string redisKey, string member, Order order = Order.Ascending)
        {
            redisKey = AddKeyPrefix(redisKey);
            return db.SortedSetRank(redisKey, member, order);
        }

        /// <summary>
        /// 获取Score
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="memebr"></param>
        /// <returns></returns>
        public double? SortedSetScore(string redisKey, string memebr)
        {
            redisKey = AddKeyPrefix(redisKey);
            return db.SortedSetScore(redisKey, memebr);
        }

        /// <summary>
        /// 在有序集合中返回指定范围的元素，默认情况下从低到高。
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <param name="order"></param>        
        /// <returns></returns>
        public IEnumerable<string> SortedSetRangeByRank(string redisKey, long start = 0, long stop = -1, Order order = Order.Ascending)
        {
            redisKey = AddKeyPrefix(redisKey);
            return db.SortedSetRangeByRank(redisKey, start, stop, order).Select(x => x.ToString());
        }

        /// <summary>
        /// 在有序集合中返回指定范围的元素，默认情况下从低到高。
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <param name="order"></param>        
        /// <returns></returns>
        public Dictionary<string, double> SortedSetRangeByRankWithScores(string redisKey, long start = 0, long stop = -1, Order order = Order.Ascending)
        {
            redisKey = AddKeyPrefix(redisKey);
            var result = db.SortedSetRangeByRankWithScores(redisKey, start, stop, order);
            return result.Select(x => new { x.Score, Value = x.Element.ToString() }).ToDictionary(x => x.Value, x => x.Score);
        }

        /// <summary>
        /// 在有序集合中返回指定范围的元素，默认情况下从低到高。
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public IEnumerable<string> SortedSetRangeByScore(string redisKey, double start = double.NegativeInfinity, double stop = double.PositiveInfinity, long skip = 0, long take = -1, Order order = Order.Ascending)
        {
            redisKey = AddKeyPrefix(redisKey);
            return db.SortedSetRangeByScore(redisKey, start, stop, order: order, skip: skip, take: take).Select(o => o.ToString());
        }

        /// <summary>
        /// 在有序集合中返回指定范围的元素，默认情况下从低到高。
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public Dictionary<string, double> SortedSetRangeByScoreWithScores(string redisKey, double start = double.NegativeInfinity, double stop = double.PositiveInfinity, long skip = 0, long take = -1, Order order = Order.Ascending)
        {
            redisKey = AddKeyPrefix(redisKey);
            var result = db.SortedSetRangeByScoreWithScores(redisKey, start, stop, order: order, skip: skip, take: take);
            return result.Select(x => new { x.Score, Value = x.Element.ToString() }).ToDictionary(x => x.Value, x => x.Score);
        }

        /// <summary>
        /// 当有序集合中的Score相同时，按照值从小到大进行排序
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>        
        /// <returns></returns>
        public IEnumerable<string> SortedSetRangeByValue(string redisKey, RedisValue min = default(RedisValue), RedisValue max = default(RedisValue), long skip = 0, long take = -1)
        {
            redisKey = AddKeyPrefix(redisKey);
            return db.SortedSetRangeByValue(redisKey, min, max, skip: skip, take: take).Select(o => o.ToString());
        }
        #endregion

        #region SortedSet-异步
        /// <summary>
        /// SortedSet 新增
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="member"></param>
        /// <param name="score"></param>
        /// <returns></returns>
        public async Task<bool> SortedSetAddAsync(string redisKey, string member, double score)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await db.SortedSetAddAsync(redisKey, member, score);
        }

        /// <summary>
        /// SortedSet 新增
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="member"></param>
        /// <param name="score"></param>
        /// <returns></returns>
        public async Task<bool> SortedSetAddAsync<T>(string redisKey, T member, double score)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await SortedSetAddAsync(redisKey, member.ToJson(), score);
        }

        /// <summary>
        /// SortedSet 移除
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="memebr"></param>
        /// <returns></returns>
        public async Task<bool> SortedSetRemoveAsync(string redisKey, string memebr)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await db.SortedSetRemoveAsync(redisKey, memebr);
        }

        /// <summary>
        /// SortedSet 移除
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns></returns>
        public async Task<long> SortedSetRemoveRangeByRankAsync(string redisKey, long start, long stop)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await db.SortedSetRemoveRangeByRankAsync(redisKey, start, stop);
        }

        /// <summary>
        /// SortedSet 移除
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns></returns>
        public async Task<long> SortedSetRemoveRangeByScoreAsync(string redisKey, double start, double stop)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await db.SortedSetRemoveRangeByScoreAsync(redisKey, start, stop);
        }

        /// <summary>
        /// SortedSet 移除
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public async Task<long> SortedSetRemoveRangeByValueAsync(string redisKey, string min, string max)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await db.SortedSetRemoveRangeByValueAsync(redisKey, min, max);
        }

        /// <summary>
        /// 增量的得分排序的集合中的成员存储键值键按增量
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="member"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Task<double> SortedSetIncrementAsync(string redisKey, string member, double value = 1)
        {
            redisKey = AddKeyPrefix(redisKey);
            return db.SortedSetIncrementAsync(redisKey, member, value);
        }

        /// <summary>
        /// 减量的得分排序的集合中的成员存储键值键按减量
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="member"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<double> SortedSetDecrementAsync(string redisKey, string member, double value)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await db.SortedSetDecrementAsync(redisKey, member, value);
        }

        /// <summary>
        /// 返回有序集合的元素个数
        /// </summary>
        /// <param name="redisKey"></param>
        /// <returns></returns>
        public async Task<long> SortedSetLengthAsync(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await db.SortedSetLengthAsync(redisKey);
        }

        /// <summary>
        /// 获取集合中的索引位置，从0开始
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="member"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public async Task<long?> SortedSetRankAsync(string redisKey, string member, Order order = Order.Ascending)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await db.SortedSetRankAsync(redisKey, member, order);
        }

        /// <summary>
        /// 获取Score
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="memebr"></param>
        /// <returns></returns>
        public async Task<double?> SortedSetScoreAsync(string redisKey, string memebr)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await db.SortedSetScoreAsync(redisKey, memebr);
        }

        /// <summary>
        /// 在有序集合中返回指定范围的元素，默认情况下从低到高。
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> SortedSetRangeByRankAsync(string redisKey, long start = 0, long stop = -1, Order order = Order.Ascending)
        {
            redisKey = AddKeyPrefix(redisKey);
            return (await db.SortedSetRangeByRankAsync(redisKey, start, stop, order)).Select(o => o.ToString());
        }

        /// <summary>
        /// 在有序集合中返回指定范围的元素，默认情况下从低到高。
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <param name="order"></param>        
        /// <returns></returns>
        public async Task<Dictionary<string, double>> SortedSetRangeByRankWithScoresAsync(string redisKey, long start = 0, long stop = -1, Order order = Order.Ascending)
        {
            redisKey = AddKeyPrefix(redisKey);
            var result = await db.SortedSetRangeByRankWithScoresAsync(redisKey, start, stop, order);
            return result.Select(x => new { x.Score, Value = x.Element.ToString() }).ToDictionary(x => x.Value, x => x.Score);
        }

        /// <summary>
        /// 在有序集合中返回指定范围的元素，默认情况下从低到高。
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> SortedSetRangeByScoreAsync(string redisKey, double start = double.NegativeInfinity, double stop = double.PositiveInfinity, long skip = 0, long take = -1, Order order = Order.Ascending)
        {
            redisKey = AddKeyPrefix(redisKey);
            return (await db.SortedSetRangeByScoreAsync(redisKey, start, stop, order: order, skip: skip, take: take)).Select(o => o.ToString());
        }

        /// <summary>
        /// 在有序集合中返回指定范围的元素，默认情况下从低到高。
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, double>> SortedSetRangeByScoreWithScoresAsync(string redisKey, double start = double.NegativeInfinity, double stop = double.PositiveInfinity, long skip = 0, long take = -1, Order order = Order.Ascending)
        {
            redisKey = AddKeyPrefix(redisKey);
            var result = await db.SortedSetRangeByScoreWithScoresAsync(redisKey, start, stop, order: order, skip: skip, take: take);
            return result.Select(x => new { x.Score, Value = x.Element.ToString() }).ToDictionary(x => x.Value, x => x.Score);
        }

        /// <summary>
        /// 当有序集合中的Score相同时，按照值从小到大进行排序
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> SortedSetRangeByValueAsync(string redisKey, RedisValue min = default(RedisValue), RedisValue max = default(RedisValue), long skip = 0, long take = -1)
        {
            redisKey = AddKeyPrefix(redisKey);
            return (await db.SortedSetRangeByValueAsync(redisKey, min, max, skip: skip, take: take)).Select(o => o.ToString());
        }
        #endregion
        #endregion

        #region key 操作
        #region key-同步
        /// <summary>
        /// 移除指定 Key
        /// </summary>
        /// <param name="redisKey"></param>
        /// <returns></returns>
        public bool KeyDelete(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return db.KeyDelete(redisKey);
        }

        /// <summary>
        /// 移除指定 Key
        /// </summary>
        /// <param name="redisKeys"></param>
        /// <returns></returns>
        public long KeyDelete(IEnumerable<string> redisKeys)
        {
            var keys = redisKeys.Select(x => (RedisKey)AddKeyPrefix(x));
            return db.KeyDelete(keys.ToArray());
        }

        /// <summary>
        /// 校验 Key 是否存在
        /// </summary>
        /// <param name="redisKey"></param>
        /// <returns></returns>
        public bool KeyExists(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return db.KeyExists(redisKey);
        }

        /// <summary>
        /// 重命名 Key
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="redisNewKey"></param>
        /// <returns></returns>
        public bool KeyRename(string redisKey, string redisNewKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return db.KeyRename(redisKey, redisNewKey);
        }

        /// <summary>
        /// 设置 Key 的时间
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public bool KeyExpire(string redisKey, TimeSpan? expiry)
        {
            redisKey = AddKeyPrefix(redisKey);
            return db.KeyExpire(redisKey, expiry);
        }

        /// <summary>
        /// 根据通配符 * 删除 Key
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="database"></param>
        /// <param name="configuredOnly"></param>
        public bool KeyDeleteByPattern(string pattern, int database = 0, bool configuredOnly = false)
        {
            var result = true;
            var points = connMultiplexer.GetEndPoints(configuredOnly);
            if (points?.Length > 0)
            {
                foreach (var point in points)
                {
                    var server = connMultiplexer.GetServer(point);
                    var keys = server.Keys(database: database, pattern: $"*{pattern}*");
                    foreach (var key in keys)
                    {
                        if (!db.KeyDelete(key))
                        {
                            result = false;
                        }
                    }
                }
            }
            else
            {
                result = false;
            }
            return result;
        }
        #endregion

        #region key-异步
        /// <summary>
        /// 移除指定 Key
        /// </summary>
        /// <param name="redisKey"></param>
        /// <returns></returns>
        public async Task<bool> KeyDeleteAsync(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await db.KeyDeleteAsync(redisKey);
        }

        /// <summary>
        /// 移除指定 Key
        /// </summary>
        /// <param name="redisKeys"></param>
        /// <returns></returns>
        public async Task<long> KeyDeleteAsync(IEnumerable<string> redisKeys)
        {
            var keys = redisKeys.Select(x => (RedisKey)AddKeyPrefix(x));
            return await db.KeyDeleteAsync(keys.ToArray());
        }

        /// <summary>
        /// 校验 Key 是否存在
        /// </summary>
        /// <param name="redisKey"></param>
        /// <returns></returns>
        public async Task<bool> KeyExistsAsync(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await db.KeyExistsAsync(redisKey);
        }

        /// <summary>
        /// 重命名 Key
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="redisNewKey"></param>
        /// <returns></returns>
        public async Task<bool> KeyRenameAsync(string redisKey, string redisNewKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await db.KeyRenameAsync(redisKey, redisNewKey);
        }

        /// <summary>
        /// 设置 Key 的时间
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public async Task<bool> KeyExpireAsync(string redisKey, TimeSpan? expiry)
        {
            redisKey = AddKeyPrefix(redisKey);
            return await db.KeyExpireAsync(redisKey, expiry);
        }

        /// <summary>
        /// 根据通配符 * 删除 Key
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="database"></param>
        /// <param name="configuredOnly"></param>
        public async Task<bool> KeyDeleteByPatternAsync(string pattern, int database = 0, bool configuredOnly = false)
        {
            var result = true;
            var points = connMultiplexer.GetEndPoints(configuredOnly);
            if (points?.Length > 0)
            {
                foreach (var point in points)
                {
                    var server = connMultiplexer.GetServer(point);
                    var keys = server.Keys(database: database, pattern: $"*{pattern}*");
                    foreach (var key in keys)
                    {
                        if (!await db.KeyDeleteAsync(key))
                        {
                            result = false;
                        }
                    }
                }
            }
            else
            {
                result = false;
            }
            return result;
        }
        #endregion
        #endregion key 操作

        #region 清空缓存
        #region Sync
        /// <summary>
        /// 清空缓存
        /// </summary>
        /// <param name="configuredOnly">默认：false</param>
        public void Clear(bool configuredOnly = false)
        {
            var points = connMultiplexer.GetEndPoints(configuredOnly);
            foreach (var point in points)
            {
                var server = connMultiplexer.GetServer(point);
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
            var server = connMultiplexer.GetServer(host, port);
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
            var server = connMultiplexer.GetServer(host, port);
            server.FlushDatabase(database);
        }
        #endregion

        #region Async
        /// <summary>
        /// 清空缓存
        /// </summary>
        /// <param name="configuredOnly">默认：false</param>
        /// <returns></returns>
        public async Task ClearAsync(bool configuredOnly = false)
        {
            var points = connMultiplexer.GetEndPoints(configuredOnly);
            foreach (var point in points)
            {
                var server = connMultiplexer.GetServer(point);
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
            var server = connMultiplexer.GetServer(host, port);
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
            var server = connMultiplexer.GetServer(host, port);
            await server.FlushDatabaseAsync(database);
        }
        #endregion
        #endregion

        #region  发布/订阅[当作消息代理中间件使用 一般使用更专业的消息队列来处理这种业务场景]
        /// <summary>
        /// 当作消息代理中间件使用
        /// 消息组建中,重要的概念便是生产者,消费者,消息中间件
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public long Publish(string channel, string message)
        {
            var sub = GetConnectionRedisMultiplexer().GetSubscriber();
            return sub.Publish(channel, message);
        }

        /// <summary>
        /// 在消费者端得到该消息并输出
        /// </summary>
        /// <param name="channelFrom"></param>
        /// <param name="subscribeFn"></param>
        /// <returns></returns>
        public void Subscribe(string channelFrom, Action<RedisValue> subscribeFn)
        {
            var sub = GetConnectionRedisMultiplexer().GetSubscriber();
            sub.Subscribe(channelFrom, (channel, message) =>
            {
                subscribeFn?.Invoke(message);
            });
        }
        #endregion
        #endregion
    }
}
