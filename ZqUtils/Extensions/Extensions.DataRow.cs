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
    public static class DataRowExtensions
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
        public static T ToEntity<T>(this DataRow @this)
        {
            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

            var entity = Activator.CreateInstance<T>();

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
