#region License
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
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using ZqUtils.FastMember;
/****************************
* [Author] 张强
* [Date] 2018-05-03
* [Describe] IDataReader扩展类
* **************************/
namespace ZqUtils.Extensions
{
    /// <summary>
    /// IDataReader扩展类
    /// </summary>
    public static class IDataReaderExtensions
    {
        #region ToDynamic
        /// <summary>
        /// IDataReader数据转为dynamic对象
        /// </summary>
        /// <param name="this">IDataReader数据源</param>
        /// <returns>dynamic</returns>
        public static dynamic ToDynamic(this IDataReader @this)
        {
            return @this.ToDynamics()?.FirstOrDefault();
        }

        /// <summary>
        /// IDataReader数据转为dynamic对象集合
        /// </summary>
        /// <param name="this">IDataReader数据源</param>
        /// <returns>dynamic集合</returns>
        public static IEnumerable<dynamic> ToDynamics(this IDataReader @this)
        {
            var res = new List<dynamic>();

            if (@this.IsNull() || @this.IsClosed || @this.FieldCount == 0)
                return res;

            using (@this)
            {
                while (@this.Read())
                {
                    var row = new Dictionary<string, object>();

                    for (var i = 0; i < @this.FieldCount; i++)
                        row.Add(@this.GetName(i), @this.GetValue(i));

                    res.Add(row);
                }
            }

            return res;
        }
        #endregion

        #region ToDictionary
        /// <summary>
        /// IDataReader数据转为Dictionary对象
        /// </summary>
        /// <param name="this">IDataReader数据源</param>
        /// <returns>Dictionary</returns>
        public static Dictionary<string, object> ToDictionary(this IDataReader @this)
        {
            return @this.ToDictionaries()?.FirstOrDefault();
        }

        /// <summary>
        /// IDataReader数据转为Dictionary对象集合
        /// </summary>
        /// <param name="this">IDataReader数据源</param>
        /// <returns>Dictionary集合</returns>
        public static IEnumerable<Dictionary<string, object>> ToDictionaries(this IDataReader @this)
        {
            if (@this.IsNull() || @this.IsClosed || @this.FieldCount == 0)
                yield break;

            using (@this)
            {
                while (@this.Read())
                {
                    var dic = new Dictionary<string, object>();
                    for (var i = 0; i < @this.FieldCount; i++)
                        dic[@this.GetName(i)] = @this.GetValue(i);

                    yield return dic;
                }
            }
        }
        #endregion

        #region ToEntity
        /// <summary>
        /// IDataReader数据转为强类型实体
        /// </summary>
        /// <param name="this">IDataReader数据源</param>
        /// <returns>强类型实体</returns>
        public static T ToEntity<T>(this IDataReader @this)
        {
            var result = @this.ToEntities<T>();
            if (result != null)
            {
                return result.FirstOrDefault();
            }
            return default;
        }

        /// <summary>
        /// IDataReader数据转为强类型实体集合
        /// </summary>
        /// <param name="this">IDataReader数据源</param>
        /// <returns>强类型实体集合</returns>
        public static IEnumerable<T> ToEntities<T>(this IDataReader @this)
        {
            if (@this.IsNull() || @this.IsClosed || @this.FieldCount == 0)
                yield break;

            using (@this)
            {
                var fields = new List<string>();
                for (int i = 0; i < @this.FieldCount; i++)
                    fields.Add(@this.GetName(i));

                var accessor = TypeAccessor.Create(typeof(T));
                var members = accessor.GetMembers();

                if (members.IsNullOrEmpty())
                    yield break;

                var memberMap = members.ToDictionary(k => k.Name, v => v, StringComparer.OrdinalIgnoreCase);

                while (@this.Read())
                {
                    if (accessor.CreateNew() is T instance)
                    {
                        foreach (var field in fields)
                        {
                            if (field.IsNullOrEmpty())
                                continue;

                            var fieldValue = @this[field];

                            if (fieldValue.IsNull())
                                continue;

                            if (memberMap.TryGetValue(field, out var member))
                            {
                                if (!member.CanWrite)
                                    continue;

                                accessor[instance, member.Name] = fieldValue.ToSafeValue(member.Type);
                            }
                        }

                        yield return instance;
                    }
                }
            }
        }
        #endregion

