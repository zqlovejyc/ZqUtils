﻿using System;
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
        public static void Write(string strName, string strValue)
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
        public static void Write(string strName, string strValue, int expires)
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
