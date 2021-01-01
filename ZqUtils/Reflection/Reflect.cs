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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using ZqUtils.Extensions;

namespace ZqUtils.Reflection
{
    /// <summary>
    /// 反射工具类
    /// </summary>
    public static class Reflect
    {
        #region 静态
        /// <summary>
        /// 当前反射提供者
        /// </summary>
        public static IReflect Provider { get; set; }

        /// <summary>
        /// 静态构造函数
        /// 如果需要使用快速反射，启用下面这一行
        /// Provider = new EmitReflect();该EmitReflect暂不可用，有bug
        /// </summary>
        static Reflect() => Provider = new DefaultReflect();
        #endregion

        #region 反射获取
        /// <summary>
        /// 根据名称获取类型。可搜索当前目录DLL，自动加载
        /// </summary>
        /// <param name="typeName">类型名</param>
        /// <param name="isLoadAssembly">是否从未加载程序集中获取类型。使用仅反射的方法检查目标类型，如果存在，则进行常规加载</param>
        /// <returns></returns>
        public static Type GetTypeEx(this string typeName, bool isLoadAssembly = true)
        {
            if (string.IsNullOrEmpty(typeName)) return null;
            var type = Type.GetType(typeName);
            if (type != null) return type;
            return Provider.GetType(typeName, isLoadAssembly);
        }

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
        public static MethodInfo GetMethodEx(this Type type, string name, params Type[] paramTypes)
        {
            if (name.IsNullOrEmpty()) return null;
            // 如果其中一个类型参数为空，得用别的办法
            if (paramTypes.Length > 0 && paramTypes.Any(e => e == null)) return Provider.GetMethods(type, name, paramTypes.Length).FirstOrDefault();
            return Provider.GetMethod(type, name, paramTypes);
        }

        /// <summary>
        /// 获取指定名称的方法集合，支持指定参数个数来匹配过滤
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="paramCount">参数个数，-1表示不过滤参数个数</param>
        /// <returns></returns>
        public static MethodInfo[] GetMethodsEx(this Type type, string name, int paramCount = -1)
        {
            if (name.IsNullOrEmpty()) return null;
            return Provider.GetMethods(type, name, paramCount);
        }

        /// <summary>
        /// 获取属性。搜索私有、静态、基类，优先返回大小写精确匹配成员
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="name">名称</param>
        /// <param name="ignoreCase">忽略大小写</param>
        /// <returns></returns>
        public static PropertyInfo GetPropertyEx(this Type type, string name, bool ignoreCase = false)
        {
            if (string.IsNullOrEmpty(name)) return null;
            return Provider.GetProperty(type, name, ignoreCase);
        }

        /// <summary>
        /// 获取字段。搜索私有、静态、基类，优先返回大小写精确匹配成员
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="name">名称</param>
        /// <param name="ignoreCase">忽略大小写</param>
        /// <returns></returns>
        public static FieldInfo GetFieldEx(this Type type, string name, bool ignoreCase = false)
        {
            if (string.IsNullOrEmpty(name)) return null;
            return Provider.GetField(type, name, ignoreCase);
        }

        /// <summary>
        /// 获取成员。搜索私有、静态、基类，优先返回大小写精确匹配成员
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="name">名称</param>
        /// <param name="ignoreCase">忽略大小写</param>
        /// <returns></returns>
        public static MemberInfo GetMemberEx(this Type type, string name, bool ignoreCase = false)
        {
            if (string.IsNullOrEmpty(name)) return null;
            return Provider.GetMember(type, name, ignoreCase);
        }

        /// <summary>
        /// 获取用于序列化的字段
        /// </summary>
        /// <remarks>过滤<seealso cref="T:NonSerializedAttribute"/>特性的字段</remarks>
        /// <param name="type"></param>
        /// <param name="baseFirst"></param>
        /// <returns></returns>
        public static IList<FieldInfo> GetFields(this Type type, bool baseFirst) => Provider.GetFields(type, baseFirst);

        /// <summary>
        /// 获取用于序列化的属性
        /// </summary>
        /// <remarks>过滤<seealso cref="T:XmlIgnoreAttribute"/>特性的属性和索引器</remarks>
        /// <param name="type"></param>
        /// <param name="baseFirst"></param>
        /// <returns></returns>
        public static IList<PropertyInfo> GetProperties(this Type type, bool baseFirst) => Provider.GetProperties(type, baseFirst);
        #endregion

