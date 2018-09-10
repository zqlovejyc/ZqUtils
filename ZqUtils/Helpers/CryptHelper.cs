using System;
using System.IO;
using System.Net;
using System.Text;
using System.Security.Cryptography;
using ZqUtils.Extensions;
using SysMD5 = System.Security.Cryptography.MD5;
using SysSH1 = System.Security.Cryptography.SHA1;
/****************************
* [Author] 张强
* [Date] 2015-10-26
* [Describe] 加密解密工具类
* **************************/
namespace ZqUtils.Helpers
{
    /// <summary>
    /// 加密解密工具类
    /// </summary>
    public class CryptHelper
    {
        #region MD5加密
        /// <summary>
        /// md5加密
        /// </summary>
        /// <param name="source">源字符串</param>
        /// <param name="type">加密类型[16位/32位/48位/64位]，默认32位</param>
        /// <returns>string</returns>
        public static string MD5(string source, int type = 32)
        {
            var result = string.Empty;
            try
            {
                var bytes = SysMD5.Create().ComputeHash(Encoding.UTF8.GetBytes(source));
                var sb = new StringBuilder();
                foreach (var item in bytes)
                {
                    //加密结果"x2"结果为32位,"x3"结果为48位,"x4"结果为64位
                    switch (type)
                    {
                        case 16:
                        case 32:
                            sb.Append(item.ToString("x2"));
                            break;
                        case 48:
                            sb.Append(item.ToString("x3"));
                            break;
                        case 64:
                            sb.Append(item.ToString("x4"));
                            break;
                    }
                }
                result = sb.ToString().ToUpper();
                if (type == 16) result = result.Substring(8, 16);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "MD5加密");
            }
            return result;
        }
        #endregion

