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
using System.Reflection;
using System.Threading.Tasks;
/****************************
* [Author] 张强
* [Date] 2020-09-08
* [Describe] MemberInfo扩展类
* **************************/
namespace ZqUtils.Extensions
{
    /// <summary>
    /// MemberInfo扩展类
    /// </summary>
    public static class MethodInfoExtensions
    {
        /// <summary>
        /// 调用异步方法
        /// </summary>
        /// <param name="this">方法信息</param>
        /// <param name="obj">实例</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public static async Task<object> InvokeAsync(this MethodInfo @this, object obj, params object[] parameters)
        {
            if (@this.ReturnType == typeof(Task))
                await (dynamic)@this.Invoke(obj, parameters);
            else
                return await (dynamic)@this.Invoke(obj, parameters);

            return null;
        }

        /// <summary>
        /// 调用异步方法
        /// </summary>
        /// <param name="this">方法所属类型</param>
        /// <param name="methodName">方法名称</param>
        /// <param name="obj">实例</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public static async Task<object> InvokeAsync(this Type @this, string methodName, object obj, params object[] parameters)
        {
            var method = @this.GetMethod(methodName);
            return await method.InvokeAsync(obj, parameters);
        }
    }
}