        #region 反射调用
        /// <summary>
        /// 创建对象实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fullName">命名空间.类型名</param>
        /// <param name="assemblyName">程序集</param>
        /// <returns>T</returns>
        public static T CreateInstance<T>(string fullName, string assemblyName)
        {
            string path = fullName + "," + assemblyName;//命名空间.类型名,程序集
            Type o = Type.GetType(path);//加载类型
            object obj = Activator.CreateInstance(o, true);//根据类型创建实例
            return (T)obj;//类型转换并返回
        }

        /// <summary>
        /// 创建对象实例
        /// </summary>
        /// <typeparam name="T">要创建对象的类型</typeparam>
        /// <param name="assemblyName">类型所在程序集名称</param>
        /// <param name="nameSpace">类型所在命名空间</param>
        /// <param name="className">类型名</param>
        /// <returns>T</returns>
        public static T CreateInstance<T>(string assemblyName, string nameSpace, string className)
        {
            try
            {
                string fullName = nameSpace + "." + className;//命名空间.类型名
                                                              //此为第一种写法
#if NETSTANDARD1_6 || NETSTANDARD2_0
                //加载程序集，创建程序集里面的 命名空间.类型名 实例s
                object ect = Assembly.Load(new AssemblyName(assemblyName)).CreateInstance(fullName);
#else
                //加载程序集，创建程序集里面的 命名空间.类型名 实例
                object ect = Assembly.Load(assemblyName).CreateInstance(fullName);
#endif
                return (T)ect;//类型转换并返回
            }
            catch
            {
                //发生异常，返回类型的默认值
                return default(T);
            }
        }

        /// <summary>
        /// 反射创建指定类型的实例
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="parameters">参数数组</param>
        /// <returns></returns>
        [DebuggerHidden]
        public static object CreateInstance(this Type type, params object[] parameters)
        {
            if (type == null) throw new ArgumentNullException("type");
            return Provider.CreateInstance(type, parameters);
        }

        /// <summary>
        /// 反射调用指定对象的方法。target为类型时调用其静态方法
        /// </summary>
        /// <param name="target">要调用其方法的对象，如果要调用静态方法，则target是类型</param>
        /// <param name="name">方法名</param>
        /// <param name="parameters">方法参数</param>
        /// <returns></returns>
        public static object Invoke(this object target, string name, params object[] parameters)
        {
            if (target == null) throw new ArgumentNullException("target");
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            if (TryInvoke(target, name, out var value, parameters)) return value;        
            var type = GetType(ref target);
            throw new Exception(string.Format("类{0}中找不到名为{1}的方法！", type, name));
        }

        /// <summary>
        /// 反射调用指定对象的方法
        /// </summary>
        /// <param name="target">要调用其方法的对象，如果要调用静态方法，则target是类型</param>
        /// <param name="name">方法名</param>
        /// <param name="value">数值</param>
        /// <param name="parameters">方法参数</param>
        /// <remarks>反射调用是否成功</remarks>
        public static bool TryInvoke(this object target, string name, out object value, params object[] parameters)
        {
            value = null;
            if (string.IsNullOrEmpty(name)) return false;
            var type = GetType(ref target);
            // 参数类型数组
            var ps = parameters.Select(e => e?.GetType()).ToArray();
            // 如果参数数组出现null，则无法精确匹配，可按参数个数进行匹配
            var method = ps.Any(e => e == null) ? GetMethodEx(type, name) : GetMethodEx(type, name, ps);
            if (method == null) method = GetMethodsEx(type, name, ps.Length > 0 ? ps.Length : -1).FirstOrDefault();
            if (method == null) return false;
            value = Invoke(target, method, parameters);
            return true;
        }

        /// <summary>
        /// 反射调用指定对象的方法
        /// </summary>
        /// <param name="target">要调用其方法的对象，如果要调用静态方法，则target是类型</param>
        /// <param name="method">方法</param>
        /// <param name="parameters">方法参数</param>
        /// <returns></returns>
        [DebuggerHidden]
        public static object Invoke(this object target, MethodBase method, params object[] parameters)
        {
            //if (target == null) throw new ArgumentNullException("target");
            if (method == null) throw new ArgumentNullException("method");
            if (!method.IsStatic && target == null) throw new ArgumentNullException("target");
            return Provider.Invoke(target, method, parameters);
        }

        /// <summary>
        /// 反射调用指定对象的方法
        /// </summary>
        /// <param name="target">要调用其方法的对象，如果要调用静态方法，则target是类型</param>
        /// <param name="method">方法</param>
        /// <param name="parameters">方法参数字典</param>
        /// <returns></returns>
        [DebuggerHidden]
        public static object InvokeWithParams(this object target, MethodBase method, IDictionary parameters)
        {
            //if (target == null) throw new ArgumentNullException("target");
            if (method == null) throw new ArgumentNullException("method");
            if (!method.IsStatic && target == null) throw new ArgumentNullException("target");
            return Provider.InvokeWithParams(target, method, parameters);
        }

