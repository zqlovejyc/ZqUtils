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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
/****************************
* [Author] 张强
* [Date] 2018-05-15
* [Describe] 集合扩展类
* **************************/
namespace ZqUtils.Extensions
{
    /// <summary>
    /// 集合扩展类
    /// </summary>
    public static partial class Extensions
    {
        #region ToList
        /// <summary>
        /// IList转成List
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="this">list数据源</param>
        /// <returns>List</returns>
        public static List<T> ToList<T>(this IList @this)
        {
            var array = new T[@this.Count];
            @this.CopyTo(array, 0);
            return new List<T>(array);
        }
        #endregion

        #region ToDataTable
        /// <summary>
        /// List集合转DataTable
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="this">list数据源</param>
        /// <returns>DataTable</returns>
        public static DataTable ToDataTable<T>(this List<T> @this)
        {
            DataTable dt = null;
            if (@this?.Count > 0)
            {
                dt = new DataTable(typeof(T).Name);
                var typeName = typeof(T).Name;
                var first = @this.FirstOrDefault();
                var firstTypeName = first.GetType().Name;
                if (typeName.Contains("Dictionary`2") || (typeName == "Object" && (firstTypeName == "DapperRow" || firstTypeName == "DynamicRow")))
                {
                    var dic = first as IDictionary<string, object>;
                    dt.Columns.AddRange(dic.Select(o => new DataColumn(o.Key, o.Value?.GetType().GetCoreType() ?? typeof(object))).ToArray());
                    var dics = @this.Select(o => o as IDictionary<string, object>);
                    foreach (var item in dics)
                    {
                        dt.Rows.Add(item.Select(o => o.Value).ToArray());
                    }
                }
                else
                {
                    var props = typeName == "Object" ? first.GetType().GetProperties() : typeof(T).GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance);
                    foreach (var prop in props)
                    {
                        dt.Columns.Add(prop.Name, prop?.PropertyType.GetCoreType() ?? typeof(object));
                    }
                    foreach (var item in @this)
                    {
                        var values = new object[props.Length];
                        for (var i = 0; i < props.Length; i++)
                        {
                            if (!props[i].CanRead) continue;
                            values[i] = props[i].GetValue(item, null);
                        }
                        dt.Rows.Add(values);
                    }
                }
            }
            return dt;
        }
        #endregion

        #region ToDictionary
        /// <summary>
        /// Converts a <see cref="NameValueCollection"/> to a <see cref="IDictionary{TKey,TValue}"/> instance.
        /// </summary>
        /// <param name="this">The <see cref="NameValueCollection"/> to convert.</param>
        /// <returns>An <see cref="IDictionary{TKey,TValue}"/> instance.</returns>
        public static IDictionary<string, IEnumerable<string>> ToDictionary(this NameValueCollection @this)
        {
            return @this.AllKeys.ToDictionary<string, string, IEnumerable<string>>(key => key, @this.GetValues);
        }
        #endregion

        #region Merge
        /// <summary>
        /// Merges a collection of <see cref="IDictionary{TKey,TValue}"/> instances into a single one.
        /// </summary>
        /// <param name="this">The list of <see cref="IDictionary{TKey,TValue}"/> instances to merge.</param>
        /// <param name="isIgnoreCase">Whether is case-sensitive</param>
        /// <returns>An <see cref="IDictionary{TKey,TValue}"/> instance containing the keys and values from the other instances.</returns>
        public static IDictionary<string, string> Merge(this IEnumerable<IDictionary<string, string>> @this, bool isIgnoreCase)
        {
            var output = new Dictionary<string, string>(isIgnoreCase ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);
            foreach (var dictionary in @this.Where(d => d != null))
            {
                foreach (var kvp in dictionary)
                {
                    if (!output.ContainsKey(kvp.Key))
                    {
                        output.Add(kvp.Key, kvp.Value);
                    }
                }
            }
            return output;
        }

        /// <summary>
        /// 合并字典参数
        /// </summary>
        /// <param name="this">字典</param>
        /// <param name="target">目标对象</param>
        /// <param name="overwrite">是否覆盖同名参数</param>
        /// <param name="excludes">排除项</param>
        /// <returns></returns>
        public static IDictionary<string, object> Merge(this IDictionary<string, object> @this, object target, bool overwrite = true, string[] excludes = null)
        {
            var exs = excludes != null ? new HashSet<string>(StringComparer.OrdinalIgnoreCase) : null;
            foreach (var item in target.ToDictionary())
            {
                if (exs == null || !exs.Contains(item.Key))
                {
                    if (overwrite || !@this.ContainsKey(item.Key)) @this[item.Key] = item.Value;
                }
            }
            return @this;
        }
        #endregion

