using System.Web;

namespace ZqUtils.WeChat.Interfaces
{
    /// <summary>
    /// 微信异步通知接口
    /// </summary>
    public interface IWeChatNotify
    {
        /// <summary>
        ///  微信支付回调通知，用于更新微信订单支付状态
        /// </summary>
        /// <param name="context">HttpContext</param>
        void Notify(HttpContext context);
    }
}
