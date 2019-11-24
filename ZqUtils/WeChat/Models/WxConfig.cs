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

using Newtonsoft.Json;

namespace ZqUtils.WeChat.Models
{
    /// <summary>
    /// 微信公众号配置信息
    /// </summary>
    public class WxConfig
    {
        /// <summary>
        /// 公众号AppId
        /// </summary>
        [JsonProperty("appid")]
        public string AppId { get; set; }

        /// <summary>
        /// 微信名
        /// </summary>
        [JsonProperty("wxname")]
        public string WxName { get; set; }

        /// <summary>
        /// 商户平台密钥
        /// </summary>
        [JsonProperty("appkey")]
        public string AppKey { get; set; }

        /// <summary>
        /// 公众号密钥
        /// </summary>
        [JsonProperty("appsecret")]
        public string AppSecret { get; set; }

        /// <summary>
        /// 商户帐号
        /// </summary>
        [JsonProperty("mch_id")]
        public string Mch_Id { get; set; }

        /// <summary>
        /// 在支付完成后，接收微信通知支付结果的URL，需给绝对路径， 255 字符内, 格式如:http://wap.tenpay.com/tenpay.asp；
        /// </summary>
        [JsonProperty("notify_url")]
        public string Notify_Url { get; set; }
    }
}
