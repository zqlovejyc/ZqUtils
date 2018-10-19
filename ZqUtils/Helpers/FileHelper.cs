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
using System.IO;
using System.Text;
using System.Web;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Security.Cryptography;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using ZqUtils.Extensions;
/****************************
* [Author] 张强
* [Date] 2015-10-26
* [Describe] 文件工具类
* **************************/
namespace ZqUtils.Helpers
{
    #region 实现泛型比较接口
    /// <summary>
    /// 实现泛型比较接口
    /// </summary>
    /// <typeparam name="T">待比较的数据类型</typeparam>
    public class OrderKeyCompare<T> : IComparer<T>
    {
        /// <summary>
        /// 实现比较方法接口
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        /// <returns>int</returns>
        public int Compare(T x, T y)
        {
            var b = decimal.TryParse(x.ToString(), out decimal d);
            if (b)
            {
                return decimal.Parse(x.ToString()).CompareTo(decimal.Parse(y.ToString()));
            }
            else
            {
                return x.ToString().CompareTo(y.ToString());
            }
        }
    }
    #endregion

    #region 文件帮助类
    /// <summary>
    /// 文件帮助类
    /// </summary>
    public class FileHelper
    {
        #region 日志写锁
        /// <summary>
        /// 日志写锁
        /// </summary>
        private static readonly ReaderWriterLockSlim logWriteLock = new ReaderWriterLockSlim();
        #endregion        

