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
using System.Web;
/****************************
 * [Author] 张强
 * [Date] 2015-10-26
 * [Describe] Cookie工具类
 * **************************/
namespace ZqUtils.Helpers
{
    /// <summary>
    /// Cookie工具类
    /// </summary>
    public class CookieHelper
    {
        #region 写入cookie
        /// <summary>
        ///  写入cookie
        /// </summary>
        /// <param name="strName">cookie名称</param>
        /// <param name="strValue">cookie值</param>
        public static void Set(string strName, string strValue)
        {
            var cookie = HttpContext.Current.Request.Cookies[strName];            
            if (cookie == null) cookie = new HttpCookie(strName);
            cookie.Value = strValue;
            HttpContext.Current.Response.AppendCookie(cookie);
        }
        
        /// <summary>
        /// 写入cookie
        /// </summary>
        /// <param name="strName">cookie名称</param>
        /// <param name="strValue">cookie值</param>
        /// <param name="expires">过期时间(单位：分钟)</param>
        public static void Set(string strName, string strValue, int expires)
        {
            var cookie = HttpContext.Current.Request.Cookies[strName];
            if (cookie == null) cookie = new HttpCookie(strName);
            cookie.Value = strValue;
            cookie.Expires = DateTime.Now.AddMinutes((double)expires);
            HttpContext.Current.Response.AppendCookie(cookie);
        }
        #endregion

        #region 读取cookie
        /// <summary>
        /// 获取cookie
        /// </summary>
        /// <param name="strName">cookie名称</param>
        /// <returns>string</returns>
        public static string Get(string strName)
        {
            string result=string.Empty;
            if (HttpContext.Current.Request.Cookies != null && HttpContext.Current.Request.Cookies[strName] != null) result = HttpContext.Current.Request.Cookies[strName].Value.ToString();
            return result;
        }
        #endregion
    }
}
