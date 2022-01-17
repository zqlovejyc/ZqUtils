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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using ZqUtils.Extensions;
/****************************
* [Author] 张强
* [Date] 2015-10-26
* [Describe] Http工具类
* **************************/
namespace ZqUtils.Helpers
{
    #region Http连接操作帮助类
    /// <summary>
    /// Http连接操作帮助类
    /// </summary>
    public class HttpHelper : IDisposable
    {
        #region 私有字段      
        /// <summary>
        /// Post数据编码
        /// </summary>
        private Encoding postEncoding = Encoding.UTF8;

        /// <summary>
        /// 设置本地的出口ip和端口
        /// </summary>
        private IPEndPoint ipEndPoint = null;

        /// <summary>
        /// HttpRequest请求对象
        /// </summary>

        private HttpRequest httpRequest = null;

        /// <summary>
        /// HttpWebRequest对象用来发起请求
        /// </summary>
        private HttpWebRequest request = null;

        /// <summary>
        /// 获取响应流的数据对象
        /// </summary>
        private HttpWebResponse response = null;

        /// <summary>
        /// Response数据编码
        /// </summary>
        private Encoding responseEncoding = Encoding.UTF8;
        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="request"></param>
        public HttpHelper(HttpRequest request = null)
        {
            if (request != null)
                httpRequest = request;
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 设置证书
        /// </summary>
        /// <param name="req">http请求参数</param>
        private void SetCer(HttpRequest req)
        {
            //这一句一定要写在创建连接的前面。使用回调的方法进行证书验证。
            if (req.Url.StartsWithIgnoreCase("https"))
                ServicePointManager.ServerCertificateValidationCallback = (request, certificate, chain, errors) => true;

            //初始化对像，并设置请求的URL地址
            request = (WebRequest.Create(req.Url) as HttpWebRequest) ?? throw new Exception("创建HttpWebRequest失败");

            //多个证书
            if (req.ClentCertificates?.Count > 0)
                SetCerList(req);

            //单个证书
            else if (req.CerPath.IsNotNullOrEmpty())
            {
                //证书是否包含密码
                if (req.CerPassword.IsNotNullOrEmpty())
                    request.ClientCertificates.Add(new X509Certificate2(req.CerPath, req.CerPassword));
                else
                    request.ClientCertificates.Add(new X509Certificate(req.CerPath));
            }
        }

        /// <summary>
        /// 设置多个证书
        /// </summary>
        /// <param name="req">http请求参数</param>
        private void SetCerList(HttpRequest req)
        {
            if (req.ClentCertificates?.Count > 0)
            {
                foreach (X509Certificate c in req.ClentCertificates)
                {
                    request.ClientCertificates.Add(c);
                }
            }
        }

        /// <summary>
        /// 设置代理
        /// </summary>
        /// <param name="req">http请求参数</param>
        private void SetProxy(HttpRequest req)
        {
            var isIeProxy = false;
            if (req.ProxyIp.IsNotNullOrEmpty())
                isIeProxy = req.ProxyIp.ContainsIgnoreCase("ieproxy");

            //非IE代理
            if (!isIeProxy)
            {
                if (req.ProxyIp.IsNotNullOrEmpty())
                {
                    //设置代理服务器
                    if (req.ProxyIp.Contains(":"))
                    {
                        var plist = req.ProxyIp.Split(':');

                        //给当前请求对象
                        request.Proxy = new WebProxy(plist[0].Trim(), Convert.ToInt32(plist[1].Trim()))
                        {
                            //建议连接
                            Credentials = new NetworkCredential(req.ProxyUserName, req.ProxyPwd)
                        };
                    }
                    else
                    {
                        //给当前请求对象
                        request.Proxy = new WebProxy(req.ProxyIp, false)
                        {
                            //建议连接
                            Credentials = new NetworkCredential(req.ProxyUserName, req.ProxyPwd)
                        };
                    }
                }
                else
                {
                    request.Proxy = req.WebProxy;
                }
            }
        }

        /// <summary>
        /// 设置Cookie
        /// </summary>
        /// <param name="req">http请求参数</param>
        private void SetCookie(HttpRequest req)
        {
            //设置Cookie字符串
            if (req.Cookie.IsNotNullOrEmpty())
                request.Headers[HttpRequestHeader.Cookie] = req.Cookie;

            //设置CookieContainer
            if (req.CookieContainer != null)
                request.CookieContainer = req.CookieContainer;
        }

        /// <summary>
        /// 设置Post数据
        /// </summary>
        /// <param name="req">http请求参数</param>
        private void SetPostData(HttpRequest req)
        {
            //验证在得到结果时是否有传入数据
            if (!request.Method.EqualIgnoreCase("get"))
            {
                //post数据编码
                if (req.PostEncoding != null)
                    postEncoding = req.PostEncoding;

                //表单数据
                byte[] buffer = null;

                //文件数据
                var fileStream = new MemoryStream();

                //分隔符
                var boundary = $"----WebKitFormBoundary{DateTime.Now.Ticks:x}";

                //byte字节
                if (req.PostDataType == PostDataType.Byte && req.PostByte?.Length > 0)
                    buffer = req.PostByte;

                //字符串
                else if (req.PostDataType == PostDataType.String && req.PostString.IsNotNullOrEmpty())
                    buffer = postEncoding.GetBytes(req.PostString);

                //文件
                else if (req.PostDataType == PostDataType.File)
                {
                    #region 表单数据+文件数据
                    var formData = new StringBuilder();

                    //表单数据
                    if (req.PostString.IsNotNullOrEmpty())
                    {
                        var formDic = req.PostString.ToDictionary();
                        if (formDic.IsNotNullOrEmpty())
                        {
                            foreach (var item in formDic)
                            {
                                formData.Append($"--{boundary}")
                                        .Append("\r\n")
                                        .Append($"Content-Disposition: form-data; name=\"{item.Key}\"")
                                        .Append("\r\n\r\n")
                                        .Append(item.Value)
                                        .Append("\r\n");
                            }
                        }
                    }

                    //文件数据
                    if (req.PostFiles.IsNotNullOrEmpty())
                    {
                        foreach (var item in req.PostFiles)
                        {
                            if (File.Exists(item.Value))
                            {
                                formData.Append($"--{boundary}")
                                        .Append("\r\n")
                                        .Append($"Content-Disposition: form-data; name=\"{item.Key}\"; filename=\"{item.Value.Substring(PathHelper.CurrentOsDirectorySeparator.ToString())}\"")
                                        .Append("\r\n")
                                        .Append($"Content-Type: application/octet-stream")
                                        .Append("\r\n\r\n");
                                //文件流
                                using var fs = new FileStream(item.Value, FileMode.Open, FileAccess.Read);
                                //写入文件
                                var fileBuffer = new byte[1024];
                                var fileBytesRead = 0;
                                while ((fileBytesRead = fs.Read(fileBuffer, 0, fileBuffer.Length)) != 0)
                                {
                                    fileStream.Write(fileBuffer, 0, fileBytesRead);
                                }
                            }
                        }
                    }

                    //文件流
                    if (req.PostFileStream?.Length > 0 && req.PostFileStreamInfo != null)
                    {
                        var (name, fileName) = req.PostFileStreamInfo.Value;

                        formData.Append($"--{boundary}")
                                .Append("\r\n")
                                .Append($"Content-Disposition: form-data; name=\"{name}\"; filename=\"{fileName}\"")
                                .Append("\r\n")
                                .Append($"Content-Type: application/octet-stream")
                                .Append("\r\n\r\n");
                        //文件流
                        using var fs = req.PostFileStream;
                        fs.Position = 0;
                        //写入文件
                        var fileBuffer = new byte[1024];
                        var fileBytesRead = 0;
                        while ((fileBytesRead = fs.Read(fileBuffer, 0, fileBuffer.Length)) != 0)
                        {
                            fileStream.Write(fileBuffer, 0, fileBytesRead);
                        }
                    }

                    //赋值表单数据
                    buffer = postEncoding.GetBytes(formData.ToString());
                    #endregion
                }

                //写入请求内容
                if (buffer?.Length > 0 || fileStream.Length > 0)
                {
                    if (fileStream.Length > 0)
                    {
                        var footData = postEncoding.GetBytes($"\r\n--{boundary}--\r\n");
                        request.ContentType = $"multipart/form-data; boundary={boundary}";
                        request.ContentLength = (buffer?.Length ?? 0) + fileStream.Length + footData.Length;

                        //请求流
                        var requestStream = request.GetRequestStream();

                        //写入表单数据
                        if (buffer != null)
                            requestStream.Write(buffer, 0, buffer.Length);

                        //写入文件内容
                        var buff = new byte[1024];
                        var bytesRead = 0;

                        //读取位置初始化
                        fileStream.Position = 0;
                        while ((bytesRead = fileStream.Read(buff, 0, buff.Length)) != 0)
                        {
                            requestStream.Write(buff, 0, bytesRead);
                        }
                        fileStream.Close();

                        //结尾 
                        requestStream.Write(footData, 0, footData.Length);
                        requestStream.Close();
                    }
                    else
                    {
                        request.ContentLength = buffer.Length;
                        request.GetRequestStream().Write(buffer, 0, buffer.Length);
                    }
                }
                else
                {
                    request.ContentLength = 0;
                }
            }
        }

        /// <summary>
        /// 为请求准备参数
        /// </summary>
        ///<param name="req">http请求参数</param>
        private void SetRequest(HttpRequest req)
        {
            //设置证书
            SetCer(req);

            //设置ip地址和端口
            if (req.IPEndPoint != null)
            {
                ipEndPoint = req.IPEndPoint;

                //设置本地的出口ip和端口
                request.ServicePoint.BindIPEndPointDelegate = (servicePoint, remoteEndPoint, retryCount) => ipEndPoint;
            }

            //设置Header参数
            if (req.Header?.Count > 0)
            {
                foreach (var key in req.Header.AllKeys)
                {
                    request.Headers.Add(key, req.Header[key]);
                }
            }

            //设置浏览器支持的编码类型
            if (req.AcceptEncoding.IsNotNullOrEmpty())
                request.Headers.Add("Accept-Encoding", req.AcceptEncoding);

            //设置用于请求的HTTP版本
            if (req.ProtocolVersion != null)
                request.ProtocolVersion = req.ProtocolVersion;

            //设置是否使用100-Continue行为
            request.ServicePoint.Expect100Continue = req.Expect100Continue;

            //请求方式Get或者Post
            request.Method = req.HttpMethod.Method;

            //请求超时时间
            request.Timeout = req.Timeout;

            //持久连接
            request.KeepAlive = req.KeepAlive;

            //写入Post数据超时间
            request.ReadWriteTimeout = req.ReadWriteTimeout;

            //设置Host的标头信息
            if (req.Host.IsNotNullOrEmpty())
                request.Host = req.Host;

            //获取和设置IfModifiedSince
            if (req.IfModifiedSince != null)
                request.IfModifiedSince = Convert.ToDateTime(req.IfModifiedSince);

            //浏览器可接受的MIME类型
            request.Accept = req.Accept;

            //Post请求数据类型
            request.ContentType = req.ContentType;

            //UserAgent客户端的访问类型，包括浏览器版本和操作系统信息
            request.UserAgent = req.UserAgent;

            //设置安全凭证
            request.Credentials = req.ICredentials;

            //来源地址
            request.Referer = req.Referer;

            //是否执行跳转功能
            request.AllowAutoRedirect = req.AllowAutoRedirect;

            //设置请求将跟随的重定向的最大数目
            if (req.MaximumAutomaticRedirections > 0)
                request.MaximumAutomaticRedirections = req.MaximumAutomaticRedirections;

            //设置最大连接
            if (req.ConnectionLimit > 0)
                request.ServicePoint.ConnectionLimit = req.ConnectionLimit;

            //设置代理
            SetProxy(req);

            //设置Cookie
            SetCookie(req);

            //设置Post数据
            SetPostData(req);
        }

        /// <summary>
        /// 设置编码
        /// </summary>
        /// <param name="req">http请求参数</param>
        /// <param name="result">http返回参数</param>
        /// <param name="responseByte">返回的字节码数组</param>
        private void SetEncoding(HttpRequest req, HttpResult result, byte[] responseByte)
        {
            //是否返回byte类型数据
            if (req.ResultType == ResultType.Byte)
                result.ResultByte = responseByte;

            //返回数据编码
            responseEncoding = req.ResponseEncoding;

            //自动识别返回数据编码
            if (responseEncoding == null)
            {
                var meta = Regex.Match(
                    Encoding.Default.GetString(responseByte),
                    "<meta[^<]*charset=([^<]*)[\"']",
                    RegexOptions.IgnoreCase);

                var c = string.Empty;
                if (meta.Groups?.Count > 0)
                    c = meta.Groups[1].Value.ToLower().Trim();

                if (c.Length > 2)
                {
                    try
                    {
                        responseEncoding = Encoding.GetEncoding(
                            c.Replace("\"", string.Empty)
                            .Replace("'", "")
                            .Replace(";", "")
                            .Replace("iso-8859-1", "gbk")
                            .Trim());
                    }
                    catch
                    {
                        if (response.CharacterSet.IsNullOrEmpty())
                            responseEncoding = Encoding.UTF8;
                        else
                            responseEncoding = Encoding.GetEncoding(response.CharacterSet);
                    }
                }
                else
                {
                    if (response.CharacterSet.IsNullOrEmpty())
                        responseEncoding = Encoding.UTF8;
                    else
                        responseEncoding = Encoding.GetEncoding(response.CharacterSet);
                }
            }
        }

        /// <summary>
        /// 获取返回的byte数据
        /// </summary>
        /// <param name="req">http请求参数</param>
        /// <param name="result">http返回参数</param>
        /// <returns>byte[]</returns>
        private byte[] GetByte(HttpRequest req, HttpResult result)
        {
            byte[] responseByte = null;
            var responseStream = response.GetResponseStream();

            //解压缩
            if (response.ContentEncoding.ContainsIgnoreCase("gzip"))
                responseStream = new GZipStream(responseStream, CompressionMode.Decompress);
            else if (response.ContentEncoding.ContainsIgnoreCase("deflate"))
                responseStream = new DeflateStream(responseStream, CompressionMode.Decompress);

            //返回类型非文件
            if (req.ResultType != ResultType.File)
            {
                using var ms = new MemoryStream();
                var buffer = new byte[1024];
                var bytesRead = 0;

                //每次读取1kb数据
                while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    ms.Write(buffer, 0, bytesRead);
                }

                responseByte = ms.ToArray();
            }

            //文件
            else if (req.DownloadSaveFilePath.IsNotNullOrEmpty())
            {
                //判断文件目录是否存在
                var dirPath = Path.GetDirectoryName(req.DownloadSaveFilePath);

                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);

                using var fs = File.Create(req.DownloadSaveFilePath);
                var buffer = new byte[1024];
                var bytesRead = 0;

                //每次读取1kb数据，然后写入文件
                while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    fs.Write(buffer, 0, bytesRead);
                }

