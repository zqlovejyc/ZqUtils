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
