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
using System.Linq;
using System.Net;
using System.Text;
using System.Net.Security;
using System.IO.Compression;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates;
/****************************
* [Author] 张强
* [Date] 2017-10-10
* [Describe] Http工具类
* **************************/
namespace ZqUtils.Utilities
{
    #region Http连接操作帮助类
    /// <summary>
    /// Http连接操作帮助类
    /// </summary>
    public class HttpUtility : IDisposable
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

        #region 私有方法                
        /// <summary>
        /// 回调验证证书问题
        /// </summary>
        /// <param name="sender">流对象</param>
        /// <param name="certificate">证书</param>
        /// <param name="chain">X509Chain</param>
        /// <param name="errors">SslPolicyErrors</param>
        /// <returns>bool</returns>
        private bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) { return true; }

        /// <summary>
        /// 设置证书
        /// </summary>
        /// <param name="item">http参数</param>
        private void SetCer(HttpItem item)
        {
            //这一句一定要写在创建连接的前面。使用回调的方法进行证书验证。
            if (item.Url.StartsWith("https", StringComparison.OrdinalIgnoreCase)) ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            //初始化对像，并设置请求的URL地址
            request = WebRequest.Create(item.Url) as HttpWebRequest;
            if (request != null)
            {
                //多个证书
                if (item.ClentCertificates != null && item.ClentCertificates.Count > 0)
                {
                    SetCerList(item);
                }
                //单个证书
                else if (!string.IsNullOrEmpty(item.CerPath))
                {
                    //证书是否包含密码
                    if (!string.IsNullOrEmpty(item.CerPassword))
                    {
                        request.ClientCertificates.Add(new X509Certificate2(item.CerPath, item.CerPassword));
                    }
                    else
                    {
                        request.ClientCertificates.Add(new X509Certificate(item.CerPath));
                    }
                }
            }
        }

        /// <summary>
        /// 设置多个证书
        /// </summary>
        /// <param name="item">http参数</param>
        private void SetCerList(HttpItem item)
        {
            if (item.ClentCertificates != null && item.ClentCertificates.Count > 0)
            {
                foreach (X509Certificate c in item.ClentCertificates)
                {
                    request.ClientCertificates.Add(c);
                }
            }
        }

        /// <summary>
        /// 通过设置这个属性，可以在发出连接的时候绑定客户端发出连接所使用的IP地址。 
        /// </summary>
        /// <param name="servicePoint">ServicePoint</param>
        /// <param name="remoteEndPoint">IPEndPoint</param>
        /// <param name="retryCount">int</param>
        /// <returns>IPEndPoint</returns>
        private IPEndPoint BindIPEndPointCallback(ServicePoint servicePoint, IPEndPoint remoteEndPoint, int retryCount) { return ipEndPoint; }

        /// <summary>
        /// 设置代理
        /// </summary>
        /// <param name="item">http参数</param>
        private void SetProxy(HttpItem item)
        {
            var isIeProxy = false;
            if (!string.IsNullOrEmpty(item.ProxyIp)) isIeProxy = item.ProxyIp.ToLower().Contains("ieproxy");
            if (!string.IsNullOrEmpty(item.ProxyIp) && !isIeProxy)
            {
                //设置代理服务器
                if (item.ProxyIp.Contains(":"))
                {
                    var plist = item.ProxyIp.Split(':');
                    var myProxy = new WebProxy(plist[0].Trim(), Convert.ToInt32(plist[1].Trim()))
                    {
                        //建议连接
                        Credentials = new NetworkCredential(item.ProxyUserName, item.ProxyPwd)
                    };
                    //给当前请求对象
                    request.Proxy = myProxy;
                }
                else
                {
                    var myProxy = new WebProxy(item.ProxyIp, false)
                    {
                        //建议连接
                        Credentials = new NetworkCredential(item.ProxyUserName, item.ProxyPwd)
                    };
                    //给当前请求对象
                    request.Proxy = myProxy;
                }
            }
            else if (isIeProxy)
            {
                //设置为IE代理
            }
            else
            {
                request.Proxy = item.WebProxy;
            }
        }

        /// <summary>
        /// 设置Cookie
        /// </summary>
        /// <param name="item">http参数</param>
        private void SetCookie(HttpItem item)
        {
            if (!string.IsNullOrEmpty(item.Cookie)) request.Headers[HttpRequestHeader.Cookie] = item.Cookie;
            //设置CookieCollection
            if (item.CookieCollection != null && item.CookieCollection.Count > 0)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(item.CookieCollection);
            }
        }

        /// <summary>
        /// 设置Post数据
        /// </summary>
        /// <param name="item">http参数</param>
        private void SetPostData(HttpItem item)
        {
            //验证在得到结果时是否有传入数据
            if (!request.Method.Trim().ToLower().Contains("get"))
            {
                //Post数据编码
                if (item.PostEncoding != null) postEncoding = item.PostEncoding;
                byte[] buffer = null;
                var fileStream = new MemoryStream();
                var boundary = $"---------------------------{DateTime.Now.Ticks.ToString("x")}";
                //写入byte类型
                if (item.PostDataType == PostDataType.Byte && item.PostByte != null && item.PostByte.Length > 0)
                {
                    //验证在得到结果时是否有传入数据
                    buffer = item.PostByte;
                }
                //写入文件
                else if (item.PostDataType == PostDataType.File)
                {
                    if ((item.PostFiles == null || item.PostFiles.Count == 0) && !string.IsNullOrEmpty(item.PostString))
                    {
                        using (var sr = new StreamReader(item.PostString, postEncoding))
                        {
                            buffer = postEncoding.GetBytes(sr.ReadToEnd());
                        }
                    }
                    else
                    {
                        #region 表单数据组装
                        var formData = new StringBuilder();
                        if (!string.IsNullOrEmpty(item.PostString))
                        {
                            //表单数据
                            formData.Append($"--{boundary}")
                                    .Append("\r\n")
                                    .Append("Content-Disposition: form-data; name=\"content\"")
                                    .Append("\r\n\r\n")
                                    .Append(item.PostString)
                                    .Append("\r\n");
                        }
                        //文件数据
                        var fileList = item.PostFiles.ToList();
                        fileList.ForEach(o =>
                        {
                            if (File.Exists(o.Value))
                            {
                                formData.Append($"--{boundary}")
                                        .Append("\r\n")
                                        .Append($"Content-Disposition: form-data; name=\"{o.Key}\"; filename=\"{o.Value}\"")
                                        .Append("\r\n")
                                        .Append("Content-Type: application/octet-stream")
                                        .Append("\r\n\r\n");
                            }
                        });
                        buffer = postEncoding.GetBytes(formData.ToString());
                        #endregion

                        #region 文件数据
                        fileList.ForEach(o =>
                        {
                            if (File.Exists(o.Value))
                            {
                                using (var fs = new FileStream(o.Value, FileMode.Open, FileAccess.Read))
                                {
                                    //写入文件
                                    var fileBuffer = new byte[1024];
                                    var fileBytesRead = 0;
                                    while ((fileBytesRead = fs.Read(fileBuffer, 0, fileBuffer.Length)) != 0)
                                    {
                                        fileStream.Write(fileBuffer, 0, fileBytesRead);
                                    }
                                }
                            }
                        });
                        #endregion                                            
                    }
                }
                //写入字符串
                else if (!string.IsNullOrEmpty(item.PostString))
                {
                    buffer = postEncoding.GetBytes(item.PostString);
                }
                if (buffer != null)
                {
                    if (item.PostFiles != null && item.PostFiles.Count > 0 && fileStream.Length > 0)
                    {
                        var footData = postEncoding.GetBytes($"\r\n--{boundary}--\r\n");
                        request.ContentType = $"multipart/form-data; boundary={boundary}";
                        request.ContentLength = buffer.Length + fileStream.Length + footData.Length;
                        //请求流
                        var requestStream = request.GetRequestStream();
                        //写入表单数据
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
        ///<param name="item">http参数</param>
        private void SetRequest(HttpItem item)
        {
            // 验证证书
            SetCer(item);
            if (item.IPEndPoint != null)
            {
                ipEndPoint = item.IPEndPoint;
                //设置本地的出口ip和端口
                request.ServicePoint.BindIPEndPointDelegate = new BindIPEndPoint(BindIPEndPointCallback);
            }
            //设置Header参数
            if (item.Header != null && item.Header.Count > 0)
            {
                foreach (var key in item.Header.AllKeys)
                {
                    request.Headers.Add(key, item.Header[key]);
                }
            }
            //设置浏览器支持的编码类型
            if (!string.IsNullOrEmpty(item.AcceptEncoding)) request.Headers.Add("Accept-Encoding", item.AcceptEncoding);
            // 设置代理
            SetProxy(item);
            if (item.ProtocolVersion != null) request.ProtocolVersion = item.ProtocolVersion;
            request.ServicePoint.Expect100Continue = item.Expect100Continue;
            //请求方式Get或者Post
            request.Method = item.Method;
            request.Timeout = item.Timeout;
            request.KeepAlive = item.KeepAlive;
            request.ReadWriteTimeout = item.ReadWriteTimeout;
            if (!string.IsNullOrEmpty(item.Host)) request.Host = item.Host;
            if (item.IfModifiedSince != null) request.IfModifiedSince = Convert.ToDateTime(item.IfModifiedSince);
            //Accept
            request.Accept = item.Accept;
            //ContentType返回类型
            request.ContentType = item.ContentType;
            //UserAgent客户端的访问类型，包括浏览器版本和操作系统信息
            request.UserAgent = item.UserAgent;
            //设置安全凭证
            request.Credentials = item.ICredentials;
            //设置Cookie
            SetCookie(item);
            //来源地址
            request.Referer = item.Referer;
            //是否执行跳转功能
            request.AllowAutoRedirect = item.AllowAutoRedirect;
            if (item.MaximumAutomaticRedirections > 0) request.MaximumAutomaticRedirections = item.MaximumAutomaticRedirections;
            //设置Post数据
            SetPostData(item);
            //设置最大连接
            if (item.ConnectionLimit > 0) request.ServicePoint.ConnectionLimit = item.ConnectionLimit;
        }

        /// <summary>
        /// 设置编码
        /// </summary>
        /// <param name="item">http参数</param>
        /// <param name="result">http返回参数</param>
        /// <param name="ResponseByte">返回的字节码数组</param>
        private void SetEncoding(HttpItem item, HttpResult result, byte[] ResponseByte)
        {
            //是否返回byte类型数据
            if (item.ResultType == ResultType.Byte) result.ResultByte = ResponseByte;
            //返回数据编码
            responseEncoding = item.ResponseEncoding;
            //自动识别返回数据编码
            if (responseEncoding == null)
            {
                var meta = Regex.Match(Encoding.Default.GetString(ResponseByte), "<meta[^<]*charset=([^<]*)[\"']", RegexOptions.IgnoreCase);
                string c = string.Empty;
                if (meta != null && meta.Groups.Count > 0)
                {
                    c = meta.Groups[1].Value.ToLower().Trim();
                }
                if (c.Length > 2)
                {
                    try
                    {
                        responseEncoding = Encoding.GetEncoding(c.Replace("\"", string.Empty).Replace("'", "").Replace(";", "").Replace("iso-8859-1", "gbk").Trim());
                    }
                    catch
                    {
                        if (string.IsNullOrEmpty(response.CharacterSet))
                        {
                            responseEncoding = Encoding.UTF8;
                        }
                        else
                        {
                            responseEncoding = Encoding.GetEncoding(response.CharacterSet);
                        }
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(response.CharacterSet))
                    {
                        responseEncoding = Encoding.UTF8;
                    }
                    else
                    {
                        responseEncoding = Encoding.GetEncoding(response.CharacterSet);
                    }
                }
            }
        }

        /// <summary>
        /// 获取返回的byte数据
        /// </summary>
        /// <param name="item">http参数</param>
        /// <param name="result">http返回参数</param>
        /// <returns>byte[]</returns>
        private byte[] GetByte(HttpItem item, HttpResult result)
        {
            byte[] ResponseByte = null;
            var responseStream = response.GetResponseStream();
            //解压缩
            if (response.ContentEncoding.ToLower().Contains("gzip"))
            {
                responseStream = new GZipStream(responseStream, CompressionMode.Decompress);
            }
            else if (response.ContentEncoding.ToLower().Contains("deflate"))
            {
                responseStream = new DeflateStream(responseStream, CompressionMode.Decompress);
            }
            if (item.ResultType != ResultType.File)
            {
                using (var ms = new MemoryStream())
                {
                    responseStream.CopyTo(ms, 10240);
                    ResponseByte = ms.ToArray();
                }
            }
            else if (!string.IsNullOrEmpty(item.DownloadSaveFileUrl))
            {
                //判断文件目录是否存在
                var dirPath = Path.GetDirectoryName(item.DownloadSaveFileUrl);
                if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
                using (var fs = File.Create(item.DownloadSaveFileUrl))
                {
                    var buffer = new byte[1024];
                    var bytesRead = 0;
                    //每次读取1kb数据，然后写入文件
                    while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        fs.Write(buffer, 0, bytesRead);
                    }
                    //赋值返回文件路径
                    result.ResultFileUrl = item.DownloadSaveFileUrl;
                }
            }
            responseStream.Close();
            return ResponseByte;
        }

        /// <summary>
        /// 获取数据的并解析的方法
        /// </summary>
        /// <param name="item">http参数</param>
        /// <param name="result">http返回参数</param>
        private void GetData(HttpItem item, HttpResult result)
        {
            if (response == null) return;
            //获取StatusCode
            result.StatusCode = response.StatusCode;
            //获取StatusDescription
            result.StatusDescription = response.StatusDescription;
            //获取Headers
            result.Header = response.Headers;
            //获取最后访问的URl
            result.ResponseUri = response.ResponseUri.ToString();
            //获取CookieCollection
            if (response.Cookies != null && response.Cookies.Count > 0)
            {
                result.CookieCollection = response.Cookies;
            }
            else if (item.CookieCollection != null && item.CookieCollection.Count > 0)
            {
                result.CookieCollection = request.CookieContainer.GetCookies(response.ResponseUri);
            }
            //获取set-cookie
            if (!string.IsNullOrEmpty(response.Headers["set-cookie"])) result.Cookie = response.Headers["set-cookie"];
            //处理网页byte
            var ResponseByte = GetByte(item, result);
            if (ResponseByte != null && ResponseByte.Length > 0)
            {
                //设置编码
                SetEncoding(item, result, ResponseByte);
                //得到返回的HTML
                result.ResultString = responseEncoding.GetString(ResponseByte);
            }
        }
        #endregion

        #region 公有方法
        /// <summary>
        /// 根据相传入的数据，得到相应页面数据
        /// </summary>
        /// <param name="item">http参数</param>
        /// <returns>返回HttpResult类型</returns>
        public HttpResult GetResult(HttpItem item)
        {
            //返回参数
            var result = new HttpResult();
            try
            {
                //准备参数
                SetRequest(item);
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
                    GetData(item, result);
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    using (response = ex.Response as HttpWebResponse)
                    {
                        GetData(item, result);
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
            if (item.IsToLower) result.ResultString = result.ResultString.ToLower();
            //重置request，response为空
            if (item.IsReset)
            {
                request = null;
                response = null;
            }
            return result;
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
    public class HttpItem
    {
        /// <summary>
        /// 请求URL必须填写
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 请求方式，默认：GET方式
        /// </summary>
        public string Method { get; set; } = "GET";

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
        /// 请求返回类型，默认：text/html
        /// </summary>
        public string ContentType { get; set; } = "text/html";

        /// <summary>
        /// 浏览器类型，默认：Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/62.0.3202.62 Safari/537.36
        /// </summary>
        public string UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/62.0.3202.62 Safari/537.36";

        /// <summary>
        /// Post的数据类型(String/Byte/File)，默认：string
        /// </summary>
        public PostDataType PostDataType { get; set; } = PostDataType.String;

        /// <summary>
        /// 设置或获取Post参数编码，默认：UTF8
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
        /// Cookie对象集合
        /// </summary>
        public CookieCollection CookieCollection { get; set; }

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
        ///item.IPEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.1"),80);
        /// </example>
        public IPEndPoint IPEndPoint { get; set; } = null;

        /// <summary>
        /// 是否重置request、response的值，默认不重置，当设置为True时request、response将被设置为Null
        /// </summary>
        public bool IsReset { get; set; } = false;

        /// <summary>
        /// 当为下载文件操作时的文件保存路径
        /// </summary>
        public string DownloadSaveFileUrl { get; set; }

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
        /// 返回的string类型数据，ResultType.String或者ResultType.Byte返回数据，其它情况为空
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
        /// 返回状态码,默认为OK
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// 最后访问的URl
        /// </summary>
        public string ResponseUri { get; set; }

        /// <summary>
        /// 获取重定向的URl
        /// </summary>
        public string RedirectUrl
        {
            get
            {
                try
                {
                    if (Header != null && Header.Count > 0)
                    {
                        if (Header.AllKeys.Any(k => k.ToLower().Contains("location")))
                        {
                            string baseurl = Header["location"].ToString().Trim();
                            string locationurl = baseurl.ToLower();
                            if (!string.IsNullOrEmpty(locationurl))
                            {
                                bool b = locationurl.StartsWith("http://") || locationurl.StartsWith("https://");
                                if (!b) baseurl = new Uri(new Uri(ResponseUri), baseurl).AbsoluteUri;
                            }
                            return baseurl;
                        }
                    }
                }
                catch { }
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
        /// 表示返回字符串和字节流，ResultByte和ResultString都有数据
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
