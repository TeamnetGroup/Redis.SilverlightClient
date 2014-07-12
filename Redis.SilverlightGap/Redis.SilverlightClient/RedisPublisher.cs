using PortableSprache;
using Redis.SilverlightClient.Messages;
using Redis.SilverlightClient.Parsers;
using System;
using System.Reactive.Concurrency;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace Redis.SilverlightClient
{
    internal class RedisPublisher : IRedisPublisher
    {
        private readonly RedisConnection redisConnection;
        private readonly IScheduler scheduler;
        private readonly byte[] buffer;

        public RedisPublisher(RedisConnection redisConnection, IScheduler scheduler)
        {
            if (redisConnection == null)
                throw new ArgumentNullException("redisConnection");

            if (scheduler == null)
                throw new ArgumentNullException("scheduler");

            this.redisConnection = redisConnection;
            this.scheduler = scheduler;
            this.buffer = new byte[4096];
        }
         
        public Task<int> PublishMessage(string channel, string message)
        {
            var callback = new TaskCompletionSource<int>();
            var publishMessage = new RedisPublishMessage(channel, message);

            redisConnection.Inbox.OnNext(async (transmitter, receiver, ex) =>
            {
                if (ex != null)
                {
                    callback.SetException(ex);
                    var tcs = new TaskCompletionSource<bool>();
                    tcs.SetResult(true);
                    await tcs.Task;
                    return;
                }

                await transmitter.SendMessage(publishMessage.ToString(), scheduler).ToTask();
                var response = await receiver.Receive(buffer, scheduler, false).ToTask();

                var pongs = RedisParsersModule.IntegerParser.TryParse(response);
                if (!pongs.WasSuccessful)
                        callback.SetException(new ParseException(string.Format("Invalid integer response for published message: {0}", response)));

                callback.SetResult(pongs.Value);
            });
            return callback.Task;
        }

        public void Dispose()
        {
            redisConnection.Dispose();
        }
    }
}
