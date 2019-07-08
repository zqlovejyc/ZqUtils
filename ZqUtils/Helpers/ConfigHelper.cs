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
using System.Xml;
using System.Configuration;
using System.Collections.Specialized;
using System.Collections.Generic;
using ZqUtils.Reflection;
using ZqUtils.Extensions;
/****************************
* [Author] 张强
* [Date] 2018-06-07
* [Describe] Config工具类
* **************************/
namespace ZqUtils.Helpers
{
    /// <summary>
    /// Config工具类
    /// </summary>
    public class ConfigHelper
    {
        #region AppSettings
        /// <summary>
        /// AppSettings配置
        /// </summary>
        public static NameValueCollection AppSettings => ConfigurationManager.AppSettings;
        #endregion

        #region GetAppSettings
        /// <summary>
        /// 根据Key取Value值
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="defaultValue">默认值</param>
        public static T GetAppSettings<T>(string key, T defaultValue = default(T))
        {
            var value = AppSettings[key]?.ToString().Trim();
            if (!string.IsNullOrEmpty(value))
            {
                return value.ToObject<T>();
            }
            return defaultValue;
        }
        #endregion

        #region SetAppSettings
        /// <summary>
        /// 根据Key修改Value
        /// </summary>
        /// <param name="key">要修改的Key</param>
        /// <param name="value">要修改的值</param>
        /// <param name="path">配置文件路径</param>
        public static void SetAppSettings(string key, string value, string path = "~/XmlConfig/system.config")
        {
            path = path.GetFullPath();
            var xDoc = new XmlDocument();
            xDoc.Load(path);
            XmlNode xNode;
            XmlElement xElem1;
            XmlElement xElem2;
            xNode = xDoc.SelectSingleNode("//appSettings");
            xElem1 = (XmlElement)xNode.SelectSingleNode("//add[@key='" + key + "']");
            if (xElem1 != null)
            {
                xElem1.SetAttribute("value", value);
            }
            else
            {
                xElem2 = xDoc.CreateElement("add");
                xElem2.SetAttribute("key", key);
                xElem2.SetAttribute("value", value);
                xNode.AppendChild(xElem2);
            }
            xDoc.Save(path);
        }
        #endregion

        #region GetConnectionString
        /// <summary>
        /// 根据Key获取ConnectionString值
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns></returns>
        public static string GetConnectionString(string key, string defaultValue = null) => ConfigurationManager.ConnectionStrings[key]?.ConnectionString?.Trim() ?? defaultValue;
        #endregion

        #region Contain
        /// <summary>
        /// 是否包含指定项的设置
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns></returns>
        public static bool Contain(string name)
        {
            try
            {
                var nvs = AppSettings;
                if (nvs == null || nvs.Count < 1) return false;
                return Array.IndexOf(nvs.AllKeys, name) >= 0;
            }
            catch (ConfigurationErrorsException)
            {
                return false;
            }
        }
        #endregion

        #region GetMutilConfig
        /// <summary>依次尝试获取一批设置项，直到找到第一个为止</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="defaultValue"></param>
        /// <param name="names"></param>
        /// <returns></returns>
        public static T GetMutilConfig<T>(T defaultValue, params string[] names)
        {
            if (TryGetMutilConfig(out T value, names)) return value;
            return defaultValue;
        }

        /// <summary>
        /// 依次尝试获取一批设置项，直到找到第一个为止
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">数值</param>
        /// <param name="names"></param>
        /// <returns></returns>
        public static bool TryGetMutilConfig<T>(out T value, params string[] names)
        {
            value = default(T);
            try
            {
                var nvs = AppSettings;
                if (nvs == null || nvs.Count < 1) return false;
                for (var i = 0; i < names.Length; i++)
                {
                    if (TryGetConfig(names[i], out value)) return true;
                }
                return false;
            }
            catch (ConfigurationErrorsException)
            {
                return false;
            }
        }
        #endregion

        #region GetConfig
        /// <summary>取得指定名称的设置项，并转为指定类型</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">名称</param>
        /// <returns></returns>
        public static T GetConfig<T>(string name)
        {
            try
            {
                var nvs = AppSettings;
                if (nvs == null || nvs.Count < 1) return default(T);
                return GetConfig(name, default(T));
            }
            catch (ConfigurationErrorsException)
            {
                return default(T);
            }
        }

        /// <summary>取得指定名称的设置项，并转为指定类型。如果设置不存在，则返回默认值</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">名称</param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T GetConfig<T>(string name, T defaultValue)
        {
            if (TryGetConfig(name, out T value)) return value;
            return defaultValue;
        }

