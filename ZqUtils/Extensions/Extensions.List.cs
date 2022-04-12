#region License
/***
 * Copyright © 2018-2022, 张强 (943620963@qq.com).
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
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
    public static class ListExtensions
    {
        #region Default
        /// <summary>
        /// 若当前集合为null时，返回默认值或空集合；若不为null时，返回集合本身；
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static List<T> DefaultIfNull<T>(this List<T> @this, List<T> defaultValue = null)
        {
            return @this ?? defaultValue ?? new List<T>();
        }

        /// <summary>
        /// 若当前集合为null时，返回默认值或空集合；若不为null时，返回集合本身；
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static IEnumerable<T> DefaultIfNull<T>(this IEnumerable<T> @this, IEnumerable<T> defaultValue = null)
        {
            return @this ?? defaultValue ?? new List<T>();
        }

        /// <summary>
        /// 若当前集合为null或empty时，返回默认值或空集合；若不为null且不为empty时，返回集合本身；
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static List<T> DefaultIfNullOrEmpty<T>(this List<T> @this, List<T> defaultValue = null)
        {
            return @this.IsNullOrEmpty() ? (defaultValue ?? new List<T>()) : @this;
        }

        /// <summary>
        /// 若当前集合为null或empty时，返回默认值或空集合；若不为null且不为empty时，返回集合本身；
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static IEnumerable<T> DefaultIfNullOrEmpty<T>(this IEnumerable<T> @this, IEnumerable<T> defaultValue = null)
        {
            return @this.IsNullOrEmpty() ? (defaultValue ?? new List<T>()) : @this;
        }
        #endregion

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
                var type = typeof(T);
                var first = @this.First();
                var firstType = first.GetType();
                if (type.IsDictionaryType() || (type.IsDynamicOrObjectType() && firstType.IsDictionaryType()))
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
                    var props = type.IsDynamicOrObjectType() ? firstType.GetProperties() : typeof(T).GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance);
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
        /// <param name="predicate">The predicate to remove.</param>
        public static void RemoveIf<T>(this ICollection<T> @this, Func<T, bool> predicate)
        {
            var res = @this.Where(predicate);
            if (res.IsNotNullOrEmpty())
            {
                @this.RemoveRange(res.ToArray());
            }
        }

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

        /// <summary>
        /// Enumerates for each in this collection.
        /// </summary>
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

        #region ForEachAsync
        /// <summary>
        /// Enumerates for each in this collection.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="func">The func.</param>
        /// <returns>An enumerator that allows foreach to be used to process for each in this collection.</returns>
        public static async Task<IEnumerable<T>> ForEachAsync<T>(this IEnumerable<T> @this, Func<T, Task> func)
        {
            foreach (var item in @this)
            {
                await func(item);
            }
            return @this;
        }

        /// <summary>
        /// Enumerates for each in this collection.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="func">The func.</param>
        /// <returns>An enumerator that allows foreach to be used to process for each in this collection.</returns>
        public static async Task<IEnumerable<T>> ForEachAsync<T>(this IEnumerable<T> @this, Func<T, int, Task> func)
        {
            var array = @this.ToArray();
            for (int i = 0; i < array.Length; i++)
            {
                await func(array[i], i);
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

        #region TreeWhere
        /// <summary>
        /// 树形查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="this">数据源</param>
        /// <param name="condition">查询条件</param>
        /// <param name="primaryKey">实体主键</param>
        /// <param name="down">是否向下查询，默认true</param>
        /// <param name="parentId">树形父级字段，默认ParentId</param>
        /// <param name="comparer">返回集合去重比较器，默认null</param>
        /// <returns>返回要查询的所有树形节点集合</returns>
        public static IEnumerable<T> TreeWhere<T>(
            this IEnumerable<T> @this,
            Predicate<T> condition,
            string primaryKey,
            bool down = true,
            string parentId = "ParentId",
            IEqualityComparer<T> comparer = null)
            where T : class
        {
            var type = typeof(T);
            var treeList = new List<T>();
            var entities = @this?.ToList().FindAll(condition);
            if (entities?.Count > 0)
            {
                foreach (var entity in entities)
                {
                    treeList.Add(entity);
                    var key = type.GetProperty(primaryKey).GetValue(entity, null)?.ToString();
                    var pid = type.GetProperty(parentId).GetValue(entity, null)?.ToString();
                    //向下查询
                    if (down)
                    {
                        if (!key.IsNullOrEmpty() && !string.Equals(key, pid, StringComparison.OrdinalIgnoreCase))
                        {
                            while (true)
                            {
                                if (key.IsNullOrEmpty())
                                    break;
                                key = @this.TreeWhere(treeList, primaryKey, key, down, parentId);
                            }
                        }
                    }
                    //向上查询
                    else
                    {
                        if (!pid.IsNullOrEmpty() && !string.Equals(key, pid, StringComparison.OrdinalIgnoreCase))
                        {
                            while (true)
                            {
                                if (pid.IsNullOrEmpty())
                                    break;
                                pid = @this.TreeWhere(treeList, primaryKey, pid, down, parentId);
                            }
                        }
                    }
                }
            }
            if (comparer == null)
                return treeList.Distinct().ToList();
            else
                return treeList.Distinct(comparer).ToList();
        }

        /// <summary>
        /// 递归树形查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="this">数据源</param>
        /// <param name="treeList">树形节点集合</param>
        /// <param name="primaryKey">实体主键</param>
        /// <param name="id">主键或者父级主键值</param>
        /// <param name="down">是否向下查询，默认true</param>
        /// <param name="parentId">树形父级字段，默认ParentId</param>
        /// <returns>返回递归是否结束标识，递归结束后返回空字符串</returns>
        public static string TreeWhere<T>(
            this IEnumerable<T> @this,
            List<T> treeList,
            string primaryKey,
            string id,
            bool down = true,
            string parentId = "ParentId")
            where T : class
        {
            var type = typeof(T);
            var parameter = Expression.Parameter(type, "t");
            var condition = Expression.Equal(parameter.Property(down ? parentId : primaryKey), Expression.Constant(id)).ToLambda<Predicate<T>>(parameter).Compile();
            var entities = @this?.ToList().FindAll(condition);
            if (entities?.Count > 0 && treeList?.Count > 0)
            {
                foreach (var entity in entities)
                {
                    treeList.Add(entity);
                    var key = type.GetProperty(primaryKey).GetValue(entity, null)?.ToString();
                    var pid = type.GetProperty(parentId).GetValue(entity, null)?.ToString();
                    //向下查询
                    if (down)
                    {
                        if (!key.IsNullOrEmpty() && !string.Equals(key, pid, StringComparison.OrdinalIgnoreCase))
                        {
                            id = @this.TreeWhere(treeList, primaryKey, key, down, parentId);
                        }
                        else
                        {
                            id = "";
                        }
                    }
                    //向上查询
                    else
                    {
                        if (!pid.IsNullOrEmpty() && !string.Equals(key, pid, StringComparison.OrdinalIgnoreCase))
                        {
                            id = @this.TreeWhere(treeList, primaryKey, pid, down, parentId);
                        }
                        else
                        {
                            id = "";
                        }
                    }
                }
            }
            else
            {
                id = "";
            }
            return id;
        }
        #endregion

        #region TreeToJson
        /// <summary>
        /// 树形集合数据转Json
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="this">数据源</param>
        /// <param name="primaryKey">实体主键</param>
        /// <param name="pids">父级字段值数组，默认: new[]{"0"}</param>
        /// <param name="parentId">树形父级字段，默认ParentId</param>
        /// <param name="childName">子集节点命名</param>
        /// <returns>返回树形Json</returns>
        public static string TreeToJson<T>(
            this IEnumerable<T> @this,
            string primaryKey,
            string[] pids = null,
            string parentId = "ParentId",
            string childName = "ChildNodes")
            where T : class
        {
            return @this.TreeToDictionary(primaryKey, pids, parentId, childName).ToJson();
        }
        #endregion

        #region TreeToDictionary
        /// <summary>
        /// 树形集合数据转Json
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="this">数据源</param>
        /// <param name="primaryKey">实体主键</param>
        /// <param name="pids">父级字段值数组，默认: new[]{"0"}</param>
        /// <param name="parentId">树形父级字段，默认ParentId</param>
        /// <param name="childName">子集节点命名</param>
        /// <returns>返回树形Json</returns>
        public static IEnumerable<Dictionary<string, object>> TreeToDictionary<T>(
            this IEnumerable<T> @this,
            string primaryKey,
            string[] pids = null,
            string parentId = "ParentId",
            string childName = "ChildNodes")
            where T : class
        {
            var type = typeof(T);
            var entities = new List<T>();
            var list = new List<Dictionary<string, object>>();
            pids ??= new[] { "0" };

            foreach (var pid in pids)
            {
                var parameter = Expression.Parameter(type, "t");
                var condition = Expression.Equal(parameter.Property(parentId), Expression.Constant(pid)).ToLambda<Predicate<T>>(parameter).Compile();
                var result = @this?.ToList().FindAll(condition);
                if (result.IsNotNullOrEmpty())
                    entities.AddRangeIfNotContains(result.ToArray());
            }

            if (entities.Count > 0)
            {
                var props = type.GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance);
                foreach (var entity in entities)
                {
                    var dic = new Dictionary<string, object>();
                    foreach (var p in props)
                    {
                        dic[p.Name] = p.GetValue(entity, null);
                    }
                    dic[childName] = @this.TreeToDictionary(primaryKey, new[] { dic[primaryKey].ToString() }, parentId, childName);
                    list.Add(dic);
                }
            }
            return list;
        }
        #endregion

        #region PageList
        /// <summary>
        /// 获取集合的指定分页数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>返回分页数据</returns>
        public static IList<T> PageList<T>(this ICollection<T> @this, int pageSize, int pageIndex)
        {
            return @this.PageList(pageSize, pageIndex, out _, out _);
        }

        /// <summary>
        /// 获取集合的指定分页数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>返回分页数据</returns>
        public static IList<T> PageList<T>(this ICollection<T> @this, int pageSize, int pageIndex, out int pageCount)
        {
            return @this.PageList(pageSize, pageIndex, out _, out pageCount);
        }

        /// <summary>
        /// 获取集合的指定分页数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="recordCount">返回总条数</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>返回分页数据</returns>
        public static IList<T> PageList<T>(this ICollection<T> @this, int pageSize, int pageIndex, out int recordCount, out int pageCount)
        {
            if (@this == null)
            {
                recordCount = 0;
                pageCount = 0;

                return new List<T>();
            }

            recordCount = @this.Count;

            if (recordCount < 1)
            {
                pageCount = 0;

                return new List<T>();
            }

            if (pageSize < 1)
                pageSize = 1;

            if (pageSize > recordCount)
                pageSize = recordCount;

            pageCount = recordCount / pageSize + (recordCount % pageSize == 0 ? 0 : 1);

            if (pageIndex < 1)
                pageIndex = 1;

            if (pageIndex > pageCount)
                pageIndex = pageCount;

            if (pageIndex > 1)
                return @this.Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToList();

            return @this.Take(pageSize).ToList();
        }

        /// <summary>
        /// 获取集合的指定分页数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>返回分页数据</returns>
        public static IList<T> PageList<T>(this IEnumerable<T> @this, int pageSize, int pageIndex)
        {
            return @this.PageList(pageSize, pageIndex, out _, out _);
        }

        /// <summary>
        /// 获取集合的指定分页数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>返回分页数据</returns>
        public static IList<T> PageList<T>(this IEnumerable<T> @this, int pageSize, int pageIndex, out int pageCount)
        {
            return @this.PageList(pageSize, pageIndex, out _, out pageCount);
        }

        /// <summary>
        /// 获取集合的指定分页数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="recordCount">返回总条数</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>返回分页数据</returns>
        public static IList<T> PageList<T>(this IEnumerable<T> @this, int pageSize, int pageIndex, out int recordCount, out int pageCount)
        {
            if (@this == null)
            {
                recordCount = 0;
                pageCount = 0;

                return new List<T>();
            }

            recordCount = @this.Count();

            if (recordCount < 1)
            {
                pageCount = 0;

                return new List<T>();
            }

            if (pageSize < 1)
                pageSize = 1;

            if (pageSize > recordCount)
                pageSize = recordCount;

            pageCount = recordCount / pageSize + (recordCount % pageSize == 0 ? 0 : 1);

            if (pageIndex < 1)
                pageIndex = 1;

            if (pageIndex > pageCount)
                pageIndex = pageCount;

            if (pageIndex > 1)
                return @this.Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToList();

            return @this.Take(pageSize).ToList();
        }
        #endregion

        #region PageEach
        #region Sync
        /// <summary>
        /// 集合分页循环处理每页数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="action">自定义页数据处理委托</param>
        public static void PageEach<T>(this ICollection<T> @this, int pageSize, Action<int, IList<T>> action)
        {
            if (action == null)
                return;

            var pageIndex = 1;

            while (true)
            {
                var item = @this.PageList(pageSize, pageIndex, out var recordCount, out var pageCount);

                if (recordCount <= 0)
                    break;

                action(pageIndex, item);

                if (pageCount <= pageIndex)
                    break;

                pageIndex++;
            }
        }

        /// <summary>
        /// 集合分页循环处理每页数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="func">自定义页数据处理委托，当结果为false时跳出循环</param>
        public static void PageEach<T>(this ICollection<T> @this, int pageSize, Func<int, IList<T>, bool> func)
        {
            if (func == null)
                return;

            var pageIndex = 1;

            while (true)
            {
                var item = @this.PageList(pageSize, pageIndex, out var recordCount, out var pageCount);

                if (recordCount <= 0)
                    break;

                var res = func(pageIndex, item);
                if (!res)
                    break;

                if (pageCount <= pageIndex)
                    break;

                pageIndex++;
            }
        }

        /// <summary>
        /// 集合分页循环处理每页数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="action">自定义页数据处理委托</param>
        public static void PageEach<T>(this ICollection<T> @this, int pageSize, Action<IList<T>> action)
        {
            @this.PageEach(pageSize, action, false);
        }

        /// <summary>
        /// 集合分页循环处理每页数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="action">自定义页数据处理委托</param>
        /// <param name="isParallel">是否并行执行</param>
        public static void PageEach<T>(this ICollection<T> @this, int pageSize, Action<IList<T>> action, bool isParallel)
        {
            if (action == null)
                return;

            var pageIndex = 1;
            var list = new List<IList<T>>();

            while (true)
            {
                var item = @this.PageList(pageSize, pageIndex, out var recordCount, out var pageCount);

                if (recordCount <= 0)
                    break;

                list.Add(item);

                if (pageCount <= pageIndex)
                    break;

                pageIndex++;
            }

            if (isParallel && list.Count > 1)
                list.AsParallel().ForAll(action);
            else
                list.ForEach(action);
        }

        /// <summary>
        /// 集合分页循环处理每页数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="action">自定义页数据处理委托</param>
        public static void PageEach<T>(this IEnumerable<T> @this, int pageSize, Action<int, IList<T>> action)
        {
            if (action == null)
                return;

            var pageIndex = 1;

            while (true)
            {
                var item = @this.PageList(pageSize, pageIndex, out var recordCount, out var pageCount);

                if (recordCount <= 0)
                    break;

                action(pageIndex, item);

                if (pageCount <= pageIndex)
                    break;

                pageIndex++;
            }
        }

        /// <summary>
        /// 集合分页循环处理每页数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="func">自定义页数据处理委托，当结果为false时跳出循环</param>
        public static void PageEach<T>(this IEnumerable<T> @this, int pageSize, Func<int, IList<T>, bool> func)
        {
            if (func == null)
                return;

            var pageIndex = 1;

            while (true)
            {
                var item = @this.PageList(pageSize, pageIndex, out var recordCount, out var pageCount);

                if (recordCount <= 0)
                    break;

                var res = func(pageIndex, item);
                if (!res)
                    break;

                if (pageCount <= pageIndex)
                    break;

                pageIndex++;
            }
        }

        /// <summary>
        /// 集合分页循环处理每页数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="action">自定义页数据处理委托</param>
        public static void PageEach<T>(this IEnumerable<T> @this, int pageSize, Action<IList<T>> action)
        {
            @this.PageEach(pageSize, action, false);
        }

        /// <summary>
        /// 集合分页循环处理每页数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="action">自定义页数据处理委托</param>
        /// <param name="isParallel">是否并行执行</param>
        public static void PageEach<T>(this IEnumerable<T> @this, int pageSize, Action<IList<T>> action, bool isParallel)
        {
            if (action == null)
                return;

            var pageIndex = 1;
            var list = new List<IList<T>>();

            while (true)
            {
                var item = @this.PageList(pageSize, pageIndex, out var recordCount, out var pageCount);

                if (recordCount <= 0)
                    break;

                list.Add(item);

                if (pageCount <= pageIndex)
                    break;

                pageIndex++;
            }

            if (isParallel && list.Count > 1)
                list.AsParallel().ForAll(action);
            else
                list.ForEach(action);
        }
        #endregion

        #region Async
        /// <summary>
        /// 集合分页循环处理每页数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="func">自定义页数据处理委托</param>
        public static async Task PageEachAsync<T>(this ICollection<T> @this, int pageSize, Func<int, IList<T>, Task> func)
        {
            if (func == null)
                return;

            var pageIndex = 1;

            while (true)
            {
                var item = @this.PageList(pageSize, pageIndex, out var recordCount, out var pageCount);

                if (recordCount <= 0)
                    break;

                await func(pageIndex, item).ConfigureAwait(false);

                if (pageCount <= pageIndex)
                    break;

                pageIndex++;
            }
        }

        /// <summary>
        /// 集合分页循环处理每页数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="func">自定义页数据处理委托，当结果为false时跳出循环</param>
        public static async Task PageEachAsync<T>(this ICollection<T> @this, int pageSize, Func<int, IList<T>, Task<bool>> func)
        {
            if (func == null)
                return;

            var pageIndex = 1;

            while (true)
            {
                var item = @this.PageList(pageSize, pageIndex, out var recordCount, out var pageCount);

                if (recordCount <= 0)
                    break;

                var res = await func(pageIndex, item).ConfigureAwait(false);
                if (!res)
                    break;

                if (pageCount <= pageIndex)
                    break;

                pageIndex++;
            }
        }

        /// <summary>
        /// 集合分页循环处理每页数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="func">自定义页数据处理委托</param>
        public static async Task PageEachAsync<T>(this IEnumerable<T> @this, int pageSize, Func<int, IList<T>, Task> func)
        {
            if (func == null)
                return;

            var pageIndex = 1;

            while (true)
            {
                var item = @this.PageList(pageSize, pageIndex, out var recordCount, out var pageCount);

                if (recordCount <= 0)
                    break;

                await func(pageIndex, item).ConfigureAwait(false);

                if (pageCount <= pageIndex)
                    break;

                pageIndex++;
            }
        }

        /// <summary>
        /// 集合分页循环处理每页数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="func">自定义页数据处理委托，当结果为false时跳出循环</param>
        public static async Task PageEachAsync<T>(this IEnumerable<T> @this, int pageSize, Func<int, IList<T>, Task<bool>> func)
        {
            if (func == null)
                return;

            var pageIndex = 1;

            while (true)
            {
                var item = @this.PageList(pageSize, pageIndex, out var recordCount, out var pageCount);

                if (recordCount <= 0)
                    break;

                var res = await func(pageIndex, item).ConfigureAwait(false);
                if (!res)
                    break;

                if (pageCount <= pageIndex)
                    break;

                pageIndex++;
            }
        }
        #endregion
        #endregion
    }
}
