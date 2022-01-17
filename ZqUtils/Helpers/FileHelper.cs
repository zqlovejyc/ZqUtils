#region License
/***
 * Copyright © 2018-2022, 张强 (943620963@qq.com).
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

using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using ZqUtils.Extensions;
/****************************
* [Author] 张强
* [Date] 2015-10-26
* [Describe] 文件工具类
* **************************/
namespace ZqUtils.Helpers
{
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

        #region 创建文件
        /// <summary>
        /// 创建文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileBytes"></param>
        public static void Create(string filePath, params byte[] fileBytes)
        {
            using var fs = File.Create(filePath);
            fs.Write(fileBytes);
        }

        /// <summary>
        /// 创建文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileStream"></param>
        public static void Create(string filePath, Stream fileStream)
        {
            using (fileStream)
            {
                using var fs = File.Create(filePath);
                var buffer = new byte[2048];
                var count = 0;
                //每次读取2kb数据，然后写入文件
                while ((count = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    fs.Write(buffer, 0, count);
                }
            }
        }

        /// <summary>
        /// 创建文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileBytes"></param>
        public static async Task CreateAsync(string filePath, params byte[] fileBytes)
        {
            using var fs = File.Create(filePath);
            await fs.WriteAsync(fileBytes, 0, fileBytes.Length);
        }

        /// <summary>
        /// 创建文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileStream"></param>
        public static async Task CreateAsync(string filePath, Stream fileStream)
        {
            using (fileStream)
            {
                using var fs = File.Create(filePath);
                var buffer = new byte[2048];
                var count = 0;
                //每次读取2kb数据，然后写入文件
                while ((count = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    await fs.WriteAsync(buffer, 0, count);
                }
            }
        }

        /// <summary>
        /// 读取嵌入资源创建指定文件
        /// </summary>
        /// <param name="assembly">程序集</param>
        /// <param name="manifestResourcePath">嵌入资源路径</param>
        /// <param name="filePath">文件路径</param>
        public static void CreateFileFromManifestResource(Assembly assembly, string manifestResourcePath, string filePath)
        {
            if (!File.Exists(filePath))
            {
                //读取嵌入资源
                using var stream = assembly.GetManifestResourceStream(manifestResourcePath);
                using var fs = File.Create(filePath);
                byte[] buffer = new byte[2048];
                var count = 0;
                //每次读取2kb数据，然后写入文件
                while ((count = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    fs.Write(buffer, 0, count);
                }
            }
        }

        /// <summary>
        /// 读取嵌入资源创建指定文件
        /// </summary>
        /// <param name="assembly">程序集</param>
        /// <param name="manifestResourcePath">嵌入资源路径</param>
        /// <param name="filePath">文件路径</param>
        public static async Task CreateFileFromManifestResourceAsync(Assembly assembly, string manifestResourcePath, string filePath)
        {
            if (!File.Exists(filePath))
            {
                //读取嵌入资源
                using var stream = assembly.GetManifestResourceStream(manifestResourcePath);
                using var fs = File.Create(filePath);
                var buffer = new byte[2048];
                var bytesRead = 0;
                //每次读取2kb数据，然后写入文件
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    await fs.WriteAsync(buffer, 0, bytesRead);
                }
            }
        }
        #endregion

        #region 获取文件
        #region 同步方法
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
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var buffer = new byte[10240];
                HttpContext.Current.Response.Clear();
                var dataToRead = fileStream.Length;
                long position = 0;
                if (HttpContext.Current.Request.Headers["Range"] != null)
                {
                    HttpContext.Current.Response.StatusCode = 206;
                    var range = HttpContext.Current.Request.Headers["Range"].Replace("bytes=", "");
                    position = long.Parse(range.Substring(0, range.IndexOf("-")));
                }

                if (position != 0)
                    HttpContext.Current.Response.AddHeader("Content-Range", $"bytes {position}-{dataToRead - 1}/{dataToRead}");

                HttpContext.Current.Response.AddHeader("Content-Length", (dataToRead - position).ToString());
                HttpContext.Current.Response.ContentType = "application/octet-stream";
                HttpContext.Current.Response.AddHeader("Content-Disposition", $"attachment; filename={HttpUtility.UrlEncode(HttpContext.Current.Request.ContentEncoding.GetBytes(fileName))}");
                //jquery.fileDownload插件必须添加以下Cookie设置，否则successCallback回调无效                
                HttpContext.Current.Response.Cookies.Add(new HttpCookie("fileDownload", "true"));
                fileStream.Position = position;
                dataToRead -= position;
                while (dataToRead > 0)
                {
                    if (HttpContext.Current.Response.IsClientConnected)
                    {
                        var length = fileStream.Read(buffer, 0, 10240);
                        HttpContext.Current.Response.OutputStream.Write(buffer, 0, length);
                        HttpContext.Current.Response.Flush();
                        buffer = new byte[10240];
                        dataToRead -= length;
                    }
                    else
                    {
                        dataToRead = -1;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (isDeleteFile)
                {
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                }
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
                using var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var binaryReader = new BinaryReader(fileStream);
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
                //jquery.fileDownload插件必须添加以下Cookie设置，否则successCallback回调无效                
                HttpContext.Current.Response.Cookies.Add(new HttpCookie("fileDownload", "true"));
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
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (isDeleteFile)
                {
                    if (File.Exists(fullPath))
                        File.Delete(fullPath);
                }
                HttpContext.Current.Response.End();
            }
        }

        /// <summary>
        /// 普通获取文件
        /// </summary>
        /// <param name="fileBytes">文件字节</param>
        /// <param name="fileName">文件名称</param>
        /// <param name="contentType">文件类型【application/octet-stream、application/pdf、text/plain、text/html、application/zip、application/msword、application/vnd.ms-excel、application/vnd.ms-powerpoint、image/gif、image/png、image/jpg】</param>
        public static void GetFile(byte[] fileBytes, string fileName, string contentType)
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
                HttpContext.Current.Response.AddHeader("Content-Length", fileBytes.Length.ToString());
                //jquery.fileDownload插件必须添加以下Cookie设置，否则successCallback回调无效                
                HttpContext.Current.Response.Cookies.Add(new HttpCookie("fileDownload", "true"));
                HttpContext.Current.Response.BinaryWrite(fileBytes);
                HttpContext.Current.Response.Flush();//输出缓存，此部不可少
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
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
                //jquery.fileDownload插件必须添加以下Cookie设置，否则successCallback回调无效                
                HttpContext.Current.Response.Cookies.Add(new HttpCookie("fileDownload", "true"));
                HttpContext.Current.Response.TransmitFile(filePath);
                HttpContext.Current.Response.Flush();//输出缓存，此部不可少
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (isDeleteFile)
                {
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                }
                HttpContext.Current.Response.End();
            }
        }