        #region ToList
        /// <summary>
        /// IDataReader转换为T集合
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="this">IDataReader数据源</param>
        /// <returns>T类型集合</returns>
        public static List<T> ToList<T>(this IDataReader @this)
        {
            if (@this.IsNull() || @this.IsClosed || @this.FieldCount == 0)
                return default;

            List<T> list = null;
            var type = typeof(T);
            if (type.AssignableTo(typeof(Dictionary<,>)))
                list = @this.ToDictionaries()?.ToList() as List<T>;

            else if (type.AssignableTo(typeof(IDictionary<,>)))
                list = @this.ToDictionaries()?.Select(o => o as IDictionary<string, object>).ToList() as List<T>;

            else if (type.IsClass && !type.IsDynamicOrObjectType() && !type.IsStringType())
                list = @this.ToEntities<T>()?.ToList() as List<T>;

            else
            {
                var result = @this.ToDynamics();
                if (result != null && result.Any())
                {
                    list = result.ToList() as List<T>;
                    if (list == null && (type.IsStringType() || type.IsValueType))
                        //适合查询单个字段的结果集
                        list = result.Select(o => (T)(o as IDictionary<string, object>).Select(x => x.Value).FirstOrDefault()).ToList();
                }
            }

            return list;
        }

        /// <summary>
        /// IDataReader转换为T集合的集合
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="this">IDataReader数据源</param>
        /// <returns>T类型集合的集合</returns>
        public static List<List<T>> ToLists<T>(this IDataReader @this)
        {
            var result = new List<List<T>>();

            if (@this.IsNull() || @this.IsClosed || @this.FieldCount == 0)
                return result;

            using (@this)
            {
                var type = typeof(T);
                do
                {
                    #region IDictionary
                    if (type.IsDictionaryType())
                    {
                        var list = new List<Dictionary<string, object>>();
                        while (@this.Read())
                        {
                            var dic = new Dictionary<string, object>();
                            for (var i = 0; i < @this.FieldCount; i++)
                            {
                                dic[@this.GetName(i)] = @this.GetValue(i);
                            }
                            list.Add(dic);
                        }
                        if (!type.AssignableTo(typeof(Dictionary<,>)))
                        {
                            result.Add(list.Select(o => o as IDictionary<string, object>).ToList() as List<T>);
                        }
                        else
                        {
                            result.Add(list as List<T>);
                        }
                    }
                    #endregion

                    #region Class T
                    else if (type.IsClass && !type.IsDynamicOrObjectType() && !type.IsStringType())
                    {
                        var list = new List<T>();
                        var fields = new List<string>();
                        for (int i = 0; i < @this.FieldCount; i++)
                            fields.Add(@this.GetName(i));

                        var accessor = TypeAccessor.Create(type);
                        var members = accessor.GetMembers();

                        if (members.IsNullOrEmpty())
                            return result;

                        var memberMap = members.ToDictionary(k => k.Name, v => v, StringComparer.OrdinalIgnoreCase);

                        while (@this.Read())
                        {
                            if (accessor.CreateNew() is T instance)
                            {
                                foreach (var field in fields)
                                {
                                    if (field.IsNullOrEmpty())
                                        continue;

                                    var fieldValue = @this[field];

                                    if (fieldValue.IsNull())
                                        continue;

                                    if (memberMap.TryGetValue(field, out var member))
                                    {
                                        if (!member.CanWrite)
                                            continue;

                                        accessor[instance, member.Name] = fieldValue.ToSafeValue(member.Type);
                                    }
                                }

                                list.Add(instance);
                            }
                        }

                        result.Add(list);
                    }
                    #endregion

                    #region dynamic
                    else
                    {
                        var list = new List<dynamic>();
                        while (@this.Read())
                        {
                            var row = new ExpandoObject() as IDictionary<string, object>;
                            for (var i = 0; i < @this.FieldCount; i++)
                            {
                                row.Add(@this.GetName(i), @this.GetValue(i));
                            }
                            list.Add(row);
                        }
                        var item = list as List<T>;
                        if (item == null && (type.IsStringType() || type.IsValueType()))
                        {
                            //适合查询单个字段的结果集
                            item = list.Select(o => (T)(o as IDictionary<string, object>).Select(x => x.Value).FirstOrDefault()).ToList();
                        }
                        result.Add(item);
                    }
                    #endregion
                } while (@this.NextResult());
            }

            return result;
        }
        #endregion