        #region 获取文件
        /// <summary>
        /// 续传获取文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="fileName">文件名</param>
        /// <param name="isDeleteFile">是否删除源文件</param>
        public static void GetFile(string filePath, string fileName, bool isDeleteFile = false)
        {
            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var buffer = new byte[10240];
                    HttpContext.Current.Response.Clear();
                    var dataToRead = fileStream.Length;
                    long p = 0;
                    if (HttpContext.Current.Request.Headers["Range"] != null)
                    {
                        HttpContext.Current.Response.StatusCode = 206;
                        var range = HttpContext.Current.Request.Headers["Range"].Replace("bytes=", "");
                        p = long.Parse(range.Substring(0, range.IndexOf("-")));
                    }
                    if (p != 0) HttpContext.Current.Response.AddHeader("Content-Range", $"bytes {p.ToString()}-{(dataToRead - 1).ToString()}/{dataToRead.ToString()}");
                    HttpContext.Current.Response.AddHeader("Content-Length", (dataToRead - p).ToString());
                    HttpContext.Current.Response.ContentType = "application/octet-stream";
                    HttpContext.Current.Response.AddHeader("Content-Disposition", $"attachment; filename={HttpUtility.UrlEncode(HttpContext.Current.Request.ContentEncoding.GetBytes(fileName))}");
                    fileStream.Position = p;
                    dataToRead = dataToRead - p;
                    while (dataToRead > 0)
                    {
                        if (HttpContext.Current.Response.IsClientConnected)
                        {
                            var length = fileStream.Read(buffer, 0, 10240);
                            HttpContext.Current.Response.OutputStream.Write(buffer, 0, length);
                            HttpContext.Current.Response.Flush();
                            buffer = new byte[10240];
                            dataToRead = dataToRead - length;
                        }
                        else
                        {
                            dataToRead = -1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (isDeleteFile) if (File.Exists(filePath)) File.Delete(filePath);
                HttpContext.Current.Response.End();
            }
        }

        /// <summary>
        ///  限速续传获取文件
        /// </summary>
        /// <param name="fileName">下载文件名</param>
        /// <param name="fullPath">带文件名下载路径</param>
        /// <param name="speed">每秒允许下载的字节数</param>
        /// <param name="isDeleteFile">是否删除源文件</param>        
        public static void GetFile(string fileName, string fullPath, long speed, bool isDeleteFile = false)
        {
            try
            {
                var request = HttpContext.Current.Request;
                var response = HttpContext.Current.Response;
                using (var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var binaryReader = new BinaryReader(fileStream))
                    {
                        response.AddHeader("Accept-Ranges", "bytes");
                        response.Buffer = false;
                        var fileLength = fileStream.Length;
                        long startBytes = 0;
                        var pack = 10240;  //10K bytes
                        var sleep = (int)Math.Floor((double)(1000 * pack / speed)) + 1;
                        if (request.Headers["Range"] != null)
                        {
                            response.StatusCode = 206;
                            var range = request.Headers["Range"].Split(new char[] { '=', '-' });
                            startBytes = Convert.ToInt64(range[1]);
                        }
                        response.AddHeader("Content-Length", (fileLength - startBytes).ToString());
                        if (startBytes != 0)
                        {
                            response.AddHeader("Content-Range", string.Format(" bytes {0}-{1}/{2}", startBytes, fileLength - 1, fileLength));
                        }
                        response.AddHeader("Connection", "Keep-Alive");
                        response.ContentType = "application/octet-stream";
                        response.AddHeader("Content-Disposition", "attachment;filename=" + HttpUtility.UrlEncode(fileName, Encoding.UTF8));
                        binaryReader.BaseStream.Seek(startBytes, SeekOrigin.Begin);
                        var maxCount = Math.Floor((double)((fileLength - startBytes) / pack)) + 1;
                        for (var i = 0d; i < maxCount; i++)
                        {
                            if (response.IsClientConnected)
                            {
                                response.BinaryWrite(binaryReader.ReadBytes(pack));
                                Thread.Sleep(sleep);
                            }
                            else
                            {
                                i = maxCount;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (isDeleteFile) if (File.Exists(fullPath)) File.Delete(fullPath);
                HttpContext.Current.Response.End();
            }
        }

        /// <summary>
        /// 普通获取文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="fileName">文件名称</param>
        /// <param name="contentType">文件类型【application/pdf、text/plain、text/html、application/zip、application/msword、application/vnd.ms-excel、application/vnd.ms-powerpoint、image/gif、image/png、image/jpg】</param>
        /// <param name="isDeleteFile">是否删除源文件</param>
        public static void GetFile(string filePath, string fileName, string contentType, bool isDeleteFile = false)
        {
            try
            {
                HttpContext.Current.Response.ClearContent();
                HttpContext.Current.Response.AddHeader("Pragma", "public");
                HttpContext.Current.Response.AddHeader("Expires", "0");
                HttpContext.Current.Response.AddHeader("Cache-Control", "must-revalidate, pre-check=0");
                HttpContext.Current.Response.AddHeader("Content-Disposition", $"attachment; filename={fileName}");
                HttpContext.Current.Response.AddHeader("Content-Type", contentType);
                HttpContext.Current.Response.AddHeader("Content-Transfer-Encoding", "binary");
                HttpContext.Current.Response.AddHeader("Content-Length", new FileInfo(filePath).Length.ToString());
                HttpContext.Current.Response.TransmitFile(filePath);
                HttpContext.Current.Response.Flush();//输出缓存，此部不可少
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (isDeleteFile) if (File.Exists(filePath)) File.Delete(filePath);
                HttpContext.Current.Response.End();
            }
        }

        /// <summary>
        /// 获取压缩过的文件
        /// </summary>
        /// <param name="pathArr">文件路径数组</param>
        /// <param name="zipName">压缩文件名</param>
        /// <param name="isDeleteFiles">是否删除源文件</param>
        public static void GetFileOfZip(string[] pathArr, string zipName, bool isDeleteFiles = false)
        {
            try
            {
                if (pathArr?.Length > 0)
                {
                    HttpContext.Current.Response.ContentType = "application/zip";
                    HttpContext.Current.Response.AddHeader("content-disposition", $"filename={zipName}");
                    using (var zipOutputStream = new ZipOutputStream(HttpContext.Current.Response.OutputStream))
                    {
                        zipOutputStream.SetLevel(3); //0-9, 9 being the highest level of compression
                        foreach (string fileName in pathArr)
                        {
                            using (var fs = File.OpenRead(fileName))
                            {
                                var entry = new ZipEntry(ZipEntry.CleanName(fileName))
                                {
                                    Size = fs.Length
                                };
                                //Setting the Size provides WinXP built-in extractor compatibility,
                                //but if not available, you can set zipOutputStream.UseZip64 = UseZip64.Off instead.
                                zipOutputStream.PutNextEntry(entry);
                                var buffer = new byte[4096];
                                var count = fs.Read(buffer, 0, buffer.Length);
                                while (count > 0)
                                {
                                    zipOutputStream.Write(buffer, 0, count);
                                    count = fs.Read(buffer, 0, buffer.Length);
                                    if (!HttpContext.Current.Response.IsClientConnected) break;
                                    HttpContext.Current.Response.Flush();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //删除文件
                if (isDeleteFiles)
                {
                    foreach (var j in pathArr)
                    {
                        if (File.Exists(j)) File.Delete(j);
                    }
                }
                HttpContext.Current.Response.End();
            }
        }
        #endregion

        #region 读取文件
        /// <summary>
        /// 读取文件内容
        /// </summary>
        /// <param name="filePath">物理路径</param>
        /// <returns>string</returns>
        public static string ReadFile(string filePath)
        {
            var sb = new StringBuilder();
            try
            {
                if (File.Exists(filePath))
                {
                    using (var sr = new StreamReader(filePath))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            sb.AppendLine(line);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "读取文件内容");
            }
            return sb.ToString();
        }

        /// <summary>
        /// 获取文件信息
        /// </summary>
        /// <param name="dir">目录实例数组</param>
        /// <returns>string</returns>
        public static string GetDirectoryInfo(DirectoryInfo[] dir)
        {
            var result = new StringBuilder();
            try
            {
                if (dir != null)
                {
                    var query = (from a in dir
                                 let files = a.GetFiles().Select(o => o.Name)
                                 let dirs = a.GetDirectories()
                                 select new
                                 {
                                     dirName = a.Name,
                                     filesName = files,
                                     dirsName = dirs
                                 })
                                 .OrderBy(o => o.dirName)
                                 .ToList();
                    result.Append("[");
                    query.ForEach(o =>
                    {
                        result.Append("{")
                              .Append($"\"dirName\":\"{o.dirName}\",")
                              .Append($"\"filesName\":{JsonConvert.SerializeObject(o.filesName.OrderBy(s => s.Contains(".") ? s.Substring(0, s.LastIndexOf(".")) : s, new OrderKeyCompare<string>()))},")
                              .Append($"\"childrensDirInfo\":{GetDirectoryInfo(o.dirsName)}")
                              .Append("},");
                    });
                    result = query.Count > 0 ? result.Remove(result.Length - 1, 1) : result;
                    result.Append("]");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "获取文件信息");
            }
            return result.ToString();
        }

        /// <summary>
        /// 获取指定路径下的文件目录和文件信息
        /// </summary>
        /// <param name="path">指定路径</param>
        /// <returns>string</returns>
        public static string GetDirectoryInfo(string path)
        {
            var result = new StringBuilder();
            if (Directory.Exists(path))
            {
                var di = new DirectoryInfo(path);
                var files = di.GetFiles();
                var dirs = di.GetDirectories();
                result.Append("{")
                      .Append($"\"dirName\":\"{path.Substring("\\")}\",")
                      .Append($"\"filesName\":{JsonConvert.SerializeObject(files.Select(o => o.Name).OrderBy(o => o.Contains(".") ? o.Substring(0, o.LastIndexOf(".")) : o, new OrderKeyCompare<string>()))},")
                      .Append($"\"childrensDirInfo\":{GetDirectoryInfo(dirs)}")
                      .Append("}");
            }
            return result.ToString();
        }
        #endregion

        #region 写入文本
        /// <summary>
        /// 写入文本
        /// </summary>
        /// <param name="content">文本内容</param>
        /// <param name="filePath">文件路径</param>
        /// <param name="isAppend">是否追加</param>
        /// <param name="encoding">编码格式</param>
        public static void WriteFile(string content, string filePath, bool isAppend = false, string encoding = "utf-8")
        {
            try
            {
                logWriteLock.EnterWriteLock();
                IsExist(filePath);
                using (var sw = new StreamWriter(filePath, isAppend, Encoding.GetEncoding(encoding)))
                {
                    sw.Write(content);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "写入文本");
            }
            finally
            {
                logWriteLock.ExitWriteLock();
            }
        }
        #endregion

        #region 检测文件
        /// <summary>
        /// 判断文件是否存在 不存在则创建
        /// </summary>
        /// <param name="path">物理绝对路径</param>
        public static void IsExist(string path)
        {
            if (!path.IsNull() && !File.Exists(path))
            {
                var directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                File.Create(path).Close();
            }
        }
        #endregion

        #region 复制文件
        /// <summary>
        /// 复制文件
        /// </summary>
        /// <param name="sourceFileName">源路径</param>
        /// <param name="destFileName">目的路径</param>
        /// <param name="overwrite">是否覆盖，默认：否</param>
        /// <returns>bool</returns>
        public static bool FileCopy(string sourceFileName, string destFileName, bool overwrite = false)
        {
            try
            {
                var isExist = File.Exists(destFileName);
                if (!overwrite && isExist) return true;
                if (overwrite && isExist) File.Delete(destFileName);
                using (var fStream = new FileStream(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (var fs = File.Create(destFileName))
                    {
                        var buffer = new byte[2048];
                        var bytesRead = 0;
                        //每次读取2kb数据，然后写入文件
                        while ((bytesRead = fStream.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            fs.Write(buffer, 0, bytesRead);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "复制文件");
                return false;
            }
        }
        #endregion

        #region 移动文件
        /// <summary>
        /// 移动文件
        /// </summary>
        /// <param name="sourceFileName">源路径</param>
        /// <param name="destFileName">目的路径</param>
        /// <param name="overwrite">是否覆盖，默认：否</param>
        /// <returns>bool</returns>
        public static bool FileMove(string sourceFileName, string destFileName, bool overwrite = false)
        {
            try
            {
                var isExist = File.Exists(destFileName);
                if (!overwrite && isExist) return true;
                if (overwrite && isExist) File.Delete(destFileName);
                using (var fStream = new FileStream(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (var fs = File.Create(destFileName))
                    {
                        var buffer = new byte[2048];
                        var bytesRead = 0;
                        //每次读取2kb数据，然后写入文件
                        while ((bytesRead = fStream.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            fs.Write(buffer, 0, bytesRead);
                        }
                    }
                }
                if (File.Exists(sourceFileName)) File.Delete(sourceFileName);
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "移动文件");
                return false;
            }
        }
        #endregion

        #region 获取文件MD5值
        /// <summary>
        /// 获取文件MD5 hash值
        /// </summary>
        /// <param name="filePath">文件路径(包含文件扩展名)</param>
        /// <returns>string</returns>
        public static string GetMD5HashFromFile(string filePath)
        {
            var result = string.Empty;
            try
            {
                using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var md5 = new MD5CryptoServiceProvider();
                    var retVal = md5.ComputeHash(file);
                    file.Close();
                    var sb = new StringBuilder();
                    for (int i = 0; i < retVal.Length; i++)
                    {
                        sb.Append(retVal[i].ToString("x2"));
                    }
                    result = sb.ToString();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "获取文件MD5 hash值");
            }
            return result;
        }
        #endregion

        #region base64数据保存到文件
        /// <summary>
        /// html5 base64数据保存到文件
        /// </summary>
        /// <param name="data">base64数据</param>
        /// <param name="filePath">文件路径(包含文件扩展名)</param>
        /// <returns>bool</returns>
        public static bool SaveBase64ToFile(string data, string filePath)
        {
            var result = false;
            try
            {
                var index = data?.ToLower().IndexOf("base64,") ?? -1;
                if (index > -1)
                {
                    data = data.Substring(index + 7);
                    using (var fs = File.Create(filePath))
                    {
                        var bytes = Convert.FromBase64String(data);
                        fs.Write(bytes, 0, bytes.Length);
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
                LogHelper.Error(ex, "html5 base64数据保存到文件");
            }
            return result;
        }
        #endregion        
    }
    #endregion
}