                //赋值返回文件路径
                result.ResultFileUrl = req.DownloadSaveFilePath;
            }

            responseStream.Close();

            return responseByte;
        }

        /// <summary>
        /// 获取数据的并解析的方法
        /// </summary>
        /// <param name="req">http请求参数</param>
        /// <param name="result">http返回参数</param>
        private void GetData(HttpRequest req, HttpResult result)
        {
            if (response == null)
                return;

            //获取StatusCode
            result.StatusCode = response.StatusCode;

            //获取StatusDescription
            result.StatusDescription = response.StatusDescription;

            //获取Headers
            result.Header = response.Headers;

            //获取最后访问的URl
            result.ResponseUri = response.ResponseUri.ToString();

            //获取CookieCollection
            if (response.Cookies?.Count > 0)
                result.CookieCollection = response.Cookies;
            else if (req.CookieContainer != null)
                result.CookieCollection = request.CookieContainer.GetCookies(response.ResponseUri);

            //获取set-cookie
            if (response.Headers["set-cookie"].IsNotNullOrEmpty())
                result.Cookie = response.Headers["set-cookie"];

            //处理返回byte
            var responseByte = GetByte(req, result);
            if (responseByte?.Length > 0)
            {
                //设置编码
                SetEncoding(req, result, responseByte);

                //赋值返回结果字符串
                if (req.ResultType == ResultType.String)
                    result.ResultString = responseEncoding.GetString(responseByte);
            }
        }
        #endregion

        #region 公有方法
        /// <summary>
        /// 获取http请求结果
        /// <para>注意：需要先构造函数初始化HttpRequest</para>
        /// </summary>
        /// <returns></returns>
        public HttpResult GetResult()
        {
            if (httpRequest == null)
                throw new Exception("未初始化HttpRequest");

            return GetResult(httpRequest);
        }

        /// <summary>
        /// 获取http请求结果
        /// </summary>
        /// <param name="req">http请求参数</param>
        /// <returns>返回HttpResult类型</returns>
        public HttpResult GetResult(HttpRequest req)
        {
            //返回参数
            var result = new HttpResult();

            try
            {
                //准备参数
                SetRequest(req);
            }
            catch (Exception ex)
            {
                //配置参数时出错
                return new HttpResult
                {
                    Cookie = string.Empty,
                    Header = null,
                    ResultString = ex.Message,
                    StatusDescription = "配置参数时出错：" + ex.Message
                };
            }

            try
            {
                //请求数据
                using (response = request.GetResponse() as HttpWebResponse)
                {
                    GetData(req, result);
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    using (response = ex.Response as HttpWebResponse)
                    {
                        GetData(req, result);
                    }
                }
                else
                {
                    result.ResultString = ex.Message;
                }
            }
            catch (Exception ex)
            {
                result.ResultString = ex.Message;
            }

            //是否设置小写
            if (req.IsToLower)
                result.ResultString = result.ResultString?.ToLower();

            //重置request，response为空
            if (req.IsReset)
            {
                request = null;
                response = null;
            }

            return result;
        }

        /// <summary>
        /// 构建自动提交的支付表单页面的HTML
        /// </summary>
        /// <param name="formName">表单名称</param>
        /// <param name="actionUrl">地址</param>
        /// <param name="formType">方式(get;post)</param>
        /// <param name="keyValues">键值对数据</param>
        /// <returns></returns>
        public static string BuildForm(
            string formName,
            string actionUrl,
            string formType,
            IDictionary<string, string> keyValues)
        {
            var sb = new StringBuilder($"<form id=\"{formName}\" name=\"{formName}\" action=\"{actionUrl}\" method=\"{formType}\">");
            foreach (KeyValuePair<string, string> kp in keyValues)
            {
                sb.Append($"<input type=\"hidden\" name=\"{kp.Key}\"  id=\"{kp.Key}\" value=\"{kp.Value}\"  />");
            }
            sb.Append("</form>");
            sb.Append($"<script>document.forms['{formName}'].submit();</script>");

            return sb.ToString();
        }

        /// <summary>
        /// Aria2下载文件
        /// </summary>
        /// <param name="url">要下载的文件url</param>
        /// <param name="fileSavePath">本地文件保存路径</param>
        /// <param name="toolPath">aria2c.exe可执行文件路径</param>
        /// <param name="action">文件下载进度委托</param>
        /// <param name="command">下载文件默认命令</param>
        /// <returns>是否下载成功</returns>
        public static void DownloadByAria2(
            string url,
            string fileSavePath,
            string toolPath,
            Action<string> action = null,
            string command = " -c -s 10 -x 10 --file-allocation=none --check-certificate=false -d {0} -o {1} {2}")
        {
            var fi = new FileInfo(fileSavePath);
            command = string.Format(command, fi.DirectoryName, fi.Name, url);
            using var process = new Process();
            CmdHelper.Execute(process, toolPath, command, (s, e) => action?.Invoke(e.Data));
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            responseEncoding = Encoding.UTF8;
            postEncoding = Encoding.UTF8;
            request = null;
            response = null;
            ipEndPoint = null;
        }
        #endregion
    }
    #endregion

    #region Http请求参数类
    /// <summary>
    /// Http请求参数类
    /// </summary>
    public class HttpRequest
    {
        /// <summary>
        /// 请求URL必须填写
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 请求方式，默认：GET方式
        /// </summary>
        public HttpMethod HttpMethod { get; set; } = HttpMethod.Get;

        /// <summary>
        /// 请求超时时间，默认：100000毫秒
        /// </summary>
        public int Timeout { get; set; } = 100000;

        /// <summary>
        /// 写入Post数据超时间，默认：30000毫秒
        /// </summary>
        public int ReadWriteTimeout { get; set; } = 30000;

        /// <summary>
        /// 设置Host的标头信息
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        ///  获取或设置一个值，该值指示是否与Internet资源建立持久性连接，默认：true。
        /// </summary>
        public bool KeepAlive { get; set; } = true;

        /// <summary>
        /// 浏览器可接受的MIME类型，默认：text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8
        /// </summary>
        public string Accept { get; set; } = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";

        /// <summary>
        /// 浏览器支持的编码类型，默认：gzip,deflate
        /// </summary>
        public string AcceptEncoding { get; set; } = "gzip,deflate";

        /// <summary>
        /// Post请求数据类型，默认：text/html
        /// </summary>
        public string ContentType { get; set; } = "text/html";

        /// <summary>
        /// 浏览器类型，默认：Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.90 Safari/537.36
        /// </summary>
        public string UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.90 Safari/537.36";

        /// <summary>
        /// Post的数据类型(String/Byte/File)，默认：string
        /// </summary>
        public PostDataType PostDataType { get; set; } = PostDataType.String;

        /// <summary>
        /// Post参数编码，默认：UTF8
        /// </summary>
        public Encoding PostEncoding { get; set; }

        /// <summary>
        /// Post请求时要发送的字符串Post数据
        /// </summary>
        public string PostString { get; set; }

        /// <summary>
        /// Post请求时要发送的byte类型的Post数据
        /// </summary>
        public byte[] PostByte { get; set; }

        /// <summary>
        /// Post上传文件集合
        /// </summary>
        public IDictionary<string, string> PostFiles { get; set; }

        /// <summary>
        /// Post上传文件流
        /// </summary>
        public Stream PostFileStream { get; set; }

        /// <summary>
        /// Post上传文件流信息，如：Content-Disposition: form-data; name="file"; filename="test.xlsx"
        /// </summary>
        public (string name, string fileName)? PostFileStreamInfo { get; set; }

        /// <summary>
        /// Cookie容器
        /// </summary>
        public CookieContainer CookieContainer { get; set; }

        /// <summary>
        /// 请求时的Cookie
        /// </summary>
        public string Cookie { get; set; }

        /// <summary>
        /// 来源地址，上次访问地址
        /// </summary>
        public string Referer { get; set; }

        /// <summary>
        /// 证书绝对路径
        /// </summary>
        public string CerPath { get; set; }

        /// <summary>
        /// 证书密码
        /// </summary>
        public string CerPassword { get; set; }

        /// <summary>
        /// 设置代理对象，不想使用IE默认配置就设置为Null，而且不要设置ProxyIp
        /// </summary>
        public WebProxy WebProxy { get; set; }

        /// <summary>
        /// 是否设置为全文小写，默认：false
        /// </summary>
        public bool IsToLower { get; set; } = false;

        /// <summary>
        /// 支持跳转页面，查询结果将是跳转后的页面，默认：false
        /// </summary>
        public bool AllowAutoRedirect { get; set; } = false;

        /// <summary>
        /// 最大连接数，默认：1024
        /// </summary>
        public int ConnectionLimit { get; set; } = 1024;

        /// <summary>
        /// 代理Proxy服务器用户名
        /// </summary>
        public string ProxyUserName { get; set; }

        /// <summary>
        /// 代理服务器密码
        /// </summary>
        public string ProxyPwd { get; set; }

        /// <summary>
        /// 代理服务IP，如果要使用IE代理就设置为ieproxy
        /// </summary>
        public string ProxyIp { get; set; }

        /// <summary>
        /// header对象，默认已初始化
        /// </summary>
        public WebHeaderCollection Header { get; set; } = new WebHeaderCollection();

        /// <summary>
        ///  获取或设置用于请求的HTTP版本，返回结果：用于请求的 HTTP 版本，默认为 System.Net.HttpVersion.Version11。
        /// </summary>
        public Version ProtocolVersion { get; set; }

        /// <summary>
        ///  获取或设置一个bool值，该值确定是否使用100-Continue行为。如果POST请求需要100-Continue响应，则为true；否则为false，默认：false。
        /// </summary>
        public bool Expect100Continue { get; set; } = false;

        /// <summary>
        /// 设置509证书集合
        /// </summary>
        public X509CertificateCollection ClentCertificates { get; set; }

        /// <summary>
        /// 获取或设置请求的身份验证信息，默认： CredentialCache.DefaultCredentials
        /// </summary>
        public ICredentials ICredentials { get; set; } = CredentialCache.DefaultCredentials;

        /// <summary>
        /// 设置请求将跟随的重定向的最大数目
        /// </summary>
        public int MaximumAutomaticRedirections { get; set; }

        /// <summary>
        /// 获取和设置IfModifiedSince，默认：null
        /// </summary>
        public DateTime? IfModifiedSince { get; set; } = null;

        /// <summary>
        /// 设置本地的出口ip和端口，默认：null
        /// </summary>]
        /// <example>
        ///req.IPEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.1"),80);
        /// </example>
        public IPEndPoint IPEndPoint { get; set; } = null;

        /// <summary>
        /// 是否重置request、response的值，默认不重置，当设置为True时request、response将被设置为Null
        /// </summary>
        public bool IsReset { get; set; } = false;

        /// <summary>
        /// 当为下载文件操作时的文件保存路径
        /// </summary>
        public string DownloadSaveFilePath { get; set; }

        /// <summary>
        /// 返回数据类型(string/byte/File)，默认：string
        /// </summary>
        public ResultType ResultType { get; set; } = ResultType.String;

        /// <summary>
        /// 返回数据编码默认为null，可以自动识别，一般为utf-8、gbk、gb2312
        /// </summary>
        public Encoding ResponseEncoding { get; set; }
    }
    #endregion

    #region Http返回参数类
    /// <summary>
    /// Http返回参数类
    /// </summary>
    public class HttpResult
    {
        /// <summary>
        /// Http请求返回的Cookie
        /// </summary>
        public string Cookie { get; set; }

        /// <summary>
        /// Http请求返回的Cookie对象集合
        /// </summary>
        public CookieCollection CookieCollection { get; set; }

        /// <summary>
        /// 返回的string类型数据，只有ResultType.String时才返回数据，其它情况为空
        /// </summary>
        public string ResultString { get; set; }

        /// <summary>
        /// 返回的byte数组，只有ResultType.Byte时才返回数据，其它情况为空
        /// </summary>
        public byte[] ResultByte { get; set; }

        /// <summary>
        /// 返回下载的文件路径，只有ResultType.File时才返回数据，其它情况为空
        /// </summary>
        public string ResultFileUrl { get; set; }

        /// <summary>
        /// header对象
        /// </summary>
        public WebHeaderCollection Header { get; set; }

        /// <summary>
        /// 返回状态说明
        /// </summary>
        public string StatusDescription { get; set; }

        /// <summary>
        /// 返回状态码,默认为ok
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// 最后访问的url
        /// </summary>
        public string ResponseUri { get; set; }

        /// <summary>
        /// 获取重定向的url
        /// </summary>
        public string RedirectUrl
        {
            get
            {
                try
                {
                    if (Header?.Count > 0)
                    {
                        if (Header.AllKeys.Any(k => k.ContainsIgnoreCase("location")))
                        {
                            var baseurl = Header["location"]?.ToString().Trim();
                            if (baseurl.IsNotNullOrEmpty())
                            {
                                var b = baseurl.StartsWithIgnoreCase("http://") || baseurl.StartsWithIgnoreCase("https://");
                                if (!b)
                                    baseurl = new Uri(new Uri(ResponseUri), baseurl).AbsoluteUri;
                            }

                            return baseurl;
                        }
                    }
                }
                catch
                {
                }

                return string.Empty;
            }
        }
    }
    #endregion

    #region Http返回类型
    /// <summary>
    /// 返回类型
    /// </summary>
    public enum ResultType
    {
        /// <summary>
        /// 表示只返回字符串，只有ResultString有数据
        /// </summary>
        String,

        /// <summary>
        /// 表示返回字节数组，只有ResultByte有数据
        /// </summary>
        Byte,

        /// <summary>
        /// 表示返回文件类型，即为下载文件操作，ResultFileUrl有数据
        /// </summary>
        File
    }
    #endregion

    #region Post的数据格式
    /// <summary>
    /// Post的数据格式默认为string
    /// </summary>
    public enum PostDataType
    {
        /// <summary>
        /// 字符串类型，这时编码Encoding可不设置
        /// </summary>
        String,

        /// <summary>
        /// byte类型，需要设置PostdataByte参数的值编码Encoding可设置为空
        /// </summary>
        Byte,

        /// <summary>
        /// 文件类型，Postdata必须设置为文件的绝对路径或者PostFiles不为空，必须设置Encoding的值
        /// </summary>
        File
    }
    #endregion
}