        #region ToFirstOrDefault
        /// <summary>
        /// IDataReader转换为T类型对象
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="this">IDataReader数据源</param>
        /// <returns>T类型对象</returns>
        public static T ToFirstOrDefault<T>(this IDataReader @this)
        {
            var list = @this.ToList<T>();
            if (list != null)
            {
                return list.FirstOrDefault();
            }
            return default;
        }
        #endregion

        #region ToDataTable
        /// <summary>
        /// IDataReader转换为DataTable
        /// </summary>
        /// <param name="this">reader数据源</param>
        /// <returns>DataTable</returns>
        public static DataTable ToDataTable(this IDataReader @this)
        {
            var table = new DataTable();

            if (@this.IsNull() || @this.IsClosed || @this.FieldCount == 0)
                return table;

            using (@this)
            {
                table.Load(@this);
            }

            return table;
        }
        #endregion

        #region ToDataSet
        /// <summary>
        /// IDataReader转换为DataSet
        /// </summary>
        /// <param name="this">reader数据源</param>
        /// <returns>DataSet</returns>
        public static DataSet ToDataSet(this IDataReader @this)
        {
            var ds = new DataSet();

            if (@this.IsNull() || @this.IsClosed || @this.FieldCount == 0)
                return ds;

            using (@this)
            {
                do
                {
                    var schemaTable = @this.GetSchemaTable();
                    var dt = new DataTable();
                    for (var i = 0; i < schemaTable.Rows.Count; i++)
                    {
                        var row = schemaTable.Rows[i];
                        dt.Columns.Add(new DataColumn((string)row["ColumnName"], (Type)row["DataType"]));
                    }
                    while (@this.Read())
                    {
                        var dataRow = dt.NewRow();
                        for (var i = 0; i < @this.FieldCount; i++)
                            dataRow[i] = @this.GetValue(i);
                        dt.Rows.Add(dataRow);
                    }
                    ds.Tables.Add(dt);
                }
                while (@this.NextResult());
            }

            return ds;
        }
        #endregion

