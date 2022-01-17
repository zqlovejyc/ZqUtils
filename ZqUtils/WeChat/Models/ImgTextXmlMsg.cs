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
using System.Text;
using System.Collections.Generic;
using ZqUtils.WeChat.Interfaces;
using ZqUtils.Extensions;

namespace ZqUtils.WeChat.Models
{
    /// <summary>
    /// 图文消息
    /// </summary>
    public class ImgTextXmlMsg : IXmlMsg
    {
        /// <summary>
        /// 接收方帐号(收到的OpenID)
        /// </summary>
        public string ToUserName { get; set; }
        
        /// <summary>
        /// 开发者微信号
        /// </summary>
        public string FromUserName { get; set; }
        
        /// <summary>
        /// 消息创建时间(整型)
        /// </summary>
        public int CreateTime { get; set; } = DateTime.Now.ToUnixTime();
        
        /// <summary>
        /// 消息类型
        /// </summary>
        public string MsgType { get; set; } = "news";
        
        /// <summary>
        /// 图文消息个数，限制为10条以内(包含10)
        /// </summary>
        public int ArticleCount { get; set; }
        
        /// <summary>
        /// 图文列表
        /// </summary>
        public List<ImgTextMsgItem> Items { get; set; }
        
        /// <summary>
        /// 转换成xml
        /// </summary>
        /// <returns>string</returns>
        public string ToXml()
        {
            var sb = new StringBuilder();
            sb.Append("<xml>")
              .Append($"<ToUserName><![CDATA[{ToUserName}]]></ToUserName>")
              .Append($"<FromUserName><![CDATA[{FromUserName}]]></FromUserName>")
              .Append($"<CreateTime>{CreateTime}</CreateTime>")
              .Append($"<MsgType><![CDATA[{MsgType}]]></MsgType>")
              .Append($"<ArticleCount>{ArticleCount}</ArticleCount>")
              .Append("<Articles>");
            Items.ForEach(o =>
            {
                sb.Append("<item>")
                  .Append($"<Title><![CDATA[{o.Title}]]></Title> ")
                  .Append($"<Description><![CDATA[{o.Description}]]></Description>")
                  .Append($"<PicUrl><![CDATA[{o.PicUrl}]]></PicUrl>")
                  .Append($"<Url><![CDATA[{o.Url}]]></Url>")
                  .Append("</item>");
            });
            sb.Append("</Articles>")
              .Append("</xml> ");
            return sb.ToString();
        }
    }
}
