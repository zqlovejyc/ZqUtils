using System.Web;

namespace ZqUtils.WeChat.Interfaces
{
    /// <summary>
    /// 微信跳转获取openId接口
    /// </summary>
    public interface IWeChatRedirect
    {
        /// <summary>
        /// 微信跳转
        /// </summary>
        /// <param name="context">HttpContext</param>
        void WxRedirect(HttpContext context);
    }
}
