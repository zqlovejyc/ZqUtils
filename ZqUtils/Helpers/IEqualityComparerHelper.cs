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
* [Date] 2020-06-15
* [Describe] IEqualityComparer泛型比较器工具类
* **************************/
namespace ZqUtils.Helpers
{
    /// <summary>
    /// IEqualityComparer泛型比较器
    /// </summary>
    public class IEqualityComparerHelper<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> _comparator;
        private readonly Func<T, int> _hashCoder;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="comparator">自定义比较委托</param>
        /// <param name="hashCoder">hash编码获取委托</param>
        public IEqualityComparerHelper(Func<T, T, bool> comparator = null, Func<T, int> hashCoder = null)
        {
            _comparator = comparator;
            _hashCoder = hashCoder;
        }

        /// <summary>
        /// 比较方法
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        bool IEqualityComparer<T>.Equals(T x, T y)
        {
            if (_comparator == null)
                return x.Equals(y);

            return _comparator(x, y);
        }

        /// <summary>
        /// Hash编码
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        int IEqualityComparer<T>.GetHashCode(T obj)
        {
            if (_hashCoder == null)
                return obj.GetHashCode();

            return _hashCoder(obj);
        }
    }
}