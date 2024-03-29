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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
#if __MOBILE__
#elif __CORE__
#else
using System.Web;
using ZqUtils.Extensions;
#endif

namespace ZqUtils.Reflection
{
    /// <summary>
    /// 程序集辅助类。使用Create创建，保证每个程序集只有一个辅助类
    /// </summary>
    public class AssemblyX
    {
        #region 属性
        /// <summary>
        /// 程序集
        /// </summary>
        public Assembly Asm { get; }

        [NonSerialized]
        private List<string> hasLoaded = new List<string>();

        private string _Name;
        /// <summary>
        /// 名称
        /// </summary>
        public string Name => _Name ?? (_Name = "" + Asm.GetName().Name);

        private string _Version;
        /// <summary>
        /// 程序集版本
        /// </summary>
        public string Version => _Version ?? (_Version = "" + Asm.GetName().Version);

        private string _Title;
        /// <summary>
        /// 程序集标题
        /// </summary>
        public string Title => _Title ?? (_Title = "" + Asm.GetCustomAttributeValue<AssemblyTitleAttribute, string>());

        private string _FileVersion;
        /// <summary>
        /// 文件版本
        /// </summary>
        public string FileVersion => _FileVersion ?? (_FileVersion = "" + Asm.GetCustomAttributeValue<AssemblyFileVersionAttribute, string>());

        private DateTime _Compile;
        /// <summary>
        /// 编译时间
        /// </summary>
        public DateTime Compile
        {
            get
            {
                if (_Compile <= DateTime.MinValue && !hasLoaded.Contains("Compile"))
                {
                    hasLoaded.Add("Compile");
                    if (!string.IsNullOrEmpty(Version))
                    {
                        var ss = Version.Split(new Char[] { '.' });
                        var d = Convert.ToInt32(ss[2]);
                        var s = Convert.ToInt32(ss[3]);
                        var dt = new DateTime(2000, 1, 1);
                        dt = dt.AddDays(d).AddSeconds(s * 2);
                        _Compile = dt;
                    }
                }
                return _Compile;
            }
        }

        private Version _CompileVersion;
        /// <summary>
        /// 编译版本
        /// </summary>
        public Version CompileVersion
        {
            get
            {
                if (_CompileVersion == null)
                {
                    var ver = Asm.GetName().Version;
                    if (ver == null) ver = new Version(1, 0);
                    var dt = Compile;
                    ver = new Version(ver.Major, ver.Minor, dt.Year, dt.Month * 100 + dt.Day);
                    _CompileVersion = ver;
                }
                return _CompileVersion;
            }
        }

        private string _Company;
        /// <summary>
        /// 公司名称
        /// </summary>
        public string Company => _Company ?? (_Company = "" + Asm.GetCustomAttributeValue<AssemblyCompanyAttribute, string>());

        private string _Description;
        /// <summary>
        /// 说明
        /// </summary>
        public string Description => _Description ?? (_Description = "" + Asm.GetCustomAttributeValue<AssemblyDescriptionAttribute, string>());

        /// <summary>
        /// 获取包含清单的已加载文件的路径或 UNC 位置。
        /// </summary>
        public string Location
        {
            get
            {
                try
                {
#if !__IOS__ && !__CORE__
                    return Asm == null || Asm is _AssemblyBuilder || Asm.IsDynamic ? null : Asm.Location;
#else
                    return Asm == null || Asm.IsDynamic ? null : Asm.Location;
#endif
                }
                catch { return null; }
            }
        }
        #endregion

        #region 构造
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="asm"></param>
        private AssemblyX(Assembly asm) => Asm = asm;

        private static ConcurrentDictionary<Assembly, AssemblyX> cache = new ConcurrentDictionary<Assembly, AssemblyX>();

        /// <summary>
        /// 创建程序集辅助对象
        /// </summary>
        /// <param name="asm"></param>
        /// <returns></returns>
        public static AssemblyX Create(Assembly asm)
        {
            if (asm == null) return null;
            return cache.GetOrAdd(asm, key => new AssemblyX(key));
        }

        static AssemblyX()
        {
#if !__MOBILE__
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += (sender, args) =>
            {
                return Assembly.ReflectionOnlyLoad(args.Name);
            };
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                return OnResolve(args.Name);
            };
#endif
        }
        #endregion

