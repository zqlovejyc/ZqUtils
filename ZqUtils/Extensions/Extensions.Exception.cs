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
/****************************
* [Author] 张强
* [Date] 2015-10-26
* [Describe] Exception扩展类
* **************************/
namespace ZqUtils.Extensions
{
    /// <summary>
    /// Exception扩展类
    /// </summary>
    public static partial class Extensions
    {
        #region 获取内部真实异常
        /// <summary>
        /// 获取内部真实异常
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static Exception GetTrue(this Exception @this)
        {
            if (@this == null) return null;

            if (@this is AggregateException)
                return GetTrue((@this as AggregateException).Flatten().InnerException);

            if (@this is TargetInvocationException)
                return GetTrue((@this as TargetInvocationException).InnerException);

            if (@this is TypeInitializationException)
                return GetTrue((@this as TypeInitializationException).InnerException);

            return @this.GetBaseException() ?? @this;
        }
        #endregion

        #region 获取异常消息
        /// <summary>
        /// 获取异常消息
        /// </summary>
        /// <param name="this">异常</param>
        /// <returns></returns>
        public static string GetMessage(this Exception @this)
        {
            var msg = @this + "";
            if (msg.IsNullOrEmpty()) return null;

            var ss = msg.Split(Environment.NewLine);
            var ns = ss.Where(e =>
            !e.StartsWith("---") &&
            !e.Contains("System.Runtime.ExceptionServices") &&
            !e.Contains("System.Runtime.CompilerServices"));

            msg = ns.Join(Environment.NewLine);

            return msg;
        }
        #endregion
    }
}
