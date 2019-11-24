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

using Dapper;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
/****************************
* [Author] 张强
* [Date] 2018-06-28
* [Describe] DbParameter扩展类
* **************************/
namespace ZqUtils.Extensions
{
    /// <summary>
    /// DbParameter扩展类
    /// </summary>
    public static partial class Extensions
    {
        #region ToDynamicParameters
        /// <summary>
        /// DbParameter转换为DynamicParameters
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static DynamicParameters ToDynamicParameters(this DbParameter[] @this)
        {
            if (@this?.Length > 0)
            {
                var args = new DynamicParameters();
                @this.ToList().ForEach(p => args.Add(p.ParameterName.Replace("?", "@").Replace(":", "@"), p.Value, p.DbType, p.Direction, p.Size));
                return args;
            }
            return null;
        }

        /// <summary>
        /// DbParameter转换为DynamicParameters
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static DynamicParameters ToDynamicParameters(this List<DbParameter> @this)
        {
            if (@this?.Count > 0)
            {
                var args = new DynamicParameters();
                @this.ForEach(p => args.Add(p.ParameterName.Replace("?", "@").Replace(":", "@"), p.Value, p.DbType, p.Direction, p.Size));
                return args;
            }
            return null;
        }

        /// <summary>
        ///  DbParameter转换为DynamicParameters
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static DynamicParameters ToDynamicParameters(this DbParameter @this)
        {
            if (@this != null)
            {
                var args = new DynamicParameters();
                args.Add(@this.ParameterName.Replace("?", "@").Replace(":", "@"), @this.Value, @this.DbType, @this.Direction, @this.Size);
                return args;
            }
            return null;
        }
        #endregion
    }
}
