using PortableSprache;
using Redis.SilverlightClient.Messages;
using Redis.SilverlightClient.Sockets;
using System;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace Redis.SilverlightClient
{
    public static class RedisSubscriber
    {
        public static IObservable<RedisSubscribeMessage> SubscribeToChannel(string host, int port, string channel)
        {
            return SubscribeToChannel(host, port, channel, Scheduler.Default);
        }

        public static IObservable<RedisSubscribeMessage> SubscribeToChannel(string host, int port, string channel, IScheduler scheduler)
        {
            var wireMessage = string.Format("*2\r\n$9\r\nSUBSCRIBE\r\n${0}\r\n{1}\r\n", channel.Length, channel);
            var receivedMessagesParts = SubscribeToRedisWithMessage(host, port, wireMessage, scheduler).Skip(1);

            return Observable.Create<RedisSubscribeMessage>(observer =>
            {
                var remainder = string.Empty;

                return receivedMessagesParts.ObserveOn(scheduler).Subscribe(
                    part =>
                    {
                        var parseTry = RedisSubscribeMessage.SubscribeMessageParser.TryParse(remainder + part);

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

                            parseTry = RedisSubscribeMessage.SubscribeMessageParser.TryParse(remainder);
                        }
                    },
                    observer.OnError);
            });
        }

        public static IObservable<RedisPatternSubscribeMessage> SubscribeToChannelPattern(
            string host,
            int port,
            string channelPattern)
        {
            return SubscribeToChannelPattern(host, port, channelPattern, Scheduler.Default);
        }

        public static IObservable<RedisPatternSubscribeMessage> SubscribeToChannelPattern(string host, int port, string channelPattern, IScheduler scheduler)
        {
            var wireMessage = string.Format("*2\r\n$10\r\nPSUBSCRIBE\r\n${0}\r\n{1}\r\n", channelPattern.Length, channelPattern);
            var receivedMessagesParts = SubscribeToRedisWithMessage(host, port, wireMessage, scheduler).Skip(1);

            return Observable.Create<RedisPatternSubscribeMessage>(observer =>
            {
                var remainder = string.Empty;

                return receivedMessagesParts.ObserveOn(scheduler).Subscribe(part =>
                {
                    var parseTry = RedisPatternSubscribeMessage.SubscribeMessageParser.TryParse(remainder + part);

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

                        parseTry = RedisPatternSubscribeMessage.SubscribeMessageParser.TryParse(remainder);
                    }
                }, ex => observer.OnError(ex));
            });
        }

        private static IObservable<string> SubscribeToRedisWithMessage(string host, int port, string message, IScheduler scheduler)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var socketAsyncEventArgs = new SocketAsyncEventArgs();

            var redisConnector = new RedisConnector(
                                    () => new SocketDecorator(socket),
                                    () => new SocketAsyncEventArgsDecorator(socketAsyncEventArgs));

            var result = from connection in redisConnector.BuildConnectionToken(host, port, scheduler)
                         let transmitter = new RedisTransmitter(connection)
                         from _ in transmitter.SendMessage(message, scheduler)
                         let receiver = new RedisReceiver(connection)
                         from messageReceived in receiver.Receive(new byte[4096], scheduler)
                         select messageReceived;

            return Observable.Using(() => socket, _ => result);
        }
    }
}
