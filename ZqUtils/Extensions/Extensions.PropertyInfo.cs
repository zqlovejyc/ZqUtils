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

using System;
using System.Linq;
using System.Reflection;
using System.ComponentModel;
using Newtonsoft.Json;
using ZqUtils.Helpers;
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
        #region GetAttribute
        /// <summary>
        /// 泛型获取属性注解
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static T GetAttribute<T>(this PropertyInfo @this) where T : Attribute
        {
            T result = null;
            if (@this?.GetCustomAttributes(typeof(T), false).FirstOrDefault() is T attribute)
            {
                result = attribute;
            }
            return result;
        }
        #endregion

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

        #region GetDescription
        /// <summary>
        /// 获取DescriptionAttribute属性名称
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static string GetDescription(this PropertyInfo @this)
        {
            var result = @this.Name;
            if (@this?.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() is DescriptionAttribute attribute)
            {
                result = attribute.Description;
            }
            return result;
        }
        #endregion

        #region GetExcelColumn
        /// <summary>
        /// 获取ExcelColumnAttribute属性列名称
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static string GetExcelColumn(this PropertyInfo @this)
        {
            var result = @this.Name;
            if (@this?.GetCustomAttributes(typeof(ExcelColumnAttribute), false).FirstOrDefault() is ExcelColumnAttribute attribute)
            {
                result = attribute.ColumnName;
            }
            return result;
        }
        #endregion
    }
}
