namespace ZqUtils.WeChat.Interfaces
{
    /// <summary>
    /// 微信服务接口
    /// </summary>
    public interface IWeChatService
    {
        /// <summary>
        /// 获取jsapicofig
        /// </summary>
        /// <param name="pageUrl">当前页面地址</param>
        /// <returns>string</returns>
        string GetJSAPIConfig(string pageUrl);
        
        /// <summary>
        /// 获取微信js接口支付配置信息
        /// </summary>
        /// <param name="ip">用户IP</param>
        /// <param name="openid">用户openId</param>
        /// <param name="out_trade_no">商品交易号</param>
        /// <returns>string</returns>
        string GetWxPayJson(string ip, string openid, string out_trade_no);
    }
}