        /// <summary>尝试获取指定名称的设置项</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">名称</param>
        /// <param name="value">数值</param>
        /// <returns></returns>
        public static bool TryGetConfig<T>(string name, out T value)
        {
            if (TryGetConfig(name, typeof(T), out object v))
            {
                value = (T)v;
                return true;
            }
            value = default(T);
            return false;
        }

        /// <summary>尝试获取指定名称的设置项</summary>
        /// <param name="name">名称</param>
        /// <param name="type">类型</param>
        /// <param name="value">数值</param>
        /// <returns></returns>
        public static bool TryGetConfig(string name, Type type, out object value)
        {
            value = null;
            try
            {
                var nvs = AppSettings;
                if (nvs == null || nvs.Count < 1) return false;
                var str = nvs[name];
                if (string.IsNullOrEmpty(str)) return false;
                var code = Type.GetTypeCode(type);
                if (code == TypeCode.String)
                    value = str;
                else if (code == TypeCode.Int32)
                    value = Convert.ToInt32(str);
                else if (code == TypeCode.Boolean)
                {
                    var b = false;
                    if (str.EqualIgnoreCase("1", bool.TrueString))
                        value = true;
                    else if (str.EqualIgnoreCase("0", bool.FalseString))
                        value = false;
                    else if (bool.TryParse(str.ToLower(), out b))
                        value = b;
                }
                else
                    value = str.ChangeType(type);
                return true;
            }
            catch (ConfigurationErrorsException)
            {
                return false;
            }
        }

        /// <summary>
        /// 根据指定前缀，获取设置项。其中key不包含前缀
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public static IDictionary<string, string> GetConfigByPrefix(string prefix)
        {
            try
            {
                var nvs = AppSettings;
                if (nvs == null || nvs.Count < 1) return null;
                var nv = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (string item in nvs)
                {
                    if (item.Length > prefix.Length && item.StartsWithIgnoreCase(prefix)) nv.Add(item.Substring(prefix.Length), nvs[item]);
                }
                return nv.Count > 0 ? nv : null;
            }
            catch (ConfigurationErrorsException)
            {
                return null;
            }
        }

        /// <summary>
        /// 取得指定名称的设置项，并分割为指定类型数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">名称</param>
        /// <param name="split"></param>
        /// <returns></returns>
        public static T[] GetConfigSplit<T>(string name, string split)
        {
            try
            {
                var nvs = AppSettings;
                if (nvs == null || nvs.Count < 1) return new T[0];
                return GetConfigSplit<T>(name, split, new T[0]);
            }
            catch (ConfigurationErrorsException)
            {
                return new T[0];
            }
        }

        /// <summary>
        /// 取得指定名称的设置项，并分割为指定类型数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">名称</param>
        /// <param name="split"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T[] GetConfigSplit<T>(string name, string split, T[] defaultValue)
        {
            try
            {
                var nvs = AppSettings;
                if (nvs == null || nvs.Count < 1) return defaultValue;
                var str = GetConfig<string>(name);
                if (string.IsNullOrEmpty(str)) return defaultValue;
                var sps = string.IsNullOrEmpty(split) ? new string[] { ",", ";" } : new string[] { split };
                var ss = str.Split(sps, StringSplitOptions.RemoveEmptyEntries);
                if (ss == null || ss.Length < 1) return defaultValue;
                var arr = new T[ss.Length];
                for (var i = 0; i < ss.Length; i++)
                {
                    str = ss[i].Trim();
                    if (string.IsNullOrEmpty(str)) continue;
                    arr[i] = str.ChangeType<T>();
                }
                return arr;
            }
            catch (ConfigurationErrorsException)
            {
                return defaultValue;
            }
        }
        #endregion

        #region SetConfig
        /// <summary>设置配置文件参数</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">名称</param>
        /// <param name="defaultValue"></param>
        public static void SetConfig<T>(string name, T defaultValue)
        {
            // 小心空引用
            SetConfig(name, "" + defaultValue);
        }

        /// <summary>
        /// 设置配置文件参数
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="value">数值</param>
        public static void SetConfig(string name, string value)
        {
            var nvs = AppSettings;
            if (nvs == null || nvs.Count < 1) return;
            nvs[name] = value;
        }
        #endregion

        #region AppSettingsKeyExists
        /// <summary>
        /// 判断appSettings中是否有此项
        /// </summary>
        /// <param name="strKey"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        private static bool AppSettingsKeyExists(string strKey, Configuration config)
        {
            foreach (var str in config.AppSettings.Settings.AllKeys)
            {
                if (str == strKey) return true;
            }
            return false;
        }
        #endregion

        #region UpdateConfig
        /// <summary>
        /// 设置配置文件参数
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="value">数值</param>
        public static void UpdateConfig(string name, string value)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (config != null && AppSettingsKeyExists(name, config))
            {
                config.AppSettings.Settings[name].Value = value;
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
        }
        #endregion
    }
}
