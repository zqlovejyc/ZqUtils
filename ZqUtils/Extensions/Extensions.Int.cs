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
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
/****************************
* [Author] 张强
* [Date] 2018-05-15
* [Describe] Int扩展类
* **************************/
namespace ZqUtils.Extensions
{
    /// <summary>
    /// Int扩展类
    /// </summary>
    public static partial class Extensions
    {
        #region BuildRandomString
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
    }
}
