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
using System.Collections.ObjectModel;
using System.Data;
using System.Reflection;
/****************************
* [Author] 张强
* [Date] 2018-07-10
* [Describe] Array扩展类
* **************************/
namespace ZqUtils.Extensions
{
    /// <summary>
    /// Array扩展类
    /// </summary>
    public static partial class Extensions
    {
        #region IndexOf
        /// <summary>
        /// Searches for the specified object and returns the index of the first occurrence within the entire one-
        /// dimensional .
        /// </summary>
        /// <param name="this">The one-dimensional  to search.</param>
        /// <param name="value">The object to locate in .</param>
        /// <returns>
        /// The index of the first occurrence of  within the entire , if found; otherwise, the lower bound of the array
        /// minus 1.
        /// </returns>
        public static int IndexOf(this Array @this, object value)
        {
            return Array.IndexOf(@this, value);
        }

        /// <summary>
        /// Searches for the specified object and returns the index of the first occurrence within the range of elements
        /// in the one-dimensional  that extends from the specified index to the last element.
        /// </summary>
        /// <param name="this">The one-dimensional  to search.</param>
        /// <param name="value">The object to locate in .</param>
        /// <param name="startIndex">The starting index of the search. 0 (zero) is valid in an empty array.</param>
        /// <returns>
        /// The index of the first occurrence of  within the range of elements in  that extends from  to the last element,
        /// if found; otherwise, the lower bound of the array minus 1.
        /// </returns>
        public static int IndexOf(this Array @this, object value, int startIndex)
        {
            return Array.IndexOf(@this, value, startIndex);
        }

        /// <summary>
        /// Searches for the specified object and returns the index of the first occurrence within the range of elements
        /// in the one-dimensional  that starts at the specified index and contains the specified number of elements.
        /// </summary>
        /// <param name="this">The one-dimensional  to search.</param>
        /// <param name="value">The object to locate in .</param>
        /// <param name="startIndex">The starting index of the search. 0 (zero) is valid in an empty array.</param>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <returns>
        /// The index of the first occurrence of  within the range of elements in  that starts at  and contains the
        /// number of elements specified in , if found; otherwise, the lower bound of the array minus 1.
        /// </returns>
        public static int IndexOf(this Array @this, object value, int startIndex, int count)
        {
            return Array.IndexOf(@this, value, startIndex, count);
        }
        #endregion

        #region ClearAll
        /// <summary>
        /// A T[] extension method that clears all described by @this.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// ###
        /// <returns>.</returns>
        public static void ClearAll<T>(this T[] @this)
        {
            Array.Clear(@this, 0, @this.Length);
        }
        #endregion

        #region ClearAt
        /// <summary>
        /// A T[] extension method that clears at.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The arrayToClear to act on.</param>
        /// <param name="at">at.</param>
        public static void ClearAt<T>(this T[] @this, int at)
        {
            Array.Clear(@this, at, 1);
        }
        #endregion

        #region ToDataTable
        /// <summary>
        /// A T[] extension method that converts the @this to a data table.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as a DataTable.</returns>
        public static DataTable ToDataTable<T>(this T[] @this)
        {
            Type type = typeof(T);

            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

            var dt = new DataTable();

            foreach (PropertyInfo property in properties)
            {
                dt.Columns.Add(property.Name, property.PropertyType);
            }

            foreach (FieldInfo field in fields)
            {
                dt.Columns.Add(field.Name, field.FieldType);
            }

            foreach (T item in @this)
            {
                DataRow dr = dt.NewRow();

                foreach (PropertyInfo property in properties)
                {
                    dr[property.Name] = property.GetValue(item, null);
                }

                foreach (FieldInfo field in fields)
                {
                    dr[field.Name] = field.GetValue(item);
                }

                dt.Rows.Add(dr);
            }

            return dt;
        }
        #endregion

        #region AsReadOnly
        /// <summary>
        /// A T[] extension method that converts an array to a read only.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="array">The array to act on.</param>
        /// <returns>A list of.</returns>
        public static ReadOnlyCollection<T> AsReadOnly<T>(this T[] array)
        {
            return Array.AsReadOnly(array);
        }
        #endregion

