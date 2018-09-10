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
