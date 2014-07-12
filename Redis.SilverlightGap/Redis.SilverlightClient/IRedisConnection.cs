using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;

namespace Redis.SilverlightClient
{
    public static class RedisConnectionExtensions
    {
        public static IRedisCacheClient AsCacheClient(this RedisConnection redisConnection)
        {
            return redisConnection.AsCacheClient(Scheduler.Default);
        }

        public static IRedisCacheClient AsCacheClient(this RedisConnection redisConnection, IScheduler scheduler)
        {
            if (redisConnection == null)
                throw new ArgumentException("redisConnection");

            return new RedisCacheClient(redisConnection, scheduler);
        }

        public static IRedisPublisher AsPublisher(this RedisConnection redisConnection)
        {
            return redisConnection.AsPublisher(Scheduler.Default);
        }

        public static IRedisPublisher AsPublisher(this RedisConnection redisConnection, IScheduler scheduler)
        {
            if (redisConnection == null)
                throw new ArgumentException("redisConnection");

            return new RedisPublisher(redisConnection, scheduler);
        }
    }
}
