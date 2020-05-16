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

using Dapper;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Common;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
/****************************
* [Author] 张强
* [Date] 2015-10-26
* [Describe] Dictionary扩展类
* **************************/
namespace ZqUtils.Extensions
{
    /// <summary>
    /// Dictionary扩展类
    /// </summary>
    public static partial class Extensions
    {
        #region OrdinalComparer
        /// <summary>
        /// ASCII值排序
        /// </summary>
        public class OrdinalComparer : IComparer<object>
        {
            /// <summary>
            /// ASCII比较方法
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            public int Compare(object x, object y)
            {
                return string.CompareOrdinal(x?.ToString(), y?.ToString());
            }
        }
        #endregion

        #region ToUrl
        /// <summary>
        /// 字典型数据转换为URL参数字符串
        /// </summary>
        /// <param name="this">字典数据源</param>
        /// <param name="seed">起点初始字符串，默认为空字符串</param>
        /// <param name="isAsciiSort">是否ASCII排序，默认升序排序</param>
        /// <param name="isRemoveNull">是否移除空值，默认移除</param>
        /// <returns>string</returns>
        public static string ToUrl<T, S>(this IDictionary<T, S> @this, string seed = "", bool isAsciiSort = true, bool isRemoveNull = true)
        {
            //移除所有空值参数
            if (isRemoveNull)
            {
                @this = @this.Where(o => o.Value?.ToString().IsNullOrEmpty() == false).ToDictionary(o => o.Key, o => o.Value);
            }
            //对参数键值对进行ASCII升序排序
            if (isAsciiSort)
            {
                //C#默认的不是ASCII排序，正确的ASCII排序规则：数字、大写字母、小写字母的顺序
                @this = @this.OrderBy(o => o.Key, new OrdinalComparer()).ToDictionary(o => o.Key, o => o.Value);
            }
            //拼接url
            return @this.Aggregate(seed, _ => $"{_.Key}={_.Value?.ToString()}&", "&");
        }
        #endregion

        #region ToXml
        /// <summary>
        /// 字典型数据转换成xml数据
        /// </summary>
        /// <param name="this">字典数据源</param>
        ///  <param name="isRemoveNull">是否移除空值，默认移除</param>
        /// <returns>string</returns>
        public static string ToXml<T, S>(this IDictionary<T, S> @this, bool isRemoveNull = true)
        {
            //移除所有空值参数
            if (isRemoveNull)
            {
                @this = @this.Where(o =>
                {
                    if (o.Value.GetType() == typeof(string))
                    {
                        return (o.Value as string)?.IsNull() == false;
                    }
                    else
                    {
                        return o.Value != null;
                    }
                }).ToDictionary(o => o.Key, p => p.Value);
            }
            //拼接xml数据
            var xml = new StringBuilder("<xml>");
            foreach (var i in @this)
            {
                if (i.Value.GetType() == typeof(int))
                {
                    //整型
                    xml.Append($"<{i.Key}>{i.Value}</{i.Key}>");
                }
                else
                {
                    //字符串
                    xml.Append($"<{i.Key}><![CDATA[{i.Value}]]></{i.Key}>");
                }
            }
            xml.Append("</xml>");
            return xml.ToString();
        }
        #endregion

        #region ToEntity
        /// <summary>
        /// IDictionary数据转为强类型实体
        /// </summary>
        /// <param name="this">IDictionary数据源</param>
        /// <returns>强类型实体</returns>
        public static T ToEntity<T>(this IDictionary<string, object> @this) where T : class, new()
        {
            if (@this?.Count > 0)
            {
                var fields = new List<string>();
                for (int i = 0; i < @this.Keys.Count; i++)
                {
                    fields.Add(@this.Keys.ElementAt(i));
                }
                var instance = Activator.CreateInstance<T>();
                var props = instance.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance);
                foreach (var p in props)
                {
                    if (!p.CanWrite) continue;
                    var field = fields.Where(o => o.ToLower() == p.Name.ToLower()).FirstOrDefault();
                    if (!field.IsNullOrEmpty() && !@this[field].IsNull())
                    {
                        p.SetValue(instance, @this[field].ToSafeValue(p.PropertyType), null);
                    }
                }
                return instance;
            }
            return default(T);
        }
        #endregion

