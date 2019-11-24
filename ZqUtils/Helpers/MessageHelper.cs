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

using System.Text;
using System.Web;
using System.Web.UI;
/****************************
* [Author] 张强
* [Date] 2015-10-26
* [Describe] Message工具类
* **************************/
namespace ZqUtils.Helpers
{
    /// <summary>
    /// Message工具类
    /// </summary>
    public class MessageHelper
    {
        #region 关闭弹出窗体
        /// <summary>
        /// 关闭弹出窗体
        /// </summary>
        /// <param name="page">目标窗体</param>
        public static void CloseIFrameDialog(Page page)
        {
            page.Response.Write("<script>parent.location.reload();</script>");
        }
        #endregion

        #region 刷新页面
        /// <summary>
        /// 刷新页面
        /// </summary>
        /// <param name="page">目标窗体</param>
        /// <param name="message">弹出消息</param>
        public static void Refreash(Page page, string message)
        {
            page.Response.Write($"<script>alert('{message}！');parent.location.reload();</script>");
        }
        #endregion

        #region 执行脚本
        /// <summary>
        /// 执行脚本
        /// </summary>
        /// <param name="script">脚本字符串</param>
        /// <param name="p">目标窗体</param>
        public static void RunJs(Page p, string script)
        {
            if (!p.ClientScript.IsStartupScriptRegistered(p.GetType(), "default")) p.ClientScript.RegisterStartupScript(p.GetType(), "default", script);
        }
        
        /// <summary>
        /// 执行脚本
        /// </summary>
        /// <param name="p">目标窗体</param>
        /// <param name="key">注册脚本的键</param>
        /// <param name="script">脚本字符串</param>
        public static void RunJs(Page p, string key, string script)
        {
            if (!p.ClientScript.IsStartupScriptRegistered(p.GetType(), "default")) p.ClientScript.RegisterStartupScript(p.GetType(), key, script);
        }
        #endregion

        #region 弹出窗
        /// <summary>
        /// JS弹出窗
        /// </summary>
        /// <param name="p">目标窗体</param>
        /// <param name="message">弹出内容</param>
        /// <param name="url">转向地址</param>
        public static void Alert(Page p, string message, string url)
        {
            if (!p.ClientScript.IsStartupScriptRegistered(p.GetType(), "default"))
            {
                if (string.IsNullOrEmpty(url))
                {
                    p.ClientScript.RegisterStartupScript(p.GetType(), "default", $"<script>alert('{message}');</script>");
                }
                else
                {
                    p.ClientScript.RegisterStartupScript(p.GetType(), "default", $"<script>alert('{message}');location.href='{url}';</script>");
                }
            }
        }
        
        /// <summary>
        /// JS弹出窗口
        /// </summary>
        /// <param name="content">弹出内容</param>
        /// <param name="url">转向地址或外部js程序</param>
        /// <param name="i">是关闭还是后退[0-继续运行,1-关闭窗口,2-后退,3父窗口弹出网址,4-创建新窗口转向网址,5-创建新窗口转向网址,并关闭当前窗口,9-运行外部js程序]</param>
        public static void Alert(string content, string url, int i = 0)
        {
            var script = new StringBuilder("<script language=\"javascript\" type=\"text/javascript\">");
            if (content != "") script.Append($"alert(\"{content}\");");
            switch (i)
            {
                case 0:/*继续运行*/
                    break;
                case 1:/*关闭窗口*/
                    script.Append("window.close();");
                    break;
                case 2:/*后退*/
                    script.Append("history.go(-1);");
                    break;
                case 3:/*父窗口弹出网址*/
                    script.Append($"window.parent.location.href=\"{url}\";");
                    break;
                case 4:/*创建新窗口转向网址*/
                    script.Append($"window.open(\"{url}\",\"_blank\");");
                    break;
                case 5:/*创建新窗口转向网址,并关闭当前窗口*/
                    script.Append($"window.open(\"{url}\",\"_blank\");self.close();");
                    break;
                case 9:/*运行外部js程序*/
                    script.Append(url);
                    break;
            }
            script.Append("</script>");
            HttpContext.Current.Response.Write(script);
            if (i != 0) HttpContext.Current.Response.End();
        }
        #endregion
    }
}