        #region 扩展属性
        /// <summary>
        /// 类型集合，当前程序集的所有类型，包括私有和内嵌，非内嵌请直接调用Asm.GetTypes()
        /// </summary>
        public IEnumerable<Type> Types
        {
            get
            {
                Type[] ts = null;
                try
                {
                    ts = Asm.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    ts = ex.Types;
                }
                if (ts == null || ts.Length < 1) yield break;
                // 先遍历一次ts，避免取内嵌类型带来不必要的性能损耗
                foreach (var item in ts)
                {
                    if (item != null) yield return item;
                }
                var queue = new Queue<Type>(ts);
                while (queue.Count > 0)
                {
                    var item = queue.Dequeue();
                    if (item == null) continue;
                    var ts2 = item.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                    if (ts2 != null && ts2.Length > 0)
                    {
                        // 从下一个元素开始插入，让内嵌类紧挨着主类
                        //int k = i + 1;
                        foreach (var elm in ts2)
                        {
                            //if (!list.Contains(item)) list.Insert(k++, item);
                            // Insert将会导致大量的数组复制
                            queue.Enqueue(elm);
                            yield return elm;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 是否系统程序集
        /// </summary>
        public bool IsSystemAssembly => CheckSystem(Asm);

        private static bool CheckSystem(Assembly asm)
        {
            if (asm == null) return false;
            var name = asm.FullName;
            if (name.EndsWith("PublicKeyToken=b77a5c561934e089")) return true;
            if (name.EndsWith("PublicKeyToken=b03f5f7f11d50a3a")) return true;
            if (name.EndsWith("PublicKeyToken=89845dcd8080cc91")) return true;
            if (name.EndsWith("PublicKeyToken=31bf3856ad364e35")) return true;
            return false;
        }
        #endregion

        #region 静态属性
        /// <summary>
        /// 入口程序集
        /// </summary>
        public static AssemblyX Entry => Create(Assembly.GetEntryAssembly());
        #endregion

        #region 方法
        ConcurrentDictionary<string, Type> typeCache2 = new ConcurrentDictionary<string, Type>();

        /// <summary>
        /// 从程序集中查找指定名称的类型
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public Type GetType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName)) throw new ArgumentNullException("typeName");
            return typeCache2.GetOrAdd(typeName, GetTypeInternal);
        }

        /// <summary>
        /// 在程序集中查找类型
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        Type GetTypeInternal(string typeName)
        {
            var type = Asm.GetType(typeName);
            if (type != null) return type;
            // 如果没有包含圆点，说明其不是FullName
            if (!typeName.Contains("."))
            {
                // 遍历所有类型，包括内嵌类型
                foreach (var item in Types)
                {
                    if (item.Name == typeName) return item;
                }
            }
            return null;
        }
        #endregion

        #region 插件
        /// <summary>
        /// 查找插件
        /// </summary>
        /// <typeparam name="TPlugin"></typeparam>
        /// <returns></returns>
        internal List<Type> FindPlugins<TPlugin>() { return FindPlugins(typeof(TPlugin)); }

        private ConcurrentDictionary<Type, List<Type>> _plugins = new ConcurrentDictionary<Type, List<Type>>();

        /// <summary>
        /// 查找插件，带缓存
        /// </summary>
        /// <param name="baseType">类型</param>
        /// <returns></returns>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal List<Type> FindPlugins(Type baseType)
        {
            // 如果type是null，则返回所有类型
            if (_plugins.TryGetValue(baseType, out var list))
                return list;
            list = new List<Type>();
            foreach (var item in Types)
            {
                if (item.IsInterface || item.IsAbstract || item.IsGenericType)
                    continue;
                if (item != baseType && item.As(baseType)) list.Add(item);
            }
            if (list.Count <= 0)
                list = null;
            _plugins.TryAdd(baseType, list);
            return list;
        }

        /// <summary>
        /// 查找所有非系统程序集中的所有插件
        /// </summary>
        /// <remarks>
        /// 继承类所在的程序集会引用baseType所在的程序集，利用这一点可以做一定程度的性能优化。
        /// </remarks>
        /// <param name="baseType"></param>
        /// <param name="isLoadAssembly">是否从未加载程序集中获取类型。使用仅反射的方法检查目标类型，如果存在，则进行常规加载</param>
        /// <param name="excludeGlobalTypes">指示是否应检查来自所有引用程序集的类型。如果为 false，则检查来自所有引用程序集的类型。 否则，只检查来自非全局程序集缓存 (GAC) 引用的程序集的类型。</param>
        /// <returns></returns>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static IEnumerable<Type> FindAllPlugins(Type baseType, bool isLoadAssembly = false, bool excludeGlobalTypes = true)
        {
            var baseAssemblyName = baseType.Assembly.GetName().Name;
            // 如果基类所在程序集没有强命名，则搜索时跳过所有强命名程序集
            // 因为继承类程序集的强命名要求基类程序集必须强命名
            var signs = baseType.Assembly.GetName().GetPublicKey();
            var hasNotSign = signs == null || signs.Length <= 0;
            var list = new List<Type>();
            foreach (var item in GetAssemblies())
            {
                signs = item.Asm.GetName().GetPublicKey();
                if (hasNotSign && signs != null && signs.Length > 0)
                    continue;
                // 如果excludeGlobalTypes为true，则指检查来自非GAC引用的程序集
                if (excludeGlobalTypes && item.Asm.GlobalAssemblyCache)
                    continue;
                // 不搜索系统程序集，不搜索未引用基类所在程序集的程序集，优化性能
                if (item.IsSystemAssembly || !IsReferencedFrom(item.Asm, baseAssemblyName))
                    continue;
                var ts = item.FindPlugins(baseType);
                if (ts != null && ts.Count > 0)
                {
                    foreach (var elm in ts)
                    {
                        if (!list.Contains(elm))
                        {
                            list.Add(elm);
                            yield return elm;
                        }
                    }
                }
            }
            if (isLoadAssembly)
            {
                foreach (var item in ReflectionOnlyGetAssemblies())
                {
                    // 如果excludeGlobalTypes为true，则指检查来自非GAC引用的程序集
                    if (excludeGlobalTypes && item.Asm.GlobalAssemblyCache)
                        continue;
                    // 不搜索系统程序集，不搜索未引用基类所在程序集的程序集，优化性能
                    if (item.IsSystemAssembly || !IsReferencedFrom(item.Asm, baseAssemblyName))
                        continue;
                    var ts = item.FindPlugins(baseType);
                    if (ts != null && ts.Count > 0)
                    {
                        var asm2 = Assembly.LoadFile(item.Asm.Location);
                        ts = Create(asm2).FindPlugins(baseType);
                        if (ts != null && ts.Count > 0)
                        {
                            foreach (var elm in ts)
                            {
                                if (!list.Contains(elm))
                                {
                                    list.Add(elm);
                                    yield return elm;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// <paramref name="asm"/> 是否引用了 <paramref name="baseAsmName"/>
        /// </summary>
        /// <param name="asm">程序集</param>
        /// <param name="baseAsmName">被引用程序集全名</param>
        /// <returns></returns>
        private static bool IsReferencedFrom(Assembly asm, string baseAsmName)
        {
            if (asm.GetName().Name.EqualIgnoreCase(baseAsmName))
                return true;
            foreach (var item in asm.GetReferencedAssemblies())
            {
                if (item.Name.EqualIgnoreCase(baseAsmName))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 根据名称获取类型
        /// </summary>
        /// <param name="typeName">类型名</param>
        /// <param name="isLoadAssembly">是否从未加载程序集中获取类型。使用仅反射的方法检查目标类型，如果存在，则进行常规加载</param>
        /// <returns></returns>
        internal static Type GetType(string typeName, bool isLoadAssembly)
        {
            var type = Type.GetType(typeName);
            if (type != null)
                return type;
            // 加速基础类型识别，忽略大小写
            if (!typeName.Contains("."))
            {
                foreach (var item in Enum.GetNames(typeof(TypeCode)))
                {
                    if (typeName.EqualIgnoreCase(item))
                    {
                        type = Type.GetType("System." + item);
                        if (type != null)
                            return type;
                    }
                }
            }
            // 尝试本程序集
            var asms = new[]
            {
                Create(Assembly.GetExecutingAssembly()),
                Create(Assembly.GetCallingAssembly()),
                Create(Assembly.GetEntryAssembly())
            };
            var loads = new List<AssemblyX>();
            foreach (var asm in asms)
            {
                if (asm == null || loads.Contains(asm))
                    continue;
                loads.Add(asm);
                type = asm.GetType(typeName);
                if (type != null)
                    return type;
            }
            // 尝试所有程序集
            foreach (var asm in GetAssemblies())
            {
                if (loads.Contains(asm))
                    continue;
                loads.Add(asm);
                type = asm.GetType(typeName);
                if (type != null)
                    return type;
            }
            // 尝试加载只读程序集
            if (!isLoadAssembly)
                return null;
            foreach (var asm in ReflectionOnlyGetAssemblies())
            {
                type = asm.GetType(typeName);
                if (type != null)
                {
                    // 真实加载
                    var file = asm.Asm.Location;
                    try
                    {
                        type = null;
                        var asm2 = Assembly.LoadFile(file);
                        var type2 = Create(asm2).GetType(typeName);
                        if (type2 == null)
                            continue;
                        type = type2;
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    return type;
                }
            }
            return null;
        }
        #endregion

        #region 静态加载
        /// <summary>
        /// 获取指定程序域所有程序集
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public static IEnumerable<AssemblyX> GetAssemblies(AppDomain domain = null)
        {
            if (domain == null)
                domain = AppDomain.CurrentDomain;
            var asms = domain.GetAssemblies();
            if (asms == null || asms.Length < 1)
                return Enumerable.Empty<AssemblyX>();
            return from e in asms select Create(e);
        }

        private static ICollection<string> _AssemblyPaths;

        /// <summary>
        /// 程序集目录集合
        /// </summary>
        public static ICollection<string> AssemblyPaths
        {
            get
            {
                if (_AssemblyPaths == null)
                {
                    var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    var basedir = AppDomain.CurrentDomain.BaseDirectory;
                    set.Add(basedir);
#if !__MOBILE__ && !__CORE__
                    if (HttpRuntime.AppDomainId != null) set.Add(HttpRuntime.BinDirectory);
#else
                    if (Directory.Exists("bin".GetFullPath())) set.Add("bin".GetFullPath());
#endif                   
                    _AssemblyPaths = set;
                }
                return _AssemblyPaths;
            }
            set
            {
                _AssemblyPaths = value;
            }
        }

        /// <summary>
        /// 获取当前程序域所有只反射程序集的辅助类
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<AssemblyX> ReflectionOnlyGetAssemblies()
        {
            var loadeds = GetAssemblies().ToList();
            // 先返回已加载的只加载程序集
            var loadeds2 = AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies().Select(e => Create(e)).ToList();
            foreach (var item in loadeds2)
            {
                if (loadeds.Any(e => e.Location.EqualIgnoreCase(item.Location)))
                    continue;
                // 尽管目录不一样，但这两个可能是相同的程序集
                // 这里导致加载了不同目录的同一个程序集，然后导致对象容器频繁报错
                //if (loadeds.Any(e => e.Asm.FullName.EqualIgnoreCase(item.Asm.FullName))) continue;
                // 相同程序集不同版本，全名不想等
                if (loadeds.Any(e => e.Asm.GetName().Name.EqualIgnoreCase(item.Asm.GetName().Name)))
                    continue;
                yield return item;
            }
            foreach (var item in AssemblyPaths)
            {
                foreach (var asm in ReflectionOnlyLoad(item)) yield return asm;
            }
        }

        /// <summary>
        /// 只反射加载指定路径的所有程序集
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IEnumerable<AssemblyX> ReflectionOnlyLoad(string path)
        {
            if (!Directory.Exists(path))
                yield break;
            // 先返回已加载的只加载程序集
            var loadeds2 = AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies().Select(e => Create(e)).ToList();
            // 再去遍历目录
            var ss = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);
            if (ss == null || ss.Length < 1)
                yield break;
            var loadeds = GetAssemblies().ToList();
            var ver = new Version(Assembly.GetExecutingAssembly().ImageRuntimeVersion.TrimStart('v'));
            foreach (var item in ss)
            {
                // 仅尝试加载dll和exe，不加载vshost文件
                if (!item.EndsWithIgnoreCase(".dll", ".exe") || item.EndsWithIgnoreCase(".vshost.exe"))
                    continue;
                if (loadeds.Any(e => e.Location.EqualIgnoreCase(item)) ||
                    loadeds2.Any(e => e.Location.EqualIgnoreCase(item)))
                    continue;

#if !__MOBILE__ && !__CORE__
                var asm = ReflectionOnlyLoadFrom(item, ver);
                if (asm == null) continue;
#else
                var asm = Assembly.LoadFrom(item);
                if (asm == null) continue;
#endif

                // 不搜索系统程序集，优化性能
                if (CheckSystem(asm))
                    continue;
                // 尽管目录不一样，但这两个可能是相同的程序集
                // 这里导致加载了不同目录的同一个程序集，然后导致对象容器频繁报错
                //if (loadeds.Any(e => e.Asm.FullName.EqualIgnoreCase(asm.FullName)) ||
                //    loadeds2.Any(e => e.Asm.FullName.EqualIgnoreCase(asm.FullName))) continue;
                // 相同程序集不同版本，全名不想等
                if (loadeds.Any(e => e.Asm.GetName().Name.EqualIgnoreCase(asm.GetName().Name)) ||
                    loadeds2.Any(e => e.Asm.GetName().Name.EqualIgnoreCase(asm.GetName().Name)))
                    continue;
                var asmx = Create(asm);
                if (asmx != null) yield return asmx;
            }
        }

#if !__MOBILE__ && !__CORE__
        /// <summary>
        /// 只反射加载指定路径的所有程序集
        /// </summary>
        /// <param name="file"></param>
        /// <param name="ver"></param>
        /// <returns></returns>
        public static Assembly ReflectionOnlyLoadFrom(string file, Version ver = null)
        {
            // 仅加载.Net文件，并且小于等于当前版本
            if (!PEImage.CanLoad(file, ver, false))
                return null;
            try
            {
                return Assembly.ReflectionOnlyLoadFrom(file);
            }
            catch { return null; }
        }
#endif

        /// <summary>
        /// 获取当前应用程序的所有程序集，不包括系统程序集，仅限本目录
        /// </summary>
        /// <returns></returns>
        public static List<AssemblyX> GetMyAssemblies()
        {
            var list = new List<AssemblyX>();
            var hs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var cur = AppDomain.CurrentDomain.BaseDirectory;
            foreach (var asmx in GetAssemblies())
            {
                // 加载程序集列表很容易抛出异常，全部屏蔽
                try
                {
                    if (string.IsNullOrEmpty(asmx.FileVersion)) continue;
                    var file = asmx.Asm.CodeBase;
                    if (string.IsNullOrEmpty(file)) continue;
                    file = file.TrimStart("file:///");
                    file = file.Replace("/", "\\");
                    if (!file.StartsWithIgnoreCase(cur)) continue;
                    if (!hs.Contains(file))
                    {
                        hs.Add(file);
                        list.Add(asmx);
                    }
                }
                catch { }
            }
#if !__CORE__
            foreach (var asmx in ReflectionOnlyGetAssemblies())
            {
                // 加载程序集列表很容易抛出异常，全部屏蔽
                try
                {
                    if (string.IsNullOrEmpty(asmx.FileVersion)) continue;
                    var file = asmx.Asm.CodeBase;
                    if (string.IsNullOrEmpty(file)) continue;
                    file = file.TrimStart("file:///");
                    file = file.Replace("/", "\\");
                    if (!file.StartsWithIgnoreCase(cur)) continue;
                    if (!hs.Contains(file))
                    {
                        hs.Add(file);
                        list.Add(asmx);
                    }
                }
                catch { }
            }
#endif
            return list;
        }

        /// <summary>
        /// 在对程序集的解析失败时发生
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static Assembly OnResolve(string name)
        {
            foreach (var item in GetAssemblies())
            {
                if (item.Asm.FullName == name) return item.Asm;
            }
            foreach (var item in ReflectionOnlyGetAssemblies())
            {
                if (item.Asm.FullName == name)
                {
                    // 只反射程序集需要真实加载
                    try
                    {
                        var asm = Assembly.LoadFile(item.Asm.Location);
                        if (asm != null) return asm;
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }

            // 支持加载不同版本
            var p = name.IndexOf(", ");
            if (p > 0)
            {
                name = name.Substring(0, p);
                foreach (var item in GetAssemblies())
                {
                    if (item.Asm.GetName().Name == name) return item.Asm;
                }
                foreach (var item in ReflectionOnlyGetAssemblies())
                {
                    if (item.Asm.GetName().Name == name)
                    {
                        try
                        {
                            var asm = Assembly.LoadFile(item.Asm.Location);
                            if (asm != null) return asm;
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                }
            }
            return null;
        }
        #endregion

        #region 重载
        /// <summary>
        /// 已重载。
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Title))
                return Title;
            else
                return Name;
        }
        #endregion
    }
}