        #region ToExpando
        /// <summary>
        /// 字典类型转为ExpandoObject
        /// Snagged from http://theburningmonk.com/2011/05/idictionarystring-object-to-expandoobject-extension-method/
        /// </summary>
        /// <param name="this">字典类型数据</param>
        /// <returns></returns>
        public static ExpandoObject ToExpando(this IDictionary<string, object> @this)
        {
            var expando = new ExpandoObject();
            var expandoDic = (IDictionary<string, object>)expando;
            // go through the items in the dictionary and copy over the key value pairs)
            foreach (var kvp in @this)
            {
                // if the value can also be turned into an ExpandoObject, then do it!
                if (kvp.Value is IDictionary<string, object>)
                {
                    var expandoValue = ((IDictionary<string, object>)kvp.Value).ToExpando();
                    expandoDic.Add(kvp.Key, expandoValue);
                }
                else if (kvp.Value is ICollection)
                {
                    // iterate through the collection and convert any strin-object dictionaries
                    // along the way into expando objects
                    var itemList = new List<object>();
                    foreach (var item in (ICollection)kvp.Value)
                    {
                        if (item is IDictionary<string, object>)
                        {
                            var expandoItem = ((IDictionary<string, object>)item).ToExpando();
                            itemList.Add(expandoItem);
                        }
                        else
                        {
                            itemList.Add(item);
                        }
                    }
                    expandoDic.Add(kvp.Key, itemList);
                }
                else
                {
                    expandoDic.Add(kvp);
                }
            }
            return expando;
        }
        #endregion

        #region TryGetValue
        /// <summary>
        /// This method is used to try to get a value in a dictionary if it does exists.
        /// </summary>
        /// <typeparam name="T">Type of the value</typeparam>
        /// <param name="this">The collection object</param>
        /// <param name="key">Key</param>
        /// <param name="value">Value of the key (or default value if key not exists)</param>
        /// <returns>True if key does exists in the dictionary</returns>
        internal static bool TryGetValue<T>(this IDictionary<string, object> @this, string key, out T value)
        {
            if (@this.TryGetValue(key, out object valueObj) && valueObj is T)
            {
                value = (T)valueObj;
                return true;
            }
            value = default(T);
            return false;
        }
        #endregion

