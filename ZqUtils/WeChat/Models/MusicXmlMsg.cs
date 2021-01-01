#region License
/***
 * Copyright © 2018-2021, 张强 (943620963@qq.com).
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
using ZqUtils.Extensions;
using ZqUtils.WeChat.Interfaces;

namespace ZqUtils.WeChat.Models
{
    /// <summary>
    /// 音乐消息
    /// </summary>
    public class MusicXmlMsg : IXmlMsg
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
        public string MsgType { get; set; } = "music";
        
        /// <summary>
        /// 音乐标题
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// 音乐描述
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// 音乐链接
        /// </summary>
        public string MusicURL { get; set; }
        
        /// <summary>
        /// 高质量音乐链接，WIFI环境优先使用该链接播放音乐
        /// </summary>
        public string HQMusicUrl { get; set; }
        
        /// <summary>
        /// 缩略图的媒体id，通过素材管理接口上传多媒体文件，得到的id
        /// </summary>
        public string ThumbMediaId { get; set; }
        
        /// <summary>
        /// 转换成xml
        /// </summary>
        /// <returns>string</returns>
        public string ToXml()
        {
            return $@"<xml>
                        <ToUserName><![CDATA[{ToUserName}]]></ToUserName>
                        <FromUserName><![CDATA[{FromUserName}]]></FromUserName>
                        <CreateTime>{CreateTime}</CreateTime>
                        <MsgType><![CDATA[{MsgType}]]></MsgType>
                        <Music>
                            <Title><![CDATA[{Title}]]></Title>
                            <Description><![CDATA[{Description}]]></Description>
                            <MusicUrl><![CDATA[{MusicURL}]]></MusicUrl>
                            <HQMusicUrl><![CDATA[{HQMusicUrl}]]></HQMusicUrl>
                            <ThumbMediaId><![CDATA[{ThumbMediaId}]]></ThumbMediaId>
                        </Music>
                    </xml>";
        }
    }
}
