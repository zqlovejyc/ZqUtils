#region License
/***
 * Copyright © 2018-2021, 张强 (943620963@qq.com).
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

using Force.DeepCloner;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web.Routing;
using System.Xml.Serialization;
/****************************
* [Author] 张强
* [Date] 2018-05-15
* [Describe] object扩展类
* **************************/
namespace ZqUtils.Extensions
{
    /// <summary>
    /// object扩展类
    /// </summary>
    public static class ObjectExtensions
    {
        #region ToJson
        /// <summary>
        /// 对象序列化为json字符串
        /// </summary>
        /// <param name="this">待序列化的对象</param>
        /// <returns>string</returns>
        public static string ToJson(this object @this)
        {
            return JsonConvert.SerializeObject(@this);
        }

        /// <summary>
        /// 对象序列化为json字符串
        /// </summary>
        /// <param name="this">待序列化的对象</param>
        /// <param name="settings">JsonSerializerSettings配置</param>
        /// <returns></returns>
        public static string ToJson(this object @this, JsonSerializerSettings settings)
        {
            return JsonConvert.SerializeObject(@this, settings ?? new JsonSerializerSettings());
        }

        /// <summary>
        /// 对象序列化为json字符串
        /// </summary>
        /// <param name="this">待序列化的对象</param>
        /// <param name="camelCase">是否驼峰</param>
        /// <param name="indented">是否缩进</param>
        /// <param name="nullValueHandling">空值处理</param>
        /// <param name="converter">json转换，如：new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" }</param>
        /// <returns>string</returns>
        public static string ToJson(this object @this, bool camelCase = false, bool indented = false, NullValueHandling nullValueHandling = NullValueHandling.Include, JsonConverter converter = null)
        {
            var options = new JsonSerializerSettings();
            if (camelCase)
            {
                options.ContractResolver = new CamelCasePropertyNamesContractResolver();
            }
            if (indented)
            {
                options.Formatting = Formatting.Indented;
            }
            options.NullValueHandling = nullValueHandling;
            if (converter != null)
            {
                options.Converters?.Add(converter);
            }
            return JsonConvert.SerializeObject(@this, options);
        }
        #endregion

