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
using System.Data.SqlClient;
using System.Reflection;
/****************************
* [Author] 张强
* [Date] 2018-07-10
* [Describe] SqlBulkCopy扩展类
* **************************/
namespace ZqUtils.Extensions
{
    /// <summary>
    /// SqlBulkCopy扩展类
    /// </summary>
    public static partial class Extensions
    {
        #region GetConnection
        /// <summary>
        /// A SqlBulkCopy extension method that gets a connection.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The connection.</returns>
        public static SqlConnection GetConnection(this SqlBulkCopy @this)
        {
            Type type = @this.GetType();
            FieldInfo field = type.GetField("_connection", BindingFlags.NonPublic | BindingFlags.Instance);
            // ReSharper disable PossibleNullReferenceException
            return field.GetValue(@this) as SqlConnection;
            // ReSharper restore PossibleNullReferenceException
        }
        #endregion

        #region GetTransaction
        /// <summary>
        /// A SqlBulkCopy extension method that gets a transaction.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The transaction.</returns>
        public static SqlTransaction GetTransaction(this SqlBulkCopy @this)
        {
            Type type = @this.GetType();
            FieldInfo field = type.GetField("_externalTransaction", BindingFlags.NonPublic | BindingFlags.Instance);
            // ReSharper disable PossibleNullReferenceException
            return field.GetValue(@this) as SqlTransaction;
            // ReSharper restore PossibleNullReferenceException
        }
        #endregion
    }
}
