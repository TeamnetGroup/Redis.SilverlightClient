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
using Redis.SilverlightClient.Messages;

namespace Redis.NetClientTests
{
    [TestFixture]
    public class RedisPubSubTests
    {
        [Test]
        public async Task CanPublishAndReceiveOnChannel()
        {
            var expected = "CanPublishAndReceiveOnChannel";

            using (var socketConnection = new SocketConnection(TestsSetup.Host, TestsSetup.Port, Scheduler.Immediate))
            {
                using (var publisherConnection = new SocketConnection(TestsSetup.Host, TestsSetup.Port, Scheduler.Immediate))
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

            using (var socketConnection = new SocketConnection(TestsSetup.Host, TestsSetup.Port, Scheduler.Immediate))
            {
                using (var publisherConnection = new SocketConnection(TestsSetup.Host, TestsSetup.Port, Scheduler.Immediate))
                {
                    var subscriber = socketConnection.AsSubscriber();
                    var publisher = publisherConnection.AsPublisher();
                    var matchedReplies = 0;
                    var tcs = new TaskCompletionSource<bool>();
                    var expectedMessagesCount = 512;

                    var messagesChannel = await subscriber.Subscribe("channel1");
                    messagesChannel.Subscribe(message =>
                    {
                        if (string.Compare(message.Content, expected, StringComparison.InvariantCultureIgnoreCase) == 0)
                            matchedReplies += 1;

                        if (matchedReplies == expectedMessagesCount)
                            tcs.SetResult(true);
                    });

                    foreach (var once in Enumerable.Repeat(Unit.Default, expectedMessagesCount))
                    {
                        var result = await publisher.PublishMessage("channel1", expected);
                    }

                    await Task.WhenAny(Task.Delay(1000), tcs.Task);
                    Assert.AreEqual(expectedMessagesCount, matchedReplies);
                }
            }
        }

        [Test]
        public async Task CanPublishAndReceiveOnChannelPattern()
        {
            var expected = "CanPublishAndReceiveOnChannelPattern";

            using (var socketConnection = new SocketConnection(TestsSetup.Host, TestsSetup.Port, Scheduler.Immediate))
            {
                using (var publisherConnection = new SocketConnection(TestsSetup.Host, TestsSetup.Port, Scheduler.Immediate))
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

            using (var socketConnection = new SocketConnection(TestsSetup.Host, TestsSetup.Port, Scheduler.Immediate))
            {
                using (var publisherConnection = new SocketConnection(TestsSetup.Host, TestsSetup.Port, Scheduler.Immediate))
                {
                    var subscriber = socketConnection.AsSubscriber();
                    var publisher = publisherConnection.AsPublisher();
                    var matchedReplies = 0;
                    var tcs = new TaskCompletionSource<bool>();
                    var expectedMessagesCount = 512;

                    var channelPatternMessages = await subscriber.PSubscribe("channel*");
                    channelPatternMessages.Subscribe(message =>
                    {
                        if (string.Compare(message.Content, expected, StringComparison.InvariantCultureIgnoreCase) == 0)
                            matchedReplies += 1;

                        if (matchedReplies == expectedMessagesCount)
                            tcs.SetResult(true);
                    });

                    foreach (var once in Enumerable.Repeat(Unit.Default, expectedMessagesCount))
                    {
                        await publisher.PublishMessage("channel1", expected);
                    }

                    await Task.WhenAny(Task.Delay(1000), tcs.Task);
                    Assert.AreEqual(expectedMessagesCount, matchedReplies);
                }
            }
        }

        [Test]
        public async Task ChannelsObservablesDontThrowExceptionWhenSocketConnectionIsDisposed()
        {
            var subscriptionCompleted = new TaskCompletionSource<bool>();

            using (var socketConnection = new SocketConnection(TestsSetup.Host, TestsSetup.Port, Scheduler.Immediate))
            {
                var subscriber = socketConnection.AsSubscriber();

                var channelPatternMessages = await subscriber.PSubscribe("channel*");

                channelPatternMessages.Subscribe(
                    _ => { }, 
                    ex => {
                        subscriptionCompleted.SetException(ex);
                    },
                    () => { subscriptionCompleted.SetResult(true); });
            }

            try
            {
                var completed = await subscriptionCompleted.Task;
                Assert.IsTrue(completed);
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is RedisException)
                {
                    throw new Exception(((RedisException)ex.InnerException).SocketError.ToString());
                }

                throw ex.InnerException;
            }
        }
    }
}
