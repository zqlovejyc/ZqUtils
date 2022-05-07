#region License
/***
 * Copyright © 2018-2025, 张强 (943620963@qq.com).
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

namespace ZqUtils.Redis
{
    /// <summary>
    /// The strategies for selecting the <see cref="IConnectionMultiplexer"/>
    /// /// </summary>
    public enum ConnectionSelectionStrategy
    {
        /// <summary>
        /// Every call will return the least loaded <see cref="IConnectionMultiplexer"/>.
        /// The load of every connection is defined by it's <see cref="ServerCounters.TotalOutstanding"/>.
        /// For more info refer to https://github.com/StackExchange/StackExchange.Redis/issues/512 .
        /// </summary>
        LeastLoaded = 0,

        /// <summary>
        /// Every call to will return the next connection in the pool in random.
        /// </summary>
        Random = 1
    }
}
