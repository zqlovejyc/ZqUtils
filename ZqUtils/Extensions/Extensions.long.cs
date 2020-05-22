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
/****************************
* [Author] 张强
* [Date] 2020-05-22
* [Describe] 10进制扩展类
* **************************/
namespace ZqUtils.Extensions
{
    /// <summary>
    /// 10进制扩展类
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// 10进制转换到2-36进制
        /// </summary>
        /// <param name="this">10进制数字</param>
        /// <param name="radix">进制，范围2-36</param>
        /// <returns></returns>
        public static string ToBase(this long @this, int radix)
        {
            const int BitsInLong = 64;
            const string Digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            if (radix < 2 || radix > Digits.Length)
                throw new ArgumentException("The radix must be >= 2 and <= " + Digits.Length.ToString());

            if (@this == 0)
                return "0";

            var index = BitsInLong - 1;
            var currentNumber = Math.Abs(@this);
            var charArray = new char[BitsInLong];

            while (currentNumber != 0)
            {
                var remainder = (int)(currentNumber % radix);
                charArray[index--] = Digits[remainder];
                currentNumber /= radix;
            }

            var result = new string(charArray, index + 1, BitsInLong - index - 1);
            if (@this < 0)
            {
                result = "-" + result;
            }

            return result;
        }
    }
}
