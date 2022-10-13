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

using System;
using System.Collections.Generic;
/****************************
* [Author] 张强
* [Date] 2022-09-06
* [Describe] 空集合工具类
* **************************/
namespace ZqUtils.Helpers
{
    /// <summary>
    /// 空集合工具类
    /// </summary>
    public class EmptyHelper
    {
        /// <summary>
        /// 空数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] EmptyArray<T>() => new T[] { };

        /// <summary>
        /// 空IEnumerable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> EmptyIEnumerable<T>() => EmptyList<T>();

        /// <summary>
        /// 空IList
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IList<T> EmptyIList<T>() => EmptyList<T>();

        /// <summary>
        /// 空List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> EmptyList<T>() => new();

        /// <summary>
        /// 空HashSet
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static HashSet<T> EmptyHashSet<T>() => new();

        /// <summary>
        /// 空HashSet
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="comparer"></param>
        /// <returns></returns>
        public static HashSet<T> EmptyHashSet<T>(IEqualityComparer<T> comparer) => new(comparer);
    }
}
