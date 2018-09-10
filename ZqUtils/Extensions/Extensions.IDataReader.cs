using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;
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
    public static partial class Extensions
    {
        #region ToDynamic
        /// <summary>
        /// IDataReader数据转为dynamic对象
        /// </summary>
        /// <param name="this">reader数据源</param>
        /// <param name="isDisposable">是否释放</param>
        /// <returns>dynamic</returns>
        public static dynamic ToDynamic(this IDataReader @this, bool isDisposable)
        {
            dynamic result = null;
            if (@this?.IsClosed == false)
            {
                if (!isDisposable || (isDisposable && @this.Read()))
                {
                    result = new ExpandoObject();
                    for (var i = 0; i < @this.FieldCount; i++)
                    {
                        try
                        {
                            ((IDictionary<string, object>)result).Add(@this.GetName(i), @this.GetValue(i));
                        }
                        catch
                        {
                            ((IDictionary<string, object>)result).Add(@this.GetName(i), null);
                        }
                    }
                }
                if (isDisposable)
                {
                    @this.Close();
                    @this.Dispose();
                }
            }
            return result;
        }

        /// <summary>
        /// IDataReader数据转为dynamic对象集合
        /// </summary>
        /// <param name="this">reader数据源</param>
        /// <returns>dynamic集合</returns>
        public static List<dynamic> ToDynamic(this IDataReader @this)
        {
            List<dynamic> list = null;
            if (@this?.IsClosed == false)
            {
                list = new List<dynamic>();
                using (@this)
                {
                    while (@this.Read())
                    {
                        list.Add(@this.ToDynamic(false));
                    }
                }
            }
            return list;
        }
        #endregion

        #region ToDictionary
        /// <summary>
        /// IDataReader数据转为dynamic对象
        /// </summary>
        /// <param name="this">reader数据源</param>
        /// <param name="isDisposable">是否释放</param>
        /// <returns>dynamic</returns>
        public static Dictionary<string, object> ToDictionary(this IDataReader @this, bool isDisposable)
        {
            Dictionary<string, object> result = null;
            if (@this?.IsClosed == false)
            {
                if (!isDisposable || (isDisposable && @this.Read()))
                {
                    result = new Dictionary<string, object>();
                    for (var i = 0; i < @this.FieldCount; i++)
                    {
                        try
                        {
                            result.Add(@this.GetName(i), @this.GetValue(i));
                        }
                        catch
                        {
                            result.Add(@this.GetName(i), null);
                        }
                    }
                }
                if (isDisposable)
                {
                    @this.Close();
                    @this.Dispose();
                }
            }
            return result;
        }

        /// <summary>
        /// IDataReader数据转为dynamic对象集合
        /// </summary>
        /// <param name="this">reader数据源</param>
        /// <returns>dynamic集合</returns>
        public static List<Dictionary<string, object>> ToDictionary(this IDataReader @this)
        {
            List<Dictionary<string, object>> list = null;
            if (@this?.IsClosed == false)
            {
                list = new List<Dictionary<string, object>>();
                using (@this)
                {
                    while (@this.Read())
                    {
                        list.Add(@this.ToDictionary(false));
                    }
                }
            }
            return list;
        }
        #endregion

        #region ToList
        /// <summary>
        /// IDataReader转换为T集合
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="this">reader数据源</param>
        /// <returns>T类型集合</returns>
        public static List<T> ToList<T>(this IDataReader @this)
        {
            List<T> list = null;
            if (@this?.IsClosed == false)
            {
                list = new List<T>();
                var type = typeof(T);
                if (type == typeof(Dictionary<string, object>))
                {
                    list = @this.ToDictionary() as List<T>;
                }
                else if (type.IsClass && type.Name != "Object")
                {
                    using (@this)
                    {
                        var fields = new List<string>();
                        for (int i = 0; i < @this.FieldCount; i++)
                        {
                            fields.Add(@this.GetName(i).ToLower());
                        }
                        while (@this.Read())
                        {
                            var instance = Activator.CreateInstance<T>();
                            var props = instance.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance);
                            foreach (var p in props)
                            {
                                if (!p.CanWrite) continue;
                                if (fields.Contains(p.Name.ToLower()) && !@this[p.Name].IsNull())
                                {
                                    p.SetValue(instance, @this[p.Name].ToSafeValue(p.PropertyType), null);
                                }
                            }
                            list.Add(instance);
                        }
                    }
                }
                else
                {
                    list = @this.ToDynamic() as List<T>;
                }
            }
            return list;
        }
        #endregion

        #region ToEntity
        /// <summary>
        /// IDataReader转换为T类型实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="this">reader数据源</param>
        /// <returns>T类型实体</returns>
        public static T ToEntity<T>(this IDataReader @this)
        {
            var result = default(T);
            if (@this?.IsClosed == false)
            {
                var type = typeof(T);
                if (type == typeof(Dictionary<string, object>))
                {
                    result = (@this.ToDictionary() as List<T>).FirstOrDefault();
                }
                else if (type.IsClass && type.Name != "Object")
                {
                    using (@this)
                    {
                        var fields = new List<string>();
                        for (int i = 0; i < @this.FieldCount; i++)
                        {
                            fields.Add(@this.GetName(i).ToLower());
                        }
                        if (@this.Read())
                        {
                            result = Activator.CreateInstance<T>();
                            var props = result.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance);
                            foreach (var p in props)
                            {
                                if (!p.CanWrite) continue;
                                if (fields.Contains(p.Name.ToLower()) && !@this[p.Name].IsNull())
                                {
                                    p.SetValue(result, @this[p.Name].ToSafeValue(p.PropertyType), null);
                                }
                            }
                        }
                    }
                }
                else
                {
                    result = (T)@this.ToDynamic(true);
                }
            }
            return result;
        }
        #endregion

        #region ToEntities
        /// <summary>
        /// Enumerates to entities in this collection.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as an IEnumerable&lt;T&gt;</returns>
        public static IEnumerable<T> ToEntities<T>(this IDataReader @this) where T : new()
        {
            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

            var list = new List<T>();

            var hash = new HashSet<string>(Enumerable.Range(0, @this.FieldCount)
                .Select(@this.GetName));

            while (@this.Read())
            {
                var entity = new T();

                foreach (PropertyInfo property in properties)
                {
                    if (hash.Contains(property.Name))
                    {
                        Type valueType = property.PropertyType;
                        property.SetValue(entity, @this[property.Name].To(valueType), null);
                    }
                }

                foreach (FieldInfo field in fields)
                {
                    if (hash.Contains(field.Name))
                    {
                        Type valueType = field.FieldType;
                        field.SetValue(entity, @this[field.Name].To(valueType));
                    }
                }

                list.Add(entity);
            }

            return list;
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
            if (@this?.IsClosed == false)
            {
                table.Load(@this);
            }
            return table;
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
