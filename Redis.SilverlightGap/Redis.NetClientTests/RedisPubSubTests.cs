using NUnit.Framework;
using Redis.SilverlightClient.Sockets;
using Redis.SilverlightClient;
using RedisIntegration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Reactive.Subjects;
using System.Reactive;

namespace Redis.NetClientTests
{
    [TestFixture]
    public class RedisPubSubTests
    {
        [Test]
        public async Task CanPublishAndReceiveOnChannel()
        {
            var expected = "CanPublishAndReceiveOnChannel";

            using (var socketConnection = new SocketConnection(TestsSetup.Host, TestsSetup.Port, Scheduler.Default))
            {
                using (var publisherConnection = new SocketConnection(TestsSetup.Host, TestsSetup.Port, Scheduler.Default))
                {
                    var subscriber = socketConnection.AsSubscriber();
                    var publisher = publisherConnection.AsPublisher();

                    var messagesChannel = await subscriber.Subscribe("channel1");
                    var replaySubject = new ReplaySubject<string>();
                    messagesChannel.Take(1).Select(x => x.Content).Subscribe(replaySubject);

                    await publisher.PublishMessage("channel1", expected);
                    var result = await replaySubject.FirstAsync();

                    Assert.AreEqual(expected, result); 
                }
            }
        }

        [Test]
        public async Task CanPublishAndReceiveMultipleMessagesOnChannel()
        {
            var expected = "CanPublishAndReceiveOnChannel";

            using (var socketConnection = new SocketConnection(TestsSetup.Host, TestsSetup.Port, Scheduler.Default))
            {
                using (var publisherConnection = new SocketConnection(TestsSetup.Host, TestsSetup.Port, Scheduler.Default))
                {
                    var subscriber = socketConnection.AsSubscriber();
                    var publisher = publisherConnection.AsPublisher();

                    var messagesChannel = await subscriber.Subscribe("channel1");
                    var replaySubject = new ReplaySubject<string>();
                    messagesChannel.Select(x => x.Content).Subscribe(replaySubject);

                    foreach (var once in Enumerable.Repeat(Unit.Default, 512))
                    {
                        await publisher.PublishMessage("channel1", expected);
                    }

                    var result = replaySubject.Take(512).ToEnumerable();

                    Assert.IsTrue(result.All(x => x == expected));
                }
            }
        }

        [Test]
        public async Task CanPublishAndReceiveOnChannelPattern()
        {
            var expected = "CanPublishAndReceiveOnChannelPattern";

            using (var socketConnection = new SocketConnection(TestsSetup.Host, TestsSetup.Port, Scheduler.Default))
            {
                using (var publisherConnection = new SocketConnection(TestsSetup.Host, TestsSetup.Port, Scheduler.Default))
                {
                    var subscriber = socketConnection.AsSubscriber();
                    var publisher = publisherConnection.AsPublisher();

                    var channelPatternMessages = await subscriber.PSubscribe("channel*");
                    var replaySubject = new ReplaySubject<string>();
                    channelPatternMessages.Take(1).Select(x => x.Content).Subscribe(replaySubject);

                    await publisher.PublishMessage("channel1", expected);
                    var result = await replaySubject.FirstAsync();

                    Assert.AreEqual(expected, result);
                }
            }
        }

        [Test]
        public async Task CanPublishAndReceiveMultipleMessagesOnChannelPattern()
        {
            var expected = "CanPublishAndReceiveMultipleMessagesOnChannelPattern";

            using (var socketConnection = new SocketConnection(TestsSetup.Host, TestsSetup.Port, Scheduler.Default))
            {
                using (var publisherConnection = new SocketConnection(TestsSetup.Host, TestsSetup.Port, Scheduler.Default))
                {
                    var subscriber = socketConnection.AsSubscriber();
                    var publisher = publisherConnection.AsPublisher();

                    var channelPatternMessages = await subscriber.PSubscribe("channel*");
                    var replaySubject = new ReplaySubject<string>();
                    channelPatternMessages.Select(x => x.Content).Subscribe(replaySubject);

                    foreach (var once in Enumerable.Repeat(Unit.Default, 512))
                    {
                        await publisher.PublishMessage("channel1", expected);
                    }

                    var result = replaySubject.Take(512).ToEnumerable();

                    Assert.IsTrue(result.All(x => x == expected));
                }
            }
        }
    }
}
