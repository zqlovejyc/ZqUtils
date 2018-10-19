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

using System.Collections.Generic;
using System.Data.SqlClient;
/****************************
* [Author] 张强
* [Date] 2018-07-10
* [Describe] SqlParameterCollection扩展类
* **************************/
namespace ZqUtils.Extensions
{
    /// <summary>
    /// SqlParameterCollection扩展类
    /// </summary>
    public static partial class Extensions
    {
        #region AddRangeWithValue
        /// <summary>
        /// A SqlParameterCollection extension method that adds a range with value to 'values'.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="values">The values.</param>
        public static void AddRangeWithValue(this SqlParameterCollection @this, Dictionary<string, object> values)
        {
            foreach (var keyValuePair in values)
            {
                @this.AddWithValue(keyValuePair.Key, keyValuePair.Value);
            }
        }
        #endregion
    }
}
