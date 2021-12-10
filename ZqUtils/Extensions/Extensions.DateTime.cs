#region License
/***
 * Copyright © 2018-2021, 张强 (943620963@qq.com).
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
using System.Globalization;
using System.Text;
/****************************
* [Author] 张强
* [Date] 2016-10-17
* [Describe] DateTime扩展类
* **************************/
namespace ZqUtils.Extensions
{
    /// <summary>
    /// DateTime扩展类
    /// </summary>
    public static class DateTimeExtensions
    {
        #region ToTimeStamp
        /// <summary>
        /// 时间戳
        /// </summary>
        /// <param name="this">默认取DateTime.UtcNow</param>
        /// <param name="type">默认10位[13位]</param>
        /// <returns>string</returns>
        public static string ToTimeStamp(this DateTime @this, int type = 10)
        {
            var ts = @this - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var t = string.Empty;
            if (type == 10)
            {
                t = Convert.ToInt64(ts.TotalSeconds).ToString();
            }
            else if (type == 13)
            {
                t = Convert.ToInt64(ts.TotalMilliseconds).ToString();
            }
            return t;
        }
        #endregion

        #region ToUnixTime
        /// <summary>
        /// datetime转换为unixtime
        /// </summary>
        /// <param name="this">默认取DateTime.Now</param>
        /// <returns>int</returns>
        public static int ToUnixTime(this DateTime @this)
        {
            var startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            return (int)(@this - startTime).TotalSeconds;
        }
        #endregion

        #region ToLongTime
        /// <summary>
        /// DateTime类型转换为长整型时间
        /// </summary>
        /// <param name="this">默认取DataTime.Now</param>
        /// <returns>long</returns>
        public static long ToLongTime(this DateTime @this)
        {
            var start = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            var timeSpan = @this.Subtract(start).Ticks.ToString();
            return long.Parse(timeSpan.Substring(0, timeSpan.Length - 4));
        }
        #endregion

        #region ToJavaLongTime
        /// <summary>
        /// DateTime类型转换为java的长整型时间
        /// </summary>
        /// <param name="this">time取DateTime.UtcNow，此时则为java的System.currentTimeMillis()</param>
        /// <returns>long</returns>
        public static long ToJavaLongTime(this DateTime @this)
        {
            return (long)(@this - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }
        #endregion

        #region ToDateTime
        /// <summary>
        /// 长整型时间转换为DateTime类型
        /// </summary>
        /// <param name="this">长整型时间</param>
        /// <returns></returns>
        public static DateTime ToDateTime(this long @this)
        {
            //var tricks = new DateTime(1970, 1, 1, 0, 0, 0).Ticks + @this * 10000;
            //return new DateTime(tricks).AddHours(8);
            var start = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Local);
            return start.Add(new TimeSpan(long.Parse(@this + "0000")));
        }

        /// <summary>
        /// 年周(YYWW/YYYYWW)转换为年月日
        /// </summary>
        /// <param name="this">年周字符串，如(2020年第34周)：2034/202034</param>
        /// <param name="dayOfWeek">周的开始日期：Sunday或者Monday</param>
        /// <param name="weekRule">年第一周规则</param>
        /// <param name="lastYear">如果当年第一天不是一周的开始，是否倒推上一年日期</param>
        /// <returns></returns>
        public static DateTime? ToDateTime(
            this string @this,
            DayOfWeek dayOfWeek = DayOfWeek.Sunday,
            CalendarWeekRule weekRule = CalendarWeekRule.FirstDay,
            bool lastYear = false)
        {
            //判断是否为空
            if (@this.IsNullOrWhiteSpace())
                return null;

            //年份
            string year = null;

            //YYWW
            if (@this.Length == 4)
                year = DateTime.Now.Year.ToString().Substring(0, 2) + @this.Substring(0, 2);
            //YYYYWW
            else if (@this.Length == 6)
                year = @this.Substring(0, 4);

            //第几周
            var weeks = @this.Substring(@this.Length - 2).ToInt();

            //当年第一天
            var start = $"{year}-01-01".ToDateTime().Value;

            //当年第一天周几
            var startDayOfWeek = start.DayOfWeek;

            //第一周剩余天数
            int firstWeekDays;

            //周日
            if (dayOfWeek == DayOfWeek.Sunday)
                firstWeekDays = 6 - (int)startDayOfWeek;
            //周一
            else
                firstWeekDays = 7 - (int)startDayOfWeek;

            //当年第一天位于年第几周
            var weekOrYear = start.WeekOfYear(dayOfWeek, weekRule);

            //如果当年第一天不是第一周，则重新计算当年第一周开始日期
            if (weekOrYear != 1)
                start = start.AddDays(firstWeekDays + 1);

            //第一周
            if (weeks == 1)
            {
                //是否倒推上一年
                if (!lastYear)
                    return start;

                //周日
                if (dayOfWeek == DayOfWeek.Sunday)
                    return start.AddDays(-(int)startDayOfWeek);
                //周一
                else
                    return start.AddDays(-(int)startDayOfWeek + 1);
            }

            //年开始日期 + 第一周天数 + 中间天数 + 最后一周只算1天
            return start.AddDays(firstWeekDays).AddDays((weeks - 2) * 7).AddDays(1);
        }

        /// <summary>
        /// 年周(YYWW/YYYYWW)转换为年月日
        /// </summary>
        /// <param name="this">年周字符串，如(2020年第34周)：2034/202034</param>
        /// <param name="calendarTypes">公历类型</param>
        /// <param name="dayOfWeek">周的开始日期：Sunday或者Monday</param>
        /// <param name="weekRule">年第一周规则</param>
        /// <param name="lastYear">如果当年第一天不是一周的开始，是否倒推上一年日期</param>
        /// <returns></returns>
        public static DateTime? ToDateTime(
            this string @this,
            GregorianCalendarTypes calendarTypes,
            DayOfWeek dayOfWeek = DayOfWeek.Sunday,
            CalendarWeekRule weekRule = CalendarWeekRule.FirstDay,
            bool lastYear = false)
        {
            //判断是否为空
            if (@this.IsNullOrWhiteSpace())
                return null;

            //年份
            string year = null;

            //YYWW
            if (@this.Length == 4)
                year = DateTime.Now.Year.ToString().Substring(0, 2) + @this.Substring(0, 2);
            //YYYYWW
            else if (@this.Length == 6)
                year = @this.Substring(0, 4);

            //第几周
            var weeks = @this.Substring(@this.Length - 2).ToInt();

            //当年第一天
            var start = $"{year}-01-01".ToDateTime().Value;

            //当年第一天周几
            var startDayOfWeek = start.DayOfWeek;

            //第一周剩余天数
            int firstWeekDays;

            //周日
            if (dayOfWeek == DayOfWeek.Sunday)
                firstWeekDays = 6 - (int)startDayOfWeek;
            //周一
            else
                firstWeekDays = 7 - (int)startDayOfWeek;

            //当年第一天位于年第几周
            var weekOrYear = start.WeekOfYear(calendarTypes, dayOfWeek, weekRule);

            //如果当年第一天不是第一周，则重新计算当年第一周开始日期
            if (weekOrYear != 1)
                start = start.AddDays(firstWeekDays + 1);

            //第一周
            if (weeks == 1)
            {
                //是否倒推上一年
                if (!lastYear)
                    return start;

                //周日
                if (dayOfWeek == DayOfWeek.Sunday)
                    return start.AddDays(-(int)startDayOfWeek);
                //周一
                else
                    return start.AddDays(-(int)startDayOfWeek + 1);
            }

            //年开始日期 + 第一周天数 + 中间天数 + 最后一周只算1天
            return start.AddDays(firstWeekDays).AddDays((weeks - 2) * 7).AddDays(1);
        }

