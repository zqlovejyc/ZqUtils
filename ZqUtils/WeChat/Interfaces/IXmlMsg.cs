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

namespace ZqUtils.WeChat.Interfaces
{
    /// <summary>
    /// 被动回复消息xml接口
    /// </summary>
    public interface IXmlMsg
    {
        /// <summary>
        /// 接收方帐号(收到的OpenID)
        /// </summary>
        string ToUserName { get; set; }
        
        /// <summary>
        /// 开发者微信号
        /// </summary>
        string FromUserName { get; set; }
        
        /// <summary>
        /// 消息创建时间(整型)
        /// </summary>
        int CreateTime { get; set; }
        
        /// <summary>
        /// 消息类型
        /// </summary>
        string MsgType { get; set; }
        
        /// <summary>
        /// 转换成xml
        /// </summary>
        /// <returns>string</returns>
        string ToXml();
    }
}
