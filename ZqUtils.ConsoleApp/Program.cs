using System;
using System.Linq;
using System.Threading.Tasks;
using ZqUtils.Extensions;
using ZqUtils.Helpers;

namespace ZqUtils.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var redis = new RedisHelper(poolSize: 10, redisConnectionString: "127.0.0.1:6379,password=123");
            Console.WriteLine(redis.RedisConnectionPoolManager.GetConnectionInformations().ToJson());
            //redis.StringSet("test", "111");

            var caches = new string[1000];
            Parallel.For(0, 1000, (i, state) =>
            {
                try
                {
                    var helper = new RedisHelper(poolSize: 10, redisConnectionString: "127.0.0.1:6379,password=123");
                    caches[i] = helper.StringGet("test");
                    //Console.WriteLine($"{i + 1}: {caches[i]}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });

            Console.WriteLine($"测试完成，数量：{caches.Count(x => x != null)}");
            Console.ReadLine();
        }
    }
}