        /// <summary>
        /// 获取目标对象指定名称的属性/字段值
        /// </summary>
        /// <param name="target">目标对象</param>
        /// <param name="name">名称</param>
        /// <param name="throwOnError">出错时是否抛出异常</param>
        /// <returns></returns>
        [DebuggerHidden]
        public static object GetValue(this object target, string name, bool throwOnError = true)
        {
            if (target == null) throw new ArgumentNullException("target");
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            if (TryGetValue(target, name, out var value)) return value;
            if (!throwOnError) return null;
            var type = GetType(ref target);
            throw new ArgumentException("类[" + type.FullName + "]中不存在[" + name + "]属性或字段。");
        }

        /// <summary>
        /// 获取目标对象指定名称的属性/字段值
        /// </summary>
        /// <param name="target">目标对象</param>
        /// <param name="name">名称</param>
        /// <param name="value">数值</param>
        /// <returns>是否成功获取数值</returns>
        public static bool TryGetValue(this object target, string name, out object value)
        {
            value = null;
            if (string.IsNullOrEmpty(name)) return false;
            var type = GetType(ref target);
            var mi = type.GetMemberEx(name, true);
            if (mi == null) return false;
            value = target.GetValue(mi);
            return true;
        }

        /// <summary>
        /// 获取目标对象的成员值
        /// </summary>
        /// <param name="target">目标对象</param>
        /// <param name="member">成员</param>
        /// <returns></returns>
        [DebuggerHidden]
        public static object GetValue(this object target, MemberInfo member)
        {
            // 有可能跟普通的 PropertyInfo.GetValue(object target) 搞混了
            if (member == null)
            {
                member = target as MemberInfo;
                target = null;
            }
            if (member is PropertyInfo)
                return Provider.GetValue(target, member as PropertyInfo);
            else if (member is FieldInfo)
                return Provider.GetValue(target, member as FieldInfo);
            else
                throw new ArgumentOutOfRangeException("member");
        }

        /// <summary>
        /// 设置目标对象指定名称的属性/字段值，若不存在返回false
        /// </summary>
        /// <param name="target">目标对象</param>
        /// <param name="name">名称</param>
        /// <param name="value">数值</param>
        /// <remarks>反射调用是否成功</remarks>
        [DebuggerHidden]
        public static bool SetValue(this object target, string name, object value)
        {
            if (string.IsNullOrEmpty(name)) return false;
            var type = GetType(ref target);
            var mi = type.GetMemberEx(name, true);
            if (mi == null) return false;
            target.SetValue(mi, value);
            //throw new ArgumentException("类[" + type.FullName + "]中不存在[" + name + "]属性或字段。");
            return true;
        }

        /// <summary>
        /// 设置目标对象的成员值
        /// </summary>
        /// <param name="target">目标对象</param>
        /// <param name="member">成员</param>
        /// <param name="value">数值</param>
        [DebuggerHidden]
        public static void SetValue(this object target, MemberInfo member, object value)
        {
            if (member is PropertyInfo)
                Provider.SetValue(target, member as PropertyInfo, value);
            else if (member is FieldInfo)
                Provider.SetValue(target, member as FieldInfo, value);
            else
                throw new ArgumentOutOfRangeException("member");
        }

        /// <summary>
        /// 从源对象拷贝数据到目标对象
        /// </summary>
        /// <param name="target">目标对象</param>
        /// <param name="src">源对象</param>
        /// <param name="deep">递归深度拷贝，直接拷贝成员值而不是引用</param>
        /// <param name="excludes">要忽略的成员</param>
        public static void Copy(this object target, object src, bool deep = false, params string[] excludes) => Provider.Copy(target, src, deep, excludes);

        /// <summary>
        /// 从源字典拷贝数据到目标对象
        /// </summary>
        /// <param name="target">目标对象</param>
        /// <param name="dic">源字典</param>
        /// <param name="deep">递归深度拷贝，直接拷贝成员值而不是引用</param>
        public static void Copy(this object target, IDictionary<string, object> dic, bool deep = false) => Provider.Copy(target, dic, deep);
        #endregion

        #region 类型辅助
        /// <summary>
        /// 获取一个类型的元素类型
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public static Type GetElementTypeEx(this Type type) => Provider.GetElementType(type);

        /// <summary>
        /// 类型转换
        /// </summary>
        /// <param name="value">数值</param>
        /// <param name="conversionType"></param>
        /// <returns></returns>
        public static object ChangeType(this object value, Type conversionType) => Provider.ChangeType(value, conversionType);

