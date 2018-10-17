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
                    var interfaceType = item.GetInterfaces();
                    if (item.IsGenericType) continue;
                    result.Add(item, interfaceType);
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
