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

using DnsClient;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Web;
using ZqUtils.Extensions;
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
        #region GetIpAddress
        /// <summary>
        /// 获取本地的IP地址
        /// </summary>
        /// <param name="ipv4">是否ipv4，否则ipv6，默认：ipv4</param>
        /// <param name="wifi">是否无线网卡，默认：有线网卡</param>
        /// <returns></returns>
        public static string GetIpAddress(bool ipv4 = true, bool wifi = false)
        {
            return NetworkInterface
                        .GetAllNetworkInterfaces()
                        .Where(x => (wifi ?
                            x.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ://WIFI
                            x.NetworkInterfaceType == NetworkInterfaceType.Ethernet) && //有线网
                            x.OperationalStatus == OperationalStatus.Up)
                        .Select(p => p.GetIPProperties())
                        .SelectMany(p => p.UnicastAddresses)
                        .Where(p => (ipv4 ?
                            p.Address.AddressFamily == AddressFamily.InterNetwork :
                            p.Address.AddressFamily == AddressFamily.InterNetworkV6) &&
                            !IPAddress.IsLoopback(p.Address))
                        .FirstOrDefault()?
                        .Address
                        .ToString();
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
            if (HttpContext.Current.IsNull() ||
                HttpContext.Current.Request.IsNull() ||
                HttpContext.Current.Request.ServerVariables.IsNull())
                return null;

            var ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            if (ip.IsNullOrEmpty())
            {
                if (HttpContext.Current.Request.ServerVariables["HTTP_VIA"].IsNotNullOrEmpty())
                    ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString().Split(',')[0].Trim();
            }

            if (ip.IsNullOrEmpty())
                ip = HttpContext.Current.Request.UserHostAddress;

            if (ip.IsNotNullOrEmpty() && ip.IsIP() && !IPAddress.IsLoopback(IPAddress.Parse(ip)))
                return ip;

            return null;
        }
        #endregion

        #region GetClientInfo
        /// <summary>
        /// 获取客户端信息
        /// </summary>
        /// <returns>Dictionary</returns>
        public static Dictionary<string, string> GetClientInfo()
        {
            var request = HttpContext.Current.Request;
            //C#6.0字典初始化器
            return new Dictionary<string, string>
            {
                ["userAgent"] = request.UserAgent,
                ["userHostName"] = request.UserHostName,
                ["userHostAddress"] = request.UserHostAddress,
                ["remote_addr"] = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"],
                ["http_x_forwarded_for"] = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"]
            };
        }
        #endregion
    }
}