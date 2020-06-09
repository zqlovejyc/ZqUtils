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

using System.Data;
using System.IO;
/****************************
* [Author] 张强
* [Date] 2018-05-15
* [Describe] DataSet扩展类
* **************************/
namespace ZqUtils.Extensions
{
    /// <summary>
    /// DataSet扩展类
    /// </summary>
    public static class DataSetExtensions
    {
        #region DataSet转Xml
        /// <summary>
        /// DataSet转Xml
        /// </summary>
        /// <param name="this">DataSet数据源</param>
        /// <returns>string</returns>
        public static string ToXml(this DataSet @this)
        {
            var result = string.Empty;
            if (@this?.Tables.Count > 0)
            {
                using (var writer = new StringWriter())
                {
                    @this.WriteXml(writer);
                    result = writer.ToString();
                }
            }
            return result;
        }
        #endregion
    }
}
