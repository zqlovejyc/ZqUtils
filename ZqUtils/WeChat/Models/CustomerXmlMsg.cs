using System;
using System.Text;
using ZqUtils.Extensions;
using ZqUtils.WeChat.Interfaces;

namespace ZqUtils.WeChat.Models
{
    /// <summary>
    /// 多客服消息
    /// </summary>
    public class CustomerXmlMsg : IXmlMsg
    {
        /// <summary>
        /// 接收方帐号(收到的OpenID)
        /// </summary>
        public string ToUserName { get; set; }
        
        /// <summary>
        /// 开发者微信号
        /// </summary>
        public string FromUserName { get; set; }
        
        /// <summary>
        /// 消息创建时间(整型)
        /// </summary>
        public int CreateTime { get; set; } = DateTime.Now.ToUnixTime();
        
        /// <summary>
        /// 消息类型
        /// </summary>
        public string MsgType { get; set; } = "transfer_customer_service";
        
        /// <summary>
        /// 指定会话接入的客服账号
        /// </summary>
        public string KfAccount { get; set; }
        
        /// <summary>
        /// 转换成xml方法
        /// </summary>
        /// <returns>string</returns>
        public string ToXml()
        {
            var sb = new StringBuilder();
            sb.Append("<xml>")
              .Append($"<ToUserName><![CDATA[{ToUserName}]]></ToUserName>")
              .Append($"<FromUserName><![CDATA[{FromUserName}]]></FromUserName>")
              .Append($"<CreateTime>{CreateTime}</CreateTime>")
              .Append($"<MsgType><![CDATA[{MsgType}]]></MsgType>");
            if (!KfAccount.IsNull())
            {
                sb.Append("<TransInfo>")
                  .Append($"<KfAccount><![CDATA[{KfAccount}]]></KfAccount>")
                  .Append("</TransInfo>");
            }
            sb.Append("</xml>");
            return sb.ToString();
        }
    }
}
