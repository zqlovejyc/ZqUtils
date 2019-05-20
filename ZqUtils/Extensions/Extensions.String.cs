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
using System.IO;
using System.Text;
using System.Web;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
/****************************
* [Author] 张强
* [Date] 2015-10-26
* [Describe] string扩展类
* **************************/
namespace ZqUtils.Extensions
{
    /// <summary>
    /// string扩展类
    /// </summary>
    public static partial class Extensions
    {
        #region 随机字符串
        /// <summary>
        /// 创建随机字符串
        /// </summary>
        /// <param name="this">默认取Guid.NewGuid()</param>
        /// <returns>string</returns>
        public static string BuildNonceStr(this Guid @this)
        {
            return @this.ToString().Replace("-", "");
        }
        #endregion

        #region json字符串过滤
        /// <summary>
        /// 过滤json字符串
        /// </summary>
        /// <param name="this">源字符串</param>
        /// <returns>string</returns>
        public static string JsonFilter(this string @this)
        {
            var sb = new StringBuilder();
            foreach (var c in @this)
            {
                switch (c)
                {
                    case '\"':
                        sb.Append("\\\""); break;
                    case '\\':
                        sb.Append("\\\\"); break;
                    case '/':
                        sb.Append("\\/"); break;
                    case '\b':
                        sb.Append("\\b"); break;
                    case '\f':
                        sb.Append("\\f"); break;
                    case '\n':
                        sb.Append("\\n"); break;
                    case '\r':
                        sb.Append("\\r"); break;
                    case '\t':
                        sb.Append("\\t"); break;
                    default:
                        sb.Append(c); break;
                }
            }
            return sb.ToString();
        }
        #endregion

        #region 文件名过滤特殊字符
        /// <summary>
        /// 过滤文件名中特殊字符
        /// </summary>
        /// <param name="this">文件名</param>
        /// <returns>string</returns>
        public static string FilterFileName(this string @this)
        {
            string[] reg = { "'", "'delete", "?", "<", ">", "%", "\"\"", ",", ".", ">=", "=<", "_", ";", "||", "[", "]", "&", "/", "-", "|", " ", "''" };
            for (var i = 0; i < reg.Length; i++)
            {
                @this = @this?.Replace(reg[i], string.Empty);
            }
            return @this;
        }
        #endregion

        #region 文件名格式化
        /// <summary>
        /// 完整文件名格式化
        /// </summary>
        /// <param name="this">源完整文件名</param>
        /// <param name="pathFormat">文件名格式[{filename}会替换成原文件名,配置这项需要注意中文乱码问题；{rand:6}会替换成随机数,后面的数字是随机数的位数；{time}会替换成时间戳；{yyyy}会替换成四位年份；{yy}会替换成两位年份；{mm}会替换成两位月份；{dd}会替换成两位日期；{hh}会替换成两位小时；{ii}会替换成两位分钟；{ss}会替换成两位秒]</param>
        /// <returns>string</returns>
        public static string Format(this string @this, string pathFormat)
        {
            if (string.IsNullOrWhiteSpace(pathFormat)) pathFormat = "{filename}{rand:6}";
            var invalidPattern = new Regex(@"[\\\/\:\*\?\042\<\>\|]");
            @this = invalidPattern.Replace(@this, "");
            string extension = Path.GetExtension(@this);
            string filename = Path.GetFileNameWithoutExtension(@this);
            pathFormat = pathFormat.Replace("{filename}", filename);
            pathFormat = new Regex(@"\{rand(\:?)(\d+)\}", RegexOptions.Compiled).Replace(pathFormat, new MatchEvaluator(delegate (Match match)
            {
                var digit = 6;
                if (match.Groups.Count > 2) digit = Convert.ToInt32(match.Groups[2].Value);
                var rand = new Random();
                return rand.Next((int)Math.Pow(10, digit), (int)Math.Pow(10, digit + 1)).ToString();
            }));
            pathFormat = pathFormat.Replace("{time}", DateTime.Now.Ticks.ToString());
            pathFormat = pathFormat.Replace("{yyyy}", DateTime.Now.Year.ToString());
            pathFormat = pathFormat.Replace("{yy}", (DateTime.Now.Year % 100).ToString("D2"));
            pathFormat = pathFormat.Replace("{mm}", DateTime.Now.Month.ToString("D2"));
            pathFormat = pathFormat.Replace("{dd}", DateTime.Now.Day.ToString("D2"));
            pathFormat = pathFormat.Replace("{hh}", DateTime.Now.Hour.ToString("D2"));
            pathFormat = pathFormat.Replace("{ii}", DateTime.Now.Minute.ToString("D2"));
            pathFormat = pathFormat.Replace("{ss}", DateTime.Now.Second.ToString("D2"));
            return pathFormat + extension;
        }
        #endregion

        #region 获取字符的长度　汉字为两字符
        /// <summary>
        /// 获取字符的长度　汉字为两字符
        /// </summary>
        /// <param name="this">字符串</param>
        /// <returns>int</returns>
        public static int StringLength(this string @this)
        {
            var tLength = 0;
            var strArray = @this.ToCharArray();
            for (int i = 0; i < strArray.Length; i++)
            {
                if (IsChinese(strArray[i].ToString()))
                {
                    tLength += 2;
                }
                else
                {
                    tLength += 1;
                }
            }
            return tLength;
        }
        #endregion

        #region 判断字符是否为中文
        /// <summary>
        /// 判断字符是否为中文字符
        /// </summary>
        /// <param name="this">要验证的字符</param>
        /// <returns>bool</returns>
        public static bool IsChinese(this string @this)
        {
            return Regex.IsMatch(@this, @"^[\u4e00-\u9fa5？，“”‘’。、；：]+$");
        }
        #endregion

        #region 货币转换
        /// <summary>
        /// 货币转换
        /// </summary>
        /// <param name="this">要转换的字符串</param>
        /// <returns>string</returns>
        public static string ToMoney(this string @this)
        {
            return string.Format("{0:C}", @this);
        }
        #endregion

        #region 判断字符是否为空
        /// <summary>
        /// 判断字符串是否为空
        /// </summary>
        /// <param name="this">源字符串</param>
        /// <param name="nullStrings">自定义空字符串，中间“|”分隔</param>
        /// <param name="isTrim">是否移除收尾空白字符串，默认：false</param>
        /// <returns>bool</returns>
        public static bool IsNull(this string @this, string nullStrings = "null|{}|[]", bool isTrim = false)
        {
            var result = true;
            if (@this != null)
            {
                if (isTrim)
                    result = @this.Trim() == "";
                else
                    result = @this == "";
                //是否为自定义空字符串
                if (!result && !string.IsNullOrEmpty(nullStrings))
                {
                    if (isTrim)
                        result = nullStrings.Split('|').Contains(@this.Trim().ToLower());
                    else
                        result = nullStrings.Split('|').Contains(@this.ToLower());
                }
            }
            return result;
        }

        /// <summary>
        /// 指示指定的字符串是 null 还是 string.Empty 字符串
        /// </summary>
        /// <param name="this">当前字符串</param>
        /// <returns>bool</returns>
        public static bool IsNullOrEmpty(this string @this) => string.IsNullOrEmpty(@this);

        /// <summary>
        /// 是否空或者空白字符串
        /// </summary>
        /// <param name="this">当前字符串</param>
        /// <returns>bool</returns>
        public static bool IsNullOrWhiteSpace(this string @this) => string.IsNullOrWhiteSpace(@this);
        #endregion

        #region 验证字符是否是字母类型
        /// <summary>
        /// 验证字符是否是字母类型[如果是true则是字母类型]
        /// </summary>
        /// <param name="this">字符串名</param>
        /// <returns>bool</returns>
        public static bool IsLetter(this string @this)
        {
            return Regex.IsMatch(@this, @"^[a-zA-Z]+$");
        }
        #endregion

        #region 验证字符是否是数字类型
        /// <summary>
        /// 验证字符是否是数字类型[如果是true则是数字类型]
        /// </summary>
        /// <param name="this">字符串名</param>
        /// <returns>bool</returns>
        public static bool IsNum(this string @this)
        {
            return Regex.IsMatch(@this, @"^\d+$");
        }
        #endregion

        #region 验证字符是不是浮点类型
        /// <summary>
        /// 验证字符是不是浮点类型[如果是true则是浮点类型]
        /// </summary>
        /// <param name="this">字符串名</param>
        /// <returns>bool</returns>
        public static bool IsFloat(this string @this)
        {
            return Regex.IsMatch(@this, @"^\d*[.]{0,1}\d*$");
        }
        #endregion

        #region 验证字符是否是Email格式
        /// <summary>
        /// 验证字符是否是Email格式[如果是true则是Email格式]
        /// </summary>
        /// <param name="this">要验证的字符串</param>
        /// <returns>bool</returns>
        public static bool IsEmail(this string @this)
        {
            return Regex.IsMatch(@this, @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");
        }
        #endregion

        #region 验证电话格式
        /// <summary>
        /// 验证字符是否是Tel格式[如果是true则是Tel格式]
        /// </summary>
        /// <param name="this">要验证的字符串</param>
        /// <returns>bool</returns>
        public static bool IsTel(this string @this)
        {
            return Regex.IsMatch(@this, @"^[0-9]{3,4}\-[0-9]{3,8}\-[0-9]{1,4}$)|(^[0-9]{3,4}\-[0-9]{3,8}$)|(^[0-9]{3,8}$)|(^\([0-9]{3,4}\)[0-9]{3,8}$");
        }
        #endregion

        #region 验证是否是手机
        /// <summary>
        /// 验证是否是手机[如果是true则是Tel格式]
        /// </summary>
        /// <param name="this">要验证的字符串</param>
        /// <returns>bool</returns>
        public static bool IsMobile(this string @this)
        {
            return Regex.IsMatch(@this, @"^1[34578]\d{9}$");
        }
        #endregion

        #region 验证是否是网址
        /// <summary>
        /// 验证是否是网址[如果是true则是网址格式]
        /// </summary>
        /// <param name="this">要验证的字符串</param>
        /// <returns>bool</returns>
        public static bool IsUrl(this string @this)
        {
            return Regex.IsMatch(@this, @"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?");
        }
        #endregion

        #region 验证字符串是否是日期类型
        /// <summary>
        /// 验证字符串是否是日期类型[如果是true则是日期类型]
        /// </summary>
        /// <param name="this">要验证的字符串</param>
        /// <returns>bool</returns>
        public static bool IsDate(this string @this)
        {
            return Regex.IsMatch(@this, @"^(\d{2}|\d{4})(-|\/)(\d{1,2})\2(\d{1,2})$");
        }
        #endregion

        #region 验证字符串是否是时间类型
        /// <summary>
        /// 验证字符串是否是时间类型[如果是true则是时间类型]
        /// </summary>
        /// <param name="this">要验证的字符串</param>
        /// <returns>bool</returns>
        public static bool IsTime(this string @this)
        {
            return Regex.IsMatch(@this, @"^\d{1,2}\:\d{1,2}\:\d{1,2}$");
        }
        #endregion

        #region 验证字符串是否是日期时间类型
        /// <summary>
        /// 验证字符串是否是日期时间类型[如果是true则是日期时间类型]
        /// </summary>
        /// <param name="this">要验证的字符串</param>
        /// <returns>bool</returns>
        public static bool IsDateTime(this string @this)
        {
            return Regex.IsMatch(@this, @"^(\d{2}|\d{4})(-|\/)(\d{1,2})\2(\d{1,2})\s\d{1,2}\:\d{1,2}\:\d{1,2}$");
        }
        #endregion

        #region 验证字符串是否是QQ类型
        /// <summary>
        /// 验证字符串是否是QQ类型[如果是true则是日期类型]
        /// </summary>
        /// <param name="this">要验证的字符串</param>
        /// <returns>bool</returns>
        public static bool IsQQ(this string @this)
        {
            return Regex.IsMatch(@this, @"^\d{5,10}$");
        }
        #endregion

        #region URL拼接字符串转换为Dictionary
        /// <summary>
        /// url拼接字符串，参数转换为Dictionary键值对数据
        /// </summary>
        /// <param name="this">url字符串</param>
        /// <returns>Dictionary</returns>
        public static Dictionary<string, string> ToDictionary(this string @this)
        {
            var dic = new Dictionary<string, string>();
            @this = Regex.Replace(Regex.Replace(@this, @"^(.*)\?", ""), @"#(.*)$", "");
            var arr = @this.Split('&');
            foreach (var i in arr)
            {
                if (!string.IsNullOrEmpty(i))
                {
                    var t = i.Split('=');
                    dic.Add(t[0], HttpUtility.UrlDecode(t[1]));
                }
            }
            return dic;
        }
        #endregion

        #region URL拼接字符串转换为强类型T
        /// <summary>
        /// URL拼接字符串转换为强类型T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static T ToEntity<T>(this string @this)
        {
            return @this.ToDictionary().ToJson().ToObject<T>();
        }
        #endregion

        #region 字符串半角转全角
        /// <summary>
        /// 半角转全角的函数(SBC case)
        /// </summary>
        /// <param name="this">源字符串</param>
        /// <returns>全角字符串</returns>
        public static string ToSBC(this string @this)
        {
            var c = @this.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 32)
                {
                    c[i] = (char)12288;
                    continue;
                }
                if (c[i] < 127) c[i] = (char)(c[i] + 65248);
            }
            return new string(c);
        }
        #endregion

