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
/****************************
* [Author] 张强
* [Date] 2015-10-26
* [Describe] 泛型单例工具类
* **************************/
namespace ZqUtils.Helpers
{
    /// <summary>
    /// 泛型单例工具类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SingletonHelper<T> where T : class
    {
        #region 私有字段
        /// <summary>
        /// 静态私有对象
        /// </summary>
        private static T _instance;

        /// <summary>
        /// 线程对象，线程锁使用
        /// </summary>
        private static readonly object _locker = new();
        #endregion

        #region 公有方法
        /// <summary>
        /// 静态获取实例
        /// </summary>
        /// <returns>T</returns>
        public static T GetInstance()
        {
            if (_instance == null)
            {
                lock (_locker)
                {
                    if (_instance == null)
                    {
                        _instance = Activator.CreateInstance<T>();
                    }
                }
            }
            return _instance;
        }

        /// <summary>
        /// 静态获取实例
        /// </summary>
        /// <param name="args">构造参数</param>
        /// <returns>T</returns>
        public static T GetInstance(params object[] args)
        {
            if (_instance == null)
            {
                lock (_locker)
                {
                    if (_instance == null)
                    {
                        _instance = (T)Activator.CreateInstance(typeof(T), args);
                    }
                }
            }
            return _instance;
        }
        #endregion
    }
}