        /// <summary>
        /// 年周(YYWW/YYYYWW)转换为年月日
        /// </summary>
        /// <param name="this">年周字符串，如(2020年第34周)：2034/202034</param>
        /// <param name="ci">区域信息，示例：new CultureInfo("zh-CN")</param>
        /// <param name="lastYear">如果当年第一天不是一周的开始，是否倒推上一年日期</param>
        /// <returns></returns>
        public static DateTime? ToDateTime(
            this string @this,
            CultureInfo ci,
            bool lastYear = false)
        {
            return @this.ToDateTime(ci.DateTimeFormat.FirstDayOfWeek, ci.DateTimeFormat.CalendarWeekRule, lastYear);
        }

        /// <summary>
        /// GMT字符串/日期字符串/时间戳 转DateTime
        /// </summary>
        /// <param name="this">日期字符串</param>
        /// <returns>DateTime</returns>
        public static DateTime? ToDateTime(this string @this)
        {
            if (@this.IsNotNullOrEmpty())
            {
                #region GMT日期
                if (@this.ContainsIgnoreCase("GMT"))
                {
                    var pattern = string.Empty;

                    if (@this.IndexOf("+0") != -1)
                    {
                        @this = @this.Replace("GMT", "").Replace("gmt", "");
                        pattern = "ddd, dd MMM yyyy HH':'mm':'ss zzz";
                    }

                    if (@this.ContainsIgnoreCase("GMT"))
                        pattern = "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'";

                    if (pattern.IsNotNullOrEmpty())
                    {
                        var dt = DateTime.ParseExact(@this, pattern, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                        return dt.ToLocalTime();
                    }
                    else
                    {
                        return Convert.ToDateTime(@this);
                    }
                }
                #endregion

                #region 日期字符串
                else if (@this.ContainsIgnoreCase("-", ":", "/"))
                {
                    if (DateTime.TryParse(@this, out DateTime time))
                        return time;
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
                        return start.Add(time);
                    }
                    else if (type == 13)
                    {
                        var ticks = long.Parse(@this + "0000");
                        var time = new TimeSpan(ticks);
                        return start.Add(time);
                    }
                }
                #endregion
            }

            return null;
        }

        /// <summary>
        /// 自定义格式转换日期字符串
        /// </summary>
        /// <param name="this"></param>
        /// <param name="format">日期字符串格式化</param>
        /// <param name="provider">格式化驱动</param>
        /// <param name="style">日期类型</param>
        /// <returns></returns>
        public static DateTime? ToDateTime(
            this string @this, 
            string format, 
            IFormatProvider provider = null,
            DateTimeStyles style = DateTimeStyles.None)
        {
            if (format.IsNotNullOrEmpty())
            {
                var res = DateTime.TryParseExact(@this, format, provider, style, out var time);
                if (res)
                    return time;
            }
            else
            {
                var res = DateTime.TryParse(@this, out var time);
                if (res)
                    return time;
            }

            return null;
        }
        #endregion

        #region ToGMT
        /// <summary>
        /// 格式化为GMT日期类型
        /// </summary>
        /// <param name="this">当前时间</param>
        /// <returns>string</returns>
        public static string ToGMT(this DateTime @this)
        {
            return @this.ToString("r") + @this.ToString("zzz").Replace(":", "");
        }
        #endregion

        #region WeekOfMonth
        /// <summary>
        /// 获取指定时间是当月的第几周
        /// </summary>
        /// <param name="this">当前时间</param>
        /// <param name="type">1:周一至周日 为一周;2:表示 周日至周六 为一周</param>
        /// <returns>int</returns>
        public static int WeekOfMonth(this DateTime @this, int type = 1)
        {
            var firstOfMonth = Convert.ToDateTime($"{@this.Date.Year}-{@this.Date.Month}-{1}");
            var i = (int)firstOfMonth.Date.DayOfWeek;
            if (i == 0) i = 7;
            if (type == 1) return (@this.Date.Day + i - 2) / 7 + 1;
            if (type == 2) return (@this.Date.Day + i - 1) / 7;
            return 0;
        }
        #endregion

        #region WeekOfYear
        /// <summary>
        /// 获取指定时间是当年的第几周
        /// </summary>
        /// <param name="this">目标日期</param>
        /// <param name="dayOfWeek">周的开始日期</param>
        /// <param name="weekRule">年第一周规则</param>
        /// <returns></returns>
        public static int WeekOfYear(
            this DateTime @this,
            DayOfWeek dayOfWeek = DayOfWeek.Sunday,
            CalendarWeekRule weekRule = CalendarWeekRule.FirstDay)
        {
            var gc = new GregorianCalendar();
            return gc.GetWeekOfYear(@this, weekRule, dayOfWeek);
        }

        /// <summary>
        /// 获取指定时间是当年的第几周
        /// </summary>
        /// <param name="this">目标日期</param>
        /// <param name="dayOfWeek">周的开始日期</param>
        /// <param name="weekRule">年第一周规则</param>
        /// <param name="lastFullWeek">最后一周不满整周，是否为下年的第一周</param>
        /// <returns></returns>
        public static int WeekOfYear(
            this DateTime @this,
            DayOfWeek dayOfWeek = DayOfWeek.Sunday,
            CalendarWeekRule weekRule = CalendarWeekRule.FirstDay,
            bool lastFullWeek = false)
        {
            var weekOfYear = @this.WeekOfYear(dayOfWeek, weekRule);

            //是否判断最后一周满周
            if (lastFullWeek)
            {
                @this = @this.ToDateString().ToDateTime().Value;
                var lastDay = $"{@this.Year}-12-31".ToDateTime().Value;
                var lastDayOfWeek = lastDay.DayOfWeek;

                //周日为周开始日期
                if (dayOfWeek == DayOfWeek.Sunday)
                {
                    //周的结束日期
                    if (lastDayOfWeek != DayOfWeek.Saturday)
                    {
                        var firstDay = lastDay.AddDays(-(int)lastDayOfWeek);
                        if (@this >= firstDay && @this <= lastDay)
                            weekOfYear = 1;
                    }
                }
                //周一为周开始日期
                else if (dayOfWeek == DayOfWeek.Monday)
                {
                    //周的结束日期
                    if (lastDayOfWeek != DayOfWeek.Sunday)
                    {
                        var firstDay = lastDay.AddDays(-(int)lastDayOfWeek + 1);
                        if (@this >= firstDay && @this <= lastDay)
                            weekOfYear = 1;
                    }
                }
            }

            return weekOfYear;
        }

