namespace ZqUtils.WeChat.Interfaces
{
    /// <summary>
    /// 被动回复消息xml接口
    /// </summary>
    public interface IXmlMsg
    {
        /// <summary>
        /// 接收方帐号(收到的OpenID)
        /// </summary>
        string ToUserName { get; set; }
        
        /// <summary>
        /// 开发者微信号
        /// </summary>
        string FromUserName { get; set; }
        
        /// <summary>
        /// 消息创建时间(整型)
        /// </summary>
        int CreateTime { get; set; }
        
        /// <summary>
        /// 消息类型
        /// </summary>
        string MsgType { get; set; }
        
        /// <summary>
        /// 转换成xml
        /// </summary>
        /// <returns>string</returns>
        string ToXml();
    }
}
