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

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
/****************************
* [Author] 张强
* [Date] 2018-05-16
* [Describe] Type扩展类
* **************************/
namespace ZqUtils.Extensions
{
    /// <summary>
    /// Type扩展类
    /// </summary>
    public static partial class Extensions
    {
        #region GetCoreType
        /// <summary>
        /// 如果type是Nullable类型则返回UnderlyingType，否则则直接返回type本身
        /// </summary>
        /// <param name="this">类型</param>
        /// <returns>Type</returns>
        public static Type GetCoreType(this Type @this)
        {
            if (@this?.IsNullable() == true)
            {
                @this = Nullable.GetUnderlyingType(@this);
            }
            return @this;
        }
        #endregion        

        #region GetGenericTypeDefinitionIfGeneric
        /// <summary>
        /// GetGenericTypeDefinitionIfGeneric
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static Type GetGenericTypeDefinitionIfGeneric(this Type @this)
        {
            return @this.IsGenericType() ? @this.GetGenericTypeDefinition() : @this;
        }
        #endregion

        #region GetGenericArguments
        /// <summary>
        /// GetGenericArguments
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static Type[] GetGenericArguments(this Type @this)
        {
            return @this.GetTypeInfo().GenericTypeArguments;
        }
        #endregion

        #region GetGenericParameters
        /// <summary>
        /// GetGenericParameters
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static Type[] GetGenericParameters(this Type @this)
        {
            return @this.GetGenericTypeDefinition().GetTypeInfo().GenericTypeParameters;
        }
        #endregion

        #region GetDeclaredConstructors
        /// <summary>
        /// GetDeclaredConstructors
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static IEnumerable<ConstructorInfo> GetDeclaredConstructors(this Type @this)
        {
            return @this.GetTypeInfo().DeclaredConstructors;
        }
        #endregion

        #region GetDeclaredMembers
        /// <summary>
        /// GetDeclaredMembers
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static IEnumerable<MemberInfo> GetDeclaredMembers(this Type @this)
        {
            return @this.GetTypeInfo().DeclaredMembers;
        }
        #endregion

        #region GetAllMembers
        /// <summary>
        /// GetAllMembers
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static IEnumerable<MemberInfo> GetAllMembers(this Type @this)
        {
            while (true)
            {
                foreach (var memberInfo in @this.GetTypeInfo().DeclaredMembers)
                {
                    yield return memberInfo;
                }
                @this = @this.BaseType();
                if (@this == null)
                {
                    yield break;
                }
            }
        }
        #endregion

        #region GetMember
        /// <summary>
        /// GetMember
        /// </summary>
        /// <param name="this"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static MemberInfo[] GetMember(this Type @this, string name)
        {
            return @this.GetAllMembers().Where(mi => mi.Name == name).ToArray();
        }
        #endregion

        #region GetDeclaredMethods
        /// <summary>
        /// GetDeclaredMethods
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static IEnumerable<MethodInfo> GetDeclaredMethods(this Type @this)
        {
            return @this.GetTypeInfo().DeclaredMethods;
        }
        #endregion

        #region GetDeclaredMethod
        /// <summary>
        /// GetDeclaredMethod
        /// </summary>
        /// <param name="this"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static MethodInfo GetDeclaredMethod(this Type @this, string name)
        {
            return @this.GetAllMethods().FirstOrDefault(mi => mi.Name == name);
        }
        #endregion

        #region GetDeclaredMethod
        /// <summary>
        /// GetDeclaredMethod
        /// </summary>
        /// <param name="this"></param>
        /// <param name="name"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static MethodInfo GetDeclaredMethod(this Type @this, string name, Type[] parameters)
        {
            return @this.GetAllMethods()
                      .Where(mi => mi.Name == name)
                      .Where(mi => mi.GetParameters().Length == parameters.Length)
                      .FirstOrDefault(mi => mi.GetParameters().Select(pi => pi.ParameterType).SequenceEqual(parameters));
        }
        #endregion

        #region GetDeclaredConstructor
        /// <summary>
        /// GetDeclaredConstructor
        /// </summary>
        /// <param name="this"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static ConstructorInfo GetDeclaredConstructor(this Type @this, Type[] parameters)
        {
            return @this.GetTypeInfo()
                      .DeclaredConstructors
                      .Where(mi => mi.GetParameters().Length == parameters.Length)
                      .FirstOrDefault(mi => mi.GetParameters().Select(pi => pi.ParameterType).SequenceEqual(parameters));
        }
        #endregion

        #region GetAllMethods
        /// <summary>
        /// GetAllMethods
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static IEnumerable<MethodInfo> GetAllMethods(this Type @this)
        {
            return @this.GetRuntimeMethods();
        }
        #endregion

        #region GetDeclaredProperties
        /// <summary>
        /// GetDeclaredProperties
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> GetDeclaredProperties(this Type @this)
        {
            return @this.GetTypeInfo().DeclaredProperties;
        }
        #endregion

