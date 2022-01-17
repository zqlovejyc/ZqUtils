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
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
/****************************
* [Author] 张强
* [Date] 2016-04-14
* [Describe] DataTable扩展类
* **************************/
namespace ZqUtils.Extensions
{
    /// <summary>
    /// DataTable扩展类
    /// </summary>
    public static class DataTableExtensions
    {
        #region ForEach
        /// <summary>
        /// DataTable遍历
        /// </summary>
        /// <typeparam name="T">DataRow/dynamic/强类型</typeparam>
        /// <param name="this">DataTable数据源</param>
        /// <param name="action">委托</param>
        public static void ForEach<T>(this DataTable @this, Action<T> action) where T : class
        {
            if (typeof(T) == typeof(DataRow))
            {
                @this.AsEnumerable().ForEach(o => action(o.To<T>()));
            }
            else
            {
                @this.ToList<T>().ForEach(action);
            }
        }
        #endregion

        #region AsEnumerable
        /// <summary>
        /// DataTable转换为IEnumerable&lt;DataRow&gt;
        /// </summary>
        /// <param name="this">DataTable数据源</param>
        /// <returns>IEnumerable</returns>
        public static IEnumerable<DataRow> AsEnumerable(this DataTable @this)
        {
            foreach (DataRow dr in @this.Rows)
            {
                yield return dr;
            }
        }
        #endregion

