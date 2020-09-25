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
using System;
using System.Text;
using ZqUtils.Extensions;
/****************************
* [Author] 张强
* [Date] 2020-09-24
* [Describe] 雪花算法生成ID工具类
* **************************/
namespace ZqUtils.Helpers
{
    /// <summary>
    /// 雪花算法生成ID工具类
    /// <para>
    /// Twitter_Snowflake<br/>
    /// SnowFlake的结构如下(每部分用-分开)<br/>
    /// 0 - 0000000000 0000000000 0000000000 0000000000 0 - 00000 - 00000 - 000000000000<br/>
    /// 1位标识，由于long基本类型在Java中是带符号的，最高位是符号位，正数是0，负数是1，所以id一般是正数，最高位是0<br/>
    /// 41位时间截(毫秒级)，注意，41位时间截不是存储当前时间的时间截，而是存储时间截的差值（当前时间截 - 开始时间截)得到的值），<br/>
    /// 41位的时间截，可以使用69年，年T = (1L &lt;&lt; 41) / (1000L * 60 * 60 * 24 * 365) = 69<br/>
    /// 这里的的开始时间截，一般是我们的id生成器开始使用的时间，由我们程序来指定的（如下下面程序IdWorker类的startTime属性）。<br/>
    /// 10位的数据机器位，可以部署在1024个节点，包括5位datacenterId和5位workerId<br/>
    /// 12位序列，毫秒内的计数，12位的计数顺序号支持每个节点每毫秒(同一机器，同一时间截)产生4096个ID序号<br/>
    /// 总共加起来刚好64位，为一个Long型。<br/>
    /// SnowFlake的优点是，整体上按照时间自增排序，并且整个分布式系统内不会产生ID碰撞(由数据中心ID和机器ID作区分)，<br/>
    /// 并且效率较高，经测试，SnowFlake单机每秒都能够产生出极限4,096,000个ID来<br/>
    /// 引用：https://www.cnblogs.com/sunyuliang/p/12161416.html
    /// </para>
    /// </summary>
    public class SnowflakeHelper
    {
        // 开始时间截 (new DateTime(2020, 1, 1).ToUniversalTime() - Jan1st1970).TotalMilliseconds
        private const long twepoch = 1577808000000L;

        // 机器id所占的位数
        private const int workerIdBits = 5;

        // 数据标识id所占的位数
        private const int datacenterIdBits = 5;

        // 支持的最大机器id，结果是31 (这个移位算法可以很快的计算出几位二进制数所能表示的最大十进制数) 
        private const long maxWorkerId = -1L ^ (-1L << workerIdBits);

        // 支持的最大数据标识id，结果是31 
        private const long maxDatacenterId = -1L ^ (-1L << datacenterIdBits);

        // 序列在id中占的位数 
        private const int sequenceBits = 12;

        // 数据标识id向左移17位(12+5) 
        private const int datacenterIdShift = sequenceBits + workerIdBits;

        // 机器ID向左移12位 
        private const int workerIdShift = sequenceBits;

        // 时间截向左移22位(5+5+12) 
        private const int timestampLeftShift = sequenceBits + workerIdBits + datacenterIdBits;

        // 生成序列的掩码，这里为4095 (0b111111111111=0xfff=4095) 
        private const long sequenceMask = -1L ^ (-1L << sequenceBits);

        // 1970.1.1
        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// 数据中心ID(0~31) 
        /// </summary>
        public long DatacenterId { get; private set; }

        /// <summary>
        /// 工作机器ID(0~31) 
        /// </summary>
        public long WorkerId { get; private set; }

        /// <summary>
        /// 毫秒内序列(0~4095) 
        /// </summary>
        public long Sequence { get; private set; }

        /// <summary>
        /// 上次生成ID的时间截 
        /// </summary>
        public long LastTimestamp { get; private set; }

        /// <summary>
        /// 静态实例
        /// </summary>
        public static SnowflakeHelper Instance { get; set; }

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static SnowflakeHelper()
        {
            var jObject = ConfigHelper.GetAppSettings<JObject>("Snowflake");
            if (jObject.IsNotNull())
            {
                var datacenterId = jObject.Value<long>("DatacenterId");
                var workId = jObject.Value<long>("WorkId");

                Instance = new SnowflakeHelper(datacenterId, workId);
            }
            else
            {
                Instance = new SnowflakeHelper(1, 1);
            }
        }

