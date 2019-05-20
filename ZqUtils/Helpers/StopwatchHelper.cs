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

using System.Diagnostics;
/****************************
* [Author] 张强
* [Date] 2018-05-16
* [Describe] Stopwatc工具类
* **************************/
namespace ZqUtils.Helpers
{
    /// <summary>
    /// Stopwatc工具类
    /// </summary>
    public class StopwatchHelper
    {
        #region 计时器开始
        /// <summary>
        /// 计时器开始
        /// </summary>
        /// <returns>Stopwatch</returns>
        public static Stopwatch TimerStart()
        {
            var watch = new Stopwatch();
            watch.Reset();
            watch.Start();
            return watch;
        }
        #endregion

        #region 计时器结束
        /// <summary>
        /// 计时器结束
        /// </summary>
        /// <param name="watch">Stopwatch</param>
        /// <returns>string</returns>
        public static string TimerEnd(Stopwatch watch)
        {
            watch.Stop();
            return watch.ElapsedMilliseconds.ToString();
        }
        #endregion
    }
}
