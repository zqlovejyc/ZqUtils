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