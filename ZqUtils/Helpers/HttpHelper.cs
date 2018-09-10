using System;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using ZqUtils.Extensions;
/****************************
* [Author] 张强
* [Date] 2015-10-26
* [Describe] Http工具类
* **************************/
namespace ZqUtils.Helpers
{
    /// <summary>
    /// Http工具类
    /// </summary>
    public class HttpHelper
    {
        #region 重定向执行进程
        /// <summary>
        /// 重定向执行进程
        /// </summary>
        /// <param name="p"></param>
        /// <param name="exe"></param>
        /// <param name="arg"></param>
        /// <param name="output"></param>
        public static void RedirectExcuteProcess(Process p, string exe, string arg, DataReceivedEventHandler output)
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
        #endregion

        #region https安全验证
        /// <summary>
        /// https安全验证
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="certificate">X509证书</param>
        /// <param name="chain">X509约束</param>
        /// <param name="errors">ssl策略错误</param>
        /// <returns>bool</returns>
        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }
        #endregion

        #region post请求
        /// <summary>
        /// post请求
        /// </summary>
        /// <param name="url">post请求地址</param>
        /// <param name="data">post请求参数</param>
        /// <param name="encoding">编码方式</param>
        /// <param name="sslCertPath">证书路径</param>
        /// <param name="sslPassword">证书密码</param>
        /// <param name="cookies">请求cookies</param>
        /// <param name="timeOut">超时时长</param>
        /// <param name="headers">请求头部</param>
        /// <returns>string</returns>
        public static string Post(string url, string data, string encoding = "utf-8", string sslCertPath = "", string sslPassword = "", CookieCollection cookies = null, int timeOut = 0, Dictionary<string, string> headers = null)
        {
            var result = string.Empty;
            try
            {
                GC.Collect();
                var postBytes = Encoding.UTF8.GetBytes(data);
                ServicePointManager.DefaultConnectionLimit = 200;
                if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase)) ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                var request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "POST";
                request.Headers.Add("Accept-Encoding", "gzip,deflate");
                if (headers != null && headers.Count > 0)
                {
                    foreach (var item in headers)
                    {
                        if (!string.IsNullOrEmpty(item.Key) && !string.IsNullOrEmpty(item.Value)) request.Headers.Add(item.Key, item.Value);
                    }
                }
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = postBytes.Length;
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36";
                request.ServicePoint.Expect100Continue = false;
                request.ProtocolVersion = HttpVersion.Version10;
                request.KeepAlive = true;
                if (timeOut > 0) request.Timeout = timeOut;
                if (!sslCertPath.IsNull() && !sslPassword.IsNull())
                {
                    var cert = new X509Certificate2(sslCertPath, sslPassword);
                    request.ClientCertificates.Add(cert);
                }
                var cc = new CookieContainer();
                if (cookies != null)
                {
                    cc.Add(cookies);
                    request.CookieContainer = cc;
                }
                var requestStream = request.GetRequestStream();
                requestStream.Write(postBytes, 0, postBytes.Length);
                if (requestStream != null)
                {
                    requestStream.Close();
                    requestStream.Dispose();
                }
                if (request.GetResponse() is HttpWebResponse response && response.StatusCode == HttpStatusCode.OK)
                {
                    if (cookies != null) response.Cookies = cc.GetCookies(response.ResponseUri);
                    var responseStream = response.GetResponseStream();
                    if (response.ContentEncoding.ToLower().Contains("gzip"))
                    {
                        responseStream = new GZipStream(responseStream, CompressionMode.Decompress);
                    }
                    else if (response.ContentEncoding.ToLower().Contains("deflate"))
                    {
                        responseStream = new DeflateStream(responseStream, CompressionMode.Decompress);
                    }
                    var sr = new StreamReader(responseStream, Encoding.GetEncoding(encoding));
                    result = sr.ReadToEnd();
                    if (sr != null)
                    {
                        sr.Close();
                        sr.Dispose();
                    }
                    if (responseStream != null)
                    {
                        responseStream.Close();
                        responseStream.Dispose();
                    }
                    if (response != null)
                    {
                        response.Close();
                        response.Dispose();
                    }
                }
                request.Abort();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "post请求");
            }
            return result;
        }

        /// <summary>
        /// post请求
        /// </summary>
        /// <param name="url">post请求地址</param>
        /// <param name="data">post请求参数</param>
        /// <param name="encoding">编码方式</param>
        /// <param name="sslCertPath">证书路径</param>
        /// <param name="sslPassword">证书密码</param>
        /// <param name="cookies">请求cookies</param>
        /// <param name="timeOut">超时时长</param>
        /// <param name="headers">请求头部</param>
        /// <returns>string</returns>
        public static string Post(string url, IDictionary<string, string> data, string encoding = "utf-8", string sslCertPath = "", string sslPassword = "", CookieCollection cookies = null, int timeOut = 0, Dictionary<string, string> headers = null)
        {
            return Post(url, data.ToUrl(), encoding, sslCertPath, sslPassword, cookies, timeOut, headers);
        }

        /// <summary>
        /// post请求
        /// </summary>
        /// <param name="url">post请求地址</param>
        /// <param name="data">post请求参数</param>
        /// <param name="sslCertPath">证书路径</param>
        /// <param name="sslPassword">证书密码</param>
        /// <param name="cookies">请求cookies</param>
        /// <param name="timeOut">超时时长</param>
        /// <param name="headers">请求头部</param>
        /// <returns>HttpWebResponse</returns>
        public static HttpWebResponse WebPost(string url, string data, string sslCertPath = "", string sslPassword = "", CookieCollection cookies = null, int timeOut = 0, Dictionary<string, string> headers = null)
        {
            HttpWebResponse response = null;
            try
            {
                GC.Collect();
                var postBytes = Encoding.UTF8.GetBytes(data);
                ServicePointManager.DefaultConnectionLimit = 200;
                if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase)) ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                var request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "POST";
                request.Headers.Add("Accept-Encoding", "gzip,deflate");
                if (headers != null && headers.Count > 0)
                {
                    foreach (var item in headers)
                    {
                        if (!string.IsNullOrEmpty(item.Key) && !string.IsNullOrEmpty(item.Value)) request.Headers.Add(item.Key, item.Value);
                    }
                }
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = postBytes.Length;
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36";
                request.ServicePoint.Expect100Continue = false;
                request.ProtocolVersion = HttpVersion.Version10;
                request.KeepAlive = true;
                if (timeOut > 0) request.Timeout = timeOut;
                if (sslCertPath.IsNull() == false && sslPassword.IsNull() == false)
                {
                    var cert = new X509Certificate2(sslCertPath, sslPassword);
                    request.ClientCertificates.Add(cert);
                }
                var cc = new CookieContainer();
                if (cookies != null)
                {
                    cc.Add(cookies);
                    request.CookieContainer = cc;
                }
                var requestStream = request.GetRequestStream();
                requestStream.Write(postBytes, 0, postBytes.Length);
                if (requestStream != null)
                {
                    requestStream.Close();
                    requestStream.Dispose();
                }
                response = request.GetResponse() as HttpWebResponse;
                if (response != null && response.StatusCode == HttpStatusCode.OK && cookies != null)
                {
                    response.Cookies = cc.GetCookies(response.ResponseUri);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "post请求");
            }
            return response;
        }

        /// <summary>
        /// post请求
        /// </summary>
        /// <param name="url">post请求地址</param>
        /// <param name="data">post请求参数</param>
        /// <param name="sslCertPath">证书路径</param>
        /// <param name="sslPassword">证书密码</param>
        /// <param name="cookies">请求cookies</param>
        /// <param name="timeOut">超时时长</param>
        /// <param name="headers">请求头部</param>
        /// <returns>HttpWebResponse</returns>
        public static HttpWebResponse WebPost(string url, IDictionary<string, string> data, string sslCertPath = "", string sslPassword = "", CookieCollection cookies = null, int timeOut = 0, Dictionary<string, string> headers = null)
        {
            return WebPost(url, data.ToUrl(), sslCertPath, sslPassword, cookies, timeOut, headers);
        }
        #endregion

        #region get请求
        /// <summary>
        /// get请求
        /// </summary>
        /// <param name="url">get请求地址</param>
        /// <param name="encoding">编码方式</param>
        /// <param name="cookies">请求cookies</param>
        /// <param name="timeOut">超时时长</param>
        /// <param name="headers">请求头部</param>
        /// <returns>string</returns>
        public static string Get(string url, string encoding = "utf-8", CookieCollection cookies = null, int timeOut = 0, Dictionary<string, string> headers = null)
        {
            var result = string.Empty;
            try
            {
                GC.Collect();
                ServicePointManager.DefaultConnectionLimit = 200;
                if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase)) ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                var request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "GET";
                request.Headers.Add("Accept-Encoding", "gzip,deflate");
                if (headers != null && headers.Count > 0)
                {
                    foreach (var item in headers)
                    {
                        if (!string.IsNullOrEmpty(item.Key) && !string.IsNullOrEmpty(item.Value)) request.Headers.Add(item.Key, item.Value);
                    }
                }
                request.ContentType = "application/x-www-form-urlencoded";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36";
                request.ServicePoint.Expect100Continue = false;
                request.ProtocolVersion = HttpVersion.Version10;
                request.KeepAlive = true;
                if (timeOut > 0) request.Timeout = timeOut;
                var cc = new CookieContainer();
                if (cookies != null)
                {
                    cc.Add(cookies);
                    request.CookieContainer = cc;
                }
                if (request.GetResponse() is HttpWebResponse response && response.StatusCode == HttpStatusCode.OK)
                {
                    if (cookies != null) response.Cookies = cc.GetCookies(response.ResponseUri);
                    var responseStream = response.GetResponseStream();
                    if (response.ContentEncoding.ToLower().Contains("gzip"))
                    {
                        responseStream = new GZipStream(responseStream, CompressionMode.Decompress);
                    }
                    else if (response.ContentEncoding.ToLower().Contains("deflate"))
                    {
                        responseStream = new DeflateStream(responseStream, CompressionMode.Decompress);
                    }
                    var sr = new StreamReader(responseStream, Encoding.GetEncoding(encoding));
                    result = sr.ReadToEnd();
                    if (sr != null)
                    {
                        sr.Close();
                        sr.Dispose();
                    }
                    if (responseStream != null)
                    {
                        responseStream.Close();
                        responseStream.Dispose();
                    }
                    if (response != null)
                    {
                        response.Close();
                        response.Dispose();
                    }
                }
                request.Abort();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "get请求");
            }
            return result;
        }

        /// <summary>
        /// get请求
        /// </summary>
        /// <param name="url">get请求地址</param>
        /// <param name="data">字典型请求参数</param>
        /// <param name="encoding">编码方式</param>
        /// <param name="cookies">请求cookies</param>
        /// <param name="timeOut">超时时长</param>
        /// <param name="headers">请求头部</param>
        /// <returns>string</returns>
        public static string Get(string url, IDictionary<string, string> data, string encoding = "utf-8", CookieCollection cookies = null, int timeOut = 0, Dictionary<string, string> headers = null)
        {
            return Get((url.Contains("?") ? url : url + "?") + data.ToUrl(), encoding, cookies, timeOut, headers);
        }

        /// <summary>
        /// get请求
        /// </summary>
        /// <param name="url">get请求地址</param>
        /// <param name="cookies">请求cookies</param>
        /// <param name="timeOut">超时时长</param>
        /// <param name="headers">请求头部</param>
        /// <returns>HttpWebResponse</returns>
        public static HttpWebResponse WebGet(string url, CookieCollection cookies = null, int timeOut = 0, Dictionary<string, string> headers = null)
        {
            HttpWebResponse response = null;
            try
            {
                GC.Collect();
                ServicePointManager.DefaultConnectionLimit = 200;
                if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase)) ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                var request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "GET";
                request.Headers.Add("Accept-Encoding", "gzip,deflate");
                if (headers != null && headers.Count > 0)
                {
                    foreach (var item in headers)
                    {
                        if (!string.IsNullOrEmpty(item.Key) && !string.IsNullOrEmpty(item.Value)) request.Headers.Add(item.Key, item.Value);
                    }
                }
                request.ContentType = "application/x-www-form-urlencoded";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36";
                request.ServicePoint.Expect100Continue = false;
                request.ProtocolVersion = HttpVersion.Version10;
                request.KeepAlive = true;
                if (timeOut > 0) request.Timeout = timeOut;
                var cc = new CookieContainer();
                if (cookies != null)
                {
                    cc.Add(cookies);
                    request.CookieContainer = cc;
                }
                response = request.GetResponse() as HttpWebResponse;
                if (response != null && response.StatusCode == HttpStatusCode.OK && cookies != null)
                {
                    response.Cookies = cc.GetCookies(response.ResponseUri);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "get请求");
            }
            return response;
        }

        /// <summary>
        /// get请求
        /// </summary>
        /// <param name="url">get请求地址</param>
        /// <param name="data">字典型请求参数</param>
        /// <param name="cookies">请求cookies</param>
        /// <param name="timeOut">超时时长</param>
        /// <param name="headers">请求头部</param>
        /// <returns>HttpWebResponse</returns>
        public static HttpWebResponse WebGet(string url, IDictionary<string, string> data, CookieCollection cookies = null, int timeOut = 0, Dictionary<string, string> headers = null)
        {
            return WebGet((url.Contains("?") ? url : url + "?") + data.ToUrl(), cookies, timeOut, headers);
        }
        #endregion

        #region http上传文件
        /// <summary>
        /// http单文件上传
        /// </summary>
        /// <param name="url">上传地址</param>
        /// <param name="path">文件路径</param>
        /// <param name="formName">form表单name</param>
        /// <param name="encoding">编码方式</param>
        /// <param name="cookies">请求cookies</param>
        /// <returns>string</returns>
        public static string Upload(string url, string path, string formName = "media", string encoding = "utf-8", CookieCollection cookies = null)
        {
            var result = string.Empty;
            try
            {
                GC.Collect();//垃圾回收，回收没有正常关闭的http连接 
                var boundary = DateTime.Now.Ticks.ToString("x"); // 随机分隔线               
                var itemBoundaryBytes = Encoding.UTF8.GetBytes($"\r\n--{boundary}\r\n");
                var endBoundaryBytes = Encoding.UTF8.GetBytes($"\r\n--{boundary}--\r\n");
                var fileName = path.Substring(path.LastIndexOf("\\") + 1);
                //请求头部信息 
                var sbHeader = new StringBuilder($"Content-Disposition:form-data;name=\"{formName}\";filename=\"{fileName}\"\r\nContent-Type:application/octet-stream\r\n\r\n");
                var postHeaderBytes = Encoding.UTF8.GetBytes(sbHeader.ToString());
                var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                //声明字节数组，大小为文件字节流长度
                var bArr = new byte[fs.Length];
                //将文件流读取到字节数组中                
                fs.Read(bArr, 0, bArr.Length);
                //关闭文件
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
                //请求
                ServicePointManager.DefaultConnectionLimit = 200;
                if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase)) ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                var request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "POST";
                request.Headers.Add("Accept-Encoding", "gzip,deflate");
                request.ContentType = $"multipart/form-data;charset=utf-8;boundary={boundary}";
                request.ContentLength = itemBoundaryBytes.Length + postHeaderBytes.Length + bArr.Length + endBoundaryBytes.Length;
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36";
                request.KeepAlive = true;
                var cc = new CookieContainer();
                if (cookies != null)
                {
                    cc.Add(cookies);
                    request.CookieContainer = cc;
                }
                var requestStream = request.GetRequestStream();
                requestStream.Write(itemBoundaryBytes, 0, itemBoundaryBytes.Length);
                requestStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);
                requestStream.Write(bArr, 0, bArr.Length);
                requestStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);
                if (requestStream != null)
                {
                    requestStream.Close();
                    requestStream.Dispose();
                }
                //发送请求并获取相应回应数据
                if (request.GetResponse() is HttpWebResponse response && response.StatusCode == HttpStatusCode.OK)
                {
                    if (cookies != null) response.Cookies = cc.GetCookies(response.ResponseUri);
                    //直到request.GetResponse()程序才开始向目标网页发送Post请求
                    var responseStream = response.GetResponseStream();
                    if (response.ContentEncoding.ToLower().Contains("gzip"))
                    {
                        responseStream = new GZipStream(responseStream, CompressionMode.Decompress);
                    }
                    else if (response.ContentEncoding.ToLower().Contains("deflate"))
                    {
                        responseStream = new DeflateStream(responseStream, CompressionMode.Decompress);
                    }
                    var sr = new StreamReader(responseStream, Encoding.GetEncoding(encoding));
                    result = sr.ReadToEnd();
                    if (sr != null)
                    {
                        sr.Close();
                        sr.Dispose();
                    }
                    if (responseStream != null)
                    {
                        responseStream.Close();
                        responseStream.Dispose();
                    }
                    if (response != null)
                    {
                        response.Close();
                        response.Dispose();
                    }
                }
                request.Abort();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "http单文件上传");
            }
            return result;
        }

        /// <summary>
        /// http多文件上传
        /// </summary>
        /// <param name="url">上传地址</param>
        /// <param name="files">需要上传的文件，Key：对应要上传的Name，Value：文件路径</param>
        /// <param name="encoding">编码方式</param>
        /// <param name="cookies">请求cookies</param>
        /// <returns>string</returns>
        public static string Upload(string url, Dictionary<string, string> files = null, string encoding = "utf-8", CookieCollection cookies = null)
        {
            var result = string.Empty;
            try
            {
                GC.Collect();
                #region 处理Form表单文件上传
                var postStream = new MemoryStream();
                var boundary = $"----{DateTime.Now.Ticks.ToString("x")}";
                var formdataTemplate = "\r\n--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: application/octet-stream\r\n\r\n";
                foreach (var file in files)
                {
                    var fs = new FileStream(file.Value, FileMode.Open, FileAccess.Read);
                    var formdata = string.Format(formdataTemplate, file.Key, file.Value);
                    var formdataBytes = Encoding.UTF8.GetBytes(postStream.Length == 0 ? formdata.Substring(2, formdata.Length - 2) : formdata);//第一行不需要换行
                    postStream.Write(formdataBytes, 0, formdataBytes.Length);
                    var buffer = new byte[1024];
                    var bytesRead = 0;
                    while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        postStream.Write(buffer, 0, bytesRead);
                    }
                    if (fs != null)
                    {
                        fs.Close();
                        fs.Dispose();
                    }
                }
                //结尾
                var footer = Encoding.UTF8.GetBytes($"\r\n--{boundary}--\r\n");
                postStream.Write(footer, 0, footer.Length);
                #endregion
                ServicePointManager.DefaultConnectionLimit = 200;
                if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                }
                var request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "POST";
                request.Headers.Add("Accept-Encoding", "gzip,deflate");
                request.ContentType = $"multipart/form-data; boundary={boundary}";
                request.ContentLength = postStream != null ? postStream.Length : 0;
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36";
                request.KeepAlive = true;
                var cc = new CookieContainer();
                if (cookies != null)
                {
                    cc.Add(cookies);
                    request.CookieContainer = cc;
                }
                #region 写入二进制流
                if (postStream != null)
                {
                    var requestStream = request.GetRequestStream();
                    var buffer = new byte[1024];
                    var bytesRead = 0;
                    postStream.Position = 0;
                    while ((bytesRead = postStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        requestStream.Write(buffer, 0, bytesRead);
                    }
                    postStream.Close();
                    postStream.Dispose();
                }
                #endregion
                if (request.GetResponse() is HttpWebResponse response && response.StatusCode == HttpStatusCode.OK)
                {
                    if (cookies != null)
                    {
                        response.Cookies = cc.GetCookies(response.ResponseUri);
                    }
                    var responseStream = response.GetResponseStream();
                    if (response.ContentEncoding.ToLower().Contains("gzip"))
                    {
                        responseStream = new GZipStream(responseStream, CompressionMode.Decompress);
                    }
                    else if (response.ContentEncoding.ToLower().Contains("deflate"))
                    {
                        responseStream = new DeflateStream(responseStream, CompressionMode.Decompress);
                    }
                    var sr = new StreamReader(responseStream, Encoding.GetEncoding(encoding));
                    result = sr.ReadToEnd();
                    if (sr != null)
                    {
                        sr.Close();
                        sr.Dispose();
                    }
                    if (responseStream != null)
                    {
                        responseStream.Close();
                        responseStream.Dispose();
                    }
                    if (response != null)
                    {
                        response.Close();
                        response.Dispose();
                    }
                }
                request.Abort();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "http多文件上传");
            }
            return result;
        }

        /// <summary>
        /// http表单和多文件同时上传
        /// </summary>
        /// <param name="url">上传地址</param>
        /// <param name="data">表单数据</param>
        /// <param name="files">需要上传的文件，Key：对应要上传的Name，Value：文件路径</param>
        /// <param name="encoding">编码方式</param>
        /// <param name="cookies">请求cookies</param>
        /// <returns>string</returns>
        public static string Upload(string url, string data, IDictionary<string, string> files, string encoding = "utf-8", CookieCollection cookies = null)
        {
            var result = string.Empty;
            try
            {
                GC.Collect();
                var boundary = $"---------------------------{DateTime.Now.Ticks.ToString("x")}";
                #region post字节流
                #region 表单
                var sb = new StringBuilder();
                //表单数据
                sb.Append($"--{boundary}")
                  .Append("\r\n")
                  .Append("Content-Disposition: form-data; name=\"content\"")
                  .Append("\r\n\r\n")
                  .Append(data)
                  .Append("\r\n");
                //文件数据
                var fileList = files.ToList();
                fileList.ForEach(o =>
                {
                    sb.Append($"--{boundary}")
                      .Append("\r\n")
                      .Append($"Content-Disposition: form-data; name=\"{o.Key}\"; filename=\"{o.Value}\"")
                      .Append("\r\n")
                      .Append("Content-Type: application/octet-stream")
                      .Append("\r\n\r\n");
                });
                //表单
                var formData = Encoding.UTF8.GetBytes(sb.ToString());
                #endregion
                #region 文件
                var fileStream = new MemoryStream();
                fileList.ForEach(o =>
                {
                    var fs = new FileStream(o.Value, FileMode.Open, FileAccess.Read);
                    //写入文件
                    var fileBuffer = new byte[1024];
                    var fileBytesRead = 0;
                    while ((fileBytesRead = fs.Read(fileBuffer, 0, fileBuffer.Length)) != 0)
                    {
                        fileStream.Write(fileBuffer, 0, fileBytesRead);
                    }
                    if (fs != null)
                    {
                        fs.Close();
                        fs.Dispose();
                    }
                });
                #endregion
                //结尾 
                var footData = Encoding.UTF8.GetBytes($"\r\n--{boundary}--\r\n");
                #endregion
                //请求 
                ServicePointManager.DefaultConnectionLimit = 200;
                if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                }
                var request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "POST";
                request.Headers.Add("Accept-Encoding", "gzip,deflate");
                request.ContentType = $"multipart/form-data; boundary={boundary}";
                request.ContentLength = formData.Length + fileStream.Length + footData.Length;
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36";
                request.KeepAlive = true;
                var cc = new CookieContainer();
                if (cookies != null)
                {
                    cc.Add(cookies);
                    request.CookieContainer = cc;
                }
                //请求流
                var requestStream = request.GetRequestStream();
                #region 写入post字节流
                //写入表单数据
                requestStream.Write(formData, 0, formData.Length);
                //写入文件内容
                var buffer = new byte[1024];
                var bytesRead = 0;
                //读取位置初始化
                fileStream.Position = 0;
                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    requestStream.Write(buffer, 0, bytesRead);
                }
                if (fileStream != null)
                {
                    fileStream.Close();
                    fileStream.Dispose();
                }
                //结尾 
                requestStream.Write(footData, 0, footData.Length);
                if (requestStream != null)
                {
                    requestStream.Close();
                    requestStream.Dispose();
                }
                #endregion
                //响应 
                if (request.GetResponse() is HttpWebResponse response && response.StatusCode == HttpStatusCode.OK)
                {
                    if (cookies != null)
                    {
                        response.Cookies = cc.GetCookies(response.ResponseUri);
                    }
                    var responseStream = response.GetResponseStream();
                    if (response.ContentEncoding.ToLower().Contains("gzip"))
                    {
                        responseStream = new GZipStream(responseStream, CompressionMode.Decompress);
                    }
                    else if (response.ContentEncoding.ToLower().Contains("deflate"))
                    {
                        responseStream = new DeflateStream(responseStream, CompressionMode.Decompress);
                    }
                    var sr = new StreamReader(responseStream, Encoding.GetEncoding(encoding));
                    result = sr.ReadToEnd();
                    if (sr != null)
                    {
                        sr.Close();
                        sr.Dispose();
                    }
                    if (responseStream != null)
                    {
                        responseStream.Close();
                        responseStream.Dispose();
                    }
                    if (response != null)
                    {
                        response.Close();
                        response.Dispose();
                    }
                }
                request.Abort();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "http表单和多文件同时上传");
            }
            return result;
        }

        /// <summary>
        /// http表单和多文件同时上传
        /// </summary>
        /// <param name="url">上传地址</param>
        /// <param name="data">表单数据字典集合</param>
        /// <param name="files">需要上传的文件，Key：对应要上传的Name，Value：文件路径</param>
        /// <param name="encoding">编码方式</param>
        /// <param name="cookies">请求cookies</param>
        /// <returns>string</returns>
        public static string Upload(string url, IDictionary<string, string> data, IDictionary<string, string> files, string encoding = "utf-8", CookieCollection cookies = null)
        {
            var result = string.Empty;
            try
            {
                GC.Collect();
                var boundary = $"---------------------------{DateTime.Now.Ticks.ToString("x")}";
                #region post字节流
                #region 表单
                var sb = new StringBuilder();
                //表单数据
                data.ToList().ForEach(o =>
                {
                    sb.Append($"--{boundary}")
                      .Append("\r\n")
                      .Append($"Content-Disposition: form-data; name=\"{o.Key}\"")
                      .Append("\r\n\r\n")
                      .Append(o.Value)
                      .Append("\r\n");
                });
                //文件数据
                var fileList = files.ToList();
                fileList.ForEach(o =>
                {
                    sb.Append($"--{boundary}")
                      .Append("\r\n")
                      .Append($"Content-Disposition: form-data; name=\"{o.Key}\"; filename=\"{o.Value}\"")
                      .Append("\r\n")
                      .Append("Content-Type: application/octet-stream")
                      .Append("\r\n\r\n");
                });
                //表单
                var formData = Encoding.UTF8.GetBytes(sb.ToString());
                #endregion
                #region 文件
                var fileStream = new MemoryStream();
                fileList.ForEach(o =>
                {
                    var fs = new FileStream(o.Value, FileMode.Open, FileAccess.Read);
                    //写入文件
                    var fileBuffer = new byte[1024];
                    var fileBytesRead = 0;
                    while ((fileBytesRead = fs.Read(fileBuffer, 0, fileBuffer.Length)) != 0)
                    {
                        fileStream.Write(fileBuffer, 0, fileBytesRead);
                    }
                    if (fs != null)
                    {
                        fs.Close();
                        fs.Dispose();
                    }
                });
                #endregion
                //结尾 
                var footData = Encoding.UTF8.GetBytes($"\r\n--{boundary}--\r\n");
                #endregion
                //请求
                ServicePointManager.DefaultConnectionLimit = 200;
                if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                }
                var request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "POST";
                request.Headers.Add("Accept-Encoding", "gzip,deflate");
                request.ContentType = $"multipart/form-data; boundary={boundary}";
                request.ContentLength = formData.Length + fileStream.Length + footData.Length;
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36";
                request.KeepAlive = true;
                var cc = new CookieContainer();
                if (cookies != null)
                {
                    cc.Add(cookies);
                    request.CookieContainer = cc;
                }
                //请求流
                var requestStream = request.GetRequestStream();
                #region 写入post字节流
                //写入表单数据
                requestStream.Write(formData, 0, formData.Length);
                //写入文件内容
                var buffer = new byte[1024];
                var bytesRead = 0;
                //读取位置初始化
                fileStream.Position = 0;
                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    requestStream.Write(buffer, 0, bytesRead);
                }
                if (fileStream != null)
                {
                    fileStream.Close();
                    fileStream.Dispose();
                }
                //结尾 
                requestStream.Write(footData, 0, footData.Length);
                if (requestStream != null)
                {
                    requestStream.Close();
                    requestStream.Dispose();
                }
                #endregion
                //响应 
                if (request.GetResponse() is HttpWebResponse response && response.StatusCode == HttpStatusCode.OK)
                {
                    if (cookies != null)
                    {
                        response.Cookies = cc.GetCookies(response.ResponseUri);
                    }
                    var responseStream = response.GetResponseStream();
                    if (response.ContentEncoding.ToLower().Contains("gzip"))
                    {
                        responseStream = new GZipStream(responseStream, CompressionMode.Decompress);
                    }
                    else if (response.ContentEncoding.ToLower().Contains("deflate"))
                    {
                        responseStream = new DeflateStream(responseStream, CompressionMode.Decompress);
                    }
                    var sr = new StreamReader(responseStream, Encoding.GetEncoding(encoding));
                    result = sr.ReadToEnd();
                    if (sr != null)
                    {
                        sr.Close();
                        sr.Dispose();
                    }
                    if (responseStream != null)
                    {
                        responseStream.Close();
                        responseStream.Dispose();
                    }
                    if (response != null)
                    {
                        response.Close();
                        response.Dispose();
                    }
                }
                request.Abort();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "http表单和多文件同时上传");
            }
            return result;
        }
        #endregion

        #region http下载文件
        /// <summary>
        /// http get方式下载任意类型文件
        /// </summary>
        /// <param name="url">文件http路径</param>
        /// <param name="saveDir">保存文件目录</param>
        /// <param name="fileExt">文件扩展名(为空时，文件保存类型根据url进行判断)</param>
        /// <param name="fileName">文件名，不包含扩展名，可为空</param>
        /// <param name="cookies">请求cookies</param>
        public static void Download(string url, string saveDir = @"Files\", string fileExt = "", string fileName = "", CookieCollection cookies = null)
        {
            try
            {
                GC.Collect();
                #region 初始化HttpWebRequest，并发出请求
                if (fileName.IsNull())
                {
                    fileName = $"{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.{url.Substring(".")}";//文件名
                    if (!fileExt.IsNull()) fileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + fileExt;
                }
                else
                {
                    fileName = fileName + fileExt;
                }
                saveDir = AppDomain.CurrentDomain.BaseDirectory + saveDir;//文件目录
                if (!Directory.Exists(saveDir)) Directory.CreateDirectory(saveDir);
                var savePath = saveDir + fileName;
                ServicePointManager.DefaultConnectionLimit = 200;
                if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase)) ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                var request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "GET";
                request.Headers.Add("Accept-Encoding", "gzip,deflate");
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36";
                request.Referer = url.Substring(0, url.LastIndexOf("/") + 1);
                request.ContentType = "application/octet-stream";
                request.KeepAlive = true;
                var cc = new CookieContainer();
                if (cookies != null)
                {
                    cc.Add(cookies);
                    request.CookieContainer = cc;
                }
                #endregion

                #region 获取请求响应流，并写入文件中
                if (request.GetResponse() is HttpWebResponse response && response.StatusCode == HttpStatusCode.OK)
                {
                    if (cookies != null) response.Cookies = cc.GetCookies(response.ResponseUri);
                    var responseStream = response.GetResponseStream();
                    if (response.ContentEncoding.ToLower().Contains("gzip"))
                    {
                        responseStream = new GZipStream(responseStream, CompressionMode.Decompress);
                    }
                    else if (response.ContentEncoding.ToLower().Contains("deflate"))
                    {
                        responseStream = new DeflateStream(responseStream, CompressionMode.Decompress);
                    }
                    var fs = File.Create(savePath);
                    var buffer = new byte[1024];
                    var bytesRead = 0;
                    //每次读取1kb数据，然后写入文件
                    while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        fs.Write(buffer, 0, bytesRead);
                    }
                    if (fs != null)
                    {
                        fs.Close();
                        fs.Dispose();
                    }
                    if (responseStream != null)
                    {
                        responseStream.Close();
                        responseStream.Dispose();
                    }
                    if (response != null)
                    {
                        response.Close();
                        response.Dispose();
                    }
                }
                request.Abort();
                #endregion
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "http get方式下载任意类型文件");
            }
        }

        /// <summary>
        /// http post方式下载任意类型文件
        /// </summary>
        /// <param name="url">文件http路径</param>
        /// <param name="data">post数据</param>
        /// <param name="saveDir">保存文件目录</param>
        /// <param name="fileExt">文件扩展名(为空时，文件保存类型根据url进行判断)</param>
        /// <param name="fileName">文件名，不包含扩展名，可为空</param>
        /// <param name="cookies">请求cookies</param>
        public static void Download(string url, string data, string saveDir = @"Files\", string fileExt = "", string fileName = "", CookieCollection cookies = null)
        {
            try
            {
                GC.Collect();
                #region 初始化HttpWebRequest，并发出请求
                if (fileName.IsNull())
                {
                    fileName = $"{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.{url.Substring(".")}";//文件名
                    if (!fileExt.IsNull()) fileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + fileExt;
                }
                else
                {
                    fileName = fileName + fileExt;
                }
                saveDir = AppDomain.CurrentDomain.BaseDirectory + saveDir;//文件目录
                if (!Directory.Exists(saveDir)) Directory.CreateDirectory(saveDir);
                var savePath = saveDir + fileName;
                var postBytes = Encoding.UTF8.GetBytes(data); //转化
                ServicePointManager.DefaultConnectionLimit = 200;
                if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase)) ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                var request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "POST";
                request.Headers.Add("Accept-Encoding", "gzip,deflate");
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36";
                request.ContentType = "application/octet-stream";
                request.ContentLength = postBytes.Length;
                request.Referer = url.Substring(0, url.LastIndexOf("/") + 1);
                request.KeepAlive = true;
                var cc = new CookieContainer();
                if (cookies != null)
                {
                    cc.Add(cookies);
                    request.CookieContainer = cc;
                }
                var requestStream = request.GetRequestStream();
                requestStream.Write(postBytes, 0, postBytes.Length);//写入参数
                if (requestStream != null)
                {
                    requestStream.Close();
                    requestStream.Dispose();
                }
                #endregion

                #region 获取请求响应流，并写入文件中
                if (request.GetResponse() is HttpWebResponse response && response.StatusCode == HttpStatusCode.OK)
                {
                    if (cookies != null) response.Cookies = cc.GetCookies(response.ResponseUri);
                    var responseStream = response.GetResponseStream();
                    if (response.ContentEncoding.ToLower().Contains("gzip"))
                    {
                        responseStream = new GZipStream(responseStream, CompressionMode.Decompress);
                    }
                    else if (response.ContentEncoding.ToLower().Contains("deflate"))
                    {
                        responseStream = new DeflateStream(responseStream, CompressionMode.Decompress);
                    }
                    var fs = File.Create(savePath);
                    var buffer = new byte[1024];
                    var bytesRead = 0;
                    //每次读取1kb数据，然后写入文件
                    while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        fs.Write(buffer, 0, bytesRead);
                    }
                    if (fs != null)
                    {
                        fs.Close();
                        fs.Dispose();
                    }
                    if (responseStream != null)
                    {
                        responseStream.Close();
                        responseStream.Dispose();
                    }
                    if (response != null)
                    {
                        response.Close();
                        response.Dispose();
                    }
                }
                request.Abort();
                #endregion
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "http post方式下载任意类型文件");
            }
        }

        /// <summary>
        /// Aria2下载文件
        /// </summary>
        /// <param name="url">要下载的文件url</param>
        /// <param name="fileSavePath">本地文件保存路径</param>
        /// <param name="toolPath">aria2c.exe可执行文件路径</param>
        /// <param name="action">文件下载进度委托</param>
        /// <returns>是否下载成功</returns>
        public static bool DownloadByAria2(string url, string fileSavePath, string toolPath, Action<string> action)
        {
            var result = true;
            try
            {
                var fi = new FileInfo(fileSavePath);
                var command = " -c -s 5 --check-certificate=false -d " + fi.DirectoryName + " -o " + fi.Name + " " + url;
                using (var p = new Process())
                {
                    RedirectExcuteProcess(p, toolPath, command, (s, e) => action(e.Data));
                }
            }
            catch (Exception ex)
            {
                result = false;
                LogHelper.Error(ex, "Aria2下载文件异常");
            }
            return result;
        }
        #endregion

        #region HttpWebResponse读取
        /// <summary>
        /// HttpWebResponse读取到字符串
        /// </summary>
        /// <param name="response">HttpWebResponse</param>
        /// <param name="encoding">编码格式</param>
        /// <returns>string</returns>
        public static string HttpWebResponseToString(HttpWebResponse response, string encoding = "utf-8")
        {
            if (response != null && response.StatusCode == HttpStatusCode.OK)
            {
                var responseStream = response.GetResponseStream();
                if (response.ContentEncoding.ToLower().Contains("gzip"))
                {
                    responseStream = new GZipStream(responseStream, CompressionMode.Decompress);
                }
                else if (response.ContentEncoding.ToLower().Contains("deflate"))
                {
                    responseStream = new DeflateStream(responseStream, CompressionMode.Decompress);
                }
                var sr = new StreamReader(responseStream, Encoding.GetEncoding(encoding));
                var s = sr.ReadToEnd();
                if (sr != null)
                {
                    sr.Close();
                    sr.Dispose();
                }
                if (responseStream != null)
                {
                    responseStream.Close();
                    responseStream.Dispose();
                }
                return s;
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region 构建自动提交的支付表单页面的HTML
        /// <summary>
        /// 构建自动提交的支付表单页面的HTML
        /// </summary>
        /// <param name="formName">表单名称</param>
        /// <param name="actionUrl">地址</param>
        /// <param name="formType">方式(get;post)</param>
        /// <param name="keyValues">键值对数据</param>
        /// <returns></returns>
        public static string BuildForm(string formName, string actionUrl, string formType, IDictionary<string, string> keyValues)
        {
            var sb = new StringBuilder();
            sb.Append($"<form id=\"{formName}\" name=\"{formName}\" action=\"{actionUrl}\" method=\"{formType}\">");
            foreach (KeyValuePair<string, string> kp in keyValues)
            {
                sb.Append($"<input type=\"hidden\" name=\"{kp.Key}\"  id=\"{kp.Key}\" value=\"{kp.Value}\"  />");
            }
            sb.Append("</form>");
            sb.Append($"<script>document.forms['{formName}'].submit();</script>");
            return sb.ToString();
        }
        #endregion        
    }
}
