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
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;
using ZqUtils.ObjectMethodExecutors;
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
        #region Field
        /// <summary>
        /// 缓存ObjectMethodExecutor
        /// </summary>
        private static readonly ConcurrentDictionary<string, ObjectMethodExecutor> _executors =
            new ConcurrentDictionary<string, ObjectMethodExecutor>();
        #endregion

        #region InvokeAsync
        /// <summary>
        /// 调用方法，仅支持异步方法
        /// </summary>
        /// <param name="this">实例</param>
        /// <param name="methodName">方法名称</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public static async Task<object> InvokeAsync(this object @this, string methodName, params object[] parameters)
        {
            return await @this.GetType().InvokeAsync(methodName, @this, parameters);
        }

        /// <summary>
        /// 调用方法，仅支持异步方法
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
        /// 调用方法，仅支持异步方法
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
        #endregion

        #region Execute
        /// <summary>
        /// 执行方法，仅支持同步方法
        /// <para>采用微软的 <see cref="ObjectMethodExecutor" /> </para>
        /// <para>https://github.com/dotnet/aspnetcore/blob/master/src/Shared/ObjectMethodExecutor/ObjectMethodExecutor.cs </para>
        /// </summary>
        /// <param name="this">实例</param>
        /// <param name="methodName">方法名称</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public static object Execute(this object @this, string methodName, params object[] parameters)
        {
            return @this.GetType().Execute(methodName, @this, parameters);
        }

        /// <summary>
        /// 执行方法，仅支持同步方法
        /// <para>采用微软的 <see cref="ObjectMethodExecutor" /> </para>
        /// <para>https://github.com/dotnet/aspnetcore/blob/master/src/Shared/ObjectMethodExecutor/ObjectMethodExecutor.cs </para>
        /// </summary>
        /// <param name="this">方法信息</param>
        /// <param name="obj">实例</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public static object Execute(this MethodInfo @this, object obj, params object[] parameters)
        {
            var key = $"{@this.Module.Name}_{@this.ReflectedType.Name}_{@this.MetadataToken}";
            var executor = _executors.GetOrAdd(key, x => ObjectMethodExecutor.Create(@this, @this.ReflectedType.GetTypeInfo()));

            if (!executor.IsMethodAsync)
                return executor.Execute(obj, parameters);

            throw new Exception($"Not supported async method:{@this.Name}");
        }

        /// <summary>
        /// 执行方法，仅支持同步方法
        /// <para>采用微软的 <see cref="ObjectMethodExecutor" /> </para>
        /// <para>https://github.com/dotnet/aspnetcore/blob/master/src/Shared/ObjectMethodExecutor/ObjectMethodExecutor.cs </para>
        /// </summary>
        /// <param name="this">方法所属类型</param>
        /// <param name="methodName">方法名称</param>
        /// <param name="obj">实例</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public static object Execute(this Type @this, string methodName, object obj, params object[] parameters)
        {
            return @this.GetTypeInfo().GetMethod(methodName).Execute(obj, parameters);
        }
        #endregion

        #region ExecuteAsync
        /// <summary>
        /// 执行方法，同时支持同步和异步方法
        /// <para>采用微软的 <see cref="ObjectMethodExecutor" /> </para>
        /// <para>https://github.com/dotnet/aspnetcore/blob/master/src/Shared/ObjectMethodExecutor/ObjectMethodExecutor.cs </para>
        /// </summary>
        /// <param name="this">实例</param>
        /// <param name="methodName">方法名称</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public static async Task<object> ExecuteAsync(this object @this, string methodName, params object[] parameters)
        {
            return await @this.GetType().ExecuteAsync(methodName, @this, parameters);
        }

        /// <summary>
        /// 执行方法，同时支持同步和异步方法
        /// <para>采用微软的 <see cref="ObjectMethodExecutor" /> </para>
        /// <para>https://github.com/dotnet/aspnetcore/blob/master/src/Shared/ObjectMethodExecutor/ObjectMethodExecutor.cs </para>
        /// </summary>
        /// <param name="this">方法信息</param>
        /// <param name="obj">实例</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public static async Task<object> ExecuteAsync(this MethodInfo @this, object obj, params object[] parameters)
        {
            var key = $"{@this.Module.Name}_{@this.ReflectedType.Name}_{@this.MetadataToken}";
            var executor = _executors.GetOrAdd(key, x => ObjectMethodExecutor.Create(@this, @this.ReflectedType.GetTypeInfo()));

            if (executor.IsMethodAsync)
                return await executor.ExecuteAsync(obj, parameters);

            return executor.Execute(obj, parameters);
        }

        /// <summary>
        /// 执行方法，同时支持同步和异步方法
        /// <para>采用微软的 <see cref="ObjectMethodExecutor" /> </para>
        /// <para>https://github.com/dotnet/aspnetcore/blob/master/src/Shared/ObjectMethodExecutor/ObjectMethodExecutor.cs </para>
        /// </summary>
        /// <param name="this">方法所属类型</param>
        /// <param name="methodName">方法名称</param>
        /// <param name="obj">实例</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public static async Task<object> ExecuteAsync(this Type @this, string methodName, object obj, params object[] parameters)
        {
            return await @this.GetTypeInfo().GetMethod(methodName).ExecuteAsync(obj, parameters);
        }
        #endregion
    }
}
