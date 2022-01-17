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
using System.IO;

namespace ZqUtils.Redis
{
    /// <summary>
    /// The redis configuration
    /// </summary>
    public class RedisConfiguration
    {
        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the Redis configuration options
        /// </summary>
        /// <value>An instanfe of <see cref="ConfigurationOptions" />.</value>
        public ConfigurationOptions ConfigurationOptions { get; set; }

        /// <summary>
        /// Gets or sets redis connections pool size.
        /// </summary>
        public int PoolSize { get; set; }

        /// <summary>
        /// Gets or sets the action to IConnectionMultiplexer.
        /// </summary>
        public Action<IConnectionMultiplexer> Action { get; set; }

        /// <summary>
        /// Gets or sets redis `ConnectionMultiplexer.Connect` log parameter
        /// </summary>
        public TextWriter ConnectLogger { get; set; }

        /// <summary>
        /// Gets or sets IConnectionMultiplexer event
        /// </summary>
        public bool RegisterConnectionEvent { get; set; } = true;

        /// <summary>
        /// Gets or sets the every ConnectionSelectionStrategy to use during connection selection,the default is `LeastLoaded`.
        /// </summary>
        public ConnectionSelectionStrategy ConnectionSelectionStrategy { get; set; } =
            ConnectionSelectionStrategy.LeastLoaded;
    }
}
