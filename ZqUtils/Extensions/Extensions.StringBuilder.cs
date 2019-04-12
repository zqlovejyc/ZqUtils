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
using System.Collections.Generic;
using System.Text;
/****************************
* [Author] 张强
* [Date] 2018-05-03
* [Describe] StringBuilder扩展类
* **************************/
namespace ZqUtils.Extensions
{
    /// <summary>
    /// StringBuilder扩展类
    /// </summary>
    public static partial class Extensions
    {
        #region AppendWhereOrAnd
        /// <summary>
        /// sql拼接where或者and
        /// </summary>
        /// <param name="this">当前sql拼接对象</param>
        /// <param name="hasWhere">是否有where</param>
        /// <param name="appendSql">拼接sql字符串</param>
        /// <param name="sqlKeywordOfAnd">sql关键字and</param>
        /// <param name="sqlKeywordOfWhere">sql关键字where</param>
        /// <param name="appendStringBuilder">拼接StringBuilder对象</param>
        /// <returns>bool</returns>
        public static bool AppendWhereOrAnd(this StringBuilder @this, bool hasWhere, string appendSql = null, string sqlKeywordOfAnd = " AND ", string sqlKeywordOfWhere = " WHERE ", StringBuilder appendStringBuilder = null)
        {
            if (hasWhere)
            {
                @this.Append(sqlKeywordOfAnd);
            }
            else
            {
                @this.Append(sqlKeywordOfWhere);
                hasWhere = true;
            }
            if (!string.IsNullOrEmpty(appendSql))
            {
                @this.Append(appendSql);
            }
            if (appendStringBuilder != null)
            {
                @this.Append(appendStringBuilder);
            }
            return hasWhere;
        }
        #endregion

        #region Separate
        /// <summary>
        /// 追加分隔符字符串，忽略开头，常用于拼接
        /// </summary>
        /// <param name="this">字符串构造者</param>
        /// <param name="separator">分隔符</param>
        /// <returns></returns>
        public static StringBuilder Separate(this StringBuilder @this, string separator)
        {
            if (@this == null || string.IsNullOrEmpty(separator)) return @this;
            if (@this.Length > 0) @this.Append(separator);
            return @this;
        }
        #endregion