        #region GetDeclaredProperty
        /// <summary>
        /// GetDeclaredProperty
        /// </summary>
        /// <param name="this"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static PropertyInfo GetDeclaredProperty(this Type @this, string name)
        {
            return @this.GetTypeInfo().GetDeclaredProperty(name);
        }
        #endregion

        #region GetCustomAttributes
        /// <summary>
        /// GetCustomAttributes
        /// </summary>
        /// <param name="this"></param>
        /// <param name="attributeType"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static object[] GetCustomAttributes(this Type @this, Type attributeType, bool inherit)
        {
            return @this.GetTypeInfo().GetCustomAttributes(attributeType, inherit).ToArray();
        }
        #endregion

        #region GetAttribute
        /// <summary>
        /// GetAttribute
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T GetAttribute<T>(this object obj) where T : class
        {
            Type type = obj.GetType();
            return type.GetAttribute<T>();
        }

        /// <summary>
        /// GetAttribute
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static T GetAttribute<T>(this Type type) where T : class
        {
            Attribute customAttribute = type.GetCustomAttribute(typeof(T));
            if (customAttribute.IsNotNull())
            {
                return customAttribute as T;
            }
            return null;
        }
        #endregion

        #region GetConstructors
        /// <summary>
        /// GetConstructors
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static ConstructorInfo[] GetConstructors(this Type @this)
        {
            return @this.GetTypeInfo().DeclaredConstructors.ToArray();
        }
        #endregion

        #region GetProperties
        /// <summary>
        /// GetProperties
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static PropertyInfo[] GetProperties(this Type @this)
        {
            return @this.GetRuntimeProperties().ToArray();
        }
        #endregion

        #region GetPropertyInfo
        /// <summary>
        /// 获取实体类键值（缓存）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static Hashtable GetPropertyInfo<T>(this T @this)
        {
            var type = @this.GetType();
            object CacheEntity = null;
            if (CacheEntity == null)
            {
                var ht = new Hashtable();
                var props = type.GetProperties();
                foreach (var prop in props)
                {
                    var name = prop.Name;
                    var value = prop.GetValue(@this, null);
                    ht[name] = value;
                }
                return ht;
            }
            else
            {
                return (Hashtable)CacheEntity;
            }
        }
        #endregion

        #region GetGetMethod
        /// <summary>
        /// GetGetMethod
        /// </summary>
        /// <param name="this"></param>
        /// <param name="ignored"></param>
        /// <returns></returns>
        public static MethodInfo GetGetMethod(this PropertyInfo @this, bool ignored)
        {
            return @this.GetMethod;
        }
        #endregion

        #region GetSetMethod
        /// <summary>
        /// GetSetMethod
        /// </summary>
        /// <param name="this"></param>
        /// <param name="ignored"></param>
        /// <returns></returns>
        public static MethodInfo GetSetMethod(this PropertyInfo @this, bool ignored)
        {
            return @this.SetMethod;
        }
        #endregion

        #region GetField
        /// <summary>
        /// GetField
        /// </summary>
        /// <param name="this"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static FieldInfo GetField(this Type @this, string name)
        {
            return @this.GetRuntimeField(name);
        }
        #endregion

        #region GetInheritedMethod
        /// <summary>
        /// GetInheritedMethod
        /// </summary>
        /// <param name="this"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static MethodInfo GetInheritedMethod(this Type @this, string name)
        {
            return GetMember(@this, name).FirstOrDefault() as MethodInfo;
        }
        #endregion

        #region GetFieldOrProperty
        /// <summary>
        /// GetFieldOrProperty
        /// </summary>
        /// <param name="this"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static MemberInfo GetFieldOrProperty(this Type @this, string name)
        {
            var memberInfo = GetMember(@this, name).FirstOrDefault();
            if (memberInfo == null)
            {
                throw new ArgumentOutOfRangeException(nameof(name), "Cannot find a field or property named " + name);
            }
            return memberInfo;
        }
        #endregion

        #region GetTypeOfNullable
        /// <summary>
        /// GetTypeOfNullable
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static Type GetTypeOfNullable(this Type @this)
        {
            return @this.GetTypeInfo().GenericTypeArguments[0];
        }
        #endregion

        #region GetIEnumerableType
        /// <summary>
        /// GetIEnumerableType
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static Type GetIEnumerableType(this Type @this)
        {
            return @this.GetGenericInterface(typeof(IEnumerable<>));
        }
        #endregion

        #region GetDictionaryType
        /// <summary>
        /// GetDictionaryType
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static Type GetDictionaryType(this Type @this)
        {
            return @this.GetGenericInterface(typeof(IDictionary<,>));
        }
        #endregion