        #region ToExpando
        /// <summary>
        /// 匿名对象转换为ExpandoObject，用于mvc中view返回model值
        /// </summary>
        /// <param name="this">匿名类型对象</param>
        /// <param name="isAnonymousType">是否是匿名类型，为false时返回null</param>
        /// <returns></returns>
        public static ExpandoObject ToExpando(this object @this, bool isAnonymousType)
        {
            if (isAnonymousType)
            {
                IDictionary<string, object> anonymousDictionary = new RouteValueDictionary(@this);
                IDictionary<string, object> expando = new ExpandoObject();
                foreach (var item in anonymousDictionary)
                {
                    expando.Add(item);
                }
                return expando as ExpandoObject;
            }
            else if (typeof(IDictionary<string, object>).IsAssignableFrom(@this.GetType()))
            {
                return (@this as IDictionary<string, object>)?.ToExpando();
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region ToSafeValue
        /// <summary>
        /// 转换为安全类型的值
        /// </summary>
        /// <param name="this">object对象</param>
        /// <param name="type">type</param>
        /// <returns>object</returns>
        public static object ToSafeValue(this object @this, Type type)
        {
            return @this == null ? null : Convert.ChangeType(@this, type.GetCoreType());
        }
        #endregion

        #region As
        /// <summary>
        /// Used to simplify and beautify casting an object to a type. 
        /// </summary>
        /// <typeparam name="T">Type to be casted</typeparam>
        /// <param name="this">object to cast</param>
        /// <returns>Casted object</returns>
        public static T As<T>(this object @this) where T : class
        {
            return (T)@this;
        }
        #endregion

        #region AsOrDefault
        /// <summary>
        /// An object extension method that converts the @this to an or default.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <returns>A T.</returns>
        public static T AsOrDefault<T>(this object @this)
        {
            try
            {
                return (T)@this;
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        /// <summary>
        /// An object extension method that converts the @this to an or default.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>A T.</returns>
        public static T AsOrDefault<T>(this object @this, T defaultValue)
        {
            try
            {
                return (T)@this;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts the @this to an or default.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>A T.</returns>
        /// <example>
        ///     <code>
        ///       using Microsoft.VisualStudio.TestTools.UnitTesting;
        ///       using Z.ExtensionMethods.Object;
        /// 
        ///       namespace ExtensionMethods.Examples
        ///       {
        ///           [TestClass]
        ///           public class System_Object_AsOrDefault
        ///           {
        ///               [TestMethod]
        ///               public void AsOrDefault()
        ///               {
        ///                   // Type
        ///                   object intValue = 1;
        ///                   object invalidValue = &quot;Fizz&quot;;
        /// 
        ///                   // Exemples
        ///                   var result1 = intValue.AsOrDefault&lt;int&gt;(); // return 1;
        ///                   var result2 = invalidValue.AsOrDefault&lt;int&gt;(); // return 0;
        ///                   int result3 = invalidValue.AsOrDefault(3); // return 3;
        ///                   int result4 = invalidValue.AsOrDefault(() =&gt; 4); // return 4;
        /// 
        ///                   // Unit Test
        ///                   Assert.AreEqual(1, result1);
        ///                   Assert.AreEqual(0, result2);
        ///                   Assert.AreEqual(3, result3);
        ///                   Assert.AreEqual(4, result4);
        ///               }
        ///           }
        ///       }
        /// </code>
        /// </example>
        /// <example>
        ///     <code>
        ///       using Microsoft.VisualStudio.TestTools.UnitTesting;
        ///       using Z.ExtensionMethods.Object;
        /// 
        ///       namespace ExtensionMethods.Examples
        ///       {
        ///           [TestClass]
        ///           public class System_Object_AsOrDefault
        ///           {
        ///               [TestMethod]
        ///               public void AsOrDefault()
        ///               {
        ///                   // Type
        ///                   object intValue = 1;
        ///                   object invalidValue = &quot;Fizz&quot;;
        /// 
        ///                   // Exemples
        ///                   var result1 = intValue.AsOrDefault&lt;int&gt;(); // return 1;
        ///                   var result2 = invalidValue.AsOrDefault&lt;int&gt;(); // return 0;
        ///                   int result3 = invalidValue.AsOrDefault(3); // return 3;
        ///                   int result4 = invalidValue.AsOrDefault(() =&gt; 4); // return 4;
        /// 
        ///                   // Unit Test
        ///                   Assert.AreEqual(1, result1);
        ///                   Assert.AreEqual(0, result2);
        ///                   Assert.AreEqual(3, result3);
        ///                   Assert.AreEqual(4, result4);
        ///               }
        ///           }
        ///       }
        /// </code>
        /// </example>
        /// <example>
        ///     <code>
        ///           using Microsoft.VisualStudio.TestTools.UnitTesting;
        ///           using Z.ExtensionMethods.Object;
        ///           
        ///           namespace ExtensionMethods.Examples
        ///           {
        ///               [TestClass]
        ///               public class System_Object_AsOrDefault
        ///               {
        ///                   [TestMethod]
        ///                   public void AsOrDefault()
        ///                   {
        ///                       // Type
        ///                       object intValue = 1;
        ///                       object invalidValue = &quot;Fizz&quot;;
        ///           
        ///                       // Exemples
        ///                       var result1 = intValue.AsOrDefault&lt;int&gt;(); // return 1;
        ///                       var result2 = invalidValue.AsOrDefault&lt;int&gt;(); // return 0;
        ///                       int result3 = invalidValue.AsOrDefault(3); // return 3;
        ///                       int result4 = invalidValue.AsOrDefault(() =&gt; 4); // return 4;
        ///           
        ///                       // Unit Test
        ///                       Assert.AreEqual(1, result1);
        ///                       Assert.AreEqual(0, result2);
        ///                       Assert.AreEqual(3, result3);
        ///                       Assert.AreEqual(4, result4);
        ///                   }
        ///               }
        ///           }
        ///     </code>
        /// </example>
        public static T AsOrDefault<T>(this object @this, Func<T> defaultValueFactory)
        {
            try
            {
                return (T)@this;
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }

        /// <summary>
        /// An object extension method that converts the @this to an or default.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>A T.</returns>
        /// <example>
        ///     <code>
        ///       using Microsoft.VisualStudio.TestTools.UnitTesting;
        ///       using Z.ExtensionMethods.Object;
        /// 
        ///       namespace ExtensionMethods.Examples
        ///       {
        ///           [TestClass]
        ///           public class System_Object_AsOrDefault
        ///           {
        ///               [TestMethod]
        ///               public void AsOrDefault()
        ///               {
        ///                   // Type
        ///                   object intValue = 1;
        ///                   object invalidValue = &quot;Fizz&quot;;
        /// 
        ///                   // Exemples
        ///                   var result1 = intValue.AsOrDefault&lt;int&gt;(); // return 1;
        ///                   var result2 = invalidValue.AsOrDefault&lt;int&gt;(); // return 0;
        ///                   int result3 = invalidValue.AsOrDefault(3); // return 3;
        ///                   int result4 = invalidValue.AsOrDefault(() =&gt; 4); // return 4;
        /// 
        ///                   // Unit Test
        ///                   Assert.AreEqual(1, result1);
        ///                   Assert.AreEqual(0, result2);
        ///                   Assert.AreEqual(3, result3);
        ///                   Assert.AreEqual(4, result4);
        ///               }
        ///           }
        ///       }
        /// </code>
        /// </example>
        /// <example>
        ///     <code>
        ///       using Microsoft.VisualStudio.TestTools.UnitTesting;
        ///       using Z.ExtensionMethods.Object;
        /// 
        ///       namespace ExtensionMethods.Examples
        ///       {
        ///           [TestClass]
        ///           public class System_Object_AsOrDefault
        ///           {
        ///               [TestMethod]
        ///               public void AsOrDefault()
        ///               {
        ///                   // Type
        ///                   object intValue = 1;
        ///                   object invalidValue = &quot;Fizz&quot;;
        /// 
        ///                   // Exemples
        ///                   var result1 = intValue.AsOrDefault&lt;int&gt;(); // return 1;
        ///                   var result2 = invalidValue.AsOrDefault&lt;int&gt;(); // return 0;
        ///                   int result3 = invalidValue.AsOrDefault(3); // return 3;
        ///                   int result4 = invalidValue.AsOrDefault(() =&gt; 4); // return 4;
        /// 
        ///                   // Unit Test
        ///                   Assert.AreEqual(1, result1);
        ///                   Assert.AreEqual(0, result2);
        ///                   Assert.AreEqual(3, result3);
        ///                   Assert.AreEqual(4, result4);
        ///               }
        ///           }
        ///       }
        /// </code>
        /// </example>
        /// <example>
        ///     <code>
        ///           using Microsoft.VisualStudio.TestTools.UnitTesting;
        ///           using Z.ExtensionMethods.Object;
        ///           
        ///           namespace ExtensionMethods.Examples
        ///           {
        ///               [TestClass]
        ///               public class System_Object_AsOrDefault
        ///               {
        ///                   [TestMethod]
        ///                   public void AsOrDefault()
        ///                   {
        ///                       // Type
        ///                       object intValue = 1;
        ///                       object invalidValue = &quot;Fizz&quot;;
        ///           
        ///                       // Exemples
        ///                       var result1 = intValue.AsOrDefault&lt;int&gt;(); // return 1;
        ///                       var result2 = invalidValue.AsOrDefault&lt;int&gt;(); // return 0;
        ///                       int result3 = invalidValue.AsOrDefault(3); // return 3;
        ///                       int result4 = invalidValue.AsOrDefault(() =&gt; 4); // return 4;
        ///           
        ///                       // Unit Test
        ///                       Assert.AreEqual(1, result1);
        ///                       Assert.AreEqual(0, result2);
        ///                       Assert.AreEqual(3, result3);
        ///                       Assert.AreEqual(4, result4);
        ///                   }
        ///               }
        ///           }
        ///     </code>
        /// </example>
        public static T AsOrDefault<T>(this object @this, Func<object, T> defaultValueFactory)
        {
            try
            {
                return (T)@this;
            }
            catch (Exception)
            {
                return defaultValueFactory(@this);
            }
        }
        #endregion

        #region To
        /// <summary>
        /// A System.Object extension method that toes the given this.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">this.</param>
        /// <returns>A T.</returns>
        /// <example>
        ///     <code>
        ///       using System;
        ///       using Microsoft.VisualStudio.TestTools.UnitTesting;
        /// 
        ///       namespace ExtensionMethods.Examples
        ///       {
        ///           [TestClass]
        ///           public class System_Object_To
        ///           {
        ///               [TestMethod]
        ///               public void To()
        ///               {
        ///                   string nullValue = null;
        ///                   string value = &quot;1&quot;;
        ///                   object dbNullValue = DBNull.Value;
        /// 
        ///                   // Exemples
        ///                   var result1 = value.To&lt;int&gt;(); // return 1;
        ///                   var result2 = value.To&lt;int?&gt;(); // return 1;
        ///                   var result3 = nullValue.To&lt;int?&gt;(); // return null;
        ///                   var result4 = dbNullValue.To&lt;int?&gt;(); // return null;
        /// 
        ///                   // Unit Test
        ///                   Assert.AreEqual(1, result1);
        ///                   Assert.AreEqual(1, result2.Value);
        ///                   Assert.IsFalse(result3.HasValue);
        ///                   Assert.IsFalse(result4.HasValue);
        ///               }
        ///           }
        ///       }
        /// </code>
        /// </example>
        /// <example>
        ///     <code>
        ///       using System;
        ///       using Microsoft.VisualStudio.TestTools.UnitTesting;
        ///       using Z.ExtensionMethods.Object;
        /// 
        ///       namespace ExtensionMethods.Examples
        ///       {
        ///           [TestClass]
        ///           public class System_Object_To
        ///           {
        ///               [TestMethod]
        ///               public void To()
        ///               {
        ///                   string nullValue = null;
        ///                   string value = &quot;1&quot;;
        ///                   object dbNullValue = DBNull.Value;
        /// 
        ///                   // Exemples
        ///                   var result1 = value.To&lt;int&gt;(); // return 1;
        ///                   var result2 = value.To&lt;int?&gt;(); // return 1;
        ///                   var result3 = nullValue.To&lt;int?&gt;(); // return null;
        ///                   var result4 = dbNullValue.To&lt;int?&gt;(); // return null;
        /// 
        ///                   // Unit Test
        ///                   Assert.AreEqual(1, result1);
        ///                   Assert.AreEqual(1, result2.Value);
        ///                   Assert.IsFalse(result3.HasValue);
        ///                   Assert.IsFalse(result4.HasValue);
        ///               }
        ///           }
        ///       }
        /// </code>
        /// </example>
        public static T To<T>(this object @this)
        {
            if (@this != null)
            {
                Type targetType = typeof(T);

                if (@this.GetType() == targetType)
                {
                    return (T)@this;
                }

                TypeConverter converter = TypeDescriptor.GetConverter(@this);
                if (converter != null)
                {
                    if (converter.CanConvertTo(targetType))
                    {
                        return (T)converter.ConvertTo(@this, targetType);
                    }
                }

                converter = TypeDescriptor.GetConverter(targetType);
                if (converter != null)
                {
                    if (converter.CanConvertFrom(@this.GetType()))
                    {
                        return (T)converter.ConvertFrom(@this);
                    }
                }

                if (@this == DBNull.Value)
                {
                    return (T)(object)null;
                }
            }

            return default;
        }

        /// <summary>
        /// A System.Object extension method that toes the given this.
        /// </summary>
        /// <param name="this">this.</param>
        /// <param name="type">The type.</param>
        /// <returns>An object.</returns>
        /// <example>
        ///     <code>
        ///       using System;
        ///       using Microsoft.VisualStudio.TestTools.UnitTesting;
        /// 
        /// 
        ///       namespace ExtensionMethods.Examples
        ///       {
        ///           [TestClass]
        ///           public class System_Object_To
        ///           {
        ///               [TestMethod]
        ///               public void To()
        ///               {
        ///                   string nullValue = null;
        ///                   string value = &quot;1&quot;;
        ///                   object dbNullValue = DBNull.Value;
        /// 
        ///                   // Exemples
        ///                   var result1 = value.To&lt;int&gt;(); // return 1;
        ///                   var result2 = value.To&lt;int?&gt;(); // return 1;
        ///                   var result3 = nullValue.To&lt;int?&gt;(); // return null;
        ///                   var result4 = dbNullValue.To&lt;int?&gt;(); // return null;
        /// 
        ///                   // Unit Test
        ///                   Assert.AreEqual(1, result1);
        ///                   Assert.AreEqual(1, result2.Value);
        ///                   Assert.IsFalse(result3.HasValue);
        ///                   Assert.IsFalse(result4.HasValue);
        ///               }
        ///           }
        ///       }
        /// </code>
        /// </example>
        /// <example>
        ///     <code>
        ///       using System;
        ///       using Microsoft.VisualStudio.TestTools.UnitTesting;
        ///       using Z.ExtensionMethods.Object;
        /// 
        ///       namespace ExtensionMethods.Examples
        ///       {
        ///           [TestClass]
        ///           public class System_Object_To
        ///           {
        ///               [TestMethod]
        ///               public void To()
        ///               {
        ///                   string nullValue = null;
        ///                   string value = &quot;1&quot;;
        ///                   object dbNullValue = DBNull.Value;
        /// 
        ///                   // Exemples
        ///                   var result1 = value.To&lt;int&gt;(); // return 1;
        ///                   var result2 = value.To&lt;int?&gt;(); // return 1;
        ///                   var result3 = nullValue.To&lt;int?&gt;(); // return null;
        ///                   var result4 = dbNullValue.To&lt;int?&gt;(); // return null;
        /// 
        ///                   // Unit Test
        ///                   Assert.AreEqual(1, result1);
        ///                   Assert.AreEqual(1, result2.Value);
        ///                   Assert.IsFalse(result3.HasValue);
        ///                   Assert.IsFalse(result4.HasValue);
        ///               }
        ///           }
        ///       }
        /// </code>
        /// </example>
        public static object To(this Object @this, Type type)
        {
            if (@this != null)
            {
                Type targetType = type;

                if (@this.GetType() == targetType)
                {
                    return @this;
                }

                TypeConverter converter = TypeDescriptor.GetConverter(@this);
                if (converter != null)
                {
                    if (converter.CanConvertTo(targetType))
                    {
                        return converter.ConvertTo(@this, targetType);
                    }
                }

                converter = TypeDescriptor.GetConverter(targetType);
                if (converter != null)
                {
                    if (converter.CanConvertFrom(@this.GetType()))
                    {
                        return converter.ConvertFrom(@this);
                    }
                }

                if (@this == DBNull.Value)
                {
                    return null;
                }
            }

            return @this;
        }
        #endregion

        #region ToOrDefault
        /// <summary>
        /// A System.Object extension method that converts this object to an or default.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">this.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to a T.</returns>
        /// <example>
        ///     <code>
        ///       using Microsoft.VisualStudio.TestTools.UnitTesting;
        ///       using Z.ExtensionMethods.Object;
        /// 
        ///       namespace ExtensionMethods.Examples
        ///       {
        ///           [TestClass]
        ///           public class System_Object_ToOrDefault
        ///           {
        ///               [TestMethod]
        ///               public void ToOrDefault()
        ///               {
        ///                   // Type
        ///                   object intValue = &quot;1&quot;;
        ///                   object invalidValue = &quot;Fizz&quot;;
        /// 
        ///                   // Exemples
        ///                   var result1 = intValue.ToOrDefault&lt;int&gt;(); // return 1;
        ///                   var result2 = invalidValue.ToOrDefault&lt;int&gt;(); // return 0;
        ///                   int result3 = invalidValue.ToOrDefault(3); // return 3;
        ///                   int result4 = invalidValue.ToOrDefault(() =&gt; 4); // return 4;
        /// 
        ///                   // Unit Test
        ///                   Assert.AreEqual(1, result1);
        ///                   Assert.AreEqual(0, result2);
        ///                   Assert.AreEqual(3, result3);
        ///                   Assert.AreEqual(4, result4);
        ///               }
        ///           }
        ///       }
        /// </code>
        /// </example>
        /// <example>
        ///     <code>
        ///       using Microsoft.VisualStudio.TestTools.UnitTesting;
        ///       using Z.ExtensionMethods.Object;
        /// 
        ///       namespace ExtensionMethods.Examples
        ///       {
        ///           [TestClass]
        ///           public class System_Object_ToOrDefault
        ///           {
        ///               [TestMethod]
        ///               public void ToOrDefault()
        ///               {
        ///                   // Type
        ///                   object intValue = &quot;1&quot;;
        ///                   object invalidValue = &quot;Fizz&quot;;
        /// 
        ///                   // Exemples
        ///                   var result1 = intValue.ToOrDefault&lt;int&gt;(); // return 1;
        ///                   var result2 = invalidValue.ToOrDefault&lt;int&gt;(); // return 0;
        ///                   int result3 = invalidValue.ToOrDefault(3); // return 3;
        ///                   int result4 = invalidValue.ToOrDefault(() =&gt; 4); // return 4;
        /// 
        ///                   // Unit Test
        ///                   Assert.AreEqual(1, result1);
        ///                   Assert.AreEqual(0, result2);
        ///                   Assert.AreEqual(3, result3);
        ///                   Assert.AreEqual(4, result4);
        ///               }
        ///           }
        ///       }
        /// </code>
        /// </example>
        /// <example>
        ///     <code>
        ///           using Microsoft.VisualStudio.TestTools.UnitTesting;
        ///           using Z.ExtensionMethods.Object;
        ///           
        ///           namespace ExtensionMethods.Examples
        ///           {
        ///               [TestClass]
        ///               public class System_Object_ToOrDefault
        ///               {
        ///                   [TestMethod]
        ///                   public void ToOrDefault()
        ///                   {
        ///                       // Type
        ///                       object intValue = &quot;1&quot;;
        ///                       object invalidValue = &quot;Fizz&quot;;
        ///           
        ///                       // Exemples
        ///                       var result1 = intValue.ToOrDefault&lt;int&gt;(); // return 1;
        ///                       var result2 = invalidValue.ToOrDefault&lt;int&gt;(); // return 0;
        ///                       int result3 = invalidValue.ToOrDefault(3); // return 3;
        ///                       int result4 = invalidValue.ToOrDefault(() =&gt; 4); // return 4;
        ///           
        ///                       // Unit Test
        ///                       Assert.AreEqual(1, result1);
        ///                       Assert.AreEqual(0, result2);
        ///                       Assert.AreEqual(3, result3);
        ///                       Assert.AreEqual(4, result4);
        ///                   }
        ///               }
        ///           }
        ///     </code>
        /// </example>
        public static T ToOrDefault<T>(this object @this, Func<object, T> defaultValueFactory)
        {
            try
            {
                if (@this != null)
                {
                    Type targetType = typeof(T);

                    if (@this.GetType() == targetType)
                    {
                        return (T)@this;
                    }

                    TypeConverter converter = TypeDescriptor.GetConverter(@this);
                    if (converter != null)
                    {
                        if (converter.CanConvertTo(targetType))
                        {
                            return (T)converter.ConvertTo(@this, targetType);
                        }
                    }

                    converter = TypeDescriptor.GetConverter(targetType);
                    if (converter != null)
                    {
                        if (converter.CanConvertFrom(@this.GetType()))
                        {
                            return (T)converter.ConvertFrom(@this);
                        }
                    }

                    if (@this == DBNull.Value)
                    {
                        return (T)(object)null;
                    }
                }

                return (T)@this;
            }
            catch (Exception)
            {
                return defaultValueFactory(@this);
            }
        }

        /// <summary>
        /// A System.Object extension method that converts this object to an or default.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">this.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to a T.</returns>
        /// <example>
        ///     <code>
        ///       using Microsoft.VisualStudio.TestTools.UnitTesting;
        ///       using Z.ExtensionMethods.Object;
        /// 
        ///       namespace ExtensionMethods.Examples
        ///       {
        ///           [TestClass]
        ///           public class System_Object_ToOrDefault
        ///           {
        ///               [TestMethod]
        ///               public void ToOrDefault()
        ///               {
        ///                   // Type
        ///                   object intValue = &quot;1&quot;;
        ///                   object invalidValue = &quot;Fizz&quot;;
        /// 
        ///                   // Exemples
        ///                   var result1 = intValue.ToOrDefault&lt;int&gt;(); // return 1;
        ///                   var result2 = invalidValue.ToOrDefault&lt;int&gt;(); // return 0;
        ///                   int result3 = invalidValue.ToOrDefault(3); // return 3;
        ///                   int result4 = invalidValue.ToOrDefault(() =&gt; 4); // return 4;
        /// 
        ///                   // Unit Test
        ///                   Assert.AreEqual(1, result1);
        ///                   Assert.AreEqual(0, result2);
        ///                   Assert.AreEqual(3, result3);
        ///                   Assert.AreEqual(4, result4);
        ///               }
        ///           }
        ///       }
        /// </code>
        /// </example>
        /// <example>
        ///     <code>
        ///       using Microsoft.VisualStudio.TestTools.UnitTesting;
        ///       using Z.ExtensionMethods.Object;
        /// 
        ///       namespace ExtensionMethods.Examples
        ///       {
        ///           [TestClass]
        ///           public class System_Object_ToOrDefault
        ///           {
        ///               [TestMethod]
        ///               public void ToOrDefault()
        ///               {
        ///                   // Type
        ///                   object intValue = &quot;1&quot;;
        ///                   object invalidValue = &quot;Fizz&quot;;
        /// 
        ///                   // Exemples
        ///                   var result1 = intValue.ToOrDefault&lt;int&gt;(); // return 1;
        ///                   var result2 = invalidValue.ToOrDefault&lt;int&gt;(); // return 0;
        ///                   int result3 = invalidValue.ToOrDefault(3); // return 3;
        ///                   int result4 = invalidValue.ToOrDefault(() =&gt; 4); // return 4;
        /// 
        ///                   // Unit Test
        ///                   Assert.AreEqual(1, result1);
        ///                   Assert.AreEqual(0, result2);
        ///                   Assert.AreEqual(3, result3);
        ///                   Assert.AreEqual(4, result4);
        ///               }
        ///           }
        ///       }
        /// </code>
        /// </example>
        /// <example>
        ///     <code>
        ///           using Microsoft.VisualStudio.TestTools.UnitTesting;
        ///           using Z.ExtensionMethods.Object;
        ///           
        ///           namespace ExtensionMethods.Examples
        ///           {
        ///               [TestClass]
        ///               public class System_Object_ToOrDefault
        ///               {
        ///                   [TestMethod]
        ///                   public void ToOrDefault()
        ///                   {
        ///                       // Type
        ///                       object intValue = &quot;1&quot;;
        ///                       object invalidValue = &quot;Fizz&quot;;
        ///           
        ///                       // Exemples
        ///                       var result1 = intValue.ToOrDefault&lt;int&gt;(); // return 1;
        ///                       var result2 = invalidValue.ToOrDefault&lt;int&gt;(); // return 0;
        ///                       int result3 = invalidValue.ToOrDefault(3); // return 3;
        ///                       int result4 = invalidValue.ToOrDefault(() =&gt; 4); // return 4;
        ///           
        ///                       // Unit Test
        ///                       Assert.AreEqual(1, result1);
        ///                       Assert.AreEqual(0, result2);
        ///                       Assert.AreEqual(3, result3);
        ///                       Assert.AreEqual(4, result4);
        ///                   }
        ///               }
        ///           }
        ///     </code>
        /// </example>
        public static T ToOrDefault<T>(this Object @this, Func<T> defaultValueFactory)
        {
            return @this.ToOrDefault(x => defaultValueFactory());
        }

        /// <summary>
        /// A System.Object extension method that converts this object to an or default.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">this.</param>
        /// <returns>The given data converted to a T.</returns>
        public static T ToOrDefault<T>(this Object @this)
        {
            return @this.ToOrDefault(x => default(T));
        }

        /// <summary>
        /// A System.Object extension method that converts this object to an or default.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">this.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to a T.</returns>
        public static T ToOrDefault<T>(this Object @this, T defaultValue)
        {
            return @this.ToOrDefault(x => defaultValue);
        }
        #endregion

        #region IsIn
        /// <summary>
        /// Check if an item is in a list.
        /// </summary>
        /// <param name="this">Item to check</param>
        /// <param name="list">List of items</param>
        /// <typeparam name="T">Type of the items</typeparam>
        public static bool IsIn<T>(this T @this, params T[] list)
        {
            return list.Contains(@this);
        }
        #endregion

        #region ToValueType
        #region ToInt
        /// <summary>
        /// 转为整数，转换失败时返回默认值。支持字符串、全角、字节数组（小端）、时间（Unix秒）
        /// </summary>
        /// <remarks>
        /// Int16/UInt32/long等，可以先转为最常用的int后再二次处理
        /// </remarks>
        /// <param name="this">待转换对象</param>
        /// <param name="defaultValue">默认值。待转换对象无效时使用</param>
        /// <returns></returns>
        public static int ToInt(this object @this, int defaultValue = 0)
        {
            if (@this == null || @this == DBNull.Value) return defaultValue;
            // 特殊处理字符串，也是最常见的
            if (@this is string str)
            {
                // 拷贝而来的逗号分隔整数
                str = str.Replace(",", null);
                str = str.ToDBC().Trim();
                if (str.IsNullOrEmpty()) return defaultValue;
                if (int.TryParse(str, out var n)) return n;
                return defaultValue;
            }
            // 特殊处理时间，转Unix秒
            if (@this is DateTime dt)
            {
                //// 先转UTC时间再相减，以得到绝对时间差
                return (int)(dt - new DateTime(1970, 1, 1)).TotalSeconds;
            }
            if (@this is byte[] buf)
            {
                if (buf == null || buf.Length < 1) return defaultValue;

                switch (buf.Length)
                {
                    case 1:
                        return buf[0];
                    case 2:
                        return BitConverter.ToInt16(buf, 0);
                    case 3:
                        return BitConverter.ToInt32(new byte[] { buf[0], buf[1], buf[2], 0 }, 0);
                    case 4:
                        return BitConverter.ToInt32(buf, 0);
                    default:
                        break;
                }
            }
            try
            {
                return Convert.ToInt32(@this);
            }
            catch
            {
                return defaultValue;
            }
        }
        #endregion

        #region ToLong
        /// <summary>
        /// 转为长整数，转换失败时返回默认值。支持字符串、全角、字节数组（小端）
        /// </summary>
        /// <param name="this">待转换对象</param>
        /// <param name="defaultValue">默认值。待转换对象无效时使用</param>
        /// <returns></returns>
        public static long ToLong(this object @this, long defaultValue = 0)
        {
            if (@this == null || @this == DBNull.Value) return defaultValue;
            // 特殊处理字符串，也是最常见的
            if (@this is string str)
            {
                // 拷贝而来的逗号分隔整数
                str = str.Replace(",", null);
                str = str.ToDBC().Trim();
                if (str.IsNullOrEmpty()) return defaultValue;
                if (long.TryParse(str, out var n)) return n;
                return defaultValue;
            }
            //暂时不做处理  先处理异常转换
            try
            {
                return Convert.ToInt64(@this);
            }
            catch { return defaultValue; }
        }
        #endregion

        #region ToLongOrDefault
        /// <summary>
        /// An object extension method that converts this object to a long or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a long.</returns>
        public static long ToLongOrDefault(this object @this)
        {
            try
            {
                return Convert.ToInt64(@this);
            }
            catch (Exception)
            {
                return default(long);
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a long or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to a long.</returns>
        public static long ToLongOrDefault(this object @this, long defaultValue)
        {
            try
            {
                return Convert.ToInt64(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a long or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to a long.</returns>
        public static long ToLongOrDefault(this object @this, long defaultValue, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValue;
            }

            try
            {
                return Convert.ToInt64(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a long or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to a long.</returns>
        public static long ToLongOrDefault(this object @this, Func<long> defaultValueFactory)
        {
            try
            {
                return Convert.ToInt64(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a long or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to a long.</returns>
        public static long ToLongOrDefault(this object @this, Func<long> defaultValueFactory, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValueFactory();
            }

            try
            {
                return Convert.ToInt64(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToDouble
        /// <summary>
        /// 转为浮点数，转换失败时返回默认值。支持字符串、全角、字节数组（小端）
        /// </summary>
        /// <remarks>
        /// single可以先转为最常用的double后再二次处理
        /// </remarks>
        /// <param name="this">待转换对象</param>
        /// <param name="defaultValue">默认值。待转换对象无效时使用</param>
        /// <returns></returns>
        public static double ToDouble(this object @this, double defaultValue = 0)
        {
            if (@this == null || @this == DBNull.Value) return defaultValue;
            // 特殊处理字符串，也是最常见的
            if (@this is string str)
            {
                str = str.ToDBC().Trim();
                if (str.IsNullOrEmpty()) return defaultValue;

                if (double.TryParse(str, out var n)) return n;
                return defaultValue;
            }
            else if (@this is byte[] buf)
            {
                if (buf == null || buf.Length < 1) return defaultValue;

                switch (buf.Length)
                {
                    case 1:
                        return buf[0];
                    case 2:
                        return BitConverter.ToInt16(buf, 0);
                    case 3:
                        return BitConverter.ToInt32(new byte[] { buf[0], buf[1], buf[2], 0 }, 0);
                    case 4:
                        return BitConverter.ToInt32(buf, 0);
                    default:
                        // 凑够8字节
                        if (buf.Length < 8)
                        {
                            var bts = new byte[8];
                            Buffer.BlockCopy(buf, 0, bts, 0, buf.Length);
                            buf = bts;
                        }
                        return BitConverter.ToDouble(buf, 0);
                }
            }
            try
            {
                return Convert.ToDouble(@this);
            }
            catch
            {
                return defaultValue;
            }
        }
        #endregion

        #region ToDoubleOrDefault
        /// <summary>
        /// An object extension method that converts this object to a double or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a double.</returns>
        public static double ToDoubleOrDefault(this object @this)
        {
            try
            {
                return Convert.ToDouble(@this);
            }
            catch (Exception)
            {
                return default(double);
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a double or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to a double.</returns>
        public static double ToDoubleOrDefault(this object @this, double defaultValue)
        {
            try
            {
                return Convert.ToDouble(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a double or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to a double.</returns>
        public static double ToDoubleOrDefault(this object @this, double defaultValue, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValue;
            }

            try
            {
                return Convert.ToDouble(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a double or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to a double.</returns>
        public static double ToDoubleOrDefault(this object @this, Func<double> defaultValueFactory)
        {
            try
            {
                return Convert.ToDouble(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a double or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to a double.</returns>
        public static double ToDoubleOrDefault(this object @this, Func<double> defaultValueFactory, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValueFactory();
            }

            try
            {
                return Convert.ToDouble(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToBoolean
        /// <summary>
        /// 转为布尔型，转换失败时返回默认值。支持大小写True/False、0和非零
        /// </summary>
        /// <param name="this">待转换对象</param>
        /// <param name="defaultValue">默认值。待转换对象无效时使用</param>
        /// <returns></returns>
        public static bool ToBoolean(this object @this, bool defaultValue = false)
        {
            if (@this == null || @this == DBNull.Value) return defaultValue;
            // 特殊处理字符串，也是最常见的
            if (@this is string str)
            {
                str = str.ToDBC().Trim();
                if (str.IsNullOrEmpty()) return defaultValue;
                if (bool.TryParse(str, out var b)) return b;
                if (string.Equals(str, bool.TrueString, StringComparison.OrdinalIgnoreCase)) return true;
                if (string.Equals(str, bool.FalseString, StringComparison.OrdinalIgnoreCase)) return false;
                // 特殊处理用数字0和1表示布尔型
                if (int.TryParse(str, out var n)) return n > 0;
                return defaultValue;
            }
            try
            {
                return Convert.ToBoolean(@this);
            }
            catch
            {
                return defaultValue;
            }
        }
        #endregion

        #region ToBooleanOrDefault
        /// <summary>
        /// An object extension method that converts this object to a boolean or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a bool.</returns>
        public static bool ToBooleanOrDefault(this object @this)
        {
            try
            {
                return Convert.ToBoolean(@this);
            }
            catch (Exception)
            {
                return default(bool);
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a boolean or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">true to default value.</param>
        /// <returns>The given data converted to a bool.</returns>
        public static bool ToBooleanOrDefault(this object @this, bool defaultValue)
        {
            try
            {
                return Convert.ToBoolean(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }
        /// <summary>
        /// An object extension method that converts this object to a boolean or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">true to default value.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to a bool.</returns>
        public static bool ToBooleanOrDefault(this object @this, bool defaultValue, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValue;
            }

            try
            {
                return Convert.ToBoolean(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a boolean or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to a bool.</returns>
        public static bool ToBooleanOrDefault(this object @this, Func<bool> defaultValueFactory)
        {
            try
            {
                return Convert.ToBoolean(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a boolean or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to a bool.</returns>
        public static bool ToBooleanOrDefault(this object @this, Func<bool> defaultValueFactory, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValueFactory();
            }

            try
            {
                return Convert.ToBoolean(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToDateTime
        /// <summary>
        /// 转为时间日期，转换失败时返回最小时间。支持字符串、整数（Unix秒）
        /// </summary>
        /// <param name="this">待转换对象</param>
        /// <returns></returns>
        public static DateTime ToDateTime(this object @this)
        {
            return ToDateTime(@this, DateTime.MinValue);
        }

        /// <summary>
        /// 转为时间日期，转换失败时返回默认值
        /// </summary>
        /// <remarks>
        /// <see cref="DateTime.MinValue"/>不是常量无法做默认值
        /// </remarks>
        /// <param name="this">待转换对象</param>
        /// <param name="defaultValue">默认值。待转换对象无效时使用</param>
        /// <returns></returns>
        public static DateTime ToDateTime(this object @this, DateTime defaultValue)
        {
            if (@this == null || @this == DBNull.Value) return defaultValue;
            // 特殊处理字符串，也是最常见的
            if (@this is string str)
            {
                str = str.ToDBC().Trim();
                if (str.IsNullOrEmpty()) return defaultValue;
                if (DateTime.TryParse(str, out var n)) return n;
                if (str.Contains("-") && DateTime.TryParseExact(str, "yyyy-M-d", null, DateTimeStyles.None, out n)) return n;
                if (str.Contains("/") && DateTime.TryParseExact(str, "yyyy/M/d", null, DateTimeStyles.None, out n)) return n;
                if (DateTime.TryParse(str, out n)) return n;
                return defaultValue;
            }
            // 特殊处理整数，Unix秒，绝对时间差，不考虑UTC时间和本地时间。
            var _dt1970 = new DateTime(1970, 1, 1);
            if (@this is int k) return _dt1970.AddSeconds(k);
            if (@this is long m)
            {
                if (m > 100 * 365 * 24 * 3600L)
                    return _dt1970.AddMilliseconds(m);
                else
                    return _dt1970.AddSeconds(m);
            }
            try
            {
                return Convert.ToDateTime(@this);
            }
            catch
            {
                return defaultValue;
            }
        }
        #endregion

        #region ToDateTimeOrDefault
        /// <summary>
        /// An object extension method that converts this object to a date time or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a DateTime.</returns>
        public static DateTime ToDateTimeOrDefault(this object @this)
        {
            try
            {
                return Convert.ToDateTime(@this);
            }
            catch (Exception)
            {
                return default(DateTime);
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a date time or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to a DateTime.</returns>
        public static DateTime ToDateTimeOrDefault(this object @this, DateTime defaultValue)
        {
            try
            {
                return Convert.ToDateTime(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a date time or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to a DateTime.</returns>
        public static DateTime ToDateTimeOrDefault(this object @this, DateTime defaultValue, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValue;
            }

            try
            {
                return Convert.ToDateTime(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a date time or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to a DateTime.</returns>
        public static DateTime ToDateTimeOrDefault(this object @this, Func<DateTime> defaultValueFactory)
        {
            try
            {
                return Convert.ToDateTime(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a date time or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to a DateTime.</returns>
        public static DateTime ToDateTimeOrDefault(this object @this, Func<DateTime> defaultValueFactory, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValueFactory();
            }

            try
            {
                return Convert.ToDateTime(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToDateTimeOffSet
        /// <summary>
        /// An object extension method that converts the @this to a date time off set.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as a DateTimeOffset.</returns>
        public static DateTimeOffset ToDateTimeOffSet(this object @this)
        {
            return new DateTimeOffset(Convert.ToDateTime(@this), TimeSpan.Zero);
        }
        #endregion

        #region ToDateTimeOffSetOrDefault
        /// <summary>
        /// An object extension method that converts this object to a date time off set or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a DateTimeOffset.</returns>
        public static DateTimeOffset ToDateTimeOffSetOrDefault(this object @this)
        {
            try
            {
                return new DateTimeOffset(Convert.ToDateTime(@this), TimeSpan.Zero);
            }
            catch (Exception)
            {
                return default(DateTimeOffset);
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a date time off set or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to a DateTimeOffset.</returns>
        public static DateTimeOffset ToDateTimeOffSetOrDefault(this object @this, DateTimeOffset defaultValue)
        {
            try
            {
                return new DateTimeOffset(Convert.ToDateTime(@this), TimeSpan.Zero);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a date time off set or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to a DateTimeOffset.</returns>
        public static DateTimeOffset ToDateTimeOffSetOrDefault(this object @this, DateTimeOffset defaultValue, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValue;
            }

            try
            {
                return new DateTimeOffset(Convert.ToDateTime(@this), TimeSpan.Zero);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a date time off set or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to a DateTimeOffset.</returns>
        public static DateTimeOffset ToDateTimeOffSetOrDefault(this object @this, Func<DateTimeOffset> defaultValueFactory)
        {
            try
            {
                return new DateTimeOffset(Convert.ToDateTime(@this), TimeSpan.Zero);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a date time off set or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to a DateTimeOffset.</returns>
        public static DateTimeOffset ToDateTimeOffSetOrDefault(this object @this, Func<DateTimeOffset> defaultValueFactory, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValueFactory();
            }

            try
            {
                return new DateTimeOffset(Convert.ToDateTime(@this), TimeSpan.Zero);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToByte
        /// <summary>
        /// An object extension method that converts the @this to a byte.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as a byte.</returns>
        public static byte ToByte(this object @this)
        {
            return Convert.ToByte(@this);
        }
        #endregion

        #region ToByteOrDefault
        /// <summary>
        /// An object extension method that converts this object to a byte or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a byte.</returns>
        public static byte ToByteOrDefault(this object @this)
        {
            try
            {
                return Convert.ToByte(@this);
            }
            catch (Exception)
            {
                return default(byte);
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a byte or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to a byte.</returns>
        public static byte ToByteOrDefault(this object @this, byte defaultValue)
        {
            try
            {
                return Convert.ToByte(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a byte or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to a byte.</returns>
        public static byte ToByteOrDefault(this object @this, byte defaultValue, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValue;
            }

            try
            {
                return Convert.ToByte(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a byte or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to a byte.</returns>
        public static byte ToByteOrDefault(this object @this, Func<byte> defaultValueFactory)
        {
            try
            {
                return Convert.ToByte(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a byte or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to a byte.</returns>
        public static byte ToByteOrDefault(this object @this, Func<byte> defaultValueFactory, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValueFactory();
            }

            try
            {
                return Convert.ToByte(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToChar
        /// <summary>
        /// An object extension method that converts the @this to a character.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as a char.</returns>
        public static char ToChar(this object @this)
        {
            return Convert.ToChar(@this);
        }
        #endregion

        #region ToCharOrDefault
        /// <summary>
        /// An object extension method that converts this object to a character or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a char.</returns>
        public static char ToCharOrDefault(this object @this)
        {
            try
            {
                return Convert.ToChar(@this);
            }
            catch (Exception)
            {
                return default(char);
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a character or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to a char.</returns>
        public static char ToCharOrDefault(this object @this, char defaultValue)
        {
            try
            {
                return Convert.ToChar(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a character or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to a char.</returns>
        public static char ToCharOrDefault(this object @this, char defaultValue, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValue;
            }

            try
            {
                return Convert.ToChar(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a character or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to a char.</returns>
        public static char ToCharOrDefault(this object @this, Func<char> defaultValueFactory)
        {
            try
            {
                return Convert.ToChar(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a character or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to a char.</returns>
        public static char ToCharOrDefault(this object @this, Func<char> defaultValueFactory, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValueFactory();
            }

            try
            {
                return Convert.ToChar(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToDecimal
        /// <summary>
        /// An object extension method that converts the @this to a decimal.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as a decimal.</returns>
        public static decimal ToDecimal(this object @this)
        {
            return Convert.ToDecimal(@this);
        }
        #endregion

        #region ToDecimalOrDefault
        /// <summary>
        /// An object extension method that converts this object to a decimal or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a decimal.</returns>
        public static decimal ToDecimalOrDefault(this object @this)
        {
            try
            {
                return Convert.ToDecimal(@this);
            }
            catch (Exception)
            {
                return default(decimal);
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a decimal or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to a decimal.</returns>
        public static decimal ToDecimalOrDefault(this object @this, decimal defaultValue)
        {
            try
            {
                return Convert.ToDecimal(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a decimal or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to a decimal.</returns>
        public static decimal ToDecimalOrDefault(this object @this, decimal defaultValue, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValue;
            }

            try
            {
                return Convert.ToDecimal(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a decimal or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to a decimal.</returns>
        public static decimal ToDecimalOrDefault(this object @this, Func<decimal> defaultValueFactory)
        {
            try
            {
                return Convert.ToDecimal(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a decimal or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to a decimal.</returns>
        public static decimal ToDecimalOrDefault(this object @this, Func<decimal> defaultValueFactory, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValueFactory();
            }

            try
            {
                return Convert.ToDecimal(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToFloat
        /// <summary>
        /// An object extension method that converts the @this to a float.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as a float.</returns>
        public static float ToFloat(this object @this)
        {
            return Convert.ToSingle(@this);
        }
        #endregion

        #region ToFloatOrDefault
        /// <summary>
        /// An object extension method that converts this object to a float or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a float.</returns>
        public static float ToFloatOrDefault(this object @this)
        {
            try
            {
                return Convert.ToSingle(@this);
            }
            catch (Exception)
            {
                return default(float);
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a float or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to a float.</returns>
        public static float ToFloatOrDefault(this object @this, float defaultValue)
        {
            try
            {
                return Convert.ToSingle(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a float or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to a float.</returns>
        public static float ToFloatOrDefault(this object @this, float defaultValue, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValue;
            }

            try
            {
                return Convert.ToSingle(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a float or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to a float.</returns>
        public static float ToFloatOrDefault(this object @this, Func<float> defaultValueFactory)
        {
            try
            {
                return Convert.ToSingle(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a float or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to a float.</returns>
        public static float ToFloatOrDefault(this object @this, Func<float> defaultValueFactory, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValueFactory();
            }

            try
            {
                return Convert.ToSingle(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToGuid
        /// <summary>
        /// An object extension method that converts the @this to a unique identifier.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as a GUID.</returns>
        public static Guid ToGuid(this object @this)
        {
            return new Guid(@this.ToString());
        }
        #endregion

        #region ToGuidOrDefault
        /// <summary>
        /// An object extension method that converts this object to a unique identifier or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a GUID.</returns>
        public static Guid ToGuidOrDefault(this object @this)
        {
            try
            {
                return new Guid(@this.ToString());
            }
            catch (Exception)
            {
                return Guid.Empty;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a unique identifier or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to a GUID.</returns>
        public static Guid ToGuidOrDefault(this object @this, Guid defaultValue)
        {
            try
            {
                return new Guid(@this.ToString());
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a unique identifier or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to a GUID.</returns>
        public static Guid ToGuidOrDefault(this object @this, Guid defaultValue, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValue;
            }

            try
            {
                return new Guid(@this.ToString());
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a unique identifier or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to a GUID.</returns>
        public static Guid ToGuidOrDefault(this object @this, Func<Guid> defaultValueFactory)
        {
            try
            {
                return new Guid(@this.ToString());
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a unique identifier or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to a GUID.</returns>
        public static Guid ToGuidOrDefault(this object @this, Func<Guid> defaultValueFactory, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValueFactory();
            }

            try
            {
                return new Guid(@this.ToString());
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToInt16
        /// <summary>
        /// An object extension method that converts the @this to an int 16.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as a short.</returns>
        public static short ToInt16(this object @this)
        {
            return Convert.ToInt16(@this);
        }
        #endregion

        #region ToInt16OrDefault
        /// <summary>
        /// An object extension method that converts this object to an int 16 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a short.</returns>
        public static short ToInt16OrDefault(this object @this)
        {
            try
            {
                return Convert.ToInt16(@this);
            }
            catch (Exception)
            {
                return default(short);
            }
        }

        /// <summary>
        /// An object extension method that converts this object to an int 16 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to a short.</returns>
        public static short ToInt16OrDefault(this object @this, short defaultValue)
        {
            try
            {
                return Convert.ToInt16(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to an int 16 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to a short.</returns>
        public static short ToInt16OrDefault(this object @this, short defaultValue, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValue;
            }

            try
            {
                return Convert.ToInt16(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to an int 16 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to a short.</returns>
        public static short ToInt16OrDefault(this object @this, Func<short> defaultValueFactory)
        {
            try
            {
                return Convert.ToInt16(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }

        /// <summary>
        /// An object extension method that converts this object to an int 16 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to a short.</returns>
        public static short ToInt16OrDefault(this object @this, Func<short> defaultValueFactory, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValueFactory();
            }

            try
            {
                return Convert.ToInt16(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToInt32
        /// <summary>
        /// An object extension method that converts the @this to an int 32.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as an int.</returns>
        public static int ToInt32(this object @this)
        {
            return Convert.ToInt32(@this);
        }
        #endregion

        #region ToInt32OrDefault
        /// <summary>
        /// An object extension method that converts this object to an int 32 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to an int.</returns>
        public static int ToInt32OrDefault(this object @this)
        {
            try
            {
                return Convert.ToInt32(@this);
            }
            catch (Exception)
            {
                return default(int);
            }
        }

        /// <summary>
        /// An object extension method that converts this object to an int 32 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to an int.</returns>
        public static int ToInt32OrDefault(this object @this, int defaultValue)
        {
            try
            {
                return Convert.ToInt32(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to an int 32 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to an int.</returns>
        public static int ToInt32OrDefault(this object @this, int defaultValue, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValue;
            }

            try
            {
                return Convert.ToInt32(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to an int 32 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to an int.</returns>
        public static int ToInt32OrDefault(this object @this, Func<int> defaultValueFactory)
        {
            try
            {
                return Convert.ToInt32(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }

        /// <summary>
        /// An object extension method that converts this object to an int 32 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to an int.</returns>
        public static int ToInt32OrDefault(this object @this, Func<int> defaultValueFactory, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValueFactory();
            }

            try
            {
                return Convert.ToInt32(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToInt64
        /// <summary>
        /// An object extension method that converts the @this to an int 64.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as a long.</returns>
        public static long ToInt64(this object @this)
        {
            return Convert.ToInt64(@this);
        }
        #endregion

        #region ToInt64OrDefault
        /// <summary>
        /// An object extension method that converts this object to an int 64 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a long.</returns>
        public static long ToInt64OrDefault(this object @this)
        {
            try
            {
                return Convert.ToInt64(@this);
            }
            catch (Exception)
            {
                return default(long);
            }
        }

        /// <summary>
        /// An object extension method that converts this object to an int 64 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to a long.</returns>
        public static long ToInt64OrDefault(this object @this, long defaultValue)
        {
            try
            {
                return Convert.ToInt64(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to an int 64 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to a long.</returns>
        public static long ToInt64OrDefault(this object @this, long defaultValue, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValue;
            }

            try
            {
                return Convert.ToInt64(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to an int 64 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to a long.</returns>
        public static long ToInt64OrDefault(this object @this, Func<long> defaultValueFactory)
        {
            try
            {
                return Convert.ToInt64(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }

        /// <summary>
        /// An object extension method that converts this object to an int 64 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to a long.</returns>
        public static long ToInt64OrDefault(this object @this, Func<long> defaultValueFactory, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValueFactory();
            }

            try
            {
                return Convert.ToInt64(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToSByte
        /// <summary>
        /// An object extension method that converts the @this to the s byte.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as a sbyte.</returns>
        public static sbyte ToSByte(this object @this)
        {
            return Convert.ToSByte(@this);
        }
        #endregion

        #region ToSByteOrDefault
        /// <summary>
        /// An object extension method that converts this object to the s byte or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a sbyte.</returns>
        public static sbyte ToSByteOrDefault(this object @this)
        {
            try
            {
                return Convert.ToSByte(@this);
            }
            catch (Exception)
            {
                return default(sbyte);
            }
        }

        /// <summary>
        /// An object extension method that converts this object to the s byte or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to a sbyte.</returns>
        public static sbyte ToSByteOrDefault(this object @this, sbyte defaultValue)
        {
            try
            {
                return Convert.ToSByte(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to the s byte or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to a sbyte.</returns>
        public static sbyte ToSByteOrDefault(this object @this, sbyte defaultValue, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValue;
            }

            try
            {
                return Convert.ToSByte(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to the s byte or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to a sbyte.</returns>
        public static sbyte ToSByteOrDefault(this object @this, Func<sbyte> defaultValueFactory)
        {
            try
            {
                return Convert.ToSByte(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }

        /// <summary>
        /// An object extension method that converts this object to the s byte or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to a sbyte.</returns>
        public static sbyte ToSByteOrDefault(this object @this, Func<sbyte> defaultValueFactory, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValueFactory();
            }

            try
            {
                return Convert.ToSByte(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToShort
        /// <summary>
        /// An object extension method that converts the @this to a short.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as a short.</returns>
        public static short ToShort(this object @this)
        {
            return Convert.ToInt16(@this);
        }
        #endregion

        #region ToShortOrDefault
        /// <summary>
        /// An object extension method that converts this object to a short or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a short.</returns>
        public static short ToShortOrDefault(this object @this)
        {
            try
            {
                return Convert.ToInt16(@this);
            }
            catch (Exception)
            {
                return default(short);
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a short or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to a short.</returns>
        public static short ToShortOrDefault(this object @this, short defaultValue)
        {
            try
            {
                return Convert.ToInt16(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a short or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to a short.</returns>
        public static short ToShortOrDefault(this object @this, short defaultValue, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValue;
            }

            try
            {
                return Convert.ToInt16(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a short or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to a short.</returns>
        public static short ToShortOrDefault(this object @this, Func<short> defaultValueFactory)
        {
            try
            {
                return Convert.ToInt16(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a short or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to a short.</returns>
        public static short ToShortOrDefault(this object @this, Func<short> defaultValueFactory, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValueFactory();
            }

            try
            {
                return Convert.ToInt16(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToSingle
        /// <summary>
        /// An object extension method that converts the @this to a single.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as a float.</returns>
        public static float ToSingle(this object @this)
        {
            return Convert.ToSingle(@this);
        }
        #endregion

        #region ToSingleOrDefault
        /// <summary>
        /// An object extension method that converts this object to a single or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a float.</returns>
        public static float ToSingleOrDefault(this object @this)
        {
            try
            {
                return Convert.ToSingle(@this);
            }
            catch (Exception)
            {
                return default(float);
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a single or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to a float.</returns>
        public static float ToSingleOrDefault(this object @this, float defaultValue)
        {
            try
            {
                return Convert.ToSingle(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a single or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to a float.</returns>
        public static float ToSingleOrDefault(this object @this, float defaultValue, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValue;
            }

            try
            {
                return Convert.ToSingle(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a single or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to a float.</returns>
        public static float ToSingleOrDefault(this object @this, Func<float> defaultValueFactory)
        {
            try
            {
                return Convert.ToSingle(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a single or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to a float.</returns>
        public static float ToSingleOrDefault(this object @this, Func<float> defaultValueFactory, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValueFactory();
            }

            try
            {
                return Convert.ToSingle(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToString
        /// <summary>
        /// An object extension method that convert this object into a string representation.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>A string that represents this object.</returns>
        public static string ToString(this object @this)
        {
            return Convert.ToString(@this);
        }
        #endregion

        #region ToStringOrDefault
        /// <summary>
        /// An object extension method that converts this object to a string or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToStringOrDefault(this object @this)
        {
            try
            {
                return Convert.ToString(@this);
            }
            catch (Exception)
            {
                return default(string);
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a string or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToStringOrDefault(this object @this, string defaultValue)
        {
            try
            {
                return Convert.ToString(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a string or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToStringOrDefault(this object @this, string defaultValue, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValue;
            }

            try
            {
                return Convert.ToString(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a string or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToStringOrDefault(this object @this, Func<string> defaultValueFactory)
        {
            try
            {
                return Convert.ToString(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a string or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to a string.</returns>
        public static string ToStringOrDefault(this object @this, Func<string> defaultValueFactory, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValueFactory();
            }

            try
            {
                return Convert.ToString(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToUInt16
        /// <summary>
        /// An object extension method that converts the @this to an u int 16.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as an ushort.</returns>
        public static ushort ToUInt16(this object @this)
        {
            return Convert.ToUInt16(@this);
        }
        #endregion

        #region ToUInt16OrDefault
        /// <summary>
        /// An object extension method that converts this object to an u int 16 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to an ushort.</returns>
        public static ushort ToUInt16OrDefault(this object @this)
        {
            try
            {
                return Convert.ToUInt16(@this);
            }
            catch (Exception)
            {
                return default(ushort);
            }
        }

        /// <summary>
        /// An object extension method that converts this object to an u int 16 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to an ushort.</returns>
        public static ushort ToUInt16OrDefault(this object @this, ushort defaultValue)
        {
            try
            {
                return Convert.ToUInt16(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to an u int 16 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to an ushort.</returns>
        public static ushort ToUInt16OrDefault(this object @this, ushort defaultValue, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValue;
            }

            try
            {
                return Convert.ToUInt16(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to an u int 16 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to an ushort.</returns>
        public static ushort ToUInt16OrDefault(this object @this, Func<ushort> defaultValueFactory)
        {
            try
            {
                return Convert.ToUInt16(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }

        /// <summary>
        /// An object extension method that converts this object to an u int 16 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to an ushort.</returns>
        public static ushort ToUInt16OrDefault(this object @this, Func<ushort> defaultValueFactory, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValueFactory();
            }

            try
            {
                return Convert.ToUInt16(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToUInt32
        /// <summary>
        /// An object extension method that converts the @this to an u int 32.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as an uint.</returns>
        public static uint ToUInt32(this object @this)
        {
            return Convert.ToUInt32(@this);
        }
        #endregion

        #region ToUInt32OrDefault
        /// <summary>
        /// An object extension method that converts this object to an u int 32 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to an uint.</returns>
        public static uint ToUInt32OrDefault(this object @this)
        {
            try
            {
                return Convert.ToUInt32(@this);
            }
            catch (Exception)
            {
                return default(uint);
            }
        }

        /// <summary>
        /// An object extension method that converts this object to an u int 32 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to an uint.</returns>
        public static uint ToUInt32OrDefault(this object @this, uint defaultValue)
        {
            try
            {
                return Convert.ToUInt32(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to an u int 32 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to an uint.</returns>
        public static uint ToUInt32OrDefault(this object @this, uint defaultValue, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValue;
            }

            try
            {
                return Convert.ToUInt32(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to an u int 32 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to an uint.</returns>
        public static uint ToUInt32OrDefault(this object @this, Func<uint> defaultValueFactory)
        {
            try
            {
                return Convert.ToUInt32(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }

        /// <summary>
        /// An object extension method that converts this object to an u int 32 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to an uint.</returns>
        public static uint ToUInt32OrDefault(this object @this, Func<uint> defaultValueFactory, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValueFactory();
            }

            try
            {
                return Convert.ToUInt32(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToUInt64
        /// <summary>
        /// An object extension method that converts the @this to an u int 64.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as an ulong.</returns>
        public static ulong ToUInt64(this object @this)
        {
            return Convert.ToUInt64(@this);
        }
        #endregion

        #region ToUInt64OrDefault
        /// <summary>
        /// An object extension method that converts this object to an u int 64 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to an ulong.</returns>
        public static ulong ToUInt64OrDefault(this object @this)
        {
            try
            {
                return Convert.ToUInt64(@this);
            }
            catch (Exception)
            {
                return default(ulong);
            }
        }

        /// <summary>
        /// An object extension method that converts this object to an u int 64 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to an ulong.</returns>
        public static ulong ToUInt64OrDefault(this object @this, ulong defaultValue)
        {
            try
            {
                return Convert.ToUInt64(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to an u int 64 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to an ulong.</returns>
        public static ulong ToUInt64OrDefault(this object @this, ulong defaultValue, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValue;
            }

            try
            {
                return Convert.ToUInt64(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to an u int 64 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to an ulong.</returns>
        public static ulong ToUInt64OrDefault(this object @this, Func<ulong> defaultValueFactory)
        {
            try
            {
                return Convert.ToUInt64(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }

        /// <summary>
        /// An object extension method that converts this object to an u int 64 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to an ulong.</returns>
        public static ulong ToUInt64OrDefault(this object @this, Func<ulong> defaultValueFactory, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValueFactory();
            }

            try
            {
                return Convert.ToUInt64(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToULong
        /// <summary>
        /// An object extension method that converts the @this to an u long.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as an ulong.</returns>
        public static ulong ToULong(this object @this)
        {
            return Convert.ToUInt64(@this);
        }
        #endregion

        #region ToULongOrDefault
        /// <summary>
        /// An object extension method that converts this object to an u long or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to an ulong.</returns>
        public static ulong ToULongOrDefault(this object @this)
        {
            try
            {
                return Convert.ToUInt64(@this);
            }
            catch (Exception)
            {
                return default(ulong);
            }
        }

        /// <summary>
        /// An object extension method that converts this object to an u long or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to an ulong.</returns>
        public static ulong ToULongOrDefault(this object @this, ulong defaultValue)
        {
            try
            {
                return Convert.ToUInt64(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to an u long or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to an ulong.</returns>
        public static ulong ToULongOrDefault(this object @this, ulong defaultValue, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValue;
            }

            try
            {
                return Convert.ToUInt64(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to an u long or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to an ulong.</returns>
        public static ulong ToULongOrDefault(this object @this, Func<ulong> defaultValueFactory)
        {
            try
            {
                return Convert.ToUInt64(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }

        /// <summary>
        /// An object extension method that converts this object to an u long or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to an ulong.</returns>
        public static ulong ToULongOrDefault(this object @this, Func<ulong> defaultValueFactory, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValueFactory();
            }

            try
            {
                return Convert.ToUInt64(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToUShort
        /// <summary>
        /// An object extension method that converts the @this to an u short.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as an ushort.</returns>
        public static ushort ToUShort(this object @this)
        {
            return Convert.ToUInt16(@this);
        }
        #endregion

        #region ToUShortOrDefault
        /// <summary>
        /// An object extension method that converts this object to an u short or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to an ushort.</returns>
        public static ushort ToUShortOrDefault(this object @this)
        {
            try
            {
                return Convert.ToUInt16(@this);
            }
            catch (Exception)
            {
                return default(ushort);
            }
        }

        /// <summary>
        /// An object extension method that converts this object to an u short or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to an ushort.</returns>
        public static ushort ToUShortOrDefault(this object @this, ushort defaultValue)
        {
            try
            {
                return Convert.ToUInt16(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to an u short or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to an ushort.</returns>
        public static ushort ToUShortOrDefault(this object @this, ushort defaultValue, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValue;
            }

            try
            {
                return Convert.ToUInt16(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to an u short or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to an ushort.</returns>
        public static ushort ToUShortOrDefault(this object @this, Func<ushort> defaultValueFactory)
        {
            try
            {
                return Convert.ToUInt16(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }

        /// <summary>
        /// An object extension method that converts this object to an u short or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <param name="useDefaultIfNull">true to use default if null.</param>
        /// <returns>The given data converted to an ushort.</returns>
        public static ushort ToUShortOrDefault(this object @this, Func<ushort> defaultValueFactory, bool useDefaultIfNull)
        {
            if (useDefaultIfNull && @this == null)
            {
                return defaultValueFactory();
            }

            try
            {
                return Convert.ToUInt16(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToNullableBoolean
        /// <summary>
        /// An object extension method that converts the @this to a nullable boolean.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as a bool?</returns>
        public static bool? ToNullableBoolean(this object @this)
        {
            if (@this == null || @this == DBNull.Value)
            {
                return null;
            }

            return Convert.ToBoolean(@this);
        }
        #endregion

        #region ToNullableBooleanOrDefault
        /// <summary>
        /// An object extension method that converts this object to a nullable boolean or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a bool?</returns>
        public static bool? ToNullableBooleanOrDefault(this object @this)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToBoolean(@this);
            }
            catch (Exception)
            {
                return default(bool);
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable boolean or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to a bool?</returns>
        public static bool? ToNullableBooleanOrDefault(this object @this, bool? defaultValue)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToBoolean(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable boolean or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to a bool?</returns>
        public static bool? ToNullableBooleanOrDefault(this object @this, Func<bool?> defaultValueFactory)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToBoolean(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToNullableByte
        /// <summary>
        /// An object extension method that converts the @this to a nullable byte.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as a byte?</returns>
        public static byte? ToNullableByte(this object @this)
        {
            if (@this == null || @this == DBNull.Value)
            {
                return null;
            }

            return Convert.ToByte(@this);
        }
        #endregion

        #region ToNullableByteOrDefault
        /// <summary>
        /// An object extension method that converts this object to a nullable byte or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a byte?</returns>
        public static byte? ToNullableByteOrDefault(this object @this)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToByte(@this);
            }
            catch (Exception)
            {
                return default(byte);
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable byte or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to a byte?</returns>
        public static byte? ToNullableByteOrDefault(this object @this, byte? defaultValue)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToByte(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable byte or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to a byte?</returns>
        public static byte? ToNullableByteOrDefault(this object @this, Func<byte?> defaultValueFactory)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToByte(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToNullableChar
        /// <summary>
        /// An object extension method that converts the @this to a nullable character.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as a char?</returns>
        public static char? ToNullableChar(this object @this)
        {
            if (@this == null || @this == DBNull.Value)
            {
                return null;
            }

            return Convert.ToChar(@this);
        }
        #endregion

        #region ToNullableCharOrDefault
        /// <summary>
        /// An object extension method that converts this object to a nullable character or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a char?</returns>
        public static char? ToNullableCharOrDefault(this object @this)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToChar(@this);
            }
            catch (Exception)
            {
                return default(char);
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable character or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to a char?</returns>
        public static char? ToNullableCharOrDefault(this object @this, char? defaultValue)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToChar(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable character or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to a char?</returns>
        public static char? ToNullableCharOrDefault(this object @this, Func<char?> defaultValueFactory)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToChar(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToNullableDateTime
        /// <summary>
        /// An object extension method that converts the @this to a nullable date time.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as a DateTime?</returns>
        public static DateTime? ToNullableDateTime(this object @this)
        {
            if (@this == null || @this == DBNull.Value)
            {
                return null;
            }

            return Convert.ToDateTime(@this);
        }
        #endregion

        #region ToNullableDateTimeOrDefault
        /// <summary>
        /// An object extension method that converts this object to a nullable date time or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a DateTime?</returns>
        public static DateTime? ToNullableDateTimeOrDefault(this object @this)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToDateTime(@this);
            }
            catch (Exception)
            {
                return default(DateTime);
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable date time or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to a DateTime?</returns>
        public static DateTime? ToNullableDateTimeOrDefault(this object @this, DateTime? defaultValue)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToDateTime(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable date time or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to a DateTime?</returns>
        public static DateTime? ToNullableDateTimeOrDefault(this object @this, Func<DateTime?> defaultValueFactory)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToDateTime(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToNullableDateTimeOffSet
        /// <summary>
        /// An object extension method that converts the @this to a nullable date time off set.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as a DateTimeOffset?</returns>
        public static DateTimeOffset? ToNullableDateTimeOffSet(this object @this)
        {
            if (@this == null || @this == DBNull.Value)
            {
                return null;
            }

            return new DateTimeOffset(Convert.ToDateTime(@this), TimeSpan.Zero);
        }
        #endregion

        #region ToNullableDateTimeOffSetOrDefault
        /// <summary>
        /// An object extension method that converts this object to a nullable date time off set or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a DateTimeOffset?</returns>
        public static DateTimeOffset? ToNullableDateTimeOffSetOrDefault(this object @this)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return new DateTimeOffset(Convert.ToDateTime(@this), TimeSpan.Zero);
            }
            catch (Exception)
            {
                return default(DateTimeOffset);
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable date time off set or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to a DateTimeOffset?</returns>
        public static DateTimeOffset? ToNullableDateTimeOffSetOrDefault(this object @this, DateTimeOffset? defaultValue)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return new DateTimeOffset(Convert.ToDateTime(@this), TimeSpan.Zero);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable date time off set or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to a DateTimeOffset?</returns>
        public static DateTimeOffset? ToNullableDateTimeOffSetOrDefault(this object @this, Func<DateTimeOffset?> defaultValueFactory)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return new DateTimeOffset(Convert.ToDateTime(@this), TimeSpan.Zero);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToNullableDecimal
        /// <summary>
        /// An object extension method that converts the @this to a nullable decimal.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as a decimal?</returns>
        public static decimal? ToNullableDecimal(this object @this)
        {
            if (@this == null || @this == DBNull.Value)
            {
                return null;
            }

            return Convert.ToDecimal(@this);
        }
        #endregion

        #region ToNullableDecimalOrDefault
        /// <summary>
        /// An object extension method that converts this object to a nullable decimal or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a decimal?</returns>
        public static decimal? ToNullableDecimalOrDefault(this object @this)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToDecimal(@this);
            }
            catch (Exception)
            {
                return default(decimal);
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable decimal or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to a decimal?</returns>
        public static decimal? ToNullableDecimalOrDefault(this object @this, decimal? defaultValue)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToDecimal(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable decimal or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to a decimal?</returns>
        public static decimal? ToNullableDecimalOrDefault(this object @this, Func<decimal?> defaultValueFactory)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToDecimal(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToNullableDouble
        /// <summary>
        /// An object extension method that converts the @this to a nullable double.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as a double?</returns>
        public static double? ToNullableDouble(this object @this)
        {
            if (@this == null || @this == DBNull.Value)
            {
                return null;
            }

            return Convert.ToDouble(@this);
        }
        #endregion

        #region ToNullableDoubleOrDefault
        /// <summary>
        /// An object extension method that converts this object to a nullable double or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a double?</returns>
        public static double? ToNullableDoubleOrDefault(this object @this)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToDouble(@this);
            }
            catch (Exception)
            {
                return default(double);
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable double or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to a double?</returns>
        public static double? ToNullableDoubleOrDefault(this object @this, double? defaultValue)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToDouble(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable double or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to a double?</returns>
        public static double? ToNullableDoubleOrDefault(this object @this, Func<double?> defaultValueFactory)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToDouble(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToNullableFloat
        /// <summary>
        /// An object extension method that converts the @this to a nullable float.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as a float?</returns>
        public static float? ToNullableFloat(this object @this)
        {
            if (@this == null || @this == DBNull.Value)
            {
                return null;
            }

            return Convert.ToSingle(@this);
        }
        #endregion

        #region ToNullableFloatOrDefault
        /// <summary>
        /// An object extension method that converts this object to a nullable float or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a float?</returns>
        public static float? ToNullableFloatOrDefault(this object @this)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToSingle(@this);
            }
            catch (Exception)
            {
                return default(float);
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable float or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to a float?</returns>
        public static float? ToNullableFloatOrDefault(this object @this, float? defaultValue)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToSingle(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable float or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to a float?</returns>
        public static float? ToNullableFloatOrDefault(this object @this, Func<float?> defaultValueFactory)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToSingle(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToNullableGuid
        /// <summary>
        /// An object extension method that converts the @this to a nullable unique identifier.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as a Guid?</returns>
        public static Guid? ToNullableGuid(this object @this)
        {
            if (@this == null || @this == DBNull.Value)
            {
                return null;
            }

            return new Guid(@this.ToString());
        }
        #endregion

        #region ToNullableGuidOrDefault
        /// <summary>
        /// An object extension method that converts this object to a nullable unique identifier or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a Guid?</returns>
        public static Guid? ToNullableGuidOrDefault(this object @this)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return new Guid(@this.ToString());
            }
            catch (Exception)
            {
                return Guid.Empty;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable unique identifier or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to a Guid?</returns>
        public static Guid? ToNullableGuidOrDefault(this object @this, Guid? defaultValue)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return new Guid(@this.ToString());
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable unique identifier or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to a Guid?</returns>
        public static Guid? ToNullableGuidOrDefault(this object @this, Func<Guid?> defaultValueFactory)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return new Guid(@this.ToString());
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToNullableInt16
        /// <summary>
        /// An object extension method that converts the @this to a nullable int 16.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as a short?</returns>
        public static short? ToNullableInt16(this object @this)
        {
            if (@this == null || @this == DBNull.Value)
            {
                return null;
            }

            return Convert.ToInt16(@this);
        }
        #endregion

        #region ToNullableInt16OrDefault
        /// <summary>
        /// An object extension method that converts this object to a nullable int 16 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a short?</returns>
        public static short? ToNullableInt16OrDefault(this object @this)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToInt16(@this);
            }
            catch (Exception)
            {
                return default(short);
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable int 16 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to a short?</returns>
        public static short? ToNullableInt16OrDefault(this object @this, short? defaultValue)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToInt16(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable int 16 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to a short?</returns>
        public static short? ToNullableInt16OrDefault(this object @this, Func<short?> defaultValueFactory)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToInt16(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToNullableInt32
        /// <summary>
        /// An object extension method that converts the @this to a nullable int 32.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as an int?</returns>
        public static int? ToNullableInt32(this object @this)
        {
            if (@this == null || @this == DBNull.Value)
            {
                return null;
            }

            return Convert.ToInt32(@this);
        }
        #endregion

        #region ToNullableInt32OrDefault
        /// <summary>
        /// An object extension method that converts this object to a nullable int 32 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to an int?</returns>
        public static int? ToNullableInt32OrDefault(this object @this)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToInt32(@this);
            }
            catch (Exception)
            {
                return default(int);
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable int 32 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to an int?</returns>
        public static int? ToNullableInt32OrDefault(this object @this, int? defaultValue)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToInt32(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable int 32 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to an int?</returns>
        public static int? ToNullableInt32OrDefault(this object @this, Func<int?> defaultValueFactory)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToInt32(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToNullableInt64
        /// <summary>
        /// An object extension method that converts the @this to a nullable int 64.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as a long?</returns>
        public static long? ToNullableInt64(this object @this)
        {
            if (@this == null || @this == DBNull.Value)
            {
                return null;
            }

            return Convert.ToInt64(@this);
        }
        #endregion

        #region ToNullableInt64OrDefault
        /// <summary>
        /// An object extension method that converts this object to a nullable int 64 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a long?</returns>
        public static long? ToNullableInt64OrDefault(this object @this)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToInt64(@this);
            }
            catch (Exception)
            {
                return default(long);
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable int 64 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to a long?</returns>
        public static long? ToNullableInt64OrDefault(this object @this, long? defaultValue)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToInt64(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable int 64 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to a long?</returns>
        public static long? ToNullableInt64OrDefault(this object @this, Func<long?> defaultValueFactory)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToInt64(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToNullableLong
        /// <summary>
        /// An object extension method that converts the @this to a nullable long.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as a long?</returns>
        public static long? ToNullableLong(this object @this)
        {
            if (@this == null || @this == DBNull.Value)
            {
                return null;
            }

            return Convert.ToInt64(@this);
        }
        #endregion

        #region ToNullableLongOrDefault
        /// <summary>
        /// An object extension method that converts this object to a nullable long or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a long?</returns>
        public static long? ToNullableLongOrDefault(this object @this)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToInt64(@this);
            }
            catch (Exception)
            {
                return default(long);
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable long or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to a long?</returns>
        public static long? ToNullableLongOrDefault(this object @this, long? defaultValue)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToInt64(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable long or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to a long?</returns>
        public static long? ToNullableLongOrDefault(this object @this, Func<long?> defaultValueFactory)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToInt64(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToNullableSByte
        /// <summary>
        /// An object extension method that converts the @this to a nullable s byte.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as a sbyte?</returns>
        public static sbyte? ToNullableSByte(this object @this)
        {
            if (@this == null || @this == DBNull.Value)
            {
                return null;
            }

            return Convert.ToSByte(@this);
        }
        #endregion

        #region ToNullableSByteOrDefault
        /// <summary>
        /// An object extension method that converts this object to a nullable s byte or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a sbyte?</returns>
        public static sbyte? ToNullableSByteOrDefault(this object @this)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToSByte(@this);
            }
            catch (Exception)
            {
                return default(sbyte);
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable s byte or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to a sbyte?</returns>
        public static sbyte? ToNullableSByteOrDefault(this object @this, sbyte? defaultValue)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToSByte(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable s byte or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to a sbyte?</returns>
        public static sbyte? ToNullableSByteOrDefault(this object @this, Func<sbyte?> defaultValueFactory)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToSByte(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToNullableShort
        /// <summary>
        /// An object extension method that converts the @this to a nullable short.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as a short?</returns>
        public static short? ToNullableShort(this object @this)
        {
            if (@this == null || @this == DBNull.Value)
            {
                return null;
            }

            return Convert.ToInt16(@this);
        }
        #endregion

        #region ToNullableShortOrDefault
        /// <summary>
        /// An object extension method that converts this object to a nullable short or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a short?</returns>
        public static short? ToNullableShortOrDefault(this object @this)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToInt16(@this);
            }
            catch (Exception)
            {
                return default(short);
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable short or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to a short?</returns>
        public static short? ToNullableShortOrDefault(this object @this, short? defaultValue)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToInt16(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable short or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to a short?</returns>
        public static short? ToNullableShortOrDefault(this object @this, Func<short?> defaultValueFactory)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToInt16(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToNullableSingle
        /// <summary>
        /// An object extension method that converts the @this to a nullable single.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as a float?</returns>
        public static float? ToNullableSingle(this object @this)
        {
            if (@this == null || @this == DBNull.Value)
            {
                return null;
            }

            return Convert.ToSingle(@this);
        }
        #endregion

        #region ToNullableSingleOrDefault
        /// <summary>
        /// An object extension method that converts this object to a nullable single or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to a float?</returns>
        public static float? ToNullableSingleOrDefault(this object @this)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToSingle(@this);
            }
            catch (Exception)
            {
                return default(float);
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable single or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to a float?</returns>
        public static float? ToNullableSingleOrDefault(this object @this, float? defaultValue)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToSingle(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable single or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to a float?</returns>
        public static float? ToNullableSingleOrDefault(this object @this, Func<float?> defaultValueFactory)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToSingle(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToNullableUInt16
        /// <summary>
        /// An object extension method that converts the @this to a nullable u int 16.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as an ushort?</returns>
        public static ushort? ToNullableUInt16(this object @this)
        {
            if (@this == null || @this == DBNull.Value)
            {
                return null;
            }

            return Convert.ToUInt16(@this);
        }
        #endregion

        #region ToNullableUInt16OrDefault
        /// <summary>
        /// An object extension method that converts this object to a nullable u int 16 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to an ushort?</returns>
        public static ushort? ToNullableUInt16OrDefault(this object @this)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToUInt16(@this);
            }
            catch (Exception)
            {
                return default(ushort);
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable u int 16 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to an ushort?</returns>
        public static ushort? ToNullableUInt16OrDefault(this object @this, ushort? defaultValue)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToUInt16(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable u int 16 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to an ushort?</returns>
        public static ushort? ToNullableUInt16OrDefault(this object @this, Func<ushort?> defaultValueFactory)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToUInt16(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToNullableUInt32
        /// <summary>
        /// An object extension method that converts the @this to a nullable u int 32.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as an uint?</returns>
        public static uint? ToNullableUInt32(this object @this)
        {
            if (@this == null || @this == DBNull.Value)
            {
                return null;
            }

            return Convert.ToUInt32(@this);
        }
        #endregion

        #region ToNullableUInt32OrDefault
        /// <summary>
        /// An object extension method that converts this object to a nullable u int 32 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to an uint?</returns>
        public static uint? ToNullableUInt32OrDefault(this object @this)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToUInt32(@this);
            }
            catch (Exception)
            {
                return default(uint);
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable u int 32 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to an uint?</returns>
        public static uint? ToNullableUInt32OrDefault(this object @this, uint? defaultValue)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToUInt32(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable u int 32 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to an uint?</returns>
        public static uint? ToNullableUInt32OrDefault(this object @this, Func<uint?> defaultValueFactory)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToUInt32(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToNullableUInt64
        /// <summary>
        /// An object extension method that converts the @this to a nullable u int 64.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as an ulong?</returns>
        public static ulong? ToNullableUInt64(this object @this)
        {
            if (@this == null || @this == DBNull.Value)
            {
                return null;
            }

            return Convert.ToUInt64(@this);
        }
        #endregion

        #region ToNullableUInt64OrDefault
        /// <summary>
        /// An object extension method that converts this object to a nullable u int 64 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to an ulong?</returns>
        public static ulong? ToNullableUInt64OrDefault(this object @this)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToUInt64(@this);
            }
            catch (Exception)
            {
                return default(ulong);
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable u int 64 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to an ulong?</returns>
        public static ulong? ToNullableUInt64OrDefault(this object @this, ulong? defaultValue)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToUInt64(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable u int 64 or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to an ulong?</returns>
        public static ulong? ToNullableUInt64OrDefault(this object @this, Func<ulong?> defaultValueFactory)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToUInt64(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToNullableULong
        /// <summary>
        /// An object extension method that converts the @this to a nullable u long.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as an ulong?</returns>
        public static ulong? ToNullableULong(this object @this)
        {
            if (@this == null || @this == DBNull.Value)
            {
                return null;
            }

            return Convert.ToUInt64(@this);
        }
        #endregion

        #region ToNullableULongOrDefault
        /// <summary>
        /// An object extension method that converts this object to a nullable u long or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to an ulong?</returns>
        public static ulong? ToNullableULongOrDefault(this object @this)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToUInt64(@this);
            }
            catch (Exception)
            {
                return default(ulong);
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable u long or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to an ulong?</returns>
        public static ulong? ToNullableULongOrDefault(this object @this, ulong? defaultValue)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToUInt64(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable u long or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to an ulong?</returns>
        public static ulong? ToNullableULongOrDefault(this object @this, Func<ulong?> defaultValueFactory)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToUInt64(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion

        #region ToNullableUShort
        /// <summary>
        /// An object extension method that converts the @this to a nullable u short.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as an ushort?</returns>
        public static ushort? ToNullableUShort(this object @this)
        {
            if (@this == null || @this == DBNull.Value)
            {
                return null;
            }

            return Convert.ToUInt16(@this);
        }
        #endregion

        #region ToNullableUShortOrDefault
        /// <summary>
        /// An object extension method that converts this object to a nullable u short or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The given data converted to an ushort?</returns>
        public static ushort? ToNullableUShortOrDefault(this object @this)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToUInt16(@this);
            }
            catch (Exception)
            {
                return default(ushort);
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable u short or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The given data converted to an ushort?</returns>
        public static ushort? ToNullableUShortOrDefault(this object @this, ushort? defaultValue)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToUInt16(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// An object extension method that converts this object to a nullable u short or default.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The given data converted to an ushort?</returns>
        public static ushort? ToNullableUShortOrDefault(this object @this, Func<ushort?> defaultValueFactory)
        {
            try
            {
                if (@this == null || @this == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToUInt16(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }
        #endregion
        #endregion

        #region IsValidValueType
        #region IsValidBoolean
        /// <summary>
        /// An object extension method that query if '@this' is valid bool.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>true if valid bool, false if not.</returns>
        public static bool IsValidBoolean(this object @this)
        {
            if (@this == null)
                return false;

            return bool.TryParse(@this.ToString(), out bool result);
        }
        #endregion

        #region IsValidByte
        /// <summary>
        /// An object extension method that query if '@this' is valid byte.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>true if valid byte, false if not.</returns>
        public static bool IsValidByte(this object @this)
        {
            if (@this == null)
                return false;

            return byte.TryParse(@this.ToString(), out byte result);
        }
        #endregion

        #region IsValidChar
        /// <summary>
        /// An object extension method that query if '@this' is valid char.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>true if valid char, false if not.</returns>
        public static bool IsValidChar(this object @this)
        {
            if (@this == null)
                return false;

            return char.TryParse(@this.ToString(), out char result);
        }
        #endregion

        #region IsValidDateTime
        /// <summary>
        /// An object extension method that query if '@this' is valid System.DateTime.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>true if valid System.DateTime, false if not.</returns>
        public static bool IsValidDateTime(this object @this)
        {
            if (@this == null)
                return false;

            return DateTime.TryParse(@this.ToString(), out DateTime result);
        }
        #endregion

        #region IsValidDateTimeOffSet
        /// <summary>
        /// An object extension method that query if '@this' is valid System.DateTimeOffset.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>true if valid System.DateTimeOffset, false if not.</returns>
        public static bool IsValidDateTimeOffSet(this object @this)
        {
            if (@this == null)
                return false;

            return DateTimeOffset.TryParse(@this.ToString(), out DateTimeOffset result);
        }
        #endregion

        #region IsValidDecimal
        /// <summary>
        /// An object extension method that query if '@this' is valid decimal.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>true if valid decimal, false if not.</returns>
        public static bool IsValidDecimal(this object @this)
        {
            if (@this == null)
                return false;

            return decimal.TryParse(@this.ToString(), out decimal result);
        }
        #endregion

        #region IsValidDouble
        /// <summary>
        /// An object extension method that query if '@this' is valid double.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>true if valid double, false if not.</returns>
        public static bool IsValidDouble(this object @this)
        {
            if (@this == null)
                return false;

            return double.TryParse(@this.ToString(), out double result);
        }
        #endregion

        #region IsValidFloat
        /// <summary>
        /// An object extension method that query if '@this' is valid float.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>true if valid float, false if not.</returns>
        public static bool IsValidFloat(this object @this)
        {
            if (@this == null)
                return false;

            return float.TryParse(@this.ToString(), out float result);
        }
        #endregion

        #region IsValidGuid
        /// <summary>
        /// An object extension method that query if '@this' is valid System.Guid.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>true if valid System.Guid, false if not.</returns>
        public static bool IsValidGuid(this object @this)
        {
            if (@this == null)
                return false;

            return Guid.TryParse(@this.ToString(), out Guid result);
        }
        #endregion

        #region IsValidInt16
        /// <summary>
        /// An object extension method that query if '@this' is valid short.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>true if valid short, false if not.</returns>
        public static bool IsValidInt16(this object @this)
        {
            if (@this == null)
                return false;

            return short.TryParse(@this.ToString(), out short result);
        }
        #endregion

        #region IsValidInt32
        /// <summary>
        /// An object extension method that query if '@this' is valid int.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>true if valid int, false if not.</returns>
        public static bool IsValidInt32(this object @this)
        {
            if (@this == null)
                return false;

            return int.TryParse(@this.ToString(), out int result);
        }
        #endregion

        #region IsValidInt64
        /// <summary>
        /// An object extension method that query if '@this' is valid long.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>true if valid long, false if not.</returns>
        public static bool IsValidInt64(this object @this)
        {
            if (@this == null)
                return false;

            return long.TryParse(@this.ToString(), out long result);
        }
        #endregion

        #region IsValidLong
        /// <summary>
        /// An object extension method that query if '@this' is valid long.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>true if valid long, false if not.</returns>
        public static bool IsValidLong(this object @this)
        {
            if (@this == null)
                return false;

            return long.TryParse(@this.ToString(), out long result);
        }
        #endregion

        #region IsValidSByte
        /// <summary>
        /// An object extension method that query if '@this' is valid sbyte.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>true if valid sbyte, false if not.</returns>
        public static bool IsValidSByte(this object @this)
        {
            if (@this == null)
                return false;

            return sbyte.TryParse(@this.ToString(), out sbyte result);
        }
        #endregion

        #region IsValidShort
        /// <summary>
        /// An object extension method that query if '@this' is valid short.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>true if valid short, false if not.</returns>
        public static bool IsValidShort(this object @this)
        {
            if (@this == null)
                return false;

            return short.TryParse(@this.ToString(), out short result);
        }
        #endregion

        #region IsValidSingle
        /// <summary>
        /// An object extension method that query if '@this' is valid float.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>true if valid float, false if not.</returns>
        public static bool IsValidSingle(this object @this)
        {
            if (@this == null)
                return false;

            return float.TryParse(@this.ToString(), out float result);
        }
        #endregion

        #region IsValidString
        /// <summary>
        /// An object extension method that query if '@this' is valid string.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>true if valid string, false if not.</returns>
        public static bool IsValidString(this object @this)
        {
            return true;
        }
        #endregion

        #region IsValidUInt16
        /// <summary>
        /// An object extension method that query if '@this' is valid ushort.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>true if valid ushort, false if not.</returns>
        public static bool IsValidUInt16(this object @this)
        {
            if (@this == null)
                return false;

            return ushort.TryParse(@this.ToString(), out ushort result);
        }
        #endregion

        #region IsValidUInt32
        /// <summary>
        /// An object extension method that query if '@this' is valid uint.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>true if valid uint, false if not.</returns>
        public static bool IsValidUInt32(this object @this)
        {
            if (@this == null)
                return false;

            return uint.TryParse(@this.ToString(), out uint result);
        }
        #endregion

        #region IsValidUInt64
        /// <summary>
        /// An object extension method that query if '@this' is valid ulong.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>true if valid ulong, false if not.</returns>
        public static bool IsValidUInt64(this object @this)
        {
            if (@this == null)
                return false;

            return ulong.TryParse(@this.ToString(), out ulong result);
        }
        #endregion

        #region IsValidULong
        /// <summary>
        /// An object extension method that query if '@this' is valid ulong.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>true if valid ulong, false if not.</returns>
        public static bool IsValidULong(this object @this)
        {
            if (@this == null)
                return false;

            return ulong.TryParse(@this.ToString(), out ulong result);
        }
        #endregion

        #region IsValidUShort
        /// <summary>
        /// An object extension method that query if '@this' is valid ushort.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>true if valid ushort, false if not.</returns>
        public static bool IsValidUShort(this object @this)
        {
            if (@this == null)
                return false;

            return ushort.TryParse(@this.ToString(), out ushort result);
        }
        #endregion
        #endregion

        #region IsAssignableFrom
        /// <summary>
        /// An object extension method that query if '@this' is assignable from.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <returns>true if assignable from, false if not.</returns>
        public static bool IsAssignableFrom<T>(this object @this)
        {
            Type type = @this.GetType();
            return type.IsAssignableFrom(typeof(T));
        }

        /// <summary>
        /// An object extension method that query if '@this' is assignable from.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <returns>true if assignable from, false if not.</returns>
        public static bool IsAssignableFrom(this object @this, Type targetType)
        {
            Type type = @this.GetType();
            return type.IsAssignableFrom(targetType);
        }
        #endregion

        #region Chain
        /// <summary>
        /// A T extension method that chains actions.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="action">The action.</param>
        /// <returns>The @this acted on.</returns>
        public static T Chain<T>(this T @this, Action<T> action)
        {
            action(@this);

            return @this;
        }
        #endregion

        #region DeepClone
        /// <summary>
        ///  Performs deep (full) copy of object and related graph
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <returns>the copied object.</returns>
        public static T DeepClone<T>(this T @this)
        {
            return DeepClonerExtensions.DeepClone(@this);
        }
        #endregion

        #region DeepCloneTo
        /// <summary>
		/// Performs deep (full) copy of object and related graph to existing object
		/// </summary>
		/// <returns>existing filled object</returns>
		/// <remarks>Method is valid only for classes, classes should be descendants in reality, not in declaration</remarks>
		public static TTo DeepCloneTo<TFrom, TTo>(this TFrom @this, TTo to) where TTo : class, TFrom
        {
            return DeepClonerExtensions.DeepCloneTo(@this, to);
        }
        #endregion

        #region ShallowClone
        /// <summary>
        /// Performs shallow (only new object returned, without cloning of dependencies) copy of object
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <returns>A T.</returns>
        public static T ShallowClone<T>(this T @this)
        {
            return DeepClonerExtensions.ShallowClone(@this);
        }
        #endregion

        #region ShallowCloneTo
        /// <summary>
		/// Performs shallow copy of object to existing object
		/// </summary>
		/// <returns>existing filled object</returns>
		/// <remarks>Method is valid only for classes, classes should be descendants in reality, not in declaration</remarks>
		public static TTo ShallowCloneTo<TFrom, TTo>(this TFrom @this, TTo to) where TTo : class, TFrom
        {
            return DeepClonerExtensions.ShallowCloneTo(@this, to);
        }
        #endregion

        #region ChangeType
        /// <summary>
        /// Returns an object of the specified type whose value is equivalent to the specified object.
        /// </summary>
        /// <param name="this">An object that implements the  interface.</param>
        /// <param name="typeCode">The type of object to return.</param>
        /// <returns>
        /// An object whose underlying type is  and whose value is equivalent to .-or-A null reference (Nothing in Visual
        /// Basic), if  is null and  is , , or .
        /// </returns>
        public static object ChangeType(this object @this, TypeCode typeCode)
        {
            return Convert.ChangeType(@this, typeCode);
        }

        /// <summary>
        /// Returns an object of the specified type whose value is equivalent to the specified object. A parameter
        /// supplies culture-specific formatting information.
        /// </summary>
        /// <param name="this">An object that implements the  interface.</param>
        /// <param name="typeCode">The type of object to return.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        /// An object whose underlying type is  and whose value is equivalent to .-or- A null reference (Nothing in
        /// Visual Basic), if  is null and  is , , or .
        /// </returns>
        public static object ChangeType(this object @this, TypeCode typeCode, IFormatProvider provider)
        {
            return Convert.ChangeType(@this, typeCode, provider);
        }

        /// <summary>
        /// Returns an object of the specified type whose value is equivalent to the specified object. A parameter
        /// supplies culture-specific formatting information.
        /// </summary>
        /// <param name="this">An object that implements the  interface.</param>
        /// <param name="conversionType">The type of object to return.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        /// An object whose type is  and whose value is equivalent to .-or- , if the  of  and  are equal.-or- A null
        /// reference (Nothing in Visual Basic), if  is null and  is not a value type.
        /// </returns>
        public static object ChangeType(this object @this, Type conversionType, IFormatProvider provider)
        {
            return Convert.ChangeType(@this, conversionType, provider);
        }

        /// <summary>
        /// Returns an object of the specified type whose value is equivalent to the specified object. A parameter
        /// supplies culture-specific formatting information.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">An object that implements the  interface.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        /// An object whose type is  and whose value is equivalent to .-or- , if the  of  and  are equal.-or- A null
        /// reference (Nothing in Visual Basic), if  is null and  is not a value type.
        /// </returns>
        public static object ChangeType<T>(this object @this, IFormatProvider provider)
        {
            return (T)Convert.ChangeType(@this, typeof(T), provider);
        }

        /// <summary>
        /// 将一个对象转换为指定类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static T ChangeType<T>(this object @this)
        {
            return (T)ChangeType(@this, typeof(T));
        }

        /// <summary>
        /// 将一个对象转换为指定类型
        /// </summary>
        /// <param name="this">待转换的对象</param>
        /// <param name="type">目标类型</param>
        /// <returns>转换后的对象</returns>
        public static object ChangeType(this object @this, Type type)
        {
            if (type == null)
                return @this;

            if (@this == null)
                return type.IsValueType ? Activator.CreateInstance(type) : null;

            var underlyingType = Nullable.GetUnderlyingType(type);

            if (type.IsAssignableFrom(@this.GetType()))
                return @this;

            else if ((underlyingType ?? type).IsEnum)
            {
                if (underlyingType != null && string.IsNullOrEmpty(@this.ToString()))
                    return null;
                else
                    return Enum.Parse(underlyingType ?? type, @this.ToString());
            }
            // 处理DateTime -> DateTimeOffset 类型
            else if (@this.GetType().Equals(typeof(DateTime)) && (underlyingType ?? type).Equals(typeof(DateTimeOffset)))
            {
                return ((DateTime)@this).ConvertToDateTimeOffset();
            }
            // 处理 DateTimeOffset -> DateTime 类型
            else if (@this.GetType().Equals(typeof(DateTimeOffset)) && (underlyingType ?? type).Equals(typeof(DateTime)))
            {
                return ((DateTimeOffset)@this).ConvertToDateTime();
            }
            else if (typeof(IConvertible).IsAssignableFrom(underlyingType ?? type))
            {
                try
                {
                    return Convert.ChangeType(@this, underlyingType ?? type, null);
                }
                catch
                {
                    return underlyingType == null ? Activator.CreateInstance(type) : null;
                }
            }
            else
            {
                var converter = TypeDescriptor.GetConverter(type);

                if (converter.CanConvertFrom(@this.GetType()))
                    return converter.ConvertFrom(@this);

                var constructor = type.GetConstructor(Type.EmptyTypes);
                if (constructor != null)
                {
                    var o = constructor.Invoke(null);
                    var propertys = type.GetProperties();
                    var oldType = @this.GetType();

                    foreach (var property in propertys)
                    {
                        var p = oldType.GetProperty(property.Name);
                        if (property.CanWrite && p != null && p.CanRead)
                        {
                            property.SetValue(o, ChangeType(p.GetValue(@this, null), property.PropertyType), null);
                        }
                    }

                    return o;
                }
            }

            return @this;
        }
        #endregion

        #region GetTypeCode
        /// <summary>
        /// Returns the  for the specified object.
        /// </summary>
        /// <param name="this">An object that implements the  interface.</param>
        /// <returns>The  for , or  if  is null.</returns>
        public static TypeCode GetTypeCode(this object @this)
        {
            return Convert.GetTypeCode(@this);
        }
        #endregion

        #region Coalesce
        /// <summary>
        /// A T extension method that that return the first not null value (including the @this).
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="values">A variable-length parameters list containing values.</param>
        /// <returns>The first not null value.</returns>
        public static T Coalesce<T>(this T @this, params T[] values) where T : class
        {
            if (@this != null)
            {
                return @this;
            }

            foreach (T value in values)
            {
                if (value != null)
                {
                    return value;
                }
            }

            return null;
        }
        #endregion

        #region CoalesceOrDefault
        /// <summary>
        /// A T extension method that that return the first not null value (including the @this) or a default value.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="values">A variable-length parameters list containing values.</param>
        /// <returns>The first not null value or a default value.</returns>
        public static T CoalesceOrDefault<T>(this T @this, params T[] values) where T : class
        {
            if (@this != null)
            {
                return @this;
            }

            foreach (T value in values)
            {
                if (value != null)
                {
                    return value;
                }
            }

            return default(T);
        }

        /// <summary>
        /// A T extension method that that return the first not null value (including the @this) or a default value.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <param name="values">A variable-length parameters list containing values.</param>
        /// <returns>The first not null value or a default value.</returns>
        /// <example>
        ///     <code>
        ///       using Microsoft.VisualStudio.TestTools.UnitTesting;
        /// 
        /// 
        ///       namespace ExtensionMethods.Examples
        ///       {
        ///           [TestClass]
        ///           public class System_Object_CoalesceOrDefault
        ///           {
        ///               [TestMethod]
        ///               public void CoalesceOrDefault()
        ///               {
        ///                   // Varable
        ///                   object nullObject = null;
        /// 
        ///                   // Type
        ///                   object @thisNull = null;
        ///                   object @thisNotNull = &quot;Fizz&quot;;
        /// 
        ///                   // Exemples
        ///                   object result1 = @thisNull.CoalesceOrDefault(nullObject, nullObject, &quot;Buzz&quot;); // return &quot;Buzz&quot;;
        ///                   object result2 = @thisNull.CoalesceOrDefault(() =&gt; &quot;Buzz&quot;, null, null); // return &quot;Buzz&quot;;
        ///                   object result3 = @thisNull.CoalesceOrDefault((x) =&gt; &quot;Buzz&quot;, null, null); // return &quot;Buzz&quot;;
        ///                   object result4 = @thisNotNull.CoalesceOrDefault(nullObject, nullObject, &quot;Buzz&quot;); // return &quot;Fizz&quot;;
        /// 
        ///                   // Unit Test
        ///                   Assert.AreEqual(&quot;Buzz&quot;, result1);
        ///                   Assert.AreEqual(&quot;Buzz&quot;, result2);
        ///                   Assert.AreEqual(&quot;Buzz&quot;, result3);
        ///                   Assert.AreEqual(&quot;Fizz&quot;, result4);
        ///               }
        ///           }
        ///       }
        /// </code>
        /// </example>
        /// <example>
        ///     <code>
        ///       using Microsoft.VisualStudio.TestTools.UnitTesting;
        ///       using Z.ExtensionMethods.Object;
        /// 
        ///       namespace ExtensionMethods.Examples
        ///       {
        ///           [TestClass]
        ///           public class System_Object_CoalesceOrDefault
        ///           {
        ///               [TestMethod]
        ///               public void CoalesceOrDefault()
        ///               {
        ///                   // Varable
        ///                   object nullObject = null;
        /// 
        ///                   // Type
        ///                   object @thisNull = null;
        ///                   object @thisNotNull = &quot;Fizz&quot;;
        /// 
        ///                   // Exemples
        ///                   object result1 = @thisNull.CoalesceOrDefault(nullObject, nullObject, &quot;Buzz&quot;); // return &quot;Buzz&quot;;
        ///                   object result2 = @thisNull.CoalesceOrDefault(() =&gt; &quot;Buzz&quot;, null, null); // return &quot;Buzz&quot;;
        ///                   object result3 = @thisNull.CoalesceOrDefault(x =&gt; &quot;Buzz&quot;, null, null); // return &quot;Buzz&quot;;
        ///                   object result4 = @thisNotNull.CoalesceOrDefault(nullObject, nullObject, &quot;Buzz&quot;); // return &quot;Fizz&quot;;
        /// 
        ///                   // Unit Test
        ///                   Assert.AreEqual(&quot;Buzz&quot;, result1);
        ///                   Assert.AreEqual(&quot;Buzz&quot;, result2);
        ///                   Assert.AreEqual(&quot;Buzz&quot;, result3);
        ///                   Assert.AreEqual(&quot;Fizz&quot;, result4);
        ///               }
        ///           }
        ///       }
        /// </code>
        /// </example>
        /// <example>
        ///     <code>
        ///           using Microsoft.VisualStudio.TestTools.UnitTesting;
        ///           using Z.ExtensionMethods.Object;
        ///           
        ///           namespace ExtensionMethods.Examples
        ///           {
        ///               [TestClass]
        ///               public class System_Object_CoalesceOrDefault
        ///               {
        ///                   [TestMethod]
        ///                   public void CoalesceOrDefault()
        ///                   {
        ///                       // Varable
        ///                       object nullObject = null;
        ///           
        ///                       // Type
        ///                       object @thisNull = null;
        ///                       object @thisNotNull = &quot;Fizz&quot;;
        ///           
        ///                       // Exemples
        ///                       object result1 = @thisNull.CoalesceOrDefault(nullObject, nullObject, &quot;Buzz&quot;); // return &quot;Buzz&quot;;
        ///                       object result2 = @thisNull.CoalesceOrDefault(() =&gt; &quot;Buzz&quot;, null, null); // return &quot;Buzz&quot;;
        ///                       object result3 = @thisNull.CoalesceOrDefault(x =&gt; &quot;Buzz&quot;, null, null); // return &quot;Buzz&quot;;
        ///                       object result4 = @thisNotNull.CoalesceOrDefault(nullObject, nullObject, &quot;Buzz&quot;); // return &quot;Fizz&quot;;
        ///           
        ///                       // Unit Test
        ///                       Assert.AreEqual(&quot;Buzz&quot;, result1);
        ///                       Assert.AreEqual(&quot;Buzz&quot;, result2);
        ///                       Assert.AreEqual(&quot;Buzz&quot;, result3);
        ///                       Assert.AreEqual(&quot;Fizz&quot;, result4);
        ///                   }
        ///               }
        ///           }
        ///     </code>
        /// </example>
        public static T CoalesceOrDefault<T>(this T @this, Func<T> defaultValueFactory, params T[] values) where T : class
        {
            if (@this != null)
            {
                return @this;
            }

            foreach (T value in values)
            {
                if (value != null)
                {
                    return value;
                }
            }

            return defaultValueFactory();
        }

        /// <summary>
        /// A T extension method that that return the first not null value (including the @this) or a default value.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <param name="values">A variable-length parameters list containing values.</param>
        /// <returns>The first not null value or a default value.</returns>
        /// <example>
        ///     <code>
        ///       using Microsoft.VisualStudio.TestTools.UnitTesting;
        /// 
        /// 
        ///       namespace ExtensionMethods.Examples
        ///       {
        ///           [TestClass]
        ///           public class System_Object_CoalesceOrDefault
        ///           {
        ///               [TestMethod]
        ///               public void CoalesceOrDefault()
        ///               {
        ///                   // Varable
        ///                   object nullObject = null;
        /// 
        ///                   // Type
        ///                   object @thisNull = null;
        ///                   object @thisNotNull = &quot;Fizz&quot;;
        /// 
        ///                   // Exemples
        ///                   object result1 = @thisNull.CoalesceOrDefault(nullObject, nullObject, &quot;Buzz&quot;); // return &quot;Buzz&quot;;
        ///                   object result2 = @thisNull.CoalesceOrDefault(() =&gt; &quot;Buzz&quot;, null, null); // return &quot;Buzz&quot;;
        ///                   object result3 = @thisNull.CoalesceOrDefault((x) =&gt; &quot;Buzz&quot;, null, null); // return &quot;Buzz&quot;;
        ///                   object result4 = @thisNotNull.CoalesceOrDefault(nullObject, nullObject, &quot;Buzz&quot;); // return &quot;Fizz&quot;;
        /// 
        ///                   // Unit Test
        ///                   Assert.AreEqual(&quot;Buzz&quot;, result1);
        ///                   Assert.AreEqual(&quot;Buzz&quot;, result2);
        ///                   Assert.AreEqual(&quot;Buzz&quot;, result3);
        ///                   Assert.AreEqual(&quot;Fizz&quot;, result4);
        ///               }
        ///           }
        ///       }
        /// </code>
        /// </example>
        /// <example>
        ///     <code>
        ///       using Microsoft.VisualStudio.TestTools.UnitTesting;
        ///       using Z.ExtensionMethods.Object;
        /// 
        ///       namespace ExtensionMethods.Examples
        ///       {
        ///           [TestClass]
        ///           public class System_Object_CoalesceOrDefault
        ///           {
        ///               [TestMethod]
        ///               public void CoalesceOrDefault()
        ///               {
        ///                   // Varable
        ///                   object nullObject = null;
        /// 
        ///                   // Type
        ///                   object @thisNull = null;
        ///                   object @thisNotNull = &quot;Fizz&quot;;
        /// 
        ///                   // Exemples
        ///                   object result1 = @thisNull.CoalesceOrDefault(nullObject, nullObject, &quot;Buzz&quot;); // return &quot;Buzz&quot;;
        ///                   object result2 = @thisNull.CoalesceOrDefault(() =&gt; &quot;Buzz&quot;, null, null); // return &quot;Buzz&quot;;
        ///                   object result3 = @thisNull.CoalesceOrDefault(x =&gt; &quot;Buzz&quot;, null, null); // return &quot;Buzz&quot;;
        ///                   object result4 = @thisNotNull.CoalesceOrDefault(nullObject, nullObject, &quot;Buzz&quot;); // return &quot;Fizz&quot;;
        /// 
        ///                   // Unit Test
        ///                   Assert.AreEqual(&quot;Buzz&quot;, result1);
        ///                   Assert.AreEqual(&quot;Buzz&quot;, result2);
        ///                   Assert.AreEqual(&quot;Buzz&quot;, result3);
        ///                   Assert.AreEqual(&quot;Fizz&quot;, result4);
        ///               }
        ///           }
        ///       }
        /// </code>
        /// </example>
        /// <example>
        ///     <code>
        ///           using Microsoft.VisualStudio.TestTools.UnitTesting;
        ///           using Z.ExtensionMethods.Object;
        ///           
        ///           namespace ExtensionMethods.Examples
        ///           {
        ///               [TestClass]
        ///               public class System_Object_CoalesceOrDefault
        ///               {
        ///                   [TestMethod]
        ///                   public void CoalesceOrDefault()
        ///                   {
        ///                       // Varable
        ///                       object nullObject = null;
        ///           
        ///                       // Type
        ///                       object @thisNull = null;
        ///                       object @thisNotNull = &quot;Fizz&quot;;
        ///           
        ///                       // Exemples
        ///                       object result1 = @thisNull.CoalesceOrDefault(nullObject, nullObject, &quot;Buzz&quot;); // return &quot;Buzz&quot;;
        ///                       object result2 = @thisNull.CoalesceOrDefault(() =&gt; &quot;Buzz&quot;, null, null); // return &quot;Buzz&quot;;
        ///                       object result3 = @thisNull.CoalesceOrDefault(x =&gt; &quot;Buzz&quot;, null, null); // return &quot;Buzz&quot;;
        ///                       object result4 = @thisNotNull.CoalesceOrDefault(nullObject, nullObject, &quot;Buzz&quot;); // return &quot;Fizz&quot;;
        ///           
        ///                       // Unit Test
        ///                       Assert.AreEqual(&quot;Buzz&quot;, result1);
        ///                       Assert.AreEqual(&quot;Buzz&quot;, result2);
        ///                       Assert.AreEqual(&quot;Buzz&quot;, result3);
        ///                       Assert.AreEqual(&quot;Fizz&quot;, result4);
        ///                   }
        ///               }
        ///           }
        ///     </code>
        /// </example>
        public static T CoalesceOrDefault<T>(this T @this, Func<T, T> defaultValueFactory, params T[] values) where T : class
        {
            if (@this != null)
            {
                return @this;
            }

            foreach (T value in values)
            {
                if (value != null)
                {
                    return value;
                }
            }

            return defaultValueFactory(@this);
        }
        #endregion

        #region GetValueOrDefault
        /// <summary>
        /// A T extension method that gets value or default.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <typeparam name="TResult">Type of the result.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="func">The function.</param>
        /// <returns>The value or default.</returns>
        public static TResult GetValueOrDefault<T, TResult>(this T @this, Func<T, TResult> func)
        {
            try
            {
                return func(@this);
            }
            catch (Exception)
            {
                return default(TResult);
            }
        }

        /// <summary>
        /// A T extension method that gets value or default.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <typeparam name="TResult">Type of the result.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="func">The function.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value or default.</returns>
        public static TResult GetValueOrDefault<T, TResult>(this T @this, Func<T, TResult> func, TResult defaultValue)
        {
            try
            {
                return func(@this);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// A T extension method that gets value or default.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <typeparam name="TResult">Type of the result.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="func">The function.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The value or default.</returns>
        /// <example>
        ///     <code>
        ///       using System.Xml;
        ///       using Microsoft.VisualStudio.TestTools.UnitTesting;
        /// 
        /// 
        ///       namespace ExtensionMethods.Examples
        ///       {
        ///           [TestClass]
        ///           public class System_Object_GetValueOrDefault
        ///           {
        ///               [TestMethod]
        ///               public void GetValueOrDefault()
        ///               {
        ///                   // Type
        ///                   var @this = new XmlDocument();
        /// 
        ///                   // Exemples
        ///                   string result1 = @this.GetValueOrDefault(x =&gt; x.FirstChild.InnerXml, &quot;FizzBuzz&quot;); // return &quot;FizzBuzz&quot;;
        ///                   string result2 = @this.GetValueOrDefault(x =&gt; x.FirstChild.InnerXml, () =&gt; &quot;FizzBuzz&quot;); // return &quot;FizzBuzz&quot;
        /// 
        ///                   // Unit Test
        ///                   Assert.AreEqual(&quot;FizzBuzz&quot;, result1);
        ///                   Assert.AreEqual(&quot;FizzBuzz&quot;, result2);
        ///               }
        ///           }
        ///       }
        /// </code>
        /// </example>
        /// <example>
        ///     <code>
        ///       using System.Xml;
        ///       using Microsoft.VisualStudio.TestTools.UnitTesting;
        ///       using Z.ExtensionMethods.Object;
        /// 
        ///       namespace ExtensionMethods.Examples
        ///       {
        ///           [TestClass]
        ///           public class System_Object_GetValueOrDefault
        ///           {
        ///               [TestMethod]
        ///               public void GetValueOrDefault()
        ///               {
        ///                   // Type
        ///                   var @this = new XmlDocument();
        /// 
        ///                   // Exemples
        ///                   string result1 = @this.GetValueOrDefault(x =&gt; x.FirstChild.InnerXml, &quot;FizzBuzz&quot;); // return &quot;FizzBuzz&quot;;
        ///                   string result2 = @this.GetValueOrDefault(x =&gt; x.FirstChild.InnerXml, () =&gt; &quot;FizzBuzz&quot;); // return &quot;FizzBuzz&quot;
        /// 
        ///                   // Unit Test
        ///                   Assert.AreEqual(&quot;FizzBuzz&quot;, result1);
        ///                   Assert.AreEqual(&quot;FizzBuzz&quot;, result2);
        ///               }
        ///           }
        ///       }
        /// </code>
        /// </example>
        /// <example>
        ///     <code>
        ///           using System.Xml;
        ///           using Microsoft.VisualStudio.TestTools.UnitTesting;
        ///           using Z.ExtensionMethods.Object;
        ///           
        ///           namespace ExtensionMethods.Examples
        ///           {
        ///               [TestClass]
        ///               public class System_Object_GetValueOrDefault
        ///               {
        ///                   [TestMethod]
        ///                   public void GetValueOrDefault()
        ///                   {
        ///                       // Type
        ///                       var @this = new XmlDocument();
        ///           
        ///                       // Exemples
        ///                       string result1 = @this.GetValueOrDefault(x =&gt; x.FirstChild.InnerXml, &quot;FizzBuzz&quot;); // return &quot;FizzBuzz&quot;;
        ///                       string result2 = @this.GetValueOrDefault(x =&gt; x.FirstChild.InnerXml, () =&gt; &quot;FizzBuzz&quot;); // return &quot;FizzBuzz&quot;
        ///           
        ///                       // Unit Test
        ///                       Assert.AreEqual(&quot;FizzBuzz&quot;, result1);
        ///                       Assert.AreEqual(&quot;FizzBuzz&quot;, result2);
        ///                   }
        ///               }
        ///           }
        ///     </code>
        /// </example>
        public static TResult GetValueOrDefault<T, TResult>(this T @this, Func<T, TResult> func, Func<TResult> defaultValueFactory)
        {
            try
            {
                return func(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory();
            }
        }

        /// <summary>
        /// A T extension method that gets value or default.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <typeparam name="TResult">Type of the result.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="func">The function.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The value or default.</returns>
        /// <example>
        ///     <code>
        ///       using System.Xml;
        ///       using Microsoft.VisualStudio.TestTools.UnitTesting;
        /// 
        /// 
        ///       namespace ExtensionMethods.Examples
        ///       {
        ///           [TestClass]
        ///           public class System_Object_GetValueOrDefault
        ///           {
        ///               [TestMethod]
        ///               public void GetValueOrDefault()
        ///               {
        ///                   // Type
        ///                   var @this = new XmlDocument();
        /// 
        ///                   // Exemples
        ///                   string result1 = @this.GetValueOrDefault(x =&gt; x.FirstChild.InnerXml, &quot;FizzBuzz&quot;); // return &quot;FizzBuzz&quot;;
        ///                   string result2 = @this.GetValueOrDefault(x =&gt; x.FirstChild.InnerXml, () =&gt; &quot;FizzBuzz&quot;); // return &quot;FizzBuzz&quot;
        /// 
        ///                   // Unit Test
        ///                   Assert.AreEqual(&quot;FizzBuzz&quot;, result1);
        ///                   Assert.AreEqual(&quot;FizzBuzz&quot;, result2);
        ///               }
        ///           }
        ///       }
        /// </code>
        /// </example>
        /// <example>
        ///     <code>
        ///       using System.Xml;
        ///       using Microsoft.VisualStudio.TestTools.UnitTesting;
        ///       using Z.ExtensionMethods.Object;
        /// 
        ///       namespace ExtensionMethods.Examples
        ///       {
        ///           [TestClass]
        ///           public class System_Object_GetValueOrDefault
        ///           {
        ///               [TestMethod]
        ///               public void GetValueOrDefault()
        ///               {
        ///                   // Type
        ///                   var @this = new XmlDocument();
        /// 
        ///                   // Exemples
        ///                   string result1 = @this.GetValueOrDefault(x =&gt; x.FirstChild.InnerXml, &quot;FizzBuzz&quot;); // return &quot;FizzBuzz&quot;;
        ///                   string result2 = @this.GetValueOrDefault(x =&gt; x.FirstChild.InnerXml, () =&gt; &quot;FizzBuzz&quot;); // return &quot;FizzBuzz&quot;
        /// 
        ///                   // Unit Test
        ///                   Assert.AreEqual(&quot;FizzBuzz&quot;, result1);
        ///                   Assert.AreEqual(&quot;FizzBuzz&quot;, result2);
        ///               }
        ///           }
        ///       }
        /// </code>
        /// </example>
        /// <example>
        ///     <code>
        ///           using System.Xml;
        ///           using Microsoft.VisualStudio.TestTools.UnitTesting;
        ///           using Z.ExtensionMethods.Object;
        ///           
        ///           namespace ExtensionMethods.Examples
        ///           {
        ///               [TestClass]
        ///               public class System_Object_GetValueOrDefault
        ///               {
        ///                   [TestMethod]
        ///                   public void GetValueOrDefault()
        ///                   {
        ///                       // Type
        ///                       var @this = new XmlDocument();
        ///           
        ///                       // Exemples
        ///                       string result1 = @this.GetValueOrDefault(x =&gt; x.FirstChild.InnerXml, &quot;FizzBuzz&quot;); // return &quot;FizzBuzz&quot;;
        ///                       string result2 = @this.GetValueOrDefault(x =&gt; x.FirstChild.InnerXml, () =&gt; &quot;FizzBuzz&quot;); // return &quot;FizzBuzz&quot;
        ///           
        ///                       // Unit Test
        ///                       Assert.AreEqual(&quot;FizzBuzz&quot;, result1);
        ///                       Assert.AreEqual(&quot;FizzBuzz&quot;, result2);
        ///                   }
        ///               }
        ///           }
        ///     </code>
        /// </example>
        public static TResult GetValueOrDefault<T, TResult>(this T @this, Func<T, TResult> func, Func<T, TResult> defaultValueFactory)
        {
            try
            {
                return func(@this);
            }
            catch (Exception)
            {
                return defaultValueFactory(@this);
            }
        }
        #endregion

        #region IfNotNull
        /// <summary>
        /// A T extension method that execute an action when the value is not null.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="action">The action.</param>
        public static void IfNotNull<T>(this T @this, Action<T> action) where T : class
        {
            if (@this != null)
            {
                action(@this);
            }
        }

        /// <summary>
        /// A T extension method that the function result if not null otherwise default value.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <typeparam name="TResult">Type of the result.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="func">The function.</param>
        /// <returns>The function result if @this is not null otherwise default value.</returns>
        public static TResult IfNotNull<T, TResult>(this T @this, Func<T, TResult> func) where T : class
        {
            return @this != null ? func(@this) : default(TResult);
        }

        /// <summary>
        /// A T extension method that the function result if not null otherwise default value.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <typeparam name="TResult">Type of the result.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="func">The function.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The function result if @this is not null otherwise default value.</returns>
        public static TResult IfNotNull<T, TResult>(this T @this, Func<T, TResult> func, TResult defaultValue) where T : class
        {
            return @this != null ? func(@this) : defaultValue;
        }

        /// <summary>
        /// A T extension method that the function result if not null otherwise default value.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <typeparam name="TResult">Type of the result.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="func">The function.</param>
        /// <param name="defaultValueFactory">The default value factory.</param>
        /// <returns>The function result if @this is not null otherwise default value.</returns>
        public static TResult IfNotNull<T, TResult>(this T @this, Func<T, TResult> func, Func<TResult> defaultValueFactory) where T : class
        {
            return @this != null ? func(@this) : defaultValueFactory();
        }
        #endregion

        #region NullIf
        /// <summary>
        /// A T extension method that null if.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns>A T.</returns>
        public static T NullIf<T>(this T @this, Func<T, bool> predicate) where T : class
        {
            if (predicate(@this))
            {
                return null;
            }
            return @this;
        }
        #endregion

        #region NullIfEquals
        /// <summary>
        /// A T extension method that null if equals.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="value">The value.</param>
        /// <returns>A T.</returns>
        public static T NullIfEquals<T>(this T @this, T value) where T : class
        {
            if (@this.Equals(value))
            {
                return null;
            }
            return @this;
        }
        #endregion

        #region NullIfEqualsAny
        /// <summary>
        /// A T extension method that null if equals any.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="values">A variable-length parameters list containing values.</param>
        /// <returns>A T.</returns>
        public static T NullIfEqualsAny<T>(this T @this, params T[] values) where T : class
        {
            if (Array.IndexOf(values, @this) != -1)
            {
                return null;
            }
            return @this;
        }
        #endregion

        #region ToStringSafe
        /// <summary>
        /// An object extension method that converts the @this to string or return an empty string if the value is null.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as a string or empty if the value is null.</returns>
        public static string ToStringSafe(this object @this)
        {
            return @this == null ? "" : @this.ToString();
        }
        #endregion

        #region Try
        /// <summary>
        /// A TType extension method that tries.
        /// </summary>
        /// <typeparam name="TType">Type of the type.</typeparam>
        /// <typeparam name="TResult">Type of the result.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="tryFunction">The try function.</param>
        /// <returns>A TResult.</returns>
        public static TResult Try<TType, TResult>(this TType @this, Func<TType, TResult> tryFunction)
        {
            try
            {
                return tryFunction(@this);
            }
            catch
            {
                return default(TResult);
            }
        }

        /// <summary>
        /// A TType extension method that tries.
        /// </summary>
        /// <typeparam name="TType">Type of the type.</typeparam>
        /// <typeparam name="TResult">Type of the result.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="tryFunction">The try function.</param>
        /// <param name="catchValue">The catch value.</param>
        /// <returns>A TResult.</returns>
        public static TResult Try<TType, TResult>(this TType @this, Func<TType, TResult> tryFunction, TResult catchValue)
        {
            try
            {
                return tryFunction(@this);
            }
            catch
            {
                return catchValue;
            }
        }

        /// <summary>
        /// A TType extension method that tries.
        /// </summary>
        /// <typeparam name="TType">Type of the type.</typeparam>
        /// <typeparam name="TResult">Type of the result.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="tryFunction">The try function.</param>
        /// <param name="catchValueFactory">The catch value factory.</param>
        /// <returns>A TResult.</returns>
        public static TResult Try<TType, TResult>(this TType @this, Func<TType, TResult> tryFunction, Func<TType, TResult> catchValueFactory)
        {
            try
            {
                return tryFunction(@this);
            }
            catch
            {
                return catchValueFactory(@this);
            }
        }

        /// <summary>
        /// A TType extension method that tries.
        /// </summary>
        /// <typeparam name="TType">Type of the type.</typeparam>
        /// <typeparam name="TResult">Type of the result.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="tryFunction">The try function.</param>
        /// <param name="result">[out] The result.</param>
        /// <returns>A TResult.</returns>
        public static bool Try<TType, TResult>(this TType @this, Func<TType, TResult> tryFunction, out TResult result)
        {
            try
            {
                result = tryFunction(@this);
                return true;
            }
            catch
            {
                result = default(TResult);
                return false;
            }
        }

        /// <summary>
        /// A TType extension method that tries.
        /// </summary>
        /// <typeparam name="TType">Type of the type.</typeparam>
        /// <typeparam name="TResult">Type of the result.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="tryFunction">The try function.</param>
        /// <param name="catchValue">The catch value.</param>
        /// <param name="result">[out] The result.</param>
        /// <returns>A TResult.</returns>
        public static bool Try<TType, TResult>(this TType @this, Func<TType, TResult> tryFunction, TResult catchValue, out TResult result)
        {
            try
            {
                result = tryFunction(@this);
                return true;
            }
            catch
            {
                result = catchValue;
                return false;
            }
        }

        /// <summary>
        /// A TType extension method that tries.
        /// </summary>
        /// <typeparam name="TType">Type of the type.</typeparam>
        /// <typeparam name="TResult">Type of the result.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="tryFunction">The try function.</param>
        /// <param name="catchValueFactory">The catch value factory.</param>
        /// <param name="result">[out] The result.</param>
        /// <returns>A TResult.</returns>
        public static bool Try<TType, TResult>(this TType @this, Func<TType, TResult> tryFunction, Func<TType, TResult> catchValueFactory, out TResult result)
        {
            try
            {
                result = tryFunction(@this);
                return true;
            }
            catch
            {
                result = catchValueFactory(@this);
                return false;
            }
        }

        /// <summary>
        /// A TType extension method that attempts to action from the given data.
        /// </summary>
        /// <typeparam name="TType">Type of the type.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="tryAction">The try action.</param>
        /// <returns>true if it succeeds, false if it fails.</returns>
        public static bool Try<TType>(this TType @this, Action<TType> tryAction)
        {
            try
            {
                tryAction(@this);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// A TType extension method that attempts to action from the given data.
        /// </summary>
        /// <typeparam name="TType">Type of the type.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="tryAction">The try action.</param>
        /// <param name="catchAction">The catch action.</param>
        /// <returns>true if it succeeds, false if it fails.</returns>
        public static bool Try<TType>(this TType @this, Action<TType> tryAction, Action<TType> catchAction)
        {
            try
            {
                tryAction(@this);
                return true;
            }
            catch
            {
                catchAction(@this);
                return false;
            }
        }
        #endregion

        #region Between
        /// <summary>
        /// A T extension method that check if the value is between (exclusif) the minValue and maxValue.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <returns>true if the value is between the minValue and maxValue, otherwise false.</returns>
        public static bool Between<T>(this T @this, T minValue, T maxValue) where T : IComparable<T>
        {
            return minValue.CompareTo(@this) == -1 && @this.CompareTo(maxValue) == -1;
        }
        #endregion

        #region In
        /// <summary>
        /// A T extension method to determines whether the object is equal to any of the provided values.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The object to be compared.</param>
        /// <param name="values">The value list to compare with the object.</param>
        /// <returns>true if the values list contains the object, else false.</returns>
        public static bool In<T>(this T @this, params T[] values)
        {
            return Array.IndexOf(values, @this) != -1;
        }
        #endregion

        #region InRange
        /// <summary>
        /// A T extension method that check if the value is between inclusively the minValue and maxValue.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <returns>true if the value is between inclusively the minValue and maxValue, otherwise false.</returns>
        public static bool InRange<T>(this T @this, T minValue, T maxValue) where T : IComparable<T>
        {
            return @this.CompareTo(minValue) >= 0 && @this.CompareTo(maxValue) <= 0;
        }
        #endregion

        #region IsDBNull
        /// <summary>
        /// Returns an indication whether the specified object is of type .
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">An object.</param>
        /// <returns>true if  is of type ; otherwise, false.</returns>
        public static bool IsDBNull<T>(this T @this) where T : class
        {
            return Convert.IsDBNull(@this);
        }
        #endregion

        #region IsDefault
        /// <summary>
        /// A T extension method that query if 'source' is the default value.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The source to act on.</param>
        /// <returns>true if default, false if not.</returns>
        public static bool IsDefault<T>(this T @this)
        {
            return @this.Equals(default(T));
        }
        #endregion

        #region IsNotNull
        /// <summary>
        /// A T extension method that query if '@this' is not null.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <returns>true if not null, false if not.</returns>
        public static bool IsNotNull<T>(this T @this) where T : class
        {
            return @this != null;
        }
        #endregion

        #region IsNull
        /// <summary>
        /// 是否为空
        /// </summary>
        /// <param name="this">object对象</param>
        /// <returns>bool</returns>
        public static bool IsNull(this object @this)
        {
            return @this == null || @this == DBNull.Value;
        }

        /// <summary>
        /// A T extension method that query if '@this' is null.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <returns>true if null, false if not.</returns>
        public static bool IsNull<T>(this T @this) where T : class, new()
        {
            return @this == null;
        }
        #endregion

        #region NotIn
        /// <summary>
        /// A T extension method to determines whether the object is not equal to any of the provided values.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The object to be compared.</param>
        /// <param name="values">The value list to compare with the object.</param>
        /// <returns>true if the values list doesn't contains the object, else false.</returns>
        public static bool NotIn<T>(this T @this, params T[] values)
        {
            return Array.IndexOf(values, @this) == -1;
        }
        #endregion

        #region ReferenceEquals
        /// <summary>
        /// Determines whether the specified  instances are the same instance.
        /// </summary>
        /// <param name="this">The first object to compare.</param>
        /// <param name="obj">The second object  to compare.</param>
        /// <returns>true if  is the same instance as  or if both are null; otherwise, false.</returns>
        public new static bool ReferenceEquals(this object @this, object obj)
        {
            return object.ReferenceEquals(@this, obj);
        }
        #endregion

        #region ToDictionary
        /// <summary>
        /// 目标匿名参数对象转为字典
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static IDictionary<string, object> ToDictionary(this object @this)
        {
            if (@this is IDictionary<string, object> dic)
                return dic;

            dic = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            if (@this.IsNotNull())
            {
                // 修正字符串字典的支持问题
                if (@this is IDictionary dic2)
                {
                    foreach (DictionaryEntry item in dic2)
                    {
                        dic[item.Key + ""] = item.Value;
                    }
                }
                else
                {
                    foreach (var pi in @this.GetType().GetProperties())
                    {
                        if (pi.GetIndexParameters().Length > 0)
                            continue;

                        if (pi.GetCustomAttribute<XmlIgnoreAttribute>().IsNotNull())
                            continue;

                        dic[pi.Name] = @this.GetValue(pi);
                    }
                }
            }
            return dic;
        }

        /// <summary>
        /// 目标匿名参数对象转为字典
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static IDictionary<TKey, TValue> ToDictionary<TKey, TValue>(this object @this)
        {
            if (@this is IDictionary<TKey, TValue> dic)
                return dic;

            dic = new Dictionary<TKey, TValue>();
            if (@this.IsNotNull())
            {
                // 修正字符串字典的支持问题
                if (@this is IDictionary dic2)
                {
                    foreach (DictionaryEntry item in dic2)
                    {
                        dic[item.Key.To<TKey>()] = item.Value.To<TValue>();
                    }
                }
                else
                {
                    foreach (var pi in @this.GetType().GetProperties())
                    {
                        if (pi.GetIndexParameters().Length > 0)
                            continue;

                        if (pi.GetCustomAttribute<XmlIgnoreAttribute>().IsNotNull())
                            continue;

                        dic[pi.Name.To<TKey>()] = pi.GetValue(@this).To<TValue>();
                    }
                }
            }
            return dic;
        }
        #endregion

        #region IsValid
        /// <summary>
        /// 验证对象是否有效
        /// </summary>
        /// <param name="this">要验证的对象</param>
        /// <returns></returns>
        public static bool IsValid(this object @this)
        {
            var isValid = true;
            if (@this is IEnumerable list)
            {
                foreach (var item in list)
                {
                    if (!item.IsValid())
                    {
                        isValid = false;
                        break;
                    }
                }
            }
            else
            {
                isValid = Validator.TryValidateObject(@this, new ValidationContext(@this, null, null), new Collection<ValidationResult>(), true);
            }
            return isValid;
        }

        /// <summary>
        /// 验证对象是否有效
        /// </summary>
        /// <param name="this">要验证的对象</param>
        /// <returns></returns>
        public static (bool isValid, Collection<ValidationResult> validationResults) IsValidWithResult(this object @this)
        {
            var isValid = true;
            var validationResults = new Collection<ValidationResult>();
            if (@this is IEnumerable list)
            {
                foreach (var item in list)
                {
                    var (r, results) = item.IsValidWithResult();
                    if (!r && results?.Count > 0)
                    {
                        isValid = false;
                        results.ToList().ForEach(o => validationResults.Add(o));
                        break;
                    }
                }
            }
            else
            {
                isValid = Validator.TryValidateObject(@this, new ValidationContext(@this, null, null), validationResults, true);
            }
            return (isValid, validationResults);
        }
        #endregion

        #region GetValue
        /// <summary>
        /// 获取目标对象的成员值
        /// </summary>
        /// <param name="target">目标对象</param>
        /// <param name="member">成员</param>
        /// <returns></returns>
        public static object GetValue(this object target, MemberInfo member)
        {
            if (member == null)
            {
                member = target as MemberInfo;
                target = null;
            }

            if (member is PropertyInfo)
                return (member as PropertyInfo)?.GetValue(target, null);
            else if (member is FieldInfo)
                return (member as FieldInfo)?.GetValue(target);
            else
                throw new ArgumentOutOfRangeException(nameof(member));
        }
        #endregion
    }
}
