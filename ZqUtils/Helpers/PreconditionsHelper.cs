﻿#region License
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
using System.Linq;
using System.Collections.Generic;
/****************************
* [Author] 张强
* [Date] 2019-02-28
* [Describe] 参数条件判断工具类
* **************************/
namespace ZqUtils.Helpers
{
    /// <summary>
    /// Collection of precondition methods for qualifying method arguments.
    /// </summary>
    public class PreconditionsHelper
    {
        /// <summary>
        /// Ensures that <paramref name="value"/> is not null.
        /// </summary>
        /// <param name="value">
        /// The value to check, must not be null.
        /// </param>
        /// <param name="name">
        /// The name of the parameter the value is taken from, must not be
        /// blank.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="value"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="name"/> is blank.
        /// </exception>
        public static void CheckNotNull<T>(T value, string name)
        {
            CheckNotNull(value, name, string.Format("{0} must not be null", name));
        }

        /// <summary>
        /// Ensures that <paramref name="value"/> is not null.
        /// </summary>
        /// <param name="value">
        /// The value to check, must not be null.
        /// </param>
        /// <param name="name">
        /// The name of the parameter the value is taken from, must not be
        /// blank.
        /// </param>
        /// <param name="message">
        /// The message to provide to the exception if <paramref name="value"/>
        /// is null, must not be blank.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="value"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="name"/> or <paramref name="message"/> are
        /// blank.
        /// </exception>
        public static void CheckNotNull<T>(T value, string name, string message)
        {
            if (value == null)
            {
                CheckNotBlank(name, "name", "name must not be blank");
                CheckNotBlank(message, "message", "message must not be blank");

                throw new ArgumentNullException(name, message);
            }
        }