        /// <summary>
        /// 获取指定时间是当年的第几周
        /// </summary>
        /// <param name="this">目标日期</param>
        /// <param name="calendarTypes">公历类型</param>
        /// <param name="dayOfWeek">周的开始日期</param>
        /// <param name="weekRule">年第一周规则</param>
        /// <returns></returns>
        public static int WeekOfYear(
            this DateTime @this,
            GregorianCalendarTypes calendarTypes,
            DayOfWeek dayOfWeek = DayOfWeek.Sunday,
            CalendarWeekRule weekRule = CalendarWeekRule.FirstDay)
        {
            var gc = new GregorianCalendar(calendarTypes);
            return gc.GetWeekOfYear(@this, weekRule, dayOfWeek);
        }

        /// <summary>
        /// 获取指定时间是当年的第几周
        /// </summary>
        /// <param name="this">目标日期</param>
        /// <param name="calendarTypes">公历类型</param>
        /// <param name="dayOfWeek">周的开始日期</param>
        /// <param name="weekRule">年第一周规则</param>
        /// <param name="lastFullWeek">最后一周不满整周，是否为下年的第一周</param>
        /// <returns></returns>
        public static int WeekOfYear(
            this DateTime @this,
            GregorianCalendarTypes calendarTypes,
            DayOfWeek dayOfWeek = DayOfWeek.Sunday,
            CalendarWeekRule weekRule = CalendarWeekRule.FirstDay,
            bool lastFullWeek = false)
        {
            var weekOfYear = @this.WeekOfYear(calendarTypes, dayOfWeek, weekRule);

            //是否判断最后一周满周
            if (lastFullWeek)
            {
                @this = @this.ToDateString().ToDateTime().Value;
                var lastDay = $"{@this.Year}-12-31".ToDateTime().Value;
                var lastDayOfWeek = lastDay.DayOfWeek;

                //周日为周开始日期
                if (dayOfWeek == DayOfWeek.Sunday)
                {
                    //周的结束日期
                    if (lastDayOfWeek != DayOfWeek.Saturday)
                    {
                        var firstDay = lastDay.AddDays(-(int)lastDayOfWeek);
                        if (@this >= firstDay && @this <= lastDay)
                            weekOfYear = 1;
                    }
                }
                //周一为周开始日期
                else if (dayOfWeek == DayOfWeek.Monday)
                {
                    //周的结束日期
                    if (lastDayOfWeek != DayOfWeek.Sunday)
                    {
                        var firstDay = lastDay.AddDays(-(int)lastDayOfWeek + 1);
                        if (@this >= firstDay && @this <= lastDay)
                            weekOfYear = 1;
                    }
                }
            }

            return weekOfYear;
        }

        /// <summary>
        /// 获取指定时间是当年的第几周
        /// </summary>
        /// <param name="this">目标日期</param>
        /// <param name="ci">区域信息，示例：new CultureInfo("zh-CN")</param>
        /// <returns></returns>
        public static int WeekOfYear(
            this DateTime @this,
            CultureInfo ci)
        {
            return ci.Calendar.GetWeekOfYear(@this, ci.DateTimeFormat.CalendarWeekRule, ci.DateTimeFormat.FirstDayOfWeek);
        }

        /// <summary>
        /// 获取指定时间是当年的第几周
        /// </summary>
        /// <param name="this">目标日期</param>
        /// <param name="ci">区域信息，示例：new CultureInfo("zh-CN")</param>
        /// <param name="lastFullWeek">最后一周不满整周，是否为下年的第一周</param>
        /// <returns></returns>
        public static int WeekOfYear(
            this DateTime @this,
            CultureInfo ci,
            bool lastFullWeek = false)
        {
            var weekOfYear = @this.WeekOfYear(ci);

            //是否判断最后一周满周
            if (lastFullWeek)
            {
                @this = @this.ToDateString().ToDateTime().Value;
                var lastDay = $"{@this.Year}-12-31".ToDateTime().Value;
                var lastDayOfWeek = lastDay.DayOfWeek;

                //周日为周开始日期
                if (ci.DateTimeFormat.FirstDayOfWeek == DayOfWeek.Sunday)
                {
                    //周的结束日期
                    if (lastDayOfWeek != DayOfWeek.Saturday)
                    {
                        var firstDay = lastDay.AddDays(-(int)lastDayOfWeek);
                        if (@this >= firstDay && @this <= lastDay)
                            weekOfYear = 1;
                    }
                }
                //周一为周开始日期
                else if (ci.DateTimeFormat.FirstDayOfWeek == DayOfWeek.Monday)
                {
                    //周的结束日期
                    if (lastDayOfWeek != DayOfWeek.Sunday)
                    {
                        var firstDay = lastDay.AddDays(-(int)lastDayOfWeek + 1);
                        if (@this >= firstDay && @this <= lastDay)
                            weekOfYear = 1;
                    }
                }
            }

            return weekOfYear;
        }
        #endregion

        #region Format DateTime
        /// <summary>
        /// 获取格式化字符串，带时分秒，格式："yyyy-MM-dd HH:mm:ss"
        /// </summary>
        /// <param name="this">日期</param>
        /// <param name="isRemoveSecond">是否移除秒</param>
        /// <param name="identifier">标识符“-”</param>
        public static string ToDateTimeString(this DateTime @this, bool isRemoveSecond = false, string identifier = "-")
        {
            if (@this == null)
                return string.Empty;
            if (isRemoveSecond)
                return @this.ToString($"yyyy{identifier}MM{identifier}dd HH:mm");
            return @this.ToString($"yyyy{identifier}MM{identifier}dd HH:mm:ss");
        }

        /// <summary>
        /// 获取格式化字符串，带时分秒，格式："yyyy-MM-dd HH:mm:ss"
        /// </summary>
        /// <param name="this">日期</param>
        /// <param name="isRemoveSecond">是否移除秒</param>
        /// <param name="identifier">标识符“-”</param>
        public static string ToDateTimeString(this DateTime? @this, bool isRemoveSecond = false, string identifier = "-")
        {
            if (@this == null)
                return string.Empty;
            return ToDateTimeString(@this.Value, isRemoveSecond, identifier);
        }

