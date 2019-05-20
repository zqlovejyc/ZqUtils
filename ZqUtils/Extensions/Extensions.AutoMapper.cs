#region License
/***
 * Copyright © 2018-2019, 张强 (943620963@qq.com).
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

using System.Collections;
using System.Collections.Generic;
using AutoMapper;
/****************************
* [Author] 张强
* [Date] 2017-12-19
* [Describe] AutoMapper扩展类
* **************************/
namespace ZqUtils.Extensions
{
    /// <summary>
    /// AutoMapper扩展类
    /// </summary>
    public static partial class Extensions
    {
        #region 类型映射
        /// <summary>
        /// 类型映射
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="this">源数据</param>
        /// <param name="config">映射配置</param>
        /// <returns>映射后的数据</returns>
        public static T MapTo<T>(this object @this, MapperConfiguration config = null)
        {
            if (@this == null) return default(T);
            if (config == null) config = new MapperConfiguration(cfg => cfg.CreateMap(@this.GetType(), typeof(T)));
            return config.CreateMapper().Map<T>(@this);
        }

        /// <summary>
        /// 类型映射
        /// </summary>
        /// <typeparam name="S">源类型</typeparam>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="this">源数据</param>
        /// <param name="destination">已存在的目标数据</param>
        /// <param name="config">映射配置</param>
        /// <returns>映射后的数据</returns>
        public static T MapTo<S, T>(this S @this, T destination, MapperConfiguration config = null)
            where T : class
            where S : class
        {
            if (@this == null) return default(T);
            if (config == null) config = new MapperConfiguration(cfg => cfg.CreateMap<S, T>());
            return config.CreateMapper().Map(@this, destination);
        }
        #endregion

        #region 集合列表类型映射
        /// <summary>
        ///  集合列表类型映射
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="this">源数据</param>
        /// <param name="config">映射配置</param>
        /// <returns>映射后的数据</returns>
        public static List<T> MapTo<T>(this IEnumerable @this, MapperConfiguration config = null)
        {
            if (@this == null) return null;
            foreach (var item in @this)
            {
                if (config == null) config = new MapperConfiguration(cfg => cfg.CreateMap(item.GetType(), typeof(T)));
                break;
            }
            return config.CreateMapper().Map<List<T>>(@this);
        }

        /// <summary>
        /// 集合列表类型映射
        /// </summary>
        /// <typeparam name="S">源类型</typeparam>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="this">源数据</param>
        /// <param name="config">映射配置</param>
        /// <returns>映射后的数据</returns>
        public static List<T> MapTo<S, T>(this IEnumerable<S> @this, MapperConfiguration config = null)
        {
            if (@this == null) return null;
            if (config == null) config = new MapperConfiguration(cfg => cfg.CreateMap<S, T>());
            return config.CreateMapper().Map<List<T>>(@this);
        }
        #endregion
    }
}
