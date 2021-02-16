#region License
/***
 * Copyright © 2018-2021, 张强 (943620963@qq.com).
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
* [Date] 2021-02-16
* [Describe] IComparer泛型比较器工具类
* **************************/
namespace ZqUtils.Helpers
{
    /// <summary>
    /// IComparer泛型比较器工具类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IComparerHelper<T> : IComparer<T>
    {
        private readonly Func<T, T, int> _comparer;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="comparer">自定义比较委托</param>
        public IComparerHelper(Func<T, T, int> comparer = null)
        {
            _comparer = comparer;
        }

        /// <summary>
        /// 实现比较方法接口
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        /// <returns>int</returns>
        public int Compare(T x, T y)
        {
            if (_comparer == null)
                return string.Compare(x?.ToString(), y?.ToString());

            return _comparer(x, y);
        }
    }
}
