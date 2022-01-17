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

using FreeRedis;
using Newtonsoft.Json;
using System;
using System.Linq;
/****************************
* [Author] 张强
* [Date] 2020-12-05
* [Describe] FreeRedis工具类
* **************************/
namespace ZqUtils.Helpers
{
    /// <summary>
    /// FreeRedis工具类
    /// </summary>
    public class FreeRedisHelper : RedisClient
    {
        /// <summary>
        /// 静态实例
        /// </summary>
        public static FreeRedisHelper Default;

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static FreeRedisHelper()
        {
            var connectionStrings = ConfigHelper.GetAppSettings<string[]>("RedisConnectionStrings");

            if (connectionStrings.Length == 1)
                //普通模式
                Default = new FreeRedisHelper(connectionStrings[0]);
            else
                //集群模式
                Default = new FreeRedisHelper(connectionStrings.Select(v => ConnectionStringBuilder.Parse(v)).ToArray());

            //配置序列化和反序列化
            Default.Serialize = obj => JsonConvert.SerializeObject(obj);
            Default.Deserialize = (json, type) => JsonConvert.DeserializeObject(json, type);
        }

        /// <summary>
        /// 集群模式
        /// </summary>
        /// <param name="clusterConnectionStrings"></param>
        public FreeRedisHelper(ConnectionStringBuilder[] clusterConnectionStrings)
            : base(clusterConnectionStrings)
        {
        }

        /// <summary>
        /// 主从模式
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="slaveConnectionStrings"></param>
        public FreeRedisHelper(ConnectionStringBuilder connectionString, params ConnectionStringBuilder[] slaveConnectionStrings)
            : base(connectionString, slaveConnectionStrings)
        {
        }

        /// <summary>
        /// Norman RedisClient
        /// </summary>
        /// <param name="connectionStrings"></param>
        /// <param name="redirectRule"></param>
        public FreeRedisHelper(ConnectionStringBuilder[] connectionStrings, Func<string, string> redirectRule)
            : base(connectionStrings, redirectRule)
        {
        }

        /// <summary>
        /// 哨兵高可用模式
        /// </summary>
        /// <param name="sentinelConnectionString"></param>
        /// <param name="sentinels"></param>
        /// <param name="rw_splitting"></param>
        public FreeRedisHelper(ConnectionStringBuilder sentinelConnectionString, string[] sentinels, bool rw_splitting)
            : base(sentinelConnectionString, sentinels, rw_splitting)
        {
        }
    }
}