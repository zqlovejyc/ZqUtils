#region License
/***
 * Copyright © 2018-2021, 张强 (943620963@qq.com).
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
using System.Collections.Generic;
using System.IO;
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
    /// Redis工具类，支持Redis单例连接和连接池两种模式
    /// </summary>
    public class RedisHelper
    {
        #region 私有字段
        /// <summary>
        /// 线程锁对象
        /// </summary>
        private static readonly object _lock = new();

        /// <summary>
        /// 线程锁
        /// </summary>
        private static readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

        /// <summary>
        /// redis单例连接对象
        /// </summary>
        private static IConnectionMultiplexer _singleConnection;

        /// <summary>
        /// redis连接池
        /// </summary>
        private readonly RedisConnectionPoolManager _poolManager;

        /// <summary>
        /// redis连接池创建的连接对象
        /// </summary>
        private readonly IConnectionMultiplexer _poolConnection;
        #endregion

        #region 公有属性
        /// <summary>
        /// 静态单例
        /// </summary>
        public static RedisHelper Instance => SingletonHelper<RedisHelper>.GetInstance();

        /// <summary>
        /// 当前Redis连接对象
        /// </summary>
        public IConnectionMultiplexer RedisConnection => _poolConnection ?? _singleConnection;

        /// <summary>
        /// Redis连接池
        /// </summary>
        public RedisConnectionPoolManager RedisConnectionPoolManager => _poolManager;

        /// <summary>
        /// 数据库
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
        /// <param name="connection">redis连接对象</param>
        public RedisHelper(
            IConnectionMultiplexer connection) =>
            Database = (_singleConnection = connection).GetDatabase();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="defaultDatabase">数据库索引</param>
        /// <param name="connection">redis连接对象</param>
        public RedisHelper(
            int defaultDatabase,
            IConnectionMultiplexer connection) =>
            Database = (_singleConnection = connection).GetDatabase(defaultDatabase);

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="action">自定义委托</param>
        /// <param name="log">redis连接日志</param>
        public RedisHelper(
            Action<IConnectionMultiplexer> action = null,
            TextWriter log = null) =>
            Database = GetConnection(action, log).GetDatabase();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="defaultDatabase">数据库索引</param>
        /// <param name="action">自定义委托</param>
        /// <param name="log">redis连接日志</param>
        public RedisHelper(
            int defaultDatabase,
            Action<IConnectionMultiplexer> action = null,
            TextWriter log = null) =>
            Database = GetConnection(action, log).GetDatabase(defaultDatabase);

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="redisConnectionString">redis连接字符串</param>
        /// <param name="action">自定义委托</param>
        /// <param name="log">redis连接日志</param>
        public RedisHelper(
            string redisConnectionString,
            Action<IConnectionMultiplexer> action = null,
            TextWriter log = null) =>
            Database = GetConnection(redisConnectionString, action, log).GetDatabase();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="poolSize">redis连接池大小</param>
        /// <param name="redisConnectionString">redis连接字符串</param>
        /// <param name="action">自定义委托</param>
        /// <param name="log">redis连接日志</param>
        public RedisHelper(
            int poolSize,
            string redisConnectionString,
            Action<IConnectionMultiplexer> action = null,
            TextWriter log = null) =>
            Database = (_poolConnection = GetConnection(poolSize, redisConnectionString, out _poolManager, action, log)).GetDatabase();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="redisConnectionString">redis连接字符串</param>
        /// <param name="defaultDatabase">数据库索引</param>
        /// <param name="action">自定义委托</param>
        /// <param name="log">redis连接日志</param>
        public RedisHelper(
            string redisConnectionString,
            int defaultDatabase,
            Action<IConnectionMultiplexer> action = null,
            TextWriter log = null) =>
            Database = GetConnection(redisConnectionString, action, log).GetDatabase(defaultDatabase);

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="poolSize">redis连接池大小</param>
        /// <param name="redisConnectionString">redis连接字符串</param>
        /// <param name="defaultDatabase">数据库索引</param>
        /// <param name="action">自定义委托</param>
        /// <param name="log">redis连接日志</param>
        public RedisHelper(
            int poolSize,
            string redisConnectionString,
            int defaultDatabase,
            Action<IConnectionMultiplexer> action = null,
            TextWriter log = null) =>
            Database = (_poolConnection = GetConnection(poolSize, redisConnectionString, out _poolManager, action, log)).GetDatabase(defaultDatabase);

        /// <summary>
        /// 构造函数
        /// </summary>        
        /// <param name="configurationOptions">连接配置</param>
        /// <param name="action">自定义委托</param>
        /// <param name="log">redis连接日志</param>
        public RedisHelper(
            ConfigurationOptions configurationOptions,
            Action<IConnectionMultiplexer> action = null,
            TextWriter log = null) =>
            Database = GetConnection(configurationOptions, action, log).GetDatabase();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="poolSize">redis连接池大小</param>        
        /// <param name="configurationOptions">连接配置</param>
        /// <param name="action">自定义委托</param>
        /// <param name="log">redis连接日志</param>
        public RedisHelper(
            int poolSize,
            ConfigurationOptions configurationOptions,
            Action<IConnectionMultiplexer> action = null,
            TextWriter log = null) =>
            Database = (_poolConnection = GetConnection(poolSize, configurationOptions, out _poolManager, action, log)).GetDatabase();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="poolSize">redis连接池大小</param>  
        /// <param name="defaultDatabase">数据库索引</param>
        /// <param name="configurationOptions">连接配置</param>
        /// <param name="action">自定义委托</param>
        /// <param name="log">redis连接日志</param>
        public RedisHelper(
            int poolSize,
            int defaultDatabase,
            ConfigurationOptions configurationOptions,
            Action<IConnectionMultiplexer> action = null,
            TextWriter log = null) =>
            Database = (_poolConnection = GetConnection(poolSize, configurationOptions, out _poolManager, action, log)).GetDatabase(defaultDatabase);
        #endregion 构造函数

        #region 连接对象
        #region GetConnection
        /// <summary>
        /// 获取redis连接对象
        /// </summary>
        /// <param name="action">自定义委托</param>
        /// <param name="log">redis连接日志</param>
        /// <returns>返回IConnectionMultiplexer</returns>
        public static IConnectionMultiplexer GetConnection(
            Action<IConnectionMultiplexer> action = null,
            TextWriter log = null)
        {
            var connectionStr = ConfigHelper.GetConnectionString("RedisConnectionStrings");

            if (connectionStr.IsNullOrEmpty())
                connectionStr = ConfigHelper.GetAppSettings<string>("RedisConnectionStrings");

            if (connectionStr.IsNullOrEmpty())
                throw new ArgumentNullException("Redis连接字符串配置为null");

            return GetConnection(connectionStr, action, log);
        }

        /// <summary>
        /// 获取redis连接对象
        /// </summary>
        /// <param name="redisConnectionString">redis连接字符串</param>
        /// <param name="action">自定义委托</param>
        /// <param name="log">redis连接日志</param>
        /// <returns>返回IConnectionMultiplexer</returns>
        public static IConnectionMultiplexer GetConnection(
            string redisConnectionString,
            Action<IConnectionMultiplexer> action = null,
            TextWriter log = null)
        {
            if (_singleConnection != null && _singleConnection.IsConnected)
                return _singleConnection;

            lock (_lock)
            {
                if (_singleConnection != null && _singleConnection.IsConnected)
                    return _singleConnection;

                _singleConnection = ConnectionMultiplexer.Connect(redisConnectionString, log);

                action?.Invoke(_singleConnection);

                RegisterEvents(_singleConnection);
            }

            return _singleConnection;
        }

        /// <summary>
        /// 获取redis连接对象
        /// </summary>
        /// <param name="configurationOptions">连接配置</param>
        /// <param name="action">自定义委托</param>
        /// <param name="log">redis连接日志</param>
        /// <returns>返回IConnectionMultiplexer</returns>
        public static IConnectionMultiplexer GetConnection(
            ConfigurationOptions configurationOptions,
            Action<IConnectionMultiplexer> action = null,
            TextWriter log = null)
        {
            if (_singleConnection != null && _singleConnection.IsConnected)
                return _singleConnection;

            lock (_lock)
            {
                if (_singleConnection != null && _singleConnection.IsConnected)
                    return _singleConnection;

                _singleConnection = ConnectionMultiplexer.Connect(configurationOptions, log);

                action?.Invoke(_singleConnection);

                RegisterEvents(_singleConnection);
            }

            return _singleConnection;
        }

        /// <summary>
        /// 获取redis连接对象
        /// </summary>
        /// <param name="poolSize">redis连接池大小</param>
        /// <param name="redisConnectionString">redis连接字符串</param>
        /// <param name="poolManager">redis连接池</param>
        /// <param name="action">自定义委托</param>
        /// <param name="log">redis连接日志</param>
        /// <returns>返回IConnectionMultiplexer</returns>
        public static IConnectionMultiplexer GetConnection(
            int poolSize,
            string redisConnectionString,
            out RedisConnectionPoolManager poolManager,
            Action<IConnectionMultiplexer> action = null,
            TextWriter log = null)
        {
            poolManager = RedisConnectionPoolManager.CreateInstance(poolSize, redisConnectionString, log);

            var connection = poolManager.GetConnection();

            action?.Invoke(connection);

            return connection;
        }

        /// <summary>
        /// 获取redis连接对象
        /// </summary>
        /// <param name="poolSize">redis连接池大小</param>
        /// <param name="configurationOptions">连接配置</param>
        /// <param name="poolManager">redis连接池</param>
        /// <param name="action">自定义委托</param>
        /// <param name="log">redis连接日志</param>
        /// <returns>返回IConnectionMultiplexer</returns>
        public static IConnectionMultiplexer GetConnection(
            int poolSize,
            ConfigurationOptions configurationOptions,
            out RedisConnectionPoolManager poolManager,
            Action<IConnectionMultiplexer> action = null,
            TextWriter log = null)
        {
            poolManager = RedisConnectionPoolManager.CreateInstance(poolSize, configurationOptions, log);

            var connection = poolManager.GetConnection();

            action?.Invoke(connection);

            return connection;
        }
        #endregion

        #region GetConnectionAsync
        /// <summary>
        /// 获取redis连接对象
        /// </summary>
        /// <param name="action">自定义委托</param>
        /// <param name="log">redis连接日志</param>
        /// <returns>返回IConnectionMultiplexer</returns>
        public static async Task<IConnectionMultiplexer> GetConnectionAsync(
            Action<IConnectionMultiplexer> action = null,
            TextWriter log = null)
        {
            var connectionStr = ConfigHelper.GetConnectionString("RedisConnectionStrings");

            if (connectionStr.IsNullOrEmpty())
                connectionStr = ConfigHelper.GetAppSettings<string>("RedisConnectionStrings");

            if (connectionStr.IsNullOrEmpty())
                throw new ArgumentNullException("Redis连接字符串配置为null");

            return await GetConnectionAsync(connectionStr, action, log);
        }

        /// <summary>
        /// 获取redis连接对象
        /// </summary>
        /// <param name="redisConnectionString">redis连接字符串</param>
        /// <param name="action">自定义委托</param>
        /// <param name="log">redis连接日志</param>
        /// <returns>返回IConnectionMultiplexer</returns>
        public static async Task<IConnectionMultiplexer> GetConnectionAsync(
            string redisConnectionString,
            Action<IConnectionMultiplexer> action = null,
            TextWriter log = null)
        {
            if (_singleConnection != null && _singleConnection.IsConnected)
                return _singleConnection;

            try
            {
                await _semaphoreSlim.WaitAsync().ConfigureAwait(false);

                if (_singleConnection != null && _singleConnection.IsConnected)
                    return _singleConnection;

                _singleConnection = await ConnectionMultiplexer.ConnectAsync(redisConnectionString, log);

                action?.Invoke(_singleConnection);

                RegisterEvents(_singleConnection);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            return _singleConnection;
        }

        /// <summary>
        /// 获取redis连接对象
        /// </summary>
        /// <param name="configurationOptions">连接配置</param>
        /// <param name="action">自定义委托</param>
        /// <param name="log">redis连接日志</param>
        /// <returns>返回IConnectionMultiplexer</returns>
        public static async Task<IConnectionMultiplexer> GetConnectionAsync(
            ConfigurationOptions configurationOptions,
            Action<IConnectionMultiplexer> action = null,
            TextWriter log = null)
        {
            if (_singleConnection != null && _singleConnection.IsConnected)
                return _singleConnection;

            try
            {
                await _semaphoreSlim.WaitAsync().ConfigureAwait(false);

                if (_singleConnection != null && _singleConnection.IsConnected)
                    return _singleConnection;

                _singleConnection = await ConnectionMultiplexer.ConnectAsync(configurationOptions, log);

                action?.Invoke(_singleConnection);

                RegisterEvents(_singleConnection);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            return _singleConnection;
        }
        #endregion

        #region SetConnectionAsync
        /// <summary>
        /// 设置IConnectionMultiplexer
        /// </summary>
        /// <param name="action">自定义委托</param>
        /// <returns></returns>
        public static async Task SetConnectionAsync(
            Action<IConnectionMultiplexer> action = null) =>
            _singleConnection = await GetConnectionAsync(action);

        /// <summary>
        /// 设置IConnectionMultiplexer
        /// </summary>
        /// <param name="redisConnectionString">连接字符串</param>
        /// <param name="action">自定义委托</param>
        /// <returns></returns>
        public static async Task SetConnectionAsync(
            string redisConnectionString,
            Action<IConnectionMultiplexer> action = null) =>
            _singleConnection = await GetConnectionAsync(redisConnectionString, action);

        /// <summary>
        /// 设置IConnectionMultiplexer
        /// </summary>
        /// <param name="configurationOptions">连接配置</param>
        /// <param name="action">自定义委托</param>
        /// <returns></returns>
        public static async Task SetConnectionAsync(
            ConfigurationOptions configurationOptions,
            Action<IConnectionMultiplexer> action = null) =>
            _singleConnection = await GetConnectionAsync(configurationOptions, action);
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
        /// 注册IConnectionMultiplexer事件
        /// </summary>
        private static void RegisterEvents(IConnectionMultiplexer connection)
        {
            if (ConfigHelper.GetAppSettings("Redis.RegisterEvent", true))
            {
                var hashCode = connection.GetHashCode();

                //物理连接恢复时
                connection.ConnectionRestored +=
                    (s, e) => LogHelper.Error(e.Exception, $"Redis(hash:{hashCode}) -> `ConnectionRestored`: {e.Exception?.Message}");

                //物理连接失败时
                connection.ConnectionFailed +=
                    (s, e) => LogHelper.Error(e.Exception, $"Redis(hash:{hashCode}) -> `ConnectionFailed`: {e.Exception?.Message}");

                //发生错误时
                connection.ErrorMessage +=
                    (s, e) => LogHelper.Error($"Redis(hash:{hashCode}) -> `ErrorMessage`: {e.Message}");

                //配置更改时
                connection.ConfigurationChanged +=
                    (s, e) => LogHelper.Info($"Redis(hash:{hashCode}) -> `ConfigurationChanged`: {e.EndPoint}");

                //更改集群时
                connection.HashSlotMoved +=
                    (s, e) => LogHelper.Info($"Redis(hash:{hashCode}) -> `HashSlotMoved`: {nameof(e.OldEndPoint)}-{e.OldEndPoint} To {nameof(e.NewEndPoint)}-{e.NewEndPoint}, ");

                //发生内部错误时（主要用于调试）
                connection.InternalError +=
                    (s, e) => LogHelper.Error(e.Exception, $"Redis(hash:{hashCode}) -> `InternalError`: {e.Exception?.Message}");

                //重新配置广播时（通常意味着主从同步更改）
                connection.ConfigurationChangedBroadcast +=
                    (s, e) => LogHelper.Info($"Redis(hash:{hashCode}) -> `ConfigurationChangedBroadcast`: {e.EndPoint}");
            }
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
            return KeyPrefix.IsNullOrEmpty() ? key : $"{KeyPrefix}:{key}";
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
            Database = RedisConnection.GetDatabase(database);

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
        /// <param name="key">字符串key</param>
        /// <param name="redisValue">字符串value</param>
        /// <param name="expiry">过期时间</param>
        /// <returns>返回是否保存成功</returns>
        public bool StringSet(string key, string redisValue, TimeSpan? expiry = null)
        {
            key = AddKeyPrefix(key);
            return Database.StringSet(key, redisValue, expiry);
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
        /// <param name="key">字符串key</param>
        /// <param name="redisValue">字符串value</param>
        /// <param name="expiry">过期时间</param>
        /// <returns>返回是否保存成功</returns>
        public bool StringSet<T>(string key, T redisValue, TimeSpan? expiry = null)
        {
            if (typeof(T) == typeof(string))
                return StringSet(key, redisValue.ToOrDefault<string>(), expiry);

            return StringSet(key, redisValue.ToJson(), expiry);
        }
        #endregion

        #region StringGet
        /// <summary>
        /// 获取字符串值
        /// </summary>
        /// <param name="key">字符串key</param>
        /// <returns>返回字符串值</returns>
        public string StringGet(string key)
        {
            key = AddKeyPrefix(key);
            return Database.StringGet(key);
        }

        /// <summary>
        /// 获取序反列化后的对象
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">字符串key</param>
        /// <returns>返回反序列化后的对象</returns>
        public T StringGet<T>(string key)
        {
            return StringGet(key).ToObject<T>();
        }
        #endregion
        #endregion

        #region 异步方法
        #region StringSetAsync
        /// <summary>
        /// 保存字符串（若key已存在，则覆盖）
        /// </summary>
        /// <param name="key">字符串key</param>
        /// <param name="redisValue">字符串value</param>
        /// <param name="expiry">过期时间</param>
        /// <returns>返回是否保存成功</returns>
        public async Task<bool> StringSetAsync(string key, string redisValue, TimeSpan? expiry = null)
        {
            key = AddKeyPrefix(key);
            return await Database.StringSetAsync(key, redisValue, expiry);
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
        /// <param name="key">字符串key</param>
        /// <param name="redisValue">字符串value</param>
        /// <param name="expiry">过期时间</param>
        /// <returns>返回是否保存成功</returns>
        public async Task<bool> StringSetAsync<T>(string key, T redisValue, TimeSpan? expiry = null)
        {
            if (typeof(T) == typeof(string))
                return await StringSetAsync(key, redisValue.ToOrDefault<string>(), expiry);

            return await StringSetAsync(key, redisValue.ToJson(), expiry);
        }
        #endregion

        #region StringGetAsync
        /// <summary>
        /// 获取字符串值
        /// </summary>
        /// <param name="key">字符串key</param>
        /// <returns>返回字符串值</returns>
        public async Task<string> StringGetAsync(string key)
        {
            key = AddKeyPrefix(key);
            return await Database.StringGetAsync(key);
        }

        /// <summary>
        /// 获取序反列化后的对象
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">字符串key</param>
        /// <returns>返回反序列化后的对象</returns>
        public async Task<T> StringGetAsync<T>(string key)
        {
            return (await StringGetAsync(key)).ToObject<T>();
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
        /// <param name="key">redis存储key</param>
        /// <param name="hashField">hash字段key</param>
        /// <param name="fieldValue">hash字段value</param>
        /// <returns>返回是否保存成功</returns>
        public bool HashSet(string key, string hashField, string fieldValue)
        {
            key = AddKeyPrefix(key);
            return Database.HashSet(key, hashField, fieldValue);
        }

        /// <summary>
        /// 保存hash字段
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="hashFields">hash字段key-value集合</param>
        public void HashSet(string key, IEnumerable<KeyValuePair<string, string>> hashFields)
        {
            key = AddKeyPrefix(key);
            var entries = hashFields.Select(x => new HashEntry(x.Key, x.Value));
            Database.HashSet(key, entries.ToArray());
        }

        /// <summary>
        /// 保存对象到hash中
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">redis存储key</param>
        /// <param name="hashField">hash字段key</param>
        /// <param name="fieldValue">hash字段value</param>
        /// <returns>返回是否保存成功</returns>
        public bool HashSet<T>(string key, string hashField, T fieldValue)
        {
            if (typeof(T) == typeof(string))
                return HashSet(key, hashField, fieldValue.ToOrDefault<string>());

            return HashSet(key, hashField, fieldValue.ToJson());
        }
        #endregion

        #region HashGet
        /// <summary>
        /// 获取hash字段值
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="hashField">hash字段key</param>
        /// <returns>返回hash字段值</returns>
        public string HashGet(string key, string hashField)
        {
            key = AddKeyPrefix(key);
            return Database.HashGet(key, hashField);
        }

        /// <summary>
        /// 获取hash中反序列化后的对象
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">redis存储key</param>
        /// <param name="hashField">hash字段key</param>
        /// <returns>返回反序列化后的对象</returns>
        public T HashGet<T>(string key, string hashField)
        {
            return HashGet(key, hashField).ToObject<T>();
        }

        /// <summary>
        /// 获取hash字段值反序列化后的对象集合
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">redis存储key</param>
        /// <returns>返回反序列化后的对象集合</returns>
        public IEnumerable<T> HashGet<T>(string key)
        {
            var hashFields = HashKeys(key);
            return HashGet<T>(key, hashFields);
        }

        /// <summary>
        /// 获取hash字段值反序列化后的对象集合
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">redis存储key</param>
        /// <param name="hashFields">hash字段key集合</param>
        /// <returns>返回反序列化后的对象集合</returns>
        public IEnumerable<T> HashGet<T>(string key, IEnumerable<string> hashFields)
        {
            key = AddKeyPrefix(key);
            var fields = hashFields.Select(x => (RedisValue)x);
            return Database.HashGet(key, fields.ToArray()).Select(o => o.ToString().ToObject<T>());
        }
        #endregion

        #region HashDelete
        /// <summary>
        /// 从hash中移除指定字段
        /// </summary>
        /// <param name="key">redis存储key</param>        
        /// <returns>返回是否删除成功</returns>
        public long HashDelete(string key)
        {
            var hashFields = HashKeys(key);
            return HashDelete(key, hashFields);
        }

        /// <summary>
        /// 从hash中移除指定字段
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="hashField">hash字段key</param>
        /// <returns>返回是否删除成功</returns>
        public bool HashDelete(string key, string hashField)
        {
            key = AddKeyPrefix(key);
            return Database.HashDelete(key, hashField);
        }

        /// <summary>
        /// 从hash中移除指定字段
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="hashFields">hash字段key集合</param>
        /// <returns>返回是否删除成功</returns>
        public long HashDelete(string key, IEnumerable<string> hashFields)
        {
            key = AddKeyPrefix(key);
            var fields = hashFields?.Select(x => (RedisValue)x);
            if (fields.IsNotNullOrEmpty())
                return Database.HashDelete(key, fields.ToArray());

            return 0;
        }
        #endregion

        #region HashExists
        /// <summary>
        /// 判断键值是否在hash中
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="hashField">hash字段key</param>
        /// <returns>返回是否存在</returns>
        public bool HashExists(string key, string hashField)
        {
            key = AddKeyPrefix(key);
            return Database.HashExists(key, hashField);
        }
        #endregion

        #region HashKeys
        /// <summary>
        /// 获取hash中指定key的所有字段key
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <returns>返回hash字段key集合</returns>
        public IEnumerable<string> HashKeys(string key)
        {
            key = AddKeyPrefix(key);
            return Database.HashKeys(key).Select(o => o.ToString());
        }
        #endregion

        #region HashValues
        /// <summary>
        /// 获取hash中指定key的所有字段value
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <returns>返回hash字段value集合</returns>
        public IEnumerable<string> HashValues(string key)
        {
            key = AddKeyPrefix(key);
            return Database.HashValues(key).Select(o => o.ToString());
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
        /// <param name="key">redis存储key</param>
        /// <param name="hashField">hash字段key</param>
        /// <param name="fieldValue">hash字段value</param>
        /// <returns>返回是否保存成功</returns>
        public async Task<bool> HashSetAsync(string key, string hashField, string fieldValue)
        {
            key = AddKeyPrefix(key);
            return await Database.HashSetAsync(key, hashField, fieldValue);
        }

        /// <summary>
        /// 保存hash字段
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="hashFields">hash字段key-value集合</param>
        public async Task HashSetAsync(string key, IEnumerable<KeyValuePair<string, string>> hashFields)
        {
            key = AddKeyPrefix(key);
            var entries = hashFields.Select(x => new HashEntry(AddKeyPrefix(x.Key), x.Value));
            await Database.HashSetAsync(key, entries.ToArray());
        }

        /// <summary>
        /// 保存对象到hash中
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="hashField">hash字段key</param>
        /// <param name="fieldValue">hash字段value</param>
        /// <returns>返回是否保存成功</returns>
        public async Task<bool> HashSetAsync<T>(string key, string hashField, T fieldValue)
        {
            if (typeof(T) == typeof(string))
                return await HashSetAsync(key, hashField, fieldValue.ToOrDefault<string>());

            return await HashSetAsync(key, hashField, fieldValue.ToJson());
        }
        #endregion

        #region HashGetAsync
        /// <summary>
        /// 获取hash字段值
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="hashField">hash字段key</param>
        /// <returns>返回hash字段值</returns>
        public async Task<string> HashGetAsync(string key, string hashField)
        {
            key = AddKeyPrefix(key);
            return await Database.HashGetAsync(key, hashField);
        }

        /// <summary>
        /// 获取hash中反序列化后的对象
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">redis存储key</param>
        /// <param name="hashField">hash字段key</param>
        /// <returns>返回反序列化后的对象</returns>
        public async Task<T> HashGetAsync<T>(string key, string hashField)
        {
            return (await HashGetAsync(key, hashField)).ToObject<T>();
        }

        /// <summary>
        /// 获取hash字段值反序列化后的对象集合
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">redis存储key</param>
        /// <returns>返回反序列化后的对象集合</returns>
        public async Task<IEnumerable<T>> HashGetAsync<T>(string key)
        {
            var hashFields = await HashKeysAsync(key);
            return await HashGetAsync<T>(key, hashFields);
        }

        /// <summary>
        /// 获取hash字段值反序列化后的对象集合
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">redis存储key</param>
        /// <param name="hashFields">hash字段key集合</param>
        /// <returns>返回反序列化后的对象集合</returns>
        public async Task<IEnumerable<T>> HashGetAsync<T>(string key, IEnumerable<string> hashFields)
        {
            key = AddKeyPrefix(key);
            var fields = hashFields.Select(x => (RedisValue)x);
            var result = (await Database.HashGetAsync(key, fields.ToArray())).Select(o => o.ToString());
            return result.Select(o => o.ToString().ToObject<T>());
        }
        #endregion

        #region HashDeleteAsync
        /// <summary>
        /// 从hash中移除指定字段
        /// </summary>
        /// <param name="key">redis存储key</param>        
        /// <returns>返回是否删除成功</returns>
        public async Task<long> HashDeleteAsync(string key)
        {
            var hashFields = await HashKeysAsync(key);
            return await HashDeleteAsync(key, hashFields);
        }

        /// <summary>
        /// 从hash中移除指定字段
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="hashField">hash字段key</param>
        /// <returns>返回是否删除成功</returns>
        public async Task<bool> HashDeleteAsync(string key, string hashField)
        {
            key = AddKeyPrefix(key);
            return await Database.HashDeleteAsync(key, hashField);
        }

        /// <summary>
        /// 从hash中移除指定字段
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="hashFields">hash字段key集合</param>
        /// <returns>返回是否删除成功</returns>
        public async Task<long> HashDeleteAsync(string key, IEnumerable<string> hashFields)
        {
            key = AddKeyPrefix(key);
            var fields = hashFields?.Select(x => (RedisValue)x);
            if (fields.IsNotNullOrEmpty())
                return await Database.HashDeleteAsync(key, fields.ToArray());

            return 0;
        }
        #endregion

        #region HashExistsAsync
        /// <summary>
        /// 判断键值是否在hash中
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="hashField">hash字段key</param>
        /// <returns>返回是否存在</returns>
        public async Task<bool> HashExistsAsync(string key, string hashField)
        {
            key = AddKeyPrefix(key);
            return await Database.HashExistsAsync(key, hashField);
        }
        #endregion

        #region HashKeysAsync
        /// <summary>
        /// 获取hash中指定key的所有字段key
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <returns>返回hash字段key集合</returns>
        public async Task<IEnumerable<string>> HashKeysAsync(string key)
        {
            key = AddKeyPrefix(key);
            return (await Database.HashKeysAsync(key)).Select(o => o.ToString());
        }
        #endregion

        #region HashValuesAsync
        /// <summary>
        /// 获取hash中指定key的所有字段value
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <returns>返回hash字段value集合</returns>
        public async Task<IEnumerable<string>> HashValuesAsync(string key)
        {
            key = AddKeyPrefix(key);
            return (await Database.HashValuesAsync(key)).Select(o => o.ToString());
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
        /// <param name="key">redis存储key</param>
        /// <param name="redisValue">redis存储value</param>
        /// <returns>返回插入后列表的长度</returns>
        public long ListLeftPush(string key, string redisValue)
        {
            key = AddKeyPrefix(key);
            return Database.ListLeftPush(key, redisValue);
        }

        /// <summary>
        /// 在列表头部插入值，如果键不存在，先创建再插入值
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">redis存储key</param>
        /// <param name="redisValue">redis存储value</param>
        /// <returns>返回插入后列表的长度</returns>
        public long ListLeftPush<T>(string key, T redisValue)
        {
            if (typeof(T) == typeof(string))
                return ListLeftPush(key, redisValue.ToOrDefault<string>());

            return ListLeftPush(key, redisValue.ToJson());
        }

        /// <summary>
        /// 移除并返回存储在该键列表的第一个元素
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <returns>返回移除元素的值</returns>
        public string ListLeftPop(string key)
        {
            key = AddKeyPrefix(key);
            return Database.ListLeftPop(key);
        }

        /// <summary>
        /// 移除并返回存储在该键列表的第一个元素
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">redis存储key</param>
        /// <returns>返回移除元素的值</returns>
        public T ListLeftPop<T>(string key)
        {
            return ListLeftPop(key).ToObject<T>();
        }
        #endregion

        #region ListRight
        /// <summary>
        /// 在列表尾部插入值，如果键不存在，先创建再插入值
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="redisValue">redis存储value</param>
        /// <returns>返回插入后列表的长度</returns>
        public long ListRightPush(string key, string redisValue)
        {
            key = AddKeyPrefix(key);
            return Database.ListRightPush(key, redisValue);
        }

        /// <summary>
        /// 在列表尾部插入值，如果键不存在，先创建再插入值
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">redis存储key</param>
        /// <param name="redisValue">redis存储value</param>
        /// <returns>返回插入后列表的长度</returns>
        public long ListRightPush<T>(string key, T redisValue)
        {
            if (typeof(T) == typeof(string))
                return ListRightPush(key, redisValue.ToOrDefault<string>());

            return ListRightPush(key, redisValue.ToJson());
        }

        /// <summary>
        /// 移除并返回存储在该键列表的最后一个元素
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <returns>返回移除元素的值</returns>
        public string ListRightPop(string key)
        {
            key = AddKeyPrefix(key);
            return Database.ListRightPop(key);
        }

        /// <summary>
        /// 移除并返回存储在该键列表的最后一个元素
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">redis存储key</param>
        /// <returns>返回移除元素的值</returns>
        public T ListRightPop<T>(string key)
        {
            return ListRightPop(key).ToObject<T>();
        }
        #endregion

        #region ListRemove
        /// <summary>
        /// 移除列表指定键上与该值相同的元素
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="redisValue">redis存储value</param>
        /// <returns>返回移除元素的数量</returns>
        public long ListRemove(string key, string redisValue)
        {
            key = AddKeyPrefix(key);
            return Database.ListRemove(key, redisValue);
        }
        #endregion

        #region ListLength
        /// <summary>
        /// 返回列表上该键的长度，如果不存在，返回 0
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <returns>返回列表的长度</returns>
        public long ListLength(string key)
        {
            key = AddKeyPrefix(key);
            return Database.ListLength(key);
        }
        #endregion

        #region ListRange
        /// <summary>
        /// 返回在该列表上键所对应的元素
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="start">开始索引位置</param>
        /// <param name="stop">结束索引位置</param>
        /// <returns>返回指定范围内的元素集合</returns>
        public IEnumerable<string> ListRange(string key, long start = 0, long stop = -1)
        {
            key = AddKeyPrefix(key);
            return Database.ListRange(key, start, stop).Select(o => o.ToString());
        }
        #endregion

        #region Queue
        /// <summary>
        /// 队列入队
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="redisValue">redis存储value</param>
        /// <returns>返回入队后队列的长度</returns>
        public long EnqueueItemOnList(string key, string redisValue)
        {
            return ListRightPush(key, redisValue);
        }

        /// <summary>
        /// 队列入队
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">redis存储key</param>
        /// <param name="redisValue">redis存储value</param>
        /// <returns>返回入队后队列的长度</returns>
        public long EnqueueItemOnList<T>(string key, T redisValue)
        {
            return ListRightPush(key, redisValue);
        }

        /// <summary>
        /// 队列出队
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <returns>返回出队元素的值</returns>
        public string DequeueItemFromList(string key)
        {
            return ListLeftPop(key);
        }

        /// <summary>
        /// 队列出队
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">redis存储key</param>
        /// <returns>返回出队元素的值</returns>
        public T DequeueItemFromList<T>(string key)
        {
            return ListLeftPop<T>(key);
        }

        /// <summary>
        /// 获取队列长度
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <returns>返回队列的长度</returns>
        public long GetQueueLength(string key)
        {
            return ListLength(key);
        }
        #endregion
        #endregion

        #region 异步方法
        #region ListLeftAsync
        /// <summary>
        /// 在列表头部插入值。如果键不存在，先创建再插入值
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="redisValue">redis存储value</param>
        /// <returns>返回插入后列表的长度</returns>
        public async Task<long> ListLeftPushAsync(string key, string redisValue)
        {
            key = AddKeyPrefix(key);
            return await Database.ListLeftPushAsync(key, redisValue);
        }

        /// <summary>
        /// 在列表头部插入值。如果键不存在，先创建再插入值
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">redis存储key</param>
        /// <param name="redisValue">redis存储value</param>
        /// <returns>返回插入后列表的长度</returns>
        public async Task<long> ListLeftPushAsync<T>(string key, T redisValue)
        {
            if (typeof(T) == typeof(string))
                return await ListLeftPushAsync(key, redisValue.ToOrDefault<string>());

            return await ListLeftPushAsync(key, redisValue.ToJson());
        }

        /// <summary>
        /// 移除并返回存储在该键列表的第一个元素
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <returns>返回移除元素的值</returns>
        public async Task<string> ListLeftPopAsync(string key)
        {
            key = AddKeyPrefix(key);
            return await Database.ListLeftPopAsync(key);
        }

        /// <summary>
        /// 移除并返回存储在该键列表的第一个元素
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">redis存储key</param>
        /// <returns>返回移除元素的值</returns>
        public async Task<T> ListLeftPopAsync<T>(string key)
        {
            return (await ListLeftPopAsync(key)).ToObject<T>();
        }
        #endregion

        #region ListRightAsync
        /// <summary>
        /// 在列表尾部插入值。如果键不存在，先创建再插入值
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="redisValue">redis存储value</param>
        /// <returns>返回插入后列表的长度</returns>
        public async Task<long> ListRightPushAsync(string key, string redisValue)
        {
            key = AddKeyPrefix(key);
            return await Database.ListRightPushAsync(key, redisValue);
        }

        /// <summary>
        /// 在列表尾部插入值。如果键不存在，先创建再插入值
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">redis存储key</param>
        /// <param name="redisValue">redis存储value</param>
        /// <returns>返回插入后列表的长度</returns>
        public async Task<long> ListRightPushAsync<T>(string key, T redisValue)
        {
            if (typeof(T) == typeof(string))
                return await ListRightPushAsync(key, redisValue.ToOrDefault<string>());

            return await ListRightPushAsync(key, redisValue.ToJson());
        }

        /// <summary>
        /// 移除并返回存储在该键列表的最后一个元素
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <returns>返回移除元素的值</returns>
        public async Task<string> ListRightPopAsync(string key)
        {
            key = AddKeyPrefix(key);
            return await Database.ListRightPopAsync(key);
        }

        /// <summary>
        /// 移除并返回存储在该键列表的最后一个元素
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">redis存储key</param>
        /// <returns>返回移除元素的值</returns>
        public async Task<T> ListRightPopAsync<T>(string key)
        {
            return (await ListRightPopAsync(key)).ToObject<T>();
        }
        #endregion

        #region ListRemoveAsync
        /// <summary>
        /// 移除列表指定键上与该值相同的元素
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="redisValue">redis存储value</param>
        /// <returns>返回移除元素的数量</returns>
        public async Task<long> ListRemoveAsync(string key, string redisValue)
        {
            key = AddKeyPrefix(key);
            return await Database.ListRemoveAsync(key, redisValue);
        }
        #endregion

        #region ListLengthAsync
        /// <summary>
        /// 返回列表上该键的长度，如果不存在，返回 0
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <returns>返回列表的长度</returns>
        public async Task<long> ListLengthAsync(string key)
        {
            key = AddKeyPrefix(key);
            return await Database.ListLengthAsync(key);
        }
        #endregion

        #region ListRangeAsync
        /// <summary>
        /// 返回在该列表上键所对应的元素
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="start">开始索引位置</param>
        /// <param name="stop">结束索引位置</param>
        /// <returns>返回指定范围内的元素集合</returns>
        public async Task<IEnumerable<string>> ListRangeAsync(string key, long start = 0, long stop = -1)
        {
            key = AddKeyPrefix(key);
            var query = await Database.ListRangeAsync(key, start, stop);
            return query.Select(x => x.ToString());
        }
        #endregion

        #region QueueAsync
        /// <summary>
        /// 队列入队
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="redisValue">redis存储value</param>
        /// <returns>返回入队后队列的长度</returns>
        public async Task<long> EnqueueItemOnListAsync(string key, string redisValue)
        {
            return await ListRightPushAsync(key, redisValue);
        }

        /// <summary>
        /// 队列入队
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">redis存储key</param>
        /// <param name="redisValue">redis存储value</param>
        /// <returns>返回入队后队列的长度</returns>
        public async Task<long> EnqueueItemOnListAsync<T>(string key, T redisValue)
        {
            return await ListRightPushAsync(key, redisValue);
        }

        /// <summary>
        /// 队列出队
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <returns>返回出队元素的值</returns>
        public async Task<string> DequeueItemFromListAsync(string key)
        {
            return await ListLeftPopAsync(key);
        }

        /// <summary>
        /// 队列出队
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">redis存储key</param>
        /// <returns>返回出队元素的值</returns>
        public async Task<T> DequeueItemFromListAsync<T>(string key)
        {
            return await ListLeftPopAsync<T>(key);
        }

        /// <summary>
        /// 获取队列长度
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <returns>返回队列的长度</returns>
        public async Task<long> GetQueueLengthAsync(string key)
        {
            return await ListLengthAsync(key);
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
        /// <param name="key">redis存储key</param>
        /// <param name="member">redis存储value</param>
        /// <param name="score">score</param>
        /// <returns>若值已存在则返回false且score被更新，否则添加成功返回true</returns>
        public bool SortedSetAdd(string key, string member, double score)
        {
            key = AddKeyPrefix(key);
            return Database.SortedSetAdd(key, member, score);
        }

        /// <summary>
        /// 新增元素到有序集合
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">redis存储key</param>
        /// <param name="member">redis存储value</param>
        /// <param name="score">score</param>
        /// <returns>若值已存在则返回false且score被更新，否则添加成功返回true</returns>
        public bool SortedSetAdd<T>(string key, T member, double score)
        {
            if (typeof(T) == typeof(string))
                return SortedSetAdd(key, member.ToOrDefault<string>(), score);

            return SortedSetAdd(key, member.ToJson(), score);
        }
        #endregion

        #region SortedSetRemove
        /// <summary>
        /// 移除有序集合中指定key-value的元素
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="member">redis存储value</param>
        /// <returns>返回是否移除成功</returns>
        public bool SortedSetRemove(string key, string member)
        {
            key = AddKeyPrefix(key);
            return Database.SortedSetRemove(key, member);
        }

        /// <summary>
        /// 移除有序集合中指定key-value的元素
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">redis存储key</param>
        /// <param name="member">redis存储value</param>
        /// <returns>返回是否移除成功</returns>
        public bool SortedSetRemove<T>(string key, T member)
        {
            if (typeof(T) == typeof(string))
                return SortedSetRemove(key, member.ToOrDefault<string>());

            return SortedSetRemove(key, member.ToJson());
        }

        /// <summary>
        /// 根据起始索引位置移除有序集合中的指定范围的元素
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="start">开始索引位置</param>
        /// <param name="stop">结束索引位置</param>
        /// <returns>返回移除元素数量</returns>
        public long SortedSetRemoveRangeByRank(string key, long start, long stop)
        {
            key = AddKeyPrefix(key);
            return Database.SortedSetRemoveRangeByRank(key, start, stop);
        }

        /// <summary>
        /// 根据score起始值移除有序集合中的指定score范围的元素
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="start">开始score</param>
        /// <param name="stop">结束score</param>
        /// <returns>返回移除元素数量</returns>
        public long SortedSetRemoveRangeByScore(string key, double start, double stop)
        {
            key = AddKeyPrefix(key);
            return Database.SortedSetRemoveRangeByScore(key, start, stop);
        }

        /// <summary>
        /// 根据value最大和最小值移除有序集合中的指定value范围的元素
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="min">value最小值</param>
        /// <param name="max">value最大值</param>
        /// <returns>返回移除元素数量</returns>
        public long SortedSetRemoveRangeByValue(string key, string min, string max)
        {
            key = AddKeyPrefix(key);
            return Database.SortedSetRemoveRangeByValue(key, min, max);
        }
        #endregion

        #region SortedSetIncrement
        /// <summary>
        /// 按增量增加按键存储的有序集合中成员的score
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="member">redis存储value</param>
        /// <param name="value">增量值</param>
        /// <returns>返回新的score</returns>
        public double SortedSetIncrement(string key, string member, double value = 1)
        {
            key = AddKeyPrefix(key);
            return Database.SortedSetIncrement(key, member, value);
        }

        /// <summary>
        /// 按增量增加按键存储的有序集合中成员的score
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">redis存储key</param>
        /// <param name="member">redis存储value</param>
        /// <param name="value">增量值</param>
        /// <returns>返回新的score</returns>
        public double SortedSetIncrement<T>(string key, T member, double value = 1)
        {
            key = AddKeyPrefix(key);

            if (typeof(T) == typeof(string))
                return Database.SortedSetIncrement(key, member.ToOrDefault<string>(), value);

            return Database.SortedSetIncrement(key, member.ToJson(), value);
        }
        #endregion

        #region SortedSetDecrement
        /// <summary>
        /// 通过递减递减存储在键处的有序集中的成员的score
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="member">redis存储value</param>
        /// <param name="value">递减值</param>
        /// <returns>返回新的score</returns>
        public double SortedSetDecrement(string key, string member, double value)
        {
            key = AddKeyPrefix(key);
            return Database.SortedSetDecrement(key, member, value);
        }

        /// <summary>
        /// 通过递减递减存储在键处的有序集中的成员的score
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">redis存储key</param>
        /// <param name="member">redis存储value</param>
        /// <param name="value">递减值</param>
        /// <returns>返回新的score</returns>
        public double SortedSetDecrement<T>(string key, T member, double value)
        {
            key = AddKeyPrefix(key);

            if (typeof(T) == typeof(string))
                return Database.SortedSetDecrement(key, member.ToOrDefault<string>(), value);

            return Database.SortedSetDecrement(key, member.ToJson(), value);
        }
        #endregion

        #region SortedSetLength
        /// <summary>
        /// 获取有序集合的长度
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <returns>返回有序集合的长度</returns>
        public long SortedSetLength(string key)
        {
            key = AddKeyPrefix(key);
            return Database.SortedSetLength(key);
        }
        #endregion

        #region SortedSetRank
        /// <summary>
        /// 获取集合中的索引位置，从0开始
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="member">redis存储value</param>
        /// <param name="order">排序方式</param>
        /// <returns>返回索引位置</returns>
        public long? SortedSetRank(string key, string member, Order order = Order.Ascending)
        {
            key = AddKeyPrefix(key);
            return Database.SortedSetRank(key, member, order);
        }

        /// <summary>
        /// 获取集合中的索引位置，从0开始
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">redis存储key</param>
        /// <param name="member">redis存储value</param>
        /// <param name="order">排序方式</param>
        /// <returns>返回索引位置</returns>
        public long? SortedSetRank<T>(string key, T member, Order order = Order.Ascending)
        {
            key = AddKeyPrefix(key);

            if (typeof(T) == typeof(string))
                return Database.SortedSetRank(key, member.ToOrDefault<string>(), order);

            return Database.SortedSetRank(key, member.ToJson(), order);
        }
        #endregion

        #region SortedSetScore
        /// <summary>
        /// 获取有序集合中指定元素的score
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="memebr">redis存储value</param>
        /// <returns>返回指定元素的score</returns>
        public double? SortedSetScore(string key, string memebr)
        {
            key = AddKeyPrefix(key);
            return Database.SortedSetScore(key, memebr);
        }

        /// <summary>
        /// 获取有序集合中指定元素的score
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">redis存储key</param>
        /// <param name="memebr">redis存储value</param>
        /// <returns>返回指定元素的score</returns>
        public double? SortedSetScore<T>(string key, T memebr)
        {
            key = AddKeyPrefix(key);

            if (typeof(T) == typeof(string))
                return Database.SortedSetScore(key, memebr.ToOrDefault<string>());

            return Database.SortedSetScore(key, memebr.ToJson());
        }
        #endregion

        #region SortedSetRange
        /// <summary>
        /// 获取有序集合中指定索引范围的元素value，默认情况下从低到高
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="start">开始索引</param>
        /// <param name="stop">结束索引</param>
        /// <param name="order">排序方式</param>        
        /// <returns>返回指定范围的元素value</returns>
        public IEnumerable<string> SortedSetRangeByRank(string key, long start = 0, long stop = -1, Order order = Order.Ascending)
        {
            key = AddKeyPrefix(key);
            return Database.SortedSetRangeByRank(key, start, stop, order).Select(x => x.ToString());
        }

        /// <summary>
        /// 获取有序集合中指定索引范围的元素value，默认情况下从低到高
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">redis存储key</param>
        /// <param name="start">开始索引</param>
        /// <param name="stop">结束索引</param>
        /// <param name="order">排序方式</param>        
        /// <returns>返回指定范围的元素value</returns>
        public IEnumerable<T> SortedSetRangeByRank<T>(string key, long start = 0, long stop = -1, Order order = Order.Ascending)
        {
            return SortedSetRangeByRank(key, start, stop, order).Select(o => o.ToObject<T>());
        }

        /// <summary>
        /// 获取有序集合中指定索引范围的元素value-score，默认情况下从低到高
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="start">开始索引</param>
        /// <param name="stop">结束索引</param>
        /// <param name="order">排序方式</param>        
        /// <returns>返回指定范围的元素value-score</returns>
        public Dictionary<string, double> SortedSetRangeByRankWithScores(string key, long start = 0, long stop = -1, Order order = Order.Ascending)
        {
            key = AddKeyPrefix(key);
            var result = Database.SortedSetRangeByRankWithScores(key, start, stop, order);
            return result.Select(x => new { x.Score, Value = x.Element.ToString() }).ToDictionary(x => x.Value, x => x.Score);
        }

        /// <summary>
        /// 获取有序集合中指定索引范围的元素value-score，默认情况下从低到高
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">redis存储key</param>
        /// <param name="start">开始索引</param>
        /// <param name="stop">结束索引</param>
        /// <param name="order">排序方式</param>        
        /// <returns>返回指定范围的元素value-score</returns>
        public Dictionary<T, double> SortedSetRangeByRankWithScores<T>(string key, long start = 0, long stop = -1, Order order = Order.Ascending)
        {
            return SortedSetRangeByRankWithScores(key, start, stop, order).ToDictionary(o => o.Key.ToObject<T>(), o => o.Value);
        }

        /// <summary>
        /// 获取有序集合中指定score起始范围的元素value，默认情况下从低到高
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="start">开始score</param>
        /// <param name="stop">结束score</param>
        /// <param name="skip">跳过元素数量</param>
        /// <param name="take">拿取元素数量</param>
        /// <param name="order">排序方式</param>
        /// <returns>返回指定范围的元素value</returns>
        public IEnumerable<string> SortedSetRangeByScore(string key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity, long skip = 0, long take = -1, Order order = Order.Ascending)
        {
            key = AddKeyPrefix(key);
            return Database.SortedSetRangeByScore(key, start, stop, order: order, skip: skip, take: take).Select(o => o.ToString());
        }

        /// <summary>
        /// 获取有序集合中指定score起始范围的元素value，默认情况下从低到高
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">redis存储key</param>
        /// <param name="start">开始score</param>
        /// <param name="stop">结束score</param>
        /// <param name="skip">跳过元素数量</param>
        /// <param name="take">拿取元素数量</param>
        /// <param name="order">排序方式</param>
        /// <returns>返回指定范围的元素value</returns>
        public IEnumerable<T> SortedSetRangeByScore<T>(string key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity, long skip = 0, long take = -1, Order order = Order.Ascending)
        {
            return SortedSetRangeByScore(key, start, stop, skip, take, order).Select(o => o.ToObject<T>());
        }

        /// <summary>
        /// 获取有序集合中指定score起始范围的元素value-score，默认情况下从低到高
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="start">开始score</param>
        /// <param name="stop">结束score</param>
        /// <param name="skip">跳过元素数量</param>
        /// <param name="take">拿取元素数量</param>
        /// <param name="order">排序方式</param>
        /// <returns>返回指定范围的元素value-score</returns>
        public Dictionary<string, double> SortedSetRangeByScoreWithScores(string key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity, long skip = 0, long take = -1, Order order = Order.Ascending)
        {
            key = AddKeyPrefix(key);
            var result = Database.SortedSetRangeByScoreWithScores(key, start, stop, order: order, skip: skip, take: take);
            return result.Select(x => new { x.Score, Value = x.Element.ToString() }).ToDictionary(x => x.Value, x => x.Score);
        }

        /// <summary>
        /// 获取有序集合中指定score起始范围的元素value-score，默认情况下从低到高
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">redis存储key</param>
        /// <param name="start">开始score</param>
        /// <param name="stop">结束score</param>
        /// <param name="skip">跳过元素数量</param>
        /// <param name="take">拿取元素数量</param>
        /// <param name="order">排序方式</param>
        /// <returns>返回指定范围的元素value-score</returns>
        public Dictionary<T, double> SortedSetRangeByScoreWithScores<T>(string key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity, long skip = 0, long take = -1, Order order = Order.Ascending)
        {
            return SortedSetRangeByScoreWithScores(key, start, stop, skip, take, order).ToDictionary(o => o.Key.ToObject<T>(), o => o.Value);
        }

        /// <summary>
        /// 获取有序集合中指定value最大最小范围的元素value，当有序集合中的score相同时，按照value从小到大进行排序
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="min">value最小值</param>
        /// <param name="max">value最大值</param>
        /// <param name="skip">跳过元素数量</param>
        /// <param name="take">拿取元素数量</param>        
        /// <returns>返回指定范围的元素value</returns>
        public IEnumerable<string> SortedSetRangeByValue(string key, RedisValue min = default(RedisValue), RedisValue max = default(RedisValue), long skip = 0, long take = -1)
        {
            key = AddKeyPrefix(key);
            return Database.SortedSetRangeByValue(key, min, max, skip: skip, take: take).Select(o => o.ToString());
        }

        /// <summary>
        /// 获取有序集合中指定value最大最小范围的元素value，当有序集合中的score相同时，按照value从小到大进行排序
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">redis存储key</param>
        /// <param name="min">value最小值</param>
        /// <param name="max">value最大值</param>
        /// <param name="skip">跳过元素数量</param>
        /// <param name="take">拿取元素数量</param>        
        /// <returns>返回指定范围的元素value</returns>
        public IEnumerable<T> SortedSetRangeByValue<T>(string key, RedisValue min = default(RedisValue), RedisValue max = default(RedisValue), long skip = 0, long take = -1)
        {
            return SortedSetRangeByValue(key, min, max, skip, take).Select(o => o.ToObject<T>());
        }
        #endregion
        #endregion

        #region 异步方法
        #region SortedSetAddAsync
        /// <summary>
        /// 新增元素到有序集合
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="member">redis存储value</param>
        /// <param name="score">score</param>
        /// <returns>若值已存在则返回false且score被更新，否则添加成功返回true</returns>
        public async Task<bool> SortedSetAddAsync(string key, string member, double score)
        {
            key = AddKeyPrefix(key);
            return await Database.SortedSetAddAsync(key, member, score);
        }

        /// <summary>
        /// 新增元素到有序集合
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">redis存储key</param>
        /// <param name="member">redis存储value</param>
        /// <param name="score">score</param>
        /// <returns>若值已存在则返回false且score被更新，否则添加成功返回true</returns>
        public async Task<bool> SortedSetAddAsync<T>(string key, T member, double score)
        {
            if (typeof(T) == typeof(string))
                return await SortedSetAddAsync(key, member.ToOrDefault<string>(), score);

            return await SortedSetAddAsync(key, member.ToJson(), score);
        }
        #endregion

        #region SortedSetRemoveAsync
        /// <summary>
        /// 移除有序集合中指定key-value的元素
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="member">redis存储value</param>
        /// <returns>返回是否移除成功</returns>
        public async Task<bool> SortedSetRemoveAsync(string key, string member)
        {
            key = AddKeyPrefix(key);
            return await Database.SortedSetRemoveAsync(key, member);
        }

        /// <summary>
        /// 移除有序集合中指定key-value的元素
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">redis存储key</param>
        /// <param name="member">redis存储value</param>
        /// <returns>返回是否移除成功</returns>
        public async Task<bool> SortedSetRemoveAsync<T>(string key, T member)
        {
            if (typeof(T) == typeof(string))
                return await SortedSetRemoveAsync(key, member.ToOrDefault<string>());

            return await SortedSetRemoveAsync(key, member.ToJson());
        }

        /// <summary>
        /// 根据起始索引位置移除有序集合中的指定范围的元素
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="start">开始索引位置</param>
        /// <param name="stop">结束索引位置</param>
        /// <returns>返回移除元素数量</returns>
        public async Task<long> SortedSetRemoveRangeByRankAsync(string key, long start, long stop)
        {
            key = AddKeyPrefix(key);
            return await Database.SortedSetRemoveRangeByRankAsync(key, start, stop);
        }

        /// <summary>
        /// 根据score起始值移除有序集合中的指定score范围的元素
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="start">开始score</param>
        /// <param name="stop">结束score</param>
        /// <returns>返回移除元素数量</returns>
        public async Task<long> SortedSetRemoveRangeByScoreAsync(string key, double start, double stop)
        {
            key = AddKeyPrefix(key);
            return await Database.SortedSetRemoveRangeByScoreAsync(key, start, stop);
        }

        /// <summary>
        /// 根据value最大和最小值移除有序集合中的指定value范围的元素
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="min">value最小值</param>
        /// <param name="max">value最大值</param>
        /// <returns>返回移除元素数量</returns>
        public async Task<long> SortedSetRemoveRangeByValueAsync(string key, string min, string max)
        {
            key = AddKeyPrefix(key);
            return await Database.SortedSetRemoveRangeByValueAsync(key, min, max);
        }
        #endregion

        #region SortedSetIncrementAsync
        /// <summary>
        /// 按增量增加按键存储的有序集合中成员的score
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="member">redis存储value</param>
        /// <param name="value">增量值</param>
        /// <returns>返回新的score</returns>
        public async Task<double> SortedSetIncrementAsync(string key, string member, double value = 1)
        {
            key = AddKeyPrefix(key);
            return await Database.SortedSetIncrementAsync(key, member, value);
        }

        /// <summary>
        /// 按增量增加按键存储的有序集合中成员的score
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">redis存储key</param>
        /// <param name="member">redis存储value</param>
        /// <param name="value">增量值</param>
        /// <returns>返回新的score</returns>
        public async Task<double> SortedSetIncrementAsync<T>(string key, T member, double value = 1)
        {
            key = AddKeyPrefix(key);

            if (typeof(T) == typeof(string))
                return await Database.SortedSetIncrementAsync(key, member.ToOrDefault<string>(), value);

            return await Database.SortedSetIncrementAsync(key, member.ToJson(), value);
        }
        #endregion

        #region SortedSetDecrementAsync
        /// <summary>
        /// 通过递减递减存储在键处的有序集中的成员的score
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="member">redis存储value</param>
        /// <param name="value">递减值</param>
        /// <returns>返回新的score</returns>
        public async Task<double> SortedSetDecrementAsync(string key, string member, double value)
        {
            key = AddKeyPrefix(key);
            return await Database.SortedSetDecrementAsync(key, member, value);
        }

        /// <summary>
        /// 通过递减递减存储在键处的有序集中的成员的score
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">redis存储key</param>
        /// <param name="member">redis存储value</param>
        /// <param name="value">递减值</param>
        /// <returns>返回新的score</returns>
        public async Task<double> SortedSetDecrementAsync<T>(string key, T member, double value)
        {
            key = AddKeyPrefix(key);

            if (typeof(T) == typeof(string))
                return await Database.SortedSetDecrementAsync(key, member.ToOrDefault<string>(), value);

            return await Database.SortedSetDecrementAsync(key, member.ToJson(), value);
        }
        #endregion

        #region SortedSetLengthAsync
        /// <summary>
        /// 获取有序集合的长度
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <returns>返回有序集合的长度</returns>
        public async Task<long> SortedSetLengthAsync(string key)
        {
            key = AddKeyPrefix(key);
            return await Database.SortedSetLengthAsync(key);
        }
        #endregion

        #region SortedSetRankAsync
        /// <summary>
        /// 获取集合中的索引位置，从0开始
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="member">redis存储value</param>
        /// <param name="order">排序方式</param>
        /// <returns>返回索引位置</returns>
        public async Task<long?> SortedSetRankAsync(string key, string member, Order order = Order.Ascending)
        {
            key = AddKeyPrefix(key);
            return await Database.SortedSetRankAsync(key, member, order);
        }

        /// <summary>
        /// 获取集合中的索引位置，从0开始
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">redis存储key</param>
        /// <param name="member">redis存储value</param>
        /// <param name="order">排序方式</param>
        /// <returns>返回索引位置</returns>
        public async Task<long?> SortedSetRankAsync<T>(string key, T member, Order order = Order.Ascending)
        {
            key = AddKeyPrefix(key);

            if (typeof(T) == typeof(string))
                return await Database.SortedSetRankAsync(key, member.ToOrDefault<string>(), order);

            return await Database.SortedSetRankAsync(key, member.ToJson(), order);
        }
        #endregion

        #region SortedSetScoreAsync
        /// <summary>
        /// 获取有序集合中指定元素的score
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="memebr">redis存储value</param>
        /// <returns>返回指定元素的score</returns>
        public async Task<double?> SortedSetScoreAsync(string key, string memebr)
        {
            key = AddKeyPrefix(key);
            return await Database.SortedSetScoreAsync(key, memebr);
        }

        /// <summary>
        /// 获取有序集合中指定元素的score
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">redis存储key</param>
        /// <param name="memebr">redis存储value</param>
        /// <returns>返回指定元素的score</returns>
        public async Task<double?> SortedSetScoreAsync<T>(string key, T memebr)
        {
            key = AddKeyPrefix(key);

            if (typeof(T) == typeof(string))
                return await Database.SortedSetScoreAsync(key, memebr.ToOrDefault<string>());

            return await Database.SortedSetScoreAsync(key, memebr.ToJson());
        }
        #endregion

        #region SortedSetRangeAsync
        /// <summary>
        /// 获取有序集合中指定索引范围的元素value，默认情况下从低到高
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="start">开始索引</param>
        /// <param name="stop">结束索引</param>
        /// <param name="order">排序方式</param>
        /// <returns>返回指定范围的元素value</returns>
        public async Task<IEnumerable<string>> SortedSetRangeByRankAsync(string key, long start = 0, long stop = -1, Order order = Order.Ascending)
        {
            key = AddKeyPrefix(key);
            return (await Database.SortedSetRangeByRankAsync(key, start, stop, order)).Select(o => o.ToString());
        }

        /// <summary>
        /// 获取有序集合中指定索引范围的元素value，默认情况下从低到高
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">redis存储key</param>
        /// <param name="start">开始索引</param>
        /// <param name="stop">结束索引</param>
        /// <param name="order">排序方式</param>
        /// <returns>返回指定范围的元素value</returns>
        public async Task<IEnumerable<T>> SortedSetRangeByRankAsync<T>(string key, long start = 0, long stop = -1, Order order = Order.Ascending)
        {
            return (await SortedSetRangeByRankAsync(key, start, stop, order)).Select(o => o.ToObject<T>());
        }

        /// <summary>
        /// 获取有序集合中指定索引范围的元素value-score，默认情况下从低到高
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="start">开始索引</param>
        /// <param name="stop">结束索引</param>
        /// <param name="order">排序方式</param>        
        /// <returns>返回指定范围的元素value-score</returns>
        public async Task<Dictionary<string, double>> SortedSetRangeByRankWithScoresAsync(string key, long start = 0, long stop = -1, Order order = Order.Ascending)
        {
            key = AddKeyPrefix(key);
            var result = await Database.SortedSetRangeByRankWithScoresAsync(key, start, stop, order);
            return result.Select(x => new { x.Score, Value = x.Element.ToString() }).ToDictionary(x => x.Value, x => x.Score);
        }

        /// <summary>
        /// 获取有序集合中指定索引范围的元素value-score，默认情况下从低到高
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">redis存储key</param>
        /// <param name="start">开始索引</param>
        /// <param name="stop">结束索引</param>
        /// <param name="order">排序方式</param>        
        /// <returns>返回指定范围的元素value-score</returns>
        public async Task<Dictionary<T, double>> SortedSetRangeByRankWithScoresAsync<T>(string key, long start = 0, long stop = -1, Order order = Order.Ascending)
        {
            return (await SortedSetRangeByRankWithScoresAsync(key, start, stop, order)).ToDictionary(o => o.Key.ToObject<T>(), o => o.Value);
        }

        /// <summary>
        /// 获取有序集合中指定score起始范围的元素value，默认情况下从低到高
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="start">开始score</param>
        /// <param name="stop">结束score</param>
        /// <param name="skip">跳过元素数量</param>
        /// <param name="take">拿取元素数量</param>
        /// <param name="order">排序方式</param>
        /// <returns>返回指定范围的元素value</returns>
        public async Task<IEnumerable<string>> SortedSetRangeByScoreAsync(string key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity, long skip = 0, long take = -1, Order order = Order.Ascending)
        {
            key = AddKeyPrefix(key);
            return (await Database.SortedSetRangeByScoreAsync(key, start, stop, order: order, skip: skip, take: take)).Select(o => o.ToString());
        }

        /// <summary>
        /// 获取有序集合中指定score起始范围的元素value，默认情况下从低到高
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">redis存储key</param>
        /// <param name="start">开始score</param>
        /// <param name="stop">结束score</param>
        /// <param name="skip">跳过元素数量</param>
        /// <param name="take">拿取元素数量</param>
        /// <param name="order">排序方式</param>
        /// <returns>返回指定范围的元素value</returns>
        public async Task<IEnumerable<T>> SortedSetRangeByScoreAsync<T>(string key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity, long skip = 0, long take = -1, Order order = Order.Ascending)
        {
            return (await SortedSetRangeByScoreAsync(key, start, stop, skip, take, order)).Select(o => o.ToObject<T>());
        }

        /// <summary>
        /// 获取有序集合中指定score起始范围的元素value-score，默认情况下从低到高
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="start">开始score</param>
        /// <param name="stop">结束score</param>
        /// <param name="skip">跳过元素数量</param>
        /// <param name="take">拿取元素数量</param>
        /// <param name="order">排序方式</param>
        /// <returns>返回指定范围的元素value-score</returns>
        public async Task<Dictionary<string, double>> SortedSetRangeByScoreWithScoresAsync(string key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity, long skip = 0, long take = -1, Order order = Order.Ascending)
        {
            key = AddKeyPrefix(key);
            var result = await Database.SortedSetRangeByScoreWithScoresAsync(key, start, stop, order: order, skip: skip, take: take);
            return result.Select(x => new { x.Score, Value = x.Element.ToString() }).ToDictionary(x => x.Value, x => x.Score);
        }

        /// <summary>
        /// 获取有序集合中指定score起始范围的元素value-score，默认情况下从低到高
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">redis存储key</param>
        /// <param name="start">开始score</param>
        /// <param name="stop">结束score</param>
        /// <param name="skip">跳过元素数量</param>
        /// <param name="take">拿取元素数量</param>
        /// <param name="order">排序方式</param>
        /// <returns>返回指定范围的元素value-score</returns>
        public async Task<Dictionary<T, double>> SortedSetRangeByScoreWithScoresAsync<T>(string key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity, long skip = 0, long take = -1, Order order = Order.Ascending)
        {
            return (await SortedSetRangeByScoreWithScoresAsync(key, start, stop, skip, take, order)).ToDictionary(o => o.Key.ToObject<T>(), o => o.Value);
        }

        /// <summary>
        /// 获取有序集合中指定value最大最小范围的元素value，当有序集合中的score相同时，按照value从小到大进行排序
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="min">value最小值</param>
        /// <param name="max">value最大值</param>
        /// <param name="skip">跳过元素数量</param>
        /// <param name="take">拿取元素数量</param>
        /// <returns>返回指定范围的元素value</returns>
        public async Task<IEnumerable<string>> SortedSetRangeByValueAsync(string key, RedisValue min = default(RedisValue), RedisValue max = default(RedisValue), long skip = 0, long take = -1)
        {
            key = AddKeyPrefix(key);
            return (await Database.SortedSetRangeByValueAsync(key, min, max, skip: skip, take: take)).Select(o => o.ToString());
        }

        /// <summary>
        /// 获取有序集合中指定value最大最小范围的元素value，当有序集合中的score相同时，按照value从小到大进行排序
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">redis存储key</param>
        /// <param name="min">value最小值</param>
        /// <param name="max">value最大值</param>
        /// <param name="skip">跳过元素数量</param>
        /// <param name="take">拿取元素数量</param>
        /// <returns>返回指定范围的元素value</returns>
        public async Task<IEnumerable<T>> SortedSetRangeByValueAsync<T>(string key, RedisValue min = default(RedisValue), RedisValue max = default(RedisValue), long skip = 0, long take = -1)
        {
            return (await SortedSetRangeByValueAsync(key, min, max, skip, take)).Select(o => o.ToObject<T>());
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
            var points = RedisConnection.GetEndPoints(configuredOnly);
            if (points?.Length > 0)
            {
                foreach (var point in points)
                {
                    var server = RedisConnection.GetServer(point);
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
        /// <param name="key">redis存储key</param>
        /// <returns>返回是否移除成功</returns>
        public bool KeyDelete(string key)
        {
            key = AddKeyPrefix(key);
            return Database.KeyDelete(key);
        }

        /// <summary>
        /// 移除指定key
        /// </summary>
        /// <param name="redisKeys">redis存储key集合</param>
        /// <returns>返回是否移除成功</returns>
        public long KeyDelete(IEnumerable<string> redisKeys)
        {
            var keys = redisKeys?.Select(x => (RedisKey)AddKeyPrefix(x));
            if (keys.IsNotNullOrEmpty())
                return Database.KeyDelete(keys.ToArray());

            return 0;
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
            var points = RedisConnection.GetEndPoints(configuredOnly);
            if (points?.Length > 0)
            {
                foreach (var point in points)
                {
                    var server = RedisConnection.GetServer(point);
                    var keys = server.Keys(database: database, pattern: pattern);

                    if (keys.IsNotNullOrEmpty())
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
        /// <param name="key">redis存储key</param>
        /// <returns>返回是否存在</returns>
        public bool KeyExists(string key)
        {
            key = AddKeyPrefix(key);
            return Database.KeyExists(key);
        }
        #endregion

        #region KeyRename
        /// <summary>
        /// 重命名key
        /// </summary>
        /// <param name="key">redis存储旧key</param>
        /// <param name="redisNewKey">redis存储新key</param>
        /// <returns>返回重命名是否成功</returns>
        public bool KeyRename(string key, string redisNewKey)
        {
            key = AddKeyPrefix(key);
            return Database.KeyRename(key, redisNewKey);
        }
        #endregion

        #region KeyExpire
        /// <summary>
        /// 设置key过期时间
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="expiry">过期时间</param>
        /// <returns>返回是否设置成功</returns>
        public bool KeyExpire(string key, TimeSpan? expiry)
        {
            key = AddKeyPrefix(key);
            return Database.KeyExpire(key, expiry);
        }
        #endregion

        #region KeyTtl
        /// <summary>
        /// 获取key过期时间
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TimeSpan? KeyTtl(string key)
        {
            key = AddKeyPrefix(key);
            return Database.KeyTimeToLive(key);
        }
        #endregion
        #endregion

        #region 异步方法
        #region KeysAsync
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
            var points = RedisConnection.GetEndPoints(configuredOnly);
            if (points?.Length > 0)
            {
                foreach (var point in points)
                {
                    var server = RedisConnection.GetServer(point);
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
        /// <param name="key">redis存储key</param>
        /// <returns>返回是否移除成功</returns>
        public async Task<bool> KeyDeleteAsync(string key)
        {
            key = AddKeyPrefix(key);
            return await Database.KeyDeleteAsync(key);
        }

        /// <summary>
        /// 移除指定key
        /// </summary>
        /// <param name="redisKeys">redis存储key集合</param>
        /// <returns>返回是否移除成功</returns>
        public async Task<long> KeyDeleteAsync(IEnumerable<string> redisKeys)
        {
            var keys = redisKeys?.Select(x => (RedisKey)AddKeyPrefix(x));
            if (keys.IsNotNullOrEmpty())
                return await Database.KeyDeleteAsync(keys.ToArray());

            return 0;
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
            var points = RedisConnection.GetEndPoints(configuredOnly);
            if (points?.Length > 0)
            {
                foreach (var point in points)
                {
                    var server = RedisConnection.GetServer(point);
                    var keys = server.Keys(database: database, pattern: pattern);
                    var keyDeletes = new List<RedisKey>();
                    foreach (var key in keys)
                    {
                        keyDeletes.Add(key);
                    }

                    if (keys.IsNotNullOrEmpty())
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
        /// <param name="key">redis存储key</param>
        /// <returns>返回是否存在</returns>
        public async Task<bool> KeyExistsAsync(string key)
        {
            key = AddKeyPrefix(key);
            return await Database.KeyExistsAsync(key);
        }
        #endregion

        #region KeyRenameAsync
        /// <summary>
        /// 重命名key
        /// </summary>
        /// <param name="key">redis存储旧key</param>
        /// <param name="redisNewKey">redis存储新key</param>
        /// <returns>返回重命名是否成功</returns>
        public async Task<bool> KeyRenameAsync(string key, string redisNewKey)
        {
            key = AddKeyPrefix(key);
            return await Database.KeyRenameAsync(key, redisNewKey);
        }
        #endregion

        #region KeyExpireAsync
        /// <summary>
        /// 设置key过期时间
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="expiry">过期时间</param>
        /// <returns>返回是否设置成功</returns>
        public async Task<bool> KeyExpireAsync(string key, TimeSpan? expiry)
        {
            key = AddKeyPrefix(key);
            return await Database.KeyExpireAsync(key, expiry);
        }
        #endregion

        #region KeyTtlAsync
        /// <summary>
        /// 获取key过期时间
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<TimeSpan?> KeyTtlAsync(string key)
        {
            key = AddKeyPrefix(key);
            return await Database.KeyTimeToLiveAsync(key);
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
            var points = RedisConnection.GetEndPoints(configuredOnly);
            foreach (var point in points)
            {
                var server = RedisConnection.GetServer(point);
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
            var server = RedisConnection.GetServer(host, port);
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
            var server = RedisConnection.GetServer(host, port);
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
            var points = RedisConnection.GetEndPoints(configuredOnly);
            foreach (var point in points)
            {
                var server = RedisConnection.GetServer(point);
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
            var server = RedisConnection.GetServer(host, port);
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
            var server = RedisConnection.GetServer(host, port);
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

        #region  发布/订阅
        /// <summary>
        /// 当作消息代理中间件使用
        /// 消息组建中,重要的概念便是生产者,消费者,消息中间件
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="message">消息</param>
        /// <returns>返回收到消息的客户端数量</returns>
        public long Publish(string channel, string message)
        {
            var sub = RedisConnection.GetSubscriber();
            return sub.Publish(channel, message);
        }

        /// <summary>
        /// 当作消息代理中间件使用
        /// 消息组建中,重要的概念便是生产者,消费者,消息中间件
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="message">消息</param>
        /// <returns>返回收到消息的客户端数量</returns>
        public async Task<long> PublishAsync(string channel, string message)
        {
            var sub = RedisConnection.GetSubscriber();
            return await sub.PublishAsync(channel, message);
        }

        /// <summary>
        /// 在消费者端得到该消息并输出
        /// </summary>
        /// <param name="channelFrom">通道来源</param>
        /// <param name="subscribeFn">订阅处理委托</param>
        public void Subscribe(string channelFrom, Action<RedisValue> subscribeFn)
        {
            var sub = RedisConnection.GetSubscriber();
            sub.Subscribe(channelFrom, (channel, message) => subscribeFn?.Invoke(message));
        }

        /// <summary>
        /// 在消费者端得到该消息并输出
        /// </summary>
        /// <param name="channelFrom">通道来源</param>
        /// <param name="subscribeFn">订阅处理委托</param>
        public async Task SubscribeAsync(string channelFrom, Action<RedisValue> subscribeFn)
        {
            var sub = RedisConnection.GetSubscriber();
            await sub.SubscribeAsync(channelFrom, (channel, message) => subscribeFn?.Invoke(message));
        }
        #endregion
        #endregion
    }

    /// <summary>
    /// Redis连接池
    /// </summary>
    public class RedisConnectionPoolManager : IDisposable
    {
        private readonly IConnectionMultiplexer[] _connections;
        private static readonly object _lock = new();
        private readonly int _poolSize;
        private readonly string _redisConnectionString;
        private readonly ConfigurationOptions _configuration;
        private readonly TextWriter _log;
        private bool _disposed;

        /// <summary>
        /// 构造函数
        /// </summary>
        public RedisConnectionPoolManager(
            int poolSize,
            string redisConnectionString,
            TextWriter log = null)
        {
            this._poolSize = poolSize;
            this._redisConnectionString = redisConnectionString;
            this._log = log;

            lock (_lock)
            {
                this._connections = new IConnectionMultiplexer[this._poolSize];
                this.EmitConnections();
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public RedisConnectionPoolManager(
            int poolSize,
            ConfigurationOptions configuration,
            TextWriter log = null)
        {
            this._poolSize = poolSize;
            this._configuration = configuration;
            this._log = log;

            lock (_lock)
            {
                this._connections = new IConnectionMultiplexer[this._poolSize];
                this.EmitConnections();
            }
        }

        /// <summary>
        /// 创建实例
        /// </summary>
        public static RedisConnectionPoolManager CreateInstance(
            int poolSize,
            string redisConnectionString,
            TextWriter log = null) =>
            SingletonHelper<RedisConnectionPoolManager>.GetInstance(
                poolSize, redisConnectionString, log);

        /// <summary>
        /// 创建实例
        /// </summary>
        public static RedisConnectionPoolManager CreateInstance(
            int poolSize,
            ConfigurationOptions configuration,
            TextWriter log = null) =>
            SingletonHelper<RedisConnectionPoolManager>.GetInstance(
                poolSize, configuration, log);

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 是否资源
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                foreach (var connection in this._connections)
                    connection?.Dispose();
            }

            _disposed = true;
        }

        /// <summary>
        /// 获取IConnectionMultiplexer连接
        /// </summary>
        /// <returns></returns>
        public IConnectionMultiplexer GetConnection()
        {
            var connection = this._connections.OrderBy(x => x.GetCounters().TotalOutstanding).First();

            LogHelper.Debug("Using connection {0} with {1} outstanding!", connection.GetHashCode(), connection.GetCounters().TotalOutstanding);

            return connection;
        }

        /// <summary>
        /// 获取连接池信息
        /// </summary>
        /// <returns></returns>
        public (int requiredPoolSize, int activeConnections, int invalidConnections) GetConnectionInformations()
        {
            var activeConnections = 0;
            var invalidConnections = 0;

            foreach (var connection in this._connections)
            {
                if (!connection.IsConnected)
                {
                    invalidConnections++;
                    continue;
                }

                activeConnections++;
            }

            return (this._poolSize, activeConnections, invalidConnections);
        }

        /// <summary>
        /// 初始化线程池连接
        /// </summary>
        private void EmitConnections()
        {
            for (var i = 0; i < this._poolSize; i++)
            {
                IConnectionMultiplexer connection = null;

                if (this._redisConnectionString.IsNotNullOrEmpty())
                    connection = ConnectionMultiplexer.Connect(this._redisConnectionString, this._log);

                if (this._configuration != null)
                    connection = ConnectionMultiplexer.Connect(this._configuration, this._log);

                if (connection == null)
                    throw new Exception($"Create the {i + 1} `IConnectionMultiplexer` connection fail");

                if (ConfigHelper.GetAppSettings("Redis.RegisterEvent", true))
                {
                    var hashCode = connection.GetHashCode();

                    connection.ConnectionFailed +=
                        (s, e) => LogHelper.Error(e.Exception, $"Redis(hash:{hashCode}) connection error {e.FailureType}.");

                    connection.ConnectionRestored +=
                        (s, e) => LogHelper.Error($"Redis(hash:{hashCode}) connection error restored.");

                    connection.InternalError +=
                        (s, e) => LogHelper.Error(e.Exception, $"Redis(hash:{hashCode}) internal error {e.Origin}.");

                    connection.ErrorMessage +=
                        (s, e) => LogHelper.Error($"Redis(hash:{hashCode}) error: {e.Message}");
                }

                this._connections[i] = connection;
            }
        }
    }
}
