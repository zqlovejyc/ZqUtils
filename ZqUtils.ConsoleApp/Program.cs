using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ZqUtils.Extensions;
using ZqUtils.Helpers;
using ZqUtils.Redis;

namespace ZqUtils.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            #region RabbitMq
            using (var rabbitMq = new RabbitMqHelper(new MqConfig()))
            {
                rabbitMq.Subscribe<string>("exchange11", "queue11", $"exchange11__queue11",
                   (message, para) => { Console.WriteLine(message); return true; },
                   (msg, retry, ex) => Console.WriteLine(ex));

                Parallel.For(0, 10, x =>
                {
                    for (int i = 0; i < 100; i++)
                    {
                        rabbitMq.Publish("exchange11", "queue11", $"exchange11__queue11", $"{x + 1}__test{i + 1}");
                    }
                });

                Thread.Sleep(10000);
            }

            #endregion

            #region Redis
            var redis = new RedisHelper(new RedisConfiguration { PoolSize = 10, ConnectionString = "127.0.0.1:6379,password=123" });
            Console.WriteLine(redis.RedisConnectionPoolManager.GetConnectionInformations().ToJson());
            //redis.StringSet("test", "111");

            var caches = new string[1000];
            Parallel.For(0, 1000, (i, state) =>
            {
                try
                {
                    var helper = new RedisHelper(new RedisConfiguration { PoolSize = 10, ConnectionString = "127.0.0.1:6379,password=123" });
                    caches[i] = helper.StringGet("test");
                    //Console.WriteLine($"{i + 1}: {caches[i]}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });

            Console.WriteLine($"测试完成，数量：{caches.Count(x => x != null)}");
            #endregion

            Console.ReadLine();
        }
    }
}
