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
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.AutoSubscribe;
using EasyNetQIServiceProvider = EasyNetQ.IServiceProvider;
/****************************
* [Author] 张强
* [Date] 2018-06-01
* [Describe] RabbitMq工具类
* **************************/
namespace ZqUtils.Helpers
{
    /// <summary>
    /// RabbitMQ工具类，基于EasyNetQ，使用时需要从nuget安装EasyNetQ。
    /// <para>
    /// <example>
    /// 使用方法：
    /// <code>
    /// using(var mq = new RabbitMqHelper('rabbitmq连接字符串'))
    /// { ...
    /// }
    /// </code>
    /// </example>
    /// </para>
    /// </summary>
    public class RabbitMqHelper : IDisposable
    {
        #region 私有字段
        /// <summary>
        /// bus
        /// </summary>
        private readonly IBus bus;
        #endregion

        #region 公有属性
        /// <summary>
        /// 静态单例
        /// </summary>
        public static RabbitMqHelper Instance => SingletonHelper<RabbitMqHelper>.GetInstance();
        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        public RabbitMqHelper()
        {
            var connectionString = ConfigHelper.GetAppSettings<string>("RabbitMqConnectionString");
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException("RabbitMqConnectionString连接字符串未进行配置！");
            bus = RabbitHutch.CreateBus(connectionString);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionString">rabbitmq连接字符串</param>
        /// <param name="loggerFunc">日志委托，默认为null</param>
        public RabbitMqHelper(string connectionString, Func<EasyNetQIServiceProvider, IEasyNetQLogger> loggerFunc = null)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));
            if (loggerFunc == null)
            {
                bus = RabbitHutch.CreateBus(connectionString);
            }
            else
            {
                bus = RabbitHutch.CreateBus(connectionString, x => x.Register(loggerFunc));
            }
        }
        #endregion

        #region 同步方法
        #region 发布/订阅
        /// <summary>
        /// 发布一条消息(广播)
        /// </summary>
        /// <typeparam name="TMessage">消息泛型类型</typeparam>
        /// <param name="message"></param>
        public void Publish<TMessage>(TMessage message) where TMessage : class
        {
            bus.Publish(message);
        }

        /// <summary>
        /// 指定Topic，发布一条消息
        /// </summary>
        /// <typeparam name="TMessage">消息泛型类型</typeparam>
        /// <param name="message"></param>
        /// <param name="topic"></param>
        public void PublishWithTopic<TMessage>(TMessage message, string topic) where TMessage : class
        {
            if (string.IsNullOrEmpty(topic))
                Publish(message);
            else
                bus.Publish(message, x => x.WithTopic(topic));
        }

        /// <summary>
        /// 发布消息。一次性发布多条
        /// </summary>
        /// <typeparam name="TMessage">消息泛型类型</typeparam>
        /// <param name="messages"></param>
        public void PublishMany<TMessage>(List<TMessage> messages) where TMessage : class
        {
            foreach (var message in messages)
            {
                Publish(message);
            }
        }

        /// <summary>
        /// 发布消息。一次性发布多条
        /// </summary>
        /// <typeparam name="TMessage">消息泛型类型</typeparam>
        /// <param name="messages"></param>
        /// <param name="topic"></param>
        public void PublishManyWithTopic<TMessage>(List<TMessage> messages, string topic) where TMessage : class
        {
            foreach (var message in messages)
            {
                PublishWithTopic(message, topic);
            }
        }

        /// <summary>
        /// 消息订阅
        /// </summary>
        /// <typeparam name="TMessage">消息泛型类型</typeparam>
        /// <param name="subscriptionId">消息订阅标识</param>
        /// <param name="process">
        /// 消息处理委托方法
        /// <para>
        /// <example>
        /// 例如：
        /// <code>
        /// message=>Task.Factory.StartNew(()=>{
        ///     Console.WriteLine(message);
        /// })
        /// </code>
        /// </example>
        /// </para>
        /// </param>
        public ISubscriptionResult Subscribe<TMessage>(string subscriptionId, Func<TMessage, Task> process) where TMessage : class
        {
            return bus.Subscribe<TMessage>(subscriptionId, message => process(message));
        }

        /// <summary>
        /// 消息订阅
        /// </summary>
        /// <typeparam name="TMessage">消息泛型类型</typeparam>
        /// <param name="subscriptionId">消息订阅标识</param>
        /// <param name="process">
        /// 消息处理委托方法
        /// <para>
        /// <example>
        /// 例如：
        /// <code>
        /// message=>Task.Factory.StartNew(()=>{
        ///     Console.WriteLine(message);
        /// })
        /// </code>
        /// </example>
        /// </para>
        /// </param>
        /// <param name="topic">topic</param>
        public ISubscriptionResult SubscribeWithTopic<TMessage>(string subscriptionId, Func<TMessage, Task> process, string topic) where TMessage : class
        {
            return bus.Subscribe<TMessage>(subscriptionId, message => process(message), x => x.WithTopic(topic));
        }

        /// <summary>
        /// 自动订阅
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <param name="subscriptionIdPrefix"></param>
        /// <param name="topic"></param>
        public void AutoSubscribe(string assemblyName, string subscriptionIdPrefix, string topic)
        {
            var subscriber = new AutoSubscriber(bus, subscriptionIdPrefix);
            if (!string.IsNullOrEmpty(topic))
                subscriber.ConfigureSubscriptionConfiguration = x => x.WithTopic(topic);
            subscriber.Subscribe(Assembly.Load(assemblyName));
        }
        #endregion

        #region 发送/接收
        /// <summary>
        /// 给指定队列发送一条信息
        /// </summary>
        /// <typeparam name="TMessage">消息泛型类型</typeparam>
        /// <param name="queue">队列名称</param>
        /// <param name="message">消息</param>
        public void Send<TMessage>(string queue, TMessage message) where TMessage : class
        {
            bus.Send(queue, message);
        }

        /// <summary>
        /// 给指定队列批量发送信息
        /// </summary>
        /// <typeparam name="TMessage">消息泛型类型</typeparam>
        /// <param name="queue">队列名称</param>
        /// <param name="messages">消息</param>
        public void SendMany<TMessage>(string queue, IList<TMessage> messages) where TMessage : class
        {
            foreach (var message in messages)
            {
                Send(queue, message);
            }
        }

        /// <summary>
        /// 从指定队列接收一条信息，并做相关处理。
        /// </summary>
        /// <typeparam name="TMessage">消息泛型类型</typeparam>
        /// <param name="queue">队列名称</param>
        /// <param name="process">
        /// 消息处理委托方法
        /// <para>
        /// <example>
        /// 例如：
        /// <code>
        /// message=>Task.Factory.StartNew(()=>{
        ///     Console.WriteLine(message);
        /// })
        /// </code>
        /// </example>
        /// </para>
        /// </param>
        public void Receive<TMessage>(string queue, Func<TMessage, Task> process) where TMessage : class
        {
            bus.Receive(queue, process);
        }

        /// <summary>
        /// 从指定队列接收一条信息，并做相关处理。
        /// </summary>
        /// <typeparam name="TMessage">消息泛型类型</typeparam>
        /// <param name="queue">队列名称</param>
        /// <param name="process">消息处理委托方法</param>
        public void Receive<TMessage>(string queue, Action<TMessage> process) where TMessage : class
        {
            bus.Receive(queue, process);
        }
        #endregion
        #endregion

        #region 异步方法
        #region 发布/订阅
        /// <summary>
        /// 发布一条消息(广播)
        /// </summary>
        /// <typeparam name="TMessage">消息泛型类型</typeparam>
        /// <param name="message"></param>
        /// <returns>Task</returns>
        public async Task PublishAsync<TMessage>(TMessage message) where TMessage : class
        {
            await bus.PublishAsync(message);
        }

        /// <summary>
        /// 指定Topic，发布一条消息
        /// </summary>
        /// <typeparam name="TMessage">消息泛型类型</typeparam>
        /// <param name="message"></param>
        /// <param name="topic"></param>
        /// <returns>Task</returns>
        public async Task PublishWithTopicAsync<TMessage>(TMessage message, string topic) where TMessage : class
        {
            if (string.IsNullOrEmpty(topic))
                await PublishAsync(message);
            else
                await bus.PublishAsync(message, x => x.WithTopic(topic));
        }

        /// <summary>
        /// 发布消息。一次性发布多条
        /// </summary>
        /// <typeparam name="TMessage">消息泛型类型</typeparam>
        /// <param name="messages"></param>
        /// <returns>Task</returns>
        public async Task PublishManyAsync<TMessage>(List<TMessage> messages) where TMessage : class
        {
            foreach (var message in messages)
            {
                await PublishAsync(message);
            }
        }

        /// <summary>
        /// 发布消息。一次性发布多条
        /// </summary>
        /// <typeparam name="TMessage">消息泛型类型</typeparam>
        /// <param name="messages"></param>
        /// <param name="topic"></param>
        /// <returns>Task</returns>
        public async Task PublishManyWithTopicAsync<TMessage>(List<TMessage> messages, string topic) where TMessage : class
        {
            foreach (var message in messages)
            {
                await PublishWithTopicAsync(message, topic);
            }
        }

        /// <summary>
        /// 消息订阅
        /// </summary>
        /// <typeparam name="TMessage">消息泛型类型</typeparam>
        /// <param name="subscriptionId">消息订阅标识</param>
        /// <param name="process">
        /// 消息处理委托方法
        /// <para>
        /// <example>
        /// 例如：
        /// <code>
        /// message=>Task.Factory.StartNew(()=>{
        ///     Console.WriteLine(message);
        /// })
        /// </code>
        /// </example>
        /// </para>
        /// </param>
        /// <returns>Task</returns>
        public async Task<ISubscriptionResult> SubscribeAsync<TMessage>(string subscriptionId, Func<TMessage, Task> process) where TMessage : class
        {
            return await Task.Run(() => bus.SubscribeAsync(subscriptionId, process));
        }

        /// <summary>
        /// 消息订阅
        /// </summary>
        /// <typeparam name="TMessage">消息泛型类型</typeparam>
        /// <param name="subscriptionId">消息订阅标识</param>
        /// <param name="process">
        /// 消息处理委托方法
        /// <para>
        /// <example>
        /// 例如：
        /// <code>
        /// message=>Task.Factory.StartNew(()=>{
        ///     Console.WriteLine(message);
        /// })
        /// </code>
        /// </example>
        /// </para>
        /// </param>
        /// <param name="topic">topic</param>
        /// <returns>Task</returns>
        public async Task<ISubscriptionResult> SubscribeWithTopicAsync<TMessage>(string subscriptionId, Func<TMessage, Task> process, string topic) where TMessage : class
        {
            return await Task.Run(() => bus.SubscribeAsync<TMessage>(subscriptionId, message => process(message), x => x.WithTopic(topic)));
        }

        /// <summary>
        /// 自动订阅
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <param name="subscriptionIdPrefix"></param>
        /// <param name="topic"></param>
        /// <returns>Task</returns>
        public async Task AutoSubscribeAsync(string assemblyName, string subscriptionIdPrefix, string topic)
        {
            var subscriber = new AutoSubscriber(bus, subscriptionIdPrefix);
            if (!string.IsNullOrEmpty(topic))
                subscriber.ConfigureSubscriptionConfiguration = x => x.WithTopic(topic);
            await Task.Run(() => subscriber.SubscribeAsync(Assembly.Load(assemblyName)));
        }
        #endregion

        #region 发送/接收
        /// <summary>
        /// 给指定队列发送一条信息
        /// </summary>
        /// <typeparam name="TMessage">消息泛型类型</typeparam>
        /// <param name="queue">队列名称</param>
        /// <param name="message">消息</param>
        /// <returns>Task</returns>
        public async Task SendAsync<TMessage>(string queue, TMessage message) where TMessage : class
        {
            await bus.SendAsync(queue, message);
        }

        /// <summary>
        /// 给指定队列批量发送信息
        /// </summary>
        /// <typeparam name="TMessage">消息泛型类型</typeparam>
        /// <param name="queue">队列名称</param>
        /// <param name="messages">消息</param>
        /// <returns>Task</returns>
        public async Task SendManyAsync<TMessage>(string queue, IList<TMessage> messages) where TMessage : class
        {
            foreach (var message in messages)
            {
                await SendAsync(queue, message);
            }
        }

        /// <summary>
        /// 从指定队列接收一条信息，并做相关处理。
        /// </summary>
        /// <typeparam name="TMessage">消息泛型类型</typeparam>
        /// <param name="queue">队列名称</param>
        /// <param name="process">
        /// 消息处理委托方法
        /// <para>
        /// <example>
        /// 例如：
        /// <code>
        /// message=>Task.Factory.StartNew(()=>{
        ///     Console.WriteLine(message);
        /// })
        /// </code>
        /// </example>
        /// </para>
        /// </param>
        /// <returns>Task</returns>
        public async Task ReceiveAsync<TMessage>(string queue, Func<TMessage, Task> process) where TMessage : class
        {
            await Task.Run(() => bus.Receive(queue, process));
        }

        /// <summary>
        /// 从指定队列接收一条信息，并做相关处理。
        /// </summary>
        /// <typeparam name="TMessage">消息泛型类型</typeparam>
        /// <param name="queue">队列名称</param>
        /// <param name="process">消息处理委托方法</param>
        /// <returns>Task</returns>
        public async Task ReceiveAsync<TMessage>(string queue, Action<TMessage> process) where TMessage : class
        {
            await Task.Run(() => bus.Receive(queue, process));
        }
        #endregion
        #endregion

        #region 释放资源
        /// <summary>
        /// 资源释放
        /// </summary>
        public void Dispose()
        {
            bus?.Dispose();
        }
        #endregion
    }
}