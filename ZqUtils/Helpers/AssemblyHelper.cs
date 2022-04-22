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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ZqUtils.Extensions;
/****************************
* [Author] 张强
* [Date] 2020-08-28
* [Describe] Assembly工具类
* **************************/
namespace ZqUtils.Helpers
{
    /// <summary>
    /// Assembly工具类
    /// </summary>
    public class AssemblyHelper
    {
        /// <summary>
        /// 根据指定路径和条件获取程序集
        /// </summary>
        /// <param name="path">程序集路径，默认：AppContext.BaseDirectory</param>
        /// <param name="filter">程序集筛选过滤器</param>
        /// <returns></returns>
        public static Assembly[] GetAssemblies(string path = null, Func<string, bool> filter = null)
        {
            var files = Directory
                            .GetFiles(path ?? AppDomain.CurrentDomain.BaseDirectory, "*.*")
                            .Where(x =>
                                x.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ||
                                x.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                            .Select(x => x
                                .Substring(Path.DirectorySeparatorChar.ToString())
                                .Replace(".dll", "")
                                .Replace(".exe", ""))
                            .Distinct();

            //判断筛选条件是否为空
            if (filter != null)
                files = files.Where(x => filter(x));

            //加载Assembly集
            var assemblies = files.Select(x => Assembly.Load(x));

            return assemblies.ToArray();
        }

        /// <summary>
        /// 获取程序集文件
        /// </summary>
        /// <param name="folderPath">目录路径</param>
        /// <param name="searchOption">检索模式</param>
        /// <returns></returns>
        public static IEnumerable<string> GetAssemblyFiles(string folderPath, SearchOption searchOption)
        {
            return Directory
               .EnumerateFiles(folderPath, "*.*", searchOption)
               .Where(s =>
                   s.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ||
                   s.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 根据指定路径和条件获取程序集中所有的类型集合
        /// </summary>
        /// <param name="path">程序集路径，默认：AppContext.BaseDirectory</param>
        /// <param name="condition">程序集筛选条件</param>
        /// <returns></returns>
        public static List<Type> GetTypesFromAssembly(string path = null, Func<string, bool> condition = null)
        {
            var types = new List<Type>();
            var assemblies = GetAssemblies(path, condition);
            if (assemblies?.Length > 0)
            {
                foreach (var assembly in assemblies)
                {
                    Type[] typeArray = null;
                    try
                    {
                        typeArray = assembly.GetTypes();
                    }
                    catch
                    {
                    }

                    if (typeArray?.Length > 0)
                        types.AddRange(typeArray);
                }
            }
            return types;
        }

        /// <summary>
        /// 获取程序集中的所有类型
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static IReadOnlyList<Type> GetAllTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types;
            }
        }
    }
}