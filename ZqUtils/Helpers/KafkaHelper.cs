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
        #region 私有字段
        /// <summary>
        /// 私有配置
        /// </summary>
        private readonly KafkaConfig _config;
        #endregion

        #region 公有属性
        /// <summary>
        /// 生产者连接配置
        /// </summary>
        public ProducerConfig ProducerConfig { get; set; }

        /// <summary>
        /// 消费者连接配置
        /// </summary>
        public ConsumerConfig ConsumerConfig { get; set; }

        /// <summary>
        /// 静态单例
        /// </summary>
        public static KafkaHelper Instance => SingletonHelper<KafkaHelper>.GetInstance();
        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        public KafkaHelper()
        {
            this.ProducerConfig = ConfigHelper.GetAppSettings<ProducerConfig>("KafkaProducerConfig");
            this.ConsumerConfig = ConfigHelper.GetAppSettings<ConsumerConfig>("KafkaConsumerConfig");
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="producerConfig">生产者连接配置</param>
        /// <param name="consumerConfig">消费者连接配置</param>
        public KafkaHelper(ProducerConfig producerConfig, ConsumerConfig consumerConfig)
        {
            this.ProducerConfig = producerConfig;
            this.ConsumerConfig = consumerConfig;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="config"></param>
        public KafkaHelper(KafkaConfig config)
        {
            _config = config;
        }
        #endregion

        #region 初始化生产者/消费者
        /// <summary>
        /// 获取并初始化生产者
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="delegate">生产者初始化委托</param>
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
        /// <param name="delegate">消费者初始化委托</param>
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
        #endregion

        #region 发布消息
        /// <summary>
        /// 发布消息
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="topic">消息主题</param>
        /// <param name="message">消息内容</param>
        /// <param name="deliveryHandler">消息发送委托</param>
        /// <param name="delegate">生产者初始化委托</param>
        public void Publish<TKey, TValue>(
            string topic,
            Message<TKey, TValue> message,
            Action<DeliveryReport<TKey, TValue>> deliveryHandler = null,
            Action<ProducerBuilder<TKey, TValue>> @delegate = null)
        {
            var producer = this.GetOrInitProducer(@delegate);

            if (producer.IsNotNull() && message.IsNotNull())
            {
                producer.Produce(topic, message, deliveryHandler);
            }
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="topic">消息主题</param>
        /// <param name="messages">消息内容</param>
        /// <param name="deliveryHandler">消息发送委托</param>
        /// <param name="delegate">生产者初始化委托</param>
        public void Publish<TKey, TValue>(
            string topic,
            IEnumerable<Message<TKey, TValue>> messages,
            Action<DeliveryReport<TKey, TValue>> deliveryHandler = null,
            Action<ProducerBuilder<TKey, TValue>> @delegate = null)
        {
            var producer = this.GetOrInitProducer(@delegate);

            if (producer.IsNotNull() && messages.IsNotNullOrEmpty())
            {
                foreach (var message in messages)
                {
                    producer.Produce(topic, message, deliveryHandler);
                }
            }
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="topic">消息主题</param>
        /// <param name="message">消息内容</param>
        /// <param name="delegate">生产者初始化委托</param>
        /// <returns></returns>
        public async Task<DeliveryResult<TKey, TValue>> PublishAsync<TKey, TValue>(
            string topic,
            Message<TKey, TValue> message,
            Action<ProducerBuilder<TKey, TValue>> @delegate = null)
        {
            var producer = this.GetOrInitProducer(@delegate);

            if (producer.IsNotNull() && message.IsNotNull())
                return await producer.ProduceAsync(topic, message);

            return null;
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="topic">消息主题</param>
        /// <param name="messages">消息内容</param>
        /// <param name="delegate">生产者初始化委托</param>
        /// <returns></returns>
        public async Task<IEnumerable<DeliveryResult<TKey, TValue>>> PublishAsync<TKey, TValue>(
            string topic,
            IEnumerable<Message<TKey, TValue>> messages,
            Action<ProducerBuilder<TKey, TValue>> @delegate = null)
        {
            var producer = this.GetOrInitProducer(@delegate);

            if (producer.IsNotNull() && messages.IsNotNullOrEmpty())
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
        #endregion

        #region 订阅消息
        /// <summary>
        /// 订阅消息主题
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="topic">消息主题</param>
        /// <param name="delegate">消费者初始化委托</param>
        /// <returns></returns>
        public IConsumer<TKey, TValue> Subscribe<TKey, TValue>(
            string topic,
            Action<ConsumerBuilder<TKey, TValue>> @delegate = null)
        {
            var consumer = this.GetOrInitConsumer(@delegate);

            if (consumer.IsNotNull() && topic.IsNotNullOrWhiteSpace())
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
        /// <param name="topics">消息主题</param>
        /// <param name="delegate">消费者初始化委托</param>
        /// <returns></returns>
        public IConsumer<TKey, TValue> Subscribe<TKey, TValue>(
            IEnumerable<string> topics,
            Action<ConsumerBuilder<TKey, TValue>> @delegate = null)
        {
            var consumer = this.GetOrInitConsumer(@delegate);

            if (consumer.IsNotNull() && topics.IsNotNullOrEmpty())
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
        /// <param name="topic">消息主题</param>
        /// <param name="receiveHandler">消息接收处理委托</param>
        /// <param name="exceptionHandler">异常处理委托</param>
        /// <param name="delegate">消费者初始化委托</param>
        /// <param name="commit">是否手动提交</param>
        /// <param name="retryCount">异常重试次数</param>
        /// <param name="durableFailOrExceptionMessage">是否持久化失败或者异常消息</param>
        public void Subscribe<TKey, TValue>(
            string topic,
            Func<ConsumeResult<TKey, TValue>, bool> receiveHandler,
            Action<ConsumeResult<TKey, TValue>, int, Exception> exceptionHandler = null,
            Action<ConsumerBuilder<TKey, TValue>> @delegate = null,
            bool commit = false,
            int retryCount = 5,
            bool durableFailOrExceptionMessage = true)
        {
            var consumer = this.Subscribe(topic, @delegate);

            if (consumer.IsNotNull())
            {
                while (true)
                {
                    var message = consumer.Consume();
                    var numberOfRetries = 0;
                    Exception exception = null;
                    bool? result = false;
                    while (numberOfRetries <= retryCount)
                    {
                        try
                        {
                            if (message.IsPartitionEOF || message.Message.IsNull() || message.Message.Value.IsNull())
                                continue;

                            result = receiveHandler?.Invoke(message);

                            //异常置空
                            exception = null;

                            break;
                        }
                        catch (Exception ex)
                        {
                            exception = ex;
                            exceptionHandler?.Invoke(message, numberOfRetries, ex);
                            numberOfRetries++;
                        }
                    }

                    //是否手动提交
                    if (commit)
                        consumer.Commit(message);

                    //是否持久化失败或者异常消息
                    if (durableFailOrExceptionMessage && (!(result == true) || exception != null))
                    {
                        this.Publish($"{topic}.{(exception != null ? "error" : "fail")}", new Message<string, string>
                        {
                            Key = message.Message.Key?.ToString(),
                            Value = new FailOrExceptionMessage
                            {
                                Body = message.Message.Value.ToJson(),
                                Topics = new[] { topic },
                                RetryCount = exception == null ? numberOfRetries : numberOfRetries - 1,
                                Exception = exception,
                                ExceptionMsg = exception?.Message
                            }.ToJson()
                        });
                    }
                }
            }
        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="topics">消息主题</param>
        /// <param name="receiveHandler">消息接收处理委托</param>
        /// <param name="exceptionHandler">异常处理委托</param>
        /// <param name="delegate">消费者初始化委托</param>
        /// <param name="commit">是否手动提交</param>
        /// <param name="retryCount">异常重试次数</param>
        /// <param name="durableFailOrExceptionMessage">是否持久化失败或者异常消息</param>
        public void Subscribe<TKey, TValue>(
            IEnumerable<string> topics,
            Func<ConsumeResult<TKey, TValue>, bool> receiveHandler,
            Action<ConsumeResult<TKey, TValue>, int, Exception> exceptionHandler = null,
            Action<ConsumerBuilder<TKey, TValue>> @delegate = null,
            bool commit = false,
            int retryCount = 5,
            bool durableFailOrExceptionMessage = true)
        {
            var consumer = this.Subscribe(topics, @delegate);

            if (consumer.IsNotNull())
            {
                while (true)
                {
                    var message = consumer.Consume();
                    var numberOfRetries = 0;
                    Exception exception = null;
                    bool? result = false;
                    while (numberOfRetries <= retryCount)
                    {
                        try
                        {
                            if (message.IsPartitionEOF || message.Message.IsNull() || message.Message.Value.IsNull())
                                continue;

                            result = receiveHandler?.Invoke(message);

                            //异常置空
                            exception = null;

                            break;
                        }
                        catch (Exception ex)
                        {
                            exception = ex;
                            exceptionHandler?.Invoke(message, numberOfRetries, ex);
                            numberOfRetries++;
                        }
                    }

                    //是否手动提交
                    if (commit)
                        consumer.Commit(message);

                    //是否持久化失败或者异常消息
                    if (durableFailOrExceptionMessage && (!(result == true) || exception != null))
                    {
                        this.Publish($"{topics.Join()}.{(exception != null ? "error" : "fail")}", new Message<string, string>
                        {
                            Key = message.Message.Key?.ToString(),
                            Value = new FailOrExceptionMessage
                            {
                                Body = message.Message.Value.ToJson(),
                                Topics = topics,
                                RetryCount = exception == null ? numberOfRetries : numberOfRetries - 1,
                                Exception = exception,
                                ExceptionMsg = exception?.Message
                            }.ToJson()
                        });
                    }
                }
            }
        }
        #endregion
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

    /// <summary>
    /// 失败或者异常消息
    /// </summary>
    public class FailOrExceptionMessage
    {
        /// <summary>
        /// 消息内容
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// 订阅主题
        /// </summary>
        public IEnumerable<string> Topics { get; set; }

        /// <summary>
        /// 重试次数
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// 异常消息
        /// </summary>
        public string ExceptionMsg { get; set; }

        /// <summary>
        /// 异常
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateDateTime { get; set; } = DateTime.Now;
    }
}