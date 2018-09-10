using System;
using Newtonsoft.Json;

namespace ZqUtils.WeChat.Models
{
    /// <summary>
    /// 微信公众号的访问令牌
    /// </summary>
    public class AccessToken
    {
        /// <summary>
        /// 公众号appId
        /// </summary>
        [JsonProperty("appid")]
        public string AppId { get; set; }

        /// <summary>
        /// 令牌字符串
        /// </summary>
        [JsonProperty("access_token")]
        public string Access_Token { get; set; }

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