        #region DistinctBy
        /// <summary>
        /// Filters a collection based on a provided key selector.
        /// </summary>
        /// <param name="this">The collection filter.</param>
        /// <param name="keySelector">The predicate to filter by.</param>
        /// <typeparam name="TSource">The type of the collection to filter.</typeparam>
        /// <typeparam name="TKey">The type of the key to filter by.</typeparam>
        /// <returns>A <see cref="IEnumerable{T}"/> instance with the filtered values.</returns>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> @this, Func<TSource, TKey> keySelector)
        {
            var knownKeys = new HashSet<TKey>();
            foreach (TSource element in @this)
            {
                if (knownKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
        #endregion

        #region Join
        /// <summary>
        /// 把一个列表组合成为一个字符串，默认逗号分隔
        /// </summary>
        /// <param name="this"></param>
        /// <param name="separator">组合分隔符，默认逗号</param>
        /// <returns>string</returns>
        public static string Join(this IEnumerable @this, string separator = ",")
        {
            var sb = new StringBuilder();
            if (@this != null)
            {
                foreach (var item in @this)
                {
                    sb.Separate(separator).Append(item + "");
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 把一个列表组合成为一个字符串，默认逗号分隔
        /// </summary>
        /// <param name="value"></param>
        /// <param name="separator">组合分隔符，默认逗号</param>
        /// <param name="func">把对象转为字符串的委托</param>
        /// <returns>string</returns>
        public static string Join<T>(this IEnumerable<T> value, string separator = ",", Func<T, string> func = null)
        {
            var sb = new StringBuilder();
            if (value != null)
            {
                if (func == null) func = obj => "{0}".F(obj);
                foreach (var item in value)
                {
                    sb.Separate(separator).Append(func(item));
                }
            }
            return sb.ToString();
        }
        #endregion

        #region ToArray
        /// <summary>
        /// 集合转为数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static T[] ToArray<T>(this ICollection<T> @this, int index = 0)
        {
            if (@this == null) return null;
            var count = @this.Count;
            if (count == 0) return new T[0];
            lock (@this)
            {
                count = @this.Count;
                if (count == 0) return new T[0];
                var arr = new T[count - index];
                @this.CopyTo(arr, index);
                return arr;
            }
        }
        #endregion

        #region AddIf
        /// <summary>
        /// An ICollection&lt;T&gt; extension method that adds only if the value satisfies the predicate.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="value">The value.</param>
        /// <returns>true if it succeeds, false if it fails.</returns>
        public static bool AddIf<T>(this ICollection<T> @this, Func<T, bool> predicate, T value)
        {
            if (predicate(value))
            {
                @this.Add(value);
                return true;
            }
            return false;
        }
        #endregion

        #region AddIfNotContains
        /// <summary>
        /// An ICollection&lt;T&gt; extension method that add value if the ICollection doesn't contains it already.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="value">The value.</param>
        /// <returns>true if it succeeds, false if it fails.</returns>
        public static bool AddIfNotContains<T>(this ICollection<T> @this, T value)
        {
            if (!@this.Contains(value))
            {
                @this.Add(value);
                return true;
            }

            return false;
        }
        #endregion

        #region AddRange
        /// <summary>
        /// An ICollection&lt;T&gt; extension method that adds a range to 'values'.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="values">A variable-length parameters list containing values.</param>
        public static void AddRange<T>(this ICollection<T> @this, params T[] values)
        {
            foreach (T value in values)
            {
                @this.Add(value);
            }
        }
        #endregion

        #region AddRangeIf
        /// <summary>
        /// An ICollection&lt;T&gt; extension method that adds a collection of objects to the end of this collection only
        /// for value who satisfies the predicate.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="values">A variable-length parameters list containing values.</param>
        public static void AddRangeIf<T>(this ICollection<T> @this, Func<T, bool> predicate, params T[] values)
        {
            foreach (T value in values)
            {
                if (predicate(value))
                {
                    @this.Add(value);
                }
            }
        }
        #endregion

        #region AddRangeIfNotContains
        /// <summary>
        /// An ICollection&lt;T&gt; extension method that adds a range of values that's not already in the ICollection.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="values">A variable-length parameters list containing values.</param>
        public static void AddRangeIfNotContains<T>(this ICollection<T> @this, params T[] values)
        {
            foreach (T value in values)
            {
                if (!@this.Contains(value))
                {
                    @this.Add(value);
                }
            }
        }
        #endregion

        #region ContainsAll
        /// <summary>
        /// An ICollection&lt;T&gt; extension method that query if '@this' contains all values.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="values">A variable-length parameters list containing values.</param>
        /// <returns>true if it succeeds, false if it fails.</returns>
        public static bool ContainsAll<T>(this ICollection<T> @this, params T[] values)
        {
            foreach (T value in values)
            {
                if (!@this.Contains(value))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// An IEnumerable&lt;T&gt; extension method that query if '@this' contains all.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="values">A variable-length parameters list containing values.</param>
        /// <returns>true if it succeeds, false if it fails.</returns>
        public static bool ContainsAll<T>(this IEnumerable<T> @this, params T[] values)
        {
            T[] list = @this.ToArray();
            foreach (T value in values)
            {
                if (!list.Contains(value))
                {
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region ContainsAny
        /// <summary>
        /// An ICollection&lt;T&gt; extension method that query if '@this' contains any value.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="values">A variable-length parameters list containing values.</param>
        /// <returns>true if it succeeds, false if it fails.</returns>
        public static bool ContainsAny<T>(this ICollection<T> @this, params T[] values)
        {
            foreach (T value in values)
            {
                if (@this.Contains(value))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// An IEnumerable&lt;T&gt; extension method that query if '@this' contains any.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="values">A variable-length parameters list containing values.</param>
        /// <returns>true if it succeeds, false if it fails.</returns>
        public static bool ContainsAny<T>(this IEnumerable<T> @this, params T[] values)
        {
            T[] list = @this.ToArray();
            foreach (T value in values)
            {
                if (list.Contains(value))
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region IsEmpty
        /// <summary>
        /// An ICollection&lt;T&gt; extension method that query if the collection is empty.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <returns>true if empty&lt; t&gt;, false if not.</returns>
        public static bool IsEmpty<T>(this ICollection<T> @this)
        {
            return @this.Count == 0;
        }

        /// <summary>
        /// An IEnumerable&lt;T&gt; extension method that query if 'collection' is empty.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The collection to act on.</param>
        /// <returns>true if empty, false if not.</returns>
        public static bool IsEmpty<T>(this IEnumerable<T> @this)
        {
            return !@this.Any();
        }
        #endregion

        #region IsNotEmpty
        /// <summary>
        /// An ICollection&lt;T&gt; extension method that query if the collection is not empty.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <returns>true if not empty&lt; t&gt;, false if not.</returns>
        public static bool IsNotEmpty<T>(this ICollection<T> @this)
        {
            return @this.Count != 0;
        }

        /// <summary>
        /// An IEnumerable&lt;T&gt; extension method that queries if a not is empty.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The collection to act on.</param>
        /// <returns>true if a not is t>, false if not.</returns>
        public static bool IsNotEmpty<T>(this IEnumerable<T> @this)
        {
            return @this.Any();
        }
        #endregion

        #region IsNotNullOrEmpty
        /// <summary>
        /// An ICollection&lt;T&gt; extension method that queries if the collection is not (null or is empty).
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <returns>true if the collection is not (null or empty), false if not.</returns>
        public static bool IsNotNullOrEmpty<T>(this ICollection<T> @this)
        {
            return @this != null && @this.Count != 0;
        }

        /// <summary>
        /// An IEnumerable&lt;T&gt; extension method that queries if a not null or is empty.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The collection to act on.</param>
        /// <returns>true if a not null or is t>, false if not.</returns>
        public static bool IsNotNullOrEmpty<T>(this IEnumerable<T> @this)
        {
            return @this != null && @this.Any();
        }
        #endregion

        #region IsNullOrEmpty
        /// <summary>
        /// An ICollection&lt;T&gt; extension method that queries if the collection is null or is empty.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <returns>true if null or empty&lt; t&gt;, false if not.</returns>
        public static bool IsNullOrEmpty<T>(this ICollection<T> @this)
        {
            return @this == null || @this.Count == 0;
        }

        /// <summary>
        ///     An IEnumerable&lt;T&gt; extension method that queries if a null or is empty.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The collection to act on.</param>
        /// <returns>true if a null or is t>, false if not.</returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> @this)
        {
            return @this == null || !@this.Any();
        }
        #endregion

        #region RemoveIf
        /// <summary>
        /// An ICollection&lt;T&gt; extension method that removes if.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="value">The value.</param>
        /// <param name="predicate">The predicate.</param>
        public static void RemoveIf<T>(this ICollection<T> @this, T value, Func<T, bool> predicate)
        {
            if (predicate(value))
            {
                @this.Remove(value);
            }
        }
        #endregion

        #region RemoveIfContains
        /// <summary>
        /// An ICollection&lt;T&gt; extension method that removes if contains.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="value">The value.</param>
        public static void RemoveIfContains<T>(this ICollection<T> @this, T value)
        {
            if (@this.Contains(value))
            {
                @this.Remove(value);
            }
        }
        #endregion

        #region RemoveRange
        /// <summary>
        /// An ICollection&lt;T&gt; extension method that removes the range.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="values">A variable-length parameters list containing values.</param>
        public static void RemoveRange<T>(this ICollection<T> @this, params T[] values)
        {
            foreach (T value in values)
            {
                @this.Remove(value);
            }
        }
        #endregion

        #region RemoveRangeIf
        /// <summary>
        /// An ICollection&lt;T&gt; extension method that removes range item that satisfy the predicate.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="values">A variable-length parameters list containing values.</param>
        public static void RemoveRangeIf<T>(this ICollection<T> @this, Func<T, bool> predicate, params T[] values)
        {
            foreach (T value in values)
            {
                if (predicate(value))
                {
                    @this.Remove(value);
                }
            }
        }
        #endregion

        #region RemoveRangeIfContains
        /// <summary>
        /// An ICollection&lt;T&gt; extension method that removes the range if contains.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="values">A variable-length parameters list containing values.</param>
        public static void RemoveRangeIfContains<T>(this ICollection<T> @this, params T[] values)
        {
            foreach (T value in values)
            {
                if (@this.Contains(value))
                {
                    @this.Remove(value);
                }
            }
        }
        #endregion

        #region RemoveWhere
        /// <summary>
        /// An ICollection&lt;T&gt; extension method that removes value that satisfy the predicate.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="predicate">The predicate.</param>
        public static void RemoveWhere<T>(this ICollection<T> @this, Func<T, bool> predicate)
        {
            List<T> list = @this.Where(predicate).ToList();
            foreach (T item in list)
            {
                @this.Remove(item);
            }
        }
        #endregion

        #region MergeDistinctInnerEnumerable
        /// <summary>
        /// Enumerates merge distinct inner enumerable in this collection.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <returns>
        /// An enumerator that allows foreach to be used to process merge distinct inner
        /// enumerable in this collection.
        /// </returns>
        public static IEnumerable<T> MergeDistinctInnerEnumerable<T>(this IEnumerable<IEnumerable<T>> @this)
        {
            List<IEnumerable<T>> listItem = @this.ToList();
            var list = new List<T>();
            foreach (var item in listItem)
            {
                list = list.Union(item).ToList();
            }
            return list;
        }
        #endregion

        #region MergeInnerEnumerable
        /// <summary>
        /// Enumerates merge inner enumerable in this collection.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <returns>
        /// An enumerator that allows foreach to be used to process merge inner enumerable in
        /// this collection.
        /// </returns>
        public static IEnumerable<T> MergeInnerEnumerable<T>(this IEnumerable<IEnumerable<T>> @this)
        {
            List<IEnumerable<T>> listItem = @this.ToList();
            var list = new List<T>();
            foreach (var item in listItem)
            {
                list.AddRange(item);
            }
            return list;
        }
        #endregion

        #region ForEach
        /// <summary>
        /// Enumerates for each in this collection.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="action">The action.</param>
        /// <returns>An enumerator that allows foreach to be used to process for each in this collection.</returns>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> @this, Action<T> action)
        {
            foreach (var item in @this)
            {
                action(item);
            }
            return @this;
        }

        /// <summary>Enumerates for each in this collection.</summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="action">The action.</param>
        /// <returns>An enumerator that allows foreach to be used to process for each in this collection.</returns>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> @this, Action<T, int> action)
        {
            var array = @this.ToArray();
            for (int i = 0; i < array.Length; i++)
            {
                action(array[i], i);
            }
            return array;
        }
        #endregion

        #region StringJoin
        /// <summary>
        /// Concatenates all the elements of a IEnumerable, using the specified separator between each element.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">An IEnumerable that contains the elements to concatenate.</param>
        /// <param name="separator">
        /// The string to use as a separator. separator is included in the returned string only if
        /// value has more than one element.
        /// </param>
        /// <returns>
        /// A string that consists of the elements in value delimited by the separator string. If value is an empty array,
        /// the method returns String.Empty.
        /// </returns>
        public static string StringJoin<T>(this IEnumerable<T> @this, string separator)
        {
            return string.Join(separator, @this);
        }

        /// <summary>
        /// Concatenates all the elements of a IEnumerable, using the specified separator between
        /// each element.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="separator">
        /// The string to use as a separator. separator is included in the
        /// returned string only if value has more than one element.
        /// </param>
        /// <returns>
        /// A string that consists of the elements in value delimited by the separator string. If
        /// value is an empty array, the method returns String.Empty.
        /// </returns>
        public static string StringJoin<T>(this IEnumerable<T> @this, char separator)
        {
            return string.Join(separator.ToString(), @this);
        }
        #endregion
    }
}
