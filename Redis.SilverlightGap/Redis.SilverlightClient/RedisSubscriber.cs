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
using Sprache;

namespace Redis.SilverlightClient
{
    internal class RedisSubscriber : IRedisSubscriber
    {
        private readonly SocketConnection socketConnection;
        private readonly Subject<RedisChannelMessage> upstreamChannelMessages;
        private readonly Subject<RedisChannelPatternMessage> upstreamChannelPatternMessages;
        private readonly CompositeDisposable disposables;

        private string remainder;

        public RedisSubscriber(SocketConnection socketConnection)
        {
            if (socketConnection == null)
                throw new ArgumentNullException("socketConnection");

            this.socketConnection = socketConnection;
            this.upstreamChannelMessages = new Subject<RedisChannelMessage>();
            this.upstreamChannelPatternMessages = new Subject<RedisChannelPatternMessage>();
            this.disposables = new CompositeDisposable();
            this.remainder = string.Empty;

            Func<bool> tryParseRemainder = () =>
            {
                var parseChannelPatternMessage = RedisChannelPatternMessage.RedisChannelPatternMessageParser.TryParse(remainder);
                if (parseChannelPatternMessage.WasSuccessful)
                {
                    var position = parseChannelPatternMessage.Remainder.Position;
                    remainder = parseChannelPatternMessage.Remainder.Source.Substring(position);
                    upstreamChannelPatternMessages.OnNext(parseChannelPatternMessage.Value);
                    return true;
                }

                var parseSubscriptionMessage = RedisParsersModule.SubscriptionMessageParser.TryParse(remainder);
                if (parseSubscriptionMessage.WasSuccessful)
                {
                    var position = parseSubscriptionMessage.Remainder.Position;
                    remainder = parseSubscriptionMessage.Remainder.Source.Substring(position);
                    return true;
                }

                var parseChannelMessage = RedisChannelMessage.RedisChannelMessageParser.TryParse(remainder);
                if (parseChannelMessage.WasSuccessful)
                {
                    var position = parseChannelMessage.Remainder.Position;
                    remainder = parseChannelMessage.Remainder.Source.Substring(position);
                    upstreamChannelMessages.OnNext(parseChannelMessage.Value);
                    return true;
                }

                return false;
            };

            disposables.Add(upstreamChannelMessages);
            disposables.Add(upstreamChannelPatternMessages);
            disposables.Add(
                socketConnection
                    .GetConnection()
                    .Select(connection =>
                        connection.ReceiveMessage())
                    .Merge(1)
                    .DoWhile(() => !socketConnection.IsDisposed)
                .SubscribeOn(socketConnection.Scheduler)
                .Subscribe(message =>
                {
                    if (message.StartsWith("-", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var error = new ParseException(remainder);
                        upstreamChannelMessages.OnError(error);
                        upstreamChannelPatternMessages.OnError(error);
                    }

                    remainder += message;
                    bool previousMessageWasParsed = false;
                    do
                    {
                        previousMessageWasParsed = tryParseRemainder();
                    }
                    while (previousMessageWasParsed);
                 }, 
                 ex => 
                 {
                     upstreamChannelMessages.OnError(ex);
                     upstreamChannelPatternMessages.OnError(ex);
                 },
                 () =>
                 {
                     upstreamChannelMessages.OnCompleted();
                     upstreamChannelPatternMessages.OnCompleted();
                 }));
        }

        public Task<IObservable<RedisChannelMessage>> Subscribe(params string[] channelNames)
        {
            var subscribeMessage = new RedisSubscribeMessage(channelNames);

            return socketConnection.GetConnection().Select(connection => 
                connection.SendMessage(subscribeMessage.ToString()))
            .Merge(1)
            .ToTask().ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    task.Exception.Handle(_ => true);
                    throw task.Exception.InnerException;
                }

                return upstreamChannelMessages.AsObservable();
            });
        }

        public Task<IObservable<RedisChannelPatternMessage>> PSubscribe(params string[] channelPatterns)
        {
            var subscribeMessage = new RedisPatternSubscribeMessage(channelPatterns);

            return socketConnection.GetConnection().Select(connection =>
                connection.SendMessage(subscribeMessage.ToString()))
            .Merge(1)
            .ToTask().ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    task.Exception.Handle(_ => true);
                    throw task.Exception.InnerException;
                }

                return upstreamChannelPatternMessages.AsObservable();
            });
        }

        public void Dispose()
        {
            disposables.Dispose();
        }
    }
}