        #region Exists
        /// <summary>
        /// A T[] extension method that exists.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The array to act on.</param>
        /// <param name="match">Specifies the match.</param>
        /// <returns>true if it succeeds, false if it fails.</returns>
        public static bool Exists<T>(this T[] @this, Predicate<T> match)
        {
            return Array.Exists(@this, match);
        }
        #endregion

        #region Find
        /// <summary>
        /// A T[] extension method that searches for the first match.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The array to act on.</param>
        /// <param name="match">Specifies the match.</param>
        /// <returns>A T.</returns>
        public static T Find<T>(this T[] @this, Predicate<T> match)
        {
            return Array.Find(@this, match);
        }
        #endregion

        #region FindAll
        /// <summary>
        /// A T[] extension method that searches for the first all.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The array to act on.</param>
        /// <param name="match">Specifies the match.</param>
        /// <returns>The found all.</returns>
        public static T[] FindAll<T>(this T[] @this, Predicate<T> match)
        {
            return Array.FindAll(@this, match);
        }
        #endregion

        #region FindIndex
        /// <summary>
        /// A T[] extension method that searches for the first index.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The array to act on.</param>
        /// <param name="match">Specifies the match.</param>
        /// <returns>The found index.</returns>
        public static int FindIndex<T>(this T[] @this, Predicate<T> match)
        {
            return Array.FindIndex(@this, match);
        }

        /// <summary>
        /// A T[] extension method that searches for the first index.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="array">The array to act on.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="match">Specifies the match.</param>
        /// <returns>The found index.</returns>
        public static int FindIndex<T>(this T[] array, int startIndex, Predicate<T> match)
        {
            return Array.FindIndex(array, startIndex, match);
        }

        /// <summary>
        /// A T[] extension method that searches for the first index.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="array">The array to act on.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="count">Number of.</param>
        /// <param name="match">Specifies the match.</param>
        /// <returns>The found index.</returns>
        public static int FindIndex<T>(this T[] array, int startIndex, int count, Predicate<T> match)
        {
            return Array.FindIndex(array, startIndex, count, match);
        }
        #endregion

        #region FindLast
        /// <summary>
        /// A T[] extension method that searches for the first last.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The array to act on.</param>
        /// <param name="match">Specifies the match.</param>
        /// <returns>The found last.</returns>
        public static T FindLast<T>(this T[] @this, Predicate<T> match)
        {
            return Array.FindLast(@this, match);
        }
        #endregion

        #region FindLastIndex
        /// <summary>
        /// A T[] extension method that searches for the last index.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The array to act on.</param>
        /// <param name="match">Specifies the match.</param>
        /// <returns>The found index.</returns>
        public static int FindLastIndex<T>(this T[] @this, Predicate<T> match)
        {
            return Array.FindLastIndex(@this, match);
        }

        /// <summary>
        /// A T[] extension method that searches for the last index.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="array">The array to act on.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="match">Specifies the match.</param>
        /// <returns>The found index.</returns>
        public static int FindLastIndex<T>(this T[] array, int startIndex, Predicate<T> match)
        {
            return Array.FindLastIndex(array, startIndex, match);
        }

        /// <summary>
        /// A T[] extension method that searches for the last index.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="array">The array to act on.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="count">Number of.</param>
        /// <param name="match">Specifies the match.</param>
        /// <returns>The found index.</returns>
        public static int FindLastIndex<T>(this T[] array, int startIndex, int count, Predicate<T> match)
        {
            return Array.FindLastIndex(array, startIndex, count, match);
        }
        #endregion

        #region ForEach
        /// <summary>
        /// A T[] extension method that applies an operation to all items in this collection.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The array to act on.</param>
        /// <param name="action">The action.</param>
        public static void ForEach<T>(this T[] @this, Action<T> action)
        {
            Array.ForEach(@this, action);
        }
        #endregion

        #region TrueForAll
        /// <summary>
        /// A T[] extension method that true for all.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The array to act on.</param>
        /// <param name="match">Specifies the match.</param>
        /// <returns>true if it succeeds, false if it fails.</returns>
        public static bool TrueForAll<T>(this T[] @this, Predicate<T> match)
        {
            return Array.TrueForAll(@this, match);
        }
        #endregion
    }
}
