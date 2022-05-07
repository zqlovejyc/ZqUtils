#region License
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

using System;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
/****************************
* [Author] 张强
* [Date] 2018-05-15
* [Describe] 数值扩展类
* **************************/
namespace ZqUtils.Extensions
{
    /// <summary>
    /// 数值扩展类
    /// </summary>
    public static class NumberExtensions
    {
        #region 随机数
        /// <summary>
        /// 创建指定界限和位数的随机小数
        /// </summary>
        /// <param name="this">小数位数</param>
        /// <param name="minValue">随机小数最小值</param>
        /// <param name="maxValue">随机小数最大值</param>
        /// <returns></returns>
        /// <remarks>
        ///     <code>
        ///         var number = 2.BuildRandomNumber(10.1,10.9);
        ///     </code>
        /// </remarks>
        public static double BuildRandomNumber(this int @this, double minValue, double maxValue)
        {
            return Math.Round(new Random().NextDouble() * (maxValue - minValue) + minValue, @this);
        }
        #endregion

        #region 随机字符串
        /// <summary>
        /// 创建随机字符串
        /// </summary>
        /// <param name="this">字符串长度</param>
        /// <returns>string</returns>
        public static string BuildRandomString(this int @this)
        {
            var codeSerial = "0,1,2,3,4,5,6,7,8,9,a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,x,y,z,A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z";
            if (@this == 0)
            {
                @this = 16;
            }
            var arr = codeSerial.Split(',');
            var code = "";
            var randValue = -1;
            var rand = new Random(unchecked((int)DateTime.Now.Ticks));
            for (var i = 0; i < @this; i++)
            {
                randValue = rand.Next(0, arr.Length - 1);
                code += arr[randValue];
            }
            return code;
        }

        /// <summary>
        /// 创建随机字符串
        /// </summary>
        /// <param name="this">字符串长度</param>
        /// <param name="allowedChars">随机字符串源</param>
        /// <returns></returns>
        public static string BuildRandomString(this int @this, string allowedChars)
        {
            if (@this < 0)
                throw new ArgumentOutOfRangeException(nameof(@this), "length cannot be less than zero.");

            if (string.IsNullOrEmpty(allowedChars))
                allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            const int byteSize = 0x100;
            char[] allowedCharSet = new HashSet<char>(allowedChars).ToArray();
            if (byteSize < allowedCharSet.Length)
                throw new ArgumentException($"allowedChars may contain no more than {byteSize} characters.");

            using (var rng = new RNGCryptoServiceProvider())
            {
                var result = new StringBuilder();
                byte[] buf = new byte[128];

                while (result.Length < @this)
                {
                    rng.GetBytes(buf);
                    for (int i = 0; i < buf.Length && result.Length < @this; ++i)
                    {
                        int outOfRangeStart = byteSize - (byteSize % allowedCharSet.Length);
                        if (outOfRangeStart <= buf[i])
                            continue;
                        result.Append(allowedCharSet[buf[i] % allowedCharSet.Length]);
                    }
                }

                return result.ToString();
            }
        }
        #endregion

        #region 进制转换
        /// <summary>
        /// 10进制转换到2-36进制
        /// </summary>
        /// <param name="this">10进制数字</param>
        /// <param name="radix">进制，范围2-36</param>
        /// <param name="digits">编码取值规则，最大转换位数不能大于该字符串的长度</param>
        /// <returns></returns>
        public static string ToBase(this long @this, int radix, string digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ")
        {
            const int BitsInLong = 64;

            if (radix < 2 || radix > digits.Length)
                throw new ArgumentException("The radix must be >= 2 and <= " + digits.Length.ToString());

            if (@this == 0)
                return "0";

            var index = BitsInLong - 1;
            var currentNumber = Math.Abs(@this);
            var charArray = new char[BitsInLong];

            while (currentNumber != 0)
            {
                var remainder = (int)(currentNumber % radix);
                charArray[index--] = digits[remainder];
                currentNumber /= radix;
            }

            var result = new string(charArray, index + 1, BitsInLong - index - 1);
            if (@this < 0)
            {
                result = "-" + result;
            }

            return result;
        }

        /// <summary>
        /// byte转16进制
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static string ToHex(this byte @this) => Convert.ToString(@this, 16);

        /// <summary>
        /// 2进制转16进制
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static string ToHex(this string @this) => Convert.ToString(Convert.ToInt64(@this, 2), 16);

        /// <summary>
        /// 16进制转2进制
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static string ToBinary(this string @this) => Convert.ToString(Convert.ToInt64(@this, 16), 2);

        /// <summary>
        /// 2进制/16进制转8进制
        /// </summary>
        /// <param name="this"></param>
        /// <param name="fromBase">2或者16，表示2进制或者16进制；</param>
        /// <returns></returns>
        public static string ToOctal(this string @this, int fromBase) => Convert.ToString(Convert.ToInt64(@this, fromBase), 8);

        /// <summary>
        /// 2进制/16进制转10进制
        /// </summary>
        /// <param name="this"></param>
        /// <param name="fromBase">2或者16，表示2进制或者16进制；</param>
        /// <returns></returns>
        public static string ToDecimalism(this string @this, int fromBase)
        {
            if (fromBase == 16)
                return Convert.ToInt32(@this, 16).ToString();
            else
                return Convert.ToString(Convert.ToInt64(@this, 2), 10);
        }
        #endregion
    }
}
