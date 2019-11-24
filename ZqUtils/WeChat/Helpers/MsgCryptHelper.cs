#region License
/***
 * Copyright © 2018-2020, 张强 (943620963@qq.com).
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
using System.Xml;
using System.Collections;
using System.Security.Cryptography;
using ZqUtils.Helpers;
/****************************
* [Author] 张强
* [Date] 2018-1-7
* [Describe] 微信消息加密解密工具类
* **************************/
namespace ZqUtils.WeChat.Helpers
{
    /// <summary>
    /// 字典排序
    /// </summary>
    public class DictionarySort : IComparer
    {
        /// <summary>
        /// 比较方法
        /// </summary>
        /// <param name="oLeft"></param>
        /// <param name="oRight"></param>
        /// <returns></returns>
        public int Compare(object oLeft, object oRight)
        {
            var sLeft = oLeft as string;
            var sRight = oRight as string;
            var iLeftLength = sLeft.Length;
            var iRightLength = sRight.Length;
            var index = 0;
            while (index < iLeftLength && index < iRightLength)
            {
                if (sLeft[index] < sRight[index])
                {
                    return -1;
                }
                else if (sLeft[index] > sRight[index])
                {
                    return 1;
                }
                else
                {
                    index++;
                }
            }
            return iLeftLength - iRightLength;
        }
    }

    /// <summary>
    /// 微信消息加密解密工具类
    /// 40001 : 签名验证错误
    /// 40002 : xml解析失败
    /// 40003 : sha加密生成签名失败
    /// 40004 : AESKey非法
    /// 40005 : appid校验错误
    /// 40006 : AES加密失败
    /// 40007 : AES解密失败
    /// 40008 : 解密后得到的buffer非法
    /// 40009 : base64加密异常
    /// 40010 : base64解密异常
    /// </summary>
    public class MsgCryptHelper
    {
        #region 私有字段
        /// <summary>
        /// 公众平台上，开发者设置的Token
        /// </summary>
        private readonly string m_sToken;

        /// <summary>
        /// 公众帐号的appid
        /// </summary>
        private readonly string m_sAppID;

        /// <summary>
        /// 消息加解密密钥(EncodingAESKey)
        /// </summary>
        private readonly string m_sEncodingAESKey;

        /// <summary>
        /// 消息加密错误码
        /// </summary>
        private enum MsgCryptErrorCode
        {
            MsgCrypt_OK = 0,
            MsgCrypt_ValidateSignature_Error = 40001,
            MsgCrypt_ParseXml_Error = 40002,
            MsgCrypt_ComputeSignature_Error = 40003,
            MsgCrypt_IllegalAesKey = 40004,
            MsgCrypt_ValidateAppid_Error = 40005,
            MsgCrypt_EncryptAES_Error = 40006,
            MsgCrypt_DecryptAES_Error = 40007,
            MsgCrypt_IllegalBuffer = 40008,
            MsgCrypt_EncodeBase64_Error = 40009,
            MsgCrypt_DecodeBase64_Error = 40010
        };
        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="sToken">公众平台上，开发者设置的Token</param>
        /// <param name="sAppID">公众帐号的appid</param>
        /// <param name="sEncodingAESKey">公众平台上，开发者设置的EncodingAESKey</param>
        public MsgCryptHelper(string sToken, string sAppID, string sEncodingAESKey)
        {
            m_sToken = sToken;
            m_sAppID = sAppID;
            m_sEncodingAESKey = sEncodingAESKey;
        }
        #endregion