        /// <summary>
        /// 获取格式化字符串，不带时分秒，格式："yyyy-MM-dd"
        /// </summary>
        /// <param name="this">日期</param>
        /// <param name="identifier">标识符“-”</param>
        public static string ToDateString(this DateTime @this, string identifier = "-")
        {
            if (@this == null)
                return string.Empty;
            return @this.ToString($"yyyy{identifier}MM{identifier}dd");
        }

        /// <summary>
        /// 获取格式化字符串，不带时分秒，格式："yyyy-MM-dd"
        /// </summary>
        /// <param name="this">日期</param>
        /// <param name="identifier">标识符“-”</param>
        public static string ToDateString(this DateTime? @this, string identifier = "-")
        {
            if (@this == null)
                return string.Empty;
            return ToDateString(@this.Value, identifier);
        }

        /// <summary>
        /// 获取格式化字符串，不带年月日，格式："HH:mm:ss"
        /// </summary>
        /// <param name="this">日期</param>
        public static string ToTimeString(this DateTime @this)
        {
            if (@this == null)
                return string.Empty;
            return @this.ToString("HH:mm:ss");
        }

        /// <summary>
        /// 获取格式化字符串，不带年月日，格式："HH:mm:ss"
        /// </summary>
        /// <param name="this">日期</param>
        public static string ToTimeString(this DateTime? @this)
        {
            if (@this == null)
                return string.Empty;
            return ToTimeString(@this.Value);
        }

        /// <summary>
        /// 获取格式化字符串，带毫秒，格式："yyyy-MM-dd HH:mm:ss.fff"
        /// </summary>
        /// <param name="this">日期</param>
        /// <param name="identifier">标识符“-”</param>
        public static string ToMillisecondString(this DateTime @this, string identifier = "-")
        {
            if (@this == null)
                return string.Empty;
            return @this.ToString($"yyyy{identifier}MM{identifier}dd HH:mm:ss.fff");
        }

        /// <summary>
        /// 获取格式化字符串，带毫秒，格式："yyyy-MM-dd HH:mm:ss.fff"
        /// </summary>
        /// <param name="this">日期</param>
        /// <param name="identifier">标识符“-”</param>
        public static string ToMillisecondString(this DateTime? @this, string identifier = "-")
        {
            if (@this == null)
                return string.Empty;
            return ToMillisecondString(@this.Value, identifier);
        }

        /// <summary>
        /// 获取格式化字符串，不带时分秒，格式："yyyy年MM月dd日"
        /// </summary>
        /// <param name="this">日期</param>
        public static string ToChineseDateString(this DateTime @this)
        {
            if (@this == null)
                return string.Empty;
            return @this.ToString("yyyy年MM月dd日");
        }

        /// <summary>
        /// 获取格式化字符串，不带时分秒，格式："yyyy年MM月dd日"
        /// </summary>
        /// <param name="this">日期</param>
        public static string ToChineseDateString(this DateTime? @this)
        {
            if (@this == null)
                return string.Empty;
            return ToChineseDateString(@this.Value);
        }

        /// <summary>
        /// 获取格式化字符串，带时分秒，格式："yyyy年MM月dd日 HH时mm分"
        /// </summary>
        /// <param name="this">日期</param>
        /// <param name="isRemoveSecond">是否移除秒</param>
        public static string ToChineseDateTimeString(this DateTime @this, bool isRemoveSecond = false)
        {
            if (@this == null)
                return string.Empty;
            var result = new StringBuilder(@this.ToString("yyyy年MM月dd日 HH时mm分")); ;
            if (isRemoveSecond == false)
                result.Append($"{@this.Second}秒");
            return result.ToString();
        }

        /// <summary>
        /// 获取格式化字符串，带时分秒，格式："yyyy年MM月dd日 HH时mm分"
        /// </summary>
        /// <param name="this">日期</param>
        /// <param name="isRemoveSecond">是否移除秒</param>
        public static string ToChineseDateTimeString(this DateTime? @this, bool isRemoveSecond = false)
        {
            if (@this == null)
                return string.Empty;
            return ToChineseDateTimeString(@this.Value);
        }
        #endregion

        #region Between
        /// <summary>
        /// A T extension method that check if the value is between (exclusif) the minValue and maxValue.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <returns>true if the value is between the minValue and maxValue, otherwise false.</returns>
        public static bool Between(this DateTime @this, DateTime minValue, DateTime maxValue)
        {
            return minValue.CompareTo(@this) == -1 && @this.CompareTo(maxValue) == -1;
        }
        #endregion

        #region In
        /// <summary>
        /// A T extension method to determines whether the object is equal to any of the provided values.
        /// </summary>
        /// <param name="this">The object to be compared.</param>
        /// <param name="values">The value list to compare with the object.</param>
        /// <returns>true if the values list contains the object, else false.</returns>
        public static bool In(this DateTime @this, params DateTime[] values)
        {
            return Array.IndexOf(values, @this) != -1;
        }
        #endregion

        #region InRange
        /// <summary>
        /// A T extension method that check if the value is between inclusively the minValue and maxValue.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <returns>true if the value is between inclusively the minValue and maxValue, otherwise false.</returns>
        public static bool InRange(this DateTime @this, DateTime minValue, DateTime maxValue)
        {
            return @this.CompareTo(minValue) >= 0 && @this.CompareTo(maxValue) <= 0;
        }
        #endregion

        #region NotIn
        /// <summary>
        /// A T extension method to determines whether the object is not equal to any of the provided values.
        /// </summary>
        /// <param name="this">The object to be compared.</param>
        /// <param name="values">The value list to compare with the object.</param>
        /// <returns>true if the values list doesn't contains the object, else false.</returns>
        public static bool NotIn(this DateTime @this, params DateTime[] values)
        {
            return Array.IndexOf(values, @this) == -1;
        }
        #endregion

        #region IsDaylightSavingTime
        /// <summary>
        /// Returns a value indicating whether the specified date and time is within the specified daylight saving time
        /// period.
        /// </summary>
        /// <param name="this">A date and time.</param>
        /// <param name="daylightTimes">A daylight saving time period.</param>
        /// <returns>true if  is in ; otherwise, false.</returns>
        public static bool IsDaylightSavingTime(this DateTime @this, DaylightTime daylightTimes)
        {
            return TimeZone.IsDaylightSavingTime(@this, daylightTimes);
        }
        #endregion

        #region ConvertTime
        /// <summary>
        /// Converts a time to the time in a particular time zone.
        /// </summary>
        /// <param name="this">The date and time to convert.</param>
        /// <param name="destinationTimeZone">The time zone to convert  to.</param>
        /// <returns>The date and time in the destination time zone.</returns>
        public static DateTime ConvertTime(this DateTime @this, TimeZoneInfo destinationTimeZone)
        {
            return TimeZoneInfo.ConvertTime(@this, destinationTimeZone);
        }

