using PortableSprache;
using Redis.SilverlightClient.Messages;
using System;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Redis.SilverlightClient
{
    public static class RedisSubscriber
    {
        public static IObservable<RedisChannelMessage> SubscribeToChannel(string host, int port, string channel)
        {
            return SubscribeToChannel(host, port, channel, Scheduler.Default);
        }

        public static IObservable<RedisChannelMessage> SubscribeToChannel(string host, int port, string channel, IScheduler scheduler)
        {
            if(string.IsNullOrEmpty(host))
                throw new ArgumentException("host");

            if (port < 4502 || port > 4534)
                throw new ArgumentException("Port must be in range 4502-4534 due to Silverlight network access restrictions");

            if (string.IsNullOrEmpty(channel))
                throw new ArgumentException("channel");

            if (scheduler == null)
                throw new ArgumentNullException("scheduler");

            var wireMessage = string.Format("*2\r\n$9\r\nSUBSCRIBE\r\n${0}\r\n{1}\r\n", channel.Length, channel);
            var receivedMessagesParts = SubscribeToRedisWithMessage(host, port, wireMessage, scheduler).Skip(1);

            return Observable.Create<RedisChannelMessage>(observer =>
            {
                var remainder = string.Empty;

                return receivedMessagesParts.ObserveOn(scheduler).Subscribe(
                    part =>
                    {
                        var parseTry = RedisChannelMessage.SubscribeMessageParser.TryParse(remainder + part);

                        if (!parseTry.WasSuccessful)
                        {
                            remainder += part;
                        }

                        while (parseTry.WasSuccessful)
                        {
                            remainder = string.Empty;
                            observer.OnNext(parseTry.Value);

                            if (!parseTry.Remainder.AtEnd)
                            {
                                remainder += parseTry.Remainder.Source.Substring(parseTry.Remainder.Position);
                            }

                            parseTry = RedisChannelMessage.SubscribeMessageParser.TryParse(remainder);
                        }
                    },
                    observer.OnError);
            });
        }

        public static IObservable<RedisChannelPatternMessage> SubscribeToChannelPattern(
            string host,
            int port,
            string channelPattern)
        {
            return SubscribeToChannelPattern(host, port, channelPattern, Scheduler.Default);
        }

        public static IObservable<RedisChannelPatternMessage> SubscribeToChannelPattern(string host, int port, string channelPattern, IScheduler scheduler)
        {
            if (string.IsNullOrEmpty(host))
                throw new ArgumentException("host");

            if (port < 4502 || port > 4534)
                throw new ArgumentException("Port must be in range 4502-4534 due to Silverlight network access restrictions");

            if (string.IsNullOrEmpty(channelPattern))
                throw new ArgumentException("channelPattern");

            if (scheduler == null)
                throw new ArgumentNullException("scheduler");

            var wireMessage = string.Format("*2\r\n$10\r\nPSUBSCRIBE\r\n${0}\r\n{1}\r\n", channelPattern.Length, channelPattern);
            var receivedMessagesParts = SubscribeToRedisWithMessage(host, port, wireMessage, scheduler).Skip(1);

            return Observable.Create<RedisChannelPatternMessage>(observer =>
            {
                var remainder = string.Empty;

                return receivedMessagesParts.ObserveOn(scheduler).Subscribe(part =>
                {
                    var parseTry = RedisChannelPatternMessage.SubscribeMessageParser.TryParse(remainder + part);

                    if (!parseTry.WasSuccessful)
                    {
                        remainder += part;
                    }

                    while (parseTry.WasSuccessful)
                    {
                        remainder = string.Empty;
                        observer.OnNext(parseTry.Value);

                        if (!parseTry.Remainder.AtEnd)
                        {
                            remainder += parseTry.Remainder.Source.Substring(parseTry.Remainder.Position);
                        }

                        parseTry = RedisChannelPatternMessage.SubscribeMessageParser.TryParse(remainder);
                    }
                }, ex => observer.OnError(ex));
            });
        }

        private static IObservable<string> SubscribeToRedisWithMessage(string host, int port, string message, IScheduler scheduler)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var socketAsyncEventArgs = new SocketAsyncEventArgs();

            var redisConnector = new RedisConnector(
                                    () => socket,
                                    () => socketAsyncEventArgs);

            var result = from connection in redisConnector.BuildConnectionToken(host, port, scheduler)
                         let transmitter = new RedisTransmitter(connection)
                         from _ in transmitter.SendMessage(message, scheduler)
                         let receiver = new RedisReceiver(connection)
                         from messageReceived in receiver.Receive(new byte[4096], scheduler, repeat: true)
                         select messageReceived;

            var disposables = new CompositeDisposable();
            disposables.Add(socket);
            disposables.Add(socketAsyncEventArgs);

            return Observable.Using(() => disposables, _ => result);
        }
    }
}