        /// <summary>
        /// 获取压缩过的文件
        /// </summary>
        /// <param name="filePaths">文件路径数组</param>
        /// <param name="zipName">压缩文件名</param>
        /// <param name="isDeleteFiles">是否删除源文件</param>
        /// <param name="password">压缩密码</param>
        public static void GetFileOfZip(string[] filePaths, string zipName, bool isDeleteFiles = false, string password = null)
        {
            try
            {
                if (filePaths?.Length > 0)
                {
                    HttpContext.Current.Response.ContentType = "application/zip";
                    HttpContext.Current.Response.AddHeader("content-disposition", $"filename={zipName}");
                    //jquery.fileDownload插件必须添加以下Cookie设置，否则successCallback回调无效                
                    HttpContext.Current.Response.Cookies.Add(new HttpCookie("fileDownload", "true"));
                    using var zipStream = new ZipOutputStream(HttpContext.Current.Response.OutputStream);

                    if (password.IsNotNullOrEmpty())
                        zipStream.Password = password;

                    zipStream.SetLevel(3); //0-9, 9 being the highest level of compression
                    foreach (string file in filePaths)
                    {
                        using var fs = File.OpenRead(file);
                        var entry = new ZipEntry(Path.GetFileName(file)) { Size = fs.Length, IsUnicodeText = true };
                        //Setting the Size provides WinXP built-in extractor compatibility,
                        //but if not available, you can set zipOutputStream.UseZip64 = UseZip64.Off instead.
                        zipStream.PutNextEntry(entry);
                        var buffer = new byte[4096];
                        var count = 0;
                        while ((count = fs.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            zipStream.Write(buffer, 0, count);
                            if (!HttpContext.Current.Response.IsClientConnected)
                                break;
                            HttpContext.Current.Response.Flush();
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                //删除文件
                if (isDeleteFiles)
                {
                    foreach (var file in filePaths)
                    {
                        if (File.Exists(file))
                            File.Delete(file);
                    }
                }
                HttpContext.Current.Response.End();
            }
        }

        /// <summary>
        /// 获取压缩过的文件
        /// </summary>
        /// <param name="filePaths">文件路径数组</param>
        /// <param name="isDeleteFiles">是否删除源文件</param>
        /// <param name="password">压缩密码</param>
        public static byte[] GetFileOfZip(string[] filePaths, bool isDeleteFiles = false, string password = null)
        {
            try
            {
                if (filePaths?.Length > 0)
                {
                    using var stream = new MemoryStream();
                    using var zipStream = new ZipOutputStream(stream);

                    if (password.IsNotNullOrEmpty())
                        zipStream.Password = password;

                    zipStream.SetLevel(3); //0-9, 9 being the highest level of compression
                    foreach (string file in filePaths)
                    {
                        using var fs = File.OpenRead(file);
                        var entry = new ZipEntry(Path.GetFileName(file)) { Size = fs.Length, IsUnicodeText = true };
                        //Setting the Size provides WinXP built-in extractor compatibility,
                        //but if not available, you can set zipOutputStream.UseZip64 = UseZip64.Off instead.
                        zipStream.PutNextEntry(entry);
                        var buffer = new byte[4096];
                        var count = 0;
                        while ((count = fs.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            zipStream.Write(buffer, 0, count);
                            zipStream.Flush();
                        }
                    }
                    zipStream.Finish();
                    return stream.ToArray();
                }
                return null;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                //删除文件
                if (isDeleteFiles)
                {
                    foreach (var file in filePaths)
                    {
                        if (File.Exists(file))
                            File.Delete(file);
                    }
                }
            }
        }
        #endregion

        #region 异步方法
        /// <summary>
        /// 续传获取文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="fileName">文件名</param>
        /// <param name="isDeleteFile">是否删除源文件</param>
        public static async Task GetFileAsync(string filePath, string fileName, bool isDeleteFile = false)
        {
            try
            {
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var buffer = new byte[10240];
                HttpContext.Current.Response.Clear();
                var dataToRead = fileStream.Length;
                long position = 0;
                if (HttpContext.Current.Request.Headers["Range"] != null)
                {
                    HttpContext.Current.Response.StatusCode = 206;
                    var range = HttpContext.Current.Request.Headers["Range"].Replace("bytes=", "");
                    position = long.Parse(range.Substring(0, range.IndexOf("-")));
                }

                if (position != 0)
                    HttpContext.Current.Response.AddHeader("Content-Range", $"bytes {position}-{dataToRead - 1}/{dataToRead}");

                HttpContext.Current.Response.AddHeader("Content-Length", (dataToRead - position).ToString());
                HttpContext.Current.Response.ContentType = "application/octet-stream";
                HttpContext.Current.Response.AddHeader("Content-Disposition", $"attachment; filename={HttpUtility.UrlEncode(HttpContext.Current.Request.ContentEncoding.GetBytes(fileName))}");
                //jquery.fileDownload插件必须添加以下Cookie设置，否则successCallback回调无效                
                HttpContext.Current.Response.Cookies.Add(new HttpCookie("fileDownload", "true"));
                fileStream.Position = position;
                dataToRead -= position;
                while (dataToRead > 0)
                {
                    if (HttpContext.Current.Response.IsClientConnected)
                    {
                        var length = await fileStream.ReadAsync(buffer, 0, 10240);
                        await HttpContext.Current.Response.OutputStream.WriteAsync(buffer, 0, length);
                        await HttpContext.Current.Response.OutputStream.FlushAsync();
                        buffer = new byte[10240];
                        dataToRead -= length;
                    }
                    else
                    {
                        dataToRead = -1;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (isDeleteFile)
                {
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                }
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
        public static async Task GetFileAsync(string fileName, string fullPath, long speed, bool isDeleteFile = false)
        {
            try
            {
                var request = HttpContext.Current.Request;
                var response = HttpContext.Current.Response;
                using var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var binaryReader = new BinaryReader(fileStream);
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
                //jquery.fileDownload插件必须添加以下Cookie设置，否则successCallback回调无效                
                HttpContext.Current.Response.Cookies.Add(new HttpCookie("fileDownload", "true"));
                binaryReader.BaseStream.Seek(startBytes, SeekOrigin.Begin);
                var maxCount = Math.Floor((double)((fileLength - startBytes) / pack)) + 1;
                for (var i = 0d; i < maxCount; i++)
                {
                    if (response.IsClientConnected)
                    {
                        var bytes = binaryReader.ReadBytes(pack);
                        await response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
                        Thread.Sleep(sleep);
                    }
                    else
                    {
                        i = maxCount;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (isDeleteFile)
                {
                    if (File.Exists(fullPath))
                        File.Delete(fullPath);
                }
                HttpContext.Current.Response.End();
            }
        }

        /// <summary>
        /// 普通获取文件
        /// </summary>
        /// <param name="fileBytes">文件字节</param>
        /// <param name="fileName">文件名称</param>
        /// <param name="contentType">文件类型【application/octet-stream、application/pdf、text/plain、text/html、application/zip、application/msword、application/vnd.ms-excel、application/vnd.ms-powerpoint、image/gif、image/png、image/jpg】</param>
        public static async Task GetFileAsync(byte[] fileBytes, string fileName, string contentType)
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
                HttpContext.Current.Response.AddHeader("Content-Length", fileBytes.Length.ToString());
                //jquery.fileDownload插件必须添加以下Cookie设置，否则successCallback回调无效                
                HttpContext.Current.Response.Cookies.Add(new HttpCookie("fileDownload", "true"));
                await HttpContext.Current.Response.OutputStream.WriteAsync(fileBytes, 0, fileBytes.Length);
                await HttpContext.Current.Response.OutputStream.FlushAsync();//输出缓存，此部不可少
                HttpContext.Current.Response.Flush();//输出缓存，此部不可少
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
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
        public static async Task GetFileAsync(string filePath, string fileName, string contentType, bool isDeleteFile = false)
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
                //jquery.fileDownload插件必须添加以下Cookie设置，否则successCallback回调无效                
                HttpContext.Current.Response.Cookies.Add(new HttpCookie("fileDownload", "true"));
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var buffer = new byte[2048];
                var bytesRead = 0;
                //每次读取2kb数据，然后写入文件
                while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    await HttpContext.Current.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                    await HttpContext.Current.Response.OutputStream.FlushAsync();//输出缓存，此部不可少
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (isDeleteFile)
                {
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                }
                HttpContext.Current.Response.End();
            }
        }

        /// <summary>
        /// 获取压缩过的文件
        /// </summary>
        /// <param name="filePaths">文件路径数组</param>
        /// <param name="zipName">压缩文件名</param>
        /// <param name="isDeleteFiles">是否删除源文件</param>
        /// <param name="password">压缩密码</param>
        public static async Task GetFileOfZipAsync(string[] filePaths, string zipName, bool isDeleteFiles = false, string password = null)
        {
            try
            {
                if (filePaths?.Length > 0)
                {
                    HttpContext.Current.Response.ContentType = "application/zip";
                    HttpContext.Current.Response.AddHeader("content-disposition", $"filename={zipName}");
                    //jquery.fileDownload插件必须添加以下Cookie设置，否则successCallback回调无效                
                    HttpContext.Current.Response.Cookies.Add(new HttpCookie("fileDownload", "true"));
                    using var zipStream = new ZipOutputStream(HttpContext.Current.Response.OutputStream);

                    if (password.IsNotNullOrEmpty())
                        zipStream.Password = password;

                    zipStream.SetLevel(3); //0-9, 9 being the highest level of compression
                    foreach (string file in filePaths)
                    {
                        using var fs = File.OpenRead(file);
                        var entry = new ZipEntry(Path.GetFileName(file)) { Size = fs.Length, IsUnicodeText = true };
                        //Setting the Size provides WinXP built-in extractor compatibility,
                        //but if not available, you can set zipOutputStream.UseZip64 = UseZip64.Off instead.
                        zipStream.PutNextEntry(entry);
                        var buffer = new byte[4096];
                        var count = 0;
                        while ((count = await fs.ReadAsync(buffer, 0, buffer.Length)) != 0)
                        {
                            await zipStream.WriteAsync(buffer, 0, count);
                            if (!HttpContext.Current.Response.IsClientConnected)
                                break;
                            await HttpContext.Current.Response.OutputStream.FlushAsync();
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                //删除文件
                if (isDeleteFiles)
                {
                    foreach (var file in filePaths)
                    {
                        if (File.Exists(file))
                            File.Delete(file);
                    }
                }
                HttpContext.Current.Response.End();
            }
        }

        /// <summary>
        /// 获取压缩过的文件
        /// </summary>
        /// <param name="filePaths">文件路径数组</param>
        /// <param name="isDeleteFiles">是否删除源文件</param>
        /// <param name="password">压缩密码</param>
        public static async Task<byte[]> GetFileOfZipAsync(string[] filePaths, bool isDeleteFiles = false, string password = null)
        {
            try
            {
                if (filePaths?.Length > 0)
                {
                    using var stream = new MemoryStream();
                    using var zipStream = new ZipOutputStream(stream);

                    if (password.IsNotNullOrEmpty())
                        zipStream.Password = password;

                    zipStream.SetLevel(3); //0-9, 9 being the highest level of compression
                    foreach (string file in filePaths)
                    {
                        using var fs = File.OpenRead(file);
                        var entry = new ZipEntry(Path.GetFileName(file)) { Size = fs.Length, IsUnicodeText = true };
                        //Setting the Size provides WinXP built-in extractor compatibility,
                        //but if not available, you can set zipOutputStream.UseZip64 = UseZip64.Off instead.
                        zipStream.PutNextEntry(entry);
                        var buffer = new byte[4096];
                        var count = 0;
                        while ((count = await fs.ReadAsync(buffer, 0, buffer.Length)) != 0)
                        {
                            await zipStream.WriteAsync(buffer, 0, count);
                            await zipStream.FlushAsync();
                        }
                    }
                    zipStream.Finish();
                    return stream.ToArray();
                }
                return null;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                //删除文件
                if (isDeleteFiles)
                {
                    foreach (var file in filePaths)
                    {
                        if (File.Exists(file))
                            File.Delete(file);
                    }
                }
            }
        }
        #endregion
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
            if (File.Exists(filePath))
            {
                using var sr = new StreamReader(filePath);
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    sb.AppendLine(line);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 读取文件内容
        /// </summary>
        /// <param name="filePath">物理路径</param>
        /// <returns>string</returns>
        public static async Task<string> ReadFileAsync(string filePath)
        {
            var sb = new StringBuilder();
            if (File.Exists(filePath))
            {
                using var sr = new StreamReader(filePath);
                string line;
                while ((line = await sr.ReadLineAsync()) != null)
                {
                    sb.AppendLine(line);
                }
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
            if (dir.IsNotNull())
            {
                var comparer = new IComparerHelper<string>((x, y) =>
                   x.IsValidDecimal() && y.IsValidDecimal()
                   ? decimal.Compare(x.ToDecimal(), y.ToDecimal())
                   : string.Compare(x, y));

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
                          .Append($"\"filesName\":{o.filesName.OrderBy(s => s.Contains(".") ? s.Substring(0, s.LastIndexOf(".")) : s, comparer).ToJson()},")
                          .Append($"\"childrensDirInfo\":{GetDirectoryInfo(o.dirsName)}")
                          .Append("},");
                });
                result = query.Count > 0 ? result.Remove(result.Length - 1, 1) : result;
                result.Append("]");
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
                var comparer = new IComparerHelper<string>((x, y) =>
                   x.IsValidDecimal() && y.IsValidDecimal()
                   ? decimal.Compare(x.ToDecimal(), y.ToDecimal())
                   : string.Compare(x, y));

                var di = new DirectoryInfo(path);
                var files = di.GetFiles();
                var dirs = di.GetDirectories();
                result.Append("{")
                      .Append($"\"dirName\":\"{path.Substring("\\")}\",")
                      .Append($"\"filesName\":{files?.Select(o => o.Name).OrderBy(o => o.Contains(".") ? o.Substring(0, o.LastIndexOf(".")) : o, comparer).ToJson()},")
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
                using var sw = new StreamWriter(filePath, isAppend, Encoding.GetEncoding(encoding));
                sw.Write(content);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                logWriteLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 写入文本
        /// </summary>
        /// <param name="content">文本内容</param>
        /// <param name="filePath">文件路径</param>
        /// <param name="isAppend">是否追加</param>
        /// <param name="encoding">编码格式</param>
        public static async Task WriteFileAsync(string content, string filePath, bool isAppend = false, string encoding = "utf-8")
        {
            try
            {
                logWriteLock.EnterWriteLock();
                IsExist(filePath);
                using var sw = new StreamWriter(filePath, isAppend, Encoding.GetEncoding(encoding));
                await sw.WriteAsync(content);
            }
            catch (Exception)
            {
                throw;
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

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

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
            var isExist = File.Exists(destFileName);

            if (!overwrite && isExist)
                return true;

            if (overwrite && isExist)
                File.Delete(destFileName);

            using var fStream = new FileStream(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var fs = File.Create(destFileName);
            var buffer = new byte[2048];
            var bytesRead = 0;
            //每次读取2kb数据，然后写入文件
            while ((bytesRead = fStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                fs.Write(buffer, 0, bytesRead);
            }
            return true;
        }

        /// <summary>
        /// 复制文件
        /// </summary>
        /// <param name="sourceFileName">源路径</param>
        /// <param name="destFileName">目的路径</param>
        /// <param name="overwrite">是否覆盖，默认：否</param>
        /// <returns>bool</returns>
        public static async Task<bool> FileCopyAsync(string sourceFileName, string destFileName, bool overwrite = false)
        {
            var isExist = File.Exists(destFileName);

            if (!overwrite && isExist)
                return true;

            if (overwrite && isExist)
                File.Delete(destFileName);

            using var fStream = new FileStream(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var fs = File.Create(destFileName);
            var buffer = new byte[2048];
            var bytesRead = 0;
            //每次读取2kb数据，然后写入文件
            while ((bytesRead = await fStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
            {
                await fs.WriteAsync(buffer, 0, bytesRead);
            }

            return true;
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
            var isExist = File.Exists(destFileName);

            if (!overwrite && isExist)
                return true;

            if (overwrite && isExist)
                File.Delete(destFileName);

            using var fStream = new FileStream(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var fs = File.Create(destFileName);
            var buffer = new byte[2048];
            var bytesRead = 0;
            //每次读取2kb数据，然后写入文件
            while ((bytesRead = fStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                fs.Write(buffer, 0, bytesRead);
            }

            if (File.Exists(sourceFileName))
                File.Delete(sourceFileName);

            return true;
        }

        /// <summary>
        /// 移动文件
        /// </summary>
        /// <param name="sourceFileName">源路径</param>
        /// <param name="destFileName">目的路径</param>
        /// <param name="overwrite">是否覆盖，默认：否</param>
        /// <returns>bool</returns>
        public static async Task<bool> FileMoveAsync(string sourceFileName, string destFileName, bool overwrite = false)
        {
            var isExist = File.Exists(destFileName);

            if (!overwrite && isExist)
                return true;

            if (overwrite && isExist)
                File.Delete(destFileName);

            using var fStream = new FileStream(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var fs = File.Create(destFileName);
            var buffer = new byte[2048];
            var bytesRead = 0;
            //每次读取2kb数据，然后写入文件
            while ((bytesRead = await fStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
            {
                await fs.WriteAsync(buffer, 0, bytesRead);
            }

            if (File.Exists(sourceFileName))
                File.Delete(sourceFileName);

            return true;
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
            var file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return GetMD5HashFromStream(file);
        }

        /// <summary>
        /// 获取文件流MD5 hash值
        /// </summary>
        /// <param name="fileStream">文件流</param>
        /// <returns>string</returns>
        public static string GetMD5HashFromStream(Stream fileStream)
        {
            using var md5 = new MD5CryptoServiceProvider();
            using (fileStream)
            {
                var retVal = md5.ComputeHash(fileStream);
                var sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
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
            var index = data?.ToLower().IndexOf("base64,") ?? -1;
            if (index > -1)
            {
                data = data.Substring(index + 7);
                using var fs = File.Create(filePath);
                var bytes = Convert.FromBase64String(data);
                fs.Write(bytes, 0, bytes.Length);
                return true;
            }
            return false;
        }

        /// <summary>
        /// html5 base64数据保存到文件
        /// </summary>
        /// <param name="data">base64数据</param>
        /// <param name="filePath">文件路径(包含文件扩展名)</param>
        /// <returns>bool</returns>
        public static async Task<bool> SaveBase64ToFileAsync(string data, string filePath)
        {
            var index = data?.ToLower().IndexOf("base64,") ?? -1;
            if (index > -1)
            {
                data = data.Substring(index + 7);
                using var fs = File.Create(filePath);
                var bytes = Convert.FromBase64String(data);
                await fs.WriteAsync(bytes, 0, bytes.Length);
                return true;
            }
            return false;
        }
        #endregion        
    }
}
