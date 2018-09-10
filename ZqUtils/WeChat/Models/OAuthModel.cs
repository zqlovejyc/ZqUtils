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