        #region ExtractChar
        /// <summary>
        /// A StringBuilder extension method that extracts the character described by @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The extracted character.</returns>
        public static char ExtractChar(this StringBuilder @this)
        {
            return @this.ExtractChar(0);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the character described by @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="endIndex">[out] The end index.</param>
        /// <returns>The extracted character.</returns>
        public static char ExtractChar(this StringBuilder @this, out int endIndex)
        {
            return @this.ExtractChar(0, out endIndex);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the character described by @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="startIndex">The start index.</param>
        /// <returns>The extracted character.</returns>
        public static char ExtractChar(this StringBuilder @this, int startIndex)
        {
            return @this.ExtractChar(startIndex, out int endIndex);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the character described by @this.
        /// </summary>
        /// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
        /// <param name="this">The @this to act on.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">[out] The end index.</param>
        /// <returns>The extracted character.</returns>
        public static char ExtractChar(this StringBuilder @this, int startIndex, out int endIndex)
        {
            if (@this.Length > startIndex + 1)
            {
                var ch1 = @this[startIndex];
                var ch2 = @this[startIndex + 1];
                var ch3 = @this[startIndex + 2];

                if (ch1 == '\'' && ch3 == '\'')
                {
                    endIndex = startIndex + 2;
                    return ch2;
                }
            }

            throw new Exception("Invalid char at position: " + startIndex);
        }
        #endregion

        #region ExtractComment
        /// <summary>
        /// A StringBuilder extension method that extracts the comment described by @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The extracted comment.</returns>
        public static StringBuilder ExtractComment(this StringBuilder @this)
        {
            return @this.ExtractComment(0);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the comment described by @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="endIndex">[out] The end index.</param>
        /// <returns>The extracted comment.</returns>
        public static StringBuilder ExtractComment(this StringBuilder @this, out int endIndex)
        {
            return @this.ExtractComment(0, out endIndex);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the comment described by @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="startIndex">The start index.</param>
        /// <returns>The extracted comment.</returns>
        public static StringBuilder ExtractComment(this StringBuilder @this, int startIndex)
        {
            return @this.ExtractComment(startIndex, out int endIndex);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the comment described by @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">[out] The end index.</param>
        /// <returns>The extracted comment.</returns>
        public static StringBuilder ExtractComment(this StringBuilder @this, int startIndex, out int endIndex)
        {
            if (@this.Length > startIndex + 1)
            {
                var ch1 = @this[startIndex];
                var ch2 = @this[startIndex + 1];

                if (ch1 == '/' && ch2 == '/')
                {
                    // Single line comment

                    return @this.ExtractCommentSingleLine(startIndex, out endIndex);
                }

                if (ch1 == '/' && ch2 == '*')
                {
                    /*
                     * Multi-line comment
                     */

                    return @this.ExtractCommentMultiLine(startIndex, out endIndex);
                }
            }

            endIndex = -1;
            return null;
        }
        #endregion

        #region ExtractCommentMultiLine
        /// <summary>
        /// A StringBuilder extension method that extracts the comment multi line described by
        /// @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The extracted comment multi line.</returns>
        public static StringBuilder ExtractCommentMultiLine(this StringBuilder @this)
        {
            return @this.ExtractCommentMultiLine(0);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the comment multi line described by
        /// @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="endIndex">[out] The end index.</param>
        /// <returns>The extracted comment multi line.</returns>
        public static StringBuilder ExtractCommentMultiLine(this StringBuilder @this, out int endIndex)
        {
            return @this.ExtractCommentMultiLine(0, out endIndex);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the comment multi line described by
        /// @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="startIndex">The start index.</param>
        /// <returns>The extracted comment multi line.</returns>
        public static StringBuilder ExtractCommentMultiLine(this StringBuilder @this, int startIndex)
        {
            return @this.ExtractCommentMultiLine(startIndex, out int endIndex);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the comment multi line described by
        /// @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">[out] The end index.</param>
        /// <returns>The extracted comment multi line.</returns>
        public static StringBuilder ExtractCommentMultiLine(this StringBuilder @this, int startIndex, out int endIndex)
        {
            var sb = new StringBuilder();

            if (@this.Length > startIndex + 1)
            {
                var ch1 = @this[startIndex];
                var ch2 = @this[startIndex + 1];

                if (ch1 == '/' && ch2 == '*')
                {
                    /*
                     * Multi-line comment
                     */

                    sb.Append(ch1);
                    sb.Append(ch2);
                    var pos = startIndex + 2;

                    while (pos < @this.Length)
                    {
                        var ch = @this[pos];
                        pos++;

                        if (ch == '*' && pos < @this.Length && @this[pos] == '/')
                        {
                            sb.Append(ch);
                            sb.Append(@this[pos]);
                            endIndex = pos;
                            return sb;
                        }

                        sb.Append(ch);
                    }

                    endIndex = pos;
                    return sb;
                }
            }

            endIndex = -1;
            return null;
        }
        #endregion

        #region ExtractCommentSingleLine
        /// <summary>
        /// A StringBuilder extension method that extracts the comment single line described by
        /// @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The extracted comment single line.</returns>
        public static StringBuilder ExtractCommentSingleLine(this StringBuilder @this)
        {
            return @this.ExtractCommentSingleLine(0);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the comment single line described by
        /// @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="endIndex">[out] The end index.</param>
        /// <returns>The extracted comment single line.</returns>
        public static StringBuilder ExtractCommentSingleLine(this StringBuilder @this, out int endIndex)
        {
            return @this.ExtractCommentSingleLine(0, out endIndex);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the comment single line described by
        /// @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="startIndex">The start index.</param>
        /// <returns>The extracted comment single line.</returns>
        public static StringBuilder ExtractCommentSingleLine(this StringBuilder @this, int startIndex)
        {
            return @this.ExtractCommentSingleLine(startIndex, out int endIndex);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the comment single line described by
        /// @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">[out] The end index.</param>
        /// <returns>The extracted comment single line.</returns>
        public static StringBuilder ExtractCommentSingleLine(this StringBuilder @this, int startIndex, out int endIndex)
        {
            var sb = new StringBuilder();

            if (@this.Length > startIndex + 1)
            {
                var ch1 = @this[startIndex];
                var ch2 = @this[startIndex + 1];

                if (ch1 == '/' && ch2 == '/')
                {
                    // Single line comment

                    sb.Append(ch1);
                    sb.Append(ch2);
                    var pos = startIndex + 2;

                    while (pos < @this.Length)
                    {
                        var ch = @this[pos];
                        pos++;

                        if (ch == '\r' && pos < @this.Length && @this[pos] == '\n')
                        {
                            endIndex = pos - 1;
                            return sb;
                        }

                        sb.Append(ch);
                    }

                    endIndex = pos;
                    return sb;
                }
            }

            endIndex = -1;
            return null;
        }
        #endregion

        #region ExtractHexadecimal
        /// <summary>
        /// A StringBuilder extension method that extracts the hexadecimal described by @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The extracted hexadecimal.</returns>
        public static StringBuilder ExtractHexadecimal(this StringBuilder @this)
        {
            return @this.ExtractHexadecimal(0);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the hexadecimal described by @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="endIndex">[out] The end index.</param>
        /// <returns>The extracted hexadecimal.</returns>
        public static StringBuilder ExtractHexadecimal(this StringBuilder @this, out int endIndex)
        {
            return @this.ExtractHexadecimal(0, out endIndex);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the hexadecimal described by @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="startIndex">The start index.</param>
        /// <returns>The extracted hexadecimal.</returns>
        public static StringBuilder ExtractHexadecimal(this StringBuilder @this, int startIndex)
        {
            return @this.ExtractHexadecimal(startIndex, out int endIndex);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the hexadecimal described by @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">[out] The end index.</param>
        /// <returns>The extracted hexadecimal.</returns>
        public static StringBuilder ExtractHexadecimal(this StringBuilder @this, int startIndex, out int endIndex)
        {
            // WARNING: This method support all kind of suffix for .NET Runtime Compiler
            // An operator can be any sequence of supported operator character

            if (startIndex + 1 < @this.Length && @this[startIndex] == '0'
                && (@this[startIndex + 1] == 'x' || @this[startIndex + 1] == 'X'))
            {
                var sb = new StringBuilder();

                var hasNumber = false;
                var hasSuffix = false;

                sb.Append(@this[startIndex]);
                sb.Append(@this[startIndex + 1]);

                var pos = startIndex + 2;

                while (pos < @this.Length)
                {
                    var ch = @this[pos];
                    pos++;

                    if (((ch >= '0' && ch <= '9')
                         || (ch >= 'a' && ch <= 'f')
                         || (ch >= 'A' && ch <= 'F'))
                        && !hasSuffix)
                    {
                        hasNumber = true;
                        sb.Append(ch);
                    }
                    else if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z'))
                    {
                        hasSuffix = true;
                        sb.Append(ch);
                    }
                    else
                    {
                        pos -= 2;
                        break;
                    }
                }

                if (hasNumber)
                {
                    endIndex = pos;
                    return sb;
                }
            }

            endIndex = -1;
            return null;
        }
        #endregion

        #region ExtractKeyword
        /// <summary>
        /// A StringBuilder extension method that extracts the keyword described by @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The extracted keyword.</returns>
        public static StringBuilder ExtractKeyword(this StringBuilder @this)
        {
            return @this.ExtractKeyword(0);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the keyword described by @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="endIndex">[out] The end index.</param>
        /// <returns>The extracted keyword.</returns>
        public static StringBuilder ExtractKeyword(this StringBuilder @this, out int endIndex)
        {
            return @this.ExtractKeyword(0, out endIndex);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the keyword described by @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="startIndex">The start index.</param>
        /// <returns>The extracted keyword.</returns>
        public static StringBuilder ExtractKeyword(this StringBuilder @this, int startIndex)
        {
            return @this.ExtractKeyword(startIndex, out int endIndex);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the keyword described by @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">[out] The end index.</param>
        /// <returns>The extracted keyword.</returns>
        public static StringBuilder ExtractKeyword(this StringBuilder @this, int startIndex, out int endIndex)
        {
            // WARNING: This method support custom operator for .NET Runtime Compiler
            // An operator can be any sequence of supported operator character
            var sb = new StringBuilder();

            var pos = startIndex;
            var hasCharacter = false;

            while (pos < @this.Length)
            {
                var ch = @this[pos];
                pos++;

                if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z'))
                {
                    hasCharacter = true;
                    sb.Append(ch);
                }
                else if (ch == '@')
                {
                    sb.Append(ch);
                }
                else if (ch >= '0' && ch <= '9' && hasCharacter)
                {
                    sb.Append(ch);
                }
                else
                {
                    pos -= 2;
                    break;
                }
            }

            if (hasCharacter)
            {
                endIndex = pos;
                return sb;
            }

            endIndex = -1;
            return null;
        }
        #endregion

        #region ExtractNumber
        /// <summary>
        /// A StringBuilder extension method that extracts the number described by @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The extracted number.</returns>
        public static StringBuilder ExtractNumber(this StringBuilder @this)
        {
            return @this.ExtractNumber(0);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the number described by @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="endIndex">[out] The end index.</param>
        /// <returns>The extracted number.</returns>
        public static StringBuilder ExtractNumber(this StringBuilder @this, out int endIndex)
        {
            return @this.ExtractNumber(0, out endIndex);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the number described by @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="startIndex">The start index.</param>
        /// <returns>The extracted number.</returns>
        public static StringBuilder ExtractNumber(this StringBuilder @this, int startIndex)
        {
            return @this.ExtractNumber(startIndex, out int endIndex);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the number described by @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">[out] The end index.</param>
        /// <returns>The extracted number.</returns>
        public static StringBuilder ExtractNumber(this StringBuilder @this, int startIndex, out int endIndex)
        {
            // WARNING: This method support all kind of suffix for .NET Runtime Compiler
            // An operator can be any sequence of supported operator character
            var sb = new StringBuilder();

            var hasNumber = false;
            var hasDot = false;
            var hasSuffix = false;

            var pos = startIndex;

            while (pos < @this.Length)
            {
                var ch = @this[pos];
                pos++;

                if (ch >= '0' && ch <= '9' && !hasSuffix)
                {
                    hasNumber = true;
                    sb.Append(ch);
                }
                else if (ch == '.' && !hasSuffix && !hasDot)
                {
                    hasDot = true;
                    sb.Append(ch);
                }
                else if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z'))
                {
                    hasSuffix = true;
                    sb.Append(ch);
                }
                else
                {
                    pos -= 2;
                    break;
                }
            }

            if (hasNumber)
            {
                endIndex = pos;
                return sb;
            }

            endIndex = -1;
            return null;
        }
        #endregion

        #region ExtractOperator
        /// <summary>
        /// A StringBuilder extension method that extracts the operator described by @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The extracted operator.</returns>
        public static StringBuilder ExtractOperator(this StringBuilder @this)
        {
            return @this.ExtractOperator(0);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the operator described by @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="endIndex">[out] The end index.</param>
        /// <returns>The extracted operator.</returns>
        public static StringBuilder ExtractOperator(this StringBuilder @this, out int endIndex)
        {
            return @this.ExtractOperator(0, out endIndex);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the operator described by @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="startIndex">The start index.</param>
        /// <returns>The extracted operator.</returns>
        public static StringBuilder ExtractOperator(this StringBuilder @this, int startIndex)
        {
            int endIndex;
            return @this.ExtractOperator(startIndex, out endIndex);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the operator described by @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">[out] The end index.</param>
        /// <returns>The extracted operator.</returns>
        public static StringBuilder ExtractOperator(this StringBuilder @this, int startIndex, out int endIndex)
        {
            // WARNING: This method support custom operator for .NET Runtime Compiler
            // An operator can be any sequence of supported operator character
            var sb = new StringBuilder();

            var pos = startIndex;

            while (pos < @this.Length)
            {
                var ch = @this[pos];
                pos++;

                switch (ch)
                {
                    case '`':
                    case '~':
                    case '!':
                    case '#':
                    case '$':
                    case '%':
                    case '^':
                    case '&':
                    case '*':
                    case '(':
                    case ')':
                    case '-':
                    case '_':
                    case '=':
                    case '+':
                    case '[':
                    case ']':
                    case '{':
                    case '}':
                    case '|':
                    case ':':
                    case ';':
                    case ',':
                    case '.':
                    case '<':
                    case '>':
                    case '?':
                    case '/':
                        sb.Append(ch);
                        break;
                    default:
                        if (sb.Length > 0)
                        {
                            endIndex = pos - 2;
                            return sb;
                        }

                        endIndex = -1;
                        return null;
                }
            }

            if (sb.Length > 0)
            {
                endIndex = pos;
                return sb;
            }

            endIndex = -1;
            return null;
        }
        #endregion

        #region ExtractString
        /// <summary>
        /// A StringBuilder extension method that extracts the string described by @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The extracted string.</returns>
        public static StringBuilder ExtractString(this StringBuilder @this)
        {
            return @this.ExtractString(0);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the string described by @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="endIndex">[out] The end index.</param>
        /// <returns>The extracted string.</returns>
        public static StringBuilder ExtractString(this StringBuilder @this, out int endIndex)
        {
            return @this.ExtractString(0, out endIndex);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the string described by @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="startIndex">The start index.</param>
        /// <returns>The extracted string.</returns>
        public static StringBuilder ExtractString(this StringBuilder @this, int startIndex)
        {
            return @this.ExtractString(startIndex, out int endIndex);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the string described by @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">[out] The end index.</param>
        /// <returns>The extracted string.</returns>
        public static StringBuilder ExtractString(this StringBuilder @this, int startIndex, out int endIndex)
        {
            if (@this.Length > startIndex + 1)
            {
                var ch1 = @this[startIndex];
                var ch2 = @this[startIndex + 1];

                if (ch1 == '@' && ch2 == '"')
                {
                    // @"my string"

                    return @this.ExtractStringArobasDoubleQuote(startIndex, out endIndex);
                }

                if (ch1 == '@' && ch2 == '\'')
                {
                    // WARNING: This is not a valid string, however single quote is often used to make it more readable in text templating
                    // @'my string'

                    return @this.ExtractStringArobasSingleQuote(startIndex, out endIndex);
                }

                if (ch1 == '"')
                {
                    // "my string"

                    return @this.ExtractStringDoubleQuote(startIndex, out endIndex);
                }

                if (ch1 == '\'')
                {
                    // WARNING: This is not a valid string, however single quote is often used to make it more readable in text templating
                    // 'my string'

                    return @this.ExtractStringSingleQuote(startIndex, out endIndex);
                }
            }

            endIndex = -1;
            return null;
        }
        #endregion

        #region ExtractStringArobasDoubleQuote
        /// <summary>
        /// A StringBuilder extension method that extracts the string arobas double quote
        /// described by @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The extracted string arobas double quote.</returns>
        public static StringBuilder ExtractStringArobasDoubleQuote(this StringBuilder @this)
        {
            return @this.ExtractStringArobasDoubleQuote(0);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the string arobas double quote
        /// described by @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="endIndex">[out] The end index.</param>
        /// <returns>The extracted string arobas double quote.</returns>
        public static StringBuilder ExtractStringArobasDoubleQuote(this StringBuilder @this, out int endIndex)
        {
            return @this.ExtractStringArobasDoubleQuote(0, out endIndex);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the string arobas double quote
        /// described by @this.
        /// </summary>
        /// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
        /// <param name="this">The @this to act on.</param>
        /// <param name="startIndex">The start index.</param>
        /// <returns>The extracted string arobas double quote.</returns>
        public static StringBuilder ExtractStringArobasDoubleQuote(this StringBuilder @this, int startIndex)
        {
            return @this.ExtractStringArobasDoubleQuote(startIndex, out int endIndex);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the string arobas double quote
        /// described by @this.
        /// </summary>
        /// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
        /// <param name="this">The @this to act on.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">[out] The end index.</param>
        /// <returns>The extracted string arobas double quote.</returns>
        public static StringBuilder ExtractStringArobasDoubleQuote(this StringBuilder @this, int startIndex, out int endIndex)
        {
            var sb = new StringBuilder();

            if (@this.Length > startIndex + 1)
            {
                var ch1 = @this[startIndex];
                var ch2 = @this[startIndex + 1];

                if (ch1 == '@' && ch2 == '"')
                {
                    // @"my string"

                    var pos = startIndex + 2;

                    while (pos < @this.Length)
                    {
                        var ch = @this[pos];
                        pos++;

                        if (ch == '"' && pos < @this.Length && @this[pos] == '"')
                        {
                            sb.Append(ch);
                            pos++; // Treat as escape character for @"abc""def"
                        }
                        else if (ch == '"')
                        {
                            endIndex = pos;
                            return sb;
                        }
                        else
                        {
                            sb.Append(ch);
                        }
                    }

                    throw new Exception("Unclosed string starting at position: " + startIndex);
                }
            }

            endIndex = -1;
            return null;
        }
        #endregion

        #region ExtractStringArobasSingleQuote
        /// <summary>
        /// A StringBuilder extension method that extracts the string arobas single quote
        /// described by @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The extracted string arobas single quote.</returns>
        public static StringBuilder ExtractStringArobasSingleQuote(this StringBuilder @this)
        {
            return @this.ExtractStringArobasSingleQuote(0);
        }
        /// <summary>
        /// A StringBuilder extension method that extracts the string arobas single quote
        /// described by @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="endIndex">[out] The end index.</param>
        /// <returns>The extracted string arobas single quote.</returns>
        public static StringBuilder ExtractStringArobasSingleQuote(this StringBuilder @this, out int endIndex)
        {
            return @this.ExtractStringArobasSingleQuote(0, out endIndex);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the string arobas single quote
        /// described by @this.
        /// </summary>
        /// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
        /// <param name="this">The @this to act on.</param>
        /// <param name="startIndex">The start index.</param>
        /// <returns>The extracted string arobas single quote.</returns>
        public static StringBuilder ExtractStringArobasSingleQuote(this StringBuilder @this, int startIndex)
        {
            return @this.ExtractStringArobasSingleQuote(startIndex, out int endIndex);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the string arobas single quote
        /// described by @this.
        /// </summary>
        /// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
        /// <param name="this">The @this to act on.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">[out] The end index.</param>
        /// <returns>The extracted string arobas single quote.</returns>
        public static StringBuilder ExtractStringArobasSingleQuote(this StringBuilder @this, int startIndex, out int endIndex)
        {
            var sb = new StringBuilder();

            if (@this.Length > startIndex + 1)
            {
                var ch1 = @this[startIndex];
                var ch2 = @this[startIndex + 1];

                if (ch1 == '@' && ch2 == '\'')
                {
                    // WARNING: This is not a valid string, however single quote is often used to make it more readable in text templating
                    // @'my string'

                    var pos = startIndex + 2;

                    while (pos < @this.Length)
                    {
                        var ch = @this[pos];
                        pos++;

                        if (ch == '\'' && pos < @this.Length && @this[pos] == '\'')
                        {
                            sb.Append(ch);
                            pos++; // Treat as escape character for @'abc''def'
                        }
                        else if (ch == '\'')
                        {
                            endIndex = pos;
                            return sb;
                        }
                        else
                        {
                            sb.Append(ch);
                        }
                    }

                    throw new Exception("Unclosed string starting at position: " + startIndex);
                }
            }

            endIndex = -1;
            return null;
        }
        #endregion

        #region ExtractStringDoubleQuote
        /// <summary>
        /// A StringBuilder extension method that extracts the string double quote described by
        /// @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The extracted string double quote.</returns>
        public static StringBuilder ExtractStringDoubleQuote(this StringBuilder @this)
        {
            return @this.ExtractStringDoubleQuote(0);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the string double quote described by
        /// @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="endIndex">[out] The end index.</param>
        /// <returns>The extracted string double quote.</returns>
        public static StringBuilder ExtractStringDoubleQuote(this StringBuilder @this, out int endIndex)
        {
            return @this.ExtractStringDoubleQuote(0, out endIndex);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the string double quote described by
        /// @this.
        /// </summary>
        /// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
        /// <param name="this">The @this to act on.</param>
        /// <param name="startIndex">The start index.</param>
        /// <returns>The extracted string double quote.</returns>
        public static StringBuilder ExtractStringDoubleQuote(this StringBuilder @this, int startIndex)
        {
            return @this.ExtractStringDoubleQuote(startIndex, out int endIndex);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the string double quote described by
        /// @this.
        /// </summary>
        /// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
        /// <param name="this">The @this to act on.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">[out] The end index.</param>
        /// <returns>The extracted string double quote.</returns>
        public static StringBuilder ExtractStringDoubleQuote(this StringBuilder @this, int startIndex, out int endIndex)
        {
            var sb = new StringBuilder();

            if (@this.Length > startIndex + 1)
            {
                var ch1 = @this[startIndex];

                if (ch1 == '"')
                {
                    // "my string"

                    var pos = startIndex + 1;

                    while (pos < @this.Length)
                    {
                        var ch = @this[pos];
                        pos++;

                        char nextChar;
                        if (ch == '\\' && pos < @this.Length && ((nextChar = @this[pos]) == '\\' || nextChar == '"'))
                        {
                            sb.Append(nextChar);
                            pos++; // Treat as escape character for \\ or \"
                        }
                        else if (ch == '"')
                        {
                            endIndex = pos;
                            return sb;
                        }
                        else
                        {
                            sb.Append(ch);
                        }
                    }

                    throw new Exception("Unclosed string starting at position: " + startIndex);
                }
            }

            endIndex = -1;
            return null;
        }
        #endregion

        #region ExtractStringSingleQuote
        /// <summary>
        /// A StringBuilder extension method that extracts the string single quote described by
        /// @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The extracted string single quote.</returns>
        public static StringBuilder ExtractStringSingleQuote(this StringBuilder @this)
        {
            return @this.ExtractStringSingleQuote(0);
        }
        /// <summary>
        /// A StringBuilder extension method that extracts the string single quote described by
        /// @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="endIndex">[out] The end index.</param>
        /// <returns>The extracted string single quote.</returns>
        public static StringBuilder ExtractStringSingleQuote(this StringBuilder @this, out int endIndex)
        {
            return @this.ExtractStringSingleQuote(0, out endIndex);
        }


        /// <summary>
        /// A StringBuilder extension method that extracts the string single quote described by
        /// @this.
        /// </summary>
        /// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
        /// <param name="this">The @this to act on.</param>
        /// <param name="startIndex">The start index.</param>
        /// <returns>The extracted string single quote.</returns>
        public static StringBuilder ExtractStringSingleQuote(this StringBuilder @this, int startIndex)
        {
            return @this.ExtractStringSingleQuote(startIndex, out int endIndex);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the string single quote described by
        /// @this.
        /// </summary>
        /// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
        /// <param name="this">The @this to act on.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">[out] The end index.</param>
        /// <returns>The extracted string single quote.</returns>
        public static StringBuilder ExtractStringSingleQuote(this StringBuilder @this, int startIndex, out int endIndex)
        {
            var sb = new StringBuilder();

            if (@this.Length > startIndex + 1)
            {
                var ch1 = @this[startIndex];

                if (ch1 == '\'')
                {
                    // WARNING: This is not a valid string, however single quote is often used to make it more readable in text templating
                    // 'my string'

                    var pos = startIndex + 1;

                    while (pos < @this.Length)
                    {
                        var ch = @this[pos];
                        pos++;

                        char nextChar;
                        if (ch == '\\' && pos < @this.Length && ((nextChar = @this[pos]) == '\\' || nextChar == '\''))
                        {
                            sb.Append(nextChar);
                            pos++; // Treat as escape character for \\ or \"
                        }
                        else if (ch == '\'')
                        {
                            endIndex = pos;
                            return sb;
                        }
                        else
                        {
                            sb.Append(ch);
                        }
                    }

                    throw new Exception("Unclosed string starting at position: " + startIndex);
                }
            }

            endIndex = -1;
            return null;
        }
        #endregion

        #region ExtractToken
        /// <summary>
        /// A StringBuilder extension method that extracts the directive described by @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The extracted directive.</returns>
        public static StringBuilder ExtractToken(this StringBuilder @this)
        {
            return @this.ExtractToken(0);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the directive described by @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="endIndex">[out] The end index.</param>
        /// <returns>The extracted directive.</returns>
        public static StringBuilder ExtractToken(this StringBuilder @this, out int endIndex)
        {
            return @this.ExtractToken(0, out endIndex);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the directive described by @this.
        /// </summary>
        /// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
        /// <param name="this">The @this to act on.</param>
        /// <param name="startIndex">The start index.</param>
        /// <returns>The extracted directive.</returns>
        public static StringBuilder ExtractToken(this StringBuilder @this, int startIndex)
        {
            return @this.ExtractToken(startIndex, out int endIndex);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the directive described by @this.
        /// </summary>
        /// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
        /// <param name="this">The @this to act on.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">[out] The end index.</param>
        /// <returns>The extracted directive.</returns>
        public static StringBuilder ExtractToken(this StringBuilder @this, int startIndex, out int endIndex)
        {
            /* A token can be:
             * - Keyword / Literal
             * - Operator
             * - String
             * - Integer
             * - Real
             */

            // CHECK first which type is the token
            var ch1 = @this[startIndex];
            var pos = startIndex + 1;

            switch (ch1)
            {
                case '@':
                    if (pos < @this.Length && @this[pos] == '"')
                    {
                        return @this.ExtractStringArobasDoubleQuote(startIndex, out endIndex);
                    }
                    if (pos < @this.Length && @this[pos] == '\'')
                    {
                        return @this.ExtractStringArobasSingleQuote(startIndex, out endIndex);
                    }

                    break;
                case '"':
                    return @this.ExtractStringDoubleQuote(startIndex, out endIndex);
                case '\'':
                    return @this.ExtractStringSingleQuote(startIndex, out endIndex);
                case '`':
                case '~':
                case '!':
                case '#':
                case '$':
                case '%':
                case '^':
                case '&':
                case '*':
                case '(':
                case ')':
                case '-':
                case '_':
                case '=':
                case '+':
                case '[':
                case ']':
                case '{':
                case '}':
                case '|':
                case ':':
                case ';':
                case ',':
                case '.':
                case '<':
                case '>':
                case '?':
                case '/':
                    return @this.ExtractOperator(startIndex, out endIndex);
                case '0':
                    if (pos < @this.Length && (@this[pos] == 'x' || @this[pos] == 'X'))
                    {
                        return @this.ExtractHexadecimal(startIndex, out endIndex);
                    }

                    return @this.ExtractNumber(startIndex, out endIndex);
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return @this.ExtractNumber(startIndex, out endIndex);
                default:
                    if ((ch1 >= 'a' && ch1 <= 'z') || (ch1 >= 'A' && ch1 <= 'Z'))
                    {
                        return @this.ExtractKeyword(startIndex, out endIndex);
                    }

                    endIndex = -1;
                    return null;
            }

            throw new Exception("Invalid token");
        }
        #endregion

        #region ExtractTriviaToken
        /// <summary>
        /// A StringBuilder extension method that extracts the trivia tokens described by
        /// @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The extracted trivia tokens.</returns>
        public static StringBuilder ExtractTriviaToken(this StringBuilder @this)
        {
            return @this.ExtractTriviaToken(0);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the trivia tokens described by
        /// @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="endIndex">[out] The end index.</param>
        /// <returns>The extracted trivia tokens.</returns>
        public static StringBuilder ExtractTriviaToken(this StringBuilder @this, out int endIndex)
        {
            return @this.ExtractTriviaToken(0, out endIndex);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the trivia tokens described by
        /// @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="startIndex">The start index.</param>
        /// <returns>The extracted trivia tokens.</returns>
        public static StringBuilder ExtractTriviaToken(this StringBuilder @this, int startIndex)
        {
            return @this.ExtractTriviaToken(startIndex, out int endIndex);
        }

        /// <summary>
        /// A StringBuilder extension method that extracts the trivia tokens described by
        /// @this.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">[out] The end index.</param>
        /// <returns>The extracted trivia tokens.</returns>
        public static StringBuilder ExtractTriviaToken(this StringBuilder @this, int startIndex, out int endIndex)
        {
            var sb = new StringBuilder();
            var pos = startIndex;

            var isSpace = false;

            while (pos < @this.Length)
            {
                var ch = @this[pos];
                pos++;

                if (ch == ' ' || ch == '\r' || ch == '\n' || ch == '\t')
                {
                    isSpace = true;
                    sb.Append(ch);
                }
                else if (ch == '/' && !isSpace)
                {
                    if (pos < @this.Length)
                    {
                        ch = @this[pos];
                        if (ch == '/')
                        {
                            return @this.ExtractCommentSingleLine(startIndex, out endIndex);
                        }
                        if (ch == '*')
                        {
                            return @this.ExtractCommentMultiLine(startIndex, out endIndex);
                        }

                        // otherwise is probably the divide operator
                        pos--;
                        break;
                    }
                }
                else
                {
                    pos -= 2;
                    break;
                }
            }

            if (isSpace)
            {
                endIndex = pos;
                return sb;
            }

            endIndex = -1;
            return null;
        }
        #endregion

        #region AppendIf
        /// <summary>
        /// A StringBuilder extension method that appends a when.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="values">A variable-length parameters list containing values.</param>
        /// <returns>A StringBuilder.</returns>
        public static StringBuilder AppendIf<T>(this StringBuilder @this, Func<T, bool> predicate, params T[] values)
        {
            foreach (var value in values)
            {
                if (predicate(value))
                {
                    @this.Append(value);
                }
            }

            return @this;
        }
        #endregion

        #region AppendJoin
        /// <summary>
        /// A StringBuilder extension method that appends a join.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="separator">The separator.</param>
        /// <param name="values">The values.</param>
        public static StringBuilder AppendJoin<T>(this StringBuilder @this, string separator, IEnumerable<T> values)
        {
            @this.Append(string.Join(separator, values));

            return @this;
        }

        /// <summary>
        /// A StringBuilder extension method that appends a join.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="separator">The separator.</param>
        /// <param name="values">The values.</param>
        public static StringBuilder AppendJoin<T>(this StringBuilder @this, string separator, params T[] values)
        {
            @this.Append(string.Join(separator, values));

            return @this;
        }
        #endregion

        #region AppendLineFormat
        /// <summary>
        /// A StringBuilder extension method that appends a line format.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="format">Describes the format to use.</param>
        /// <param name="args">A variable-length parameters list containing arguments.</param>
        public static StringBuilder AppendLineFormat(this StringBuilder @this, string format, params object[] args)
        {
            @this.AppendLine(string.Format(format, args));
            return @this;
        }

        /// <summary>
        /// A StringBuilder extension method that appends a line format.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="format">Describes the format to use.</param>
        /// <param name="args">A variable-length parameters list containing arguments.</param>
        public static StringBuilder AppendLineFormat(this StringBuilder @this, string format, List<IEnumerable<object>> args)
        {
            @this.AppendLine(string.Format(format, args));
            return @this;
        }
        #endregion

        #region AppendLineIf
        /// <summary>
        /// A StringBuilder extension method that appends a line when.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="values">A variable-length parameters list containing values.</param>
        /// <returns>A StringBuilder.</returns>
        public static StringBuilder AppendLineIf<T>(this StringBuilder @this, Func<T, bool> predicate, params T[] values)
        {
            foreach (var value in values)
            {
                if (predicate(value))
                {
                    @this.AppendLine(value.ToString());
                }
            }
            return @this;
        }
        #endregion

        #region AppendLineJoin
        /// <summary>
        /// A StringBuilder extension method that appends a line join.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="separator">The separator.</param>
        /// <param name="values">The values.</param>
        public static StringBuilder AppendLineJoin<T>(this StringBuilder @this, string separator, IEnumerable<T> values)
        {
            @this.AppendLine(string.Join(separator, values));
            return @this;
        }

        /// <summary>
        /// A StringBuilder extension method that appends a line join.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="separator">The separator.</param>
        /// <param name="values">The values.</param>
        public static StringBuilder AppendLineJoin(this StringBuilder @this, string separator, params object[] values)
        {
            @this.AppendLine(string.Join(separator, values));
            return @this;
        }
        #endregion

        #region GetIndexAfterNextDoubleQuote
        /// <summary>
        /// A StringBuilder extension method that gets index after next double quote.
        /// </summary>
        /// <param name="this">The path to act on.</param>
        /// <returns>The index after next double quote.</returns>
        public static int GetIndexAfterNextDoubleQuote(this StringBuilder @this)
        {
            return @this.GetIndexAfterNextDoubleQuote(0, false);
        }

        /// <summary>
        /// A StringBuilder extension method that gets index after next double quote.
        /// </summary>
        /// <param name="this">The path to act on.</param>
        /// <param name="allowEscape">true to allow, false to deny escape.</param>
        /// <returns>The index after next double quote.</returns>
        public static int GetIndexAfterNextDoubleQuote(this StringBuilder @this, bool allowEscape)
        {
            return @this.GetIndexAfterNextDoubleQuote(0, allowEscape);
        }

        /// <summary>
        /// A StringBuilder extension method that gets index after next double quote.
        /// </summary>
        /// <param name="this">The path to act on.</param>
        /// <param name="startIndex">The start index.</param>
        /// <returns>The index after next double quote.</returns>
        public static int GetIndexAfterNextDoubleQuote(this StringBuilder @this, int startIndex)
        {
            return @this.GetIndexAfterNextDoubleQuote(startIndex, false);
        }

        /// <summary>
        /// A StringBuilder extension method that gets index after next double quote.
        /// </summary>
        /// <param name="this">The path to act on.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="allowEscape">true to allow, false to deny escape.</param>
        /// <returns>The index after next double quote.</returns>
        public static int GetIndexAfterNextDoubleQuote(this StringBuilder @this, int startIndex, bool allowEscape)
        {
            while (startIndex < @this.Length)
            {
                char ch = @this[startIndex];
                startIndex++;

                char nextChar;
                if (allowEscape && ch == '\\' && startIndex < @this.Length && ((nextChar = @this[startIndex]) == '\\' || nextChar == '"'))
                {
                    startIndex++; // Treat as escape character for \\ or \"
                }
                else if (ch == '"')
                {
                    return startIndex;
                }
            }

            return startIndex;
        }
        #endregion

        #region GetIndexAfterNextSingleQuote
        /// <summary>
        /// Gets index after next single quote.
        /// </summary>
        /// <param name="this">Full pathname of the file.</param>
        /// <returns>The index after next single quote.</returns>
        public static int GetIndexAfterNextSingleQuote(this StringBuilder @this)
        {
            return @this.GetIndexAfterNextSingleQuote(0, false);
        }

        /// <summary>
        /// Gets index after next single quote.
        /// </summary>
        /// <param name="this">Full pathname of the file.</param>
        /// <param name="allowEscape">true to allow, false to deny escape.</param>
        /// <returns>The index after next single quote.</returns>
        public static int GetIndexAfterNextSingleQuote(this StringBuilder @this, bool allowEscape)
        {
            return @this.GetIndexAfterNextSingleQuote(0, allowEscape);
        }

        /// <summary>
        /// Gets index after next single quote.
        /// </summary>
        /// <param name="this">Full pathname of the file.</param>
        /// <param name="startIndex">The start index.</param>
        /// <returns>The index after next single quote.</returns>
        public static int GetIndexAfterNextSingleQuote(this StringBuilder @this, int startIndex)
        {
            return @this.GetIndexAfterNextSingleQuote(startIndex, false);
        }

        /// <summary>
        /// Gets index after next single quote.
        /// </summary>
        /// <param name="this">Full pathname of the file.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="allowEscape">true to allow, false to deny escape.</param>
        /// <returns>The index after next single quote.</returns>
        public static int GetIndexAfterNextSingleQuote(this StringBuilder @this, int startIndex, bool allowEscape)
        {
            while (startIndex < @this.Length)
            {
                char ch = @this[startIndex];
                startIndex++;

                char nextChar;
                if (allowEscape && ch == '\\' && startIndex < @this.Length && ((nextChar = @this[startIndex]) == '\\' || nextChar == '\''))
                {
                    startIndex++; // Treat as escape character for \\ or \'
                }
                else if (ch == '\'')
                {
                    return startIndex;
                }
            }

            return startIndex;
        }
        #endregion

        #region Substring
        /// <summary>
        /// A StringBuilder extension method that substrings.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="startIndex">The start index.</param>
        /// <returns>A string.</returns>
        public static string Substring(this StringBuilder @this, int startIndex)
        {
            return @this.ToString(startIndex, @this.Length - startIndex);
        }

        /// <summary>
        /// A StringBuilder extension method that substrings.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="length">The length.</param>
        /// <returns>A string.</returns>
        public static string Substring(this StringBuilder @this, int startIndex, int length)
        {
            return @this.ToString(startIndex, length);
        }
        #endregion

        #region SubStringBuilder
        /// <summary>
        /// 根据指定起始索引位置截取StringBuilder
        /// </summary>
        /// <param name="this">源StringBuilder</param>
        /// <param name="startIndex">起始索引位置</param>
        /// <returns>StringBuilder</returns>
        public static StringBuilder SubStringBuilder(this StringBuilder @this, int startIndex)
        {
            if (startIndex <= -1)
                return @this;
            return @this.Remove(0, startIndex - 1);
        }

        /// <summary>
        /// 根据起始索引位置和指定长度截取StringBuilder
        /// </summary>
        /// <param name="this">源StringBuilder</param>
        /// <param name="startIndex">起始索引位置</param>
        /// <param name="length">截取长度</param>
        /// <returns>StringBuilder</returns>
        public static StringBuilder SubStringBuilder(this StringBuilder @this, int startIndex, int length)
        {
            return @this.SubStringBuilder(startIndex).Remove(length, @this.Length - length);
        }
        #endregion

        #region Remove
        /// <summary>
        /// 移除起始索引位置开始到尾部的内容
        /// </summary>
        /// <param name="this">源StringBuilder</param>
        /// <param name="startIndex">起始索引</param>
        /// <returns>StringBuilder</returns>
        public static StringBuilder Remove(this StringBuilder @this, int startIndex)
        {
            return @this.Remove(startIndex, @this.Length - startIndex);
        }
        #endregion

        #region IndexOf
        /// <summary>
        /// 获取指定字符串首次匹配的索引位置
        /// </summary>
        /// <param name="this">源StringBuilder</param>
        /// <param name="input">要查询的字符串</param>
        /// <returns>int</returns>
        public static int IndexOf(this StringBuilder @this, string input)
        {
            return @this.ToString().IndexOf(input);
        }

        /// <summary>
        /// 获取指定字符串首次匹配的索引位置
        /// </summary>
        /// <param name="this">源StringBuilder</param>
        /// <param name="input">要查询的字符串</param>
        /// <param name="startIndex">起始索引</param>
        /// <returns>int</returns>
        public static int IndexOf(this StringBuilder @this, string input, int startIndex)
        {
            return @this.ToString().IndexOf(input, startIndex);
        }
        #endregion

        #region LastIndexOf
        /// <summary>
        /// 获取指定字符串最后一次匹配的索引位置
        /// </summary>
        /// <param name="this">源StringBuilder</param>
        /// <param name="input">要查询的字符串</param>
        /// <returns>int</returns>
        public static int LastIndexOf(this StringBuilder @this, string input)
        {
            return @this.ToString().LastIndexOf(input);
        }

        /// <summary>
        ///  获取指定字符串最后一次匹配的索引位置
        /// </summary>
        /// <param name="this">源StringBuilder</param>
        /// <param name="input">要查询的字符串</param>
        /// <param name="startIndex">起始索引</param>
        /// <returns>int</returns>
        public static int LastIndexOf(this StringBuilder @this, string input, int startIndex)
        {
            return @this.ToString().LastIndexOf(input, startIndex);
        }
        #endregion
    }
}