        /// <summary>
        /// 类型转换
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="value">数值</param>
        /// <returns></returns>
        public static TResult ChangeType<TResult>(this object value)
        {
            if (value is TResult) return (TResult)value;
            return (TResult)ChangeType(value, typeof(TResult));
        }

        /// <summary>
        /// 获取类型的友好名称
        /// </summary>
        /// <param name="type">指定类型</param>
        /// <param name="isfull">是否全名，包含命名空间</param>
        /// <returns></returns>
        public static string GetName(this Type type, bool isfull = false) => Provider.GetName(type, isfull);

        /// <summary>
        /// 从参数数组中获取类型数组
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        internal static Type[] GetTypeArray(this object[] args)
        {
            if (args == null) return Type.EmptyTypes;
            var typeArray = new Type[args.Length];
            for (var i = 0; i < typeArray.Length; i++)
            {
                if (args[i] == null)
                    typeArray[i] = typeof(object);
                else
                    typeArray[i] = args[i].GetType();
            }
            return typeArray;
        }

        /// <summary>
        /// 获取成员的类型，字段和属性是它们的类型，方法是返回类型，类型是自身
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public static Type GetMemberType(this MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Constructor:
                    return (member as ConstructorInfo).DeclaringType;
                case MemberTypes.Field:
                    return (member as FieldInfo).FieldType;
                case MemberTypes.Method:
                    return (member as MethodInfo).ReturnType;
                case MemberTypes.Property:
                    return (member as PropertyInfo).PropertyType;
                case MemberTypes.TypeInfo:
                case MemberTypes.NestedType:
                    return member as Type;
                default:
                    return null;
            }
        }

        /// <summary>
        /// 获取类型代码
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static TypeCode GetTypeCode(this Type type) => Type.GetTypeCode(type);

        /// <summary>
        /// 是否整数
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsInt(this Type type)
        {
            switch (type.GetTypeCode())
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

        /// <summary>
        /// 是否泛型列表
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsList(this Type type) => type != null && type.IsGenericType && type.As(typeof(IList<>));

        /// <summary>
        /// 是否泛型字典
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsDictionary(this Type type) => type != null && type.IsGenericType && type.As(typeof(IDictionary<,>));
        #endregion

        #region 插件
        /// <summary>
        /// 是否能够转为指定基类
        /// </summary>
        /// <param name="type"></param>
        /// <param name="baseType"></param>
        /// <returns></returns>
        public static bool As(this Type type, Type baseType) => Provider.As(type, baseType);

        /// <summary>
        /// 是否能够转为指定基类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool As<T>(this Type type) => Provider.As(type, typeof(T));

        /// <summary>
        /// 在指定程序集中查找指定基类的子类
        /// </summary>
        /// <param name="asm">指定程序集</param>
        /// <param name="baseType">基类或接口</param>
        /// <returns></returns>
        public static IEnumerable<Type> GetSubclasses(this Assembly asm, Type baseType) => Provider.GetSubclasses(asm, baseType);

        /// <summary>
        /// 在所有程序集中查找指定基类或接口的子类实现
        /// </summary>
        /// <param name="baseType">基类或接口</param>
        /// <param name="isLoadAssembly">是否加载为加载程序集</param>
        /// <returns></returns>
        public static IEnumerable<Type> GetAllSubclasses(this Type baseType, bool isLoadAssembly = false) => Provider.GetAllSubclasses(baseType, isLoadAssembly);
        #endregion

        #region 辅助方法
        /// <summary>
        /// 获取类型，如果target是Type类型，则表示要反射的是静态成员
        /// </summary>
        /// <param name="target">目标对象</param>
        /// <returns></returns>
        static Type GetType(ref object target)
        {
            if (target == null) throw new ArgumentNullException("target");
            var type = target as Type;
            if (type == null)
                type = target.GetType();
            else
                target = null;
            return type;
        }

        /// <summary>
        /// 判断某个类型是否可空类型
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        static bool IsNullable(Type type)
        {
            if (type.IsGenericType && !type.IsGenericTypeDefinition &&
                object.ReferenceEquals(type.GetGenericTypeDefinition(), typeof(Nullable<>))) return true;
            return false;
        }

        /// <summary>
        /// 把一个方法转为泛型委托，便于快速反射调用
        /// </summary>
        /// <typeparam name="TFunc"></typeparam>
        /// <param name="method"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static TFunc As<TFunc>(this MethodInfo method, object target = null)
        {
            if (method == null) return default(TFunc);
            if (target == null)
                return (TFunc)(object)Delegate.CreateDelegate(typeof(TFunc), method, true);
            else
                return (TFunc)(object)Delegate.CreateDelegate(typeof(TFunc), target, method, true);
        }
        #endregion
    }
}