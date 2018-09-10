using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Reflection;
/****************************
* [Author] 张强
* [Date] 2018-05-15
* [Describe] DataRow扩展类
* **************************/
namespace ZqUtils.Extensions
{
    /// <summary>
    /// DataRow扩展类
    /// </summary>
    public static partial class Extensions
    {
        #region ToHashTable
        /// <summary>
        /// DataRow转HashTable
        /// </summary>
        /// <param name="this">DataRow数据源</param>
        /// <returns>Hashtable</returns>
        public static Hashtable ToHashTable(this DataRow @this)
        {
            Hashtable ht = null;
            if (@this != null)
            {
                ht = new Hashtable(@this.ItemArray.Length);
                foreach (DataColumn dc in @this.Table.Columns)
                {
                    ht.Add(dc.ColumnName, @this[dc.ColumnName]);
                }
            }
            return ht;
        }
        #endregion

        #region ToEntity
        /// <summary>
        /// A DataRow extension method that converts the @this to the entities.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as a T.</returns>
        public static T ToEntity<T>(this DataRow @this) where T : new()
        {
            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

            var entity = new T();

            foreach (PropertyInfo property in properties)
            {
                if (@this.Table.Columns.Contains(property.Name))
                {
                    Type valueType = property.PropertyType;
                    property.SetValue(entity, @this[property.Name].To(valueType), null);
                }
            }

            foreach (FieldInfo field in fields)
            {
                if (@this.Table.Columns.Contains(field.Name))
                {
                    Type valueType = field.FieldType;
                    field.SetValue(entity, @this[field.Name].To(valueType));
                }
            }

            return entity;
        }
        #endregion

        #region ToExpandoObject
        /// <summary>
        /// A DataRow extension method that converts the @this to an expando object.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as a dynamic.</returns>
        public static dynamic ToExpandoObject(this DataRow @this)
        {
            dynamic entity = new ExpandoObject();
            var expandoDict = (IDictionary<string, object>)entity;

            foreach (DataColumn column in @this.Table.Columns)
            {
                expandoDict.Add(column.ColumnName, @this[column]);
            }

            return expandoDict;
        }
        #endregion
    }
}