        /// <summary>
        /// 雪花ID
        /// </summary>
        /// <param name="datacenterId">数据中心ID</param>
        /// <param name="workerId">工作机器ID</param>
        public SnowflakeHelper(long datacenterId, long workerId)
        {
            if (datacenterId > maxDatacenterId || datacenterId < 0)
            {
                throw new Exception(string.Format("datacenter Id can't be greater than {0} or less than 0", maxDatacenterId));
            }
            if (workerId > maxWorkerId || workerId < 0)
            {
                throw new Exception(string.Format("worker Id can't be greater than {0} or less than 0", maxWorkerId));
            }
            this.WorkerId = workerId;
            this.DatacenterId = datacenterId;
            this.Sequence = 0L;
            this.LastTimestamp = -1L;
        }

        /// <summary>
        /// 获得下一个ID
        /// </summary>
        /// <returns></returns>
        public long NextId()
        {
            lock (this)
            {
                long timestamp = GetCurrentTimestamp();
                if (timestamp > LastTimestamp) //时间戳改变，毫秒内序列重置
                {
                    Sequence = 0L;
                }
                else if (timestamp == LastTimestamp) //如果是同一时间生成的，则进行毫秒内序列
                {
                    Sequence = (Sequence + 1) & sequenceMask;
                    if (Sequence == 0) //毫秒内序列溢出
                    {
                        timestamp = GetNextTimestamp(LastTimestamp); //阻塞到下一个毫秒,获得新的时间戳
                    }
                }
                else   //当前时间小于上一次ID生成的时间戳，证明系统时钟被回拨，此时需要做回拨处理
                {
                    Sequence = (Sequence + 1) & sequenceMask;
                    if (Sequence > 0)
                    {
                        timestamp = LastTimestamp;     //停留在最后一次时间戳上，等待系统时间追上后即完全度过了时钟回拨问题。
                    }
                    else   //毫秒内序列溢出
                    {
                        timestamp = LastTimestamp + 1;   //直接进位到下一个毫秒                          
                    }
                    //throw new Exception(string.Format("Clock moved backwards.  Refusing to generate id for {0} milliseconds", lastTimestamp - timestamp));
                }

                LastTimestamp = timestamp;       //上次生成ID的时间截

                //移位并通过或运算拼到一起组成64位的ID
                var id = ((timestamp - twepoch) << timestampLeftShift)
                        | (DatacenterId << datacenterIdShift)
                        | (WorkerId << workerIdShift)
                        | Sequence;
                return id;
            }
        }

        /// <summary>
        /// 生成唯一标识ID
        /// </summary>
        /// <returns></returns>
        public static long NewId() => Instance.NextId();

        /// <summary>
        /// 解析雪花ID
        /// </summary>
        /// <returns></returns>
        public static string AnalyzeId(long Id)
        {
            var sb = new StringBuilder();

            var timestamp = (Id >> timestampLeftShift);
            var time = Jan1st1970.AddMilliseconds(timestamp + twepoch);
            sb.Append(time.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss:fff"));

            var datacenterId = (Id ^ (timestamp << timestampLeftShift)) >> datacenterIdShift;
            sb.Append("_" + datacenterId);

            var workerId = (Id ^ ((timestamp << timestampLeftShift) | (datacenterId << datacenterIdShift))) >> workerIdShift;
            sb.Append("_" + workerId);

            var sequence = Id & sequenceMask;
            sb.Append("_" + sequence);

            return sb.ToString();
        }

        /// <summary>
        /// 阻塞到下一个毫秒，直到获得新的时间戳
        /// </summary>
        /// <param name="lastTimestamp">上次生成ID的时间截</param>
        /// <returns>当前时间戳</returns>
        private static long GetNextTimestamp(long lastTimestamp)
        {
            long timestamp = GetCurrentTimestamp();
            while (timestamp <= lastTimestamp)
            {
                timestamp = GetCurrentTimestamp();
            }
            return timestamp;
        }

        /// <summary>
        /// 获取当前时间戳
        /// </summary>
        /// <returns></returns>
        private static long GetCurrentTimestamp()
        {
            return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
        }
    }
}