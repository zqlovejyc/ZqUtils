﻿#region License
/***
 * Copyright © 2018-2025, 张强 (943620963@qq.com).
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using ZqUtils.Extensions;

namespace ZqUtils.Reflection
{
    /// <summary>
    /// 反射接口
    /// </summary>
    /// <remarks>
    /// 该接口仅用于扩展，不建议外部使用
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface IReflect
    {
        #region 反射获取
        /// <summary>
        /// 根据名称获取类型
        /// </summary>
        /// <param name="typeName">类型名</param>
        /// <param name="isLoadAssembly">是否从未加载程序集中获取类型。使用仅反射的方法检查目标类型，如果存在，则进行常规加载</param>
        /// <returns></returns>
        Type GetType(string typeName, bool isLoadAssembly);

        /// <summary>
        /// 获取方法
        /// </summary>
        /// <remarks>
        /// 用于具有多个签名的同名方法的场合，不确定是否存在性能问题，不建议普通场合使用
        /// </remarks>
        /// <param name="type">类型</param>
        /// <param name="name">名称</param>
        /// <param name="paramTypes">参数类型数组</param>
        /// <returns></returns>
        MethodInfo GetMethod(Type type, string name, params Type[] paramTypes);

        /// <summary>
        /// 获取指定名称的方法集合，支持指定参数个数来匹配过滤
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="paramCount">参数个数，-1表示不过滤参数个数</param>
        /// <returns></returns>
        MethodInfo[] GetMethods(Type type, string name, int paramCount = -1);

        /// <summary>
        /// 获取属性
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="name">名称</param>
        /// <param name="ignoreCase">忽略大小写</param>
        /// <returns></returns>
        PropertyInfo GetProperty(Type type, string name, bool ignoreCase);

        /// <summary>
        /// 获取字段
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="name">名称</param>
        /// <param name="ignoreCase">忽略大小写</param>
        /// <returns></returns>
        FieldInfo GetField(Type type, string name, bool ignoreCase);

        /// <summary>
        /// 获取成员
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="name">名称</param>
        /// <param name="ignoreCase">忽略大小写</param>
        /// <returns></returns>
        MemberInfo GetMember(Type type, string name, bool ignoreCase);

        /// <summary>
        /// 获取字段
        /// </summary>
        /// <param name="type"></param>
        /// <param name="baseFirst"></param>
        /// <returns></returns>
        IList<FieldInfo> GetFields(Type type, bool baseFirst = true);

        /// <summary>
        /// 获取属性
        /// </summary>
        /// <param name="type"></param>
        /// <param name="baseFirst"></param>
        /// <returns></returns>
        IList<PropertyInfo> GetProperties(Type type, bool baseFirst = true);
        #endregion

        #region 反射调用
        /// <summary>
        /// 反射创建指定类型的实例
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="parameters">参数数组</param>
        /// <returns></returns>
        object CreateInstance(Type type, params object[] parameters);

        /// <summary>
        /// 反射调用指定对象的方法
        /// </summary>
        /// <param name="target">要调用其方法的对象，如果要调用静态方法，则target是类型</param>
        /// <param name="method">方法</param>
        /// <param name="parameters">方法参数</param>
        /// <returns></returns>
        object Invoke(object target, MethodBase method, params object[] parameters);

        /// <summary>
        /// 反射调用指定对象的方法
        /// </summary>
        /// <param name="target">要调用其方法的对象，如果要调用静态方法，则target是类型</param>
        /// <param name="method">方法</param>
        /// <param name="parameters">方法参数字典</param>
        /// <returns></returns>
        object InvokeWithParams(object target, MethodBase method, IDictionary parameters);

        /// <summary>
        /// 获取目标对象的属性值
        /// </summary>
        /// <param name="target">目标对象</param>
        /// <param name="property">属性</param>
        /// <returns></returns>
        object GetValue(object target, PropertyInfo property);

        /// <summary>
        /// 获取目标对象的字段值
        /// </summary>
        /// <param name="target">目标对象</param>
        /// <param name="field">字段</param>
        /// <returns></returns>
        object GetValue(object target, FieldInfo field);

        /// <summary>
        /// 设置目标对象的属性值
        /// </summary>
        /// <param name="target">目标对象</param>
        /// <param name="property">属性</param>
        /// <param name="value">数值</param>
        void SetValue(object target, PropertyInfo property, object value);

        /// <summary>
        /// 设置目标对象的字段值
        /// </summary>
        /// <param name="target">目标对象</param>
        /// <param name="field">字段</param>
        /// <param name="value">数值</param>
        void SetValue(object target, FieldInfo field, object value);

        /// <summary>
        /// 从源对象拷贝数据到目标对象
        /// </summary>
        /// <param name="target">目标对象</param>
        /// <param name="src">源对象</param>
        /// <param name="deep">递归深度拷贝，直接拷贝成员值而不是引用</param>
        /// <param name="excludes">要忽略的成员</param>
        void Copy(object target, object src, bool deep = false, params string[] excludes);

        /// <summary>
        /// 从源字典拷贝数据到目标对象
        /// </summary>
        /// <param name="target">目标对象</param>
        /// <param name="dic">源字典</param>
        /// <param name="deep">递归深度拷贝，直接拷贝成员值而不是引用</param>
        void Copy(object target, IDictionary<string, object> dic, bool deep = false);
        #endregion

        #region 类型辅助
        /// <summary>
        /// 获取一个类型的元素类型
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        Type GetElementType(Type type);

        /// <summary>
        /// 类型转换
        /// </summary>
        /// <param name="value">数值</param>
        /// <param name="conversionType"></param>
        /// <returns></returns>
        object ChangeType(object value, Type conversionType);

        /// <summary>
        /// 获取类型的友好名称
        /// </summary>
        /// <param name="type">指定类型</param>
        /// <param name="isfull">是否全名，包含命名空间</param>
        /// <returns></returns>
        string GetName(Type type, bool isfull);
        #endregion

        #region 插件
        /// <summary>
        /// 是否能够转为指定基类
        /// </summary>
        /// <param name="type"></param>
        /// <param name="baseType"></param>
        /// <returns></returns>
        bool As(Type type, Type baseType);

        /// <summary>
        /// 在指定程序集中查找指定基类或接口的所有子类实现
        /// </summary>
        /// <param name="asm">指定程序集</param>
        /// <param name="baseType">基类或接口，为空时返回所有类型</param>
        /// <returns></returns>
        IEnumerable<Type> GetSubclasses(Assembly asm, Type baseType);

        /// <summary>
        /// 在所有程序集中查找指定基类或接口的子类实现
        /// </summary>
        /// <param name="baseType">基类或接口</param>
        /// <param name="isLoadAssembly">是否加载为加载程序集</param>
        /// <returns></returns>
        IEnumerable<Type> GetAllSubclasses(Type baseType, bool isLoadAssembly);
        #endregion
    }

    /// <summary>
    /// 默认反射实现
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public class DefaultReflect : IReflect
    {
        #region 反射获取
        /// <summary>
        /// 根据名称获取类型
        /// </summary>
        /// <param name="typeName">类型名</param>
        /// <param name="isLoadAssembly">是否从未加载程序集中获取类型。使用仅反射的方法检查目标类型，如果存在，则进行常规加载</param>
        /// <returns></returns>
        public virtual Type GetType(string typeName, bool isLoadAssembly) => AssemblyX.GetType(typeName, isLoadAssembly);

        static BindingFlags bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
        static BindingFlags bfic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase;

        /// <summary>
        /// 获取方法
        /// </summary>
        /// <remarks>
        /// 用于具有多个签名的同名方法的场合，不确定是否存在性能问题，不建议普通场合使用
        /// </remarks>
        /// <param name="type">类型</param>
        /// <param name="name">名称</param>
        /// <param name="paramTypes">参数类型数组</param>
        /// <returns></returns>
        public virtual MethodInfo GetMethod(Type type, string name, params Type[] paramTypes)
        {
            MethodInfo mi = null;
            while (true)
            {
                if (paramTypes == null || paramTypes.Length == 0)
                    mi = type.GetMethod(name, bf);
                else
                    mi = type.GetMethod(name, bf, null, paramTypes, null);
                if (mi != null) return mi;
                type = type.BaseType;
                if (type == null || type == typeof(object)) break;
            }
            return null;
        }

        /// <summary>
        /// 获取指定名称的方法集合，支持指定参数个数来匹配过滤
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="paramCount">参数个数，-1表示不过滤参数个数</param>
        /// <returns></returns>
        public virtual MethodInfo[] GetMethods(Type type, string name, int paramCount = -1)
        {
            var ms = type.GetMethods(bf);
            if (ms == null || ms.Length == 0) return ms;
            var list = new List<MethodInfo>();
            foreach (var item in ms)
            {
                if (item.Name == name)
                {
                    if (paramCount >= 0 && item.GetParameters().Length == paramCount) list.Add(item);
                }
            }
            return list.ToArray();
        }

        /// <summary>
        /// 获取属性
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="name">名称</param>
        /// <param name="ignoreCase">忽略大小写</param>
        /// <returns></returns>
        public virtual PropertyInfo GetProperty(Type type, string name, bool ignoreCase)
        {
            // 父类私有属性的获取需要递归，可见范围则不需要，有些类型的父类为空，比如接口
            while (type != null && type != typeof(object))
            {                
                var pi = type.GetProperty(name, bf);
                if (pi != null) return pi;
                if (ignoreCase)
                {
                    pi = type.GetProperty(name, bfic);
                    if (pi != null) return pi;
                }
                type = type.BaseType;
            }
            return null;
        }

        /// <summary>
        /// 获取字段
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="name">名称</param>
        /// <param name="ignoreCase">忽略大小写</param>
        /// <returns></returns>
        public virtual FieldInfo GetField(Type type, string name, bool ignoreCase)
        {
            // 父类私有字段的获取需要递归，可见范围则不需要，有些类型的父类为空，比如接口
            while (type != null && type != typeof(object))
            {                
                var fi = type.GetField(name, bf);
                if (fi != null) return fi;
                if (ignoreCase)
                {
                    fi = type.GetField(name, bfic);
                    if (fi != null) return fi;
                }
                type = type.BaseType;
            }
            return null;
        }

        /// <summary>
        /// 获取成员
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="name">名称</param>
        /// <param name="ignoreCase">忽略大小写</param>
        /// <returns></returns>
        public virtual MemberInfo GetMember(Type type, string name, bool ignoreCase)
        {
            // 父类私有成员的获取需要递归，可见范围则不需要，有些类型的父类为空，比如接口
            while (type != null && type != typeof(object))
            {
                var fs = type.GetMember(name, ignoreCase ? bfic : bf);
                if (fs != null && fs.Length > 0)
                {
                    // 得到多个的时候，优先返回精确匹配
                    if (ignoreCase && fs.Length > 1)
                    {
                        foreach (var fi in fs)
                        {
                            if (fi.Name == name) return fi;
                        }
                    }
                    return fs[0];
                }
                type = type.BaseType;
            }
            return null;
        }
        #endregion

        #region 反射获取 字段/属性
        private ConcurrentDictionary<Type, IList<FieldInfo>> _cache1 = new ConcurrentDictionary<Type, IList<FieldInfo>>();
        private ConcurrentDictionary<Type, IList<FieldInfo>> _cache2 = new ConcurrentDictionary<Type, IList<FieldInfo>>();

        /// <summary>
        /// 获取字段
        /// </summary>
        /// <param name="type"></param>
        /// <param name="baseFirst"></param>
        /// <returns></returns>
        public virtual IList<FieldInfo> GetFields(Type type, bool baseFirst = true)
        {
            if (baseFirst)
                return _cache1.GetOrAdd(type, key => GetFields2(key, true));
            else
                return _cache2.GetOrAdd(type, key => GetFields2(key, false));
        }

        IList<FieldInfo> GetFields2(Type type, bool baseFirst)
        {
            var list = new List<FieldInfo>();
            // Void*的基类就是null
            if (type == typeof(object) || type.BaseType == null) return list;
            if (baseFirst) list.AddRange(GetFields(type.BaseType));
            var fis = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var fi in fis)
            {
                if (fi.GetCustomAttribute<NonSerializedAttribute>() != null) continue;
                list.Add(fi);
            }
            if (!baseFirst) list.AddRange(GetFields(type.BaseType));
            return list;
        }

        private ConcurrentDictionary<Type, IList<PropertyInfo>> _cache3 = new ConcurrentDictionary<Type, IList<PropertyInfo>>();
        private ConcurrentDictionary<Type, IList<PropertyInfo>> _cache4 = new ConcurrentDictionary<Type, IList<PropertyInfo>>();

        /// <summary>
        /// 获取属性
        /// </summary>
        /// <param name="type"></param>
        /// <param name="baseFirst"></param>
        /// <returns></returns>
        public virtual IList<PropertyInfo> GetProperties(Type type, bool baseFirst = true)
        {
            if (baseFirst)
                return _cache3.GetOrAdd(type, key => GetProperties2(key, true));
            else
                return _cache4.GetOrAdd(type, key => GetProperties2(key, false));
        }

        IList<PropertyInfo> GetProperties2(Type type, bool baseFirst)
        {
            var list = new List<PropertyInfo>();
            // Void*的基类就是null
            if (type == typeof(object) || type.BaseType == null) return list;
            // 本身type.GetProperties就可以得到父类属性，只是不能保证父类属性在子类属性之前
            if (baseFirst) list.AddRange(GetProperties(type.BaseType));
            // 父类子类可能因为继承而有重名的属性，此时以子类优先，否则反射父类属性会出错
            var set = new HashSet<string>(list.Select(e => e.Name));
            var pis = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var pi in pis)
            {
                if (pi.GetIndexParameters().Length > 0) continue;
                if (pi.GetCustomAttribute<XmlIgnoreAttribute>() != null) continue;
                if (pi.GetCustomAttribute<ScriptIgnoreAttribute>() != null) continue;
                if (!set.Contains(pi.Name))
                {
                    list.Add(pi);
                    set.Add(pi.Name);
                }
            }
            if (!baseFirst) list.AddRange(GetProperties(type.BaseType).Where(e => !set.Contains(e.Name)));
            return list;
        }
        #endregion

        #region 反射调用
        /// <summary>
        /// 反射创建指定类型的实例
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="parameters">参数数组</param>
        /// <returns></returns>
        public virtual object CreateInstance(Type type, params object[] parameters)
        {
            try
            {
                if (parameters == null || parameters.Length == 0)
                    return Activator.CreateInstance(type, true);
                else
                    return Activator.CreateInstance(type, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("创建对象失败 type={0} parameters={1} {2}".F(type.FullName, parameters.Join(), ex.GetTrue()?.Message), ex);
            }
        }

        /// <summary>
        /// 反射调用指定对象的方法
        /// </summary>
        /// <param name="target">要调用其方法的对象，如果要调用静态方法，则target是类型</param>
        /// <param name="method">方法</param>
        /// <param name="parameters">方法参数</param>
        /// <returns></returns>
        public virtual object Invoke(object target, MethodBase method, params object[] parameters) => method.Invoke(target, parameters);

        /// <summary>
        /// 反射调用指定对象的方法
        /// </summary>
        /// <param name="target">要调用其方法的对象，如果要调用静态方法，则target是类型</param>
        /// <param name="method">方法</param>
        /// <param name="parameters">方法参数字典</param>
        /// <returns></returns>
        public virtual object InvokeWithParams(object target, MethodBase method, IDictionary parameters)
        {
            // 该方法没有参数，无视外部传入参数
            var pis = method.GetParameters();
            if (pis == null || pis.Length < 1) return Invoke(target, method, null);
            var ps = new object[pis.Length];
            for (var i = 0; i < pis.Length; i++)
            {
                object v = null;
                if (parameters != null && parameters.Contains(pis[i].Name)) v = parameters[pis[i].Name];
                ps[i] = v.ChangeType(pis[i].ParameterType);
            }
            return method.Invoke(target, ps);
        }

        /// <summary>
        /// 获取目标对象的属性值
        /// </summary>
        /// <param name="target">目标对象</param>
        /// <param name="property">属性</param>
        /// <returns></returns>
        public virtual object GetValue(object target, PropertyInfo property) => property.GetValue(target, null);

        /// <summary>
        /// 获取目标对象的字段值
        /// </summary>
        /// <param name="target">目标对象</param>
        /// <param name="field">字段</param>
        /// <returns></returns>
        public virtual object GetValue(object target, FieldInfo field) => field.GetValue(target);

        /// <summary>
        /// 设置目标对象的属性值
        /// </summary>
        /// <param name="target">目标对象</param>
        /// <param name="property">属性</param>
        /// <param name="value">数值</param>
        public virtual void SetValue(object target, PropertyInfo property, object value) => property.SetValue(target, value.ChangeType(property.PropertyType), null);

        /// <summary>
        /// 设置目标对象的字段值
        /// </summary>
        /// <param name="target">目标对象</param>
        /// <param name="field">字段</param>
        /// <param name="value">数值</param>
        public virtual void SetValue(object target, FieldInfo field, object value) => field.SetValue(target, value.ChangeType(field.FieldType));
        #endregion

        #region 对象拷贝
        /// <summary>
        /// 从源对象拷贝数据到目标对象
        /// </summary>
        /// <param name="target">目标对象</param>
        /// <param name="src">源对象</param>
        /// <param name="deep">递归深度拷贝，直接拷贝成员值而不是引用</param>
        /// <param name="excludes">要忽略的成员</param>
        public virtual void Copy(object target, object src, bool deep = false, params string[] excludes)
        {
            if (target == null || src == null || target == src) return;
            var type = target.GetType();
            // 基础类型无法拷贝
            if (type.GetTypeCode() != TypeCode.Object) throw new Exception(string.Format("基础类型 {0} 无法拷贝", type.FullName));
            // 不是深度拷贝时，直接复制引用
            if (!deep)
            {
                var stype = src.GetType();
                foreach (var pi in type.GetProperties())
                {
                    if (!pi.CanWrite) continue;
                    if (excludes != null && excludes.Contains(pi.Name)) continue;
                    if (pi.GetIndexParameters().Length > 0) continue;
                    if (pi.GetCustomAttribute<XmlIgnoreAttribute>() != null) continue;
                    var pi2 = stype.GetProperty(pi.Name);
                    if (pi2 != null && pi2.CanRead) SetValue(target, pi, GetValue(src, pi2));
                }
                return;
            }
            // 来源对象转为字典
            var dic = new Dictionary<string, object>();
            foreach (var pi in src.GetType().GetProperties())
            {
                if (!pi.CanRead) continue;
                if (excludes != null && excludes.Contains(pi.Name)) continue;
                if (pi.GetIndexParameters().Length > 0) continue;
                if (pi.GetCustomAttribute<XmlIgnoreAttribute>() != null) continue;

                dic[pi.Name] = GetValue(src, pi);
            }
            Copy(target, dic, deep);
        }

        /// <summary>
        /// 从源字典拷贝数据到目标对象
        /// </summary>
        /// <param name="target">目标对象</param>
        /// <param name="dic">源字典</param>
        /// <param name="deep">递归深度拷贝，直接拷贝成员值而不是引用</param>
        public virtual void Copy(object target, IDictionary<string, object> dic, bool deep = false)
        {
            if (target == null || dic == null || dic.Count == 0 || target == dic) return;
            foreach (var pi in target.GetType().GetProperties())
            {
                if (!pi.CanWrite) continue;
                if (pi.GetIndexParameters().Length > 0) continue;
                if (pi.GetCustomAttribute<XmlIgnoreAttribute>() != null) continue;
                if (dic.TryGetValue(pi.Name, out var obj))
                {
                    // 基础类型直接拷贝，不考虑深拷贝
                    if (deep && pi.PropertyType.GetTypeCode() == TypeCode.Object)
                    {
                        var v = GetValue(target, pi);
                        // 如果目标对象该成员为空，需要创建再拷贝
                        if (v == null)
                        {
                            v = pi.PropertyType.CreateInstance();
                            SetValue(target, pi, v);
                        }
                        Copy(v, obj, deep);
                    }
                    else
                        SetValue(target, pi, obj);
                }
            }
        }
        #endregion

        #region 类型辅助
        /// <summary>
        /// 获取一个类型的元素类型
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public virtual Type GetElementType(Type type)
        {
            if (type.HasElementType) return type.GetElementType();
            if (type.As<IEnumerable>())
            {
                // 如果实现了IEnumerable<>接口，那么取泛型参数
                foreach (var item in type.GetInterfaces())
                {
                    if (item.IsGenericType && item.GetGenericTypeDefinition() == typeof(IEnumerable<>)) return item.GetGenericArguments()[0];
                }
            }
            return null;
        }

        /// <summary>
        /// 类型转换
        /// </summary>
        /// <param name="value">数值</param>
        /// <param name="conversionType"></param>
        /// <returns></returns>
        public virtual object ChangeType(object value, Type conversionType)
        {
            Type vtype = null;
            if (value != null) vtype = value.GetType();
            if (vtype == conversionType) return value;
            conversionType = Nullable.GetUnderlyingType(conversionType) ?? conversionType;
            if (conversionType.IsEnum)
            {
                if (vtype == typeof(string))
                    return Enum.Parse(conversionType, (string)value, true);
                else
                    return Enum.ToObject(conversionType, value);
            }
            // 字符串转为货币类型，处理一下
            if (vtype == typeof(string))
            {
                var str = (string)value;
                if (Type.GetTypeCode(conversionType) == TypeCode.Decimal)
                {
                    value = str.TrimStart(new Char[] { '$', '￥' });
                }
                else if (conversionType.As<Type>())
                {
                    return GetType((string)value, false);
                }
                // 字符串转为简单整型，如果长度比较小，满足32位整型要求，则先转为32位再改变类型
                var code = Type.GetTypeCode(conversionType);
                if (code >= TypeCode.Int16 && code <= TypeCode.UInt64 && str.Length <= 10) return Convert.ChangeType(value.ToInt(), conversionType);
            }

            if (value != null)
            {
                // 尝试基础类型转换
                switch (Type.GetTypeCode(conversionType))
                {
                    case TypeCode.Boolean:
                        return value.ToBoolean();
                    case TypeCode.DateTime:
                        return value.ToDateTime();
                    case TypeCode.Double:
                        return value.ToDouble();
                    case TypeCode.Int16:
                        return (Int16)value.ToInt();
                    case TypeCode.Int32:
                        return value.ToInt();
                    case TypeCode.UInt16:
                        return (UInt16)value.ToInt();
                    case TypeCode.UInt32:
                        return (UInt32)value.ToInt();
                    default:
                        break;
                }

                if (value is IConvertible) value = Convert.ChangeType(value, conversionType);
            }
            else
            {
                // 如果原始值是null，要转为值类型，则new一个空白的返回
                if (conversionType.IsValueType) value = CreateInstance(conversionType);
            }
            if (conversionType.IsAssignableFrom(vtype)) return value;
            return value;
        }

        /// <summary>
        /// 获取类型的友好名称
        /// </summary>
        /// <param name="type">指定类型</param>
        /// <param name="isfull">是否全名，包含命名空间</param>
        /// <returns></returns>
        public virtual string GetName(Type type, bool isfull) => isfull ? type.FullName : type.Name;
        #endregion

        #region 插件
        /// <summary>
        /// 是否子类
        /// </summary>
        /// <param name="type"></param>
        /// <param name="baseType"></param>
        /// <returns></returns>
        public bool As(Type type, Type baseType)
        {
            if (type == null) return false;
            // 如果基类是泛型定义
            if (baseType.IsGenericTypeDefinition && type.IsGenericType && !type.IsGenericTypeDefinition) type = type.GetGenericTypeDefinition();
            if (type == baseType) return true;
            if (baseType.IsAssignableFrom(type)) return true;
            // 接口
            if (baseType.IsInterface)
            {
                if (type.GetInterface(baseType.FullName) != null) return true;
                if (type.GetInterfaces().Any(e => e.IsGenericType && baseType.IsGenericTypeDefinition ? e.GetGenericTypeDefinition() == baseType : e == baseType)) return true;
            }
            // 判断是否子类时，支持只反射加载的程序集
            if (type.Assembly.ReflectionOnly)
            {
                // 反射加载时，需要特殊处理接口                
                while (type != typeof(object))
                {
                    if (type.FullName == baseType.FullName &&
                        type.AssemblyQualifiedName == baseType.AssemblyQualifiedName)
                        return true;
                    type = type.BaseType;
                }
            }
            return false;
        }

        /// <summary>
        /// 在指定程序集中查找指定基类的子类
        /// </summary>
        /// <param name="asm">指定程序集</param>
        /// <param name="baseType">基类或接口，为空时返回所有类型</param>
        /// <returns></returns>
        public virtual IEnumerable<Type> GetSubclasses(Assembly asm, Type baseType)
        {
            return AssemblyX.Create(asm).FindPlugins(baseType);
        }

        /// <summary>
        /// 在所有程序集中查找指定基类或接口的子类实现
        /// </summary>
        /// <param name="baseType">基类或接口</param>
        /// <param name="isLoadAssembly">是否加载为加载程序集</param>
        /// <returns></returns>
        public virtual IEnumerable<Type> GetAllSubclasses(Type baseType, bool isLoadAssembly)
        {
            return AssemblyX.FindAllPlugins(baseType, isLoadAssembly);
        }
        #endregion

        #region 辅助方法
        /// <summary>
        /// 获取类型，如果target是Type类型，则表示要反射的是静态成员
        /// </summary>
        /// <param name="target">目标对象</param>
        /// <returns></returns>
        protected virtual Type GetType(ref object target)
        {
            if (target == null) throw new ArgumentNullException("target");
            var type = target as Type;
            if (type == null)
                type = target.GetType();
            else
                target = null;
            return type;
        }
        #endregion
    }
}