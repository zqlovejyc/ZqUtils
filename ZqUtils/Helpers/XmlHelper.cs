using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using ZqUtils.Extensions;
/****************************
* [Author] 张强
* [Date] 2015-10-26
* [Describe] Xml工具类
* **************************/
namespace ZqUtils.Helpers
{
    /// <summary>
    /// Xml工具类
    /// </summary>
    public class XmlHelper
    {
        #region xml转换为Dictionary
        /// <summary>
        /// xml字符串转换为Dictionary
        /// </summary>
        /// <param name="xmlStr">xml字符串</param>
        /// <returns>Dictionary</returns>
        public static Dictionary<string, string> XmlToDictionary(string xmlStr)
        {
            var dic = new Dictionary<string, string>();
            try
            {
                if (!xmlStr.IsNull())
                {
                    var xmlDoc = new XmlDocument
                    {
                        //修复XML外部实体注入漏洞(XML External Entity Injection，简称 XXE)
                        XmlResolver = null
                    };
                    xmlDoc.LoadXml(xmlStr);
                    var root = xmlDoc.SelectSingleNode("xml");
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
        /// stream转换为Dictionary
        /// </summary>
        /// <param name="stream">字节流</param>
        /// <returns>Dictionary</returns>
        public static Dictionary<string, string> XmlToDictionary(Stream stream)
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
                    var root = xmlDoc.SelectSingleNode("xml");
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

        #region xml序列化初始化
        /// <summary>
        /// xml序列化初始化
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
