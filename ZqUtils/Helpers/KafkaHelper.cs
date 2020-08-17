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

using Confluent.Kafka;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZqUtils.Extensions;
/****************************
* [Author] 张强
* [Date] 2020-08-15
* [Describe] Kafka工具类
* **************************/
namespace ZqUtils.Helpers
{
    /// <summary>
    /// Kafka工具类
    /// </summary>
    public class KafkaHelper
    {
        /// <summary>
        /// 私有配置
        /// </summary>
        private readonly KafkaConfig _config;

        /// <summary>
        /// 生产者连接配置
        /// </summary>
        public ProducerConfig ProducerConfig { get; set; }

        /// <summary>
        /// 消费者连接配置
        /// </summary>
        public ConsumerConfig ConsumerConfig { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public KafkaHelper() { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="config"></param>
        public KafkaHelper(KafkaConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// 获取并初始化生产者
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="delegate"></param>
        /// <returns></returns>
        public IProducer<TKey, TValue> GetOrInitProducer<TKey, TValue>(Action<ProducerBuilder<TKey, TValue>> @delegate = null)
        {
            ProducerBuilder<TKey, TValue> builder;

            if (this.ProducerConfig.IsNotNull())
                builder = new ProducerBuilder<TKey, TValue>(this.ProducerConfig);
            else
                builder = new ProducerBuilder<TKey, TValue>(_config.AsKafkaConfig());

            @delegate?.Invoke(builder);

            return builder?.Build();
        }

        /// <summary>
        /// 获取并初始化消费者
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="delegate"></param>
        /// <returns></returns>
        public IConsumer<TKey, TValue> GetOrInitConsumer<TKey, TValue>(Action<ConsumerBuilder<TKey, TValue>> @delegate = null)
        {
            ConsumerBuilder<TKey, TValue> builder;

            if (this.ConsumerConfig.IsNotNull())
                builder = new ConsumerBuilder<TKey, TValue>(this.ConsumerConfig);
            else
                builder = new ConsumerBuilder<TKey, TValue>(_config.AsKafkaConfig());

            @delegate?.Invoke(builder);

            return builder?.Build();
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="topic"></param>
        /// <param name="message"></param>
        /// <param name="deliveryHandler"></param>
        /// <param name="delegate"></param>
        /// <returns></returns>
        public bool Publish<TKey, TValue>(
            string topic,
            Message<TKey, TValue> message,
            Action<DeliveryReport<TKey, TValue>> deliveryHandler = null,
            Action<ProducerBuilder<TKey, TValue>> @delegate = null)
        {
            var producer = this.GetOrInitProducer(@delegate);

            if (producer.IsNotNull())
            {
                producer.Produce(topic, message, deliveryHandler);

                return true;
            }

            return false;
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="topic"></param>
        /// <param name="messages"></param>
        /// <param name="deliveryHandler"></param>
        /// <param name="delegate"></param>
        /// <returns></returns>
        public bool Publish<TKey, TValue>(
            string topic,
            IEnumerable<Message<TKey, TValue>> messages,
            Action<DeliveryReport<TKey, TValue>> deliveryHandler = null,
            Action<ProducerBuilder<TKey, TValue>> @delegate = null)
        {
            var producer = this.GetOrInitProducer(@delegate);

            if (producer.IsNotNull())
            {
                foreach (var message in messages)
                {
                    producer.Produce(topic, message, deliveryHandler);
                }

                producer.Flush(TimeSpan.FromSeconds(10));

                return true;
            }

            return false;
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="topic"></param>
        /// <param name="message"></param>
        /// <param name="delegate"></param>
        /// <returns></returns>
        public async Task<DeliveryResult<TKey, TValue>> PublishAsync<TKey, TValue>(
            string topic,
            Message<TKey, TValue> message,
            Action<ProducerBuilder<TKey, TValue>> @delegate = null)
        {
            var producer = this.GetOrInitProducer(@delegate);
            if (producer.IsNotNull())
                return await producer.ProduceAsync(topic, message);

            return null;
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="topic"></param>
        /// <param name="messages"></param>
        /// <param name="delegate"></param>
        /// <returns></returns>
        public async Task<IEnumerable<DeliveryResult<TKey, TValue>>> PublishAsync<TKey, TValue>(
            string topic,
            IEnumerable<Message<TKey, TValue>> messages,
            Action<ProducerBuilder<TKey, TValue>> @delegate = null)
        {
            var producer = this.GetOrInitProducer(@delegate);
            if (producer.IsNotNull())
            {
                var result = new List<DeliveryResult<TKey, TValue>>();
                foreach (var message in messages)
                {
                    var res = await producer.ProduceAsync(topic, message);
                    result.Add(res);
                }

                return result;
            }

            return null;
        }

        /// <summary>
        /// 订阅消息主题
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="topic"></param>
        /// <param name="delegate"></param>
        /// <returns></returns>
        public IConsumer<TKey, TValue> Subscribe<TKey, TValue>(
            string topic,
            Action<ConsumerBuilder<TKey, TValue>> @delegate = null)
        {
            var consumer = this.GetOrInitConsumer(@delegate);
            if (consumer.IsNotNull())
            {
                consumer.Subscribe(topic);
                return consumer;
            }

            return null;
        }

        /// <summary>
        /// 批量订阅消息主题
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="topics"></param>
        /// <param name="delegate"></param>
        /// <returns></returns>
        public IConsumer<TKey, TValue> Subscribe<TKey, TValue>(
            IEnumerable<string> topics,
            Action<ConsumerBuilder<TKey, TValue>> @delegate = null)
        {
            var consumer = this.GetOrInitConsumer(@delegate);
            if (consumer.IsNotNull())
            {
                consumer.Subscribe(topics);
                return consumer;
            }

            return null;
        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="topic"></param>
        /// <param name="receiveHandler"></param>
        /// <param name="commit"></param>
        /// <param name="delegate"></param>
        public void Subscribe<TKey, TValue>(
            string topic,
            Action<ConsumeResult<TKey, TValue>> receiveHandler,
            bool commit = false,
            Action<ConsumerBuilder<TKey, TValue>> @delegate = null)
        {
            var consumer = this.Subscribe(topic, @delegate);

            if (consumer.IsNotNull())
            {
                while (true)
                {

                    var res = consumer.Consume();

                    if (res.IsPartitionEOF || res.Message.IsNull() || res.Message.Value.IsNull())
                        continue;

                    receiveHandler?.Invoke(res);

                    if (commit)
                        consumer.Commit(res);
                }
            }
        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="topics"></param>
        /// <param name="receiveHandler"></param>
        /// <param name="commit"></param>
        /// <param name="delegate"></param>
        public void Subscribe<TKey, TValue>(
            IEnumerable<string> topics,
            Action<ConsumeResult<TKey, TValue>> receiveHandler,
            bool commit = false,
            Action<ConsumerBuilder<TKey, TValue>> @delegate = null)
        {
            var consumer = this.Subscribe(topics, @delegate);

            if (consumer.IsNotNull())
            {
                while (true)
                {

                    var res = consumer.Consume();

                    if (res.IsPartitionEOF || res.Message.IsNull() || res.Message.Value.IsNull())
                        continue;

                    receiveHandler?.Invoke(res);

                    if (commit)
                        consumer.Commit(res);
                }
            }
        }
    }

    /// <summary>
    /// Kafka配置
    /// </summary>
    public class KafkaConfig
    {
        private IEnumerable<KeyValuePair<string, string>> _kafkaConfig;

        /// <summary>
        /// MainConfig
        /// </summary>
        public ConcurrentDictionary<string, string> MainConfig { get; set; }

        /// <summary>
        /// KafkaConfig
        /// </summary>
        public KafkaConfig()
        {
            MainConfig = new ConcurrentDictionary<string, string>();
        }

        /// <summary>
        /// KafkaConfig
        /// </summary>
        public KafkaConfig(Dictionary<string, string> config)
        {
            MainConfig = new ConcurrentDictionary<string, string>();
            if (config.IsNotNullOrEmpty())
            {
                MainConfig.AddRangeIfNotContains(config.ToArray());
            }
        }

        /// <summary>
        /// The `bootstrap.servers` item config of <see cref="MainConfig" />.
        /// <para>
        /// Initial list of brokers as a CSV list of broker host or host:port.
        /// </para>
        /// </summary>
        public string Servers { get; set; }

        /// <summary>
        /// AsKafkaConfig
        /// </summary>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<string, string>> AsKafkaConfig()
        {
            if (_kafkaConfig == null)
            {
                if (Servers.IsNullOrWhiteSpace() && !MainConfig.Keys.Contains("bootstrap.servers"))
                {
                    throw new ArgumentNullException(nameof(Servers));
                }

                if (!MainConfig.Keys.Contains("bootstrap.servers"))
                    MainConfig["bootstrap.servers"] = Servers;

                if (!MainConfig.Keys.Contains("queue.buffering.max.ms"))
                    MainConfig["queue.buffering.max.ms"] = "10";

                if (!MainConfig.Keys.Contains("enable.auto.commit"))
                    MainConfig["enable.auto.commit"] = "false";

                if (!MainConfig.Keys.Contains("log.connection.close"))
                    MainConfig["log.connection.close"] = "false";

                if (!MainConfig.Keys.Contains("request.timeout.ms"))
                    MainConfig["request.timeout.ms"] = "3000";

                if (!MainConfig.Keys.Contains("message.timeout.ms"))
                    MainConfig["message.timeout.ms"] = "5000";

                _kafkaConfig = MainConfig.AsEnumerable();
            }

            return _kafkaConfig;
        }
    }
}