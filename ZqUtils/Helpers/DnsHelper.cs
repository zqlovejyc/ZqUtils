using DnsClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Web;
/****************************
* [Author] 张强
* [Date] 2018-08-24
* [Describe] Dns工具类
* **************************/
namespace ZqUtils.Helpers
{
    /// <summary>
    /// Dns工具类
    /// </summary>
    public class DnsHelper
    {
        #region GetIpAddressAsync
        /// <summary>
        /// 获取本地的IP地址
        /// </summary>
        /// <param name="ipv4">是否ipv4</param>
        /// <returns></returns>
        public static async Task<string> GetIpAddressAsync(bool ipv4 = true)
        {
            var client = new LookupClient();
            var hostEntry = await client.GetHostEntryAsync(Dns.GetHostName());
            IPAddress ipAddress = null;
            if (ipv4)
            {
                ipAddress = hostEntry
                                .AddressList
                                .Where(ip => !IPAddress.IsLoopback(ip) && ip.AddressFamily == AddressFamily.InterNetwork)
                                .FirstOrDefault();
            }
            else
            {
                ipAddress = hostEntry
                                .AddressList
                                .Where(ip => !IPAddress.IsLoopback(ip) && ip.AddressFamily == AddressFamily.InterNetworkV6)
                                .FirstOrDefault();
            }
            return ipAddress?.ToString();
        }

        /// <summary>
        /// 根据域名获取对应的IP地址
        /// </summary>
        /// <param name="domain">域名</param>
        /// <param name="type">请求类型</param>
        /// <returns></returns>
        public static async Task<string> GetIpAddressAsync(string domain, QueryType type = QueryType.ANY)
        {
            var lookup = new LookupClient();
            var result = await lookup.QueryAsync(domain, type);
            var record = result.Answers.ARecords().FirstOrDefault();
            return record?.Address?.ToString();
        }
        #endregion

        #region GetClientIp
        /// <summary>
        /// 获取客户端IP
        /// </summary>
        /// <returns>string</returns>
        public static string GetClientIp()
        {
            var ip = string.Empty;
            try
            {
                string[] temp;
                var isErr = false;
                var request = HttpContext.Current.Request;
                if (request.ServerVariables["HTTP_X_ForWARDED_For"] == null)
                {
                    ip = request.ServerVariables["REMOTE_ADDR"].ToString();
                }
                else
                {
                    ip = request.ServerVariables["HTTP_X_ForWARDED_For"].ToString();
                }
                if (ip.Length > 15)
                {
                    isErr = true;
                }
                else
                {
                    temp = ip.Split('.');
                    if (temp.Length == 4)
                    {
                        for (int i = 0; i < temp.Length; i++)
                        {
                            if (temp[i].Length > 3) isErr = true;
                        }
                    }
                    else
                    {
                        isErr = true;
                    }
                }
                if (isErr) ip = "1.1.1.1";
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "获取客户端IP");
            }
            return ip;
        }
        #endregion

        #region GetClientInfo
        /// <summary>
        /// 获取客户端信息
        /// </summary>
        /// <returns>Dictionary</returns>
        public static Dictionary<string, string> GetClientInfo()
        {
            try
            {
                var request = HttpContext.Current.Request;
                //C#6.0字典初始化器
                return new Dictionary<string, string>
                {
                    ["userAgent"] = request.UserAgent,
                    ["userHostName"] = request.UserHostName,
                    ["userHostAddress"] = request.UserHostAddress
                };
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "获取客户端信息");
                return null;
            }
        }
        #endregion
    }
}
