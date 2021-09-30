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
using System.Linq;
using System.Runtime.InteropServices;
using ZqUtils.Extensions;
using ZqUtils.Helpers;

namespace ZqUtils.Redis
{
    /// <summary>
    /// Redis连接池
    /// </summary>
    public class RedisConnectionPoolManager : IDisposable
    {
        private static readonly object _lock = new();
        private readonly IConnectionMultiplexer[] _connections;
        private readonly RedisConfiguration _redisConfiguration;
        private IntPtr _nativeResource = Marshal.AllocHGlobal(100);
        private bool _disposed;

        /// <summary>
        /// 构造函数
        /// </summary>
        public RedisConnectionPoolManager(RedisConfiguration configuration)
        {
            this._redisConfiguration = configuration;

            lock (_lock)
            {
                this._connections = new IConnectionMultiplexer[this._redisConfiguration.PoolSize];
                this.EmitConnections();
            }
        }

        /// <summary>
        /// 创建实例
        /// </summary>
        public static RedisConnectionPoolManager CreateInstance(RedisConfiguration configuration) =>
            SingletonHelper<RedisConnectionPoolManager>.GetInstance(configuration);

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

            // free native resources if there are any.
            if (_nativeResource != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_nativeResource);
                _nativeResource = IntPtr.Zero;
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
        public ConnectionPoolInformation GetConnectionInformations()
        {
            var activeConnections = 0;
            var invalidConnections = 0;

            var activeConnectionHashCodes = new List<int>();
            var invalidConnectionHashCodes = new List<int>();

            foreach (var connection in _connections)
            {
                if (!connection.IsConnected)
                {
                    invalidConnections++;
                    invalidConnectionHashCodes.Add(connection.GetHashCode());

                    continue;
                }

                activeConnections++;
                activeConnectionHashCodes.Add(connection.GetHashCode());
            }

            return new ConnectionPoolInformation()
            {
                RequiredPoolSize = _redisConfiguration.PoolSize,
                ActiveConnections = activeConnections,
                InvalidConnections = invalidConnections,
                ActiveConnectionHashCodes = activeConnectionHashCodes,
                InvalidConnectionHashCodes = invalidConnectionHashCodes
            };
        }

        /// <summary>
        /// 初始化线程池连接
        /// </summary>
        private void EmitConnections()
        {
            for (var i = 0; i < this._redisConfiguration.PoolSize; i++)
            {
                IConnectionMultiplexer connection = null;

                if (this._redisConfiguration.ConnectionString.IsNotNullOrEmpty())
                    connection = ConnectionMultiplexer.Connect(
                        this._redisConfiguration.ConnectionString,
                        this._redisConfiguration.ConnectLogger);

                if (this._redisConfiguration.ConfigurationOptions != null)
                    connection = ConnectionMultiplexer.Connect(
                        this._redisConfiguration.ConfigurationOptions,
                        this._redisConfiguration.ConnectLogger);

                if (connection == null)
                    throw new Exception($"Create the {i + 1} `IConnectionMultiplexer` connection fail");

                if (this._redisConfiguration.RegisterConnectionEvent)
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

                this._redisConfiguration.Action?.Invoke(connection);

                this._connections[i] = connection;
            }
        }
    }
}