        #region GetOrDefault
        /// <summary>
        /// Gets a value from the dictionary with given key. Returns default value if can not find.
        /// </summary>
        /// <param name="this">Dictionary to check and get</param>
        /// <param name="key">Key to find the value</param>
        /// <typeparam name="TKey">Type of the key</typeparam>
        /// <typeparam name="TValue">Type of the value</typeparam>
        /// <returns>Value if found, default if can not found.</returns>
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key)
        {
            return @this.TryGetValue(key, out TValue obj) ? obj : default(TValue);
        }
        #endregion

        #region GetOrAdd
        /// <summary>
        /// Gets a value from the dictionary with given key. Returns default value if can not find.
        /// </summary>
        /// <param name="this">Dictionary to check and get</param>
        /// <param name="key">Key to find the value</param>
        /// <param name="factory">A factory method used to create the value if not found in the dictionary</param>
        /// <typeparam name="TKey">Type of the key</typeparam>
        /// <typeparam name="TValue">Type of the value</typeparam>
        /// <returns>Value if found, default if can not found.</returns>
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, Func<TKey, TValue> factory)
        {
            if (@this.TryGetValue(key, out TValue obj))
            {
                return obj;
            }
            return @this[key] = factory(key);
        }

        /// <summary>
        /// Gets a value from the dictionary with given key. Returns default value if can not find.
        /// </summary>
        /// <param name="this">Dictionary to check and get</param>
        /// <param name="key">Key to find the value</param>
        /// <param name="factory">A factory method used to create the value if not found in the dictionary</param>
        /// <typeparam name="TKey">Type of the key</typeparam>
        /// <typeparam name="TValue">Type of the value</typeparam>
        /// <returns>Value if found, default if can not found.</returns>
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, Func<TValue> factory)
        {
            return @this.GetOrAdd(key, k => factory());
        }

        /// <summary>
        ///     Adds a key/value pair to the IDictionary&lt;TKey, TValue&gt; if the key does not already exist.
        /// </summary>
        /// <typeparam name="TKey">Type of the key.</typeparam>
        /// <typeparam name="TValue">Type of the value.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value to be added, if the key does not already exist.</param>
        /// <returns>
        ///     The value for the key. This will be either the existing value for the key if the key is already in the
        ///     dictionary, or the new value if the key was not in the dictionary.
        /// </returns>
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, TValue value)
        {
            if (!@this.ContainsKey(key))
            {
                @this.Add(new KeyValuePair<TKey, TValue>(key, value));
            }

            return @this[key];
        }
        #endregion

        #region ToNameValueCollection
        /// <summary>
        /// Converts an <see cref="IDictionary{TKey,TValue}"/> instance to a <see cref="NameValueCollection"/> instance.
        /// </summary>
        /// <param name="this">The <see cref="IDictionary{TKey,TValue}"/> instance to convert.</param>
        /// <returns>A <see cref="NameValueCollection"/> instance.</returns>
        public static NameValueCollection ToNameValueCollection(this IDictionary<string, IEnumerable<string>> @this)
        {
            var collection = new NameValueCollection();
            foreach (var key in @this.Keys)
            {
                foreach (var value in @this[key])
                {
                    collection.Add(key, value);
                }
            }
            return collection;
        }
        #endregion

        #region ToDbParameters
        /// <summary>
        /// An IDictionary&lt;string,object&gt; extension method that converts this object to a database parameters.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="command">The command.</param>        
        /// <returns>The given data converted to a DbParameter[].</returns>
        public static DbParameter[] ToDbParameters(this IDictionary<string, object> @this, DbCommand command)
        {
            if (@this?.Count > 0)
            {
                return @this.Select(x =>
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = x.Key;
                    parameter.Value = x.Value;
                    return parameter;
                }).ToArray();
            }
            return null;
        }

        /// <summary>
        ///  An IDictionary&lt;string,object&gt; extension method that converts this object to a database parameters.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="connection">The connection.</param>
        /// <returns>The given data converted to a DbParameter[].</returns>
        public static DbParameter[] ToDbParameters(this IDictionary<string, object> @this, DbConnection connection)
        {
            if (@this?.Count > 0)
            {
                var command = connection.CreateCommand();
                return @this.Select(x =>
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = x.Key;
                    parameter.Value = x.Value;
                    return parameter;
                }).ToArray();
            }
            return null;
        }
        #endregion

        #region ToSqlParameters
        /// <summary>
        /// An IDictionary&lt;string,object&gt; extension method that converts the @this to a SQL parameters.
        /// </summary>
        /// <param name="this">The @this to act on.</param>        
        /// <returns>@this as a SqlParameter[].</returns>
        public static SqlParameter[] ToSqlParameters(this IDictionary<string, object> @this)
        {
            if (@this?.Count > 0)
            {
                return @this.Select(x => new SqlParameter(x.Key.Replace("?", "@").Replace(":", "@"), x.Value)).ToArray();
            }
            return null;
        }
        #endregion

        #region ToHashtable
        /// <summary>
        /// An IDictionary extension method that converts the @this to a hashtable.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as a Hashtable.</returns>
        public static Hashtable ToHashtable(this IDictionary @this)
        {
            return new Hashtable(@this);
        }
        #endregion

        #region AddIfNotContainsKey
        /// <summary>
        /// An IDictionary&lt;TKey,TValue&gt; extension method that adds if not contains key.
        /// </summary>
        /// <typeparam name="TKey">Type of the key.</typeparam>
        /// <typeparam name="TValue">Type of the value.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>true if it succeeds, false if it fails.</returns>
        public static bool AddIfNotContainsKey<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, TValue value)
        {
            if (!@this.ContainsKey(key))
            {
                @this.Add(key, value);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     An IDictionary&lt;TKey,TValue&gt; extension method that adds if not contains key.
        /// </summary>
        /// <typeparam name="TKey">Type of the key.</typeparam>
        /// <typeparam name="TValue">Type of the value.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="key">The key.</param>
        /// <param name="valueFactory">The value factory.</param>
        /// <returns>true if it succeeds, false if it fails.</returns>
        public static bool AddIfNotContainsKey<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, Func<TValue> valueFactory)
        {
            if (!@this.ContainsKey(key))
            {
                @this.Add(key, valueFactory());
                return true;
            }

            return false;
        }

        /// <summary>
        ///     An IDictionary&lt;TKey,TValue&gt; extension method that adds if not contains key.
        /// </summary>
        /// <typeparam name="TKey">Type of the key.</typeparam>
        /// <typeparam name="TValue">Type of the value.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="key">The key.</param>
        /// <param name="valueFactory">The value factory.</param>
        /// <returns>true if it succeeds, false if it fails.</returns>
        public static bool AddIfNotContainsKey<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, Func<TKey, TValue> valueFactory)
        {
            if (!@this.ContainsKey(key))
            {
                @this.Add(key, valueFactory(key));
                return true;
            }

            return false;
        }
        #endregion

        #region AddOrUpdate
        /// <summary>
        /// Uses the specified functions to add a key/value pair to the IDictionary&lt;TKey, TValue&gt; if the key does
        /// not already exist, or to update a key/value pair in the IDictionary&lt;TKey, TValue&gt;> if the key already
        /// exists.
        /// </summary>
        /// <typeparam name="TKey">Type of the key.</typeparam>
        /// <typeparam name="TValue">Type of the value.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="key">The key to be added or whose value should be updated.</param>
        /// <param name="value">The value to be added or updated.</param>
        /// <returns>The new value for the key.</returns>
        public static TValue AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, TValue value)
        {
            if (!@this.ContainsKey(key))
            {
                @this.Add(new KeyValuePair<TKey, TValue>(key, value));
            }
            else
            {
                @this[key] = value;
            }

            return @this[key];
        }

        /// <summary>
        /// Uses the specified functions to add a key/value pair to the IDictionary&lt;TKey, TValue&gt; if the key does
        /// not already exist, or to update a key/value pair in the IDictionary&lt;TKey, TValue&gt;> if the key already
        /// exists.
        /// </summary>
        /// <typeparam name="TKey">Type of the key.</typeparam>
        /// <typeparam name="TValue">Type of the value.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="key">The key to be added or whose value should be updated.</param>
        /// <param name="addValue">The value to be added for an absent key.</param>
        /// <param name="updateValueFactory">
        /// The function used to generate a new value for an existing key based on the key's
        /// existing value.
        /// </param>
        /// <returns>
        /// The new value for the key. This will be either be addValue (if the key was absent) or the result of
        /// updateValueFactory (if the key was present).
        /// </returns>
        public static TValue AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            if (!@this.ContainsKey(key))
            {
                @this.Add(new KeyValuePair<TKey, TValue>(key, addValue));
            }
            else
            {
                @this[key] = updateValueFactory(key, @this[key]);
            }

            return @this[key];
        }

        /// <summary>
        /// Uses the specified functions to add a key/value pair to the IDictionary&lt;TKey, TValue&gt; if the key does
        /// not already exist, or to update a key/value pair in the IDictionary&lt;TKey, TValue&gt;> if the key already
        /// exists.
        /// </summary>
        /// <typeparam name="TKey">Type of the key.</typeparam>
        /// <typeparam name="TValue">Type of the value.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="key">The key to be added or whose value should be updated.</param>
        /// <param name="addValueFactory">The function used to generate a value for an absent key.</param>
        /// <param name="updateValueFactory">
        /// The function used to generate a new value for an existing key based on the key's
        /// existing value.
        /// </param>
        /// <returns>
        /// The new value for the key. This will be either be the result of addValueFactory (if the key was absent) or
        /// the result of updateValueFactory (if the key was present).
        /// </returns>
        public static TValue AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            if (!@this.ContainsKey(key))
            {
                @this.Add(new KeyValuePair<TKey, TValue>(key, addValueFactory(key)));
            }
            else
            {
                @this[key] = updateValueFactory(key, @this[key]);
            }

            return @this[key];
        }
        #endregion

        #region ContainsAllKey
        /// <summary>
        /// An IDictionary&lt;TKey,TValue&gt; extension method that query if '@this' contains all key.
        /// </summary>
        /// <typeparam name="TKey">Type of the key.</typeparam>
        /// <typeparam name="TValue">Type of the value.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="keys">A variable-length parameters list containing keys.</param>
        /// <returns>true if it succeeds, false if it fails.</returns>
        public static bool ContainsAllKey<TKey, TValue>(this IDictionary<TKey, TValue> @this, params TKey[] keys)
        {
            foreach (TKey value in keys)
            {
                if (!@this.ContainsKey(value))
                {
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region ContainsAnyKey
        /// <summary>
        /// An IDictionary&lt;TKey,TValue&gt; extension method that query if '@this' contains any key.
        /// </summary>
        /// <typeparam name="TKey">Type of the key.</typeparam>
        /// <typeparam name="TValue">Type of the value.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="keys">A variable-length parameters list containing keys.</param>
        /// <returns>true if it succeeds, false if it fails.</returns>
        public static bool ContainsAnyKey<TKey, TValue>(this IDictionary<TKey, TValue> @this, params TKey[] keys)
        {
            foreach (TKey value in keys)
            {
                if (@this.ContainsKey(value))
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region RemoveIfContainsKey
        /// <summary>
        /// An IDictionary&lt;TKey,TValue&gt; extension method that removes if contains key.
        /// </summary>
        /// <typeparam name="TKey">Type of the key.</typeparam>
        /// <typeparam name="TValue">Type of the value.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="key">The key.</param>
        public static void RemoveIfContainsKey<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key)
        {
            if (@this.ContainsKey(key))
            {
                @this.Remove(key);
            }
        }
        #endregion

        #region ToSortedDictionary
        /// <summary>
        /// An IDictionary&lt;TKey,TValue&gt; extension method that converts the @this to a sorted dictionary.
        /// </summary>
        /// <typeparam name="TKey">Type of the key.</typeparam>
        /// <typeparam name="TValue">Type of the value.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as a SortedDictionary&lt;TKey,TValue&gt;</returns>
        public static SortedDictionary<TKey, TValue> ToSortedDictionary<TKey, TValue>(this IDictionary<TKey, TValue> @this)
        {
            return new SortedDictionary<TKey, TValue>(@this);
        }

        /// <summary>
        /// An IDictionary&lt;TKey,TValue&gt; extension method that converts the @this to a sorted dictionary.
        /// </summary>
        /// <typeparam name="TKey">Type of the key.</typeparam>
        /// <typeparam name="TValue">Type of the value.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="comparer">The comparer.</param>
        /// <returns>@this as a SortedDictionary&lt;TKey,TValue&gt;</returns>
        public static SortedDictionary<TKey, TValue> ToSortedDictionary<TKey, TValue>(this IDictionary<TKey, TValue> @this, IComparer<TKey> comparer)
        {
            return new SortedDictionary<TKey, TValue>(@this, comparer);
        }
        #endregion        

        #region ToDynamicParameters
        /// <summary>
        ///  IDictionary转换为DynamicParameters
        /// </summary>
        /// <param name="this"></param>        
        /// <returns></returns>
        public static DynamicParameters ToDynamicParameters(this IDictionary<string, object> @this)
        {
            if (@this?.Count > 0)
            {
                var args = new DynamicParameters();
                foreach (var item in @this)
                {
                    args.Add(item.Key, item.Value);
                }
                return args;
            }
            return null;
        }
        #endregion

        #region ToKeyArray
        /// <summary>
        /// 集合转为数组
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="this"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static IList<TKey> ToKeyArray<TKey, TValue>(this IDictionary<TKey, TValue> @this, int index = 0)
        {
            if (@this == null) return null;
            if (@this is ConcurrentDictionary<TKey, TValue> cdiv) return cdiv.Keys as IList<TKey>;
            if (@this.Count == 0) return new TKey[0];
            lock (@this)
            {
                var arr = new TKey[@this.Count - index];
                @this.Keys.CopyTo(arr, index);
                return arr;
            }
        }
        #endregion

        #region ToValueArray
        /// <summary>
        /// 集合转为数组
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="this"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static IList<TValue> ToValueArray<TKey, TValue>(this IDictionary<TKey, TValue> @this, int index = 0)
        {
            if (@this == null) return null;
            if (@this is ConcurrentDictionary<TKey, TValue> cdiv) return cdiv.Values as IList<TValue>;
            if (@this.Count == 0) return new TValue[0];
            lock (@this)
            {
                var arr = new TValue[@this.Count - index];
                @this.Values.CopyTo(arr, index);
                return arr;
            }
        }
        #endregion
    }
}