        /// <summary>
        /// Converts a time from one time zone to another.
        /// </summary>
        /// <param name="this">The date and time to convert.</param>
        /// <param name="sourceTimeZone">The time zone of .</param>
        /// <param name="destinationTimeZone">The time zone to convert  to.</param>
        /// <returns>
        /// The date and time in the destination time zone that corresponds to the  parameter in the source time zone.
        /// </returns>
        public static DateTime ConvertTime(this DateTime @this, TimeZoneInfo sourceTimeZone, TimeZoneInfo destinationTimeZone)
        {
            return TimeZoneInfo.ConvertTime(@this, sourceTimeZone, destinationTimeZone);
        }
        #endregion

        #region ConvertToDateTimeOffset
        /// <summary>
        /// 将 DateTime 转换成 DateTimeOffset
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTimeOffset ConvertToDateTimeOffset(this DateTime dateTime)
        {
            return DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
        }
        #endregion

        #region ConvertTimeBySystemTimeZoneId
        /// <summary>
        /// Converts a time to the time in another time zone based on the time zone&#39;s identifier.
        /// </summary>
        /// <param name="this">The date and time to convert.</param>
        /// <param name="destinationTimeZoneId">The identifier of the destination time zone.</param>
        /// <returns>The date and time in the destination time zone.</returns>
        public static DateTime ConvertTimeBySystemTimeZoneId(this DateTime @this, string destinationTimeZoneId)
        {
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(@this, destinationTimeZoneId);
        }

        /// <summary>
        /// Converts a time from one time zone to another based on time zone identifiers.
        /// </summary>
        /// <param name="this">The date and time to convert.</param>
        /// <param name="sourceTimeZoneId">The identifier of the source time zone.</param>
        /// <param name="destinationTimeZoneId">The identifier of the destination time zone.</param>
        /// <returns>
        /// The date and time in the destination time zone that corresponds to the  parameter in the source time zone.
        /// </returns>
        public static DateTime ConvertTimeBySystemTimeZoneId(this DateTime @this, string sourceTimeZoneId, string destinationTimeZoneId)
        {
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(@this, sourceTimeZoneId, destinationTimeZoneId);
        }
        #endregion

        #region ConvertTimeFromUtc
        /// <summary>
        /// Converts a Coordinated Universal Time (UTC) to the time in a specified time zone.
        /// </summary>
        /// <param name="this">The Coordinated Universal Time (UTC).</param>
        /// <param name="destinationTimeZone">The time zone to convert  to.</param>
        /// <returns>
        /// The date and time in the destination time zone. Its  property is  if  is ; otherwise, its  property is .
        /// </returns>
        public static DateTime ConvertTimeFromUtc(this DateTime @this, TimeZoneInfo destinationTimeZone)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(@this, destinationTimeZone);
        }
        #endregion

        #region ConvertTimeToUtc
        /// <summary>
        /// Converts the current date and time to Coordinated Universal Time (UTC).
        /// </summary>
        /// <param name="this">The date and time to convert.</param>
        /// <returns>
        /// The Coordinated Universal Time (UTC) that corresponds to the  parameter. The  value&#39;s  property is always
        /// set to .
        /// </returns>
        public static DateTime ConvertTimeToUtc(this DateTime @this)
        {
            return TimeZoneInfo.ConvertTimeToUtc(@this);
        }

        /// <summary>
        /// Converts the time in a specified time zone to Coordinated Universal Time (UTC).
        /// </summary>
        /// <param name="this">The date and time to convert.</param>
        /// <param name="sourceTimeZone">The time zone of .</param>
        /// <returns>
        /// The Coordinated Universal Time (UTC) that corresponds to the  parameter. The  object&#39;s  property is
        /// always set to .
        /// </returns>
        public static DateTime ConvertTimeToUtc(this DateTime @this, TimeZoneInfo sourceTimeZone)
        {
            return TimeZoneInfo.ConvertTimeToUtc(@this, sourceTimeZone);
        }
        #endregion

        #region ToFullDateTimeString
        /// <summary>
        /// A DateTime extension method that converts this object to a full date time string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToFullDateTimeString(this DateTime @this)
        {
            return @this.ToString("F", DateTimeFormatInfo.CurrentInfo);
        }

        /// <summary>
        /// A DateTime extension method that converts this object to a full date time string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToFullDateTimeString(this DateTime @this, string culture)
        {
            return @this.ToString("F", new CultureInfo(culture));
        }

        /// <summary>
        /// A DateTime extension method that converts this object to a full date time string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToFullDateTimeString(this DateTime @this, CultureInfo culture)
        {
            return @this.ToString("F", culture);
        }
        #endregion

        #region ToLongDateShortTimeString
        /// <summary>
        /// A DateTime extension method that converts this object to a long date short time string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToLongDateShortTimeString(this DateTime @this)
        {
            return @this.ToString("f", DateTimeFormatInfo.CurrentInfo);
        }

        /// <summary>
        /// A DateTime extension method that converts this object to a long date short time string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToLongDateShortTimeString(this DateTime @this, string culture)
        {
            return @this.ToString("f", new CultureInfo(culture));
        }

        /// <summary>
        /// A DateTime extension method that converts this object to a long date short time string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToLongDateShortTimeString(this DateTime @this, CultureInfo culture)
        {
            return @this.ToString("f", culture);
        }
        #endregion

        #region ToLongDateString
        /// <summary>
        /// A DateTime extension method that converts this object to a long date string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToLongDateString(this DateTime @this)
        {
            return @this.ToString("D", DateTimeFormatInfo.CurrentInfo);
        }

        /// <summary>
        /// A DateTime extension method that converts this object to a long date string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToLongDateString(this DateTime @this, string culture)
        {
            return @this.ToString("D", new CultureInfo(culture));
        }

        /// <summary>
        /// A DateTime extension method that converts this object to a long date string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToLongDateString(this DateTime @this, CultureInfo culture)
        {
            return @this.ToString("D", culture);
        }
        #endregion

        #region ToLongDateTimeString
        /// <summary>
        /// A DateTime extension method that converts this object to a long date time string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToLongDateTimeString(this DateTime @this)
        {
            return @this.ToString("F", DateTimeFormatInfo.CurrentInfo);
        }

        /// <summary>
        /// A DateTime extension method that converts this object to a long date time string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToLongDateTimeString(this DateTime @this, string culture)
        {
            return @this.ToString("F", new CultureInfo(culture));
        }

        /// <summary>
        /// A DateTime extension method that converts this object to a long date time string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToLongDateTimeString(this DateTime @this, CultureInfo culture)
        {
            return @this.ToString("F", culture);
        }
        #endregion

