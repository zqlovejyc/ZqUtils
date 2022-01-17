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
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ZqUtils.Extensions;
/****************************
* [Author] 张强
* [Date] 2018-05-22
* [Describe] Type帮助工具类
* **************************/
namespace ZqUtils.Helpers
{
    /// <summary>
    /// Type帮助工具类
    /// </summary>
    public static class TypeHelper
    {
        #region CreateInstance
        /// <summary>
        /// 使用区分大小写的搜索，从此程序集中查找指定的类型，然后使用系统激活器创建它的实例。
        /// </summary>
        /// <param name="assembly">程序集</param>
        /// <param name="typeName">要查找类型的 System.Type.FullName。</param>
        /// <returns>返回创建的实例</returns>
        public static object CreateInstance(Assembly assembly, string typeName)
        {
            return assembly.CreateInstance(typeName);
        }

        /// <summary>
        /// 使用可选的区分大小写搜索，从此程序集中查找指定的类型，然后使用系统激活器创建它的实例。
        /// </summary>
        /// <param name="assembly">程序集</param>
        /// <param name="typeName">要查找类型的 System.Type.FullName。</param>
        /// <param name="ignoreCase">如果为 true，则忽略类型名的大小写；否则，为 false。</param>
        /// <returns>返回创建的实例</returns>
        public static object CreateInstance(Assembly assembly, string typeName, bool ignoreCase)
        {
            return assembly.CreateInstance(typeName, ignoreCase);
        }

        /// <summary>
        ///  使用命名的程序集文件和默认构造函数，创建名称已指定的类型的实例。
        /// </summary>
        /// <param name="assemblyFile">包含某程序集的文件的名称，将在该程序集内查找名为 typeName 的类型。</param>
        /// <param name="typeName">首选类型的名称。</param>
        /// <returns>创建的实例</returns>
        public static object CreateInstance(string assemblyFile, string typeName)
        {
            return Activator.CreateInstanceFrom(assemblyFile, typeName).Unwrap();
        }

        /// <summary>
        /// 使用命名的程序集文件和默认构造函数，创建名称已指定的类型的实例。
        /// </summary>
        /// <param name="assemblyFile">包含某程序集的文件的名称，将在该程序集内查找名为 typeName 的类型。</param>
        /// <param name="typeName">首选类型的名称。</param>
        /// <param name="activationAttributes">
        /// 包含一个或多个可以参与激活的特性的数组。 这通常为包含单个 System.Runtime.Remoting.Activation.UrlAttribute
        /// 对象的数组，该对象指定激活远程对象所需的 URL。 此参数与客户端激活的对象相关。 客户端激活是一项传统技术，保留用于向后兼容，但不建议用于新的开发。 应改用
        /// Windows Communication Foundation 来开发分布式应用程序。
        /// </param>
        /// <returns>创建的实例</returns>
        public static object CreateInstance(string assemblyFile, string typeName, object[] activationAttributes)
        {
            return Activator.CreateInstanceFrom(assemblyFile, typeName, activationAttributes).Unwrap();
        }

        /// <summary>
        ///  使用命名的程序集文件和默认构造函数，来创建其名称在指定的远程域中指定的类型的实例。
        /// </summary>
        /// <param name="domain">在其中创建名为 typeName 的类型的远程域。</param>
        /// <param name="assemblyFile">包含某程序集的文件的名称，将在该程序集内查找名为 typeName 的类型。</param>
        /// <param name="typeName">首选类型的名称。</param>
        /// <returns>创建的实例</returns>
        public static object CreateInstance(AppDomain domain, string assemblyFile, string typeName)
        {
            return Activator.CreateInstanceFrom(domain, assemblyFile, typeName).Unwrap();
        }
        #endregion

        #region GetElementType
        /// <summary>
        /// GetElementType
        /// </summary>
        /// <param name="enumerableType"></param>
        /// <returns></returns>
        public static Type GetElementType(Type enumerableType)
        {
            return GetElementTypes(enumerableType, null)[0];
        }

