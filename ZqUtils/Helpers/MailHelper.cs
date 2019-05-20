#region License
/***
 * Copyright © 2018-2019, 张强 (943620963@qq.com).
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * without warranties or conditions of any kind, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#endregion

using System;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Collections.Generic;
using OpenPop.Mime;
using OpenPop.Mime.Header;
using OpenPop.Pop3;
/****************************
* [Author] 张强
* [Date] 2015-10-26
* [Describe] Email工具类
* **************************/
namespace ZqUtils.Helpers
{
    /// <summary>
    /// Email工具类
    /// </summary>
    public class MailHelper
    {
        #region 获取邮件
        /// <summary>
        /// 邮件附件
        /// </summary>
        public class MailAttachment
        {
            /// <summary>
            /// 邮件头部信息
            /// </summary>
            public MessageHeader Header { get; set; }
            
            /// <summary>
            /// 邮件内容部分
            /// </summary>
            public MessagePart Part { get; set; }
        }
        
        /// <summary>
        /// 获取邮件的所有附件
        /// </summary>
        /// <param name="hostname">服务器主机名，如： pop3.live.com</param>
        /// <param name="port">服务器端口号，通常： 110 for plain POP3, 995 for SSL POP3</param>
        /// <param name="useSsl">是否使用SSL连接服务器</param>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="isDelete">是否删除所有邮件</param>
        /// <returns>获取所有附件从POP3服务器</returns>
        public static List<MailAttachment> FetchAllAttachments(string hostname, int port, bool useSsl, string username, string password, bool isDelete = false)
        {
            var attachments = new List<MailAttachment>();
            try
            {
                using (var client = new Pop3Client())
                {
                    client.Connect(hostname, port, useSsl);
                    client.Authenticate(username, password);
                    //获取邮件数量                
                    var messageCount = client.GetMessageCount();
                    for (var i = messageCount; i > 0; i--)
                    {
                        var message = client.GetMessage(i);
                        if (isDelete) client.DeleteMessage(i);
                        var messageParts = message.FindAllAttachments();
                        if (messageParts.Count > 0)
                        {
                            messageParts.ForEach(o =>
                            {
                                attachments.Add(new MailAttachment
                                {
                                    Header = message.Headers,
                                    Part = o
                                });
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "获取邮件的所有附件");
            }
            return attachments;
        }
        #endregion

        #region 发送邮件
        /// <summary> 
        /// 发送邮件
        /// </summary> 
        /// <param name="from">发送人邮件地址</param> 
        /// <param name="fromname">发送人显示名称</param> 
        /// <param name="to">发送给谁（邮件地址），多个邮件地址以逗号分割</param> 
        /// <param name="subject">标题</param> 
        /// <param name="body">内容</param> 
        /// <param name="username">邮件登录名</param> 
        /// <param name="password">邮件密码</param> 
        /// <param name="server">邮件服务器（smtp.126.com）</param> 
        /// <param name="fujian">附件</param> 
        /// <returns>bool</returns> 
        public static bool SendMail(string from, string fromname, string to, string subject, string body, string username, string password, string server, string fujian)
        {
            var result = false;
            try
            {
                //邮件发送类 
                using (var mail = new MailMessage())
                {
                    //是谁发送的邮件 
                    mail.From = new MailAddress(from, fromname);
                    //发送给谁 
                    mail.To.Add(to);
                    //标题 
                    mail.Subject = subject;
                    //内容编码 
                    mail.BodyEncoding = Encoding.Default;
                    //发送优先级 
                    mail.Priority = MailPriority.High;
                    //邮件内容 
                    mail.Body = body;
                    //是否HTML形式发送 
                    mail.IsBodyHtml = true;
                    //附件 
                    if (fujian.Length > 0) mail.Attachments.Add(new Attachment(fujian));
                    //邮件服务器和端口 
                    using (var smtp = new SmtpClient(server, 25))
                    {
                        smtp.UseDefaultCredentials = true;
                        //指定发送方式 
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                        //指定登录名和密码 
                        smtp.Credentials = new NetworkCredential(username, password);
                        //超时时间 
                        smtp.Timeout = 10000;
                        smtp.Send(mail);
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "发送邮件");
            }
            return result;
        }
        #endregion
    }
}
