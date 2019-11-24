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
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using Newtonsoft.Json;
using ZqUtils.Extensions;
/****************************
* [Author] 张强
* [Date] 2015-10-26
* [Describe] XML工具类
* **************************/
namespace ZqUtils.Helpers
{
    /// <summary>
    /// XML工具类
    /// </summary>
    public class XmlHelper
    {
        #region XML转换为Dictionary
        /// <summary>
        /// XML字符串转换为Dictionary
        /// </summary>
        /// <param name="xml">XML字符串</param>
        /// <param name="rootNode">根节点</param>
        /// <returns>Dictionary</returns>
        public static Dictionary<string, string> XmlToDictionary(string xml, string rootNode = "xml")
        {
            var dic = new Dictionary<string, string>();
            try
            {
                if (!xml.IsNull())
                {
                    var xmlDoc = new XmlDocument
                    {
                        //修复XML外部实体注入漏洞(XML External Entity Injection，简称 XXE)
                        XmlResolver = null
                    };
                    xmlDoc.LoadXml(xml);
                    var root = xmlDoc.SelectSingleNode(rootNode);
                    var xnl = root.ChildNodes;
                    foreach (XmlNode xnf in xnl)
                    {
                        dic.Add(xnf.Name, xnf.InnerText);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "xml字符串转换为Dictionary");
            }
            return dic;
        }

        /// <summary>
        /// Stream转换为Dictionary
        /// </summary>
        /// <param name="stream">XML字节流</param>
        /// <param name="rootNode">根节点</param>
        /// <returns>Dictionary</returns>
        public static Dictionary<string, string> XmlToDictionary(Stream stream, string rootNode = "xml")
        {
            var dic = new Dictionary<string, string>();
            try
            {
                if (stream?.Length > 0)
                {
                    var xmlDoc = new XmlDocument
                    {
                        //修复XML外部实体注入漏洞(XML External Entity Injection，简称 XXE)
                        XmlResolver = null
                    };
                    xmlDoc.Load(stream);
                    var root = xmlDoc.SelectSingleNode(rootNode);
                    var xnl = root.ChildNodes;
                    foreach (XmlNode xnf in xnl)
                    {
                        dic.Add(xnf.Name, xnf.InnerText);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "stream转换为Dictionary");
            }
            return dic;
        }
        #endregion

        #region XML转换为JSON字符串
        /// <summary>
        /// XML字符串转JSON字符串
        /// </summary>
        /// <param name="xml">XML字符串</param>
        /// <returns>JSON字符串</returns>
        public static string XmlToJson(string xml)
        {
            try
            {
                if (xml.IsNullOrEmpty())
                    return null;

                var doc = new XmlDocument()
                {
                    //修复XML外部实体注入漏洞(XML External Entity Injection，简称 XXE)
                    XmlResolver = null
                };
                doc.LoadXml(xml);

                return JsonConvert.SerializeXmlNode(doc);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "xml字符串转换为json异常");
            }
            return null;
        }
        #endregion

        #region XML序列化初始化
        /// <summary>
        /// XML序列化初始化
        /// </summary>
        /// <param name="stream">字节流</param>
        /// <param name="o">要序列化的对象</param>
        /// <param name="encoding">编码方式</param>
        private static void XmlSerializeInternal(Stream stream, object o, Encoding encoding)
        {
            if (o == null) throw new ArgumentNullException("o");
            var serializer = new XmlSerializer(o.GetType());
            var settings = new XmlWriterSettings
            {
                Indent = true,
                NewLineChars = "\r\n",
                Encoding = encoding ?? throw new ArgumentNullException("encoding"),
                IndentChars = "    "
            };
            using (var writer = XmlWriter.Create(stream, settings))
            {
                serializer.Serialize(writer, o);
                writer.Close();
            }
        }
        #endregion

        #region 将一个对象序列化为XML字符串
        /// <summary>
        /// 将一个对象序列化为XML字符串
        /// </summary>
        /// <param name="o">要序列化的对象</param>
        /// <param name="encoding">编码方式</param>
        /// <returns>序列化产生的XML字符串</returns>
        public static string XmlSerialize(object o, Encoding encoding)
        {
            using (var stream = new MemoryStream())
            {
                XmlSerializeInternal(stream, o, encoding);
                stream.Position = 0;
                using (var reader = new StreamReader(stream, encoding))
                {
                    return reader.ReadToEnd();
                }
            }
        }
        #endregion

        #region 将一个对象按XML序列化的方式写入到一个文件
        /// <summary>
        /// 将一个对象按XML序列化的方式写入到一个文件
        /// </summary>
        /// <param name="o">要序列化的对象</param>
        /// <param name="path">保存文件路径</param>
        /// <param name="encoding">编码方式</param>
        public static void XmlSerializeToFile(object o, string path, Encoding encoding)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            using (var file = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                XmlSerializeInternal(file, o, encoding);
            }
        }
        #endregion

        #region 从XML字符串中反序列化对象
        /// <summary>
        /// 从XML字符串中反序列化对象
        /// </summary>
        /// <typeparam name="T">结果对象类型</typeparam>
        /// <param name="s">包含对象的XML字符串</param>
        /// <param name="encoding">编码方式</param>
        /// <returns>反序列化得到的对象</returns>
        public static T XmlDeserialize<T>(string s, Encoding encoding)
        {
            if (string.IsNullOrEmpty(s)) throw new ArgumentNullException("s");
            if (encoding == null) throw new ArgumentNullException("encoding");
            var serializer = new XmlSerializer(typeof(T));
            using (var ms = new MemoryStream(encoding.GetBytes(s)))
            {
                using (var sr = new StreamReader(ms, encoding))
                {
                    return (T)serializer.Deserialize(sr);
                }
            }
        }
        #endregion

        #region 读入一个文件，并按XML的方式反序列化对象
        /// <summary>
        /// 读入一个文件，并按XML的方式反序列化对象。
        /// </summary>
        /// <typeparam name="T">结果对象类型</typeparam>
        /// <param name="path">文件路径</param>
        /// <param name="encoding">编码方式</param>
        /// <returns>反序列化得到的对象</returns>
        public static T XmlDeserializeFromFile<T>(string path, Encoding encoding)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (encoding == null) throw new ArgumentNullException("encoding");
            var xml = File.ReadAllText(path, encoding);
            return XmlDeserialize<T>(xml, encoding);
        }
        #endregion
    }
}
