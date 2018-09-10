using System;
using System.Collections;
using System.Linq;
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