        #region GetGenericInterface
        /// <summary>
        /// GetGenericInterface
        /// </summary>
        /// <param name="this"></param>
        /// <param name="genericInterface"></param>
        /// <returns></returns>
        public static Type GetGenericInterface(this Type @this, Type genericInterface)
        {
            if (@this.IsGenericType(genericInterface))
            {
                return @this;
            }
            return @this.GetTypeInfo().ImplementedInterfaces.FirstOrDefault(t => t.IsGenericType(genericInterface));
        }
        #endregion

        #region GetGenericElementType
        /// <summary>
        /// GetGenericElementType
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static Type GetGenericElementType(this Type @this)
        {
            if (@this.HasElementType)
                return @this.GetElementType();
            return @this.GetTypeInfo().GenericTypeArguments[0];
        }
        #endregion

        #region GetStaticMethods
        /// <summary>
        /// GetStaticMethods
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static IEnumerable<MethodInfo> GetStaticMethods(this Type @this)
        {
            return @this.GetRuntimeMethods().Where(m => m.IsStatic);
        }
        #endregion

        #region IsNullable
        /// <summary>
        /// 判断类型是否是Nullable类型
        /// </summary>
        /// <param name="this">类型</param>
        /// <returns>bool</returns>
        public static bool IsNullable(this Type @this)
        {
            return @this.IsValueType && @this.IsGenericType && @this.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
        #endregion

        #region IsAssignableFrom
        /// <summary>
        /// IsAssignableFrom
        /// </summary>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool IsAssignableFrom(this Type @this, Type other)
        {
            return @this.GetTypeInfo().IsAssignableFrom(other.GetTypeInfo());
        }
        #endregion

        #region IsAssignableTo
        /// <summary>
        /// IsAssignableTo
        /// </summary>
        /// <param name="type"></param>
        /// <param name="baseType"></param>
        /// <returns></returns>
        public static bool IsAssignableTo(this Type type, Type baseType)
        {
            var typeInfo = type.GetTypeInfo();
            var baseTypeInfo = baseType.GetTypeInfo();

            if (baseTypeInfo.IsGenericTypeDefinition)
            {
                return typeInfo.IsAssignableToGenericTypeDefinition(baseTypeInfo);
            }

            return baseTypeInfo.IsAssignableFrom(typeInfo);
        }
        #endregion

        #region IsAssignableToGenericTypeDefinition
        /// <summary>
        /// IsAssignableToGenericTypeDefinition
        /// </summary>
        /// <param name="typeInfo"></param>
        /// <param name="genericTypeInfo"></param>
        /// <returns></returns>
        public static bool IsAssignableToGenericTypeDefinition(this TypeInfo typeInfo, TypeInfo genericTypeInfo)
        {
            var interfaceTypes = typeInfo.ImplementedInterfaces.Select(t => t.GetTypeInfo());

            foreach (var interfaceType in interfaceTypes)
            {
                if (interfaceType.IsGenericType)
                {
                    var typeDefinitionTypeInfo = interfaceType
                        .GetGenericTypeDefinition()
                        .GetTypeInfo();

                    if (typeDefinitionTypeInfo.Equals(genericTypeInfo))
                    {
                        return true;
                    }
                }
            }

            if (typeInfo.IsGenericType)
            {
                var typeDefinitionTypeInfo = typeInfo
                    .GetGenericTypeDefinition()
                    .GetTypeInfo();

                if (typeDefinitionTypeInfo.Equals(genericTypeInfo))
                {
                    return true;
                }
            }

            var baseTypeInfo = typeInfo.BaseType?.GetTypeInfo();

            if (baseTypeInfo is null)
            {
                return false;
            }

            return baseTypeInfo.IsAssignableToGenericTypeDefinition(genericTypeInfo);
        }
        #endregion

        #region IsNonAbstractClass
        /// <summary>
        /// IsNonAbstractClass
        /// </summary>
        /// <param name="type"></param>
        /// <param name="publicOnly"></param>
        /// <returns></returns>
        public static bool IsNonAbstractClass(this Type type, bool publicOnly)
        {
            var typeInfo = type.GetTypeInfo();

            if (typeInfo.IsSpecialName)
            {
                return false;
            }

            if (typeInfo.IsClass && !typeInfo.IsAbstract)
            {
                if (typeInfo.IsDefined(typeof(CompilerGeneratedAttribute), inherit: true))
                {
                    return false;
                }

                if (publicOnly)
                {
                    return typeInfo.IsPublic || typeInfo.IsNestedPublic;
                }

                return true;
            }

            return false;
        }
        #endregion

        #region IsInNamespace
        /// <summary>
        /// IsInNamespace
        /// </summary>
        /// <param name="type"></param>
        /// <param name="namespace"></param>
        /// <returns></returns>
        public static bool IsInNamespace(this Type type, string @namespace)
        {
            var typeNamespace = type.Namespace ?? string.Empty;

            if (@namespace.Length > typeNamespace.Length)
            {
                return false;
            }

            var typeSubNamespace = typeNamespace.Substring(0, @namespace.Length);

            if (typeSubNamespace.Equals(@namespace, StringComparison.Ordinal))
            {
                if (typeNamespace.Length == @namespace.Length)
                {
                    //exactly the same
                    return true;
                }

                //is a subnamespace?
                return typeNamespace[@namespace.Length] == '.';
            }

            return false;
        }
        #endregion

        #region IsInExactNamespace
        /// <summary>
        /// IsInExactNamespace
        /// </summary>
        /// <param name="type"></param>
        /// <param name="namespace"></param>
        /// <returns></returns>
        public static bool IsInExactNamespace(this Type type, string @namespace)
        {
            return string.Equals(type.Namespace, @namespace, StringComparison.Ordinal);
        }
        #endregion

        #region IsAbstract
        /// <summary>
        /// IsAbstract
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool IsAbstract(this Type @this)
        {
            return @this.GetTypeInfo().IsAbstract;
        }
        #endregion

        #region IsClass
        /// <summary>
        /// IsClass
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool IsClass(this Type @this)
        {
            return @this.GetTypeInfo().IsClass;
        }
        #endregion

        #region IsEnum
        /// <summary>
        /// IsEnum
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool IsEnum(this Type @this)
        {
            return @this.GetTypeInfo().IsEnum;
        }
        #endregion

        #region IsGenericType
        /// <summary>
        /// IsGenericType
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool IsGenericType(this Type @this)
        {
            return @this.GetTypeInfo().IsGenericType;
        }
        #endregion

        #region IsGenericTypeDefinition
        /// <summary>
        /// IsGenericTypeDefinition
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool IsGenericTypeDefinition(this Type @this)
        {
            return @this.GetTypeInfo().IsGenericTypeDefinition;
        }
        #endregion

        #region IsInterface
        /// <summary>
        /// IsInterface
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool IsInterface(this Type @this)
        {
            return @this.GetTypeInfo().IsInterface;
        }
        #endregion

        #region IsPrimitive
        /// <summary>
        /// IsPrimitive
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool IsPrimitive(this Type @this)
        {
            return @this.GetTypeInfo().IsPrimitive;
        }
        #endregion

        #region IsSealed
        /// <summary>
        /// IsSealed
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool IsSealed(this Type @this)
        {
            return @this.GetTypeInfo().IsSealed;
        }
        #endregion

        #region IsValueType
        /// <summary>
        /// IsValueType
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool IsValueType(this Type @this)
        {
            return @this.GetTypeInfo().IsValueType;
        }
        #endregion

        #region IsInstanceOfType
        /// <summary>
        /// IsInstanceOfType
        /// </summary>
        /// <param name="this"></param>
        /// <param name="o"></param>
        /// <returns></returns>
        public static bool IsInstanceOfType(this Type @this, object o)
        {
            return o != null && @this.GetTypeInfo().IsAssignableFrom(o.GetType().GetTypeInfo());
        }
        #endregion

        #region IsStatic
        /// <summary>
        /// IsStatic
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool IsStatic(this FieldInfo @this)
        {
            return @this?.IsStatic ?? false;
        }

        /// <summary>
        /// IsStatic
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool IsStatic(this PropertyInfo @this)
        {
            return @this?.GetGetMethod(true)?.IsStatic
                ?? @this?.GetSetMethod(true)?.IsStatic
                ?? false;
        }

        /// <summary>
        /// IsStatic
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool IsStatic(this MemberInfo @this)
        {
            return (@this as FieldInfo).IsStatic()
                || (@this as PropertyInfo).IsStatic()
                || ((@this as MethodInfo)?.IsStatic
                ?? false);
        }
        #endregion

        #region IsPublic
        /// <summary>
        /// IsPublic
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool IsPublic(this PropertyInfo @this)
        {
            return (@this?.GetGetMethod(true)?.IsPublic ?? false)
                || (@this?.GetSetMethod(true)?.IsPublic ?? false);
        }

        /// <summary>
        /// IsPublic
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool IsPublic(this MemberInfo @this)
        {
            return (@this as FieldInfo)?.IsPublic ?? (@this as PropertyInfo).IsPublic();
        }
        #endregion

        #region IsNotPublic
        /// <summary>
        /// IsNotPublic
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool IsNotPublic(this ConstructorInfo @this)
        {
            return @this.IsPrivate
                   || @this.IsFamilyAndAssembly
                   || @this.IsFamilyOrAssembly
                   || @this.IsFamily;
        }
        #endregion

        #region IsNullableType
        /// <summary>
        /// IsNullableType
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool IsNullableType(this Type @this)
        {
            return @this.IsGenericType(typeof(Nullable<>));
        }
        #endregion

        #region IsCollectionType
        /// <summary>
        /// IsCollectionType
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool IsCollectionType(this Type @this)
        {
            return @this.IsImplementsGenericInterface(typeof(ICollection<>));
        }
        #endregion

        #region IsEnumerableType
        /// <summary>
        /// IsEnumerableType
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool IsEnumerableType(this Type @this)
        {
            return typeof(IEnumerable).IsAssignableFrom(@this);
        }
        #endregion

        #region IsQueryableType
        /// <summary>
        /// IsQueryableType
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool IsQueryableType(this Type @this)
        {
            return typeof(IQueryable).IsAssignableFrom(@this);
        }
        #endregion

        #region IsListType
        /// <summary>
        /// IsListType
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool IsListType(this Type @this)
        {
            return typeof(IList).IsAssignableFrom(@this);
        }
        #endregion

        #region IsListOrDictionaryType
        /// <summary>
        /// IsListOrDictionaryType
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool IsListOrDictionaryType(this Type @this)
        {
            return @this.IsListType() || @this.IsDictionaryType();
        }
        #endregion

        #region IsDictionaryType
        /// <summary>
        /// IsDictionaryType
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool IsDictionaryType(this Type @this)
        {
            return @this.IsImplementsGenericInterface(typeof(IDictionary<,>));
        }
        #endregion

        #region ImplementsGenericInterface
        /// <summary>
        /// ImplementsGenericInterface
        /// </summary>
        /// <param name="this"></param>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        public static bool IsImplementsGenericInterface(this Type @this, Type interfaceType)
        {
            if (@this.IsGenericType(interfaceType))
            {
                return true;
            }
            foreach (var @interface in @this.GetTypeInfo().ImplementedInterfaces)
            {
                if (@interface.IsGenericType(interfaceType))
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region IsGenericType
        /// <summary>
        /// IsGenericType
        /// </summary>
        /// <param name="this"></param>
        /// <param name="genericType"></param>
        /// <returns></returns>
        public static bool IsGenericType(this Type @this, Type genericType)
        {
            return @this.IsGenericType() && @this.GetGenericTypeDefinition() == genericType;
        }
        #endregion

        #region PropertiesWithAnInaccessibleSetter
        /// <summary>
        /// PropertiesWithAnInaccessibleSetter
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> PropertiesWithAnInaccessibleSetter(this Type @this)
        {
            return @this.GetDeclaredProperties().Where(pm => pm.HasAnInaccessibleSetter());
        }
        #endregion

        #region HasAnInaccessibleSetter
        /// <summary>
        /// HasAnInaccessibleSetter
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool HasAnInaccessibleSetter(this PropertyInfo @this)
        {
            var setMethod = @this.GetSetMethod(true);
            return setMethod == null || setMethod.IsPrivate || setMethod.IsFamily;
        }
        #endregion

        #region Assembly  
        /// <summary>
        /// Assembly
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static Assembly Assembly(this Type @this)
        {
            return @this.GetTypeInfo().Assembly;
        }
        #endregion

        #region BaseType
        /// <summary>
        /// BaseType
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static Type BaseType(this Type @this)
        {
            return @this.GetTypeInfo().BaseType;
        }
        #endregion                

        #region GetBaseTypes
        /// <summary>
        /// GetBaseTypes
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetBaseTypes(this Type type)
        {
            var typeInfo = type.GetTypeInfo();

            foreach (var implementedInterface in typeInfo.ImplementedInterfaces)
            {
                yield return implementedInterface;
            }

            var baseType = typeInfo.BaseType;

            while (baseType != null)
            {
                var baseTypeInfo = baseType.GetTypeInfo();

                yield return baseType;

                baseType = baseTypeInfo.BaseType;
            }
        }
        #endregion

        #region Has
        /// <summary>
        /// Has
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool Has<TAttribute>(this Type @this) where TAttribute : Attribute
        {
            return @this.GetTypeInfo().IsDefined(typeof(TAttribute), inherit: false);
        }
        #endregion

        #region HasAttribute
        /// <summary>
        /// HasAttribute
        /// </summary>
        /// <param name="type"></param>
        /// <param name="attributeType"></param>
        /// <returns></returns>
        public static bool HasAttribute(this Type type, Type attributeType)
        {
            return type.GetTypeInfo().IsDefined(attributeType, inherit: true);
        }

        /// <summary>
        /// HasAttribute
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static bool HasAttribute<T>(this Type type, Func<T, bool> predicate) where T : Attribute
        {
            return type.GetTypeInfo().GetCustomAttributes<T>(inherit: true).Any(predicate);
        }
        #endregion

        #region As
        /// <summary>
        /// 是否能够转为指定基类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool As<T>(this Type @this) => @this.As(typeof(T));

        /// <summary>
        /// 是否子类
        /// </summary>
        /// <param name="this"></param>
        /// <param name="baseType"></param>
        /// <returns></returns>
        public static bool As(this Type @this, Type baseType)
        {
            if (@this == null) return false;
            // 如果基类是泛型定义
            if (baseType.IsGenericTypeDefinition && @this.IsGenericType && !@this.IsGenericTypeDefinition) @this = @this.GetGenericTypeDefinition();
            if (@this == baseType) return true;
            if (baseType.IsAssignableFrom(@this)) return true;
            // 接口
            if (baseType.IsInterface)
            {
                if (@this.GetInterface(baseType.FullName) != null) return true;
                if (@this.GetInterfaces().Any(e => e.IsGenericType && baseType.IsGenericTypeDefinition ? e.GetGenericTypeDefinition() == baseType : e == baseType)) return true;
            }
            // 判断是否子类时，支持只反射加载的程序集
            if (@this.Assembly.ReflectionOnly)
            {
                // 反射加载时，需要特殊处理接口                
                while (@this != typeof(object))
                {
                    if (@this.FullName == baseType.FullName &&
                        @this.AssemblyQualifiedName == baseType.AssemblyQualifiedName)
                        return true;
                    @this = @this.BaseType;
                }
            }
            return false;
        }
        #endregion

        #region GetElementType
        /// <summary>
        /// 获取一个类型的元素类型
        /// </summary>
        /// <param name="this">类型</param>
        /// <returns></returns>
        public static Type GetElementType(this Type @this)
        {
            if (@this.HasElementType) return @this.GetElementType();
            if (@this.As<IEnumerable>())
            {
                // 如果实现了IEnumerable<>接口，那么取泛型参数
                foreach (var item in @this.GetInterfaces())
                {
                    if (item.IsGenericType && item.GetGenericTypeDefinition() == typeof(IEnumerable<>)) return item.GetGenericArguments()[0];
                }
            }
            return null;
        }
        #endregion

        #region GetTypeArray
        /// <summary>
        /// 从参数数组中获取类型数组
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static Type[] GetTypeArray(this object[] @this)
        {
            if (@this == null) return Type.EmptyTypes;
            var typeArray = new Type[@this.Length];
            for (var i = 0; i < typeArray.Length; i++)
            {
                if (@this[i] == null)
                    typeArray[i] = typeof(object);
                else
                    typeArray[i] = @this[i].GetType();
            }
            return typeArray;
        }
        #endregion

        #region GetMemberType
        /// <summary>
        /// 获取成员的类型，字段和属性是它们的类型，方法是返回类型，类型是自身
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static Type GetMemberType(this MemberInfo @this)
        {
            switch (@this.MemberType)
            {
                case MemberTypes.Constructor:
                    return (@this as ConstructorInfo).DeclaringType;
                case MemberTypes.Field:
                    return (@this as FieldInfo).FieldType;
                case MemberTypes.Method:
                    return (@this as MethodInfo).ReturnType;
                case MemberTypes.Property:
                    return (@this as PropertyInfo).PropertyType;
                case MemberTypes.TypeInfo:
                case MemberTypes.NestedType:
                    return @this as Type;
                default:
                    return null;
            }
        }
        #endregion

        #region GetTypeCode
        /// <summary>
        /// 获取类型代码
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static TypeCode GetTypeCode(this Type @this) => Type.GetTypeCode(@this);
        #endregion

        #region IsInt
        /// <summary>
        /// 是否整数
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool IsInt(this Type @this)
        {
            switch (@this.GetTypeCode())
            {
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }
        #endregion

        #region IsList
        /// <summary>
        /// 是否泛型列表
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool IsList(this Type @this) => @this != null && @this.IsGenericType && @this.As(typeof(IList<>));
        #endregion

        #region IsDictionary
        /// <summary>
        /// 是否泛型字典
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool IsDictionary(this Type @this) => @this != null && @this.IsGenericType && @this.As(typeof(IDictionary<,>));
        #endregion

        #region CreateInstance
        /// <summary>
        /// A Type extension method that creates an instance.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="bindingAttr">The binding attribute.</param>
        /// <param name="binder">The binder.</param>
        /// <param name="args">The arguments.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The new instance.</returns>
        public static T CreateInstance<T>(this Type @this, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture)
        {
            return (T)Activator.CreateInstance(@this, bindingAttr, binder, args, culture);
        }

        /// <summary>
        /// A Type extension method that creates an instance.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="bindingAttr">The binding attribute.</param>
        /// <param name="binder">The binder.</param>
        /// <param name="args">The arguments.</param>
        /// <param name="culture">The culture.</param>
        /// <param name="activationAttributes">The activation attributes.</param>
        /// <returns>The new instance.</returns>
        public static T CreateInstance<T>(this Type @this, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes)
        {
            return (T)Activator.CreateInstance(@this, bindingAttr, binder, args, culture, activationAttributes);
        }

        /// <summary>
        /// A Type extension method that creates an instance.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>The new instance.</returns>
        public static T CreateInstance<T>(this Type @this, object[] args)
        {
            return (T)Activator.CreateInstance(@this, args);
        }

        /// <summary>
        /// A Type extension method that creates an instance.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="args">The arguments.</param>
        /// <param name="activationAttributes">The activation attributes.</param>
        /// <returns>The new instance.</returns>
        public static T CreateInstance<T>(this Type @this, object[] args, object[] activationAttributes)
        {
            return (T)Activator.CreateInstance(@this, args, activationAttributes);
        }

        /// <summary>
        /// A Type extension method that creates an instance.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The new instance.</returns>
        public static T CreateInstance<T>(this Type @this)
        {
            return (T)Activator.CreateInstance(@this);
        }

        /// <summary>
        /// A Type extension method that creates an instance.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="nonPublic">true to non public.</param>
        /// <returns>The new instance.</returns>
        public static T CreateInstance<T>(this Type @this, bool nonPublic)
        {
            return (T)Activator.CreateInstance(@this, nonPublic);
        }

        /// <summary>
        ///     Creates an instance of the specified type using the constructor that best matches the specified parameters.
        /// </summary>
        /// <param name="type">The type of object to create.</param>
        /// <param name="bindingAttr">
        ///     A combination of zero or more bit flags that affect the search for the  constructor. If
        ///     is zero, a case-sensitive search for public constructors is conducted.
        /// </param>
        /// <param name="binder">
        ///     An object that uses  and  to seek and identify the  constructor. If  is null, the default
        ///     binder is used.
        /// </param>
        /// <param name="args">
        ///     An array of arguments that match in number, order, and type the parameters of the constructor
        ///     to invoke. If  is an empty array or null, the constructor that takes no parameters (the default constructor) is
        ///     invoked.
        /// </param>
        /// <param name="culture">
        ///     Culture-specific information that governs the coercion of  to the formal types declared for
        ///     the  constructor. If  is null, the  for the current thread is used.
        /// </param>
        /// <returns>A reference to the newly created object.</returns>
        public static object CreateInstance(this Type type, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture)
        {
            return Activator.CreateInstance(type, bindingAttr, binder, args, culture);
        }

        /// <summary>
        ///     Creates an instance of the specified type using the constructor that best matches the specified parameters.
        /// </summary>
        /// <param name="type">The type of object to create.</param>
        /// <param name="bindingAttr">
        ///     A combination of zero or more bit flags that affect the search for the  constructor. If
        ///     is zero, a case-sensitive search for public constructors is conducted.
        /// </param>
        /// <param name="binder">
        ///     An object that uses  and  to seek and identify the  constructor. If  is null, the default
        ///     binder is used.
        /// </param>
        /// <param name="args">
        ///     An array of arguments that match in number, order, and type the parameters of the constructor
        ///     to invoke. If  is an empty array or null, the constructor that takes no parameters (the default constructor) is
        ///     invoked.
        /// </param>
        /// <param name="culture">
        ///     Culture-specific information that governs the coercion of  to the formal types declared for
        ///     the  constructor. If  is null, the  for the current thread is used.
        /// </param>
        /// <param name="activationAttributes">
        ///     An array of one or more attributes that can participate in activation. This
        ///     is typically an array that contains a single  object. The  specifies the URL that is required to activate a
        ///     remote object.
        /// </param>
        /// <returns>A reference to the newly created object.</returns>
        public static object CreateInstance(this Type type, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes)
        {
            return Activator.CreateInstance(type, bindingAttr, binder, args, culture, activationAttributes);
        }

        /// <summary>
        ///     Creates an instance of the specified type using the constructor that best matches the specified parameters.
        /// </summary>
        /// <param name="type">The type of object to create.</param>
        /// <param name="args">
        ///     An array of arguments that match in number, order, and type the parameters of the constructor
        ///     to invoke. If  is an empty array or null, the constructor that takes no parameters (the default constructor) is
        ///     invoked.
        /// </param>
        /// <returns>A reference to the newly created object.</returns>
        public static object CreateInstance(this Type type, object[] args)
        {
            return Activator.CreateInstance(type, args);
        }

        /// <summary>
        ///     Creates an instance of the specified type using the constructor that best matches the specified parameters.
        /// </summary>
        /// <param name="type">The type of object to create.</param>
        /// <param name="args">
        ///     An array of arguments that match in number, order, and type the parameters of the constructor
        ///     to invoke. If  is an empty array or null, the constructor that takes no parameters (the default constructor) is
        ///     invoked.
        /// </param>
        /// <param name="activationAttributes">
        ///     An array of one or more attributes that can participate in activation. This
        ///     is typically an array that contains a single  object. The  specifies the URL that is required to activate a
        ///     remote object.
        /// </param>
        /// <returns>A reference to the newly created object.</returns>
        public static object CreateInstance(this Type type, object[] args, object[] activationAttributes)
        {
            return Activator.CreateInstance(type, args, activationAttributes);
        }

        /// <summary>
        ///     Creates an instance of the specified type using that type&#39;s default constructor.
        /// </summary>
        /// <param name="type">The type of object to create.</param>
        /// <returns>A reference to the newly created object.</returns>
        public static object CreateInstance(this Type type)
        {
            return Activator.CreateInstance(type);
        }

        /// <summary>
        ///     Creates an instance of the specified type using that type&#39;s default constructor.
        /// </summary>
        /// <param name="type">The type of object to create.</param>
        /// <param name="nonPublic">
        ///     true if a public or nonpublic default constructor can match; false if only a public
        ///     default constructor can match.
        /// </param>
        /// <returns>A reference to the newly created object.</returns>
        public static object CreateInstance(this Type type, bool nonPublic)
        {
            return Activator.CreateInstance(type, nonPublic);
        }
        #endregion

        #region GetObject
        /// <summary>
        /// Creates a proxy for the well-known object indicated by the specified type and URL.
        /// </summary>
        /// <param name="this">The type of the well-known object to which you want to connect.</param>
        /// <param name="url">The URL of the well-known object.</param>
        /// <returns>A proxy that points to an endpoint served by the requested well-known object.</returns>
        public static object GetObject(this Type @this, string url)
        {
            return Activator.GetObject(@this, url);
        }

        /// <summary>
        /// Creates a proxy for the well-known object indicated by the specified type, URL, and channel data.
        /// </summary>
        /// <param name="this">The type of the well-known object to which you want to connect.</param>
        /// <param name="url">The URL of the well-known object.</param>
        /// <param name="state">Channel-specific data or null.</param>
        /// <returns>A proxy that points to an endpoint served by the requested well-known object.</returns>
        public static object GetObject(this Type @this, string url, object state)
        {
            return Activator.GetObject(@this, url, state);
        }
        #endregion

        #region ToDbType
        /// <summary>
        /// 转换为DbType
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static DbType? ToDbType(this Type @this)
        {
            var typeMap = new Dictionary<Type, DbType>
            {
                [typeof(byte)] = DbType.Byte,
                [typeof(sbyte)] = DbType.SByte,
                [typeof(short)] = DbType.Int16,
                [typeof(ushort)] = DbType.UInt16,
                [typeof(int)] = DbType.Int32,
                [typeof(uint)] = DbType.UInt32,
                [typeof(long)] = DbType.Int64,
                [typeof(ulong)] = DbType.UInt64,
                [typeof(float)] = DbType.Single,
                [typeof(double)] = DbType.Double,
                [typeof(decimal)] = DbType.Decimal,
                [typeof(bool)] = DbType.Boolean,
                [typeof(string)] = DbType.String,
                [typeof(char)] = DbType.StringFixedLength,
                [typeof(Guid)] = DbType.Guid,
                [typeof(DateTime)] = DbType.DateTime,
                [typeof(DateTimeOffset)] = DbType.DateTimeOffset,
                [typeof(byte[])] = DbType.Binary,
                [typeof(byte?)] = DbType.Byte,
                [typeof(sbyte?)] = DbType.SByte,
                [typeof(short?)] = DbType.Int16,
                [typeof(ushort?)] = DbType.UInt16,
                [typeof(int?)] = DbType.Int32,
                [typeof(uint?)] = DbType.UInt32,
                [typeof(long?)] = DbType.Int64,
                [typeof(ulong?)] = DbType.UInt64,
                [typeof(float?)] = DbType.Single,
                [typeof(double?)] = DbType.Double,
                [typeof(decimal?)] = DbType.Decimal,
                [typeof(bool?)] = DbType.Boolean,
                [typeof(char?)] = DbType.StringFixedLength,
                [typeof(Guid?)] = DbType.Guid,
                [typeof(DateTime?)] = DbType.DateTime,
                [typeof(DateTimeOffset?)] = DbType.DateTimeOffset
            };
            if (typeMap.Keys.Contains(@this))
            {
                return typeMap[@this];
            }
            return null;
        }
        #endregion

        #region GetDescriptions
        /// <summary>
        /// 获取枚举类型的所有字段注释
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static Dictionary<int, string> GetDescriptions(this Type @this)
        {
            var dic = new Dictionary<int, string>();
            foreach (var item in @this.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (!item.IsStatic) continue;
                var value = Convert.ToInt32(item.GetValue(null));
                var des = item.Name;
                var dna = item.GetCustomAttribute<DisplayNameAttribute>(false);
                if (dna != null && !string.IsNullOrEmpty(dna.DisplayName)) des = dna.DisplayName;
                var att = item.GetCustomAttribute<DescriptionAttribute>(false);
                if (att != null && !string.IsNullOrEmpty(att.Description)) des = att.Description;
                // 有些枚举可能不同名称有相同的值
                dic[value] = des;
            }
            return dic;
        }
        #endregion
    }
}