        #region ToLongTimeString
        /// <summary>
        /// A DateTime extension method that converts this object to a long time string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToLongTimeString(this DateTime @this)
        {
            return @this.ToString("T", DateTimeFormatInfo.CurrentInfo);
        }

        /// <summary>
        /// A DateTime extension method that converts this object to a long time string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToLongTimeString(this DateTime @this, string culture)
        {
            return @this.ToString("T", new CultureInfo(culture));
        }

        /// <summary>
        /// A DateTime extension method that converts this object to a long time string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToLongTimeString(this DateTime @this, CultureInfo culture)
        {
            return @this.ToString("T", culture);
        }
        #endregion

        #region ToMonthDayString
        /// <summary>
        /// A DateTime extension method that converts this object to a month day string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToMonthDayString(this DateTime @this)
        {
            return @this.ToString("m", DateTimeFormatInfo.CurrentInfo);
        }

        /// <summary>
        /// A DateTime extension method that converts this object to a month day string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToMonthDayString(this DateTime @this, string culture)
        {
            return @this.ToString("m", new CultureInfo(culture));
        }

        /// <summary>
        /// A DateTime extension method that converts this object to a month day string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToMonthDayString(this DateTime @this, CultureInfo culture)
        {
            return @this.ToString("m", culture);
        }
        #endregion

        #region ToRFC1123String
        /// <summary>
        /// A DateTime extension method that converts this object to a rfc 1123 string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToRFC1123String(this DateTime @this)
        {
            return @this.ToString("r", DateTimeFormatInfo.CurrentInfo);
        }

        /// <summary>
        /// A DateTime extension method that converts this object to a rfc 1123 string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToRFC1123String(this DateTime @this, string culture)
        {
            return @this.ToString("r", new CultureInfo(culture));
        }

        /// <summary>
        /// A DateTime extension method that converts this object to a rfc 1123 string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToRFC1123String(this DateTime @this, CultureInfo culture)
        {
            return @this.ToString("r", culture);
        }
        #endregion

        #region ToShortDateLongTimeString

        /// <summary>
        /// A DateTime extension method that converts this object to a short date long time string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToShortDateLongTimeString(this DateTime @this)
        {
            return @this.ToString("G", DateTimeFormatInfo.CurrentInfo);
        }

        /// <summary>
        /// A DateTime extension method that converts this object to a short date long time string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToShortDateLongTimeString(this DateTime @this, string culture)
        {
            return @this.ToString("G", new CultureInfo(culture));
        }

        /// <summary>
        /// A DateTime extension method that converts this object to a short date long time string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToShortDateLongTimeString(this DateTime @this, CultureInfo culture)
        {
            return @this.ToString("G", culture);
        }
        #endregion

        #region ToShortDateString
        /// <summary>
        /// A DateTime extension method that converts this object to a short date string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToShortDateString(this DateTime @this)
        {
            return @this.ToString("d", DateTimeFormatInfo.CurrentInfo);
        }

        /// <summary>
        /// A DateTime extension method that converts this object to a short date string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToShortDateString(this DateTime @this, string culture)
        {
            return @this.ToString("d", new CultureInfo(culture));
        }

        /// <summary>
        /// A DateTime extension method that converts this object to a short date string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToShortDateString(this DateTime @this, CultureInfo culture)
        {
            return @this.ToString("d", culture);
        }
        #endregion

        #region ToShortDateTimeString
        /// <summary>
        /// A DateTime extension method that converts this object to a short date time string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToShortDateTimeString(this DateTime @this)
        {
            return @this.ToString("g", DateTimeFormatInfo.CurrentInfo);
        }

        /// <summary>
        /// A DateTime extension method that converts this object to a short date time string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToShortDateTimeString(this DateTime @this, string culture)
        {
            return @this.ToString("g", new CultureInfo(culture));
        }

        /// <summary>
        /// A DateTime extension method that converts this object to a short date time string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToShortDateTimeString(this DateTime @this, CultureInfo culture)
        {
            return @this.ToString("g", culture);
        }
        #endregion

        #region ToShortTimeString
        /// <summary>
        /// A DateTime extension method that converts this object to a short time string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToShortTimeString(this DateTime @this)
        {
            return @this.ToString("t", DateTimeFormatInfo.CurrentInfo);
        }

        /// <summary>
        /// A DateTime extension method that converts this object to a short time string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToShortTimeString(this DateTime @this, string culture)
        {
            return @this.ToString("t", new CultureInfo(culture));
        }

        /// <summary>
        /// A DateTime extension method that converts this object to a short time string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToShortTimeString(this DateTime @this, CultureInfo culture)
        {
            return @this.ToString("t", culture);
        }
        #endregion

        #region ToSortableDateTimeString
        /// <summary>
        /// A DateTime extension method that converts this object to a sortable date time string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToSortableDateTimeString(this DateTime @this)
        {
            return @this.ToString("s", DateTimeFormatInfo.CurrentInfo);
        }

        /// <summary>
        /// A DateTime extension method that converts this object to a sortable date time string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToSortableDateTimeString(this DateTime @this, string culture)
        {
            return @this.ToString("s", new CultureInfo(culture));
        }

        /// <summary>
        /// A DateTime extension method that converts this object to a sortable date time string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToSortableDateTimeString(this DateTime @this, CultureInfo culture)
        {
            return @this.ToString("s", culture);
        }
        #endregion

        #region ToUniversalSortableDateTimeString
        /// <summary>
        /// A DateTime extension method that converts this object to an universal sortable date time string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToUniversalSortableDateTimeString(this DateTime @this)
        {
            return @this.ToString("u", DateTimeFormatInfo.CurrentInfo);
        }

        /// <summary>
        /// A DateTime extension method that converts this object to an universal sortable date time string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToUniversalSortableDateTimeString(this DateTime @this, string culture)
        {
            return @this.ToString("u", new CultureInfo(culture));
        }

        /// <summary>
        /// A DateTime extension method that converts this object to an universal sortable date time string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToUniversalSortableDateTimeString(this DateTime @this, CultureInfo culture)
        {
            return @this.ToString("u", culture);
        }
        #endregion

        #region ToUniversalSortableLongDateTimeString
        /// <summary>
        /// A DateTime extension method that converts this object to an universal sortable long date time string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToUniversalSortableLongDateTimeString(this DateTime @this)
        {
            return @this.ToString("U", DateTimeFormatInfo.CurrentInfo);
        }

        /// <summary>
        /// A DateTime extension method that converts this object to an universal sortable long date time string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToUniversalSortableLongDateTimeString(this DateTime @this, string culture)
        {
            return @this.ToString("U", new CultureInfo(culture));
        }

        /// <summary>
        /// A DateTime extension method that converts this object to an universal sortable long date time string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToUniversalSortableLongDateTimeString(this DateTime @this, CultureInfo culture)
        {
            return @this.ToString("U", culture);
        }
        #endregion

