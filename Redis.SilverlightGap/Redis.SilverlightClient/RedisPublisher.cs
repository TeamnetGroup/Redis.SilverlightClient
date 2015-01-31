using PortableSprache;
using Redis.SilverlightClient.Messages;
using Redis.SilverlightClient.Parsers;
using Redis.SilverlightClient.Sockets;
using System;
using System.Reactive.Concurrency;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Linq;

namespace Redis.SilverlightClient
{
    internal class RedisPublisher : IRedisPublisher, IDisposable
    {
        private readonly SocketConnection socketConnection;
        private readonly byte[] buffer;

        public RedisPublisher(SocketConnection socketConnection)
        {
            if (socketConnection == null)
                throw new ArgumentNullException("socketConnection");

            this.socketConnection = socketConnection;
            this.buffer = new byte[4096];
        }
         
        public Task<int> PublishMessage(string channelName, string message)
        {
            var publishMessage = new RedisPublishMessage(channelName, message);

            return socketConnection.Connection.Select(connection =>
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
            }).Merge().ToTask();
        }

        public void Dispose()
        {
            socketConnection.Dispose();
        }
    }
}