        #region IEnumerable
        /// <summary>
        /// DataTable转换为IEnumerable&lt;dynamic&gt;
        /// </summary>
        /// <param name="this">DataTable数据源</param>
        /// <param name="mappingRow">映射类型，默认：DapperRow</param>
        /// <returns>IEnumerable</returns>
        public static IEnumerable<dynamic> ToIEnumerable(this DataTable @this, MappingRow mappingRow = MappingRow.DapperRow)
        {
            if (mappingRow == MappingRow.DynamicRow)
            {
                return @this?.Rows.Count > 0 ? @this.AsEnumerable().Select(row => new DynamicRow(row)) : null;
            }
            else if (mappingRow == MappingRow.DapperRow)
            {
                var fieldNames = @this.Columns.OfType<DataColumn>().Select(o => o.ColumnName).ToArray();
                return @this?.Rows.Count > 0 ? @this.AsEnumerable().Select(row => new DapperRow(new DapperTable(fieldNames), row.ItemArray)) : null;
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region ToExpandoObjects
        /// <summary>
        /// Enumerates to expando objects in this collection.
        /// </summary>        
        /// <param name="this">The @this to act on.</param>
        /// <returns>
        /// @this as an IEnumerable&lt;dynamic&gt;
        /// </returns>
        public static IEnumerable<dynamic> ToExpandoObjects(this DataTable @this)
        {
            var list = new List<dynamic>();
            foreach (DataRow dr in @this.Rows)
            {
                dynamic entity = new ExpandoObject();
                var expandoDict = (IDictionary<string, object>)entity;
                foreach (DataColumn column in @this.Columns)
                {
                    expandoDict.Add(column.ColumnName, dr[column]);
                }
                list.Add(entity);
            }
            return list;
        }
        #endregion

        #region ToDictionary
        /// <summary>
        /// 转换为字典集合
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static List<Dictionary<string, object>> ToDictionary(this DataTable @this)
        {
            var list = new List<Dictionary<string, object>>();

            if (@this?.Rows.Count > 0)
            {
                foreach (DataRow dr in @this.Rows)
                {
                    var dic = new Dictionary<string, object>();
                    foreach (DataColumn dc in @this.Columns)
                    {
                        dic.Add(dc.ColumnName, dr[dc.ColumnName]);
                    }
                    list.Add(dic);
                }
            }

            return list;
        }

        /// <summary>
        /// 转换为字典集合
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static List<Dictionary<string, T>> ToDictionary<T>(this DataTable @this)
        {
            var list = new List<Dictionary<string, T>>();

            if (@this?.Rows.Count > 0)
            {
                foreach (DataRow dr in @this.Rows)
                {
                    var dic = new Dictionary<string, T>();
                    foreach (DataColumn dc in @this.Columns)
                    {
                        dic.Add(dc.ColumnName, dr[dc.ColumnName].To<T>());
                    }
                    list.Add(dic);
                }
            }

            return list;
        }
        #endregion

        #region ToList
        /// <summary>
        /// DataTable转换强类型List集合
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="this">DataTable数据源</param>
        /// <returns>IList</returns>
        public static IList<T> ToList<T>(this DataTable @this)
        {
            if (@this.IsNull() || @this.Rows.IsNull() || @this.Rows.Count == 0)
                return null;

            var list = new List<T>();
            var type = typeof(T);
            if (type.IsDynamicOrObjectType() || type.IsStringType() || type.IsValueType)
            {
                var result = @this.ToIEnumerable(type == typeof(DynamicRow) ? MappingRow.DynamicRow : MappingRow.DapperRow)?.ToList();
                if (result.IsNotNullOrEmpty())
                {
                    if (type.IsDynamicOrObjectType())
                        list = result as List<T>;
                    else
                        list = result.Select(x => (T)(x as IDictionary<string, object>).Select(x => x.Value).FirstOrDefault()).ToList();
                }
            }
            else if (type.AssignableTo(typeof(Dictionary<,>)))
            {
                list = @this.ToDictionary() as List<T>;
            }
            else if (type.AssignableTo(typeof(IDictionary<,>)))
            {
                var result = @this.ToIEnumerable(type == typeof(DynamicRow) ? MappingRow.DynamicRow : MappingRow.DapperRow)?.ToList();
                if (result.IsNotNullOrEmpty())
                    list = result.Select(x => (T)x).ToList();
            }
            else if (type.IsClass)
            {
                foreach (DataRow row in @this.Rows)
                {
                    list.Add(row.ToEntity<T>());
                }
            }

            return list;
        }
        #endregion

        #region ToEntities
        /// <summary>
        /// Enumerates to entities in this collection.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as an IEnumerable&lt;T&gt;</returns>
        public static IEnumerable<T> ToEntities<T>(this DataTable @this) where T : new()
        {
            var type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            var list = new List<T>();
            foreach (DataRow dr in @this.Rows)
            {
                var entity = new T();
                foreach (PropertyInfo property in properties)
                {
                    if (@this.Columns.Contains(property.Name))
                    {
                        Type valueType = property.PropertyType;
                        property.SetValue(entity, dr[property.Name].To(valueType), null);
                    }
                }
                foreach (FieldInfo field in fields)
                {
                    if (@this.Columns.Contains(field.Name))
                    {
                        Type valueType = field.FieldType;
                        field.SetValue(entity, dr[field.Name].To(valueType));
                    }
                }
                list.Add(entity);
            }
            return list;
        }
        #endregion

        #region ToHashtable
        /// <summary>
        /// DataTable转Hashtable
        /// </summary>
        /// <param name="this">DataTable数据源</param>
        /// <returns>Hashtable</returns>
        public static Hashtable ToHashtable(this DataTable @this)
        {
            Hashtable ht = null;
            if (@this?.Rows.Count > 0)
            {
                ht = new Hashtable();
                foreach (DataRow dr in @this.Rows)
                {
                    for (int i = 0; i < @this.Columns.Count; i++)
                    {
                        var key = @this.Columns[i].ColumnName;
                        ht[key] = dr[key];
                    }
                }
            }
            return ht;
        }
        #endregion

        #region Filter
        /// <summary>
        /// 根据条件过滤表的内容
        /// </summary>
        /// <param name="this">DataTable数据源</param>
        /// <param name="condition">过滤条件</param>
        /// <returns>DataTable</returns>
        public static DataTable Filter(this DataTable @this, string condition)
        {
            if (@this?.Rows.Count > 0 && !string.IsNullOrEmpty(condition))
            {
                var newdt = @this.Clone();
                var dr = @this.Select(condition);
                for (var i = 0; i < dr.Length; i++)
                {
                    newdt.ImportRow(dr[i]);
                }
                @this = newdt;
            }
            return @this;
        }

        /// <summary>
        /// 根据条件过滤表的内容
        /// </summary>
        /// <param name="dt">DataTable数据源</param>
        /// <param name="condition">过滤条件</param>
        /// <param name="sort">排序字段</param>
        /// <returns>DataTable</returns>
        public static DataTable Filter(this DataTable dt, string condition, string sort)
        {
            if (dt?.Rows.Count > 0 && !string.IsNullOrEmpty(condition) && !string.IsNullOrEmpty(sort))
            {
                var newdt = dt.Clone();
                var dr = dt.Select(condition, sort);
                for (var i = 0; i < dr.Length; i++)
                {
                    newdt.ImportRow(dr[i]);
                }
                dt = newdt;
            }
            return dt;
        }
        #endregion

        #region ToXml
        /// <summary>
        /// DataTable转Xml
        /// </summary>
        /// <param name="this">DataTable数据源</param>
        /// <returns>string</returns>
        public static string ToXml(this DataTable @this)
        {
            var result = string.Empty;
            if (@this?.Rows.Count > 0)
            {
                using (var writer = new StringWriter())
                {
                    @this.WriteXml(writer);
                    result = writer.ToString();
                }
            }
            return result;
        }
        #endregion

        #region DynamicRow
        /// <summary>
        /// 自定义动态类
        /// </summary>
        [Serializable]
        public sealed class DynamicRow : DynamicObject, IDictionary<string, object>
        {
            /// <summary>
            /// 私有字段
            /// </summary>
            private readonly DataRow _row;

            /// <summary>
            /// 含参构造函数
            /// </summary>
            /// <param name="row"></param>
            public DynamicRow(DataRow row) => _row = row;

            /// <summary>
            /// 重写ToString()
            /// </summary>
            /// <returns></returns>
            public override string ToString() => this.ToJson();

            #region 实现DynamicObject
            /// <summary>
            /// GetDynamicMemberNames
            /// </summary>
            /// <returns></returns>
            public override IEnumerable<string> GetDynamicMemberNames() => base.GetDynamicMemberNames();

            /// <summary>
            /// TryGetMember
            /// </summary>
            /// <param name="binder"></param>
            /// <param name="result"></param>
            /// <returns></returns>
            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                var retVal = _row.Table.Columns.Contains(binder.Name);
                result = null;
                if (retVal && !(_row[binder.Name] is DBNull)) result = _row[binder.Name];
                return retVal;
            }

            /// <summary>
            /// TrySetMember
            /// </summary>
            /// <param name="binder"></param>
            /// <param name="value"></param>
            /// <returns></returns>
            public override bool TrySetMember(SetMemberBinder binder, object value)
            {
                if (!_row.Table.Columns.Contains(binder.Name))
                {
                    var dc = new DataColumn(binder.Name, value.GetType());
                    _row.Table.Columns.Add(dc);
                }
                _row[binder.Name] = value;
                return true;
            }

            /// <summary>
            /// TryInvoke
            /// </summary>
            /// <param name="binder"></param>
            /// <param name="args"></param>
            /// <param name="result"></param>
            /// <returns></returns>
            public override bool TryInvoke(InvokeBinder binder, object[] args, out object result) => base.TryInvoke(binder, args, out result);

            /// <summary>
            /// TryInvokeMember
            /// </summary>
            /// <param name="binder"></param>
            /// <param name="args"></param>
            /// <param name="result"></param>
            /// <returns></returns>
            public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) => base.TryInvokeMember(binder, args, out result);
            #endregion

            #region 实现IDictionary<string, object>
            ICollection<string> IDictionary<string, object>.Keys => _row.Table.Columns.OfType<DataColumn>().Select(o => o.ColumnName).ToArray();
            ICollection<object> IDictionary<string, object>.Values => _row.ItemArray.ToList();
            int ICollection<KeyValuePair<string, object>>.Count => _row.Table.Columns.Count;
            bool ICollection<KeyValuePair<string, object>>.IsReadOnly => false;
            object IDictionary<string, object>.this[string key]
            {
                get { return _row[key]; }
                set { _row[key] = value; }
            }
            bool IDictionary<string, object>.ContainsKey(string key) => _row.Table.Columns.Contains(key);
            void IDictionary<string, object>.Add(string key, object value)
            {
                if (!_row.Table.Columns.Contains(key))
                {
                    var dc = new DataColumn(key, value.GetType());
                    _row.Table.Columns.Add(dc);
                }
                _row[key] = value;
            }
            bool IDictionary<string, object>.Remove(string key)
            {
                var r = false;
                if (_row.Table.Columns.Contains(key))
                {
                    _row.Table.Columns.Remove(key);
                    r = true;
                }
                return r;
            }
            bool IDictionary<string, object>.TryGetValue(string key, out object value)
            {
                value = null;
                var b = _row.Table.Columns.Contains(key);
                if (b)
                {
                    if (!(_row[key] is DBNull)) value = _row[key];
                }
                return b;
            }
            void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
            {
                if (!_row.Table.Columns.Contains(item.Key))
                {
                    var dc = new DataColumn(item.Key, item.Value.GetType());
                    _row.Table.Columns.Add(item.Key);
                }
                _row[item.Key] = item.Value;
            }
            void ICollection<KeyValuePair<string, object>>.Clear() => _row.Table.Columns.Clear();
            bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item) => _row.Table.Columns.Contains(item.Key);
            void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
            {
                foreach (var kv in this)
                {
                    array[arrayIndex++] = kv;
                }
            }
            bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
            {
                var r = false;
                if (_row.Table.Columns.Contains(item.Key))
                {
                    _row.Table.Columns.Remove(item.Key);
                    r = true;
                }
                return r;
            }
            IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator() => GetEnumerator();

