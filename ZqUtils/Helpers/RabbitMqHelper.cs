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
using System.Collections.Concurrent;
using System.Collections.Generic;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ZqUtils.Extensions;
/****************************
* [Author] 张强
* [Date] 2018-06-01
* [Describe] RabbitMq工具类
* **************************/
namespace ZqUtils.Helpers
{
    /// <summary>
    /// RabbitMq工具类
    /// </summary>
    public class RabbitMqHelper : IDisposable
    {
        #region 私有静态字段
        /// <summary>
        /// RabbitMQ建议客户端线程之间不要共用Model，至少要保证共用Model的线程发送消息必须是串行的，但是建议尽量共用Connection。
        /// </summary>
        private static readonly ConcurrentDictionary<string, IModel> ChannelDic = new ConcurrentDictionary<string, IModel>();

        /// <summary>
        /// RabbitMq连接
        /// </summary>
        private static IConnection _conn;

        /// <summary>
        /// 线程对象，线程锁使用
        /// </summary>
        private static readonly object locker = new object();
        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="config">RabbitMq配置</param>
        public RabbitMqHelper(MqConfig config)
        {
            if (_conn != null) return;
            lock (locker)
            {
                var factory = new ConnectionFactory
                {
                    //设置主机名
                    HostName = config.HostName,

                    //虚拟主机
                    VirtualHost = config.VirtualHost,

                    //设置心跳时间
                    RequestedHeartbeat = config.RequestedHeartbeat,

                    //设置自动重连
                    AutomaticRecoveryEnabled = config.AutomaticRecoveryEnabled,

                    //重连时间
                    NetworkRecoveryInterval = config.NetworkRecoveryInterval,

                    //用户名
                    UserName = config.UserName,

                    //密码
                    Password = config.Password
                };
                _conn = _conn ?? factory.CreateConnection();
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="factory">RabbitMq连接工厂</param>
        public RabbitMqHelper(ConnectionFactory factory)
        {
            if (_conn != null || factory == null)
                return;
            lock (locker)
            {
                _conn = _conn ?? factory.CreateConnection();
            }
        }
        #endregion

        #region 通道
        /// <summary>
        /// 获取Channel
        /// </summary>
        /// <returns></returns>
        public IModel GetChannel() => _conn.CreateModel();

        /// <summary>
        /// 获取Channel
        /// </summary>
        /// <param name="exchange">交换机名称</param>
        /// <param name="queue">队列名称</param>
        /// <param name="routingKey">路由key</param>
        /// <param name="exchangeType">交换机类型</param>
        /// <param name="durable">持久化</param>
        /// <param name="queueArguments">队列参数</param>
        /// <param name="exchangeArguments">交换机参数</param>
        /// <returns></returns>
        public IModel GetChannel(
            string exchange,
            string queue,
            string routingKey,
            string exchangeType = ExchangeType.Direct,
            bool durable = true,
            IDictionary<string, object> queueArguments = null,
            IDictionary<string, object> exchangeArguments = null)
        {
            return ChannelDic.GetOrAdd(queue, key =>
            {
                var channel = _conn.CreateModel();
                ExchangeDeclare(channel, exchange, exchangeType, durable, arguments: exchangeArguments);
                QueueDeclare(channel, queue, durable, arguments: queueArguments);
                channel.QueueBind(queue, exchange, routingKey);
                ChannelDic[queue] = channel;
                return channel;
            });
        }

        /// <summary>
        /// 获取Model
        /// </summary>
        /// <param name="queue">队列名称</param>
        /// <param name="durable">持久化</param>
        /// <param name="prefetchCount">预取数量</param>
        /// <returns></returns>
        public IModel GetChannel(
            string queue,
            bool durable = true,
            ushort prefetchCount = 1)
        {
            return ChannelDic.GetOrAdd(queue, key =>
            {
                var channel = _conn.CreateModel();
                QueueDeclare(channel, queue, durable);
                //设置每次预取数量
                channel.BasicQos(0, prefetchCount, false);
                ChannelDic[queue] = channel;
                return channel;
            });
        }
        #endregion

        #region 交换机
        /// <summary>
        /// 声明交换机
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="exchange">交换机名称</param>
        /// <param name="exchangeType">交换机类型：
        /// 1、Direct Exchange – 处理路由键。需要将一个队列绑定到交换机上，要求该消息与一个特定的路由键完全
        /// 匹配。这是一个完整的匹配。如果一个队列绑定到该交换机上要求路由键 “dog”，则只有被标记为“dog”的
        /// 消息才被转发，不会转发dog.puppy，也不会转发dog.guard，只会转发dog
        /// 2、Fanout Exchange – 不处理路由键。你只需要简单的将队列绑定到交换机上。一个发送到交换机的消息都
        /// 会被转发到与该交换机绑定的所有队列上。很像子网广播，每台子网内的主机都获得了一份复制的消息。Fanout
        /// 交换机转发消息是最快的。
        /// 3、Topic Exchange – 将路由键和某模式进行匹配。此时队列需要绑定要一个模式上。符号“#”匹配一个或多
        /// 个词，符号“*”匹配不多不少一个词。因此“audit.#”能够匹配到“audit.irs.corporate”，但是“audit.*”
        /// 只会匹配到“audit.irs”。</param>
        /// <param name="durable">持久化</param>
        /// <param name="autoDelete">自动删除</param>
        /// <param name="arguments">参数</param>
        public void ExchangeDeclare(
            IModel channel,
            string exchange,
            string exchangeType = ExchangeType.Direct,
            bool durable = true,
            bool autoDelete = false,
            IDictionary<string, object> arguments = null)
        {
            (channel ?? _conn.CreateModel()).ExchangeDeclare(exchange, exchangeType, durable, autoDelete, arguments);
        }

        /// <summary>
        /// 声明交换机
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="exchange">交换机名称</param>
        /// <param name="exchangeType">交换机类型：
        /// 1、Direct Exchange – 处理路由键。需要将一个队列绑定到交换机上，要求该消息与一个特定的路由键完全
        /// 匹配。这是一个完整的匹配。如果一个队列绑定到该交换机上要求路由键 “dog”，则只有被标记为“dog”的
        /// 消息才被转发，不会转发dog.puppy，也不会转发dog.guard，只会转发dog
        /// 2、Fanout Exchange – 不处理路由键。你只需要简单的将队列绑定到交换机上。一个发送到交换机的消息都
        /// 会被转发到与该交换机绑定的所有队列上。很像子网广播，每台子网内的主机都获得了一份复制的消息。Fanout
        /// 交换机转发消息是最快的。
        /// 3、Topic Exchange – 将路由键和某模式进行匹配。此时队列需要绑定要一个模式上。符号“#”匹配一个或多
        /// 个词，符号“*”匹配不多不少一个词。因此“audit.#”能够匹配到“audit.irs.corporate”，但是“audit.*”
        /// 只会匹配到“audit.irs”。</param>
        /// <param name="durable">持久化</param>
        /// <param name="autoDelete">自动删除</param>
        /// <param name="arguments">参数</param>
        public void ExchangeDeclareNoWait(
            IModel channel,
            string exchange,
            string exchangeType = ExchangeType.Direct,
            bool durable = true,
            bool autoDelete = false,
            IDictionary<string, object> arguments = null)
        {
            (channel ?? _conn.CreateModel()).ExchangeDeclareNoWait(exchange, exchangeType, durable, autoDelete, arguments);
        }

        /// <summary>
        /// 删除交换机
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="exchange">交换机名称</param>
        /// <param name="ifUnused">是否没有被使用</param>
        public void ExchangeDelete(
            IModel channel,
            string exchange,
            bool ifUnused = false)
        {
            (channel ?? _conn.CreateModel()).ExchangeDelete(exchange, ifUnused);
        }

        /// <summary>
        /// 删除交换机
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="exchange">交换机名称</param>
        /// <param name="ifUnused">是否没有被使用</param>
        public void ExchangeDeleteNoWait(
            IModel channel,
            string exchange,
            bool ifUnused = false)
        {
            (channel ?? _conn.CreateModel()).ExchangeDeleteNoWait(exchange, ifUnused);
        }

        /// <summary>
        /// 绑定交换机
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="destinationExchange">目标交换机</param>
        /// <param name="sourceExchange">源交换机</param>
        /// <param name="routingKey">路由键</param>
        /// <param name="arguments">参数</param>
        public void ExchangeBind(
            IModel channel,
            string destinationExchange,
            string sourceExchange,
            string routingKey,
            IDictionary<string, object> arguments = null)
        {
            (channel ?? _conn.CreateModel()).ExchangeBind(destinationExchange, sourceExchange, routingKey, arguments);
        }

        /// <summary>
        /// 绑定交换机
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="destinationExchange">目标交换机</param>
        /// <param name="sourceExchange">源交换机</param>
        /// <param name="routingKey">路由键</param>
        /// <param name="arguments">参数</param>
        public void ExchangeBindNoWait(
            IModel channel,
            string destinationExchange,
            string sourceExchange,
            string routingKey,
            IDictionary<string, object> arguments = null)
        {
            (channel ?? _conn.CreateModel()).ExchangeBindNoWait(destinationExchange, sourceExchange, routingKey, arguments);
        }

        /// <summary>
        /// 解绑交换机
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="destinationExchange">目标交换机</param>
        /// <param name="sourceExchange">源交换机</param>
        /// <param name="routingKey">路由键</param>
        /// <param name="arguments">参数</param>
        public void ExchangeUnbind(
            IModel channel,
            string destinationExchange,
            string sourceExchange,
            string routingKey,
            IDictionary<string, object> arguments = null)
        {
            (channel ?? _conn.CreateModel()).ExchangeUnbind(destinationExchange, sourceExchange, routingKey, arguments);
        }

        /// <summary>
        /// 解绑交换机
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="destinationExchange">目标交换机</param>
        /// <param name="sourceExchange">源交换机</param>
        /// <param name="routingKey">路由键</param>
        /// <param name="arguments">参数</param>
        public void ExchangeUnbindNoWait(
            IModel channel,
            string destinationExchange,
            string sourceExchange,
            string routingKey,
            IDictionary<string, object> arguments = null)
        {
            (channel ?? _conn.CreateModel()).ExchangeUnbindNoWait(destinationExchange, sourceExchange, routingKey, arguments);
        }
        #endregion

        #region 队列
        /// <summary>
        /// 声明队列
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="queue">队列名称</param>
        /// <param name="durable">持久化</param>
        /// <param name="exclusive">排他队列，如果一个队列被声明为排他队列，该队列仅对首次声明它的连接可见，
        /// 并在连接断开时自动删除。这里需要注意三点：其一，排他队列是基于连接可见的，同一连接的不同信道是可
        /// 以同时访问同一个连接创建的排他队列的。其二，“首次”，如果一个连接已经声明了一个排他队列，其他连
        /// 接是不允许建立同名的排他队列的，这个与普通队列不同。其三，即使该队列是持久化的，一旦连接关闭或者
        /// 客户端退出，该排他队列都会被自动删除的。这种队列适用于只限于一个客户端发送读取消息的应用场景。</param>
        /// <param name="autoDelete">自动删除</param>
        /// <param name="arguments">参数</param>
        public void QueueDeclare(
            IModel channel,
            string queue,
            bool durable = true,
            bool exclusive = false,
            bool autoDelete = false,
            IDictionary<string, object> arguments = null)
        {
            (channel ?? _conn.CreateModel()).QueueDeclare(queue, durable, exclusive, autoDelete, arguments);
        }

        /// <summary>
        /// 声明队列
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="queue">队列名称</param>
        /// <param name="durable">持久化</param>
        /// <param name="exclusive">排他队列，如果一个队列被声明为排他队列，该队列仅对首次声明它的连接可见，
        /// 并在连接断开时自动删除。这里需要注意三点：其一，排他队列是基于连接可见的，同一连接的不同信道是可
        /// 以同时访问同一个连接创建的排他队列的。其二，“首次”，如果一个连接已经声明了一个排他队列，其他连
        /// 接是不允许建立同名的排他队列的，这个与普通队列不同。其三，即使该队列是持久化的，一旦连接关闭或者
        /// 客户端退出，该排他队列都会被自动删除的。这种队列适用于只限于一个客户端发送读取消息的应用场景。</param>
        /// <param name="autoDelete">自动删除</param>
        /// <param name="arguments">参数</param>
        public void QueueDeclareNoWait(
            IModel channel,
            string queue,
            bool durable = true,
            bool exclusive = false,
            bool autoDelete = false,
            IDictionary<string, object> arguments = null)
        {
            (channel ?? _conn.CreateModel()).QueueDeclareNoWait(queue, durable, exclusive, autoDelete, arguments);
        }

        /// <summary>
        /// 删除队列
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="queue">队列名称</param>
        /// <param name="ifUnused">是否没有被使用</param>
        /// <param name="ifEmpty">是否为空</param>
        public void QueueDelete(
            IModel channel,
            string queue,
            bool ifUnused = false,
            bool ifEmpty = false)
        {
            (channel ?? _conn.CreateModel()).QueueDelete(queue, ifUnused, ifEmpty);
        }

        /// <summary>
        /// 删除队列
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="queue">队列名称</param>
        /// <param name="ifUnused">是否没有被使用</param>
        /// <param name="ifEmpty">是否为空</param>
        public void QueueDeleteNoWait(
            IModel channel,
            string queue,
            bool ifUnused = false,
            bool ifEmpty = false)
        {
            (channel ?? _conn.CreateModel()).QueueDeleteNoWait(queue, ifUnused, ifEmpty);
        }

        /// <summary>
        /// 绑定队列
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="exchange">交换机名称</param>
        /// <param name="queue">队列名称</param>
        /// <param name="routingKey">路由键</param>
        /// <param name="arguments">参数</param>
        public void QueueBind(
            IModel channel,
            string exchange,
            string queue,
            string routingKey,
            IDictionary<string, object> arguments = null)
        {
            (channel ?? _conn.CreateModel()).QueueBind(queue, exchange, routingKey, arguments);
        }

        /// <summary>
        /// 绑定队列
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="exchange">交换机名称</param>
        /// <param name="queue">队列名称</param>
        /// <param name="routingKey">路由键</param>
        /// <param name="arguments">参数</param>
        public void QueueBindNoWait(
            IModel channel,
            string exchange,
            string queue,
            string routingKey,
            IDictionary<string, object> arguments = null)
        {
            (channel ?? _conn.CreateModel()).QueueBindNoWait(queue, exchange, routingKey, arguments);
        }

        /// <summary>
        /// 解绑队列
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="exchange">交换机名称</param>
        /// <param name="queue">队列名称</param>
        /// <param name="routingKey">路由键</param>
        /// <param name="arguments">参数</param>
        public void QueueUnbind(
            IModel channel,
            string exchange,
            string queue,
            string routingKey,
            IDictionary<string, object> arguments = null)
        {
            (channel ?? _conn.CreateModel()).QueueUnbind(queue, exchange, routingKey, arguments);
        }

        /// <summary>
        /// 清除队列
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="queue">队列名称</param>
        public void QueuePurge(IModel channel, string queue)
        {
            (channel ?? _conn.CreateModel()).QueuePurge(queue);
        }
        #endregion

        #region 发布消息
        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="command">消息指令</param>
        /// <returns></returns>
        public void Publish<T>(T command) where T : class
        {
            var attribute = typeof(T).GetAttribute<RabbitMqAttribute>();

            if (attribute == null)
                throw new ArgumentException("RabbitMqAttribute Is Null!");

            var body = command.ToJson();
            var exchange = attribute.ExchangeName;
            var exchangeType = attribute.ExchangeType;
            var queue = attribute.QueueName;
            var routingKey = attribute.RoutingKey;
            var durable = attribute.Durable;
            var isDeadLetter = attribute.IsDeadLetter;
            //是否设置死信队列
            Dictionary<string, object> arguments = null;
            if (isDeadLetter)
            {
                arguments = new Dictionary<string, object>
                {
                    ["x-dead-letter-exchange"] = $"DeadLetterExchange",
                    ["x-message-ttl"] = attribute.MessageTTL,
                    ["x-dead-letter-routing-key"] = $"{routingKey}@DeadLetter"
                };
            }
            Publish(exchange, queue, routingKey, body, exchangeType, durable, arguments);
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="exchange">交换机名称</param>
        /// <param name="queue">队列名称</param>
        /// <param name="routingKey">路由键</param>
        /// <param name="body">消息内容</param>
        /// <param name="exchangeType">交换机类型</param>
        /// <param name="durable">持久化</param>
        /// <param name="queueArguments">队列参数</param>
        /// <param name="exchangeArguments">交换机参数</param>
        public void Publish(
            string exchange,
            string queue,
            string routingKey,
            string body,
            string exchangeType = ExchangeType.Direct,
            bool durable = true,
            IDictionary<string, object> queueArguments = null,
            IDictionary<string, object> exchangeArguments = null)
        {
            var channel = GetChannel(exchange, queue, routingKey, exchangeType, durable, queueArguments, exchangeArguments);
            var props = channel.CreateBasicProperties();
            props.Persistent = durable;//持久化
            channel.BasicPublish(exchange, routingKey, props, body.SerializeUtf8());
        }

        /// <summary>
        /// 发布消息到死信队列
        /// </summary>
        /// <param name="queue">死信队列名称</param>
        /// <param name="body">消息内容</param>
        /// <param name="ex">异常</param>
        /// <param name="retryCount">重试次数</param>
        private void PublishToDead<T>(
            string queue,
            string body,
            Exception ex,
            int retryCount) where T : class
        {
            var attribute = typeof(T).GetAttribute<RabbitMqAttribute>();
            if (attribute == null)
                throw new ArgumentException("RabbitMqAttribute Is Null!");

            //死信交换机写死：DeadLetterExchange
            var deadLetterExchange = attribute.ExchangeName;
            var deadLetterQueue = attribute.QueueName.Replace("{queue}", queue);
            var deadLetterRoutingKey = attribute.RoutingKey.Replace("{routingkey}", queue);

            //死信队列内容
            var deadLetterBody = new DeadLetterQueue
            {
                Body = body,
                CreateDateTime = DateTime.Now,
                Exception = ex,
                ExceptionMsg = ex?.Message,
                Queue = queue,
                RoutingKey = deadLetterRoutingKey,
                Exchange = deadLetterExchange,
                RetryCount = retryCount
            };
            Publish(deadLetterExchange, deadLetterQueue, deadLetterRoutingKey, deadLetterBody.ToJson());
        }
        #endregion

        #region 订阅消息
        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriber">消费处理委托</param>
        /// <param name="handler">异常处理委托</param>
        public void Subscribe<T>(
            Func<T, bool> subscriber,
            Action<string, int,
            Exception> handler) where T : class
        {
            var attribute = typeof(T).GetAttribute<RabbitMqAttribute>();
            if (attribute == null)
                throw new ArgumentException("RabbitMqAttribute Is Null!");

            Subscribe(attribute.QueueName, subscriber, handler, attribute.RetryCount, attribute.Durable, attribute.PrefetchCount, attribute.IsDeadLetter);
        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queue">队列名称</param>
        /// <param name="subscriber">消费处理委托</param>
        /// <param name="handler">异常处理委托</param>
        /// <param name="retryCount">重试次数</param>
        /// <param name="durable">持久化</param>
        /// <param name="prefetchCount">预取数量</param>
        /// <param name="isDeadLetter">异常是否进入死信队列</param>
        public void Subscribe<T>(
            string queue,
            Func<T, bool> subscriber,
            Action<string, int, Exception> handler,
            int retryCount = 5,
            bool durable = true,
            ushort prefetchCount = 1,
            bool isDeadLetter = false) where T : class
        {
            //队列声明
            var channel = GetChannel(queue, durable, prefetchCount);
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.DeserializeUtf8();
                var numberOfRetries = 0;
                Exception exception = null;
                bool? result = false;
                while (numberOfRetries <= retryCount)
                {
                    try
                    {
                        var msg = body.ToObject<T>();
                        result = subscriber?.Invoke(msg);
                        if (result == true)
                            channel.BasicAck(ea.DeliveryTag, false);
                        else
                            channel.BasicNack(ea.DeliveryTag, false, false);
                        //异常置空
                        exception = null;
                        break;
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                        handler?.Invoke(body, numberOfRetries, ex);
                        numberOfRetries++;
                    }
                }
                //重试后异常仍未解决
                if (exception != null)
                {
                    channel.BasicNack(ea.DeliveryTag, false, false);
                }
                //是否进入死信队列
                if (isDeadLetter && (!(result == true) || exception != null))
                {
                    PublishToDead<DeadLetterQueue>(queue, body, exception, exception == null ? numberOfRetries : numberOfRetries - 1);
                }
            };
            //手动确认
            channel.BasicConsume(queue, false, consumer);
        }
        #endregion

        #region 获取消息
        /// <summary>
        /// 获取消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler">消费处理委托</param>
        public void Pull<T>(Action<T> handler) where T : class
        {
            var attribute = typeof(T).GetAttribute<RabbitMqAttribute>();
            if (attribute == null)
                throw new ArgumentException("RabbitMqAttribute Is Null!");

            Pull(attribute.ExchangeName, attribute.QueueName, attribute.RoutingKey, handler);
        }

        /// <summary>
        /// 获取消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="exchange">交换机名称</param>
        /// <param name="queue">队列名称</param>
        /// <param name="routingKey">路由键</param>
        /// <param name="handler">消费处理委托</param>
        public void Pull<T>(
            string exchange,
            string queue,
            string routingKey,
            Action<T> handler) where T : class
        {
            var channel = GetChannel(exchange, queue, routingKey);

            var result = channel.BasicGet(queue, false);
            if (result == null)
                return;

            var msg = result.Body.DeserializeUtf8().ToObject<T>();
            try
            {
                handler(msg);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                channel.BasicAck(result.DeliveryTag, false);
            }
        }
        #endregion

        #region 释放资源
        /// <summary>
        /// 执行与释放或重置非托管资源关联的应用程序定义的任务。
        /// </summary>
        public void Dispose()
        {
            foreach (var item in ChannelDic)
            {
                item.Value?.Dispose();
            }
            _conn?.Dispose();
        }
        #endregion
    }

    /// <summary>
    /// RabbitMq连接配置
    /// </summary>
    public class MqConfig
    {
        /// <summary>
        /// 主机名
        /// </summary>
        public string HostName { get; set; } = "127.0.0.1";

        /// <summary>
        /// 心跳时间
        /// </summary>
        public ushort RequestedHeartbeat { get; set; } = 10;

        /// <summary>
        /// 自动重连
        /// </summary>
        public bool AutomaticRecoveryEnabled { get; set; } = true;

        /// <summary>
        /// 重连时间
        /// </summary>
        public TimeSpan NetworkRecoveryInterval { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; } = "guest";

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; } = "guest";

        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { get; set; } = 5672;

        /// <summary>
        /// 虚拟主机
        /// </summary>
        public string VirtualHost { get; set; } = "/";
    }

    /// <summary>
    /// 自定义的RabbitMq队列信息实体特性
    /// </summary>
    public class RabbitMqAttribute : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="queueName"></param>
        public RabbitMqAttribute(string queueName)
        {
            QueueName = queueName ?? string.Empty;
        }

        /// <summary>
        /// 交换机名称
        /// </summary>
        public string ExchangeName { get; set; }

        /// <summary>
        /// 交换机类型
        /// </summary>
        public string ExchangeType { get; set; }

        /// <summary>
        /// 队列名称
        /// </summary>
        public string QueueName { get; set; }

        /// <summary>
        /// 路由键
        /// </summary>
        public string RoutingKey { get; set; }

        /// <summary>
        /// 是否持久化
        /// </summary>
        public bool Durable { get; set; }

        /// <summary>
        /// 预取数量
        /// </summary>
        public ushort PrefetchCount { get; set; }

        /// <summary>
        /// 异常重试次数
        /// </summary>
        public int RetryCount { get; set; } = 5;

        /// <summary>
        /// 是否进入死信队列
        /// </summary>
        public bool IsDeadLetter { get; set; }

        /// <summary>
        /// 死信交换机生存时间
        /// </summary>
        public int MessageTTL { get; set; } = 30 * 1000;
    }

    /// <summary>
    /// 死信队列实体
    /// </summary>
    [RabbitMq("{queue}@DeadLetter", ExchangeName = "DeadLetterExchange", RoutingKey = "{routingkey}@DeadLetter")]
    public class DeadLetterQueue
    {
        /// <summary>
        /// 消息内容
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// 交换机
        /// </summary>
        public string Exchange { get; set; }

        /// <summary>
        /// 队列
        /// </summary>
        public string Queue { get; set; }

        /// <summary>
        /// 路由键
        /// </summary>
        public string RoutingKey { get; set; }

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