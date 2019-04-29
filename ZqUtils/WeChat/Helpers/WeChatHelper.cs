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

using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using ZqUtils.Helpers;
using ZqUtils.Extensions;
using ZqUtils.WeChat.Models;
/****************************
* [Author] 张强
* [Date] 2015-10-12
* [Describe] 微信工具类
* **************************/
namespace ZqUtils.WeChat.Helpers
{
    /// <summary>
    /// 微信工具类
    /// </summary>
    public class WeChatHelper : IDisposable
    {
        #region 私有字段
        /// <summary>
        /// 公众号令牌 AccessToken
        /// </summary>
        private string accessToken;

        /// <summary>
        /// jsapi_ticket(调用微信JS接口的临时票据)
        /// </summary>
        private string jsApiTicket;

        /// <summary>
        /// 微信配置对象
        /// </summary>
        private WxConfig wxConfig;
        #endregion

        #region 构造函数
        /// <summary>
        /// 含参构造函数
        /// </summary>
        /// <param name="wxName">微信名称</param>
        /// <param name="f_WxConfig">获取WxConfig泛型委托</param>
        public WeChatHelper(string wxName, Func<string, WxConfig> f_WxConfig)
        {
            try
            {
                wxConfig = f_WxConfig(wxName);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "含参构造函数");
            }
        }

