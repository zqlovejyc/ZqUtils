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
using System.Reflection;
using Newtonsoft.Json;
/****************************
* [Author] 张强
* [Date] 2018-08-20
* [Describe] PropertyInfo扩展类
* **************************/
namespace ZqUtils.Extensions
{
    /// <summary>
    /// PropertyInfo扩展类
    /// </summary>
    public static partial class Extensions
    {
        #region GetJsonProperty
        /// <summary>
        /// 获取JsonProperty属性名称
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static string GetJsonProperty(this PropertyInfo @this)
        {
            var result = @this.Name;
            try
            {
                if (@this?.GetCustomAttributes(typeof(JsonPropertyAttribute), false).FirstOrDefault() is JsonPropertyAttribute jpa)
                {
                    result = jpa.PropertyName;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }
        #endregion
    }
}