        /// <summary>
        /// Ensures that <paramref name="value"/> is not blank.
        /// </summary>
        /// <param name="value">
        /// The value to check, must not be blank.
        /// </param>
        /// <param name="name">
        /// The name of the parameter the value is taken from, must not be
        /// blank.
        /// </param>
        /// <param name="message">
        /// The message to provide to the exception if <paramref name="value"/>
        /// is blank, must not be blank.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="value"/>, <paramref name="name"/>, or
        /// <paramref name="message"/> are blank.
        /// </exception>
        public static void CheckNotBlank(string value, string name, string message)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("name must not be blank", "name");
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("message must not be blank", "message");
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(message, name);
            }
        }

        /// <summary>
        /// Ensures that <paramref name="value"/> is not blank.
        /// </summary>
        /// <param name="value">
        /// The value to check, must not be blank.
        /// </param>
        /// <param name="name">
        /// The name of the parameter the value is taken from, must not be
        /// blank.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="value"/> or <paramref name="name"/> are
        /// blank.
        /// </exception>
        public static void CheckNotBlank(string value, string name)
        {
            CheckNotBlank(value, name, string.Format("{0} must not be blank", name));
        }

        /// <summary>
        /// Ensures that <paramref name="collection"/> contains at least one
        /// item.
        /// </summary>
        /// <param name="collection">
        /// The collection to check, must not be null or empty.
        /// </param>
        /// <param name="name">
        /// The name of the parameter the collection is taken from, must not be
        /// blank.
        /// </param>
        /// <param name="message">
        /// The message to provide to the exception if <paramref name="collection"/>
        /// is empty, must not be blank.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="collection"/> is empty, or if
        /// <paramref name="message"/> or <paramref name="name"/> are blank.
        /// </exception>
        public static void CheckAny<T>(IEnumerable<T> collection, string name, string message)
        {
            if (collection == null || !collection.Any())
            {
                CheckNotBlank(name, "name", "name must not be blank");
                CheckNotBlank(message, "message", "message must not be blank");

                throw new ArgumentException(message, name);
            }
        }

        /// <summary>
        /// Ensures that <paramref name="value"/> is true.
        /// </summary>
        /// <param name="value">
        /// The value to check, must be true.
        /// </param>
        /// <param name="name">
        /// The name of the parameter the value is taken from, must not be
        /// blank.
        /// </param>
        /// <param name="message">
        /// The message to provide to the exception if <paramref name="value"/>
        /// is false, must not be blank.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="value"/> is false, or if <paramref name="name"/>
        /// or <paramref name="message"/> are blank.
        /// </exception>
        public static void CheckTrue(bool value, string name, string message)
        {
            if (!value)
            {
                CheckNotBlank(name, "name", "name must not be blank");
                CheckNotBlank(message, "message", "message must not be blank");

                throw new ArgumentException(message, name);
            }
        }

        /// <summary>
        /// Ensures that <paramref name="value"/> is false.
        /// </summary>
        /// <param name="value">
        /// The value to check, must be false.
        /// </param>
        /// <param name="name">
        /// The name of the parameter the value is taken from, must not be
        /// blank.
        /// </param>
        /// <param name="message">
        /// The message to provide to the exception if <paramref name="value"/>
        /// is true, must not be blank.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="value"/> is true, or if <paramref name="name"/>
        /// or <paramref name="message"/> are blank.
        /// </exception>
        public static void CheckFalse(bool value, string name, string message)
        {
            if (value)
            {
                CheckNotBlank(name, "name", "name must not be blank");
                CheckNotBlank(message, "message", "message must not be blank");

                throw new ArgumentException(message, name);
            }
        }

        /// <summary>
        /// Ensures  that <paramref name="value"/> is less than or equal to 255 characters.
        /// </summary>
        /// <param name="value">
        /// The value to check, must be than or equal to 255 characters.
        /// </param>
        /// <param name="name">
        /// The name of the parameter the value is taken from, must not be blank.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Throw if <paramref name="name"/> is blank.
        /// </exception>
        public static void CheckShortString(string value, string name)
        {
            CheckNotNull(value, name);
            if (value.Length > 255)
            {
                throw new ArgumentException(string.Format("Argument '{0}' must be less than or equal to 255 characters.", name));
            }
        }

        /// <summary>
        /// Ensures  that <paramref name="value"/> is type <paramref name="expectedType"/>.
        /// </summary>
        /// <param name="expectedType">
        /// The expectedType
        /// </param>
        /// <param name="value">
        /// The value to check
        /// </param>
        /// <param name="name">
        /// The name must not be blank.
        /// </param>
        /// <param name="message">
        /// The message to provide to the exception if <paramref name="value"/>
        /// is not type of <paramref name="expectedType"/>.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Throw if <paramref name="name"/> is blank.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Throw if <paramref name="message"/> is blank.
        /// </exception>
        public static void CheckTypeMatches(Type expectedType, object value, string name, string message)
        {
            bool assignable = expectedType.IsAssignableFrom(value.GetType());
            if (!assignable)
            {
                CheckNotBlank(name, "name", "name must not be blank");
                CheckNotBlank(message, "message", "message must not be blank");

                throw new ArgumentException(message, name);
            }
        }

        /// <summary>
        /// Ensures that <paramref name="value"/> is less than <paramref name="maxValue"/>.
        /// </summary>
        /// <param name="value">
        /// The value to check, must be less than <paramref name="maxValue"/>.
        /// </param>
        /// <param name="maxValue">
        /// The maxValue is to be compared
        /// </param>
        /// <param name="name">
        /// The name of the parameter the value is taken from, must not be blank.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Throw if <paramref name="name"/> is blank.
        /// </exception>
        public static void CheckLess(TimeSpan value, TimeSpan maxValue, string name)
        {
            if (value < maxValue)
                return;
            throw new ArgumentOutOfRangeException(name, string.Format("Arguments {0} must be less than maxValue", name));
        }

        /// <summary>
        /// Ensures that <paramref name="value"/> is null.
        /// </summary>
        /// <param name="value">
        /// The value to check, must be null.
        /// </param>
        /// <param name="name">
        /// The name of the parameter the value is taken from, must not be blank.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="value"/> is not null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Throw if <paramref name="name"/> is blank.
        /// </exception>
        public static void CheckNull<T>(T value, string name)
        {
            if (value == null)
                return;
            throw new ArgumentException(string.Format("{0} must be null", name), name);
        }
    }
}