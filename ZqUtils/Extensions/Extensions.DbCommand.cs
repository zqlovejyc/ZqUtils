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
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
/****************************
* [Author] 张强
* [Date] 2018-07-10
* [Describe] DbCommand扩展类
* **************************/
namespace ZqUtils.Extensions
{
    /// <summary>
    /// DbCommand扩展类
    /// </summary>
    public static partial class Extensions
    {
        #region ExecuteEntities
        /// <summary>
        /// Enumerates execute entities in this collection.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <returns>An enumerator that allows foreach to be used to process execute entities in this collection.</returns>
        public static IEnumerable<T> ExecuteEntities<T>(this DbCommand @this) where T : new()
        {
            using (IDataReader reader = @this.ExecuteReader())
            {
                return reader.ToEntities<T>();
            }
        }
        #endregion

        #region ExecuteEntity
        /// <summary>
        /// A DbCommand extension method that executes the entity operation.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <returns>A T.</returns>
        public static T ExecuteEntity<T>(this DbCommand @this) where T : new()
        {
            using (IDataReader reader = @this.ExecuteReader())
            {
                reader.Read();
                return reader.ToEntity<T>();
            }
        }
        #endregion

        #region ExecuteExpandoObject
        /// <summary>
        /// A DbCommand extension method that executes the expando object operation.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>A dynamic.</returns>
        public static dynamic ExecuteExpandoObject(this DbCommand @this)
        {
            using (IDataReader reader = @this.ExecuteReader())
            {
                reader.Read();
                return reader.ToExpandoObject();
            }
        }
        #endregion

        #region ExecuteExpandoObjects
        /// <summary>
        /// Enumerates execute expando objects in this collection.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>
        /// An enumerator that allows foreach to be used to process execute expando objects in this collection.
        /// </returns>
        public static IEnumerable<dynamic> ExecuteExpandoObjects(this DbCommand @this)
        {
            using (IDataReader reader = @this.ExecuteReader())
            {
                return reader.ToExpandoObjects();
            }
        }
        #endregion

        #region ExecuteScalarAs
        /// <summary>
        /// A DbCommand extension method that executes the scalar as operation.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <returns>A T.</returns>
        public static T ExecuteScalarAs<T>(this DbCommand @this)
        {
            return (T)@this.ExecuteScalar();
        }
        #endregion

        #region ExecuteScalarAsOrDefault
        /// <summary>
        /// A DbCommand extension method that executes the scalar as or default operation.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <returns>A T.</returns>
        public static T ExecuteScalarAsOrDefault<T>(this DbCommand @this)
        {
            try
            {
                return (T)@this.ExecuteScalar();
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        /// <summary>
        /// A DbCommand extension method that executes the scalar as or default operation.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>A T.</returns>
        public static T ExecuteScalarAsOrDefault<T>(this DbCommand @this, T defaultValue)
        {
            try
            {
                return (T)@this.ExecuteScalar();
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// A DbCommand extension method that executes the scalar as or default operation.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>A T.</returns>
        public static T ExecuteScalarAsOrDefault<T>(this DbCommand @this, Func<DbCommand, T> defaultValueFactory)
        {
            try
            {
                return (T)@this.ExecuteScalar();
            }
            catch (Exception)
            {
                return defaultValueFactory(@this);
            }
        }
        #endregion

        #region ExecuteScalarTo
        /// <summary>
        /// A DbCommand extension method that executes the scalar to operation.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <returns>A T.</returns>
        public static T ExecuteScalarTo<T>(this DbCommand @this)
        {
            return @this.ExecuteScalar().To<T>();
        }
        #endregion

        #region ExecuteScalarToOrDefault
        /// <summary>
        /// A DbCommand extension method that executes the scalar to or default operation.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <returns>A T.</returns>
        public static T ExecuteScalarToOrDefault<T>(this DbCommand @this)
        {
            try
            {
                return @this.ExecuteScalar().To<T>();
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        /// <summary>
        /// A DbCommand extension method that executes the scalar to or default operation.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>A T.</returns>
        public static T ExecuteScalarToOrDefault<T>(this DbCommand @this, T defaultValue)
        {
            try
            {
                return @this.ExecuteScalar().To<T>();
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// A DbCommand extension method that executes the scalar to or default operation.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>A T.</returns>
        public static T ExecuteScalarToOrDefault<T>(this DbCommand @this, Func<DbCommand, T> defaultValueFactory)
        {
            try
            {
                return @this.ExecuteScalar().To<T>();
            }
            catch (Exception)
            {
                return defaultValueFactory(@this);
            }
        }
        #endregion
    }
}
