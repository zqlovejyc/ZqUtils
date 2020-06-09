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
* [Date] 2018-07-12
* [Describe] DateTimeOffset扩展类
* **************************/
namespace ZqUtils.Extensions
{
    /// <summary>
    /// DateTimeOffset扩展类
    /// </summary>
    public static class DateTimeOffsetExtensions
    {
        #region Between
        /// <summary>
        /// A T extension method that check if the value is between (exclusif) the minValue and maxValue.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <returns>true if the value is between the minValue and maxValue, otherwise false.</returns>
        public static bool Between(this DateTimeOffset @this, DateTimeOffset minValue, DateTimeOffset maxValue)
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
        public static bool In(this DateTimeOffset @this, params DateTimeOffset[] values)
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
        public static bool InRange(this DateTimeOffset @this, DateTimeOffset minValue, DateTimeOffset maxValue)
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
        public static bool NotIn(this DateTimeOffset @this, params DateTimeOffset[] values)
        {
            return Array.IndexOf(values, @this) == -1;
        }
        #endregion

        #region ConvertTime
        /// <summary>
        /// Converts a time to the time in a particular time zone.
        /// </summary>
        /// <param name="this">The date and time to convert.</param>
        /// <param name="destinationTimeZone">The time zone to convert  to.</param>
        /// <returns>The date and time in the destination time zone.</returns>
        public static DateTimeOffset ConvertTime(this DateTimeOffset @this, TimeZoneInfo destinationTimeZone)
        {
            return TimeZoneInfo.ConvertTime(@this, destinationTimeZone);
        }
        #endregion

        #region ConvertTimeBySystemTimeZoneId
        /// <summary>
        /// Converts a time to the time in another time zone based on the time zone&#39;s identifier.
        /// </summary>
        /// <param name="this">The date and time to convert.</param>
        /// <param name="destinationTimeZoneId">The identifier of the destination time zone.</param>
        /// <returns>The date and time in the destination time zone.</returns>
        public static DateTimeOffset ConvertTimeBySystemTimeZoneId(this DateTimeOffset @this, string destinationTimeZoneId)
        {
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(@this, destinationTimeZoneId);
        }
        #endregion

        #region SetTime
        /// <summary>
        /// Sets the time of the current date with minute precision.
        /// </summary>
        /// <param name="this">The current date.</param>
        /// <param name="hour">The hour.</param>
        /// <returns>A DateTimeOffset.</returns>
        public static DateTimeOffset SetTime(this DateTimeOffset @this, int hour)
        {
            return SetTime(@this, hour, 0, 0, 0);
        }

        /// <summary>
        /// Sets the time of the current date with minute precision.
        /// </summary>
        /// <param name="this">The current date.</param>
        /// <param name="hour">The hour.</param>
        /// <param name="minute">The minute.</param>
        /// <returns>A DateTimeOffset.</returns>
        public static DateTimeOffset SetTime(this DateTimeOffset @this, int hour, int minute)
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
        /// <returns>A DateTimeOffset.</returns>
        public static DateTimeOffset SetTime(this DateTimeOffset @this, int hour, int minute, int second)
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
        /// <returns>A DateTimeOffset.</returns>
        public static DateTimeOffset SetTime(this DateTimeOffset @this, int hour, int minute, int second, int millisecond)
        {
            return new DateTime(@this.Year, @this.Month, @this.Day, hour, minute, second, millisecond);
        }
        #endregion
    }
}
