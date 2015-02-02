using PortableSprache;
using Redis.SilverlightClient.Messages;
using Redis.SilverlightClient.Parsers;
using Redis.SilverlightClient.Sockets;
using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace Redis.SilverlightClient
{
    public class RedisSubscriber : IRedisSubscriber, IDisposable
    {
        private readonly SocketConnection socketConnection;
        private readonly byte[] buffer;
        private readonly Subject<RedisChannelMessage> upstreamChannelMessages;
        private readonly Subject<RedisChannelPatternMessage> upstreamChannelPatternMessages;
        private readonly CompositeDisposable disposables;

        public RedisSubscriber(SocketConnection socketConnection)
        {
            if (socketConnection == null)
                throw new ArgumentNullException("socketConnection");

            this.socketConnection = socketConnection;
            this.buffer = new byte[4096];
            this.upstreamChannelMessages = new Subject<RedisChannelMessage>();
            this.upstreamChannelPatternMessages = new Subject<RedisChannelPatternMessage>();

            var remainder = string.Empty;

            disposables = new CompositeDisposable();
            disposables.Add(socketConnection);
            disposables.Add(upstreamChannelMessages);
            disposables.Add(upstreamChannelPatternMessages);
            disposables.Add(socketConnection.Scheduler.Schedule(self =>
            {
                socketConnection
                    .Connection
                    .Select(connection => connection.ReceiveMessage())
                    .Merge(1)
                    .Subscribe(message =>
                     {
                         remainder += message;
                         bool retry = true;

                         while (retry)
                         {
                             retry = false;

                             var parseSubscriptionMessage = RedisParsersModule.SubscriptionMessageParser.TryParse(remainder);
                             if (parseSubscriptionMessage.WasSuccessful)
                             {
                                 var position = parseSubscriptionMessage.Remainder.Position;
                                 remainder = parseSubscriptionMessage.Remainder.Source.Substring(position);
                                 retry = remainder.Length > 0;
                             }

                             var parseChannelMessage = RedisChannelMessage.RedisChannelMessageParser.TryParse(remainder);
                             if (parseChannelMessage.WasSuccessful)
                             {
                                 var position = parseChannelMessage.Remainder.Position;
                                 remainder = parseChannelMessage.Remainder.Source.Substring(position);
                                 upstreamChannelMessages.OnNext(parseChannelMessage.Value);
                                 retry = remainder.Length > 0;
                             }

                             var parseChannelPatternMessage = RedisChannelPatternMessage.RedisChannelPatternMessageParser.TryParse(remainder);
                             if(parseChannelPatternMessage.WasSuccessful)
                             {
                                 var position = parseChannelPatternMessage.Remainder.Position;
                                 remainder = parseChannelPatternMessage.Remainder.Source.Substring(position);
                                 upstreamChannelPatternMessages.OnNext(parseChannelPatternMessage.Value);
                                 retry = remainder.Length > 0;
                             }
                         }

                         self();
                     }, ex =>
                     {
                         upstreamChannelMessages.OnError(ex);
                         upstreamChannelPatternMessages.OnError(ex);
                     },
                     () =>
                     {
                         upstreamChannelMessages.OnCompleted();
                         upstreamChannelPatternMessages.OnCompleted();
                     });
            }));
        }

        public Task<IObservable<RedisChannelMessage>> Subscribe(params string[] channelNames)
        {
            var subscribeMessage = new RedisSubscribeMessage(channelNames);

            return socketConnection.Connection.Take(1).Select(connection =>
            {
                return connection.SendMessage(subscribeMessage.ToString());
            }).Merge(1).ToTask().ContinueWith(task =>
            {
                if (task.Exception != null)
                    throw task.Exception;

                return upstreamChannelMessages.AsObservable();
            });
        }

        public Task<IObservable<RedisChannelPatternMessage>> PSubscribe(params string[] channelPatterns)
        {
            var subscribeMessage = new RedisPatternSubscribeMessage(channelPatterns);

            return socketConnection.Connection.Take(1).Select(connection =>
            {
                return connection.SendMessage(subscribeMessage.ToString());
            }).Merge(1).ToTask().ContinueWith(task =>
            {
                if(task.Exception != null)
                    throw task.Exception;

                return upstreamChannelPatternMessages.AsObservable();
            });
        }

        public void Dispose()
        {
            disposables.Dispose();
        }
    }
}