        #region 解密消息
        /// <summary>
        /// 检验消息的真实性，并且获取解密后的明文
        /// </summary>
        /// <param name="sMsgSignature">签名串，对应URL参数的msg_signature</param>
        /// <param name="sTimeStamp">时间戳，对应URL参数的timestamp</param>
        /// <param name="sNonce">随机字符串，对应URL参数的nonce</param>
        /// <param name="sPostData">密文，对应POST请求的数据</param>
        /// <param name="sMsg">解密后的原文，当return返回0时有效</param>
        /// <returns>成功返回0，失败返回对应的错误码</returns>
        public int DecryptMsg(string sMsgSignature, string sTimeStamp, string sNonce, string sPostData, ref string sMsg)
        {
            if (m_sEncodingAESKey.Length != 43) return (int)MsgCryptErrorCode.MsgCrypt_IllegalAesKey;
            var doc = new XmlDocument();
            XmlNode root;
            string sEncryptMsg;
            try
            {
                doc.LoadXml(sPostData);
                root = doc.FirstChild;
                sEncryptMsg = root["Encrypt"].InnerText;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "xml解析失败");
                return (int)MsgCryptErrorCode.MsgCrypt_ParseXml_Error;
            }
            //verify signature
            var ret = VerifySignature(m_sToken, sTimeStamp, sNonce, sEncryptMsg, sMsgSignature);
            if (ret != 0) return ret;
            //decrypt
            var cpid = "";
            try
            {
                sMsg = CryptHelper.DecryptByAesOfWechat(sEncryptMsg, m_sEncodingAESKey, ref cpid);
            }
            catch (FormatException ex)
            {
                LogHelper.Error(ex, "base64解密异常");
                return (int)MsgCryptErrorCode.MsgCrypt_DecodeBase64_Error;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "AES解密失败");
                return (int)MsgCryptErrorCode.MsgCrypt_DecryptAES_Error;
            }
            if (cpid != m_sAppID) return (int)MsgCryptErrorCode.MsgCrypt_ValidateAppid_Error;
            return 0;
        }
        #endregion

        #region 加密消息
        /// <summary>
        /// 将回复用户的消息加密打包
        /// </summary>
        /// <param name="sReplyMsg">待回复用户的消息，xml格式的字符串</param>
        /// <param name="sTimeStamp">时间戳，可以自己生成，也可以用URL参数的timestamp</param>
        /// <param name="sNonce">随机字符串，可以自己生成，也可以用URL参数的nonce</param>
        /// <param name="sEncryptMsg">加密后的可以直接回复用户的密文，包括msg_signature, timestamp, nonce, encrypt的xml格式的字符串</param>
        /// <returns>成功返回0，失败返回对应的错误码</returns>
        public int EncryptMsg(string sReplyMsg, string sTimeStamp, string sNonce, ref string sEncryptMsg)
        {
            if (m_sEncodingAESKey.Length != 43) return (int)MsgCryptErrorCode.MsgCrypt_IllegalAesKey;
            var raw = "";
            try
            {
                raw = CryptHelper.EncryptByAesOfWechat(sReplyMsg, m_sEncodingAESKey, m_sAppID);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "AES加密失败");
                return (int)MsgCryptErrorCode.MsgCrypt_EncryptAES_Error;
            }
            var MsgSigature = "";
            var ret = GenarateSinature(m_sToken, sTimeStamp, sNonce, raw, ref MsgSigature);
            if (0 != ret) return ret;
            sEncryptMsg = $@"<xml>
                                <Encrypt><![CDATA[{raw}]]></Encrypt>
                                <MsgSignature><![CDATA[{MsgSigature}]]></MsgSignature>
                                <TimeStamp><![CDATA[{sTimeStamp}]]></TimeStamp>
                                <Nonce><![CDATA[{sNonce}]]></Nonce>
                             </xml>";
            return 0;
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 校验签名
        /// </summary>
        /// <param name="sToken">公众平台上，开发者设置的Token</param>
        /// <param name="sTimeStamp">时间戳</param>
        /// <param name="sNonce">随机字符串</param>
        /// <param name="sMsgEncrypt">加密消息</param>
        /// <param name="sSigture">消息签名</param>
        /// <returns>成功返回0，失败返回对应的错误码</returns>
        private int VerifySignature(string sToken, string sTimeStamp, string sNonce, string sMsgEncrypt, string sSigture)
        {
            var hash = "";
            var ret = GenarateSinature(sToken, sTimeStamp, sNonce, sMsgEncrypt, ref hash);
            if (ret != 0) return ret;
            if (hash == sSigture)
            {
                return 0;
            }
            else
            {
                return (int)MsgCryptErrorCode.MsgCrypt_ValidateSignature_Error;
            }
        }

        /// <summary>
        /// 生成签名
        /// </summary>
        /// <param name="sToken">公众平台上，开发者设置的Token</param>
        /// <param name="sTimeStamp">时间戳</param>
        /// <param name="sNonce">随机字符串</param>
        /// <param name="sMsgEncrypt">加密消息</param>
        /// <param name="sMsgSignature">生成的消息签名</param>
        /// <returns>成功返回0，失败返回对应的错误码</returns>
        private int GenarateSinature(string sToken, string sTimeStamp, string sNonce, string sMsgEncrypt, ref string sMsgSignature)
        {
            var al = new ArrayList
            {
                sToken,
                sTimeStamp,
                sNonce,
                sMsgEncrypt
            };
            al.Sort(new DictionarySort());
            var raw = "";
            for (var i = 0; i < al.Count; ++i)
            {
                raw += al[i];
            }
            SHA1 sha;
            ASCIIEncoding enc;
            var hash = "";
            try
            {
                sha = new SHA1CryptoServiceProvider();
                enc = new ASCIIEncoding();
                var dataToHash = enc.GetBytes(raw);
                var dataHashed = sha.ComputeHash(dataToHash);
                hash = BitConverter.ToString(dataHashed).Replace("-", "");
                hash = hash.ToLower();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "sha加密生成签名失败");
                return (int)MsgCryptErrorCode.MsgCrypt_ComputeSignature_Error;
            }
            sMsgSignature = hash;
            return 0;
        }
        #endregion
    }
}