        #region 字符串全角转半角
        /// <summary>
        /// 全角转半角的函数(DBC case)
        /// </summary>
        /// <param name="this">源字符串</param>
        /// <returns>半角字符串</returns>
        public static string ToDBC(this string @this)
        {
            var c = @this.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 12288)
                {
                    c[i] = (char)32;
                    continue;
                }
                if (c[i] > 65280 && c[i] < 65375) c[i] = (char)(c[i] - 65248);
            }
            string str = c.ToString().Replace("。", ".");
            return new string(c);
        }
        #endregion

        #region html加密
        /// <summary>
        /// html字符串加密
        /// </summary>
        /// <param name="this">html字符串</param>
        /// <param name="encodeBlank">空白加密，默认：是</param>
        /// <returns>string</returns>
        public static string ToHtmlEncode(this string @this, bool encodeBlank = true)
        {
            if ((@this == "") || (@this == null)) return "";
            var sb = new StringBuilder(@this);
            sb.Replace("&", "&amp;")
              .Replace("<", "&lt;")
              .Replace(">", "&gt;")
              .Replace("\"", "&quot;")
              .Replace("'", "&#39;")
              .Replace("\t", "&nbsp; &nbsp; ");
            if (encodeBlank) sb.Replace(" ", "&nbsp;");
            sb.Replace("\r", "")
              .Replace("\n\n", "<p><br/></p>")
              .Replace("\n", "<br />");
            return sb.ToString();
        }
        #endregion

        #region 文本加密
        /// <summary>
        /// 文本字符串加密
        /// </summary>
        /// <param name="this">字符串</param>
        /// <returns>string</returns>
        public static string ToTextEncode(this string @this)
        {
            var sb = new StringBuilder(@this);
            sb.Replace("&", "&amp;")
              .Replace("<", "&lt;")
              .Replace(">", "&gt;")
              .Replace("\"", "&quot;")
              .Replace("'", "&#39;");
            return sb.ToString();
        }
        #endregion

        #region 字符串反向
        /// <summary>
        /// 字符串方向
        /// </summary>
        /// <param name="this">字符串</param>
        /// <returns>string</returns>
        public static string ToReverse(this string @this)
        {
            var charArray = @this.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
        #endregion

        #region 中文拼音
        /// <summary>
        /// 中文转拼音
        /// </summary>
        /// <param name="this">中文字符串</param>
        /// <returns>string</returns>
        public static string ToPinyin(this string @this)
        {
            var iA = new int[]
            {
                 -20319 ,-20317 ,-20304 ,-20295 ,-20292 ,-20283 ,-20265 ,-20257 ,-20242 ,-20230
                 ,-20051 ,-20036 ,-20032 ,-20026 ,-20002 ,-19990 ,-19986 ,-19982 ,-19976 ,-19805
                 ,-19784 ,-19775 ,-19774 ,-19763 ,-19756 ,-19751 ,-19746 ,-19741 ,-19739 ,-19728
                 ,-19725 ,-19715 ,-19540 ,-19531 ,-19525 ,-19515 ,-19500 ,-19484 ,-19479 ,-19467
                 ,-19289 ,-19288 ,-19281 ,-19275 ,-19270 ,-19263 ,-19261 ,-19249 ,-19243 ,-19242
                 ,-19238 ,-19235 ,-19227 ,-19224 ,-19218 ,-19212 ,-19038 ,-19023 ,-19018 ,-19006
                 ,-19003 ,-18996 ,-18977 ,-18961 ,-18952 ,-18783 ,-18774 ,-18773 ,-18763 ,-18756
                 ,-18741 ,-18735 ,-18731 ,-18722 ,-18710 ,-18697 ,-18696 ,-18526 ,-18518 ,-18501
                 ,-18490 ,-18478 ,-18463 ,-18448 ,-18447 ,-18446 ,-18239 ,-18237 ,-18231 ,-18220
                 ,-18211 ,-18201 ,-18184 ,-18183 ,-18181 ,-18012 ,-17997 ,-17988 ,-17970 ,-17964
                 ,-17961 ,-17950 ,-17947 ,-17931 ,-17928 ,-17922 ,-17759 ,-17752 ,-17733 ,-17730
                 ,-17721 ,-17703 ,-17701 ,-17697 ,-17692 ,-17683 ,-17676 ,-17496 ,-17487 ,-17482
                 ,-17468 ,-17454 ,-17433 ,-17427 ,-17417 ,-17202 ,-17185 ,-16983 ,-16970 ,-16942
                 ,-16915 ,-16733 ,-16708 ,-16706 ,-16689 ,-16664 ,-16657 ,-16647 ,-16474 ,-16470
                 ,-16465 ,-16459 ,-16452 ,-16448 ,-16433 ,-16429 ,-16427 ,-16423 ,-16419 ,-16412
                 ,-16407 ,-16403 ,-16401 ,-16393 ,-16220 ,-16216 ,-16212 ,-16205 ,-16202 ,-16187
                 ,-16180 ,-16171 ,-16169 ,-16158 ,-16155 ,-15959 ,-15958 ,-15944 ,-15933 ,-15920
                 ,-15915 ,-15903 ,-15889 ,-15878 ,-15707 ,-15701 ,-15681 ,-15667 ,-15661 ,-15659
                 ,-15652 ,-15640 ,-15631 ,-15625 ,-15454 ,-15448 ,-15436 ,-15435 ,-15419 ,-15416
                 ,-15408 ,-15394 ,-15385 ,-15377 ,-15375 ,-15369 ,-15363 ,-15362 ,-15183 ,-15180
                 ,-15165 ,-15158 ,-15153 ,-15150 ,-15149 ,-15144 ,-15143 ,-15141 ,-15140 ,-15139
                 ,-15128 ,-15121 ,-15119 ,-15117 ,-15110 ,-15109 ,-14941 ,-14937 ,-14933 ,-14930
                 ,-14929 ,-14928 ,-14926 ,-14922 ,-14921 ,-14914 ,-14908 ,-14902 ,-14894 ,-14889
                 ,-14882 ,-14873 ,-14871 ,-14857 ,-14678 ,-14674 ,-14670 ,-14668 ,-14663 ,-14654
                 ,-14645 ,-14630 ,-14594 ,-14429 ,-14407 ,-14399 ,-14384 ,-14379 ,-14368 ,-14355
                 ,-14353 ,-14345 ,-14170 ,-14159 ,-14151 ,-14149 ,-14145 ,-14140 ,-14137 ,-14135
                 ,-14125 ,-14123 ,-14122 ,-14112 ,-14109 ,-14099 ,-14097 ,-14094 ,-14092 ,-14090
                 ,-14087 ,-14083 ,-13917 ,-13914 ,-13910 ,-13907 ,-13906 ,-13905 ,-13896 ,-13894
                 ,-13878 ,-13870 ,-13859 ,-13847 ,-13831 ,-13658 ,-13611 ,-13601 ,-13406 ,-13404
                 ,-13400 ,-13398 ,-13395 ,-13391 ,-13387 ,-13383 ,-13367 ,-13359 ,-13356 ,-13343
                 ,-13340 ,-13329 ,-13326 ,-13318 ,-13147 ,-13138 ,-13120 ,-13107 ,-13096 ,-13095
                 ,-13091 ,-13076 ,-13068 ,-13063 ,-13060 ,-12888 ,-12875 ,-12871 ,-12860 ,-12858
                 ,-12852 ,-12849 ,-12838 ,-12831 ,-12829 ,-12812 ,-12802 ,-12607 ,-12597 ,-12594
                 ,-12585 ,-12556 ,-12359 ,-12346 ,-12320 ,-12300 ,-12120 ,-12099 ,-12089 ,-12074
                 ,-12067 ,-12058 ,-12039 ,-11867 ,-11861 ,-11847 ,-11831 ,-11798 ,-11781 ,-11604
                 ,-11589 ,-11536 ,-11358 ,-11340 ,-11339 ,-11324 ,-11303 ,-11097 ,-11077 ,-11067
                 ,-11055 ,-11052 ,-11045 ,-11041 ,-11038 ,-11024 ,-11020 ,-11019 ,-11018 ,-11014
                 ,-10838 ,-10832 ,-10815 ,-10800 ,-10790 ,-10780 ,-10764 ,-10587 ,-10544 ,-10533
                 ,-10519 ,-10331 ,-10329 ,-10328 ,-10322 ,-10315 ,-10309 ,-10307 ,-10296 ,-10281
                 ,-10274 ,-10270 ,-10262 ,-10260 ,-10256 ,-10254
             };
            var sA = new string[]
            {
                "a","ai","an","ang","ao"
                ,"ba","bai","ban","bang","bao","bei","ben","beng","bi","bian","biao","bie","bin"
                ,"bing","bo","bu"
                ,"ca","cai","can","cang","cao","ce","ceng","cha","chai","chan","chang","chao","che"
                ,"chen","cheng","chi","chong","chou","chu","chuai","chuan","chuang","chui","chun"
                ,"chuo","ci","cong","cou","cu","cuan","cui","cun","cuo"
                ,"da","dai","dan","dang","dao","de","deng","di","dian","diao","die","ding","diu"
                ,"dong","dou","du","duan","dui","dun","duo"
                ,"e","en","er"
                ,"fa","fan","fang","fei","fen","feng","fo","fou","fu"
                ,"ga","gai","gan","gang","gao","ge","gei","gen","geng","gong","gou","gu","gua","guai"
                ,"guan","guang","gui","gun","guo"
                ,"ha","hai","han","hang","hao","he","hei","hen","heng","hong","hou","hu","hua","huai"
                ,"huan","huang","hui","hun","huo"
                ,"ji","jia","jian","jiang","jiao","jie","jin","jing","jiong","jiu","ju","juan","jue"
                ,"jun"
                ,"ka","kai","kan","kang","kao","ke","ken","keng","kong","kou","ku","kua","kuai","kuan"
                ,"kuang","kui","kun","kuo"
                ,"la","lai","lan","lang","lao","le","lei","leng","li","lia","lian","liang","liao","lie"
                ,"lin","ling","liu","long","lou","lu","lv","luan","lue","lun","luo"
                ,"ma","mai","man","mang","mao","me","mei","men","meng","mi","mian","miao","mie","min"
                ,"ming","miu","mo","mou","mu"
                ,"na","nai","nan","nang","nao","ne","nei","nen","neng","ni","nian","niang","niao","nie"
                ,"nin","ning","niu","nong","nu","nv","nuan","nue","nuo"
                ,"o","ou"
                ,"pa","pai","pan","pang","pao","pei","pen","peng","pi","pian","piao","pie","pin","ping"
                ,"po","pu"
                ,"qi","qia","qian","qiang","qiao","qie","qin","qing","qiong","qiu","qu","quan","que"
                ,"qun"
                ,"ran","rang","rao","re","ren","reng","ri","rong","rou","ru","ruan","rui","run","ruo"
                ,"sa","sai","san","sang","sao","se","sen","seng","sha","shai","shan","shang","shao","she"
                ,"shen","sheng","shi","shou","shu","shua","shuai","shuan","shuang","shui","shun","shuo","si"
                ,"song","sou","su","suan","sui","sun","suo"
                ,"ta","tai","tan","tang","tao","te","teng","ti","tian","tiao","tie","ting","tong","tou","tu"
                ,"tuan","tui","tun","tuo"
                ,"wa","wai","wan","wang","wei","wen","weng","wo","wu"
                ,"xi","xia","xian","xiang","xiao","xie","xin","xing","xiong","xiu","xu","xuan","xue","xun"
                ,"ya","yan","yang","yao","ye","yi","yin","ying","yo","yong","you","yu","yuan","yue","yun"
                ,"za","zai","zan","zang","zao","ze","zei","zen","zeng","zha","zhai","zhan","zhang","zhao"
                ,"zhe","zhen","zheng","zhi","zhong","zhou","zhu","zhua","zhuai","zhuan","zhuang","zhui"
                ,"zhun","zhuo","zi","zong","zou","zu","zuan","zui","zun","zuo"
            };
            var B = new byte[2];
            var s = "";
            var c = @this.ToCharArray();
            for (var j = 0; j < c.Length; j++)
            {
                B = Encoding.Default.GetBytes(c[j].ToString());
                if (B[0] <= 160 && B[0] >= 0)
                {
                    s += c[j];
                }
                else
                {
                    for (var i = (iA.Length - 1); i >= 0; i--)
                    {
                        if (iA[i] <= B[0] * 256 + B[1] - 65536)
                        {
                            s += sA[i];
                            break;
                        }
                    }
                }
            }
            return s;
        }
        #endregion

        #region json字符串反序列化为强类型对象
        /// <summary>
        /// json字符串反序列化为强类型对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="this">json字符串</param>
        /// <returns>T</returns>
        public static T ToObject<T>(this string @this)
        {
            if (@this.IsJsonObjectString() || @this.IsJsonArrayString())
            {
                return JsonConvert.DeserializeObject<T>(@this);
            }
            else
            {
                return @this.ToOrDefault<T>();
            }
        }
        #endregion

        #region 判断指定字符串是否对象类型的Json字符串格式
        /// <summary>
        /// 判断指定字符串是否对象类型的Json字符串格式
        /// </summary>
        /// <param name="this">json字符串</param>
        /// <returns>bool</returns>
        public static bool IsJsonObjectString(this string @this)
        {
            return @this != null && @this.StartsWith("{") && @this.EndsWith("}");
        }
        #endregion

        #region 判断指定字符串是否集合类型的Json字符串格式
        /// <summary>
        /// 判断指定字符串是否集合类型的Json字符串格式
        /// </summary>
        /// <param name="this">json字符串</param>
        /// <returns>bool</returns>
        public static bool IsJsonArrayString(this string @this)
        {
            return @this != null && @this.StartsWith("[") && @this.EndsWith("]");
        }
        #endregion

        #region json字符串转换为JObject对象
        /// <summary>
        /// json字符串转换为JObject对象
        /// </summary>
        /// <param name="this">json字符串</param>
        /// <returns>JObject</returns>
        public static JObject ToJObject(this string @this)
        {
            if (@this.IsJsonObjectString())
            {
                return JObject.Parse(@this);
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region json字符串转换为JArray对象
        /// <summary>
        /// json字符串转换为JArray对象
        /// </summary>
        /// <param name="this">json字符串</param>
        /// <returns>JArray</returns>
        public static JArray ToJArray(this string @this)
        {
            if (@this.IsJsonArrayString())
            {
                return JArray.Parse(@this);
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region 判断是否是Json字符串
        /// <summary>
        /// 验证字符串是否是Json字符串
        /// </summary>
        /// <param name="this">json字符串</param>
        /// <returns>bool</returns>
        public static bool IsJsonString(this string @this)
        {
            try
            {
                if (@this.IsJsonObjectString() && @this.ToJObject() != null)
                    return true;
                if (@this.IsJsonArrayString() && @this.ToJArray() != null)
                    return true;
                return false;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region 字符串转日期
        /// <summary>
        /// GMT字符串/时间戳转DateTime
        /// </summary>
        /// <param name="this">GMT日期字符串</param>
        /// <returns>DateTime</returns>
        public static DateTime ToDateTime(this string @this)
        {
            var dt = DateTime.MinValue;
            if (!@this.IsNull())
            {
                #region GMT日期
                if (@this.ToUpper().Contains("GMT") == true)
                {
                    var pattern = "";
                    if (@this.IndexOf("+0") != -1)
                    {
                        @this = @this.Replace("GMT", "");
                        pattern = "ddd, dd MMM yyyy HH':'mm':'ss zzz";
                    }
                    if (@this.ToUpper().IndexOf("GMT") != -1) pattern = "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'";
                    if (pattern != "")
                    {
                        dt = DateTime.ParseExact(@this, pattern, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                        dt = dt.ToLocalTime();
                    }
                    else
                    {
                        dt = Convert.ToDateTime(@this);
                    }
                }
                #endregion

                #region 时间戳
                else
                {
                    var start = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Local);
                    var type = @this.Length;
                    if (type == 10)
                    {
                        var ticks = long.Parse(@this + "0000000");
                        var time = new TimeSpan(ticks);
                        dt = start.Add(time);
                    }
                    else if (type == 13)
                    {
                        var ticks = long.Parse(@this + "0000");
                        var time = new TimeSpan(ticks);
                        dt = start.Add(time);
                    }
                }
                #endregion
            }
            return dt;
        }
        #endregion

        #region 首字母转换
        /// <summary>
        /// 首字母转换
        /// </summary>
        /// <param name="this">源字符串</param>
        /// <param name="converter">转换委托</param>
        /// <returns>string</returns>
        public static string ConvertFirstCharacter(this string @this, Func<string, string> converter)
        {
            if (string.IsNullOrEmpty(@this))
            {
                return string.Empty;
            }
            return string.Concat(converter(@this.Substring(0, 1)), @this.Substring(1));
        }
        #endregion

        #region 驼峰命名法
        /// <summary>
        /// 转换为驼峰命名法
        /// </summary>
        /// <param name="this">源字符串</param>
        /// <returns>string</returns>
        public static string ToCamelCase(this string @this)
        {
            return @this.ConvertFirstCharacter(x => x.ToLowerInvariant());
        }
        #endregion

        #region 帕斯卡命名法
        /// <summary>
        /// 转换为帕斯卡命名法
        /// </summary>
        /// <param name="this">源字符串</param>
        /// <returns>string</returns>
        public static string ToPascalCase(this string @this)
        {
            return @this.ConvertFirstCharacter(x => x.ToUpperInvariant());
        }
        #endregion

        #region Base64加密
        /// <summary>
        /// 字符串Base64加密
        /// </summary>
        /// <param name="this">源字符串</param>
        /// <param name="encoding">编码方式，默认：utf-8</param>
        /// <returns>加密后的字符串</returns>
        public static string ToBase64(this string @this, Encoding encoding = null)
        {
            var result = string.Empty;
            try
            {
                var bytes = (encoding ?? Encoding.UTF8).GetBytes(@this);
                result = Convert.ToBase64String(bytes);
            }
            catch
            {
                result = @this;
            }
            return result;
        }
        #endregion

        #region Base64解密
        /// <summary>
        /// 字符串Base64解密
        /// </summary>
        /// <param name="this">源字符串</param>
        /// <param name="encoding">编码方式，默认：utf-8</param>
        /// <returns>解密后的字符串</returns>
        public static string DecodeBase64(this string @this, Encoding encoding = null)
        {
            var result = string.Empty;
            try
            {
                var bytes = Convert.FromBase64String(@this);
                result = (encoding ?? Encoding.UTF8).GetString(bytes);
            }
            catch
            {
                result = @this;
            }
            return result;
        }
        #endregion

        #region 字符串比较
        /// <summary>
        /// 忽略大小写的字符串相等比较，判断是否以任意一个待比较字符串相等
        /// </summary>
        /// <param name="this">当前字符串</param>
        /// <param name="strs">待比较字符串数组</param>
        /// <returns>bool</returns>
        public static bool EqualIgnoreCase(this string @this, params string[] strs)
        {
            foreach (var item in strs)
            {
                if (string.Equals(@this, item, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        /// <summary>
        /// 忽略大小写的字符串开始比较，判断是否以任意一个待比较字符串开始
        /// </summary>
        /// <param name="this">当前字符串</param>
        /// <param name="strs">待比较字符串数组</param>
        /// <returns>bool</returns>
        public static bool StartsWithIgnoreCase(this string @this, params string[] strs)
        {
            if (string.IsNullOrEmpty(@this)) return false;
            foreach (var item in strs)
            {
                if (@this.StartsWith(item, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        /// <summary>
        /// 忽略大小写的字符串结束比较，判断是否以任意一个待比较字符串结束
        /// </summary>
        /// <param name="value">当前字符串</param>
        /// <param name="strs">待比较字符串数组</param>
        /// <returns>bool</returns>
        public static bool EndsWithIgnoreCase(this string value, params string[] strs)
        {
            if (string.IsNullOrEmpty(value)) return false;
            foreach (var item in strs)
            {
                if (value.EndsWith(item, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }
        #endregion

        #region 拆分字符串
        /// <summary>
        /// 拆分字符串，过滤空格，无效时返回空数组
        /// </summary>
        /// <param name="this">当前字符串</param>
        /// <param name="separators">分组分隔符，默认逗号分号</param>
        /// <returns>string[]</returns>
        public static string[] Split(this string @this, params string[] separators)
        {
            if (string.IsNullOrEmpty(@this)) return new string[0];
            if (separators == null || separators.Length < 1 || separators.Length == 1 && separators[0].IsNullOrEmpty()) separators = new string[] { ",", ";" };
            return @this.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// 拆分字符串成为整型数组，默认逗号分号分隔，无效时返回空数组
        /// </summary>
        /// <remarks>
        /// 过滤空格、过滤无效、不过滤重复
        /// </remarks>
        /// <param name="this">当前字符串</param>
        /// <param name="separators">分组分隔符，默认逗号分号</param>
        /// <returns>int[]</returns>
        public static int[] SplitAsInt(this string @this, params string[] separators)
        {
            if (string.IsNullOrEmpty(@this)) return new int[0];
            if (separators == null || separators.Length < 1) separators = new string[] { ",", ";" };
            var arr = @this.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            var list = new List<int>();
            foreach (var item in arr)
            {
                if (!int.TryParse(item.Trim(), out var id)) continue;
                // 本意只是拆分字符串然后转为数字，不应该过滤重复项
                //if (!list.Contains(id))
                list.Add(id);
            }
            return list.ToArray();
        }

        /// <summary>
        /// 拆分字符串成为不区分大小写的可空名值字典。逗号分号分组，等号分隔
        /// </summary>
        /// <param name="this">当前字符串</param>
        /// <param name="nameValueSeparator">名值分隔符，默认等于号</param>
        /// <param name="separators">分组分隔符，默认逗号分号</param>
        /// <returns>string</returns>
        public static IDictionary<string, string> SplitAsDictionary(this string @this, string nameValueSeparator = "=", params string[] separators)
        {
            var dic = new Dictionary<string, string>();
            if (@this.IsNullOrWhiteSpace()) return dic;
            if (string.IsNullOrEmpty(nameValueSeparator)) nameValueSeparator = "=";
            if (separators == null || separators.Length < 1) separators = new string[] { ",", ";" };
            var arr = @this.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            if (arr == null || arr.Length < 1) return null;
            foreach (var item in arr)
            {
                var p = item.IndexOf(nameValueSeparator);
                // 在前后都不行
                if (p <= 0 || p >= item.Length - 1) continue;
                var key = item.Substring(0, p).Trim();
                dic[key] = item.Substring(p + nameValueSeparator.Length).Trim();
            }
            return dic;
        }
        #endregion

        #region 字符串转数组
        /// <summary>
        /// 字符串转数组
        /// </summary>
        /// <param name="this">当前字符串</param>
        /// <param name="encoding">编码，默认utf-8无BOM</param>
        /// <returns>string</returns>
        public static byte[] GetBytes(this string @this, Encoding encoding = null)
        {
            if (@this == null) return null;
            if (@this == string.Empty) return new byte[0];
            if (encoding == null) encoding = Encoding.UTF8;
            return encoding.GetBytes(@this);
        }
        #endregion

        #region 字符串截取
        /// <summary>
        /// 确保字符串以指定的另一字符串开始，不区分大小写
        /// </summary>
        /// <param name="this">当前字符串</param>
        /// <param name="start">开始字符串</param>
        /// <returns>string</returns>
        public static string EnsureStart(this string @this, string start)
        {
            if (string.IsNullOrEmpty(start)) return @this;
            if (string.IsNullOrEmpty(@this)) return start;
            if (@this.StartsWith(start, StringComparison.OrdinalIgnoreCase)) return @this;
            return start + @this;
        }

        /// <summary>
        /// 确保字符串以指定的另一字符串结束，不区分大小写
        /// </summary>
        /// <param name="this">当前字符串</param>
        /// <param name="end">结束字符串</param>
        /// <returns>string</returns>
        public static string EnsureEnd(this string @this, string end)
        {
            if (string.IsNullOrEmpty(end)) return @this;
            if (string.IsNullOrEmpty(@this)) return end;
            if (@this.EndsWith(end, StringComparison.OrdinalIgnoreCase)) return @this;
            return @this + end;
        }

        /// <summary>
        /// 从当前字符串开头移除另一字符串，不区分大小写，循环多次匹配前缀
        /// </summary>
        /// <param name="this">当前字符串</param>
        /// <param name="starts">另一字符串</param>
        /// <returns>string</returns>
        public static string TrimStart(this string @this, params string[] starts)
        {
            if (string.IsNullOrEmpty(@this)) return @this;
            if (starts == null || starts.Length < 1 || string.IsNullOrEmpty(starts[0])) return @this;
            for (var i = 0; i < starts.Length; i++)
            {
                if (@this.StartsWith(starts[i], StringComparison.OrdinalIgnoreCase))
                {
                    @this = @this.Substring(starts[i].Length);
                    if (string.IsNullOrEmpty(@this)) break;
                    // 从头开始
                    i = -1;
                }
            }
            return @this;
        }

        /// <summary>
        /// 从当前字符串结尾移除另一字符串，不区分大小写，循环多次匹配后缀
        /// </summary>
        /// <param name="this">当前字符串</param>
        /// <param name="ends">另一字符串</param>
        /// <returns>string</returns>
        public static string TrimEnd(this string @this, params string[] ends)
        {
            if (string.IsNullOrEmpty(@this)) return @this;
            if (ends == null || ends.Length < 1 || string.IsNullOrEmpty(ends[0])) return @this;
            for (var i = 0; i < ends.Length; i++)
            {
                if (@this.EndsWith(ends[i], StringComparison.OrdinalIgnoreCase))
                {
                    @this = @this.Substring(0, @this.Length - ends[i].Length);
                    if (string.IsNullOrEmpty(@this)) break;
                    // 从头开始
                    i = -1;
                }
            }
            return @this;
        }

        /// <summary>
        /// 从分隔符开始向尾部截取字符串
        /// </summary>
        /// <param name="this">源字符串</param>
        /// <param name="separator">分隔符</param>
        /// <param name="lastIndexOf">true：从最后一个匹配的分隔符开始截取，false：从第一个匹配的分隔符开始截取，默认：true</param>
        /// <returns>string</returns>
        public static string Substring(this string @this, string separator, bool lastIndexOf = true)
        {
            var start = (lastIndexOf ? @this.LastIndexOf(separator) : @this.IndexOf(separator)) + separator.Length;
            var length = @this.Length - start;
            return @this.Substring(start, length);
        }

        /// <summary>
        /// 根据开始和结束字符串截取字符串
        /// </summary>
        /// <param name="this">源字符串</param>
        /// <param name="begin">开始字符串</param>
        /// <param name="end">结束字符串</param>
        /// <param name="beginIsIndexOf">开始字符串是否是IndexOf，默认true，否则LastIndexOf</param>
        /// <param name="endIsIndexOf">结束字符串是否是IndexOf，默认true，否则LastIndexOf</param>
        /// <returns>string</returns>
        public static string Substring(this string @this, string begin, string end, bool beginIsIndexOf = true, bool endIsIndexOf = true)
        {
            if (string.IsNullOrEmpty(@this))
                return "";
            if (string.IsNullOrEmpty(begin))
                return "";
            if (string.IsNullOrEmpty(end))
                return "";

            int li;
            if (beginIsIndexOf)
                li = @this.IndexOf(begin);
            else
                li = @this.LastIndexOf(begin);
            if (li == -1)
                return "";

            li += begin.Length;

            int ri;
            if (endIsIndexOf)
                ri = @this.IndexOf(end, li);
            else
                ri = @this.LastIndexOf(end);
            if (ri == -1)
                return "";

            return @this.Substring(li, ri - li);
        }

        /// <summary>
        /// 从字符串中检索子字符串，在指定头部字符串之后，指定尾部字符串之前
        /// </summary>
        /// <remarks>
        /// 常用于截取xml某一个元素等操作
        /// </remarks>
        /// <param name="this">当前字符串</param>
        /// <param name="after">头部字符串，在它之后</param>
        /// <param name="before">尾部字符串，在它之前</param>
        /// <param name="startIndex">搜索的开始位置</param>
        /// <param name="positions">位置数组，两个元素分别记录头尾位置</param>
        /// <returns>string</returns>
        public static string Cutstring(this string @this, string after, string before = null, int startIndex = 0, int[] positions = null)
        {
            if (string.IsNullOrEmpty(@this)) return @this;
            if (string.IsNullOrEmpty(after) && string.IsNullOrEmpty(before)) return @this;

            /*
             * 1，只有start，从该字符串之后部分
             * 2，只有end，从开头到该字符串之前
             * 3，同时start和end，取中间部分
             */

            var p = -1;
            if (!string.IsNullOrEmpty(after))
            {
                p = @this.IndexOf(after, startIndex);
                if (p < 0) return null;
                p += after.Length;
                // 记录位置
                if (positions != null && positions.Length > 0) positions[0] = p;
            }
            if (string.IsNullOrEmpty(before)) return @this.Substring(p);
            var f = @this.IndexOf(before, p >= 0 ? p : startIndex);
            if (f < 0) return null;
            // 记录位置
            if (positions != null && positions.Length > 1) positions[1] = f;
            if (p >= 0)
                return @this.Substring(p, f - p);
            else
                return @this.Substring(0, f);
        }

        /// <summary>
        /// 截断包含中文字符串
        /// </summary>
        /// <param name="this">源字符串</param>
        /// <param name="length">长度</param>
        /// <returns>string</returns>
        public static string Cutstring(this string @this, int length)
        {
            var tLength = 0;
            tLength = @this.StringLength();
            if (tLength <= length) return @this;
            var strArray = @this.ToCharArray();
            var subStr = string.Empty;
            tLength = 0;
            for (int i = 0; i < strArray.Length; i++)
            {
                if (tLength >= length) break;
                subStr += strArray[i];
                if (IsChinese(strArray[i].ToString()))
                {
                    tLength += 2;
                }
                else
                {
                    tLength += 1;
                }
            }
            return subStr + "...";
        }

        /// <summary>
        /// 根据最大长度截取字符串，并允许以指定空白填充末尾
        /// </summary>
        /// <param name="this">当前字符串</param>
        /// <param name="maxLength">截取后字符串的最大允许长度，包含后面填充</param>
        /// <param name="pad">需要填充在后面的字符串，比如几个圆点</param>
        /// <returns>string</returns>
        public static string Cut(this string @this, int maxLength, string pad = null)
        {
            if (string.IsNullOrEmpty(@this) || maxLength <= 0 || @this.Length < maxLength) return @this;
            // 计算截取长度
            var len = maxLength;
            if (!string.IsNullOrEmpty(pad)) len -= pad.Length;
            if (len <= 0) return pad;
            return @this.Substring(0, len) + pad;
        }

        /// <summary>
        /// 从当前字符串开头移除另一字符串以及之前的部分
        /// </summary>
        /// <param name="this">当前字符串</param>
        /// <param name="starts">另一字符串</param>
        /// <returns>string</returns>
        public static string CutStart(this string @this, params string[] starts)
        {
            if (string.IsNullOrEmpty(@this)) return @this;
            if (starts == null || starts.Length < 1 || string.IsNullOrEmpty(starts[0])) return @this;
            for (var i = 0; i < starts.Length; i++)
            {
                var p = @this.IndexOf(starts[i]);
                if (p >= 0)
                {
                    @this = @this.Substring(p + starts[i].Length);
                    if (string.IsNullOrEmpty(@this)) break;
                }
            }
            return @this;
        }

        /// <summary>
        /// 从当前字符串结尾移除另一字符串以及之后的部分
        /// </summary>
        /// <param name="this">当前字符串</param>
        /// <param name="ends">另一字符串</param>
        /// <returns>string</returns>
        public static string CutEnd(this string @this, params string[] ends)
        {
            if (string.IsNullOrEmpty(@this)) return @this;
            if (ends == null || ends.Length < 1 || string.IsNullOrEmpty(ends[0])) return @this;
            for (var i = 0; i < ends.Length; i++)
            {
                var p = @this.LastIndexOf(ends[i]);
                if (p >= 0)
                {
                    @this = @this.Substring(0, p);
                    if (string.IsNullOrEmpty(@this)) break;
                }
            }
            return @this;
        }
        #endregion

        #region 字符串格式化
        /// <summary>
        /// 格式化字符串。特别支持无格式化字符串的时间参数
        /// </summary>
        /// <param name="this">格式字符串</param>
        /// <param name="args">参数</param>
        /// <returns></returns>
        public static string F(this string @this, params object[] args)
        {
            if (string.IsNullOrEmpty(@this)) return @this;
            // 特殊处理时间格式化。这些年，无数项目实施因为时间格式问题让人发狂
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i] is DateTime)
                {
                    // 没有写格式化字符串的时间参数，一律转为标准时间字符串
                    if (@this.Contains("{" + i + "}")) args[i] = ((DateTime)args[i]).ToDateTimeString();
                }
            }
            return string.Format(@this, args);
        }
        #endregion

        #region 字符串累加器
        /// <summary>
        /// 字符串累加器
        /// </summary>
        /// <param name="this">源字符串</param>
        /// <param name="separator">分隔符</param>
        /// <param name="seed">初始种子</param>
        /// <param name="current">累加选项</param>
        /// <param name="remove">最后移除字符串</param>
        /// <param name="isEnableNullValue">空值是否有效，默认：false</param>
        /// <param name="distinct">是否去重，默认：true</param>
        /// <returns></returns>
        public static string Aggregate(this string @this, char separator, string seed, Func<string, string> current, string remove, bool isEnableNullValue = false, bool distinct = true)
        {
            if (!@this.IsNullOrEmpty())
            {
                var sb = new StringBuilder(seed);
                IEnumerable<string> array = @this.TrimEnd(separator).Split(separator);
                if (!isEnableNullValue)
                {
                    array = array.Where(o => !o.IsNullOrEmpty());
                }
                if (distinct)
                {
                    array = array.Distinct();
                }
                foreach (var item in array)
                {
                    sb.Append(current(item));
                }
                if (array.Count() > 0 && !remove.IsNullOrEmpty())
                {
                    sb = sb.Remove(sb.ToString().LastIndexOf(remove), remove.Length);
                }
                return sb.ToString();
            }
            return @this;
        }

        /// <summary>
        /// 字符串累加器
        /// </summary>
        /// <param name="this">源字符串</param>
        /// <param name="seed">初始种子</param>
        /// <param name="current">累加选项</param>
        /// <param name="remove">最后移除字符串</param>
        /// <param name="isEnableNullValue">空值是否有效，默认：false</param>
        /// <param name="distinct">是否去重，默认：true</param>
        /// <returns></returns>
        public static string Aggregate<T>(this IEnumerable<T> @this, string seed, Func<T, string> current, string remove, bool isEnableNullValue = false, bool distinct = true)
        {
            if (@this?.Count() > 0)
            {
                var sb = new StringBuilder(seed);
                if (!isEnableNullValue)
                {
                    @this = @this.Where(o => o?.ToString().IsNullOrEmpty() == false);
                }
                if (distinct)
                {
                    @this = @this.Distinct();
                }
                foreach (var item in @this)
                {
                    sb.Append(current(item));
                }
                if (@this.Count() > 0 && !remove.IsNullOrEmpty())
                {
                    sb = sb.Remove(sb.ToString().LastIndexOf(remove), remove.Length);
                }
                return sb.ToString();
            }
            return null;
        }
        #endregion

        #region 转换为安全的sql
        /// <summary>
        /// 转换为安全的sql
        /// </summary>
        /// <param name="this">源字符串</param>
        /// <returns></returns>
        public static string ToSafeSql(this string @this)
        {
            if (!string.IsNullOrEmpty(@this))
            {
                var sql = "or|exec|execute|insert|select|delete|update|alter|create|drop|count|and|where|in|like|chr|char|asc|mid|substring|master|truncate|declare|xp_cmdshell|restore|backup|net +user|'|>|<|=|,";
                var array = sql.Split('|');
                foreach (var item in array)
                {
                    @this = @this.Replace(item, "").Replace(item.ToUpper(), "");
                }
            }
            return @this;
        }
        #endregion

        #region 正则替换
        /// <summary>
        /// 正则替换
        /// </summary>
        /// <param name="this">源字符串</param>
        /// <param name="replacement">替换内容</param>
        /// <param name="pattern">正则表达式</param>
        /// <returns>替换后的字符串</returns>
        public static string ReplaceOfRegex(this string @this, string replacement = "", string pattern = @"\s")
        {
            if (!string.IsNullOrEmpty(@this))
            {
                @this = Regex.Replace(@this, pattern, replacement);
            }
            return @this;
        }
        #endregion

        #region 获取html内容的第一张图片Url地址
        /// <summary>
        /// 获取html内容的第一张图片Url地址
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string GetFirstImageUrl(this string source)
        {
            string rc = "";

            if (!string.IsNullOrWhiteSpace(source?.Trim()))
            {
                Regex reg = new Regex(@"<\s*?img\s*?src=['""](.*?[^'])['""]", RegexOptions.IgnoreCase);
                Match match = reg.Match(source);
                if (match.Success && match.Groups?.Count > 1)
                    rc = match.Groups[1].Value;
            }

            return rc;
        }
        #endregion

        #region 截取内容的部分字符作为摘要，默认提取100个字符
        /// <summary>
        /// 截取内容的部分字符作为摘要，默认提取100个字符
        /// </summary>
        /// <param name="source"></param>
        /// <param name="length">需要截取字符串的长度</param>
        /// <param name="isStripHtml">是否清除HTML代码 true:是 false:否</param>
        /// <returns></returns>
        public static string GetAbstract(this string source, int length = 100, bool isStripHtml = true)
        {
            if (string.IsNullOrEmpty(source) || length == 0)
                return "";
            if (isStripHtml)
            {
                Regex re = new Regex("<[^>]*>");
                source = re.Replace(source, "");
                source = source.Replace("　", "").Replace(" ", "");
                if (source.Length <= length)
                    return source;
                else
                    return source.Substring(0, length) + "……";
            }
            else
            {
                if (source.Length <= length)
                    return source;

                int pos = 0, npos = 0, size = 0;
                bool firststop = false, notr = false, noli = false;
                StringBuilder sb = new StringBuilder();
                while (true)
                {
                    if (pos >= source.Length)
                        break;
                    string cur = source.Substring(pos, 1);
                    if (cur == "<")
                    {
                        string next = source.Substring(pos + 1, 3).ToLower();
                        if (next.IndexOf("p") == 0 && next.IndexOf("pre") != 0)
                        {
                            npos = source.IndexOf(">", pos) + 1;
                        }
                        else if (next.IndexOf("/p") == 0 && next.IndexOf("/pr") != 0)
                        {
                            npos = source.IndexOf(">", pos) + 1;
                            if (size < length)
                                sb.Append("<br/>");
                        }
                        else if (next.IndexOf("br") == 0)
                        {
                            npos = source.IndexOf(">", pos) + 1;
                            if (size < length)
                                sb.Append("<br/>");
                        }
                        else if (next.IndexOf("img") == 0)
                        {
                            npos = source.IndexOf(">", pos) + 1;
                            if (size < length)
                            {
                                sb.Append(source.Substring(pos, npos - pos));
                                size += npos - pos + 1;
                            }
                        }
                        else if (next.IndexOf("li") == 0 || next.IndexOf("/li") == 0)
                        {
                            npos = source.IndexOf(">", pos) + 1;
                            if (size < length)
                            {
                                sb.Append(source.Substring(pos, npos - pos));
                            }
                            else
                            {
                                if (!noli && next.IndexOf("/li") == 0)
                                {
                                    sb.Append(source.Substring(pos, npos - pos));
                                    noli = true;
                                }
                            }
                        }
                        else if (next.IndexOf("tr") == 0 || next.IndexOf("/tr") == 0)
                        {
                            npos = source.IndexOf(">", pos) + 1;
                            if (size < length)
                            {
                                sb.Append(source.Substring(pos, npos - pos));
                            }
                            else
                            {
                                if (!notr && next.IndexOf("/tr") == 0)
                                {
                                    sb.Append(source.Substring(pos, npos - pos));
                                    notr = true;
                                }
                            }
                        }
                        else if (next.IndexOf("td") == 0 || next.IndexOf("/td") == 0)
                        {
                            npos = source.IndexOf(">", pos) + 1;
                            if (size < length)
                            {
                                sb.Append(source.Substring(pos, npos - pos));
                            }
                            else
                            {
                                if (!notr)
                                {
                                    sb.Append(source.Substring(pos, npos - pos));
                                }
                            }
                        }
                        else
                        {
                            npos = source.IndexOf(">", pos) + 1;
                            sb.Append(source.Substring(pos, npos - pos));
                        }
                        if (npos <= pos)
                            npos = pos + 1;
                        pos = npos;
                    }
                    else
                    {
                        if (size < length)
                        {
                            sb.Append(cur);
                            size++;
                        }
                        else
                        {
                            if (!firststop)
                            {
                                sb.Append("……");
                                firststop = true;
                            }
                        }
                        pos++;
                    }

                }
                return sb.ToString();
            }
        }
        #endregion

        #region 获取每个汉字的首字母(大写)
        /// <summary>
        /// 获取每个汉字的首字母(大写)
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string GetFirstLetterForEachChineseCharacter(this string source)
        {
            return GetFirstLetterForEachChineseCharacterMultipal(source)?.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)?.FirstOrDefault();
        }
        #endregion

        #region 获取每个汉字的首字母(大写)，如果是多音字，则以半角逗号分隔每个拼音组合。
        /// <summary>
        /// 获取每个汉字的首字母(大写)，如果是多音字，则以半角逗号分隔每个拼音组合。
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string GetFirstLetterForEachChineseCharacterMultipal(this string source)
        {
            source = source?.Trim();
            if (string.IsNullOrWhiteSpace(source)) return "";

            int i, j, k, m;
            string tmpStr;
            string returnStr = ""; //返回最终结果的字符串
            string[] tmpArr;
            for (i = 0; i < source.Length; i++)
            { //处理汉字字符串,对每个汉字的首字母进行一次循环
                tmpStr = GetPinyin((char)source[i]); //获取第i个汉字的拼音首字母,可能为1个或多个
                if (tmpStr.Length > 0)
                { //汉字的拼音首字母存在的情况才进行操作
                    if (returnStr != "")
                    { //不是第一个汉字
                        Regex regex = new Regex(",");
                        tmpArr = regex.Split(returnStr);
                        returnStr = "";
                        for (k = 0; k < tmpArr.Length; k++)
                        {
                            for (j = 0; j < tmpStr.Length; j++) //对返回的每个首字母进行拼接
                            {
                                string charcode = tmpStr[j].ToString(); //取出第j个拼音字母
                                returnStr += tmpArr[k] + charcode + ",";
                            }
                        }
                        if (returnStr != "")
                            returnStr = returnStr.Substring(0, returnStr.Length - 1);
                    }
                    else
                    { //构造第一个汉字返回结果
                        for (m = 0; m < tmpStr.Length - 1; m++)
                            returnStr += tmpStr[m] + ",";
                        returnStr += tmpStr[tmpStr.Length - 1];
                    }
                }
            }
            return returnStr; //返回处理结果字符串，以，分隔每个拼音组合
        }

        //获取单个汉字对应的拼音首字符字符串
        private static string GetPinyin(char chineseCharacter)
        {
            // 汉字拼音首字母列表 本列表包含了20902个汉字,收录的字符的Unicode编码范围为19968至40869
            string strChineseFirstPY = "YDYQSXMWZSSXJBYMGCCZQPSSQBYCDSCDQLDYLYBSSJGYZZJJFKCCLZDHWDWZJLJPFYYNWJJTMYHZWZHFLZPPQHGSCYYYNJQYXXGJHHSDSJNKKTMOMLCRXYPSNQSECCQZGGLLYJLMYZZSECYKYYHQWJSSGGYXYZYJWWKDJHYCHMYXJTLXJYQBYXZLDWRDJRWYSRLDZJPCBZJJBRCFTLECZSTZFXXZHTRQHYBDLYCZSSYMMRFMYQZPWWJJYFCRWFDFZQPYDDWYXKYJAWJFFXYPSFTZYHHYZYSWCJYXSCLCXXWZZXNBGNNXBXLZSZSBSGPYSYZDHMDZBQBZCWDZZYYTZHBTSYYBZGNTNXQYWQSKBPHHLXGYBFMJEBJHHGQTJCYSXSTKZHLYCKGLYSMZXYALMELDCCXGZYRJXSDLTYZCQKCNNJWHJTZZCQLJSTSTBNXBTYXCEQXGKWJYFLZQLYHYXSPSFXLMPBYSXXXYDJCZYLLLSJXFHJXPJBTFFYABYXBHZZBJYZLWLCZGGBTSSMDTJZXPTHYQTGLJSCQFZKJZJQNLZWLSLHDZBWJNCJZYZSQQYCQYRZCJJWYBRTWPYFTWEXCSKDZCTBZHYZZYYJXZCFFZZMJYXXSDZZOTTBZLQWFCKSZSXFYRLNYJMBDTHJXSQQCCSBXYYTSYFBXDZTGBCNSLCYZZPSAZYZZSCJCSHZQYDXLBPJLLMQXTYDZXSQJTZPXLCGLQTZWJBHCTSYJSFXYEJJTLBGXSXJMYJQQPFZASYJNTYDJXKJCDJSZCBARTDCLYJQMWNQNCLLLKBYBZZSYHQQLTWLCCXTXLLZNTYLNEWYZYXCZXXGRKRMTCNDNJTSYYSSDQDGHSDBJGHRWRQLYBGLXHLGTGXBQJDZPYJSJYJCTMRNYMGRZJCZGJMZMGXMPRYXKJNYMSGMZJYMKMFXMLDTGFBHCJHKYLPFMDXLQJJSMTQGZSJLQDLDGJYCALCMZCSDJLLNXDJFFFFJCZFMZFFPFKHKGDPSXKTACJDHHZDDCRRCFQYJKQCCWJDXHWJLYLLZGCFCQDSMLZPBJJPLSBCJGGDCKKDEZSQCCKJGCGKDJTJDLZYCXKLQSCGJCLTFPCQCZGWPJDQYZJJBYJHSJDZWGFSJGZKQCCZLLPSPKJGQJHZZLJPLGJGJJTHJJYJZCZMLZLYQBGJWMLJKXZDZNJQSYZMLJLLJKYWXMKJLHSKJGBMCLYYMKXJQLBMLLKMDXXKWYXYSLMLPSJQQJQXYXFJTJDXMXXLLCXQBSYJBGWYMBGGBCYXPJYGPEPFGDJGBHBNSQJYZJKJKHXQFGQZKFHYGKHDKLLSDJQXPQYKYBNQSXQNSZSWHBSXWHXWBZZXDMNSJBSBKBBZKLYLXGWXDRWYQZMYWSJQLCJXXJXKJEQXSCYETLZHLYYYSDZPAQYZCMTLSHTZCFYZYXYLJSDCJQAGYSLCQLYYYSHMRQQKLDXZSCSSSYDYCJYSFSJBFRSSZQSBXXPXJYSDRCKGJLGDKZJZBDKTCSYQPYHSTCLDJDHMXMCGXYZHJDDTMHLTXZXYLYMOHYJCLTYFBQQXPFBDFHHTKSQHZYYWCNXXCRWHOWGYJLEGWDQCWGFJYCSNTMYTOLBYGWQWESJPWNMLRYDZSZTXYQPZGCWXHNGPYXSHMYQJXZTDPPBFYHZHTJYFDZWKGKZBLDNTSXHQEEGZZYLZMMZYJZGXZXKHKSTXNXXWYLYAPSTHXDWHZYMPXAGKYDXBHNHXKDPJNMYHYLPMGOCSLNZHKXXLPZZLBMLSFBHHGYGYYGGBHSCYAQTYWLXTZQCEZYDQDQMMHTKLLSZHLSJZWFYHQSWSCWLQAZYNYTLSXTHAZNKZZSZZLAXXZWWCTGQQTDDYZTCCHYQZFLXPSLZYGPZSZNGLNDQTBDLXGTCTAJDKYWNSYZLJHHZZCWNYYZYWMHYCHHYXHJKZWSXHZYXLYSKQYSPSLYZWMYPPKBYGLKZHTYXAXQSYSHXASMCHKDSCRSWJPWXSGZJLWWSCHSJHSQNHCSEGNDAQTBAALZZMSSTDQJCJKTSCJAXPLGGXHHGXXZCXPDMMHLDGTYBYSJMXHMRCPXXJZCKZXSHMLQXXTTHXWZFKHCCZDYTCJYXQHLXDHYPJQXYLSYYDZOZJNYXQEZYSQYAYXWYPDGXDDXSPPYZNDLTWRHXYDXZZJHTCXMCZLHPYYYYMHZLLHNXMYLLLMDCPPXHMXDKYCYRDLTXJCHHZZXZLCCLYLNZSHZJZZLNNRLWHYQSNJHXYNTTTKYJPYCHHYEGKCTTWLGQRLGGTGTYGYHPYHYLQYQGCWYQKPYYYTTTTLHYHLLTYTTSPLKYZXGZWGPYDSSZZDQXSKCQNMJJZZBXYQMJRTFFBTKHZKBXLJJKDXJTLBWFZPPTKQTZTGPDGNTPJYFALQMKGXBDCLZFHZCLLLLADPMXDJHLCCLGYHDZFGYDDGCYYFGYDXKSSEBDHYKDKDKHNAXXYBPBYYHXZQGAFFQYJXDMLJCSQZLLPCHBSXGJYNDYBYQSPZWJLZKSDDTACTBXZDYZYPJZQSJNKKTKNJDJGYYPGTLFYQKASDNTCYHBLWDZHBBYDWJRYGKZYHEYYFJMSDTYFZJJHGCXPLXHLDWXXJKYTCYKSSSMTWCTTQZLPBSZDZWZXGZAGYKTYWXLHLSPBCLLOQMMZSSLCMBJCSZZKYDCZJGQQDSMCYTZQQLWZQZXSSFPTTFQMDDZDSHDTDWFHTDYZJYQJQKYPBDJYYXTLJHDRQXXXHAYDHRJLKLYTWHLLRLLRCXYLBWSRSZZSYMKZZHHKYHXKSMDSYDYCJPBZBSQLFCXXXNXKXWYWSDZYQOGGQMMYHCDZTTFJYYBGSTTTYBYKJDHKYXBELHTYPJQNFXFDYKZHQKZBYJTZBXHFDXKDASWTAWAJLDYJSFHBLDNNTNQJTJNCHXFJSRFWHZFMDRYJYJWZPDJKZYJYMPCYZNYNXFBYTFYFWYGDBNZZZDNYTXZEMMQBSQEHXFZMBMFLZZSRXYMJGSXWZJSPRYDJSJGXHJJGLJJYNZZJXHGXKYMLPYYYCXYTWQZSWHWLYRJLPXSLSXMFSWWKLCTNXNYNPSJSZHDZEPTXMYYWXYYSYWLXJQZQXZDCLEEELMCPJPCLWBXSQHFWWTFFJTNQJHJQDXHWLBYZNFJLALKYYJLDXHHYCSTYYWNRJYXYWTRMDRQHWQCMFJDYZMHMYYXJWMYZQZXTLMRSPWWCHAQBXYGZYPXYYRRCLMPYMGKSJSZYSRMYJSNXTPLNBAPPYPYLXYYZKYNLDZYJZCZNNLMZHHARQMPGWQTZMXXMLLHGDZXYHXKYXYCJMFFYYHJFSBSSQLXXNDYCANNMTCJCYPRRNYTYQNYYMBMSXNDLYLYSLJRLXYSXQMLLYZLZJJJKYZZCSFBZXXMSTBJGNXYZHLXNMCWSCYZYFZLXBRNNNYLBNRTGZQYSATSWRYHYJZMZDHZGZDWYBSSCSKXSYHYTXXGCQGXZZSHYXJSCRHMKKBXCZJYJYMKQHZJFNBHMQHYSNJNZYBKNQMCLGQHWLZNZSWXKHLJHYYBQLBFCDSXDLDSPFZPSKJYZWZXZDDXJSMMEGJSCSSMGCLXXKYYYLNYPWWWGYDKZJGGGZGGSYCKNJWNJPCXBJJTQTJWDSSPJXZXNZXUMELPXFSXTLLXCLJXJJLJZXCTPSWXLYDHLYQRWHSYCSQYYBYAYWJJJQFWQCQQCJQGXALDBZZYJGKGXPLTZYFXJLTPADKYQHPMATLCPDCKBMTXYBHKLENXDLEEGQDYMSAWHZMLJTWYGXLYQZLJEEYYBQQFFNLYXRDSCTGJGXYYNKLLYQKCCTLHJLQMKKZGCYYGLLLJDZGYDHZWXPYSJBZKDZGYZZHYWYFQYTYZSZYEZZLYMHJJHTSMQWYZLKYYWZCSRKQYTLTDXWCTYJKLWSQZWBDCQYNCJSRSZJLKCDCDTLZZZACQQZZDDXYPLXZBQJYLZLLLQDDZQJYJYJZYXNYYYNYJXKXDAZWYRDLJYYYRJLXLLDYXJCYWYWNQCCLDDNYYYNYCKCZHXXCCLGZQJGKWPPCQQJYSBZZXYJSQPXJPZBSBDSFNSFPZXHDWZTDWPPTFLZZBZDMYYPQJRSDZSQZSQXBDGCPZSWDWCSQZGMDHZXMWWFYBPDGPHTMJTHZSMMBGZMBZJCFZWFZBBZMQCFMBDMCJXLGPNJBBXGYHYYJGPTZGZMQBQTCGYXJXLWZKYDPDYMGCFTPFXYZTZXDZXTGKMTYBBCLBJASKYTSSQYYMSZXFJEWLXLLSZBQJJJAKLYLXLYCCTSXMCWFKKKBSXLLLLJYXTYLTJYYTDPJHNHNNKBYQNFQYYZBYYESSESSGDYHFHWTCJBSDZZTFDMXHCNJZYMQWSRYJDZJQPDQBBSTJGGFBKJBXTGQHNGWJXJGDLLTHZHHYYYYYYSXWTYYYCCBDBPYPZYCCZYJPZYWCBDLFWZCWJDXXHYHLHWZZXJTCZLCDPXUJCZZZLYXJJTXPHFXWPYWXZPTDZZBDZCYHJHMLXBQXSBYLRDTGJRRCTTTHYTCZWMXFYTWWZCWJWXJYWCSKYBZSCCTZQNHXNWXXKHKFHTSWOCCJYBCMPZZYKBNNZPBZHHZDLSYDDYTYFJPXYNGFXBYQXCBHXCPSXTYZDMKYSNXSXLHKMZXLYHDHKWHXXSSKQYHHCJYXGLHZXCSNHEKDTGZXQYPKDHEXTYKCNYMYYYPKQYYYKXZLTHJQTBYQHXBMYHSQCKWWYLLHCYYLNNEQXQWMCFBDCCMLJGGXDQKTLXKGNQCDGZJWYJJLYHHQTTTNWCHMXCXWHWSZJYDJCCDBQCDGDNYXZTHCQRXCBHZTQCBXWGQWYYBXHMBYMYQTYEXMQKYAQYRGYZSLFYKKQHYSSQYSHJGJCNXKZYCXSBXYXHYYLSTYCXQTHYSMGSCPMMGCCCCCMTZTASMGQZJHKLOSQYLSWTMXSYQKDZLJQQYPLSYCZTCQQPBBQJZCLPKHQZYYXXDTDDTSJCXFFLLCHQXMJLWCJCXTSPYCXNDTJSHJWXDQQJSKXYAMYLSJHMLALYKXCYYDMNMDQMXMCZNNCYBZKKYFLMCHCMLHXRCJJHSYLNMTJZGZGYWJXSRXCWJGJQHQZDQJDCJJZKJKGDZQGJJYJYLXZXXCDQHHHEYTMHLFSBDJSYYSHFYSTCZQLPBDRFRZTZYKYWHSZYQKWDQZRKMSYNBCRXQBJYFAZPZZEDZCJYWBCJWHYJBQSZYWRYSZPTDKZPFPBNZTKLQYHBBZPNPPTYZZYBQNYDCPJMMCYCQMCYFZZDCMNLFPBPLNGQJTBTTNJZPZBBZNJKLJQYLNBZQHKSJZNGGQSZZKYXSHPZSNBCGZKDDZQANZHJKDRTLZLSWJLJZLYWTJNDJZJHXYAYNCBGTZCSSQMNJPJYTYSWXZFKWJQTKHTZPLBHSNJZSYZBWZZZZLSYLSBJHDWWQPSLMMFBJDWAQYZTCJTBNNWZXQXCDSLQGDSDPDZHJTQQPSWLYYJZLGYXYZLCTCBJTKTYCZJTQKBSJLGMGZDMCSGPYNJZYQYYKNXRPWSZXMTNCSZZYXYBYHYZAXYWQCJTLLCKJJTJHGDXDXYQYZZBYWDLWQCGLZGJGQRQZCZSSBCRPCSKYDZNXJSQGXSSJMYDNSTZTPBDLTKZWXQWQTZEXNQCZGWEZKSSBYBRTSSSLCCGBPSZQSZLCCGLLLZXHZQTHCZMQGYZQZNMCOCSZJMMZSQPJYGQLJYJPPLDXRGZYXCCSXHSHGTZNLZWZKJCXTCFCJXLBMQBCZZWPQDNHXLJCTHYZLGYLNLSZZPCXDSCQQHJQKSXZPBAJYEMSMJTZDXLCJYRYYNWJBNGZZTMJXLTBSLYRZPYLSSCNXPHLLHYLLQQZQLXYMRSYCXZLMMCZLTZSDWTJJLLNZGGQXPFSKYGYGHBFZPDKMWGHCXMSGDXJMCJZDYCABXJDLNBCDQYGSKYDQTXDJJYXMSZQAZDZFSLQXYJSJZYLBTXXWXQQZBJZUFBBLYLWDSLJHXJYZJWTDJCZFQZQZZDZSXZZQLZCDZFJHYSPYMPQZMLPPLFFXJJNZZYLSJEYQZFPFZKSYWJJJHRDJZZXTXXGLGHYDXCSKYSWMMZCWYBAZBJKSHFHJCXMHFQHYXXYZFTSJYZFXYXPZLCHMZMBXHZZSXYFYMNCWDABAZLXKTCSHHXKXJJZJSTHYGXSXYYHHHJWXKZXSSBZZWHHHCWTZZZPJXSNXQQJGZYZYWLLCWXZFXXYXYHXMKYYSWSQMNLNAYCYSPMJKHWCQHYLAJJMZXHMMCNZHBHXCLXTJPLTXYJHDYYLTTXFSZHYXXSJBJYAYRSMXYPLCKDUYHLXRLNLLSTYZYYQYGYHHSCCSMZCTZQXKYQFPYYRPFFLKQUNTSZLLZMWWTCQQYZWTLLMLMPWMBZSSTZRBPDDTLQJJBXZCSRZQQYGWCSXFWZLXCCRSZDZMCYGGDZQSGTJSWLJMYMMZYHFBJDGYXCCPSHXNZCSBSJYJGJMPPWAFFYFNXHYZXZYLREMZGZCYZSSZDLLJCSQFNXZKPTXZGXJJGFMYYYSNBTYLBNLHPFZDCYFBMGQRRSSSZXYSGTZRNYDZZCDGPJAFJFZKNZBLCZSZPSGCYCJSZLMLRSZBZZLDLSLLYSXSQZQLYXZLSKKBRXBRBZCYCXZZZEEYFGKLZLYYHGZSGZLFJHGTGWKRAAJYZKZQTSSHJJXDCYZUYJLZYRZDQQHGJZXSSZBYKJPBFRTJXLLFQWJHYLQTYMBLPZDXTZYGBDHZZRBGXHWNJTJXLKSCFSMWLSDQYSJTXKZSCFWJLBXFTZLLJZLLQBLSQMQQCGCZFPBPHZCZJLPYYGGDTGWDCFCZQYYYQYSSCLXZSKLZZZGFFCQNWGLHQYZJJCZLQZZYJPJZZBPDCCMHJGXDQDGDLZQMFGPSYTSDYFWWDJZJYSXYYCZCYHZWPBYKXRYLYBHKJKSFXTZJMMCKHLLTNYYMSYXYZPYJQYCSYCWMTJJKQYRHLLQXPSGTLYYCLJSCPXJYZFNMLRGJJTYZBXYZMSJYJHHFZQMSYXRSZCWTLRTQZSSTKXGQKGSPTGCZNJSJCQCXHMXGGZTQYDJKZDLBZSXJLHYQGGGTHQSZPYHJHHGYYGKGGCWJZZYLCZLXQSFTGZSLLLMLJSKCTBLLZZSZMMNYTPZSXQHJCJYQXYZXZQZCPSHKZZYSXCDFGMWQRLLQXRFZTLYSTCTMJCXJJXHJNXTNRZTZFQYHQGLLGCXSZSJDJLJCYDSJTLNYXHSZXCGJZYQPYLFHDJSBPCCZHJJJQZJQDYBSSLLCMYTTMQTBHJQNNYGKYRQYQMZGCJKPDCGMYZHQLLSLLCLMHOLZGDYYFZSLJCQZLYLZQJESHNYLLJXGJXLYSYYYXNBZLJSSZCQQCJYLLZLTJYLLZLLBNYLGQCHXYYXOXCXQKYJXXXYKLXSXXYQXCYKQXQCSGYXXYQXYGYTQOHXHXPYXXXULCYEYCHZZCBWQBBWJQZSCSZSSLZYLKDESJZWMYMCYTSDSXXSCJPQQSQYLYYZYCMDJDZYWCBTJSYDJKCYDDJLBDJJSODZYSYXQQYXDHHGQQYQHDYXWGMMMAJDYBBBPPBCMUUPLJZSMTXERXJMHQNUTPJDCBSSMSSSTKJTSSMMTRCPLZSZMLQDSDMJMQPNQDXCFYNBFSDQXYXHYAYKQYDDLQYYYSSZBYDSLNTFQTZQPZMCHDHCZCWFDXTMYQSPHQYYXSRGJCWTJTZZQMGWJJTJHTQJBBHWZPXXHYQFXXQYWYYHYSCDYDHHQMNMTMWCPBSZPPZZGLMZFOLLCFWHMMSJZTTDHZZYFFYTZZGZYSKYJXQYJZQBHMBZZLYGHGFMSHPZFZSNCLPBQSNJXZSLXXFPMTYJYGBXLLDLXPZJYZJYHHZCYWHJYLSJEXFSZZYWXKZJLUYDTMLYMQJPWXYHXSKTQJEZRPXXZHHMHWQPWQLYJJQJJZSZCPHJLCHHNXJLQWZJHBMZYXBDHHYPZLHLHLGFWLCHYYTLHJXCJMSCPXSTKPNHQXSRTYXXTESYJCTLSSLSTDLLLWWYHDHRJZSFGXTSYCZYNYHTDHWJSLHTZDQDJZXXQHGYLTZPHCSQFCLNJTCLZPFSTPDYNYLGMJLLYCQHYSSHCHYLHQYQTMZYPBYWRFQYKQSYSLZDQJMPXYYSSRHZJNYWTQDFZBWWTWWRXCWHGYHXMKMYYYQMSMZHNGCEPMLQQMTCWCTMMPXJPJJHFXYYZSXZHTYBMSTSYJTTQQQYYLHYNPYQZLCYZHZWSMYLKFJXLWGXYPJYTYSYXYMZCKTTWLKSMZSYLMPWLZWXWQZSSAQSYXYRHSSNTSRAPXCPWCMGDXHXZDZYFJHGZTTSBJHGYZSZYSMYCLLLXBTYXHBBZJKSSDMALXHYCFYGMQYPJYCQXJLLLJGSLZGQLYCJCCZOTYXMTMTTLLWTGPXYMZMKLPSZZZXHKQYSXCTYJZYHXSHYXZKXLZWPSQPYHJWPJPWXQQYLXSDHMRSLZZYZWTTCYXYSZZSHBSCCSTPLWSSCJCHNLCGCHSSPHYLHFHHXJSXYLLNYLSZDHZXYLSXLWZYKCLDYAXZCMDDYSPJTQJZLNWQPSSSWCTSTSZLBLNXSMNYYMJQBQHRZWTYYDCHQLXKPZWBGQYBKFCMZWPZLLYYLSZYDWHXPSBCMLJBSCGBHXLQHYRLJXYSWXWXZSLDFHLSLYNJLZYFLYJYCDRJLFSYZFSLLCQYQFGJYHYXZLYLMSTDJCYHBZLLNWLXXYGYYHSMGDHXXHHLZZJZXCZZZCYQZFNGWPYLCPKPYYPMCLQKDGXZGGWQBDXZZKZFBXXLZXJTPJPTTBYTSZZDWSLCHZHSLTYXHQLHYXXXYYZYSWTXZKHLXZXZPYHGCHKCFSYHUTJRLXFJXPTZTWHPLYXFCRHXSHXKYXXYHZQDXQWULHYHMJTBFLKHTXCWHJFWJCFPQRYQXCYYYQYGRPYWSGSUNGWCHKZDXYFLXXHJJBYZWTSXXNCYJJYMSWZJQRMHXZWFQSYLZJZGBHYNSLBGTTCSYBYXXWXYHXYYXNSQYXMQYWRGYQLXBBZLJSYLPSYTJZYHYZAWLRORJMKSCZJXXXYXCHDYXRYXXJDTSQFXLYLTSFFYXLMTYJMJUYYYXLTZCSXQZQHZXLYYXZHDNBRXXXJCTYHLBRLMBRLLAXKYLLLJLYXXLYCRYLCJTGJCMTLZLLCYZZPZPCYAWHJJFYBDYYZSMPCKZDQYQPBPCJPDCYZMDPBCYYDYCNNPLMTMLRMFMMGWYZBSJGYGSMZQQQZTXMKQWGXLLPJGZBQCDJJJFPKJKCXBLJMSWMDTQJXLDLPPBXCWRCQFBFQJCZAHZGMYKPHYYHZYKNDKZMBPJYXPXYHLFPNYYGXJDBKXNXHJMZJXSTRSTLDXSKZYSYBZXJLXYSLBZYSLHXJPFXPQNBYLLJQKYGZMCYZZYMCCSLCLHZFWFWYXZMWSXTYNXJHPYYMCYSPMHYSMYDYSHQYZCHMJJMZCAAGCFJBBHPLYZYLXXSDJGXDHKXXTXXNBHRMLYJSLTXMRHNLXQJXYZLLYSWQGDLBJHDCGJYQYCMHWFMJYBMBYJYJWYMDPWHXQLDYGPDFXXBCGJSPCKRSSYZJMSLBZZJFLJJJLGXZGYXYXLSZQYXBEXYXHGCXBPLDYHWETTWWCJMBTXCHXYQXLLXFLYXLLJLSSFWDPZSMYJCLMWYTCZPCHQEKCQBWLCQYDPLQPPQZQFJQDJHYMMCXTXDRMJWRHXCJZYLQXDYYNHYYHRSLSRSYWWZJYMTLTLLGTQCJZYABTCKZCJYCCQLJZQXALMZYHYWLWDXZXQDLLQSHGPJFJLJHJABCQZDJGTKHSSTCYJLPSWZLXZXRWGLDLZRLZXTGSLLLLZLYXXWGDZYGBDPHZPBRLWSXQBPFDWOFMWHLYPCBJCCLDMBZPBZZLCYQXLDOMZBLZWPDWYYGDSTTHCSQSCCRSSSYSLFYBFNTYJSZDFNDPDHDZZMBBLSLCMYFFGTJJQWFTMTPJWFNLBZCMMJTGBDZLQLPYFHYYMJYLSDCHDZJWJCCTLJCLDTLJJCPDDSQDSSZYBNDBJLGGJZXSXNLYCYBJXQYCBYLZCFZPPGKCXZDZFZTJJFJSJXZBNZYJQTTYJYHTYCZHYMDJXTTMPXSPLZCDWSLSHXYPZGTFMLCJTYCBPMGDKWYCYZCDSZZYHFLYCTYGWHKJYYLSJCXGYWJCBLLCSNDDBTZBSCLYZCZZSSQDLLMQYYHFSLQLLXFTYHABXGWNYWYYPLLSDLDLLBJCYXJZMLHLJDXYYQYTDLLLBUGBFDFBBQJZZMDPJHGCLGMJJPGAEHHBWCQXAXHHHZCHXYPHJAXHLPHJPGPZJQCQZGJJZZUZDMQYYBZZPHYHYBWHAZYJHYKFGDPFQSDLZMLJXKXGALXZDAGLMDGXMWZQYXXDXXPFDMMSSYMPFMDMMKXKSYZYSHDZKXSYSMMZZZMSYDNZZCZXFPLSTMZDNMXCKJMZTYYMZMZZMSXHHDCZJEMXXKLJSTLWLSQLYJZLLZJSSDPPMHNLZJCZYHMXXHGZCJMDHXTKGRMXFWMCGMWKDTKSXQMMMFZZYDKMSCLCMPCGMHSPXQPZDSSLCXKYXTWLWJYAHZJGZQMCSNXYYMMPMLKJXMHLMLQMXCTKZMJQYSZJSYSZHSYJZJCDAJZYBSDQJZGWZQQXFKDMSDJLFWEHKZQKJPEYPZYSZCDWYJFFMZZYLTTDZZEFMZLBNPPLPLPEPSZALLTYLKCKQZKGENQLWAGYXYDPXLHSXQQWQCQXQCLHYXXMLYCCWLYMQYSKGCHLCJNSZKPYZKCQZQLJPDMDZHLASXLBYDWQLWDNBQCRYDDZTJYBKBWSZDXDTNPJDTCTQDFXQQMGNXECLTTBKPWSLCTYQLPWYZZKLPYGZCQQPLLKCCYLPQMZCZQCLJSLQZDJXLDDHPZQDLJJXZQDXYZQKZLJCYQDYJPPYPQYKJYRMPCBYMCXKLLZLLFQPYLLLMBSGLCYSSLRSYSQTMXYXZQZFDZUYSYZTFFMZZSMZQHZSSCCMLYXWTPZGXZJGZGSJSGKDDHTQGGZLLBJDZLCBCHYXYZHZFYWXYZYMSDBZZYJGTSMTFXQYXQSTDGSLNXDLRYZZLRYYLXQHTXSRTZNGZXBNQQZFMYKMZJBZYMKBPNLYZPBLMCNQYZZZSJZHJCTZKHYZZJRDYZHNPXGLFZTLKGJTCTSSYLLGZRZBBQZZKLPKLCZYSSUYXBJFPNJZZXCDWXZYJXZZDJJKGGRSRJKMSMZJLSJYWQSKYHQJSXPJZZZLSNSHRNYPZTWCHKLPSRZLZXYJQXQKYSJYCZTLQZYBBYBWZPQDWWYZCYTJCJXCKCWDKKZXSGKDZXWWYYJQYYTCYTDLLXWKCZKKLCCLZCQQDZLQLCSFQCHQHSFSMQZZLNBJJZBSJHTSZDYSJQJPDLZCDCWJKJZZLPYCGMZWDJJBSJQZSYZYHHXJPBJYDSSXDZNCGLQMBTSFSBPDZDLZNFGFJGFSMPXJQLMBLGQCYYXBQKDJJQYRFKZTJDHCZKLBSDZCFJTPLLJGXHYXZCSSZZXSTJYGKGCKGYOQXJPLZPBPGTGYJZGHZQZZLBJLSQFZGKQQJZGYCZBZQTLDXRJXBSXXPZXHYZYCLWDXJJHXMFDZPFZHQHQMQGKSLYHTYCGFRZGNQXCLPDLBZCSCZQLLJBLHBZCYPZZPPDYMZZSGYHCKCPZJGSLJLNSCDSLDLXBMSTLDDFJMKDJDHZLZXLSZQPQPGJLLYBDSZGQLBZLSLKYYHZTTNTJYQTZZPSZQZTLLJTYYLLQLLQYZQLBDZLSLYYZYMDFSZSNHLXZNCZQZPBWSKRFBSYZMTHBLGJPMCZZLSTLXSHTCSYZLZBLFEQHLXFLCJLYLJQCBZLZJHHSSTBRMHXZHJZCLXFNBGXGTQJCZTMSFZKJMSSNXLJKBHSJXNTNLZDNTLMSJXGZJYJCZXYJYJWRWWQNZTNFJSZPZSHZJFYRDJSFSZJZBJFZQZZHZLXFYSBZQLZSGYFTZDCSZXZJBQMSZKJRHYJZCKMJKHCHGTXKXQGLXPXFXTRTYLXJXHDTSJXHJZJXZWZLCQSBTXWXGXTXXHXFTSDKFJHZYJFJXRZSDLLLTQSQQZQWZXSYQTWGWBZCGZLLYZBCLMQQTZHZXZXLJFRMYZFLXYSQXXJKXRMQDZDMMYYBSQBHGZMWFWXGMXLZPYYTGZYCCDXYZXYWGSYJYZNBHPZJSQSYXSXRTFYZGRHZTXSZZTHCBFCLSYXZLZQMZLMPLMXZJXSFLBYZMYQHXJSXRXSQZZZSSLYFRCZJRCRXHHZXQYDYHXSJJHZCXZBTYNSYSXJBQLPXZQPYMLXZKYXLXCJLCYSXXZZLXDLLLJJYHZXGYJWKJRWYHCPSGNRZLFZWFZZNSXGXFLZSXZZZBFCSYJDBRJKRDHHGXJLJJTGXJXXSTJTJXLYXQFCSGSWMSBCTLQZZWLZZKXJMLTMJYHSDDBXGZHDLBMYJFRZFSGCLYJBPMLYSMSXLSZJQQHJZFXGFQFQBPXZGYYQXGZTCQWYLTLGWSGWHRLFSFGZJMGMGBGTJFSYZZGZYZAFLSSPMLPFLCWBJZCLJJMZLPJJLYMQDMYYYFBGYGYZMLYZDXQYXRQQQHSYYYQXYLJTYXFSFSLLGNQCYHYCWFHCCCFXPYLYPLLZYXXXXXKQHHXSHJZCFZSCZJXCPZWHHHHHAPYLQALPQAFYHXDYLUKMZQGGGDDESRNNZLTZGCHYPPYSQJJHCLLJTOLNJPZLJLHYMHEYDYDSQYCDDHGZUNDZCLZYZLLZNTNYZGSLHSLPJJBDGWXPCDUTJCKLKCLWKLLCASSTKZZDNQNTTLYYZSSYSSZZRYLJQKCQDHHCRXRZYDGRGCWCGZQFFFPPJFZYNAKRGYWYQPQXXFKJTSZZXSWZDDFBBXTBGTZKZNPZZPZXZPJSZBMQHKCYXYLDKLJNYPKYGHGDZJXXEAHPNZKZTZCMXCXMMJXNKSZQNMNLWBWWXJKYHCPSTMCSQTZJYXTPCTPDTNNPGLLLZSJLSPBLPLQHDTNJNLYYRSZFFJFQWDPHZDWMRZCCLODAXNSSNYZRESTYJWJYJDBCFXNMWTTBYLWSTSZGYBLJPXGLBOCLHPCBJLTMXZLJYLZXCLTPNCLCKXTPZJSWCYXSFYSZDKNTLBYJCYJLLSTGQCBXRYZXBXKLYLHZLQZLNZCXWJZLJZJNCJHXMNZZGJZZXTZJXYCYYCXXJYYXJJXSSSJSTSSTTPPGQTCSXWZDCSYFPTFBFHFBBLZJCLZZDBXGCXLQPXKFZFLSYLTUWBMQJHSZBMDDBCYSCCLDXYCDDQLYJJWMQLLCSGLJJSYFPYYCCYLTJANTJJPWYCMMGQYYSXDXQMZHSZXPFTWWZQSWQRFKJLZJQQYFBRXJHHFWJJZYQAZMYFRHCYYBYQWLPEXCCZSTYRLTTDMQLYKMBBGMYYJPRKZNPBSXYXBHYZDJDNGHPMFSGMWFZMFQMMBCMZZCJJLCNUXYQLMLRYGQZCYXZLWJGCJCGGMCJNFYZZJHYCPRRCMTZQZXHFQGTJXCCJEAQCRJYHPLQLSZDJRBCQHQDYRHYLYXJSYMHZYDWLDFRYHBPYDTSSCNWBXGLPZMLZZTQSSCPJMXXYCSJYTYCGHYCJWYRXXLFEMWJNMKLLSWTXHYYYNCMMCWJDQDJZGLLJWJRKHPZGGFLCCSCZMCBLTBHBQJXQDSPDJZZGKGLFQYWBZYZJLTSTDHQHCTCBCHFLQMPWDSHYYTQWCNZZJTLBYMBPDYYYXSQKXWYYFLXXNCWCXYPMAELYKKJMZZZBRXYYQJFLJPFHHHYTZZXSGQQMHSPGDZQWBWPJHZJDYSCQWZKTXXSQLZYYMYSDZGRXCKKUJLWPYSYSCSYZLRMLQSYLJXBCXTLWDQZPCYCYKPPPNSXFYZJJRCEMHSZMSXLXGLRWGCSTLRSXBZGBZGZTCPLUJLSLYLYMTXMTZPALZXPXJTJWTCYYZLBLXBZLQMYLXPGHDSLSSDMXMBDZZSXWHAMLCZCPJMCNHJYSNSYGCHSKQMZZQDLLKABLWJXSFMOCDXJRRLYQZKJMYBYQLYHETFJZFRFKSRYXFJTWDSXXSYSQJYSLYXWJHSNLXYYXHBHAWHHJZXWMYLJCSSLKYDZTXBZSYFDXGXZJKHSXXYBSSXDPYNZWRPTQZCZENYGCXQFJYKJBZMLJCMQQXUOXSLYXXLYLLJDZBTYMHPFSTTQQWLHOKYBLZZALZXQLHZWRRQHLSTMYPYXJJXMQSJFNBXYXYJXXYQYLTHYLQYFMLKLJTMLLHSZWKZHLJMLHLJKLJSTLQXYLMBHHLNLZXQJHXCFXXLHYHJJGBYZZKBXSCQDJQDSUJZYYHZHHMGSXCSYMXFEBCQWWRBPYYJQTYZCYQYQQZYHMWFFHGZFRJFCDPXNTQYZPDYKHJLFRZXPPXZDBBGZQSTLGDGYLCQMLCHHMFYWLZYXKJLYPQHSYWMQQGQZMLZJNSQXJQSYJYCBEHSXFSZPXZWFLLBCYYJDYTDTHWZSFJMQQYJLMQXXLLDTTKHHYBFPWTYYSQQWNQWLGWDEBZWCMYGCULKJXTMXMYJSXHYBRWFYMWFRXYQMXYSZTZZTFYKMLDHQDXWYYNLCRYJBLPSXCXYWLSPRRJWXHQYPHTYDNXHHMMYWYTZCSQMTSSCCDALWZTCPQPYJLLQZYJSWXMZZMMYLMXCLMXCZMXMZSQTZPPQQBLPGXQZHFLJJHYTJSRXWZXSCCDLXTYJDCQJXSLQYCLZXLZZXMXQRJMHRHZJBHMFLJLMLCLQNLDXZLLLPYPSYJYSXCQQDCMQJZZXHNPNXZMEKMXHYKYQLXSXTXJYYHWDCWDZHQYYBGYBCYSCFGPSJNZDYZZJZXRZRQJJYMCANYRJTLDPPYZBSTJKXXZYPFDWFGZZRPYMTNGXZQBYXNBUFNQKRJQZMJEGRZGYCLKXZDSKKNSXKCLJSPJYYZLQQJYBZSSQLLLKJXTBKTYLCCDDBLSPPFYLGYDTZJYQGGKQTTFZXBDKTYYHYBBFYTYYBCLPDYTGDHRYRNJSPTCSNYJQHKLLLZSLYDXXWBCJQSPXBPJZJCJDZFFXXBRMLAZHCSNDLBJDSZBLPRZTSWSBXBCLLXXLZDJZSJPYLYXXYFTFFFBHJJXGBYXJPMMMPSSJZJMTLYZJXSWXTYLEDQPJMYGQZJGDJLQJWJQLLSJGJGYGMSCLJJXDTYGJQJQJCJZCJGDZZSXQGSJGGCXHQXSNQLZZBXHSGZXCXYLJXYXYYDFQQJHJFXDHCTXJYRXYSQTJXYEFYYSSYYJXNCYZXFXMSYSZXYYSCHSHXZZZGZZZGFJDLTYLNPZGYJYZYYQZPBXQBDZTZCZYXXYHHSQXSHDHGQHJHGYWSZTMZMLHYXGEBTYLZKQWYTJZRCLEKYSTDBCYKQQSAYXCJXWWGSBHJYZYDHCSJKQCXSWXFLTYNYZPZCCZJQTZWJQDZZZQZLJJXLSBHPYXXPSXSHHEZTXFPTLQYZZXHYTXNCFZYYHXGNXMYWXTZSJPTHHGYMXMXQZXTSBCZYJYXXTYYZYPCQLMMSZMJZZLLZXGXZAAJZYXJMZXWDXZSXZDZXLEYJJZQBHZWZZZQTZPSXZTDSXJJJZNYAZPHXYYSRNQDTHZHYYKYJHDZXZLSWCLYBZYECWCYCRYLCXNHZYDZYDYJDFRJJHTRSQTXYXJRJHOJYNXELXSFSFJZGHPZSXZSZDZCQZBYYKLSGSJHCZSHDGQGXYZGXCHXZJWYQWGYHKSSEQZZNDZFKWYSSTCLZSTSYMCDHJXXYWEYXCZAYDMPXMDSXYBSQMJMZJMTZQLPJYQZCGQHXJHHLXXHLHDLDJQCLDWBSXFZZYYSCHTYTYYBHECXHYKGJPXHHYZJFXHWHBDZFYZBCAPNPGNYDMSXHMMMMAMYNBYJTMPXYYMCTHJBZYFCGTYHWPHFTWZZEZSBZEGPFMTSKFTYCMHFLLHGPZJXZJGZJYXZSBBQSCZZLZCCSTPGXMJSFTCCZJZDJXCYBZLFCJSYZFGSZLYBCWZZBYZDZYPSWYJZXZBDSYUXLZZBZFYGCZXBZHZFTPBGZGEJBSTGKDMFHYZZJHZLLZZGJQZLSFDJSSCBZGPDLFZFZSZYZYZSYGCXSNXXCHCZXTZZLJFZGQSQYXZJQDCCZTQCDXZJYQJQCHXZTDLGSCXZSYQJQTZWLQDQZTQCHQQJZYEZZZPBWKDJFCJPZTYPQYQTTYNLMBDKTJZPQZQZZFPZSBNJLGYJDXJDZZKZGQKXDLPZJTCJDQBXDJQJSTCKNXBXZMSLYJCQMTJQWWCJQNJNLLLHJCWQTBZQYDZCZPZZDZYDDCYZZZCCJTTJFZDPRRTZTJDCQTQZDTJNPLZBCLLCTZSXKJZQZPZLBZRBTJDCXFCZDBCCJJLTQQPLDCGZDBBZJCQDCJWYNLLZYZCCDWLLXWZLXRXNTQQCZXKQLSGDFQTDDGLRLAJJTKUYMKQLLTZYTDYYCZGJWYXDXFRSKSTQTENQMRKQZHHQKDLDAZFKYPBGGPZREBZZYKZZSPEGJXGYKQZZZSLYSYYYZWFQZYLZZLZHWCHKYPQGNPGBLPLRRJYXCCSYYHSFZFYBZYYTGZXYLXCZWXXZJZBLFFLGSKHYJZEYJHLPLLLLCZGXDRZELRHGKLZZYHZLYQSZZJZQLJZFLNBHGWLCZCFJYSPYXZLZLXGCCPZBLLCYBBBBUBBCBPCRNNZCZYRBFSRLDCGQYYQXYGMQZWTZYTYJXYFWTEHZZJYWLCCNTZYJJZDEDPZDZTSYQJHDYMBJNYJZLXTSSTPHNDJXXBYXQTZQDDTJTDYYTGWSCSZQFLSHLGLBCZPHDLYZJYCKWTYTYLBNYTSDSYCCTYSZYYEBHEXHQDTWNYGYCLXTSZYSTQMYGZAZCCSZZDSLZCLZRQXYYELJSBYMXSXZTEMBBLLYYLLYTDQYSHYMRQWKFKBFXNXSBYCHXBWJYHTQBPBSBWDZYLKGZSKYHXQZJXHXJXGNLJKZLYYCDXLFYFGHLJGJYBXQLYBXQPQGZTZPLNCYPXDJYQYDYMRBESJYYHKXXSTMXRCZZYWXYQYBMCLLYZHQYZWQXDBXBZWZMSLPDMYSKFMZKLZCYQYCZLQXFZZYDQZPZYGYJYZMZXDZFYFYTTQTZHGSPCZMLCCYTZXJCYTJMKSLPZHYSNZLLYTPZCTZZCKTXDHXXTQCYFKSMQCCYYAZHTJPCYLZLYJBJXTPNYLJYYNRXSYLMMNXJSMYBCSYSYLZYLXJJQYLDZLPQBFZZBLFNDXQKCZFYWHGQMRDSXYCYTXNQQJZYYPFZXDYZFPRXEJDGYQBXRCNFYYQPGHYJDYZXGRHTKYLNWDZNTSMPKLBTHBPYSZBZTJZSZZJTYYXZPHSSZZBZCZPTQFZMYFLYPYBBJQXZMXXDJMTSYSKKBJZXHJCKLPSMKYJZCXTMLJYXRZZQSLXXQPYZXMKYXXXJCLJPRMYYGADYSKQLSNDHYZKQXZYZTCGHZTLMLWZYBWSYCTBHJHJFCWZTXWYTKZLXQSHLYJZJXTMPLPYCGLTBZZTLZJCYJGDTCLKLPLLQPJMZPAPXYZLKKTKDZCZZBNZDYDYQZJYJGMCTXLTGXSZLMLHBGLKFWNWZHDXUHLFMKYSLGXDTWWFRJEJZTZHYDXYKSHWFZCQSHKTMQQHTZHYMJDJSKHXZJZBZZXYMPAGQMSTPXLSKLZYNWRTSQLSZBPSPSGZWYHTLKSSSWHZZLYYTNXJGMJSZSUFWNLSOZTXGXLSAMMLBWLDSZYLAKQCQCTMYCFJBSLXCLZZCLXXKSBZQCLHJPSQPLSXXCKSLNHPSFQQYTXYJZLQLDXZQJZDYYDJNZPTUZDSKJFSLJHYLZSQZLBTXYDGTQFDBYAZXDZHZJNHHQBYKNXJJQCZMLLJZKSPLDYCLBBLXKLELXJLBQYCXJXGCNLCQPLZLZYJTZLJGYZDZPLTQCSXFDMNYCXGBTJDCZNBGBQYQJWGKFHTNPYQZQGBKPBBYZMTJDYTBLSQMPSXTBNPDXKLEMYYCJYNZCTLDYKZZXDDXHQSHDGMZSJYCCTAYRZLPYLTLKXSLZCGGEXCLFXLKJRTLQJAQZNCMBYDKKCXGLCZJZXJHPTDJJMZQYKQSECQZDSHHADMLZFMMZBGNTJNNLGBYJBRBTMLBYJDZXLCJLPLDLPCQDHLXZLYCBLCXZZJADJLNZMMSSSMYBHBSQKBHRSXXJMXSDZNZPXLGBRHWGGFCXGMSKLLTSJYYCQLTSKYWYYHYWXBXQYWPYWYKQLSQPTNTKHQCWDQKTWPXXHCPTHTWUMSSYHBWCRWXHJMKMZNGWTMLKFGHKJYLSYYCXWHYECLQHKQHTTQKHFZLDXQWYZYYDESBPKYRZPJFYYZJCEQDZZDLATZBBFJLLCXDLMJSSXEGYGSJQXCWBXSSZPDYZCXDNYXPPZYDLYJCZPLTXLSXYZYRXCYYYDYLWWNZSAHJSYQYHGYWWAXTJZDAXYSRLTDPSSYYFNEJDXYZHLXLLLZQZSJNYQYQQXYJGHZGZCYJCHZLYCDSHWSHJZYJXCLLNXZJJYYXNFXMWFPYLCYLLABWDDHWDXJMCXZTZPMLQZHSFHZYNZTLLDYWLSLXHYMMYLMBWWKYXYADTXYLLDJPYBPWUXJMWMLLSAFDLLYFLBHHHBQQLTZJCQJLDJTFFKMMMBYTHYGDCQRDDWRQJXNBYSNWZDBYYTBJHPYBYTTJXAAHGQDQTMYSTQXKBTZPKJLZRBEQQSSMJJBDJOTGTBXPGBKTLHQXJJJCTHXQDWJLWRFWQGWSHCKRYSWGFTGYGBXSDWDWRFHWYTJJXXXJYZYSLPYYYPAYXHYDQKXSHXYXGSKQHYWFDDDPPLCJLQQEEWXKSYYKDYPLTJTHKJLTCYYHHJTTPLTZZCDLTHQKZXQYSTEEYWYYZYXXYYSTTJKLLPZMCYHQGXYHSRMBXPLLNQYDQHXSXXWGDQBSHYLLPJJJTHYJKYPPTHYYKTYEZYENMDSHLCRPQFDGFXZPSFTLJXXJBSWYYSKSFLXLPPLBBBLBSFXFYZBSJSSYLPBBFFFFSSCJDSTZSXZRYYSYFFSYZYZBJTBCTSBSDHRTJJBYTCXYJEYLXCBNEBJDSYXYKGSJZBXBYTFZWGENYHHTHZHHXFWGCSTBGXKLSXYWMTMBYXJSTZSCDYQRCYTWXZFHMYMCXLZNSDJTTTXRYCFYJSBSDYERXJLJXBBDEYNJGHXGCKGSCYMBLXJMSZNSKGXFBNBPTHFJAAFXYXFPXMYPQDTZCXZZPXRSYWZDLYBBKTYQPQJPZYPZJZNJPZJLZZFYSBTTSLMPTZRTDXQSJEHBZYLZDHLJSQMLHTXTJECXSLZZSPKTLZKQQYFSYGYWPCPQFHQHYTQXZKRSGTTSQCZLPTXCDYYZXSQZSLXLZMYCPCQBZYXHBSXLZDLTCDXTYLZJYYZPZYZLTXJSJXHLPMYTXCQRBLZSSFJZZTNJYTXMYJHLHPPLCYXQJQQKZZSCPZKSWALQSBLCCZJSXGWWWYGYKTJBBZTDKHXHKGTGPBKQYSLPXPJCKBMLLXDZSTBKLGGQKQLSBKKTFXRMDKBFTPZFRTBBRFERQGXYJPZSSTLBZTPSZQZSJDHLJQLZBPMSMMSXLQQNHKNBLRDDNXXDHDDJCYYGYLXGZLXSYGMQQGKHBPMXYXLYTQWLWGCPBMQXCYZYDRJBHTDJYHQSHTMJSBYPLWHLZFFNYPMHXXHPLTBQPFBJWQDBYGPNZTPFZJGSDDTQSHZEAWZZYLLTYYBWJKXXGHLFKXDJTMSZSQYNZGGSWQSPHTLSSKMCLZXYSZQZXNCJDQGZDLFNYKLJCJLLZLMZZNHYDSSHTHZZLZZBBHQZWWYCRZHLYQQJBEYFXXXWHSRXWQHWPSLMSSKZTTYGYQQWRSLALHMJTQJSMXQBJJZJXZYZKXBYQXBJXSHZTSFJLXMXZXFGHKZSZGGYLCLSARJYHSLLLMZXELGLXYDJYTLFBHBPNLYZFBBHPTGJKWETZHKJJXZXXGLLJLSTGSHJJYQLQZFKCGNNDJSSZFDBCTWWSEQFHQJBSAQTGYPQLBXBMMYWXGSLZHGLZGQYFLZBYFZJFRYSFMBYZHQGFWZSYFYJJPHZBYYZFFWODGRLMFTWLBZGYCQXCDJYGZYYYYTYTYDWEGAZYHXJLZYYHLRMGRXXZCLHNELJJTJTPWJYBJJBXJJTJTEEKHWSLJPLPSFYZPQQBDLQJJTYYQLYZKDKSQJYYQZLDQTGJQYZJSUCMRYQTHTEJMFCTYHYPKMHYZWJDQFHYYXWSHCTXRLJHQXHCCYYYJLTKTTYTMXGTCJTZAYYOCZLYLBSZYWJYTSJYHBYSHFJLYGJXXTMZYYLTXXYPZLXYJZYZYYPNHMYMDYYLBLHLSYYQQLLNJJYMSOYQBZGDLYXYLCQYXTSZEGXHZGLHWBLJHEYXTWQMAKBPQCGYSHHEGQCMWYYWLJYJHYYZLLJJYLHZYHMGSLJLJXCJJYCLYCJPCPZJZJMMYLCQLNQLJQJSXYJMLSZLJQLYCMMHCFMMFPQQMFYLQMCFFQMMMMHMZNFHHJGTTHHKHSLNCHHYQDXTMMQDCYZYXYQMYQYLTDCYYYZAZZCYMZYDLZFFFMMYCQZWZZMABTBYZTDMNZZGGDFTYPCGQYTTSSFFWFDTZQSSYSTWXJHXYTSXXYLBYQHWWKXHZXWZNNZZJZJJQJCCCHYYXBZXZCYZTLLCQXYNJYCYYCYNZZQYYYEWYCZDCJYCCHYJLBTZYYCQWMPWPYMLGKDLDLGKQQBGYCHJXY";
            //此处收录了375个多音字
            string MultiPinyin = "19969:DZ,19975:WM,19988:QJ,20048:YL,20056:SC,20060:NM,20094:QG,20127:QJ,20167:QC,20193:YG,20250:KH,20256:ZC,20282:SC,20285:QJG,20291:TD,20314:YD,20340:NE,20375:TD,20389:YJ,20391:CZ,20415:PB,20446:YS,20447:SQ,20504:TC,20608:KG,20854:QJ,20857:ZC,20911:PF,20504:TC,20608:KG,20854:QJ,20857:ZC,20911:PF,20985:AW,21032:PB,21048:XQ,21049:SC,21089:YS,21119:JC,21242:SB,21273:SC,21305:YP,21306:QO,21330:ZC,21333:SDC,21345:QK,21378:CA,21397:SC,21414:XS,21442:SC,21477:JG,21480:TD,21484:ZS,21494:YX,21505:YX,21512:HG,21523:XH,21537:PB,21542:PF,21549:KH,21571:E,21574:DA,21588:TD,21589:O,21618:ZC,21621:KHA,21632:ZJ,21654:KG,21679:LKG,21683:KH,21710:A,21719:YH,21734:WOE,21769:A,21780:WN,21804:XH,21834:A,21899:ZD,21903:RN,21908:WO,21939:ZC,21956:SA,21964:YA,21970:TD,22003:A,22031:JG,22040:XS,22060:ZC,22066:ZC,22079:MH,22129:XJ,22179:XA,22237:NJ,22244:TD,22280:JQ,22300:YH,22313:XW,22331:YQ,22343:YJ,22351:PH,22395:DC,22412:TD,22484:PB,22500:PB,22534:ZD,22549:DH,22561:PB,22612:TD,22771:KQ,22831:HB,22841:JG,22855:QJ,22865:XQ,23013:ML,23081:WM,23487:SX,23558:QJ,23561:YW,23586:YW,23614:YW,23615:SN,23631:PB,23646:ZS,23663:ZT,23673:YG,23762:TD,23769:ZS,23780:QJ,23884:QK,24055:XH,24113:DC,24162:ZC,24191:GA,24273:QJ,24324:NL,24377:TD,24378:QJ,24439:PF,24554:ZS,24683:TD,24694:WE,24733:LK,24925:TN,25094:ZG,25100:XQ,25103:XH,25153:PB,25170:PB,25179:KG,25203:PB,25240:ZS,25282:FB,25303:NA,25324:KG,25341:ZY,25373:WZ,25375:XJ,25384:A,25457:A,25528:SD,25530:SC,25552:TD,25774:ZC,25874:ZC,26044:YW,26080:WM,26292:PB,26333:PB,26355:ZY,26366:CZ,26397:ZC,26399:QJ,26415:ZS,26451:SB,26526:ZC,26552:JG,26561:TD,26588:JG,26597:CZ,26629:ZS,26638:YL,26646:XQ,26653:KG,26657:XJ,26727:HG,26894:ZC,26937:ZS,26946:ZC,26999:KJ,27099:KJ,27449:YQ,27481:XS,27542:ZS,27663:ZS,27748:TS,27784:SC,27788:ZD,27795:TD,27812:O,27850:PB,27852:MB,27895:SL,27898:PL,27973:QJ,27981:KH,27986:HX,27994:XJ,28044:YC,28065:WG,28177:SM,28267:QJ,28291:KH,28337:ZQ,28463:TL,28548:DC,28601:TD,28689:PB,28805:JG,28820:QG,28846:PB,28952:TD,28975:ZC,29100:A,29325:QJ,29575:SL,29602:FB,30010:TD,30044:CX,30058:PF,30091:YSP,30111:YN,30229:XJ,30427:SC,30465:SX,30631:YQ,30655:QJ,30684:QJG,30707:SD,30729:XH,30796:LG,30917:PB,31074:NM,31085:JZ,31109:SC,31181:ZC,31192:MLB,31293:JQ,31400:YX,31584:YJ,31896:ZN,31909:ZY,31995:XJ,32321:PF,32327:ZY,32418:HG,32420:XQ,32421:HG,32438:LG,32473:GJ,32488:TD,32521:QJ,32527:PB,32562:ZSQ,32564:JZ,32735:ZD,32793:PB,33071:PF,33098:XL,33100:YA,33152:PB,33261:CX,33324:BP,33333:TD,33406:YA,33426:WM,33432:PB,33445:JG,33486:ZN,33493:TS,33507:QJ,33540:QJ,33544:ZC,33564:XQ,33617:YT,33632:QJ,33636:XH,33637:YX,33694:WG,33705:PF,33728:YW,33882:SR,34067:WM,34074:YW,34121:QJ,34255:ZC,34259:XL,34425:JH,34430:XH,34485:KH,34503:YS,34532:HG,34552:XS,34558:YE,34593:ZL,34660:YQ,34892:XH,34928:SC,34999:QJ,35048:PB,35059:SC,35098:ZC,35203:TQ,35265:JX,35299:JX,35782:SZ,35828:YS,35830:E,35843:TD,35895:YG,35977:MH,36158:JG,36228:QJ,36426:XQ,36466:DC,36710:JC,36711:ZYG,36767:PB,36866:SK,36951:YW,37034:YX,37063:XH,37218:ZC,37325:ZC,38063:PB,38079:TD,38085:QY,38107:DC,38116:TD,38123:YD,38224:HG,38241:XTC,38271:ZC,38415:YE,38426:KH,38461:YD,38463:AE,38466:PB,38477:XJ,38518:YT,38551:WK,38585:ZC,38704:XS,38739:LJ,38761:GJ,38808:SQ,39048:JG,39049:XJ,39052:HG,39076:CZ,39271:XT,39534:TD,39552:TD,39584:PB,39647:SB,39730:LG,39748:TPB,40109:ZQ,40479:ND,40516:HG,40536:HG,40583:QJ,40765:YQ,40784:QJ,40840:YK,40863:QJG,";
            string resStr = "";
            int i, j, uni;
            uni = (UInt16)chineseCharacter;
            if (uni > 40869 || uni < 19968)
                return resStr;
            //返回该字符在Unicode字符集中的编码值
            i = MultiPinyin.IndexOf(uni.ToString());
            //检查是否是多音字,是按多音字处理,不是就直接在strChineseFirstPY字符串中找对应的首字母
            if (i < 0)
            //获取非多音字汉字首字母
            {
                resStr = strChineseFirstPY[uni - 19968].ToString();
            }
            else
            { //获取多音字汉字首字母
                j = MultiPinyin.IndexOf(",", i);
                resStr = MultiPinyin.Substring(i + 6, j - i - 6);
            }
            return resStr;

        }
        #endregion
    }
}