        /// <summary>
        /// 含参构造函数
        /// </summary>
        /// <param name="appIdOrName">微信appid/微信名称</param>
        /// <param name="f_AccessToken">获取AccessToken泛型委托</param>
        public WeChatHelper(string appIdOrName, Func<string, string> f_AccessToken)
        {
            try
            {
                accessToken = f_AccessToken(appIdOrName);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "含参构造函数");
            }
        }

        /// <summary>
        /// 含参构造函数
        /// </summary>
        /// <param name="accessToken">访问令牌</param>
        /// <param name="jsApiTicket">微信jssdk配置请求票据</param>
        /// <param name="wxConfig">微信配置对象</param>
        public WeChatHelper(string accessToken, string jsApiTicket, WxConfig wxConfig)
        {
            this.accessToken = accessToken;
            this.jsApiTicket = jsApiTicket;
            this.wxConfig = wxConfig;
        }

        /// <summary>
        /// 含参构造函数
        /// </summary>
        /// <param name="appIdOrName">微信appid/微信名称</param>
        /// <param name="f_WxConfig">获取WxConfig泛型委托</param>
        /// <param name="f_AccessToken">获取AccessToken泛型委托</param>
        public WeChatHelper(string appIdOrName, Func<string, WxConfig> f_WxConfig, Func<string, string> f_AccessToken)
        {
            try
            {
                wxConfig = f_WxConfig(appIdOrName);
                accessToken = f_AccessToken(appIdOrName);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "含参构造函数");
            }
        }

        /// <summary>
        /// 含参构造函数
        /// </summary>
        /// <param name="wxName">微信名称</param>
        /// <param name="f_WxConfig">获取WxConfig泛型委托</param>
        /// <param name="f_AccessToken">获取AccessToken泛型委托</param>
        /// <param name="f_JsApiTicket">获取JsApiTicket泛型委托</param>
        public WeChatHelper(string wxName, Func<string, WxConfig> f_WxConfig, Func<string, string> f_AccessToken, Func<string, string> f_JsApiTicket)
        {
            try
            {
                wxConfig = f_WxConfig(wxName);
                accessToken = f_AccessToken(wxConfig.AppId);
                jsApiTicket = f_JsApiTicket(wxConfig.AppId);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "含参构造函数");
            }
        }

        /// <summary>
        /// 含参构造函数
        /// </summary>
        /// <param name="wxName">微信名称</param>
        /// <param name="f_WxConfig">获取WxConfig泛型委托</param>
        /// <param name="f_AccessToken">获取AccessToken泛型委托</param>
        /// <param name="f_JsApiTicket">获取JsApiTicket泛型委托</param>
        /// <param name="IsAppIdUnique">是否微信appId唯一，参数值必须置为false，否则抛出异常</param>
        public WeChatHelper(string wxName, Func<string, WxConfig> f_WxConfig, Func<string, string> f_AccessToken, Func<string, string> f_JsApiTicket, bool IsAppIdUnique)
        {
            if (!IsAppIdUnique)
            {
                try
                {
                    wxConfig = f_WxConfig(wxName);
                    accessToken = f_AccessToken(wxName);
                    jsApiTicket = f_JsApiTicket(wxName);
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex, "含参构造函数");
                }
            }
            else
            {
                throw new ArgumentException("IsAppIdUnique不可为空，必须置为false！");
            }
        }
        #endregion

        #region 微信公众号
        #region 获取AccessToken
        /// <summary>
        /// 访问官网获取公众号令牌 Access_Token
        /// </summary>
        /// <returns>string</returns>
        public string GetAccessToken()
        {
            var accessToken = string.Empty;
            try
            {
                var url = $"https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={wxConfig.AppId}&secret={wxConfig.AppSecret}";
                var res = HttpHelper.Get(url);
                if (!string.IsNullOrEmpty(res))
                {
                    var obj = res.ToObject<AccessToken>();
                    accessToken = obj?.Access_Token;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "访问官网获取公众号令牌 Access_Token");
            }
            return accessToken;
        }
        #endregion

        #region 获取JsApiTicket
        /// <summary>
        /// 通过访问官网获取jsapi_ticket
        /// </summary>
        /// <returns>string</returns>
        public string GetJsApiTicket()
        {
            var ticket = string.Empty;
            try
            {
                var url = $"https://api.weixin.qq.com/cgi-bin/ticket/getticket?access_token={accessToken}&type=jsapi";
                var res = HttpHelper.Get(url);
                if (!string.IsNullOrEmpty(res))
                {
                    var obj = res.ToObject<JsApiTicket>();
                    ticket = obj?.Ticket;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "通过访问官网获取jsapi_ticket");
            }
            return ticket;
        }
        #endregion

        #region 获取调用微信JS接口的config配置json信息
        /// <summary>
        /// 获取调用微信JS接口的config配置信息
        /// </summary>
        /// <param name="pageurl">当前网页的URL，不包含#及其后面部分(必须是调用JS接口页面的完整URL)</param>
        /// <returns>string</returns>
        public string GetJsApiConfig(string pageurl)
        {
            var timestamp = DateTime.UtcNow.ToTimeStamp();//时间戳
            var nonceStr = Guid.NewGuid().BuildNonceStr();//随机字符串
            var jsTicket = jsApiTicket;
            //C#6.0字典初始化器
            var dic = new Dictionary<string, string>
            {
                ["jsapi_ticket"] = jsTicket,
                ["noncestr"] = nonceStr,
                ["timestamp"] = timestamp,
                ["url"] = pageurl
            };
            var asciiSort = dic.ToUrl();
            var signature = CryptHelper.SHA1(asciiSort).ToLower();//配置签名采用小写
            var jsapiJsonDic = new Dictionary<string, string>
            {
                ["timestamp"] = timestamp,//必填，生成签名的时间戳
                ["nonceStr"] = nonceStr,// 必填，生成签名的随机串
                ["signature"] = signature,//必填，签名
                ["appId"] = wxConfig.AppId//必填，公众号的唯一标识
            };
            return jsapiJsonDic.ToJson();
        }
        #endregion

        #region 短连接
        /// <summary>
        /// 长连接转短连接
        /// </summary>
        /// <param name="longUrl">长连接地址</param>
        /// <returns>string</returns>
        public string GetShortUrl(string longUrl)
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/shorturl?access_token={accessToken}";
            var postJson = $"{{\"action\":\"long2short\",\"long_url\":\"{longUrl}\"}}";
            return HttpHelper.Post(url, postJson);
        }

        /// <summary>
        /// 扫码原生支付模式一中的二维码链接转成短链接
        /// </summary>
        /// <param name="longUrl">扫码原生支付模式一中的二维码链接</param>
        /// <returns>string</returns>
        public string NativePayUrlToShort(string longUrl)
        {
            var result = string.Empty;
            var url = "https://api.mch.weixin.qq.com/tools/shorturl";
            //随机字符串
            var nonce_str = Guid.NewGuid().BuildNonceStr();
            var dic = new Dictionary<string, string>
            {
                ["appid"] = wxConfig.AppId,//公众号ID或APP应用ID
                ["mch_id"] = wxConfig.Mch_Id,//商户号
                ["nonce_str"] = nonce_str,//随机字符串
                ["long_url"] = longUrl//扫码原生支付模式一中的二维码链接                             
            };
            //ASCII排序
            var asciiSort = dic.ToUrl();
            //签名与支付签名算法一致，大写
            var sign = CryptHelper.MD5($"{asciiSort}&key={wxConfig.AppKey}").ToUpper();
            dic.Add("sign", sign);
            //发送请求       
            var responseXml = HttpHelper.Post(url, dic.ToXml());
            var dicXml = XmlHelper.XmlToDictionary(responseXml);
            if
            (
                dicXml.Count > 0
                && dicXml.Keys.Contains("return_code")
                && dicXml.Keys.Contains("result_code")
                && dicXml["return_code"].ToUpper().Contains("SUCCESS")
                && dicXml["result_code"].ToUpper().Contains("SUCCESS")
                && CheckMD5Sign(dicXml)
            )
            {
                result = dicXml["short_url"];
            }
            else
            {
                LogHelper.Info($"扫码原生支付模式一中的二维码链接转成短链接失败，返回信息：{dicXml.ToJson()}");
            }
            return result;
        }
        #endregion

        #region 带参数二维码
        /// <summary>
        /// 生成带参数的二维码
        /// </summary>
        /// <param name="postJson">post数据</param>
        /// <returns>string</returns>
        public string CreateQRCode(string postJson)
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/qrcode/create?access_token={accessToken}";
            return HttpHelper.Post(url, postJson);
        }

        /// <summary>
        /// 通过创建二维码返回的ticket获取二维码图片
        /// </summary>
        /// <param name="ticket">获取的二维码ticket</param>
        /// <returns>string</returns>
        public string ShowQRCode(string ticket)
        {
            return $"https://mp.weixin.qq.com/cgi-bin/showqrcode?ticket={System.Web.HttpUtility.UrlEncode(ticket)}";
        }
        #endregion

        #region 验证服务器地址的有效性
        /// <summary>
        /// 校验服务器地址get返回数据的签名是否有效
        /// </summary>
        /// <param name="dic">字典数据</param>
        /// <param name="token">微信公众平台设置的令牌token</param>
        /// <returns>bool</returns>
        public bool CheckSHA1Sign(Dictionary<string, string> dic, string token)
        {
            var signature = dic["signature"];
            var arr = new[] { token, dic["timestamp"], dic["nonce"] }.OrderBy(o => o).ToArray();
            var str = CryptHelper.SHA1(string.Join("", arr));
            return str.ToLower() == signature;
        }
        #endregion

        #region 微信支付
        /// <summary>
        /// 校验订单查询中返回数据的md5签名是否有效
        /// </summary>
        /// <param name="dic">字典数据</param>
        /// <returns>bool</returns>
        public bool CheckMD5Sign(Dictionary<string, string> dic)
        {
            //返回的签名
            var responSign = dic["sign"];
            //sign不参与签名
            dic.Remove("sign");
            //ASCII排序
            var asciiSort = dic.ToUrl();
            //追加key参数，MD5加密，最后转换为大写
            var sign = CryptHelper.MD5($"{asciiSort}&key={ wxConfig.AppKey }").ToUpper();
            //判断签名是否一致
            return sign == responSign;
        }

        /// <summary>
        /// 获取微信支付预支付订单号
        /// </summary>
        /// <param name="order">微信订单</param>
        /// <param name="tradeType">支付类型</param>
        /// <param name="isLimitOpenId">是否限制付款者微信openid</param>
        /// <returns>string</returns>
        public string GetPrePayId(WxOrder order, string tradeType = "JSAPI", bool isLimitOpenId = true)
        {
            var prepay_id = string.Empty;
            var url = "https://api.mch.weixin.qq.com/pay/unifiedorder";
            //随机字符串
            var nonce_str = Guid.NewGuid().BuildNonceStr();
            var dic = new Dictionary<string, string>
            {
                ["appid"] = wxConfig.AppId,//公众号ID或APP应用ID
                ["mch_id"] = wxConfig.Mch_Id,//商户号
                ["nonce_str"] = nonce_str,//随机字符串
                ["body"] = order.Body,//商品描述                             
                ["out_trade_no"] = order.Out_Trade_No,//商户订单号  
                ["total_fee"] = order.Total_Fee,//订单总金额 单位（分）
                ["spbill_create_ip"] = order.Spbill_Create_Ip,//订单生成的机器IP
                ["notify_url"] = wxConfig.Notify_Url,//在支付完成后，接收微信通知支付结果的URL，需给绝对路径，255字符内   
                ["trade_type"] = tradeType//交易类型
            };
            //是否限制付款者微信openid
            if (isLimitOpenId && !order.OpenId.IsNull()) dic.Add("openid", order.OpenId);
            //扫码支付时，此参数必传（商品ID）
            if (tradeType.ToUpper().Equals("NATIVE")) dic.Add("product_id", order.Product_Id);
            //H5网页支付，此参数必传（场景信息）
            if (tradeType.ToUpper().Equals("MWEB")) dic.Add("scene_info", order.Scene_Info);
            //ASCII排序
            var asciiSort = dic.ToUrl();
            //签名与支付签名算法一致，大写
            var sign = CryptHelper.MD5($"{asciiSort}&key={wxConfig.AppKey}").ToUpper();
            dic.Add("sign", sign);
            //发送请求       
            var responseXml = HttpHelper.Post(url, dic.ToXml());
            var dicXml = XmlHelper.XmlToDictionary(responseXml);
            if (dicXml.Count > 0 && dicXml.Keys.Contains("prepay_id"))
            {
                prepay_id = dicXml["prepay_id"];
                //H5支付，返回prepay_id和mweb_url的json字符串
                if (tradeType.ToUpper().Equals("MWEB")) prepay_id = $"{{\"prepay_id\":\"{dicXml["prepay_id"]}\",\"mweb_url\":\"{dicXml["mweb_url"]}\"}}";
            }
            else
            {
                LogHelper.Info($"获取预支付订单号失败，返回信息：{responseXml}");
            }
            return prepay_id;
        }

        /// <summary>
        /// 获取微信支付配置信息
        /// </summary>
        /// <param name="order">微信订单</param>
        /// <returns>string</returns>
        public string GetWxPayJson(WxOrder order)
        {
            //时间戳
            var timeStamp = DateTime.UtcNow.ToTimeStamp();
            //随机字符串
            var nonceStr = Guid.NewGuid().BuildNonceStr();
            //订单详情扩展字符串
            var package = $"prepay_id={order.Prepay_Id}";//预支付订单号
            //参与签名的参数字符串
            var dic = new Dictionary<string, string>
            {
                ["appId"] = wxConfig.AppId,
                ["nonceStr"] = nonceStr,
                ["package"] = package,
                ["signType"] = "MD5",
                ["timeStamp"] = timeStamp
            };
            //ASCII排序
            var asciiSort = dic.ToUrl();
            //微信支付签名
            var paySign = CryptHelper.MD5($"{asciiSort}&key={wxConfig.AppKey}").ToUpper();
            dic.Add("paySign", paySign);
            return dic.ToJson();
        }

        /// <summary>
        /// 获取扫码支付的二维码url(模式一)
        /// </summary>
        /// <param name="productId">商品Id</param>
        /// <returns>string</returns>
        public string GetNativePayUrl(string productId)
        {
            var timeStamp = DateTime.UtcNow.ToTimeStamp();//时间戳
            var nonce_str = Guid.NewGuid().BuildNonceStr();//随机字符串
            var dic = new Dictionary<string, string>
            {
                ["appid"] = wxConfig.AppId,
                ["mch_id"] = wxConfig.Mch_Id,
                ["time_stamp"] = timeStamp,
                ["nonce_str"] = nonce_str,
                ["product_id"] = productId
            };
            var asciiSort = dic.ToUrl();
            //签名与支付签名算法一致，大写
            var sign = CryptHelper.MD5($"{asciiSort}&key={wxConfig.AppKey}").ToUpper();
            var payUrl = $"weixin://wxpay/bizpayurl?sign={sign}&{asciiSort}";
            //扫码链接转短链接
            var shortPayUrl = NativePayUrlToShort(payUrl);
            payUrl = !string.IsNullOrEmpty(shortPayUrl) ? shortPayUrl : payUrl;
            return payUrl;
        }

        /// <summary>
        /// 微信扫码支付回调处理(扫码回调地址需要在公众平台预先设置)
        /// </summary>
        /// <param name="dic">微信回调返回参数</param>
        /// <param name="fn">根据商品Id获取WxOrder(openId,productId)委托(委托里面完成统一下单，即生成预支付订单号)</param>
        /// <returns>string</returns>
        public string NativeNotify(Dictionary<string, string> dic, Func<string, string, WxOrder> fn)
        {
            var dicError = new Dictionary<string, string>
            {
                ["return_code"] = "FAIL",
                ["return_msg"] = "统一下单失败"
            };
            var result = dicError.ToXml();
            //判断回调参数是否存在和签名是否正确
            if (dic.ContainsKey("openid") && dic.ContainsKey("product_id") && CheckMD5Sign(dic))
            {
                //获取微信订单
                var wxOrder = fn(dic["openid"], dic["product_id"]);
                var para = new Dictionary<string, string>
                {
                    ["return_code"] = "SUCCESS",
                    ["return_msg"] = "OK",
                    ["appid"] = wxConfig.AppId,
                    ["mch_id"] = wxConfig.Mch_Id,
                    ["nonce_str"] = Guid.NewGuid().BuildNonceStr(),
                    ["prepay_id"] = wxOrder.Prepay_Id,
                    ["result_code"] = "SUCCESS",
                    ["err_code_des"] = "OK"
                };
                var asciiSort = para.ToUrl();
                //签名与支付签名算法一致，大写
                var sign = CryptHelper.MD5($"{asciiSort}&key={wxConfig.AppKey}").ToUpper();
                para.Add("sign", sign);
                result = para.ToXml();
            }
            else
            {
                dicError = new Dictionary<string, string>
                {
                    ["return_code"] = "FAIL",
                    ["return_msg"] = "回调数据异常"
                };
                result = dicError.ToXml();
            }
            return result;
        }

        /// <summary>
        /// 查询订单
        /// </summary>
        /// <param name="transactionId">微信订单号（优先使用）</param>
        /// <param name="outTradeNo">商户订单号，默认为空</param>
        /// <returns>Dictionary</returns>
        public string QueryOrder(string transactionId, string outTradeNo = "")
        {
            var result = string.Empty;
            try
            {
                var url = "https://api.mch.weixin.qq.com/pay/orderquery";
                var nonceStr = Guid.NewGuid().BuildNonceStr();
                var dic = new Dictionary<string, string>
                {
                    ["appid"] = wxConfig.AppId,
                    ["mch_id"] = wxConfig.Mch_Id,
                    ["nonce_str"] = nonceStr
                };
                //微信订单号
                if (outTradeNo.IsNull())
                {
                    dic.Add("transaction_id", transactionId);
                }
                //商户订单号
                else
                {
                    dic.Add("out_trade_no", outTradeNo);
                }
                //ASCII排序
                var asciiSort = dic.ToUrl();
                //微信支付签名
                var sign = CryptHelper.MD5($"{asciiSort}&key={wxConfig.AppKey}").ToUpper();
                dic.Add("sign", sign);
                //请求微信，查询订单信息
                var responseXml = HttpHelper.Post(url, dic.ToXml());
                result = XmlHelper.XmlToDictionary(responseXml).ToJson();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "查询订单");
            }
            return result;
        }

        /// <summary>
        /// 关闭订单
        /// </summary>
        /// <param name="tradeNo">商户订单号</param>
        /// <returns>bool</returns>
        public bool CloseOrder(string tradeNo)
        {
            var result = false;
            var url = "https://api.mch.weixin.qq.com/pay/orderquery";
            var dic = new Dictionary<string, string>
            {
                ["appid"] = wxConfig.AppId,
                ["mch_id"] = wxConfig.Mch_Id,
                ["nonce_str"] = Guid.NewGuid().BuildNonceStr(),
                ["out_trade_no"] = tradeNo
            };
            //ASCII排序
            var asciiSort = dic.ToUrl();
            //微信支付签名
            var sign = CryptHelper.MD5($"{asciiSort}&key={wxConfig.AppKey}").ToUpper();
            dic.Add("sign", sign);
            //请求微信，查询订单信息
            var responseXml = HttpHelper.Post(url, dic.ToXml());
            var dicXml = XmlHelper.XmlToDictionary(responseXml);
            //判断订单是否有效并校验签名是否正确
            if
            (
                dicXml.Count > 0
                && dicXml.Keys.Contains("return_code")
                && dicXml.Keys.Contains("result_code")
                && dicXml["return_code"].ToUpper().Contains("SUCCESS")
                && dicXml["result_code"].ToUpper().Contains("SUCCESS")
                && CheckMD5Sign(dicXml)
            )
            {
                result = true;
            }
            else
            {
                LogHelper.Info($"关闭订单失败，返回信息：{dicXml.ToJson()}");
            }
            return result;
        }

        /// <summary>
        /// 核实订单是否有效
        /// </summary>
        /// <param name="transactionId">微信订单号（优先使用）</param>
        /// <param name="outTradeNo">商户订单号，默认为空</param>
        /// <returns>bool</returns>
        public bool CheckOrder(string transactionId, string outTradeNo = "")
        {
            var result = false;
            var url = "https://api.mch.weixin.qq.com/pay/orderquery";
            var nonceStr = Guid.NewGuid().BuildNonceStr();
            var dic = new Dictionary<string, string>
            {
                ["appid"] = wxConfig.AppId,
                ["mch_id"] = wxConfig.Mch_Id,
                ["nonce_str"] = nonceStr
            };
            //微信订单号
            if (outTradeNo.IsNull())
            {
                dic.Add("transaction_id", transactionId);
            }
            //商户订单号
            else
            {
                dic.Add("out_trade_no", outTradeNo);
            }
            //ASCII排序
            var asciiSort = dic.ToUrl();
            //微信支付签名
            var sign = CryptHelper.MD5($"{asciiSort}&key={wxConfig.AppKey}").ToUpper();
            dic.Add("sign", sign);
            //请求微信，查询订单信息
            var responseXml = HttpHelper.Post(url, dic.ToXml());
            var dicXml = XmlHelper.XmlToDictionary(responseXml);
            //判断订单是否有效并校验签名是否正确
            if
            (
                dicXml.Count > 0
                && dicXml.Keys.Contains("return_code")
                && dicXml.Keys.Contains("result_code")
                && dicXml["return_code"].ToUpper().Contains("SUCCESS")
                && dicXml["result_code"].ToUpper().Contains("SUCCESS")
                && CheckMD5Sign(dicXml)
            )
            {
                result = true;
            }
            else
            {
                LogHelper.Info($"订单核实失败，返回信息：{dicXml.ToJson()}");
            }
            return result;
        }

        /// <summary>
        /// 发送普通红包
        /// </summary>
        /// <param name="toOpenId">要发送者的openId</param>
        /// <param name="tradeNo">商户订单号</param>
        /// <param name="ip">ip地址</param>
        /// <param name="totalMoney">红包金额</param>
        /// <param name="totalNum">红包数量</param>
        /// <param name="sendName">发送者名称</param>
        /// <param name="wishing">红包祝福语</param>
        /// <param name="activeName">活动名称</param>
        /// <param name="remark">备注</param>
        /// <param name="sslCertPath">ca证书路径</param>
        /// <param name="sslCertPassword">ca证书密码</param>
        /// <returns>string</returns>
        public string SendRedPack(string toOpenId, string tradeNo, string ip, int totalMoney, int totalNum, string sendName, string wishing, string activeName, string remark, string sslCertPath, string sslCertPassword)
        {
            var url = "https://api.mch.weixin.qq.com/mmpaymkttransfers/sendredpack";
            //随机字符串
            var nonceStr = Guid.NewGuid().BuildNonceStr();
            //参与签名的参数字符串
            var dic = new Dictionary<string, object>
            {
                ["wxappid"] = wxConfig.AppId,
                ["mch_id"] = wxConfig.Mch_Id,
                ["mch_billno"] = tradeNo,
                ["client_ip"] = ip,
                ["re_openid"] = toOpenId,
                ["total_amount"] = totalMoney,
                ["total_num"] = totalNum,
                ["nonce_str"] = nonceStr,
                ["send_name"] = sendName,
                ["wishing"] = wishing,
                ["act_name"] = activeName,
                ["remark"] = remark
            };
            var asciiSort = dic.ToUrl();
            //微信签名
            var sign = CryptHelper.MD5($"{asciiSort}&key={wxConfig.AppKey}").ToUpper();
            //添加签名
            dic.Add("sign", sign);
            var responseXml = HttpHelper.Post(url, dic.ToXml(), sslCertPath: sslCertPath, sslPassword: sslCertPassword);
            return XmlHelper.XmlToDictionary(responseXml).ToJson();
        }

        /// <summary>
        /// 发送裂变红包
        /// </summary>
        /// <param name="toOpenId">要发送者的openId</param>
        /// <param name="tradeNo">商户订单号</param>
        /// <param name="totalMoney">红包金额</param>
        /// <param name="totalNum">红包数量</param>
        /// <param name="sendName">发送者名称</param>
        /// <param name="wishing">红包祝福语</param>
        /// <param name="activeName">活动名称</param>
        /// <param name="remark">备注</param>
        /// <param name="sslCertPath">ca证书路径</param>
        /// <param name="sslCertPassword">ca证书密码</param>
        /// <returns>string</returns>
        public string SendFissionPack(string toOpenId, string tradeNo, int totalMoney, int totalNum, string sendName, string wishing, string activeName, string remark, string sslCertPath, string sslCertPassword)
        {
            var url = "https://api.mch.weixin.qq.com/mmpaymkttransfers/sendgroupredpack";
            //随机字符串
            var nonceStr = Guid.NewGuid().BuildNonceStr();
            //参与签名的参数字符串
            var dic = new Dictionary<string, object>
            {
                ["wxappid"] = wxConfig.AppId,
                ["mch_id"] = wxConfig.Mch_Id,
                ["mch_billno"] = tradeNo,
                ["amt_type"] = "ALL_RAND",
                ["re_openid"] = toOpenId,
                ["total_amount"] = totalMoney,
                ["total_num"] = totalNum,
                ["nonce_str"] = nonceStr,
                ["send_name"] = sendName,
                ["wishing"] = wishing,
                ["act_name"] = activeName,
                ["remark"] = remark
            };
            var asciiSort = dic.ToUrl();
            //微信签名
            var sign = CryptHelper.MD5($"{asciiSort}&key={wxConfig.AppKey}").ToUpper();
            //添加签名
            dic.Add("sign", sign);
            var responseXml = HttpHelper.Post(url, dic.ToXml(), sslCertPath: sslCertPath, sslPassword: sslCertPassword);
            return XmlHelper.XmlToDictionary(responseXml).ToJson();
        }

        /// <summary>
        /// 微信退款
        /// </summary>
        /// <param name="tradeNo">商户订单号</param>
        /// <param name="transactionId">微信订单号</param>
        /// <param name="outRefundNo">商户退款单号</param>
        /// <param name="totalMoney">订单金额</param>
        /// <param name="refundMoney">退款金额</param>
        /// <param name="sslCertPath">ca证书路径</param>
        /// <param name="sslCertPassword">ca证书密码</param>
        /// <returns>string</returns>
        public string WxRefund(string tradeNo, string transactionId, string outRefundNo, int totalMoney, int refundMoney, string sslCertPath, string sslCertPassword)
        {
            var url = "https://api.mch.weixin.qq.com/secapi/pay/refund";
            //参与签名的参数字符串
            var dic = new Dictionary<string, object>
            {
                ["appid"] = wxConfig.AppId,
                ["mch_id"] = wxConfig.Mch_Id,
                ["nonce_str"] = Guid.NewGuid().BuildNonceStr(),
                ["out_refund_no"] = outRefundNo,
                ["total_fee"] = totalMoney,
                ["refund_fee"] = refundMoney,
                ["op_user_id"] = wxConfig.Mch_Id
            };
            if (!transactionId.IsNull())
            {
                dic.Add("transaction_id", transactionId);
            }
            else
            {
                dic.Add("out_trade_no", tradeNo);
            }
            var asciiSort = dic.ToUrl();
            //微信签名
            var sign = CryptHelper.MD5($"{asciiSort}&key={wxConfig.AppKey}").ToUpper();
            //添加签名
            dic.Add("sign", sign);
            var responseXml = HttpHelper.Post(url, dic.ToXml(), sslCertPath: sslCertPath, sslPassword: sslCertPassword);
            return XmlHelper.XmlToDictionary(responseXml).ToJson();
        }

        /// <summary>
        /// 查询退款，四个参数中选择其一即可
        /// </summary>
        /// <param name="tradeNo">商户订单号</param>
        /// <param name="transactionId">微信订单号</param>
        /// <param name="outRefundNo">商户系统内部的退款单号</param>
        /// <param name="refundId">微信生成的退款单号，在申请退款接口有返回</param>
        /// <returns>string</returns>
        public string RefundQuery(string tradeNo, string transactionId = null, string outRefundNo = null, string refundId = null)
        {
            var url = "https://api.mch.weixin.qq.com/pay/refundquery";
            //参与签名的参数字符串
            var dic = new Dictionary<string, object>
            {
                ["appid"] = wxConfig.AppId,
                ["mch_id"] = wxConfig.Mch_Id,
                ["nonce_str"] = Guid.NewGuid().BuildNonceStr()
            };
            if (tradeNo.IsNull() && transactionId.IsNull() && outRefundNo.IsNull() && refundId.IsNull())
            {
                throw new ArgumentException("四个参数不能同时为空，选择其一即可，默认为商户订单号。");
            }
            if (!tradeNo.IsNull()) dic.Add("out_trade_no", tradeNo);
            if (!transactionId.IsNull()) dic.Add("transaction_id", transactionId);
            if (!outRefundNo.IsNull()) dic.Add("out_refund_no", outRefundNo);
            if (!refundId.IsNull()) dic.Add("refund_id", refundId);
            var asciiSort = dic.ToUrl();
            //微信签名
            var sign = CryptHelper.MD5($"{asciiSort}&key={wxConfig.AppKey}").ToUpper();
            //添加签名
            dic.Add("sign", sign);
            var responseXml = HttpHelper.Post(url, dic.ToXml());
            return XmlHelper.XmlToDictionary(responseXml).ToJson();
        }

        /// <summary>
        /// 企业付款
        /// </summary>
        /// <param name="tradeNo">商户订单号</param>
        /// <param name="openId">收款用户openId</param>
        /// <param name="checkName">校验用户姓名选项[NO_CHECK：不校验真实姓名；FORCE_CHECK：强校验真实姓名（未实名认证的用户会校验失败，无法转账）；OPTION_CHECK：针对已实名认证的用户才校验真实姓名（未实名认证用户不校验，可以转账成功）]</param>
        /// <param name="reUserName">收款用户姓名[收款用户真实姓名。如果check_name设置为FORCE_CHECK或OPTION_CHECK，则必填用户真实姓名]</param>
        /// <param name="amount">企业付款金额，单位为分</param>
        /// <param name="desc">企业付款描述信息</param>
        /// <param name="ip">调用接口的机器Ip地址</param>
        /// <param name="sslCertPath">ca证书路径</param>
        /// <param name="sslCertPassword">ca证书密码</param>
        /// <returns>string</returns>
        public string QyPayment(string tradeNo, string openId, string checkName, string reUserName, int amount, string desc, string ip, string sslCertPath, string sslCertPassword)
        {
            var url = "https://api.mch.weixin.qq.com/mmpaymkttransfers/promotion/transfers";
            //随机字符串
            var nonceStr = Guid.NewGuid().BuildNonceStr();
            //参与签名的参数字符串
            var dic = new Dictionary<string, object>
            {
                ["mch_appid"] = wxConfig.AppId,
                ["mchid"] = wxConfig.Mch_Id,
                ["nonce_str"] = nonceStr,
                ["partner_trade_no"] = tradeNo,
                ["openid"] = openId,
                ["check_name"] = checkName,
                ["re_user_name"] = reUserName,
                ["amount"] = amount,
                ["desc"] = desc,
                ["spbill_create_ip"] = ip
            };
            var asciiSort = dic.ToUrl();
            //微信签名
            var sign = CryptHelper.MD5($"{asciiSort}&key={wxConfig.AppKey}").ToUpper();
            //添加签名
            dic.Add("sign", sign);
            var responseXml = HttpHelper.Post(url, dic.ToXml(), sslCertPath: sslCertPath, sslPassword: sslCertPassword);
            return XmlHelper.XmlToDictionary(responseXml).ToJson();
        }

        /// <summary>
        /// 微信刷卡支付
        /// </summary>
        /// <param name="order">微信订单</param>
        /// <param name="fn">刷卡支付结果处理委托</param>
        public void MicroPay(WxOrder order, Action<Dictionary<string, string>> fn)
        {
            var url = "https://api.mch.weixin.qq.com/pay/micropay";
            //随机字符串
            var nonce_str = Guid.NewGuid().BuildNonceStr();
            var dic = new Dictionary<string, string>
            {
                ["appid"] = wxConfig.AppId,//公众号ID或APP应用ID
                ["mch_id"] = wxConfig.Mch_Id,//商户号
                ["nonce_str"] = nonce_str,//随机字符串
                ["body"] = order.Body,//商品描述                             
                ["out_trade_no"] = order.Out_Trade_No,//商户订单号  
                ["total_fee"] = order.Total_Fee,//订单总金额 单位（分）
                ["spbill_create_ip"] = order.Spbill_Create_Ip,//订单生成的机器IP
                ["auth_code"] = order.Auth_Code//扫码支付授权码，设备读取用户微信中的条码或者二维码信息（注：用户刷卡条形码规则：18位纯数字，以10、11、12、13、14、15开头）
            };
            //ASCII排序
            var asciiSort = dic.ToUrl();
            //签名与支付签名算法一致，大写
            var sign = CryptHelper.MD5($"{asciiSort}&key={wxConfig.AppKey}").ToUpper();
            dic.Add("sign", sign);
            //发送请求       
            var responseXml = HttpHelper.Post(url, dic.ToXml());
            var dicResponse = XmlHelper.XmlToDictionary(responseXml);
            fn(dicResponse);
        }

        /// <summary>
        /// 下载对账单
        /// </summary>
        /// <param name="date">对账单日期，格式：20140603，三个月以内的账单</param>
        /// <param name="type">账单类型，ALL：返回当日所有订单信息；默认值SUCCESS：返回当日成功支付的订单；REFUND：返回当日退款订单；RECHARGE_REFUND：返回当日充值退款订单（相比其他对账单多一栏“返还手续费”）</param>
        /// <param name="isDownload">是否开启下载，默认是</param>
        /// <param name="saveDir">文件保存目录</param>
        /// <param name="fileName">文件名称，不包含扩展名，默认为当前时间生成的字符串</param>
        /// <param name="fileExt">文件扩展名</param>
        /// <returns>不开启下载，直接返回账单数据；开启下载，返回下载后的gzip文件路径</returns>
        public string DownloadBill(string date, string type = "ALL", bool isDownload = true, string saveDir = @"Files\", string fileName = null, string fileExt = ".gzip")
        {
            var url = "https://api.mch.weixin.qq.com/pay/downloadbill";
            var dic = new Dictionary<string, string>
            {
                ["appid"] = wxConfig.AppId,
                ["mch_id"] = wxConfig.Mch_Id,
                ["nonce_str"] = Guid.NewGuid().BuildNonceStr(),
                ["bill_date"] = date,
                ["bill_type"] = type
            };
            if (isDownload) dic.Add("tar_type", "GZIP");
            //ASCII排序
            var asciiSort = dic.ToUrl();
            //签名与支付签名算法一致，大写
            var sign = CryptHelper.MD5($"{asciiSort}&key={wxConfig.AppKey}").ToUpper();
            dic.Add("sign", sign);
            var xml = dic.ToXml();
            if (isDownload)
            {
                if (fileName.IsNull()) fileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                HttpHelper.Download(url, xml, saveDir: saveDir, fileExt: fileExt, fileName: fileName);
                return AppDomain.CurrentDomain.BaseDirectory + saveDir + fileName + fileExt;
            }
            else
            {
                return HttpHelper.Post(url, xml);
            }
        }
        #endregion

        #region 用户管理
        /// <summary>
        /// 获取公众号对应的openId
        /// </summary>
        /// <param name="code">授权码</param>
        /// <returns>string</returns>
        public string GetOpenId(string code)
        {
            var openId = string.Empty;
            try
            {
                var url = $"https://api.weixin.qq.com/sns/oauth2/access_token?appid={wxConfig.AppId}&secret={wxConfig.AppSecret}&code={code}&grant_type=authorization_code";
                var res = HttpHelper.Post(url, string.Empty);
                if (!string.IsNullOrEmpty(res))
                {
                    var obj = res.ToObject<OAuthModel>();
                    openId = obj?.OpenId;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "获取公众号对应的openId");
            }
            return openId;
        }

        /// <summary>
        /// 根据openId获取用户信息（已关注该公众号）
        /// </summary>
        /// <param name="openId">微信openId</param>
        /// <returns>string</returns>
        public string GetUserInfo(string openId)
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/user/info?access_token={accessToken}&openid={openId}&lang=zh_CN";
            return HttpHelper.Get(url);
        }

        /// <summary>
        /// 须用户授权同意，获取用户信息
        /// </summary>
        /// <param name="code">授权码</param>
        /// <param name="fnOAuthModel">对授权返回数据处理函数</param>
        /// <returns>WxUserInfo</returns>
        public WxUserInfo GetUserInfoByAuthorize(string code, Action<OAuthModel> fnOAuthModel = null)
        {
            var user = new WxUserInfo();
            try
            {
                var url = $"https://api.weixin.qq.com/sns/oauth2/access_token?appid={wxConfig.AppId}&secret={wxConfig.AppSecret}&code={code}&grant_type=authorization_code";
                var res = HttpHelper.Post(url, string.Empty);
                if (!string.IsNullOrEmpty(res))
                {
                    var obj = res.ToObject<OAuthModel>();
                    if (obj != null)
                    {
                        fnOAuthModel?.Invoke(obj);
                        //根据临时access_token和openid获取用户信息
                        var link = $"https://api.weixin.qq.com/sns/userinfo?access_token={obj.Access_Token}&openid={obj.OpenId}&lang=zh_CN";
                        var r = HttpHelper.Post(link, string.Empty);
                        if (!string.IsNullOrEmpty(r)) user = r.ToObject<WxUserInfo>();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "须用户授权同意，获取用户信息");
            }
            return user;
        }

        /// <summary>
        /// 由于access_token有效期只有2个小时，refresh_token有效期30天，无需用户短时间内再次授权，因此可以通过refresh_token获取用户信息
        /// </summary>
        /// <param name="refreshToken">用户刷新access_token所用到的token</param>
        /// <param name="fnOAuthModel">对授权返回数据处理函数</param>
        /// <returns>WxUserInfo</returns>
        public WxUserInfo GetUserInfoByRefreshToken(string refreshToken, Action<OAuthModel> fnOAuthModel = null)
        {
            var user = new WxUserInfo();
            try
            {
                var url = $"https://api.weixin.qq.com/sns/oauth2/refresh_token?appid={wxConfig.AppId}&grant_type=refresh_token&refresh_token={refreshToken}";
                var obj = HttpHelper.Post(url, string.Empty).ToObject<OAuthModel>();
                if (obj != null)
                {
                    fnOAuthModel?.Invoke(obj);
                    //根据临时access_token和openid获取用户信息
                    var link = $"https://api.weixin.qq.com/sns/userinfo?access_token={obj.Access_Token}&openid={obj.OpenId}&lang=zh_CN";
                    var res = HttpHelper.Post(link, string.Empty);
                    if (!string.IsNullOrEmpty(res)) user = res.ToObject<WxUserInfo>();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "通过refresh_token获取用户信息");
            }
            return user;
        }

        /// <summary>
        /// 批量获取用户基本信息(最多支持一次拉取100条)
        /// </summary>
        /// <param name="postJson">post数据</param>
        /// <returns>string</returns>
        public string BatchGetUserInfo(string postJson)
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/user/info/batchget?access_token={accessToken}";
            return HttpHelper.Post(url, postJson);
        }

        /// <summary>
        /// 创建标签
        /// </summary>
        /// <param name="name">标签名称</param>
        /// <returns>string</returns>
        public string CreateTag(string name)
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/tags/create?access_token={accessToken}";
            var postJson = $"{{\"tag\":{{\"name\":\"{name}\"}}}}";
            return HttpHelper.Post(url, postJson);
        }

        /// <summary>
        /// 编辑标签
        /// </summary>
        /// <param name="id">标签id</param>
        /// <param name="name">标签名称</param>
        /// <returns>string</returns>
        public string UpdateTag(int id, string name)
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/tags/update?access_token={accessToken}";
            var postJson = $"{{\"tag\":{{\"id\":{id},\"name\":\"{name}\"}}}}";
            return HttpHelper.Post(url, postJson);
        }

        /// <summary>
        /// 删除标签
        /// </summary>
        /// <param name="id">标签id</param>
        /// <returns>string</returns>
        public string DeleteTag(int id)
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/tags/delete?access_token={accessToken}";
            var postJson = $"{{\"tag\":{{\"id\":{id}}}}}";
            return HttpHelper.Post(url, postJson);
        }

        /// <summary>
        /// 获取公众号已创建的标签
        /// </summary>
        /// <returns>string</returns>
        public string GetAllTag()
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/tags/get?access_token={accessToken}";
            return HttpHelper.Get(url);
        }

        /// <summary>
        /// 获取用户身上的标签列表
        /// </summary>
        /// <param name="openId">用户openId</param>
        /// <returns>string</returns>
        public string GetUserTag(string openId)
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/tags/getidlist?access_token={accessToken}";
            var postJson = $"{{\"openid\":\"{openId}\"}}";
            return HttpHelper.Post(url, postJson);
        }

        /// <summary>
        /// 获取标签下粉丝列表
        /// </summary>
        /// <param name="id">标签id</param>
        /// <param name="nextOpenId">第一个拉取的openid，不填默认从头开始拉取</param>
        /// <returns></returns>
        public string GetUserByTag(int id, string nextOpenId = "")
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/user/tag/get?access_token={accessToken}";
            var postJson = $"{{\"tagid\":{id},\"next_openid\":\"{nextOpenId}\"}}";
            return HttpHelper.Post(url, postJson);
        }

        /// <summary>
        /// 批量为用户打标签
        /// </summary>
        /// <param name="listOpenId">用户openId列表</param>
        /// <param name="id">标签id</param>
        /// <returns>string</returns>
        public string BatchSetTag(List<string> listOpenId, int id)
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/tags/members/batchtagging?access_token={accessToken}";
            var postJson = $"{{\"openid_list\":{listOpenId.ToJson()},\"tagid\":{id}}}";
            return HttpHelper.Post(url, postJson);
        }

        /// <summary>
        /// 批量为用户取消标签
        /// </summary>
        /// <param name="listOpenId">用户openId列表</param>
        /// <param name="id">标签id</param>
        /// <returns>string</returns>
        public string BatchCancelTag(List<string> listOpenId, int id)
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/tags/members/batchuntagging?access_token={accessToken}";
            var postJson = $"{{\"openid_list\":{listOpenId.ToJson()},\"tagid\":{id}}}";
            return HttpHelper.Post(url, postJson);
        }

        /// <summary>
        /// 设置用户备注
        /// </summary>
        /// <param name="openId">用户openId</param>
        /// <param name="remark">备注内容</param>
        /// <returns>string</returns>
        public string SetUserRemark(string openId, string remark)
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/user/info/updateremark?access_token={accessToken}";
            var postJson = $"{{\"openid\":\"{openId}\",\"remark\":\"{remark}\"}}";
            return HttpHelper.Post(url, postJson);
        }

        /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <param name="nextOpenId">默认为空，从头读取最多10000个用户</param>
        /// <returns>string</returns>
        public string GetUserList(string nextOpenId = "")
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/user/get?access_token={accessToken}&next_openid={nextOpenId}";
            return HttpHelper.Get(url);
        }
        #endregion

        #region 消息管理
        /// <summary>
        /// 获取微信模板Id(此模版消息Id直接可以在公众平台获取)
        /// </summary>
        /// <param name="templateNo">模板编号</param>
        /// <returns>string</returns>
        public string GetTemplateId(string templateNo)
        {
            var result = string.Empty;
            var wxUrl = $"https://api.weixin.qq.com/cgi-bin/template/api_add_template?access_token={accessToken}";
            var response = HttpHelper.Post(wxUrl, $"{{\"template_id_short\":\"{templateNo}\"}}");
            var jo = response.ToJObject();
            if (jo != null && jo.Value<string>("errmsg").Contains("ok"))
            {
                result = jo.Value<string>("template_id");
            }
            else
            {
                LogHelper.Info($"获取微信模板Id失败，返回信息：{response}");
            }
            return result;
        }

        /// <summary>
        /// 发送模板消息
        /// </summary>
        /// <param name="jsonContent">消息内容</param>
        /// <returns>string</returns>
        public string SendTemplateMsg(string jsonContent)
        {
            var wxUrl = $"https://api.weixin.qq.com/cgi-bin/message/template/send?access_token={accessToken}";
            return HttpHelper.Post(wxUrl, jsonContent);
        }

        /// <summary>
        /// 发送客服消息
        /// </summary>
        /// <param name="msgJson">消息json字符串</param>
        /// <returns>string</returns>
        public string SendCustomerServiceMsg(string msgJson)
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/message/custom/send?access_token={accessToken}";
            return HttpHelper.Post(url, msgJson);
        }

        /// <summary>
        /// 根据OpenID列表群发【订阅号不可用，服务号认证后可用】
        /// </summary>
        /// <param name="msgContent">post消息内容</param>
        /// <returns>string</returns>
        public string SendMsgOfOpenIdList(string msgContent)
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/message/mass/send?access_token={accessToken}";
            return HttpHelper.Post(url, msgContent);
        }

        /// <summary>
        /// 根据分组进行群发【订阅号与服务号认证后均可用】
        /// </summary>
        /// <param name="msgContent">post消息内容</param>
        /// <returns>string</returns>
        public string SendMsgOfGroup(string msgContent)
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/message/mass/sendall?access_token={accessToken}";
            return HttpHelper.Post(url, msgContent);
        }

        /// <summary>
        /// 删除群发消息(由于技术限制，群发只有在刚发出的半小时内可以删除，发出半小时之后将无法被删除)
        /// </summary>
        /// <param name="msgId">消息Id</param>
        /// <returns>string</returns>
        public string DeleteMsgOfGroup(string msgId)
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/message/mass/delete?access_token={accessToken}";
            return HttpHelper.Post(url, $"{{\"msg_id\":\"{msgId}\"}}");
        }

        /// <summary>
        /// 预览群发接口，每日调用次数有限制（100次）
        /// </summary>
        /// <param name="msgJson">图文消息json内容</param>
        /// <returns>string</returns>
        public string PreviewMsgOfGroup(string msgJson)
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/message/mass/preview?access_token={accessToken}";
            return HttpHelper.Post(url, msgJson);
        }

        /// <summary>
        /// 查询群发消息发送状态
        /// </summary>
        /// <param name="msgId">消息Id</param>
        /// <returns>string</returns>
        public string SearchMsgState(string msgId)
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/message/mass/get?access_token={accessToken}";
            return HttpHelper.Post(url, $"{{\"msg_id\": \"{msgId}\"}}");
        }

        /// <summary>
        /// 获取自动回复规则
        /// </summary>
        /// <returns>string</returns>
        public string GetCurrentAutoReplyInfo()
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/get_current_autoreply_info?access_token={accessToken}";
            return HttpHelper.Get(url);
        }
        #endregion

        #region 素材管理
        /// <summary>
        /// 新增临时素材
        /// </summary>
        /// <param name="type">素材类型</param>
        /// <param name="filePath">源文件路径</param>
        /// <returns>string</returns>
        public string AddTempMaterial(string type, string filePath)
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/media/upload?access_token={accessToken}&type={type}";
            return HttpHelper.Upload(url, filePath);
        }

        /// <summary>
        /// 获取临时素材
        /// </summary>
        /// <param name="mediaId">多媒体文件Id</param>
        /// <param name="filePath">文件路径</param>
        /// <param name="fileExt">文件扩展名</param>
        /// <param name="fileName">文件名，不包含扩展名</param>
        public void GetTempMaterial(string mediaId, string filePath, string fileExt, string fileName)
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/media/get?access_token={accessToken}&media_id={mediaId}";
            HttpHelper.Download(url, saveDir: filePath, fileExt: fileExt, fileName: fileName);
        }

        /// <summary>
        /// 新增永久图文素材
        /// </summary>
        /// <param name="imgTxtJson">图文消息json字符串内容</param>
        /// <returns>string</returns>
        public string AddForeverImgTxtMaterial(string imgTxtJson)
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/material/add_news?access_token={accessToken}";
            return HttpHelper.Post(url, imgTxtJson);
        }

        /// <summary>
        /// 修改永久图文素材
        /// </summary>
        /// <param name="imgTxtJson">图文消息json字符串内容</param>
        /// <returns>string</returns>
        public string UpdateForeverImgTxtMaterial(string imgTxtJson)
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/material/update_news?access_token={accessToken}";
            return HttpHelper.Post(url, imgTxtJson);
        }

        /// <summary>
        /// 上传永久图文素材中用到的图片/图文消息中图片（本接口所上传的图片不占用公众号的素材库中图片数量的5000个的限制。图片仅支持jpg/png格式，大小必须在1MB以下）
        /// </summary>
        /// <param name="imgPath">图片路径</param>
        /// <returns>string</returns>
        public string UploadForeverImgTxtMaterialUsedImg(string imgPath)
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/media/uploadimg?access_token={accessToken}";
            return HttpHelper.Upload(url, imgPath);
        }

        /// <summary>
        /// 新增其他类型永久素材
        /// </summary>
        /// <param name="type">素材类型</param>
        /// <param name="filePath">文件路径</param>
        /// <returns>string</returns>
        public string AddOtherForeverMaterial(string type, string filePath)
        {
            string result = string.Empty;
            var url = $"https://api.weixin.qq.com/cgi-bin/material/add_material?access_token={accessToken}&type={type}";
            if (type != "video")
            {
                result = HttpHelper.Upload(url, filePath, formName: "media");
            }
            else
            {
                //新增永久视频素材
                var postData = $"{{\"title\":\"{filePath.Substring(".")}\",\"introduction\":\"永久视频素材\"}}";
                var fileDic = new Dictionary<string, string> { ["description"] = filePath };
                result = HttpHelper.Upload(url, postData, fileDic);
            }
            return result;
        }

        /// <summary>
        /// 获取永久素材(不包含其他类型永久素材)
        /// </summary>
        /// <param name="mediaId">多媒体文件Id</param>
        /// <returns>string</returns>
        public string GetForeverMaterial(string mediaId)
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/material/get_material?access_token={accessToken}";
            return HttpHelper.Post(url, $"{{\"media_id\":\"{mediaId}\"}}");
        }

        /// <summary>
        /// 获取其他类型永久素材(保存为文件)
        /// </summary>
        /// <param name="mediaId">多媒体文件Id</param>
        /// <param name="filePath">保存文件路径</param>
        /// <param name="fileExt">文件扩展名</param>
        /// <param name="fileName">文件名，不包含扩展名</param>
        public void GetOtherForeverMaterial(string mediaId, string filePath, string fileExt, string fileName)
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/material/get_material?access_token={accessToken}";
            HttpHelper.Download(url, $"{{\"media_id\":\"{mediaId}\"}}", filePath, fileExt, fileName);
        }

        /// <summary>
        /// 删除永久素材
        /// </summary>
        /// <param name="mediaId">多媒体文件Id</param>
        /// <returns>string</returns>
        public string DeleteForeverMaterial(string mediaId)
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/material/del_material?access_token={accessToken}";
            return HttpHelper.Post(url, $"{{\"media_id\":\"{mediaId}\"}}");
        }

        /// <summary>
        /// 获取永久素材总数
        /// </summary>
        /// <returns>string</returns>
        public string GetForeverMaterialCount()
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/material/get_materialcount?access_token={accessToken}";
            return HttpHelper.Get(url);
        }

        /// <summary>
        /// 获取永久素材列表
        /// </summary>
        /// <param name="postJson">post数据</param>
        /// <returns>string</returns>
        public string GetForeverMaterialList(string postJson)
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/material/batchget_material?access_token={accessToken}";
            return HttpHelper.Post(url, postJson);
        }

        /// <summary>
        /// 上传图文消息素材(群发图文消息中用到)
        /// </summary>
        /// <param name="imgTextJson">图文消息json字符串</param>
        /// <returns>string</returns>
        public string UploadImgTxtMaterial(string imgTextJson)
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/media/uploadnews?access_token={accessToken}";
            return HttpHelper.Post(url, imgTextJson);
        }

        /// <summary>
        /// 获取视频(图文消息)消息的MediaId
        /// </summary>
        /// <param name="postData">post数据</param>
        /// <returns>string</returns>
        public string GetVideoMediaIdOfImgTxt(string postData)
        {
            var url = $"https://file.api.weixin.qq.com/cgi-bin/media/uploadvideo?access_token={accessToken}";
            return HttpHelper.Post(url, postData);
        }
        #endregion

        #region 自定义菜单管理
        /// <summary>
        /// 创建自定义菜单
        /// </summary>
        /// <param name="menuJson">菜单json字符串</param>
        /// <returns>string</returns>
        public string CreateMenu(string menuJson)
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/menu/create?access_token={accessToken}";
            return HttpHelper.Post(url, menuJson);
        }

        /// <summary>
        /// 创建个性化菜单
        /// </summary>
        /// <param name="menuJson">菜单json字符串</param>
        /// <returns>string</returns>
        public string CreatePersonalMenu(string menuJson)
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/menu/addconditional?access_token={accessToken}";
            return HttpHelper.Post(url, menuJson);
        }

        /// <summary>
        /// 获取接口自定义菜单json字符串
        /// </summary>
        /// <returns>string</returns>
        public string GetMenuJson()
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/menu/get?access_token={accessToken}";
            return HttpHelper.Get(url);
        }

        /// <summary>
        /// 获取通用（公众平台/接口自定义）菜单信息
        /// </summary>
        /// <returns>string</returns>
        public string GetMenuApiJson()
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/get_current_selfmenu_info?access_token={accessToken}";
            return HttpHelper.Get(url);
        }

        /// <summary>
        /// 删除自定义菜单
        /// </summary>
        /// <returns>string</returns>
        public string DeleteMenu()
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/menu/delete?access_token={accessToken}";
            return HttpHelper.Get(url);
        }

        /// <summary>
        /// 删除个性化菜单
        /// </summary>
        /// <param name="menuId">菜单Id</param>
        /// <returns>string</returns>
        public string DeletePersonalMenu(string menuId)
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/menu/delconditional?access_token={accessToken}";
            return HttpHelper.Post(url, $"{{\"menuid\":\"{menuId}\"}}");
        }
        #endregion

        #region 微信多客服
        /// <summary>
        /// 获取客服基本信息
        /// </summary>
        /// <returns>string</returns>
        public string GetKFList()
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/customservice/getkflist?access_token={accessToken}";
            return HttpHelper.Get(url);
        }

        /// <summary>
        /// 获取在线客服接待信息
        /// </summary>
        /// <returns>string</returns>
        public string GetOnLineKFList()
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/customservice/getonlinekflist?access_token={accessToken}";
            return HttpHelper.Get(url);
        }

        /// <summary>
        /// 添加客服账号(每个公众号最多添加100个客服账号)
        /// </summary>
        /// <param name="postJson">客服json字符串</param>
        /// <returns>string</returns>
        public string AddKFAccount(string postJson)
        {
            var url = $"https://api.weixin.qq.com/customservice/kfaccount/add?access_token={accessToken}";
            return HttpHelper.Post(url, postJson);
        }

        /// <summary>
        /// 设置客服信息
        /// </summary>
        /// <param name="postJson">客服json字符串</param>
        /// <returns>string</returns>
        public string UpdateKFAccount(string postJson)
        {
            var url = $"https://api.weixin.qq.com/customservice/kfaccount/update?access_token={accessToken}";
            return HttpHelper.Post(url, postJson);
        }

        /// <summary>
        /// 上传客服头像(必须是jpg格式，推荐640*640大小)
        /// </summary>
        /// <param name="kfAccount">完整客服账号</param>
        /// <param name="filePath">头像路径</param>
        /// <returns>string</returns>
        public string UploadKFHeadImg(string kfAccount, string filePath)
        {
            var url = $"https://api.weixin.qq.com/customservice/kfaccount/uploadheadimg?access_token={accessToken}&kf_account={kfAccount}";
            return HttpHelper.Upload(url, filePath);
        }

        /// <summary>
        /// 删除客服账号
        /// </summary>
        /// <param name="kfAccount">完整客服账号</param>
        /// <returns>string</returns>
        public string DeleteKFAccount(string kfAccount)
        {
            var url = $"https://api.weixin.qq.com/customservice/kfaccount/del?access_token={accessToken}&kf_account={kfAccount}";
            return HttpHelper.Get(url);
        }

        /// <summary>
        /// 为在线客服创建会话
        /// </summary>
        /// <param name="postJson">json字符串</param>
        /// <returns>string</returns>
        public string CreateKFSession(string postJson)
        {
            var url = $"https://api.weixin.qq.com/customservice/kfsession/create?access_token={accessToken}";
            return HttpHelper.Post(url, postJson);
        }

        /// <summary>
        /// 关闭会话
        /// </summary>
        /// <param name="postJson">json字符串</param>
        /// <returns>string</returns>
        public string CloseKFSession(string postJson)
        {
            var url = $"https://api.weixin.qq.com/customservice/kfsession/close?access_token={accessToken}";
            return HttpHelper.Post(url, postJson);
        }

        /// <summary>
        /// 获取客户的会话状态
        /// </summary>
        /// <param name="openId">客户openId</param>
        /// <returns>string</returns>
        public string GetCustomerSeeion(string openId)
        {
            var url = $"https://api.weixin.qq.com/customservice/kfsession/getsession?access_token={accessToken}&openid={openId}";
            return HttpHelper.Get(url);
        }

        /// <summary>
        /// 获取客服的会话列表
        /// </summary>
        /// <param name="kfAccount">完整客服账号</param>
        /// <returns>string</returns>
        public string GetKFSessionList(string kfAccount)
        {
            var url = $"https://api.weixin.qq.com/customservice/kfsession/getsessionlist?access_token={accessToken}&kf_account={kfAccount}";
            return HttpHelper.Get(url);
        }

        /// <summary>
        /// 获取未接入会话列表
        /// </summary>
        /// <returns>string</returns>
        public string GetWaitCaseSessionList()
        {
            var url = $"https://api.weixin.qq.com/customservice/kfsession/getwaitcase?access_token={accessToken}";
            return HttpHelper.Get(url);
        }

        /// <summary>
        /// 获取客服聊天记录接口
        /// </summary>
        /// <param name="postJson">json字符串</param>
        /// <returns>string</returns>
        public string GetKFMsgRecord(string postJson)
        {
            var url = $"https://api.weixin.qq.com/customservice/msgrecord/getrecord?access_token={accessToken}";
            return HttpHelper.Post(url, postJson);
        }
        #endregion

        #region 微信卡券
        /// <summary>
        /// 获取卡券原生code
        /// </summary>
        /// <param name="encryptCode">卡券加密code</param>
        /// <returns>string</returns>
        public string GetDecryptCode(string encryptCode)
        {
            var code = string.Empty;
            var decryptUrl = $"https://api.weixin.qq.com/card/code/decrypt?access_token={accessToken}";
            var response = HttpHelper.Post(decryptUrl, $"{{\"encrypt_code\":\"{encryptCode}\"}}");
            var jo = response.ToJObject();
            if (jo != null)
            {
                var errmsg = jo.Value<string>("errmsg");
                if (errmsg.Equals("ok"))
                {
                    code = jo.Value<string>("code");
                }
                else
                {
                    LogHelper.Info($"获取卡券原生code失败，返回信息：{response}");
                }
            }
            return code;
        }

        /// <summary>
        /// 核销卡券
        /// </summary>
        /// <param name="code">原生卡券code</param>
        /// <param name="cardId">卡券id</param>
        /// <param name="isUseCustomCode">是否是自定义code(默认true)</param>
        /// <returns>string</returns>
        public string DestroyCode(string code, string cardId, bool isUseCustomCode = true)
        {
            var wxUrl = $"https://api.weixin.qq.com/card/code/consume?access_token={accessToken}";
            var param = new StringBuilder();
            if (isUseCustomCode)
            {
                param.Append("{")
                     .Append($"\"code\":\"{code}\",")
                     .Append($"\"card_id\":\"{cardId}\"")
                     .Append("}");
            }
            else
            {
                param.Append("{")
                     .Append($"\"code\":\"{code}\"")
                     .Append("}");
            }
            return HttpHelper.Post(wxUrl, param.ToString());
        }
        #endregion

        #region 接口清零
        /// <summary>
        /// 每个帐号每月共10次清零操作机会
        /// </summary>
        /// <returns>string</returns>
        public string ClearQuota()
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/clear_quota?access_token={accessToken}";
            return HttpHelper.Post(url, $"{{\"appid\":\"{wxConfig.AppId}\"}}");
        }
        #endregion

        #region 数据统计
        /// <summary>
        /// 获取用户增减数据（7天）
        /// </summary>
        /// <param name="beginDate">获取数据的起始日期</param>
        /// <param name="endDate">获取数据的结束日期，最大为昨日</param>
        /// <returns>string</returns>
        public string GetUserSummary(string beginDate, string endDate)
        {
            var url = $"https://api.weixin.qq.com/datacube/getusersummary?access_token={accessToken}";
            return HttpHelper.Post(url, $"{{ \"begin_date\": \"{beginDate}\",\"end_date\": \"{endDate}\"}}");
        }

        /// <summary>
        /// 获取累计用户数据（7天）
        /// </summary>
        /// <param name="beginDate">获取数据的起始日期</param>
        /// <param name="endDate">获取数据的结束日期，最大为昨日</param>
        /// <returns>string</returns>
        public string GetUserCumulate(string beginDate, string endDate)
        {
            var url = $"https://api.weixin.qq.com/datacube/getusercumulate?access_token={accessToken}";
            return HttpHelper.Post(url, $"{{ \"begin_date\": \"{beginDate}\",\"end_date\": \"{endDate}\"}}");
        }

        /// <summary>
        /// 获取图文群发每日数据（1天）
        /// </summary>
        /// <param name="beginDate">获取数据的起始日期</param>
        /// <param name="endDate">获取数据的结束日期，最大为昨日</param>
        /// <returns>string</returns>
        public string GetArticleSummary(string beginDate, string endDate)
        {
            var url = $"https://api.weixin.qq.com/datacube/getarticlesummary?access_token={accessToken}";
            return HttpHelper.Post(url, $"{{ \"begin_date\": \"{beginDate}\",\"end_date\": \"{endDate}\"}}");
        }

        /// <summary>
        /// 获取图文群发总数据（1天）
        /// </summary>
        /// <param name="beginDate">获取数据的起始日期</param>
        /// <param name="endDate">获取数据的结束日期，最大为昨日</param>
        /// <returns>string</returns>
        public string GetArticleTotal(string beginDate, string endDate)
        {
            var url = $"https://api.weixin.qq.com/datacube/getarticletotal?access_token={accessToken}";
            return HttpHelper.Post(url, $"{{ \"begin_date\": \"{beginDate}\",\"end_date\": \"{endDate}\"}}");
        }

        /// <summary>
        /// 获取图文统计数据（3天）
        /// </summary>
        /// <param name="beginDate">获取数据的起始日期</param>
        /// <param name="endDate">获取数据的结束日期，最大为昨日</param>
        /// <returns>string</returns>
        public string GetUserRead(string beginDate, string endDate)
        {
            var url = $"https://api.weixin.qq.com/datacube/getuserread?access_token={accessToken}";
            return HttpHelper.Post(url, $"{{ \"begin_date\": \"{beginDate}\",\"end_date\": \"{endDate}\"}}");
        }

        /// <summary>
        /// 获取图文统计分时数据（1天）
        /// </summary>
        /// <param name="beginDate">获取数据的起始日期</param>
        /// <param name="endDate">获取数据的结束日期，最大为昨日</param>
        /// <returns>string</returns>
        public string GetUserReadHour(string beginDate, string endDate)
        {
            var url = $"https://api.weixin.qq.com/datacube/getuserreadhour?access_token={accessToken}";
            return HttpHelper.Post(url, $"{{ \"begin_date\": \"{beginDate}\",\"end_date\": \"{endDate}\"}}");
        }

        /// <summary>
        /// 获取图文分享转发数据（7天）
        /// </summary>
        /// <param name="beginDate">获取数据的起始日期</param>
        /// <param name="endDate">获取数据的结束日期，最大为昨日</param>
        /// <returns>string</returns>
        public string GetUserShare(string beginDate, string endDate)
        {
            var url = $"https://api.weixin.qq.com/datacube/getusershare?access_token={accessToken}";
            return HttpHelper.Post(url, $"{{ \"begin_date\": \"{beginDate}\",\"end_date\": \"{endDate}\"}}");
        }

        /// <summary>
        /// 获取图文分享转发分时数据（1天）
        /// </summary>
        /// <param name="beginDate">获取数据的起始日期</param>
        /// <param name="endDate">获取数据的结束日期，最大为昨日</param>
        /// <returns>string</returns>
        public string GetUserShareHour(string beginDate, string endDate)
        {
            var url = $"https://api.weixin.qq.com/datacube/getusersharehour?access_token={accessToken}";
            return HttpHelper.Post(url, $"{{ \"begin_date\": \"{beginDate}\",\"end_date\": \"{endDate}\"}}");
        }

        /// <summary>
        /// 获取消息发送概况数据（7天）
        /// </summary>
        /// <param name="beginDate">获取数据的起始日期</param>
        /// <param name="endDate">获取数据的结束日期，最大为昨日</param>
        /// <returns>string</returns>
        public string GetUpStreamMsg(string beginDate, string endDate)
        {
            var url = $"https://api.weixin.qq.com/datacube/getupstreammsg?access_token={accessToken}";
            return HttpHelper.Post(url, $"{{ \"begin_date\": \"{beginDate}\",\"end_date\": \"{endDate}\"}}");
        }

        /// <summary>
        /// 获取消息分送分时数据（1天）
        /// </summary>
        /// <param name="beginDate">获取数据的起始日期</param>
        /// <param name="endDate">获取数据的结束日期，最大为昨日</param>
        /// <returns>string</returns>
        public string GetUpStreamMsgHour(string beginDate, string endDate)
        {
            var url = $"https://api.weixin.qq.com/datacube/getupstreammsghour?access_token={accessToken}";
            return HttpHelper.Post(url, $"{{ \"begin_date\": \"{beginDate}\",\"end_date\": \"{endDate}\"}}");
        }

        /// <summary>
        /// 获取消息发送周数据（30天）
        /// </summary>
        /// <param name="beginDate">获取数据的起始日期</param>
        /// <param name="endDate">获取数据的结束日期，最大为昨日</param>
        /// <returns>string</returns>
        public string GetUpStreamMsgWeek(string beginDate, string endDate)
        {
            var url = $"https://api.weixin.qq.com/datacube/getupstreammsgweek?access_token={accessToken}";
            return HttpHelper.Post(url, $"{{ \"begin_date\": \"{beginDate}\",\"end_date\": \"{endDate}\"}}");
        }

        /// <summary>
        /// 获取消息发送月数据（30天）
        /// </summary>
        /// <param name="beginDate">获取数据的起始日期</param>
        /// <param name="endDate">获取数据的结束日期，最大为昨日</param>
        /// <returns>string</returns>
        public string GetUpStreamMsgMonth(string beginDate, string endDate)
        {
            var url = $"https://api.weixin.qq.com/datacube/getupstreammsgmonth?access_token={accessToken}";
            return HttpHelper.Post(url, $"{{ \"begin_date\": \"{beginDate}\",\"end_date\": \"{endDate}\"}}");
        }

        /// <summary>
        /// 获取消息发送分布数据（15天）
        /// </summary>
        /// <param name="beginDate">获取数据的起始日期</param>
        /// <param name="endDate">获取数据的结束日期，最大为昨日</param>
        /// <returns>string</returns>
        public string GetUpStreamMsgDist(string beginDate, string endDate)
        {
            var url = $"https://api.weixin.qq.com/datacube/getupstreammsgdist?access_token={accessToken}";
            return HttpHelper.Post(url, $"{{ \"begin_date\": \"{beginDate}\",\"end_date\": \"{endDate}\"}}");
        }

        /// <summary>
        /// 获取消息发送分布周数据（30天）
        /// </summary>
        /// <param name="beginDate">获取数据的起始日期</param>
        /// <param name="endDate">获取数据的结束日期，最大为昨日</param>
        /// <returns>string</returns>
        public string GetUpStreamMsgDistWeek(string beginDate, string endDate)
        {
            var url = $"https://api.weixin.qq.com/datacube/getupstreammsgdistweek?access_token={accessToken}";
            return HttpHelper.Post(url, $"{{ \"begin_date\": \"{beginDate}\",\"end_date\": \"{endDate}\"}}");
        }

        /// <summary>
        /// 获取消息发送分布月数据（30天）
        /// </summary>
        /// <param name="beginDate">获取数据的起始日期</param>
        /// <param name="endDate">获取数据的结束日期，最大为昨日</param>
        /// <returns>string</returns>
        public string GetUpStreamMsgDistMonth(string beginDate, string endDate)
        {
            var url = $"https://api.weixin.qq.com/datacube/getupstreammsgdistmonth?access_token={accessToken}";
            return HttpHelper.Post(url, $"{{ \"begin_date\": \"{beginDate}\",\"end_date\": \"{endDate}\"}}");
        }

        /// <summary>
        /// 获取接口分析数据（30天）
        /// </summary>
        /// <param name="beginDate">获取数据的起始日期</param>
        /// <param name="endDate">获取数据的结束日期，最大为昨日</param>
        /// <returns>string</returns>
        public string GetInterfaceSummary(string beginDate, string endDate)
        {
            var url = $"https://api.weixin.qq.com/datacube/getinterfacesummary?access_token={accessToken}";
            return HttpHelper.Post(url, $"{{ \"begin_date\": \"{beginDate}\",\"end_date\": \"{endDate}\"}}");
        }

        /// <summary>
        /// 获取接口分析分时数据（1天）
        /// </summary>
        /// <param name="beginDate">获取数据的起始日期</param>
        /// <param name="endDate">获取数据的结束日期，最大为昨日</param>
        /// <returns>string</returns>
        public string GetInterfaceSummaryHour(string beginDate, string endDate)
        {
            var url = $"https://api.weixin.qq.com/datacube/getinterfacesummaryhour?access_token={accessToken}";
            return HttpHelper.Post(url, $"{{ \"begin_date\": \"{beginDate}\",\"end_date\": \"{endDate}\"}}");
        }
        #endregion
        #endregion

        #region 微信企业号
        #region 获取企业号AccessToken
        /// <summary>
        /// 企业号令牌 Access_Token
        /// </summary>
        /// <returns>string</returns>
        public string GetQyAccessToken()
        {
            var accessToken = string.Empty;
            try
            {
                var url = $"https://qyapi.weixin.qq.com/cgi-bin/gettoken?corpid=wxConfig.appid&corpsecret={wxConfig.AppSecret}";
                var res = HttpHelper.Get(url);
                if (!string.IsNullOrEmpty(res))
                {
                    var obj = res.ToObject<AccessToken>();
                    accessToken = obj?.Access_Token;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "企业号令牌 Access_Token");
            }
            return accessToken;
        }
        #endregion

        #region 获取企业号JsApiTicket
        /// <summary>
        /// 企业号jsapi_ticket
        /// </summary>
        /// <returns>string</returns>
        public string GetQyJsApiTicket()
        {
            var ticket = string.Empty;
            try
            {
                var url = $"https://qyapi.weixin.qq.com/cgi-bin/get_jsapi_ticket?access_token={accessToken}";
                var res = HttpHelper.Get(url);
                if (!string.IsNullOrEmpty(res))
                {
                    var obj = res.ToObject<JsApiTicket>();
                    ticket = obj?.Ticket;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "企业号jsapi_ticket");
            }
            return ticket;
        }
        #endregion

        #region 用户管理
        /// <summary>
        /// 获取企业用户信息
        /// </summary>
        /// <param name="code">企业code(与公众号)</param>
        /// <returns>string</returns>
        public string GetQyUserInfo(string code)
        {
            var url = $"https://qyapi.weixin.qq.com/cgi-bin/user/getuserinfo?access_token={accessToken}&code={code}";
            return HttpHelper.Post(url, string.Empty);
        }

        /// <summary>
        /// userid转换成openid
        /// </summary>
        /// <param name="userId">企业号内的成员id</param>
        /// <param name="agentId">企业应用的id</param>
        /// <returns>string</returns>
        public string UserIdToOpenId(string userId, string agentId = "")
        {
            var url = $"https://qyapi.weixin.qq.com/cgi-bin/user/convert_to_openid?access_token={accessToken}";
            var postJson = string.Empty;
            if (!string.IsNullOrEmpty(agentId))
            {
                postJson = $"{{\"userid\":\"{userId}\",\"agentid\":{agentId}}}";
            }
            else
            {
                postJson = $"{{\"userid\":\"{userId}\"}}";
            }
            return HttpHelper.Post(url, postJson);
        }

        /// <summary>
        /// openid转换成userid
        /// </summary>
        /// <param name="openId">微信openId</param>
        /// <returns>string</returns>
        public string OpenIdToUserId(string openId)
        {
            var url = $"https://qyapi.weixin.qq.com/cgi-bin/user/convert_to_userid?access_token={accessToken}";
            return HttpHelper.Post(url, $"{{\"openid\":\"{openId}\"}}");
        }
        #endregion

        #region 素材管理
        /// <summary>
        /// 新增企业临时素材
        /// </summary>
        /// <param name="type">素材类型</param>
        /// <param name="filePath">源文件路径</param>
        /// <returns>string</returns>
        public string AddQyTempMaterial(string type, string filePath)
        {
            var url = $"https://qyapi.weixin.qq.com/cgi-bin/media/upload?access_token={accessToken}&type={type}";
            return HttpHelper.Upload(url, filePath);
        }

        /// <summary>
        /// 获取企业临时素材
        /// </summary>
        /// <param name="mediaId">多媒体文件Id</param>
        /// <param name="filePath">文件路径</param>
        /// <param name="fileExt">文件扩展名</param>
        /// <param name="fileName">文件名，不包含扩展名</param>
        public void GetQyTempMaterial(string mediaId, string filePath, string fileExt, string fileName)
        {
            var url = $"https://qyapi.weixin.qq.com/cgi-bin/media/get?access_token={accessToken}&media_id={mediaId}";
            HttpHelper.Download(url, saveDir: filePath, fileExt: fileExt, fileName: fileName);
        }

        /// <summary>
        /// 新增企业永久图文素材
        /// </summary>
        /// <param name="imgTxtJson">图文消息json字符串内容</param>
        /// <returns>string</returns>
        public string AddQyForeverImgTxtMaterial(string imgTxtJson)
        {
            var url = $"https://qyapi.weixin.qq.com/cgi-bin/material/add_mpnews?access_token={accessToken}";
            return HttpHelper.Post(url, imgTxtJson);
        }

        /// <summary>
        /// 新增其他类型企业永久素材
        /// </summary>
        /// <param name="agentId">企业应用的id</param>
        /// <param name="type">素材类型</param>
        /// <param name="filePath">文件路径</param>
        /// <returns>string</returns>
        public string AddQyOtherForeverMaterial(string agentId, string type, string filePath)
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/material/add_material?access_token={accessToken}&type={type}";
            return HttpHelper.Upload(url, filePath);
        }

        /// <summary>
        /// 获取企业永久素材(不包含其他类型永久素材)
        /// </summary>
        /// <param name="mediaId">多媒体文件Id</param>
        /// <param name="agentId">企业应用的id</param>
        /// <returns>string</returns>
        public string GetQyForeverMaterial(string mediaId, int agentId)
        {
            var url = $"https://qyapi.weixin.qq.com/cgi-bin/material/get?access_token={accessToken}&media_id={mediaId}&agentid={agentId}";
            return HttpHelper.Get(url);
        }

        /// <summary>
        /// 获取企业其他类型永久素材(保存为文件)
        /// </summary>
        /// <param name="mediaId">多媒体文件Id</param>
        /// <param name="agentId">企业应用的id</param>
        /// <param name="filePath">保存文件路径</param>
        /// <param name="fileExt">文件扩展名</param>
        /// <param name="fileName">文件名，不包含扩展名</param>
        public void GetQyOtherForeverMaterial(string mediaId, int agentId, string filePath, string fileExt, string fileName)
        {
            var url = $"https://qyapi.weixin.qq.com/cgi-bin/material/get?access_token={accessToken}&media_id={mediaId}&agentid={agentId}";
            HttpHelper.Download(url, saveDir: filePath, fileExt: fileExt, fileName: fileName);
        }

        /// <summary>
        /// 删除企业永久素材
        /// </summary>
        /// <param name="mediaId">多媒体文件Id</param>
        /// <param name="agentId">企业应用的id</param>
        /// <returns>string</returns>
        public string DeleteQyForeverMaterial(string mediaId, int agentId)
        {
            var url = $"https://qyapi.weixin.qq.com/cgi-bin/material/del?access_token={accessToken}&agentid={agentId}&media_id={mediaId}";
            return HttpHelper.Get(url);
        }

        /// <summary>
        /// 修改企业永久图文素材
        /// </summary>
        /// <param name="imgTxtJson">图文消息json字符串内容</param>
        /// <returns>string</returns>
        public string UpdateQyForeverImgTxtMaterial(string imgTxtJson)
        {
            var url = $"https://qyapi.weixin.qq.com/cgi-bin/material/update_mpnews?access_token={accessToken}";
            return HttpHelper.Post(url, imgTxtJson);
        }

        /// <summary>
        /// 获取企业永久素材总数
        /// </summary>
        /// <param name="agentId">企业应用的id</param>
        /// <returns>string</returns>
        public string GetQyForeverMaterialCount(int agentId)
        {
            var url = $"https://qyapi.weixin.qq.com/cgi-bin/material/get_count?access_token={accessToken}&agentid={agentId}";
            return HttpHelper.Get(url);
        }

        /// <summary>
        /// 获取企业永久素材列表
        /// </summary>
        /// <param name="postJson">post数据</param>
        /// <returns>string</returns>
        public string GetQyForeverMaterialList(string postJson)
        {
            var url = $"https://qyapi.weixin.qq.com/cgi-bin/material/batchget?access_token={accessToken}";
            return HttpHelper.Post(url, postJson);
        }

        /// <summary>
        /// 上传企业图文消息中用到的图片
        /// </summary>
        /// <param name="filePath">图片路径</param>
        /// <returns>string</returns>
        public string UploadQyImgTextUsedImg(string filePath)
        {
            var url = $"https://qyapi.weixin.qq.com/cgi-bin/media/uploadimg?access_token={accessToken}";
            return HttpHelper.Upload(url, filePath);
        }
        #endregion

        #region 企业号应用
        /// <summary>
        /// 获取企业号应用
        /// </summary>
        /// <param name="agentId">企业应用的id</param>
        /// <returns>string</returns>
        public string GetQyAgentInfo(string agentId)
        {
            var url = $"https://qyapi.weixin.qq.com/cgi-bin/agent/get?access_token={accessToken}&agentid={agentId}";
            return HttpHelper.Get(url);
        }

        /// <summary>
        /// 设置企业号应用
        /// </summary>
        /// <param name="postJson">post数据</param>
        /// <returns>string</returns>
        public string SetQyAgent(string postJson)
        {
            var url = $"https://qyapi.weixin.qq.com/cgi-bin/agent/set?access_token={accessToken}";
            return HttpHelper.Post(url, postJson);
        }

        /// <summary>
        /// 获取应用概况列表
        /// </summary>
        /// <returns>string</returns>
        public string GetQyAgentList()
        {
            var url = $"https://qyapi.weixin.qq.com/cgi-bin/agent/list?access_token={accessToken}";
            return HttpHelper.Get(url);
        }
        #endregion

        #region 自定义菜单
        /// <summary>
        /// 创建企业菜单
        /// </summary>
        /// <param name="agentId">企业应用的id</param>
        /// <param name="menuJson">菜单json数据</param>
        /// <returns>string</returns>
        public string CreateQyMenu(string agentId, string menuJson)
        {
            var url = $"https://qyapi.weixin.qq.com/cgi-bin/menu/create?access_token={accessToken}&agentid={agentId}";
            return HttpHelper.Post(url, menuJson);
        }

        /// <summary>
        /// 删除企业菜单
        /// </summary>
        /// <param name="agentId">企业应用的id</param>
        /// <returns>string</returns>
        public string DeleteQyMenu(string agentId)
        {
            var url = $"https://qyapi.weixin.qq.com/cgi-bin/menu/delete?access_token={accessToken}&agentid={agentId}";
            return HttpHelper.Get(url);
        }

        /// <summary>
        /// 获取企业菜单列表
        /// </summary>
        /// <param name="agentId">企业应用的id</param>
        /// <returns>string</returns>
        public string GetQyMenuList(string agentId)
        {
            var url = $"https://qyapi.weixin.qq.com/cgi-bin/menu/get?access_token={accessToken}&agentid={agentId}";
            return HttpHelper.Get(url);
        }
        #endregion

        #region 企业消息与会话
        /// <summary>
        /// 发送企业消息
        /// </summary>
        /// <param name="msgJson">消息json数据</param>
        /// <returns>string</returns>
        public string SendQyMsg(string msgJson)
        {
            var url = $"https://qyapi.weixin.qq.com/cgi-bin/message/send?access_token={accessToken}";
            return HttpHelper.Post(url, msgJson);
        }

        /// <summary>
        /// 创建会话
        /// </summary>
        /// <param name="msgJson">消息json数据</param>
        /// <returns>string</returns>
        public string CreateQyChat(string msgJson)
        {
            var url = $"https://qyapi.weixin.qq.com/cgi-bin/chat/create?access_token={accessToken}";
            return HttpHelper.Post(url, msgJson);
        }

        /// <summary>
        /// 获取会话
        /// </summary>
        /// <param name="chatId">会话id</param>
        /// <returns>string</returns>
        public string GetQyChat(string chatId)
        {
            var url = $"https://qyapi.weixin.qq.com/cgi-bin/chat/get?access_token={accessToken}&chatid={chatId}";
            return HttpHelper.Get(url);
        }

        /// <summary>
        /// 修改会话信息
        /// </summary>
        /// <param name="msgJson">消息json修改数据</param>
        /// <returns>string</returns>
        public string UpdateQyChat(string msgJson)
        {
            var url = $"https://qyapi.weixin.qq.com/cgi-bin/chat/update?access_token={accessToken}";
            return HttpHelper.Post(url, msgJson);
        }

        /// <summary>
        /// 退出会话
        /// </summary>
        /// <param name="postJson">json数据</param>
        /// <returns>string</returns>
        public string QuitQyChat(string postJson)
        {
            var url = $"https://qyapi.weixin.qq.com/cgi-bin/chat/quit?access_token={accessToken}";
            return HttpHelper.Post(url, postJson);
        }

        /// <summary>
        /// 清除会话未读状态
        /// </summary>
        /// <param name="postJson">json数据</param>
        /// <returns>string</returns>
        public string ClearQyChat(string postJson)
        {
            var url = $"https://qyapi.weixin.qq.com/cgi-bin/chat/clearnotify?access_token={postJson}";
            return HttpHelper.Post(url, postJson);
        }

        /// <summary>
        /// 发消息会话
        /// </summary>
        /// <param name="msgJson">消息json数据</param>
        /// <returns>string</returns>
        public string SendQyChatMsg(string msgJson)
        {
            var url = $"https://qyapi.weixin.qq.com/cgi-bin/chat/send?access_token={accessToken}";
            return HttpHelper.Post(url, msgJson);
        }

        /// <summary>
        /// 设置成员新消息免打扰
        /// </summary>
        /// <param name="postJson">json数据</param>
        /// <returns>string</returns>
        public string SetQyChatMute(string postJson)
        {
            var url = $"https://qyapi.weixin.qq.com/cgi-bin/chat/setmute?access_token={accessToken}";
            return HttpHelper.Post(url, postJson);
        }
        #endregion
        #endregion

        #region 微信小程序
        #region 获取seesion_key
        /// <summary>
        /// 小程序登录code获取session_key
        /// </summary>
        /// <param name="code">小程序调用wx.login后返回的code</param>
        /// <returns>string</returns>
        public string JsCodeToSession(string code)
        {
            var url = $"https://api.weixin.qq.com/sns/jscode2session?appid={wxConfig.AppId}&secret={wxConfig.AppSecret}&js_code={code}&grant_type=authorization_code";
            return HttpHelper.Get(url);
        }
        #endregion

        #region AES-128-CBC-PKCS7解密
        /// <summary>
        /// 微信小程序AES-128-CBC-PKCS7解密
        /// </summary>
        /// <param name="text">解密字符串</param>
        /// <param name="key">密钥</param>
        /// <param name="iv">初始化向量</param>
        /// <returns>string</returns>
        public string AESDecrypt(string text, string key, string iv)
        {
            return CryptHelper.DecryptByAes(text, key, iv);
        }
        #endregion

        #region 发送模板消息
        /// <summary>
        /// 发送微信小程序模板消息
        /// </summary>
        /// <param name="jsonContent">消息内容</param>
        /// <returns>string</returns>
        public string SendWxOpenTemplateMsg(string jsonContent)
        {
            var wxUrl = $"https://api.weixin.qq.com/cgi-bin/message/wxopen/template/send?access_token={accessToken}";
            return HttpHelper.Post(wxUrl, jsonContent);
        }
        #endregion
        #endregion

        #region Dispose
        /// <summary>
        /// 清空私有属性，可以使用using方法初始化WeChatHelper
        /// </summary>
        public void Dispose()
        {
            accessToken = null;
            jsApiTicket = null;
            wxConfig = null;
        }
        #endregion
    }
}