        #region ToYearMonthString
        /// <summary>
        /// A DateTime extension method that converts this object to a year month string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToYearMonthString(this DateTime @this)
        {
            return @this.ToString("y", DateTimeFormatInfo.CurrentInfo);
        }

        /// <summary>
        /// A DateTime extension method that converts this object to a year month string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToYearMonthString(this DateTime @this, string culture)
        {
            return @this.ToString("y", new CultureInfo(culture));
        }

        /// <summary>
        /// A DateTime extension method that converts this object to a year month string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToYearMonthString(this DateTime @this, CultureInfo culture)
        {
            return @this.ToString("y", culture);
        }
        #endregion

        #region Age
        /// <summary>
        /// A DateTime extension method that ages the given this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>An int.</returns>
        public static int Age(this DateTime @this)
        {
            if (DateTime.Today.Month < @this.Month ||
                DateTime.Today.Month == @this.Month &&
                DateTime.Today.Day < @this.Day)
            {
                return DateTime.Today.Year - @this.Year - 1;
            }
            return DateTime.Today.Year - @this.Year;
        }
        #endregion

        #region Elapsed
        /// <summary>
        /// A DateTime extension method that elapsed the given datetime.
        /// </summary>
        /// <param name="this">The datetime to act on.</param>
        /// <returns>A TimeSpan.</returns>
        public static TimeSpan Elapsed(this DateTime @this)
        {
            return DateTime.Now - @this;
        }
        #endregion

        #region EndOfDay
        /// <summary>
        /// A DateTime extension method that return a DateTime with the time set to "23:59:59:999". The last moment of
        /// the day. Use "DateTime2" column type in sql to keep the precision.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>A DateTime of the day with the time set to "23:59:59:999".</returns>
        public static DateTime EndOfDay(this DateTime @this)
        {
            return new DateTime(@this.Year, @this.Month, @this.Day).AddDays(1).Subtract(new TimeSpan(0, 0, 0, 0, 1));
        }
        #endregion

        #region EndOfMonth
        /// <summary>
        /// A DateTime extension method that return a DateTime of the last day of the month with the time set to
        /// "23:59:59:999". The last moment of the last day of the month.  Use "DateTime2" column type in sql to keep the
        /// precision.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>A DateTime of the last day of the month with the time set to "23:59:59:999".</returns>
        public static DateTime EndOfMonth(this DateTime @this)
        {
            return new DateTime(@this.Year, @this.Month, 1).AddMonths(1).Subtract(new TimeSpan(0, 0, 0, 0, 1));
        }
        #endregion

        #region EndOfWeek
        /// <summary>
        /// A System.DateTime extension method that ends of week.
        /// </summary>
        /// <param name="this">Date/Time of the dt.</param>
        /// <param name="startDayOfWeek">(Optional) the start day of week.</param>
        /// <returns>A DateTime.</returns>
        public static DateTime EndOfWeek(this DateTime @this, DayOfWeek startDayOfWeek = DayOfWeek.Sunday)
        {
            DateTime end = @this;
            DayOfWeek endDayOfWeek = startDayOfWeek - 1;
            if (endDayOfWeek < 0)
            {
                endDayOfWeek = DayOfWeek.Saturday;
            }

            if (end.DayOfWeek != endDayOfWeek)
            {
                if (endDayOfWeek < end.DayOfWeek)
                {
                    end = end.AddDays(7 - (end.DayOfWeek - endDayOfWeek));
                }
                else
                {
                    end = end.AddDays(endDayOfWeek - end.DayOfWeek);
                }
            }

            return new DateTime(end.Year, end.Month, end.Day, 23, 59, 59, 999);
        }
        #endregion

        #region EndOfYear
        /// <summary>
        /// A DateTime extension method that return a DateTime of the last day of the year with the time set to
        /// "23:59:59:999". The last moment of the last day of the year.  Use "DateTime2" column type in sql to keep the
        /// precision.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>A DateTime of the last day of the year with the time set to "23:59:59:999".</returns>
        public static DateTime EndOfYear(this DateTime @this)
        {
            return new DateTime(@this.Year, 1, 1).AddYears(1).Subtract(new TimeSpan(0, 0, 0, 0, 1));
        }
        #endregion

        #region FirstDayOfWeek
        /// <summary>
        /// A DateTime extension method that first day of week.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>A DateTime.</returns>
        public static DateTime FirstDayOfWeek(this DateTime @this)
        {
            return new DateTime(@this.Year, @this.Month, @this.Day).AddDays(-(int)@this.DayOfWeek);
        }
        #endregion

        #region IsAfternoon
        /// <summary>
        /// A DateTime extension method that query if '@this' is afternoon.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>true if afternoon, false if not.</returns>
        public static bool IsAfternoon(this DateTime @this)
        {
            return @this.TimeOfDay >= new DateTime(2000, 1, 1, 12, 0, 0).TimeOfDay;
        }
        #endregion

        #region IsDateEqual
        /// <summary>
        /// A DateTime extension method that query if 'date' is date equal.
        /// </summary>
        /// <param name="this">The date to act on.</param>
        /// <param name="dateToCompare">Date/Time of the date to compare.</param>
        /// <returns>true if date equal, false if not.</returns>
        public static bool IsDateEqual(this DateTime @this, DateTime dateToCompare)
        {
            return @this.Date == dateToCompare.Date;
        }
        #endregion

        #region IsFuture
        /// <summary>
        /// A DateTime extension method that query if '@this' is in the future.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>true if the value is in the future, false if not.</returns>
        public static bool IsFuture(this DateTime @this)
        {
            return @this > DateTime.Now;
        }
        #endregion

        #region IsMorning
        /// <summary>
        /// A DateTime extension method that query if '@this' is morning.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>true if morning, false if not.</returns>
        public static bool IsMorning(this DateTime @this)
        {
            return @this.TimeOfDay < new DateTime(2000, 1, 1, 12, 0, 0).TimeOfDay;
        }
        #endregion

        #region IsNow
        /// <summary>
        /// A DateTime extension method that query if '@this' is now.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>true if now, false if not.</returns>
        public static bool IsNow(this DateTime @this)
        {
            return @this == DateTime.Now;
        }
        #endregion

        #region IsPast
        /// <summary>
        /// A DateTime extension method that query if '@this' is in the past.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>true if the value is in the past, false if not.</returns>
        public static bool IsPast(this DateTime @this)
        {
            return @this < DateTime.Now;
        }
        #endregion

        #region IsTimeEqual
        /// <summary>
        /// A DateTime extension method that query if 'time' is time equal.
        /// </summary>
        /// <param name="this">The time to act on.</param>
        /// <param name="timeToCompare">Date/Time of the time to compare.</param>
        /// <returns>true if time equal, false if not.</returns>
        public static bool IsTimeEqual(this DateTime @this, DateTime timeToCompare)
        {
            return @this.TimeOfDay == timeToCompare.TimeOfDay;
        }
        #endregion

