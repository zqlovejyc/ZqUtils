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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
/****************************
* [Author] 张强
* [Date] 2018-06-14
* [Describe] 枚举扩展类
* **************************/
namespace ZqUtils.Extensions
{
    /// <summary>
    /// 枚举扩展类
    /// </summary>
    public static class EnumExtensions
    {
        #region Has
        /// <summary>
        /// 枚举变量是否包含指定标识
        /// </summary>
        /// <param name="this">枚举变量</param>
        /// <param name="flag">要判断的标识</param>
        /// <returns></returns>
        public static bool Has(this Enum @this, Enum flag)
        {
            if (@this.GetType() != flag.GetType()) throw new ArgumentException("flag", "枚举标识判断必须是相同的类型！");
            var num = Convert.ToUInt64(flag);
            return (Convert.ToUInt64(@this) & num) == num;
        }
        #endregion

        #region Set
        /// <summary>
        /// 设置标识位
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="flag"></param>
        /// <param name="value">数值</param>
        /// <returns></returns>
        public static T Set<T>(this Enum @this, T flag, bool value)
        {
            if (!(@this is T)) throw new ArgumentException("source", "枚举标识判断必须是相同的类型！");

            var s = Convert.ToUInt64(@this);
            var f = Convert.ToUInt64(flag);

            if (value)
            {
                s |= f;
            }
            else
            {
                s &= ~f;
            }

            return (T)Enum.ToObject(typeof(T), s);
        }
        #endregion

        #region GetDescription
        /// <summary>
        /// 获取枚举字段的注释
        /// </summary>
        /// <param name="this">数值</param>
        /// <returns></returns>
        public static string GetDescription(this Enum @this)
        {
            if (@this == null) return null;

            var type = @this.GetType();
            var item = type.GetField(@this.ToString(), BindingFlags.Public | BindingFlags.Static);
            //需要判断是否为null
            if (item == null) return null;
            var att = item.GetCustomAttribute<DescriptionAttribute>(false);
            if (att != null && !string.IsNullOrEmpty(att.Description)) return att.Description;

            return null;
        }
        #endregion

        #region GetDescriptions
        /// <summary>
        /// 获取枚举类型的所有字段注释
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <returns></returns>
        public static Dictionary<TEnum, string> GetDescriptions<TEnum>()
        {
            var dic = new Dictionary<TEnum, string>();
            foreach (var item in typeof(TEnum).GetDescriptions())
            {
                dic.Add((TEnum)(object)item.Key, item.Value);
            }
            return dic;
        }
        #endregion
    }
}
