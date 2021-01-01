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

namespace ZqUtils.WeChat.Models
{
    /// <summary>
    /// 图文消息的子项
    /// </summary>
    public class ImgTextMsgItem
    {
        /// <summary>
        /// 图文消息标题
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// 图文消息描述
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// 图片链接，支持JPG、PNG格式，较好的效果为大图360*200，小图200*200
        /// </summary>
        public string PicUrl { get; set; }
        
        /// <summary>
        /// 点击图文消息跳转链接(最好为短连接)
        /// </summary>
        public string Url { get; set; }
    }
}
