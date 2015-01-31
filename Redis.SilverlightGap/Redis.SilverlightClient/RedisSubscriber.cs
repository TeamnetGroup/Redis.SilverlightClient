using PortableSprache;
using Redis.SilverlightClient.Messages;
using Redis.SilverlightClient.Parsers;
using Redis.SilverlightClient.Sockets;
using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Linq;
using System.Reactive.Subjects;
using System.Reactive.Disposables;

namespace Redis.SilverlightClient
{
    public class RedisSubscriber : IDisposable
    {
        private readonly SocketConnection socketConnection;
        private readonly byte[] buffer;
        private readonly Subject<string> upstream;
        private readonly CompositeDisposable disposables;

        public RedisSubscriber(SocketConnection socketConnection)
        {
            if (socketConnection == null)
                throw new ArgumentNullException("socketConnection");

            this.socketConnection = socketConnection;
            this.buffer = new byte[4096];
            this.upstream = new Subject<string>();

            socketConnection.Scheduler.Schedule(self =>
            {
                socketConnection
                    .Connection
                    .Select(connection => connection.ReceiveMessage())
                    .Merge(1)
                    .Do(_ => self())
                    .Subscribe(upstream);
            });
        }

        public IObservable<RedisChannelMessage> Subscribe(params string[] channelNames)
        {
            var subscribeMessage = new RedisSubscribeMessage(channelNames);

            return socketConnection.Connection.Select(connection =>
            {
                return connection
                    .SendMessage(subscribeMessage.ToString())
                    .Select(_ => upstream.Select(part =>
                    {
                        var subscriptionMessageResult = RedisParsersModule.SubscriptionMessageParser.TryParse(part);
                        if (subscriptionMessageResult.WasSuccessful)
                            return null;

                        var channelMessageResult = RedisChannelMessage.RedisChannelMessageParser.TryParse(part);

                        if (!channelMessageResult.WasSuccessful)
                            throw new ParseException(string.Format("Invalid subscriber message: {0}", part));

                        return channelMessageResult.Value;
                    })).Merge(1).Where(x => x != null);
            }).Merge(1);
        }

        public void Dispose()
        {
            upstream.OnCompleted();
            upstream.Dispose();
            socketConnection.Dispose();
        }
    }
}
