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

using Newtonsoft.Json.Linq;
using Snowflake.Net;
using ZqUtils.Extensions;
/****************************
* [Author] 张强
* [Date] 2020-09-24
* [Describe] 雪花算法生成Id工具类
* **************************/
namespace ZqUtils.Helpers
{
    /// <summary>
    /// 雪花算法生成Id工具类
    /// </summary>
    public class SnowflakeHelper
    {
        /// <summary>
        /// Id生产者
        /// </summary>
        public static IdWorker Worker { get; set; }

        /// <summary>
        /// 唯一标识Id
        /// </summary>
        public static long NextId => Worker.NextId();

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static SnowflakeHelper()
        {
            var jObject = ConfigHelper.GetAppSettings<JObject>("Snowflake");
            if (jObject.IsNotNull())
            {
                var workId = jObject.Value<long>("WorkId");
                var datacenterId = jObject.Value<long>("DatacenterId");
                var sequence = jObject.Value<long>("Sequence");

                Worker = new IdWorker(workId, datacenterId, sequence);
            }
            else
            {
                Worker = new IdWorker(1, 1);
            }
        }
    }
}