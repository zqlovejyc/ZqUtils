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

using System.Dynamic;
using System.Xml.Linq;

namespace ZqUtils.Reflection
{
    /// <summary>
    /// 动态Xml
    /// </summary>
    /// <example>
    /// 使用示例：
    /// <code>
    ///     dynamic xml = new DynamicXml("Test");
    ///     xml.Name = "NewLife";
    ///     xml.Sign = "学无先后达者为师！";
    ///     xml.Detail = new DynamicXml();
    ///     xml.Detail.Name = "新生命开发团队";
    ///     xml.Detail.CreateTime = new DateTime(2002, 12, 31);
    ///     var node = xml.Node as XElement;
    ///     var str = node.ToString();
    ///     Console.WriteLine(str);
    /// </code>   
    /// </example>
    public class DynamicXml : DynamicObject
    {
        /// <summary>
        /// 节点
        /// </summary>
        public XElement Node { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public DynamicXml() { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="node"></param>
        public DynamicXml(XElement node)
        {
            Node = node;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name"></param>
        public DynamicXml(string name)
        {
            Node = new XElement(name);
        }

        /// <summary>
        /// 设置
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var setNode = Node.Element(binder.Name);
            if (setNode != null)
                setNode.SetValue(value);
            else
            {
                if (value.GetType() == typeof(DynamicXml))
                    Node.Add(new XElement(binder.Name));
                else
                    Node.Add(new XElement(binder.Name, value));
            }
            return true;
        }

        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;
            var getNode = Node.Element(binder.Name);
            if (getNode == null) return false;
            result = new DynamicXml(getNode);
            return true;
        }
    }
}