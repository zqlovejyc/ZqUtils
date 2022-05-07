#region License
/***
 * Copyright © 2018-2025, 张强 (943620963@qq.com).
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
using Newtonsoft.Json;

namespace ZqUtils.WeChat.Models
{
    /// <summary>
    /// 公众号用于调用微信JS接口的临时票据
    /// </summary>
    public class JsApiTicket
    {
        /// <summary>
        /// 公众号appid
        /// </summary>
        [JsonProperty("appid")]
        public string AppId { get; set; }

        /// <summary>
        /// 状态码
        /// </summary>
        [JsonProperty("errcode")]
        public string ErrCode { get; set; }

        /// <summary>
        /// 返回消息
        /// </summary>
        [JsonProperty("errmsg")]
        public string ErrMsg { get; set; }

        /// <summary>
        /// jsapi票据
        /// </summary>
        [JsonProperty("ticket")]
        public string Ticket { get; set; }

        /// <summary>
        /// 过期时间长度
        /// </summary>
        [JsonProperty("expires_in")]
        public string Expires_In { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        [JsonProperty("begintime")]
        public DateTime BeginTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        [JsonProperty("endtime")]
        public DateTime EndTime { get; set; }
    }
}