        #region SHA1
        /// <summary>
        /// 哈希算法
        /// </summary>
        /// <param name="source">源字符串</param>
        /// <returns>string</returns>
        public static string SHA1(string source)
        {
            var result = string.Empty;
            try
            {
                var bytes = SysSH1.Create().ComputeHash(Encoding.UTF8.GetBytes(source));
                var sb = new StringBuilder();
                foreach (var item in bytes)
                {
                    sb.Append(item.ToString("x2"));
                }
                result = sb.ToString().ToUpper();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "哈希算法");
            }
            return result;
        }
        #endregion

        #region HMAC-SHA256
        /// <summary>
        /// HMAC-SHA256加密算法
        /// </summary>
        /// <param name="source">源字符串</param>
        /// <param name="secret">密钥</param>
        /// <returns>string</returns>
        public static string HmacSha256(string source, string secret)
        {
            var result = string.Empty;
            try
            {
                source = source ?? "";
                secret = secret ?? "";
                var keyByte = Encoding.UTF8.GetBytes(secret);
                var messageBytes = Encoding.UTF8.GetBytes(source);
                using (var hmacsha256 = new HMACSHA256(keyByte))
                {
                    var hashmessage = hmacsha256.ComputeHash(messageBytes);
                    var sb = new StringBuilder();
                    foreach (var b in hashmessage)
                    {
                        sb.AppendFormat("{0:x2}", b);
                    }
                    result = sb.ToString();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "HMAC-SHA256加密算法");
            }
            return result;
        }
        #endregion

        #region 自定义加密/解密
        /// <summary>
        /// 加密函数
        /// </summary>
        /// <param name="source">加密字符串</param>
        /// <param name="key">密钥</param>
        /// <returns>string</returns>
        public static string Encrypt(string source, string key)
        {
            var ret = new StringBuilder();
            try
            {
                var des = new DESCryptoServiceProvider();
                var inputByteArray = Encoding.UTF8.GetBytes(source);
                des.Key = Encoding.UTF8.GetBytes(MD5(key).Substring(0, 8));
                des.IV = Encoding.UTF8.GetBytes(MD5(source).Substring(0, 8));
                var ms = new MemoryStream();
                var cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                foreach (var b in ms.ToArray())
                {
                    ret.AppendFormat("{0:X2}", b);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "自定义加密函数");
            }
            return ret.ToString();
        }

        /// <summary>
        /// 解密函数
        /// </summary>
        /// <param name="source">解密字符串</param>
        /// <param name="key">密钥</param>
        /// <returns>string</returns>
        public static string Decrypt(string source, string key)
        {
            var res = string.Empty;
            try
            {
                var des = new DESCryptoServiceProvider();
                var len = source.Length / 2;
                var inputByteArray = new byte[len];
                int x, i;
                for (x = 0; x < len; x++)
                {
                    i = Convert.ToInt32(source.Substring(x * 2, 2), 16);
                    inputByteArray[x] = (byte)i;
                }
                des.Key = Encoding.UTF8.GetBytes(MD5(key).Substring(0, 8));
                des.IV = Encoding.UTF8.GetBytes(MD5(key).Substring(0, 8));
                var ms = new MemoryStream();
                var cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                res = Encoding.UTF8.GetString(ms.ToArray());
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "自定义解密函数");
            }
            return res;
        }
        #endregion

        #region 微信小程序AES-128-CBC加密/解密
        /// <summary>
        /// 微信小程序AES-128-CBC加密
        /// </summary>
        /// <param name="text">加密字符</param>
        /// <param name="key">密钥</param>
        /// <param name="iv">初始化向量</param>
        /// <returns>string</returns>
        public static string AES_128_CBC_Encrypt(string text, string key, string iv)
        {
            var res = string.Empty;
            try
            {
                var rijndaelCipher = new RijndaelManaged
                {
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.PKCS7,
                    KeySize = 128,
                    BlockSize = 128,
                    Key = Convert.FromBase64String(key),
                    IV = Convert.FromBase64String(iv)
                };
                var transform = rijndaelCipher.CreateEncryptor();
                var plainText = Encoding.UTF8.GetBytes(text);
                var cipherBytes = transform.TransformFinalBlock(plainText, 0, plainText.Length);
                res = Convert.ToBase64String(cipherBytes);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "微信小程序AES-128-CBC加密");
            }
            return res;
        }

        /// <summary>
        /// 微信小程序AES-128-CBC解密
        /// </summary>
        /// <param name="text">解密字符串</param>
        /// <param name="key">密钥</param>
        /// <param name="iv">初始化向量</param>
        /// <returns>string</returns>
        public static string AES_128_CBC_Decrypt(string text, string key, string iv)
        {
            var res = string.Empty;
            try
            {
                var rijndaelCipher = new RijndaelManaged
                {
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.PKCS7,
                    KeySize = 128,
                    BlockSize = 128
                };
                var encryptedData = Convert.FromBase64String(text);
                rijndaelCipher.Key = Convert.FromBase64String(key);
                rijndaelCipher.IV = Convert.FromBase64String(iv);
                var transform = rijndaelCipher.CreateDecryptor();
                var plainText = transform.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
                res = Encoding.UTF8.GetString(plainText);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "微信小程序AES-128-CBC解密");
            }
            return res;
        }
        #endregion

        #region 加密函数[对应javascript里面的escape]
        /// <summary>
        /// 加密函数[对应javascript里面的escape]
        /// </summary>
        /// <param name="s">要加密的字符串</param>
        /// <returns>string</returns>
        public static string Escape(string s)
        {
            var sb = new StringBuilder();
            try
            {
                var ba = Encoding.Unicode.GetBytes(s);
                for (var i = 0; i < ba.Length; i += 2)
                {
                    sb.Append("%u")
                      .Append(ba[i + 1].ToString("X2"))
                      .Append(ba[i].ToString("X2"));
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "escape加密函数");
            }
            return sb.ToString();
        }
        #endregion

        #region 微信消息加密解密
        #region 公有方法
        /// <summary>
        /// 将短值由主机字节顺序转换为网络字节顺序
        /// </summary>
        /// <param name="inval">32位无符号整数</param>
        /// <returns>UInt32</returns>
        public static UInt32 HostToNetworkOrder(UInt32 inval)
        {
            UInt32 outval = 0;
            for (int i = 0; i < 4; i++)
            {
                outval = (outval << 8) + ((inval >> (i * 8)) & 255);
            }
            return outval;
        }

        /// <summary>
        /// 将短值由主机字节顺序转换为网络字节顺序
        /// </summary>
        /// <param name="inval">32位有符号整数</param>
        /// <returns>int</returns>
        public static int HostToNetworkOrder(int inval)
        {
            int outval = 0;
            for (int i = 0; i < 4; i++)
            {
                outval = (outval << 8) + ((inval >> (i * 8)) & 255);
            }
            return outval;
        }

        /// <summary>
        /// 微信消息解密方法
        /// </summary>
        /// <param name="input">解密字符串</param>
        /// <param name="encodingAESKey">aes密钥</param>
        /// <param name="appid">微信appid</param>
        /// <returns>string</returns>
        public static string AES_Decrypt(string input, string encodingAESKey, ref string appid)
        {
            var oriMsg = string.Empty;
            try
            {
                var key = Convert.FromBase64String(encodingAESKey + "=");
                var iv = new byte[16];
                Array.Copy(key, iv, 16);
                var btmpMsg = AES_Decrypt(input, iv, key);
                var len = BitConverter.ToInt32(btmpMsg, 16);
                len = IPAddress.NetworkToHostOrder(len);
                var bMsg = new byte[len];
                var bAppid = new byte[btmpMsg.Length - 20 - len];
                Array.Copy(btmpMsg, 20, bMsg, 0, len);
                Array.Copy(btmpMsg, 20 + len, bAppid, 0, btmpMsg.Length - 20 - len);
                oriMsg = Encoding.UTF8.GetString(bMsg);
                appid = Encoding.UTF8.GetString(bAppid);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "微信消息解密方法");
            }
            return oriMsg;
        }

        /// <summary>
        /// 微信消息加密方法
        /// </summary>
        /// <param name="input">加密字符串</param>
        /// <param name="encodingAESKey">aes密钥</param>
        /// <param name="appid">微信appid</param>
        /// <returns>string</returns>
        public static string AES_Encrypt(string input, string encodingAESKey, string appid)
        {
            var result = string.Empty;
            try
            {
                var key = Convert.FromBase64String(encodingAESKey + "=");
                var iv = new byte[16];
                Array.Copy(key, iv, 16);
                var Randcode = 16.BuildRandCode();
                var bRand = Encoding.UTF8.GetBytes(Randcode);
                var bAppid = Encoding.UTF8.GetBytes(appid);
                var btmpMsg = Encoding.UTF8.GetBytes(input);
                var bMsgLen = BitConverter.GetBytes(HostToNetworkOrder(btmpMsg.Length));
                var bMsg = new byte[bRand.Length + bMsgLen.Length + bAppid.Length + btmpMsg.Length];
                Array.Copy(bRand, bMsg, bRand.Length);
                Array.Copy(bMsgLen, 0, bMsg, bRand.Length, bMsgLen.Length);
                Array.Copy(btmpMsg, 0, bMsg, bRand.Length + bMsgLen.Length, btmpMsg.Length);
                Array.Copy(bAppid, 0, bMsg, bRand.Length + bMsgLen.Length + btmpMsg.Length, bAppid.Length);
                result = AES_Encrypt(bMsg, iv, key);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "微信消息加密方法");
            }
            return result;
        }
        #endregion

        #region 私有方法        
        /// <summary>
        /// 私有加密方法
        /// </summary>
        /// <param name="input">加密字符串</param>
        /// <param name="iv">对称算法的初始化向量</param>
        /// <param name="key">对称算法的密钥</param>
        /// <returns>加密后的字符串</returns>
        private static string AES_Encrypt(string input, byte[] iv, byte[] key)
        {
            var aes = new RijndaelManaged
            {
                //秘钥的大小，以位为单位
                KeySize = 256,
                //支持的块大小
                BlockSize = 128,
                //填充模式
                Padding = PaddingMode.PKCS7,
                Mode = CipherMode.CBC,
                Key = key,
                IV = iv
            };
            var encrypt = aes.CreateEncryptor(aes.Key, aes.IV);
            byte[] xBuff = null;
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, encrypt, CryptoStreamMode.Write))
                {
                    var xXml = Encoding.UTF8.GetBytes(input);
                    cs.Write(xXml, 0, xXml.Length);
                }
                xBuff = ms.ToArray();
            }
            return Convert.ToBase64String(xBuff); ;
        }

        /// <summary>
        /// 私有加密方法
        /// </summary>
        /// <param name="input">加密字符串</param>
        /// <param name="iv">对称算法的初始化向量</param>
        /// <param name="key">对称算法的密钥</param>
        /// <returns>加密后的字符串</returns>
        private static string AES_Encrypt(byte[] input, byte[] iv, byte[] key)
        {
            var aes = new RijndaelManaged
            {
                //秘钥的大小，以位为单位
                KeySize = 256,
                //支持的块大小
                BlockSize = 128,
                //填充模式
                //aes.Padding = PaddingMode.PKCS7;
                Padding = PaddingMode.None,
                Mode = CipherMode.CBC,
                Key = key,
                IV = iv
            };
            var encrypt = aes.CreateEncryptor(aes.Key, aes.IV);
            byte[] xBuff = null;

            #region 自己进行PKCS7补位，用系统自己带的不行
            var msg = new byte[input.Length + 32 - input.Length % 32];
            Array.Copy(input, msg, input.Length);
            var pad = KCS7Encoder(input.Length);
            Array.Copy(pad, 0, msg, input.Length, pad.Length);
            #endregion

            #region 注释的也是一种方法，效果一样
            //ICryptoTransform transform = aes.CreateEncryptor();
            //byte[] xBuff = transform.TransformFinalBlock(msg, 0, msg.Length);
            #endregion

            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, encrypt, CryptoStreamMode.Write))
                {
                    cs.Write(msg, 0, msg.Length);
                }
                xBuff = ms.ToArray();
            }
            return Convert.ToBase64String(xBuff);
        }

        /// <summary>
        /// KCS7加密
        /// </summary>
        /// <param name="text_length">字符串长度</param>
        /// <returns>加密后的字节数组</returns>
        private static byte[] KCS7Encoder(int text_length)
        {
            var block_size = 32;
            // 计算需要填充的位数
            var amount_to_pad = block_size - (text_length % block_size);
            if (amount_to_pad == 0) amount_to_pad = block_size;
            // 获得补位所用的字符
            var pad_chr = Chr(amount_to_pad);
            var tmp = "";
            for (var index = 0; index < amount_to_pad; index++)
            {
                tmp += pad_chr;
            }
            return Encoding.UTF8.GetBytes(tmp);
        }

        /// <summary>
        /// 将数字转化成ASCII码对应的字符，用于对明文进行补码
        /// </summary>
        /// <param name="a">需要转化的数字</param>
        /// <returns>转化得到的字符</returns>
        private static char Chr(int a)
        {
            var target = (byte)(a & 0xFF);
            return (char)target;
        }

        /// <summary>
        /// 私有解密方法
        /// </summary>
        /// <param name="input">解密字符串</param>
        /// <param name="iv">对称算法的初始化向量</param>
        /// <param name="key">对称算法的密钥</param>
        /// <returns>解密后的字节数组</returns>
        private static byte[] AES_Decrypt(string input, byte[] iv, byte[] key)
        {
            var aes = new RijndaelManaged
            {
                KeySize = 256,
                BlockSize = 128,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.None,
                Key = key,
                IV = iv
            };
            var decrypt = aes.CreateDecryptor(aes.Key, aes.IV);
            byte[] xBuff = null;
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, decrypt, CryptoStreamMode.Write))
                {
                    var xXml = Convert.FromBase64String(input);
                    var msg = new byte[xXml.Length + 32 - xXml.Length % 32];
                    Array.Copy(xXml, msg, xXml.Length);
                    cs.Write(xXml, 0, xXml.Length);
                }
                xBuff = Decode(ms.ToArray());
            }
            return xBuff;
        }

        /// <summary>
        /// 对字节数组进行解码
        /// </summary>
        /// <param name="decrypted">解密字节数组</param>
        /// <returns>解码后的字节数组</returns>
        private static byte[] Decode(byte[] decrypted)
        {
            var pad = (int)decrypted[decrypted.Length - 1];
            if (pad < 1 || pad > 32) pad = 0;
            var res = new byte[decrypted.Length - pad];
            Array.Copy(decrypted, 0, res, 0, decrypted.Length - pad);
            return res;
        }
        #endregion
        #endregion
    }
}
