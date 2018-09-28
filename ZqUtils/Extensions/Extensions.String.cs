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
        /// <returns>bool</returns>
        public static bool IsNull(this string @this)
        {
            var result = true;
            if (@this != null) result = @this.Trim() == "" || @this.Trim().ToLower() == "null" || @this.Trim() == "[]" || @this.Trim() == "{}";
            return result;
        }
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
                return default(T);
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

        #region GMT日期字符串转换为DateTime
        /// <summary>
        /// GMT字符串转换为DateTime
        /// </summary>
        /// <param name="this">GMT日期字符串</param>
        /// <returns>DateTime</returns>
        public static DateTime ToDateTime(this string @this)
        {
            var dt = DateTime.MinValue;
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

        #region 字符串判断是否为空
        /// <summary>
        /// 指示指定的字符串是 null 还是 string.Empty 字符串
        /// </summary>
        /// <param name="this">当前字符串</param>
        /// <returns>bool</returns>
        public static bool IsNullOrEmpty(this string @this) => @this == null || @this.Length <= 0;

        /// <summary>
        /// 是否空或者空白字符串
        /// </summary>
        /// <param name="this">当前字符串</param>
        /// <returns>bool</returns>
        public static bool IsNullOrWhiteSpace(this string @this)
        {
            if (@this != null)
            {
                for (var i = 0; i < @this.Length; i++)
                {
                    if (!char.IsWhiteSpace(@this[i])) return false;
                }
            }
            return true;
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
            if (!string.IsNullOrEmpty(@this))
            {
                var sb = new StringBuilder(seed);
                var array = @this.TrimEnd(separator).Split(separator);
                if (!isEnableNullValue)
                {
                    array = array.Where(o => !string.IsNullOrEmpty(o)).ToArray();
                }
                if (distinct)
                {
                    array = array.Distinct().ToArray();
                }
                foreach (var item in array)
                {
                    sb.Append(current(item));
                }
                if (!string.IsNullOrEmpty(remove))
                {
                    sb = sb.Remove(sb.ToString().LastIndexOf(remove), remove.Length);
                }
                return sb.ToString();
            }
            return @this;
        }
        #endregion
    }
}
