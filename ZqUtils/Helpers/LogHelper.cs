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

using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Concurrent;
using ZqUtils.Extensions;
/****************************
* [Author] 张强
* [Date] 2015-10-26
* [Describe] 日志工具类
* **************************/
namespace ZqUtils.Helpers
{
    /// <summary>
    /// 日志工具类
    /// </summary>
    public class LogHelper
    {
        #region 私有变量
        /// <summary>
        /// 线程安全队列
        /// </summary>
        private static readonly ConcurrentQueue<LogMessage> _que;

        /// <summary>
        /// 信号
        /// </summary>
        private static readonly ManualResetEvent _mre;

        /// <summary>
        /// 日志写锁
        /// </summary>
        private static readonly ReaderWriterLockSlim _lock;
        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        static LogHelper()
        {
            _que = new ConcurrentQueue<LogMessage>();
            _mre = new ManualResetEvent(false);
            _lock = new ReaderWriterLockSlim();
            Task.Run(() => Initialize());
        }
        #endregion

        #region 信息日志
        /// <summary>
        /// 信息日志
        /// </summary>
        /// <param name="message">日志内容</param>
        /// <param name="args">字符串格式化参数</param>
        public static void Info(string message, params object[] args)
        {
            var sf = new StackTrace(true).GetFrame(1);
            var logMessage = new LogMessage
            {
                Level = LogLevel.Info,
                Message = string.Format((message?.Replace("{", "{{").Replace("}", "}}") ?? "").ReplaceOfRegex("{$1}", @"{{(\d+)}}"), args),
                StackFrame = sf
            };
            _que.Enqueue(logMessage);
            _mre.Set();
        }
        #endregion

        #region 错误日志
        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="message">自定义信息</param>
        /// <param name="args">字符串格式化参数</param>
        public static void Error(string message, params object[] args)
        {
            var sf = new StackTrace(true).GetFrame(1);
            var logMessage = new LogMessage
            {
                Level = LogLevel.Error,
                Message = string.Format((message?.Replace("{", "{{").Replace("}", "}}") ?? "").ReplaceOfRegex("{$1}", @"{{(\d+)}}"), args),
                StackFrame = sf
            };
            _que.Enqueue(logMessage);
            _mre.Set();
        }

        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="ex">错误Exception</param>
        /// <param name="message">自定义信息</param>
        /// <param name="args">字符串格式化参数</param>
        public static void Error(Exception ex, string message = "", params object[] args)
        {
            StackFrame sf = null;
            if (ex != null)
            {
                var frames = new StackTrace(ex, true).GetFrames();
                sf = frames?[frames.Length - 1];
            }
            else
            {
                sf = new StackTrace(true).GetFrame(1);
            }
            var logMessage = new LogMessage
            {
                Level = LogLevel.Error,
                Exception = ex,
                Message = string.Format((message?.Replace("{", "{{").Replace("}", "}}") ?? "").ReplaceOfRegex("{$1}", @"{{(\d+)}}"), args),
                StackFrame = sf
            };
            _que.Enqueue(logMessage);
            _mre.Set();
        }
        #endregion

        #region 私有方法/实体
        #region 日志初始化
        /// <summary>
        /// 日志初始化
        /// </summary>
        private static void Initialize()
        {
            while (true)
            {
                //等待信号通知
                _mre.WaitOne();
                //写入日志
                Write();
                //重新设置信号
                _mre.Reset();
                Thread.Sleep(1);
            }
        }
        #endregion

        #region 写入日志
        /// <summary>
        /// 写入日志
        /// </summary>
        private static void Write()
        {
            //获取物理路径
            var infoDir = (ConfigHelper.GetAppSettings<string>("logInfo") ?? @"logs\info").GetPhysicalPath();
            var errorDir = (ConfigHelper.GetAppSettings<string>("logError") ?? @"logs\error").GetPhysicalPath();

            //根据当天日期创建日志文件
            var fileName = $"{DateTime.Now:yyyy-MM-dd}.log";
            var infoPath = infoDir + fileName;
            var errorPath = errorDir + fileName;

            try
            {
                //进入写锁
                _lock.EnterWriteLock();

                //判断目录是否存在，不存在则重新创建
                if (!Directory.Exists(infoDir))
                    Directory.CreateDirectory(infoDir);

                if (!Directory.Exists(errorDir))
                    Directory.CreateDirectory(errorDir);

                //创建StreamWriter
                StreamWriter swInfo = null;
                StreamWriter swError = null;

                if (_que?.ToList().Exists(o => o.Level == LogLevel.Info) == true)
                    swInfo = new StreamWriter(infoPath, true, Encoding.UTF8);

                if (_que?.ToList().Exists(o => o.Level == LogLevel.Error) == true)
                    swError = new StreamWriter(errorPath, true, Encoding.UTF8);

                //判断日志队列中是否有内容，从列队中获取内容，并删除列队中的内容
                while (_que?.Count > 0 && _que.TryDequeue(out LogMessage logMessage))
                {
                    var sf = logMessage.StackFrame;

                    //Info
                    if (swInfo != null && logMessage.Level == LogLevel.Info)
                        swInfo.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.ffff} [Info] [{sf?.GetMethod().DeclaringType.FullName}] [{sf?.GetFileLineNumber()}] {sf?.GetMethod().Name} : {logMessage.Message}");

                    //Error
                    if (swError != null && logMessage.Level == LogLevel.Error)
                        swError.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.ffff} [Error] [{sf?.GetMethod().DeclaringType.FullName}] [{sf?.GetFileLineNumber()}] {sf?.GetMethod().Name} : {logMessage.Message} {logMessage.Exception}");
                }

                //关闭并释放资源
                if (swInfo != null)
                {
                    swInfo.Close();
                    swInfo.Dispose();
                }

                if (swError != null)
                {
                    swError.Close();
                    swError.Dispose();
                }
            }
            finally
            {
                //退出写锁
                _lock.ExitWriteLock();
            }
        }
        #endregion

        #region 日志实体
        /// <summary>
        /// 日志级别
        /// </summary>
        private enum LogLevel
        {
            Info,
            Error
        }

        /// <summary>
        /// 消息实体
        /// </summary>
        private class LogMessage
        {
            /// <summary>
            /// 日志级别
            /// </summary>
            public LogLevel Level { get; set; }

            /// <summary>
            /// 消息内容
            /// </summary>
            public string Message { get; set; }

            /// <summary>
            /// 异常对象
            /// </summary>
            public Exception Exception { get; set; }

            /// <summary>
            /// 堆栈帧信息
            /// </summary>
            public StackFrame StackFrame { get; set; }
        }
        #endregion
        #endregion
    }
}
