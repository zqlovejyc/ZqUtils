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
using System.Threading.Tasks;
/****************************
 * [Author] 张强
 * [Date] 2017-12-20
 * [Describe] 异步工具类
 * **************************/
namespace ZqUtils.Helpers
{
    /// <summary>
    /// 异步工具类
    /// </summary>
    public class TaskHelper
    {
        /// <summary>  
        /// 异步执行同步方法  
        /// </summary>  
        /// <param name="function">无返回值委托</param>  
        /// <param name="callback">回调方法</param>  
        public static async void RunAsync(Action function, Action callback = null)
        {
            await Task.Run(() => function?.Invoke());
            callback?.Invoke();
        }

        /// <summary>  
        /// 异步执行同步方法
        /// </summary>  
        /// <typeparam name="T">泛型类型</typeparam>  
        /// <param name="function">有返回值委托</param>  
        /// <param name="callback">回调方法</param>  
        public static async void RunAsync<T>(Func<T> function, Action<T> callback = null)
        {
            var result = await Task.Run(() => function == null ? default(T) : function());
            callback?.Invoke(result);
        }
    }
}
