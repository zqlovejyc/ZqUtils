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
using System.Linq;
using System.Reflection;
/****************************
* [Author] 张强
* [Date] 2019-04-24
* [Describe] Array扩展类
* **************************/
namespace ZqUtils.Extensions
{
    /// <summary>
    /// Assembly扩展类
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// 通过 继承的接口名 查找 对应的 实体类型集合
        /// </summary>
        /// <param name="assemblies"></param>
        /// <param name="implementedInterfaceName">继承的接口名称</param>
        /// <returns></returns>
        public static IEnumerable<Type> GetTypes(this IEnumerable<Assembly> assemblies, string implementedInterfaceName)
        {
            List<Type> rc = new List<Type>();
            implementedInterfaceName = implementedInterfaceName?.Trim();
            if (!string.IsNullOrWhiteSpace(implementedInterfaceName) && assemblies?.Count() > 0)
            {
                foreach (var assembly in assemblies)
                {
                    var types = assembly?.DefinedTypes?.Where(it => it.ImplementedInterfaces.Any(item => item.Name == implementedInterfaceName)).Select(it => it.UnderlyingSystemType);
                    if (types?.Count() > 0)
                        rc.AddRange(types.ToList());
                }
            }

            return rc;
        }

        /// <summary>
        /// 通过 拥有的特性名 和 在该特性中的公共属性名和属性值 去 查找 对应的 实体类型集合
        /// </summary>
        /// <param name="types"></param>
        /// <param name="attributeType"></param>
        /// <param name="pubPropertyNameInAttribute"></param>
        /// <param name="pubPropertyValueInAttribute"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetTypes(this IEnumerable<Type> types, Type attributeType, string pubPropertyNameInAttribute = "", object pubPropertyValueInAttribute = null)
        {
            List<Type> rc = new List<Type>();

            pubPropertyNameInAttribute = pubPropertyNameInAttribute?.Trim();

            if (attributeType != null && types?.Count() > 0)
            {
                foreach (var type in types)
                {
                    var list = type.GetCustomAttributes(attributeType, false);
                    if (list?.Length > 0)
                    {
                        if (string.IsNullOrWhiteSpace(pubPropertyNameInAttribute))
                            rc.Add(type);
                        else
                        {
                            foreach (var item in list)
                            {
                                var pro = item.GetType().GetProperty(pubPropertyNameInAttribute);
                                if (pro != null)
                                {
                                    var value = pro.GetValue(item, null);
                                    if ((value == null && pubPropertyValueInAttribute == null) || (value != null && value.Equals(pubPropertyValueInAttribute)))
                                        rc.Add(type);
                                }
                            }

                        }
                    }
                }
            }

            return rc;
        }
    }
}