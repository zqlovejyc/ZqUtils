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

using Newtonsoft.Json;

namespace ZqUtils.WeChat.Models
{
    /// <summary>
    /// 微信授权返回数据模型
    /// </summary>
    public class OAuthModel
    {
        /// <summary>
        /// 网页授权接口调用凭证,注意：此access_token与基础支持的access_token不同
        /// </summary>
        [JsonProperty("access_token")]
        public string Access_Token { get; set; }

        /// <summary>
        /// access_token过期时间
        /// </summary>
        [JsonProperty("access_token_expires_in")]
        public string Access_Token_Expires_In { get; set; }

        /// <summary>
        /// 用户刷新access_token
        /// </summary>
        [JsonProperty("refresh_token")]
        public string Refresh_Token { get; set; }

        /// <summary>
        /// 用户刷新access_token过期时间
        /// </summary>
        [JsonProperty("refresh_token_expires_in")]
        public string Refresh_Token_Expires_In { get; set; }

        /// <summary>
        /// 用户openId
        /// </summary>
        [JsonProperty("openid")]
        public string OpenId { get; set; }

        /// <summary>
        /// 用户授权的作用域
        /// </summary>
        [JsonProperty("scope")]
        public string Scope { get; set; }
    }
}