            /// <summary>
            /// GetEnumerator
            /// </summary>
            /// <returns></returns>
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            /// <summary>
            /// GetEnumerator
            /// </summary>
            /// <returns></returns>
            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                var columns = _row.Table.Columns;
                for (var i = 0; i < columns.Count; i++)
                {
                    var key = i < columns.Count ? _row.Table.Columns[i].ColumnName : null;
                    var value = i < columns.Count ? _row[i] : null;
                    yield return new KeyValuePair<string, object>(key, value);
                }
            }
            #endregion
        }
        #endregion

        #region DapperRow
        /// <summary>
        /// perThreadStringBuilderCache
        /// </summary>
        [ThreadStatic]
        private static StringBuilder perThreadStringBuilderCache;

        /// <summary>
        /// GetStringBuilder
        /// </summary>
        /// <returns></returns>
        private static StringBuilder GetStringBuilder()
        {
            var tmp = perThreadStringBuilderCache;
            if (tmp != null)
            {
                perThreadStringBuilderCache = null;
                tmp.Length = 0;
                return tmp;
            }
            return new StringBuilder();
        }

        /// <summary>
        /// __ToStringRecycle
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static string __ToStringRecycle(this StringBuilder obj)
        {
            if (obj == null) return "";
            var s = obj.ToString();
            perThreadStringBuilderCache = perThreadStringBuilderCache ?? obj;
            return s;
        }

        /// <summary>
        /// DapperTable
        /// </summary>
        public sealed class DapperTable
        {
            private string[] fieldNames;
            private readonly Dictionary<string, int> fieldNameLookup;

            internal string[] FieldNames => fieldNames;

            /// <summary>
            /// DapperTable
            /// </summary>
            /// <param name="fieldNames"></param>
            public DapperTable(string[] fieldNames)
            {
                this.fieldNames = fieldNames ?? throw new ArgumentNullException(nameof(fieldNames));
                fieldNameLookup = new Dictionary<string, int>(fieldNames.Length, StringComparer.Ordinal);
                // if there are dups, we want the **first** key to be the "winner" - so iterate backwards
                for (int i = fieldNames.Length - 1; i >= 0; i--)
                {
                    string key = fieldNames[i];
                    if (key != null) fieldNameLookup[key] = i;
                }
            }

            internal int IndexOfName(string name) => (name != null && fieldNameLookup.TryGetValue(name, out int result)) ? result : -1;

            internal int AddField(string name)
            {
                if (name == null) throw new ArgumentNullException(nameof(name));
                if (fieldNameLookup.ContainsKey(name)) throw new InvalidOperationException("Field already exists: " + name);
                int oldLen = fieldNames.Length;
                Array.Resize(ref fieldNames, oldLen + 1); // yes, this is sub-optimal, but this is not the expected common case
                fieldNames[oldLen] = name;
                fieldNameLookup[name] = oldLen;
                return oldLen;
            }

            internal bool FieldExists(string key) => key != null && fieldNameLookup.ContainsKey(key);

            /// <summary>
            /// FieldCount
            /// </summary>
            public int FieldCount => fieldNames.Length;
        }

        /// <summary>
        /// DapperRowMetaObject
        /// </summary>
        private sealed class DapperRowMetaObject : DynamicMetaObject
        {
            private static readonly MethodInfo getValueMethod = typeof(IDictionary<string, object>).GetProperty("Item").GetGetMethod();
            private static readonly MethodInfo setValueMethod = typeof(DapperRow).GetMethod("SetValue", new Type[] { typeof(string), typeof(object) });

            public DapperRowMetaObject(
                Expression expression,
                BindingRestrictions restrictions)
                : base(expression, restrictions)
            {
            }

            public DapperRowMetaObject(
                Expression expression,
                BindingRestrictions restrictions,
                object value
                )
                : base(expression, restrictions, value)
            {
            }

            private DynamicMetaObject CallMethod(
                MethodInfo method,
                Expression[] parameters
                )
            {
                var callMethod = new DynamicMetaObject(
                    Expression.Call(
                        Expression.Convert(Expression, LimitType),
                        method,
                        parameters),
                    BindingRestrictions.GetTypeRestriction(Expression, LimitType)
                    );
                return callMethod;
            }

            public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
            {
                var parameters = new Expression[] { Expression.Constant(binder.Name) };
                var callMethod = CallMethod(getValueMethod, parameters);
                return callMethod;
            }

            // Needed for Visual basic dynamic support
            public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
            {
                var parameters = new Expression[] { Expression.Constant(binder.Name) };
                var callMethod = CallMethod(getValueMethod, parameters);
                return callMethod;
            }

            public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
            {
                var parameters = new Expression[] { Expression.Constant(binder.Name), value.Expression, };
                var callMethod = CallMethod(setValueMethod, parameters);
                return callMethod;
            }
        }

        /// <summary>
        /// DapperRow
        /// </summary>
        public sealed class DapperRow : IDynamicMetaObjectProvider, IDictionary<string, object>, IReadOnlyDictionary<string, object>
        {
            private readonly DapperTable table;
            private object[] values;

            /// <summary>
            /// DapperRow
            /// </summary>
            /// <param name="table"></param>
            /// <param name="values"></param>
            public DapperRow(DapperTable table, object[] values)
            {
                this.table = table ?? throw new ArgumentNullException(nameof(table));
                this.values = values ?? throw new ArgumentNullException(nameof(values));
            }

            private sealed class DeadValue
            {
                public static readonly DeadValue Default = new DeadValue();
                private DeadValue() { /* hiding constructor */ }
            }

            int ICollection<KeyValuePair<string, object>>.Count
            {
                get
                {
                    int count = 0;
                    for (int i = 0; i < values.Length; i++)
                    {
                        if (!(values[i] is DeadValue)) count++;
                    }
                    return count;
                }
            }

            /// <summary>
            /// TryGetValue
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value"></param>
            /// <returns></returns>
            public bool TryGetValue(string key, out object value)
            {
                var index = table.IndexOfName(key);
                if (index < 0)
                { // doesn't exist
                    value = null;
                    return false;
                }
                // exists, **even if** we don't have a value; consider table rows heterogeneous
                value = index < values.Length ? values[index] : null;
                if (value is DeadValue)
                { // pretend it isn't here
                    value = null;
                    return false;
                }
                return true;
            }

            /// <summary>
            /// ToString
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                var sb = GetStringBuilder().Append("{DapperRow");
                foreach (var kv in this)
                {
                    var value = kv.Value;
                    sb.Append(", ").Append(kv.Key);
                    if (value != null)
                    {
                        sb.Append(" = '").Append(kv.Value).Append('\'');
                    }
                    else
                    {
                        sb.Append(" = NULL");
                    }
                }
                return sb.Append('}').__ToStringRecycle();
            }

            DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) => new DapperRowMetaObject(parameter, BindingRestrictions.Empty, this);

            /// <summary>
            /// GetEnumerator
            /// </summary>
            /// <returns></returns>
            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                var names = table.FieldNames;
                for (var i = 0; i < names.Length; i++)
                {
                    object value = i < values.Length ? values[i] : null;
                    if (!(value is DeadValue))
                    {
                        yield return new KeyValuePair<string, object>(names[i], value);
                    }
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            #region Implementation of ICollection<KeyValuePair<string,object>>
            void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
            {
                IDictionary<string, object> dic = this;
                dic.Add(item.Key, item.Value);
            }

            void ICollection<KeyValuePair<string, object>>.Clear()
            { // removes values for **this row**, but doesn't change the fundamental table
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = DeadValue.Default;
                }
            }

            bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
            {
                return TryGetValue(item.Key, out object value) && Equals(value, item.Value);
            }

            void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
            {
                foreach (var kv in this)
                {
                    array[arrayIndex++] = kv; // if they didn't leave enough space; not our fault
                }
            }

            bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
            {
                IDictionary<string, object> dic = this;
                return dic.Remove(item.Key);
            }

            bool ICollection<KeyValuePair<string, object>>.IsReadOnly => false;
            #endregion

            #region Implementation of IDictionary<string,object>
            bool IDictionary<string, object>.ContainsKey(string key)
            {
                int index = table.IndexOfName(key);
                if (index < 0 || index >= values.Length || values[index] is DeadValue) return false;
                return true;
            }

            void IDictionary<string, object>.Add(string key, object value) => SetValue(key, value, true);

            bool IDictionary<string, object>.Remove(string key)
            {
                int index = table.IndexOfName(key);
                if (index < 0 || index >= values.Length || values[index] is DeadValue) return false;
                values[index] = DeadValue.Default;
                return true;
            }

            object IDictionary<string, object>.this[string key]
            {
                get { TryGetValue(key, out object val); return val; }
                set { SetValue(key, value, false); }
            }

            /// <summary>
            /// SetValue
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value"></param>
            /// <returns></returns>
            public object SetValue(string key, object value) => SetValue(key, value, false);

            private object SetValue(string key, object value, bool isAdd)
            {
                if (key == null) throw new ArgumentNullException(nameof(key));
                int index = table.IndexOfName(key);
                if (index < 0)
                {
                    index = table.AddField(key);
                }
                else if (isAdd && index < values.Length && !(values[index] is DeadValue))
                {
                    // then semantically, this value already exists
                    throw new ArgumentException("An item with the same key has already been added", nameof(key));
                }
                int oldLength = values.Length;
                if (oldLength <= index)
                {
                    // we'll assume they're doing lots of things, and
                    // grow it to the full width of the table
                    Array.Resize(ref values, table.FieldCount);
                    for (int i = oldLength; i < values.Length; i++)
                    {
                        values[i] = DeadValue.Default;
                    }
                }
                return values[index] = value;
            }

            ICollection<string> IDictionary<string, object>.Keys => this.Select(kv => kv.Key).ToArray();

            ICollection<object> IDictionary<string, object>.Values => this.Select(kv => kv.Value).ToArray();
            #endregion

            #region Implementation of IReadOnlyDictionary<string,object>
            int IReadOnlyCollection<KeyValuePair<string, object>>.Count => values.Count(t => !(t is DeadValue));

            bool IReadOnlyDictionary<string, object>.ContainsKey(string key)
            {
                int index = table.IndexOfName(key);
                return index >= 0 && index < values.Length && !(values[index] is DeadValue);
            }

            object IReadOnlyDictionary<string, object>.this[string key]
            {
                get { TryGetValue(key, out object val); return val; }
            }

            IEnumerable<string> IReadOnlyDictionary<string, object>.Keys => this.Select(kv => kv.Key);

            IEnumerable<object> IReadOnlyDictionary<string, object>.Values => this.Select(kv => kv.Value);

            #endregion
        }
        #endregion

        #region MappingRow
        /// <summary>
        /// DataTable映射类型
        /// </summary>
        public enum MappingRow
        {
            /// <summary>
            /// DynamicRow
            /// </summary>
            DynamicRow,

            /// <summary>
            /// DapperRow
            /// </summary>
            DapperRow
        }
        #endregion

        #region Append
        /// <summary>
        /// 将table追加到现有DataTable中
        /// </summary>
        /// <param name="this">源DataTable</param>
        /// <param name="table">目标DataTable</param>
        /// <returns>返回dt1</returns>
        public static DataTable Append(this DataTable @this, DataTable table)
        {
            var obj = new object[@this.Columns.Count];
            foreach (DataRow dr in table.Rows)
            {
                dr.ItemArray.CopyTo(obj, 0);
                @this.Rows.Add(obj);
            }
            return @this;
        }
        #endregion

        #region FirstRow
        /// <summary>
        /// A DataTable extension method that return the first row.
        /// </summary>
        /// <param name="this">The table to act on.</param>
        /// <returns>The first row of the table.</returns>
        public static DataRow FirstRow(this DataTable @this)
        {
            return @this.Rows[0];
        }
        #endregion

        #region LastRow
        /// <summary>
        /// A DataTable extension method that last row.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>A DataRow.</returns>
        public static DataRow LastRow(this DataTable @this)
        {
            return @this.Rows[@this.Rows.Count - 1];
        }
        #endregion

        #region Pagination
        /// <summary>
        /// 分页获取DataTable数据
        /// </summary>
        /// <param name="this">源DataTable</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">每页数量</param>
        /// <returns></returns>
        public static DataTable Pagination(this DataTable @this, int pageIndex, int pageSize)
        {
            var result = new DataTable();
            if (@this?.Rows.Count > 0)
            {
                result = @this.Clone();
                var rows = @this.AsEnumerable().Skip((pageIndex - 1) * pageSize).Take(pageSize);
                foreach (var item in rows)
                {
                    result.ImportRow(item);
                }
            }
            return result;
        }
        #endregion
    }
}
