using Newtonsoft.Json;

namespace ZqUtils.WeChat.Models
{
    /// <summary>
    /// 微信订单类
    /// </summary>
    public class WxOrder
    {
        /// <summary>
        /// 商品描述
        /// </summary>
        [JsonProperty("body")]
        public string Body { get; set; }

        /// <summary>
        /// 商户订单号
        /// </summary>
        [JsonProperty("out_trade_no")]
        public string Out_Trade_No { get; set; }

        /// <summary>
        /// 订单金额
        /// </summary>
        [JsonProperty("total_fee")]
        public string Total_Fee { get; set; }

        /// <summary>
        /// 下单IP
        /// </summary>
        [JsonProperty("spbill_create_ip")]
        public string Spbill_Create_Ip { get; set; }

        /// <summary>
        /// 下单者微信openid(trade_type=JSAPI，此参数必传)
        /// </summary>
        [JsonProperty("openid")]
        public string OpenId { get; set; }

        /// <summary>
        /// 微信预支付订单号
        /// </summary>
        [JsonProperty("prepay_id")]
        public string Prepay_Id { get; set; }

        /// <summary>
        /// 商品ID(trade_type=NATIVE，此参数必传)
        /// </summary>
        [JsonProperty("product_id")]
        public string Product_Id { get; set; }

        /// <summary>
        /// 场景信息(trade_type=MWEB，此参数必传)
        /// </summary>
        [JsonProperty("scene_info")]
        public string Scene_Info { get; set; }

        /// <summary>
        /// 刷卡支付中支付授权码，设备读取用户微信中的条码或者二维码信息（此参数仅用于刷卡支付）
        /// </summary>
        [JsonProperty("auth_code")]
        public string Auth_Code { get; set; }
    }
}
