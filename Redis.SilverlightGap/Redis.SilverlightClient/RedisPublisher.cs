﻿using Redis.SilverlightClient.Messages;
using Redis.SilverlightClient.Parsers;
using Redis.SilverlightClient.Sockets;
using Sprache;
using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace Redis.SilverlightClient
{
    internal class RedisPublisher : IRedisPublisher
    {
        private readonly SocketConnection socketConnection;

        public RedisPublisher(SocketConnection socketConnection)
        {
            if (socketConnection == null)
                throw new ArgumentNullException("socketConnection");

            this.socketConnection = socketConnection;
        }
         
        public Task<int> PublishMessage(string channelName, string message)
        {
            var publishMessage = new RedisPublishMessage(channelName, message);

            return socketConnection.GetConnection().Take(1).Select(connection =>
            {
                var request = connection.SendMessage(publishMessage.ToString());
                var response = connection.ReceiveMessage();

                return request.Zip(response, (_, result) => result).Select(result =>
                {
                    var pongs = RedisParsersModule.IntegerParser.TryParse(result);

                    if (!pongs.WasSuccessful)
                        throw new ParseException(string.Format("Invalid integer response for published message: {0}", result));

                    return pongs.Value;
                });
            }).Merge(1).ToTask();
        }
    }
}
