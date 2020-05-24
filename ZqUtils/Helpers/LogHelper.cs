#region License
/***
 * Copyright © 2018-2020, 张强 (943620963@qq.com).
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

using NLog;
using System;
/****************************
* [Author] 张强
* [Date] 2020-05-24
* [Describe] 日志工具类
* **************************/
namespace ZqUtils.Helpers
{
    /// <summary>
    /// 日志工具类
    /// </summary>
    public class LogHelper
    {
        /// <summary>
        /// Logger
        /// </summary>
        private static readonly Logger logger;

        /// <summary>
        /// Constructor
        /// </summary>
        static LogHelper()
        {
            var nlog = @"XmlConfig/NLog.config".GetFullPath();
            LogManager.LogFactory.LoadConfiguration(nlog);
            logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Debug
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void Debug(string message, params object[] args)
        {
            logger.Debug(message, args);
        }

        /// <summary>
        /// Debug
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="message"></param>
        public static void Debug(Exception ex, string message)
        {
            logger.Debug(ex, message);
        }

        /// <summary>
        /// Info
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void Info(string message, params object[] args)
        {
            logger.Info(message, args);
        }

        /// <summary>
        /// Info
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="message"></param>
        public static void Info(Exception ex, string message)
        {
            logger.Info(ex, message);
        }

        /// <summary>
        /// Warn
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void Warn(string message, params object[] args)
        {
            logger.Warn(message, args);
        }

        /// <summary>
        /// Warn
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="message"></param>
        public static void Warn(Exception ex, string message)
        {
            logger.Warn(ex, message);
        }

        /// <summary>
        /// Trace
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void Trace(string message, params object[] args)
        {
            logger.Trace(message, args);
        }

        /// <summary>
        /// Trace
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="message"></param>
        public static void Trace(Exception ex, string message)
        {
            logger.Trace(ex, message);
        }

        /// <summary>
        /// Error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void Error(string message, params object[] args)
        {
            logger.Error(message, args);
        }

        /// <summary>
        /// Error
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="message"></param>
        public static void Error(Exception ex, string message)
        {
            logger.Error(ex, message);
        }

        /// <summary>
        /// Fatal
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void Fatal(string message, params object[] args)
        {
            logger.Fatal(message, args);
        }

        /// <summary>
        /// Fatal
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="message"></param>
        public static void Fatal(Exception ex, string message)
        {
            logger.Fatal(ex, message);
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="timeoutMilliseconds">Maximum time to allow for the flush. Any messages after that time will be discarded.</param>
        public static void Flush(int? timeoutMilliseconds = null)
        {
            if (timeoutMilliseconds != null)
                LogManager.Flush(timeoutMilliseconds.Value);
            else
                LogManager.Flush();
        }
    }
}