        #region ContainsColumn
        /// <summary>
        /// An IDataReader extension method that query if '@this' contains column.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="columnIndex">Zero-based index of the column.</param>
        /// <returns>true if it succeeds, false if it fails.</returns>
        public static bool ContainsColumn(this IDataReader @this, int columnIndex)
        {
            try
            {
                // Check if FieldCount is implemented first
                return @this.FieldCount > columnIndex;
            }
            catch (Exception)
            {
                try
                {
                    return @this[columnIndex] != null;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        /// <summary>
        ///  An IDataReader extension method that query if '@this' contains column.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <returns>true if it succeeds, false if it fails.</returns>
        public static bool ContainsColumn(this IDataReader @this, string columnName)
        {
            try
            {
                // Check if GetOrdinal is implemented first
                return @this.GetOrdinal(columnName) != -1;
            }
            catch (Exception)
            {
                try
                {
                    return @this[columnName] != null;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
        #endregion

        #region ForEach
        /// <summary>
        /// An IDataReader extension method that applies an operation to all items in this collection.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="action">The action.</param>
        /// <returns>An IDataReader.</returns>
        public static IDataReader ForEach(this IDataReader @this, Action<IDataReader> action)
        {
            while (@this.Read())
            {
                action(@this);
            }
            return @this;
        }
        #endregion

        #region GetColumnNames
        /// <summary>
        /// Gets the column names in this collection.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>An enumerator that allows foreach to be used to get the column names in this collection.</returns>
        public static IEnumerable<string> GetColumnNames(this IDataRecord @this)
        {
            return
                Enumerable
                .Range(0, @this.FieldCount)
                .Select(@this.GetName)
                .ToList();
        }
        #endregion

        #region GetValueAs
        /// <summary>
        /// An IDataReader extension method that gets value as.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="index">Zero-based index of the.</param>
        /// <returns>The value as.</returns>
        public static T GetValueAs<T>(this IDataReader @this, int index)
        {
            return (T)@this.GetValue(index);
        }

        /// <summary>
        /// An IDataReader extension method that gets value as.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <returns>The value as.</returns>
        public static T GetValueAs<T>(this IDataReader @this, string columnName)
        {
            return (T)@this.GetValue(@this.GetOrdinal(columnName));
        }
        #endregion

        #region GetValueAsOrDefault
        /// <summary>
        /// An IDataReader extension method that gets value as or default.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="index">Zero-based index of the.</param>
        /// <returns>The value as or default.</returns>
        public static T GetValueAsOrDefault<T>(this IDataReader @this, int index)
        {
            try
            {
                return (T)@this.GetValue(index);
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// An IDataReader extension method that gets value as or default.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="index">Zero-based index of the.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value as or default.</returns>
        public static T GetValueAsOrDefault<T>(this IDataReader @this, int index, T defaultValue)
        {
            try
            {
                return (T)@this.GetValue(index);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An IDataReader extension method that gets value as or default.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="index">Zero-based index of the.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The value as or default.</returns>
        public static T GetValueAsOrDefault<T>(this IDataReader @this, int index, Func<IDataReader, int, T> defaultValueFactory)
        {
            try
            {
                return (T)@this.GetValue(index);
            }
            catch
            {
                return defaultValueFactory(@this, index);
            }
        }

        /// <summary>
        /// An IDataReader extension method that gets value as or default.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <returns>The value as or default.</returns>
        public static T GetValueAsOrDefault<T>(this IDataReader @this, string columnName)
        {
            try
            {
                return (T)@this.GetValue(@this.GetOrdinal(columnName));
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// An IDataReader extension method that gets value as or default.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value as or default.</returns>
        public static T GetValueAsOrDefault<T>(this IDataReader @this, string columnName, T defaultValue)
        {
            try
            {
                return (T)@this.GetValue(@this.GetOrdinal(columnName));
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An IDataReader extension method that gets value as or default.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The value as or default.</returns>
        public static T GetValueAsOrDefault<T>(this IDataReader @this, string columnName, Func<IDataReader, string, T> defaultValueFactory)
        {
            try
            {
                return (T)@this.GetValue(@this.GetOrdinal(columnName));
            }
            catch
            {
                return defaultValueFactory(@this, columnName);
            }
        }
        #endregion

        #region GetValueTo
        /// <summary>
        /// An IDataReader extension method that gets value to.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="index">Zero-based index of the.</param>
        /// <returns>The value to.</returns>
        public static T GetValueTo<T>(this IDataReader @this, int index)
        {
            return @this.GetValue(index).To<T>();
        }

        /// <summary>
        /// An IDataReader extension method that gets value to.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <returns>The value to.</returns>
        public static T GetValueTo<T>(this IDataReader @this, string columnName)
        {
            return @this.GetValue(@this.GetOrdinal(columnName)).To<T>();
        }
        #endregion

        #region GetValueToOrDefault
        /// <summary>
        /// An IDataReader extension method that gets value to or default.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="index">Zero-based index of the.</param>
        /// <returns>The value to or default.</returns>
        public static T GetValueToOrDefault<T>(this IDataReader @this, int index)
        {
            try
            {
                return @this.GetValue(index).To<T>();
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// An IDataReader extension method that gets value to or default.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="index">Zero-based index of the.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value to or default.</returns>
        public static T GetValueToOrDefault<T>(this IDataReader @this, int index, T defaultValue)
        {
            try
            {
                return @this.GetValue(index).To<T>();
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An IDataReader extension method that gets value to or default.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="index">Zero-based index of the.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The value to or default.</returns>
        public static T GetValueToOrDefault<T>(this IDataReader @this, int index, Func<IDataReader, int, T> defaultValueFactory)
        {
            try
            {
                return @this.GetValue(index).To<T>();
            }
            catch
            {
                return defaultValueFactory(@this, index);
            }
        }

        /// <summary>
        /// An IDataReader extension method that gets value to or default.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <returns>The value to or default.</returns>
        public static T GetValueToOrDefault<T>(this IDataReader @this, string columnName)
        {
            try
            {
                return @this.GetValue(@this.GetOrdinal(columnName)).To<T>();
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// An IDataReader extension method that gets value to or default.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value to or default.</returns>
        public static T GetValueToOrDefault<T>(this IDataReader @this, string columnName, T defaultValue)
        {
            try
            {
                return @this.GetValue(@this.GetOrdinal(columnName)).To<T>();
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An IDataReader extension method that gets value to or default.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The value to or default.</returns>
        public static T GetValueToOrDefault<T>(this IDataReader @this, string columnName, Func<IDataReader, string, T> defaultValueFactory)
        {
            try
            {
                return @this.GetValue(@this.GetOrdinal(columnName)).To<T>();
            }
            catch
            {
                return defaultValueFactory(@this, columnName);
            }
        }
        #endregion

        #region IsDBNull
        /// <summary>
        /// An IDataReader extension method that query if '@this' is database null.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="name">The name.</param>
        /// <returns>true if database null, false if not.</returns>
        public static bool IsDBNull(this IDataReader @this, string name)
        {
            return @this.IsDBNull(@this.GetOrdinal(name));
        }
        #endregion

        #region ToExpandoObject
        /// <summary>
        /// An IDataReader extension method that converts the @this to an expando object.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as a dynamic.</returns>
        public static dynamic ToExpandoObject(this IDataReader @this)
        {
            Dictionary<int, KeyValuePair<int, string>> columnNames = Enumerable.Range(0, @this.FieldCount)
                .Select(x => new KeyValuePair<int, string>(x, @this.GetName(x)))
                .ToDictionary(pair => pair.Key);

            dynamic entity = new ExpandoObject();
            var expandoDict = (IDictionary<string, object>)entity;

            Enumerable.Range(0, @this.FieldCount)
                .ToList()
                .ForEach(x => expandoDict.Add(columnNames[x].Value, @this[x]));

            return entity;
        }
        #endregion

        #region ToExpandoObjects
        /// <summary>
        /// Enumerates to expando objects in this collection.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as an IEnumerable&lt;dynamic&gt;</returns>
        public static IEnumerable<dynamic> ToExpandoObjects(this IDataReader @this)
        {
            Dictionary<int, KeyValuePair<int, string>> columnNames = Enumerable.Range(0, @this.FieldCount)
                .Select(x => new KeyValuePair<int, string>(x, @this.GetName(x)))
                .ToDictionary(pair => pair.Key);

            var list = new List<dynamic>();

            while (@this.Read())
            {
                dynamic entity = new ExpandoObject();
                var expandoDict = (IDictionary<string, object>)entity;

                Enumerable.Range(0, @this.FieldCount)
                    .ToList()
                    .ForEach(x => expandoDict.Add(columnNames[x].Value, @this[x]));

                list.Add(entity);
            }

            return list;
        }
        #endregion
    }
}
