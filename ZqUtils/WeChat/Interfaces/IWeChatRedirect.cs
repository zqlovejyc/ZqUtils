﻿#region License
/***
 * Copyright © 2018-2025, 张强 (943620963@qq.com).
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