        #region IsToday
        /// <summary>
        /// A DateTime extension method that query if '@this' is today.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>true if today, false if not.</returns>
        public static bool IsToday(this DateTime @this)
        {
            return @this.Date == DateTime.Today;
        }
        #endregion

        #region IsWeekDay
        /// <summary>
        /// A DateTime extension method that query if '@this' is a week day.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>true if '@this' is a week day, false if not.</returns>
        public static bool IsWeekDay(this DateTime @this)
        {
            return !(@this.DayOfWeek == DayOfWeek.Saturday || @this.DayOfWeek == DayOfWeek.Sunday);
        }
        #endregion

        #region IsWeekendDay
        /// <summary>
        /// A DateTime extension method that query if '@this' is a weekend day.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>true if '@this' is a weekend day, false if not.</returns>
        public static bool IsWeekendDay(this DateTime @this)
        {
            return @this.DayOfWeek == DayOfWeek.Saturday || @this.DayOfWeek == DayOfWeek.Sunday;
        }
        #endregion

        #region LastDayOfWeek
        /// <summary>
        /// A DateTime extension method that last day of week.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>A DateTime.</returns>
        public static DateTime LastDayOfWeek(this DateTime @this)
        {
            return new DateTime(@this.Year, @this.Month, @this.Day).AddDays(6 - (int)@this.DayOfWeek);
        }
        #endregion

        #region SetTime
        /// <summary>
        /// Sets the time of the current date with minute precision.
        /// </summary>
        /// <param name="this">The current date.</param>
        /// <param name="hour">The hour.</param>
        /// <returns>A DateTime.</returns>
        public static DateTime SetTime(this DateTime @this, int hour)
        {
            return SetTime(@this, hour, 0, 0, 0);
        }

        /// <summary>
        /// Sets the time of the current date with minute precision.
        /// </summary>
        /// <param name="this">The current date.</param>
        /// <param name="hour">The hour.</param>
        /// <param name="minute">The minute.</param>
        /// <returns>A DateTime.</returns>
        public static DateTime SetTime(this DateTime @this, int hour, int minute)
        {
            return SetTime(@this, hour, minute, 0, 0);
        }

        /// <summary>
        /// Sets the time of the current date with second precision.
        /// </summary>
        /// <param name="this">The current date.</param>
        /// <param name="hour">The hour.</param>
        /// <param name="minute">The minute.</param>
        /// <param name="second">The second.</param>
        /// <returns>A DateTime.</returns>
        public static DateTime SetTime(this DateTime @this, int hour, int minute, int second)
        {
            return SetTime(@this, hour, minute, second, 0);
        }

        /// <summary>
        /// Sets the time of the current date with millisecond precision.
        /// </summary>
        /// <param name="this">The current date.</param>
        /// <param name="hour">The hour.</param>
        /// <param name="minute">The minute.</param>
        /// <param name="second">The second.</param>
        /// <param name="millisecond">The millisecond.</param>
        /// <returns>A DateTime.</returns>
        public static DateTime SetTime(this DateTime @this, int hour, int minute, int second, int millisecond)
        {
            return new DateTime(@this.Year, @this.Month, @this.Day, hour, minute, second, millisecond);
        }
        #endregion

        #region StartOfDay
        /// <summary>
        /// A DateTime extension method that return a DateTime with the time set to "00:00:00:000". The first moment of
        /// the day.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>A DateTime of the day with the time set to "00:00:00:000".</returns>
        public static DateTime StartOfDay(this DateTime @this)
        {
            return new DateTime(@this.Year, @this.Month, @this.Day);
        }
        #endregion

        #region StartOfMonth
        /// <summary>
        /// A DateTime extension method that return a DateTime of the first day of the month with the time set to
        /// "00:00:00:000". The first moment of the first day of the month.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>A DateTime of the first day of the month with the time set to "00:00:00:000".</returns>
        public static DateTime StartOfMonth(this DateTime @this)
        {
            return new DateTime(@this.Year, @this.Month, 1);
        }
        #endregion

        #region StartOfWeek
        /// <summary>
        /// A DateTime extension method that starts of week.
        /// </summary>
        /// <param name="this">The dt to act on.</param>
        /// <param name="startDayOfWeek">(Optional) the start day of week.</param>
        /// <returns>A DateTime.</returns>
        public static DateTime StartOfWeek(this DateTime @this, DayOfWeek startDayOfWeek = DayOfWeek.Sunday)
        {
            var start = new DateTime(@this.Year, @this.Month, @this.Day);

            if (start.DayOfWeek != startDayOfWeek)
            {
                int d = startDayOfWeek - start.DayOfWeek;
                if (startDayOfWeek <= start.DayOfWeek)
                {
                    return start.AddDays(d);
                }
                return start.AddDays(-7 + d);
            }

            return start;
        }
        #endregion

        #region StartOfYear
        /// <summary>
        /// A DateTime extension method that return a DateTime of the first day of the year with the time set to
        /// "00:00:00:000". The first moment of the first day of the year.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>A DateTime of the first day of the year with the time set to "00:00:00:000".</returns>
        public static DateTime StartOfYear(this DateTime @this)
        {
            return new DateTime(@this.Year, 1, 1);
        }
        #endregion

        #region ToEpochTimeSpan
        /// <summary>
        /// A DateTime extension method that converts the @this to an epoch time span.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as a TimeSpan.</returns>
        public static TimeSpan ToEpochTimeSpan(this DateTime @this)
        {
            return @this.Subtract(new DateTime(1970, 1, 1));
        }
        #endregion

        #region Tomorrow
        /// <summary>
        /// A DateTime extension method that tomorrows the given this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>Tomorrow date at same time.</returns>
        public static DateTime Tomorrow(this DateTime @this)
        {
            return @this.AddDays(1);
        }
        #endregion

        #region Yesterday
        /// <summary>
        /// A DateTime extension method that yesterdays the given this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>Yesterday date at same time.</returns>
        public static DateTime Yesterday(this DateTime @this)
        {
            return @this.AddDays(-1);
        }
        #endregion

        #region Now
        /// <summary>
        /// 系统当前时间
        /// </summary>
        /// <param name="utc"></param>
        /// <returns></returns>
        public static DateTime Now(bool utc = false)
        {
            if (utc)
                return DateTime.UtcNow;

            return DateTime.Now;
        }

        /// <summary>
        /// 系统当前时间
        /// </summary>
        /// <param name="utc"></param>
        /// <returns></returns>
        public static DateTimeOffset OffsetNow(bool utc = false)
        {
            if (utc)
                return DateTimeOffset.UtcNow;

            return DateTimeOffset.Now;
        }
        #endregion
    }
}
