using System.Web;

namespace ZqUtils.WeChat.Interfaces
{
    /// <summary>
    /// 微信公众平台后台服务器接口
    /// </summary>
    public interface IWeChatServerUrl
    {
        /// <summary>
        /// 处理微信请求，被动回复消息
        /// </summary>
        /// <param name="context">HttpContext</param>
        void WxService(HttpContext context);
    }
}