        /// <summary>
        /// GetElementTypes
        /// </summary>
        /// <param name="enumerableType"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static Type[] GetElementTypes(Type enumerableType, ElementTypeFlags flags = ElementTypeFlags.None)
        {
            return GetElementTypes(enumerableType, null, flags);
        }

        /// <summary>
        /// GetElementType
        /// </summary>
        /// <param name="enumerableType"></param>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        public static Type GetElementType(Type enumerableType, IEnumerable enumerable)
        {
            return GetElementTypes(enumerableType, enumerable)[0];
        }
        #endregion

        #region GetElementTypes
        /// <summary>
        /// GetElementTypes
        /// </summary>
        /// <param name="enumerableType"></param>
        /// <param name="enumerable"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static Type[] GetElementTypes(Type enumerableType, IEnumerable enumerable, ElementTypeFlags flags = ElementTypeFlags.None)
        {
            if (enumerableType.HasElementType)
            {
                return new[] { enumerableType.GetElementType() };
            }
            var idictionaryType = enumerableType.GetDictionaryType();
            if (idictionaryType != null && flags.HasFlag(ElementTypeFlags.BreakKeyValuePair))
            {
                return idictionaryType.GetTypeInfo().GenericTypeArguments;
            }
            var ienumerableType = enumerableType.GetIEnumerableType();
            if (ienumerableType != null)
            {
                return ienumerableType.GetTypeInfo().GenericTypeArguments;
            }
            if (typeof(IEnumerable).IsAssignableFrom(enumerableType))
            {
                var first = enumerable?.Cast<object>().FirstOrDefault();

                return new[] { first?.GetType() ?? typeof(object) };
            }
            throw new ArgumentException($"Unable to find the element type for type '{enumerableType}'.", nameof(enumerableType));
        }
        #endregion

        #region GetEnumerationType
        /// <summary>
        /// GetEnumerationType
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static Type GetEnumerationType(Type enumType)
        {
            if (enumType.IsNullableType())
            {
                enumType = enumType.GetTypeInfo().GenericTypeArguments[0];
            }
            if (!enumType.IsEnum())
            {
                return null;
            }
            return enumType;
        }
        #endregion

        #region GetClassAndInheritInterfaces
        /// <summary>  
        /// 获取程序集中的实现类对应的多个接口
        /// </summary>  
        /// <param name="assemblyName">程序集</param>
        public static Dictionary<Type, Type[]> GetClassAndInheritInterfaces(string assemblyName)
        {
            var result = new Dictionary<Type, Type[]>();
            if (!string.IsNullOrEmpty(assemblyName))
            {
                var assembly = Assembly.Load(assemblyName);
                var ts = assembly.GetTypes().ToList();
                foreach (var item in ts.Where(s => !s.IsInterface))
                {
                    var interfaces = item.GetInterfaces();
                    if (item.IsGenericType) continue;
                    if (interfaces?.Length > 0) result.Add(item, interfaces);
                }
            }
            return result;
        }

        /// <summary>  
        /// 获取程序集中的实现类对应的多个接口
        /// </summary>  
        /// <param name="assemblyNames">程序集数组</param>
        public static Dictionary<Type, Type[]> GetClassAndInheritInterfaces(params string[] assemblyNames)
        {
            var result = new Dictionary<Type, Type[]>();
            if (assemblyNames?.Length > 0)
            {
                foreach (var assemblyName in assemblyNames)
                {
                    result = result.Union(GetClassAndInheritInterfaces(assemblyName)).ToDictionary(o => o.Key, o => o.Value);
                }
            }
            return result;
        }
        #endregion
    }

    /// <summary>
    /// 元素类型标识
    /// </summary>
    public enum ElementTypeFlags
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,

        /// <summary>
        /// BreakKeyValuePair
        /// </summary>
        BreakKeyValuePair = 1
    }
}
