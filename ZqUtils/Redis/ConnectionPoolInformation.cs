#region License
/***
 * Copyright © 2018-2022, 张强 (943620963@qq.com).
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

using System.Collections.Generic;

namespace ZqUtils.Redis
{
    /// <summary>
    /// A class that contains redis connection pool informations.
    /// </summary>
    public class ConnectionPoolInformation
    {
        /// <summary>
        /// Gets or sets the connection pool desiderated size.
        /// </summary>
        public int RequiredPoolSize { get; set; }

        /// <summary>
        /// Gets or sets the number of active connections in the connection pool.
        /// </summary>
        public int ActiveConnections { get; set; }

        /// <summary>
        /// Gets or sets the hash code of active connections in the connection pool.
        /// </summary>
        public List<int> ActiveConnectionHashCodes { get; set; }

        /// <summary>
        /// Gets or sets the number of invalid connections in the connection pool.
        /// </summary>
        public int InvalidConnections { get; set; }

        /// <summary>
        /// Gets or sets the hash code of invalid connections in the connection pool.
        /// </summary>
        public List<int> InvalidConnectionHashCodes { get; set; }
    }
}
