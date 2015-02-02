using Redis.SilverlightClient.Sockets;
using System;

namespace Redis.SilverlightClient
{
    public static class SocketConnectionExtensions
    {
        public static IRedisCacheClient AsCacheClient(this SocketConnection socketConnection)
        {
            if (socketConnection == null)
                throw new ArgumentException("socketConnection");

            return new RedisCacheClient(socketConnection);
        }

        public static IRedisPublisher AsPublisher(this SocketConnection socketConnection)
        {
            if (socketConnection == null)
                throw new ArgumentException("socketConnection");

            return new RedisPublisher(socketConnection);
        }

        public static RedisSubscriber AsSubscriber(this SocketConnection socketConnection)
        {
            if (socketConnection == null)
                throw new ArgumentException("socketConnection");

            return new RedisSubscriber(socketConnection);
        }
    }
}
