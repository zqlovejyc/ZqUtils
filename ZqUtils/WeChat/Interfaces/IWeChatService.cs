#region License
/***
 * Copyright © 2018, 张强 (943620963@qq.com).
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
