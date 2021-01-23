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
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using ZqUtils.Extensions;
/****************************
* [Author] 张强
* [Date] 2020-11-26
* [Describe] cmd工具类
* **************************/
namespace ZqUtils.Helpers
{
    /// <summary>
    /// cmd工具类
    /// </summary>
    public class CmdHelper
    {
        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static CmdResult Execute(params string[] cmd)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return new CmdResult { Error = "The current system does not support it!" };

            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                ErrorDialog = false
            };

            if (cmd.IsNotNull())
            {
                var cmds = cmd.ToList();
                cmds.AddIfNotContains("exit");
                cmd = cmds.ToArray();
            }

            return Execute(startInfo, cmd);
        }

        /// <summary>
        /// 执行指定exe程序命令
        /// </summary>
        /// <param name="p">进程</param>
        /// <param name="exe">exe可执行文件路径</param>
        /// <param name="arg">参数</param>
        /// <param name="output">委托</param>
        /// <example>
        ///     <code>
        ///         var tool = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\aria2-1.34.0-win-64bit-build1\\aria2c.exe";
        ///         var fi = new FileInfo(strFileName);
        ///         var command = " -c -s 10 -x 10 --file-allocation=none --check-certificate=false -d " + fi.DirectoryName + " -o " + fi.Name + " " + url;
        ///         using (var p = new Process())
        ///         {
        ///             Execute(p, tool, command, (s, e) => ShowInfo(url, e.Data));
        ///         }
        ///     </code>
        /// </example>
        public static void Execute(
            Process p,
            string exe,
            string arg,
            DataReceivedEventHandler output)
        {
            p.StartInfo.FileName = exe;
            p.StartInfo.Arguments = arg;

            //输出信息重定向
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;

            p.OutputDataReceived += output;
            p.ErrorDataReceived += output;

            //启动线程
            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            //等待进程结束
            p.WaitForExit();
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="startInfo"></param>
        /// <param name="cmds"></param>
        /// <returns></returns>
        public static CmdResult Execute(ProcessStartInfo startInfo, string[] cmds = null)
        {
            try
            {
                using (var process = new Process { StartInfo = startInfo })
                {
                    process.Start();

                    if (cmds.IsNotNullOrEmpty())
                    {
                        foreach (var cmd in cmds)
                        {
                            process.StandardInput.WriteLine(cmd);
                        }
                    }

                    var result = process.StandardOutput.ReadToEnd();
                    var error = process.StandardError.ReadToEnd();

                    process.WaitForExit();

                    var code = process.ExitCode;

                    process.Close();

                    return new CmdResult
                    {
                        Success = code == 0,
                        Error = error,
                        Output = result
                    };
                }
            }
            catch (Exception ex)
            {
                return new CmdResult
                {
                    Error = ex.Message
                };
            }
        }
    }

    /// <summary>
    /// cmd命令返回结果
    /// </summary>
    public class CmdResult
    {
        /// <summary>
        /// 输出内容
        /// </summary>
        public string Output { get; set; }

        /// <summary>
        /// 错误内容
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }
    }
}