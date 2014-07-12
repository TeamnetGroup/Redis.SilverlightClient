using PortableSprache;
using Redis.SilverlightClient.Messages;
using Redis.SilverlightClient.Parsers;
using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace Redis.SilverlightClient
{
    internal class RedisCacheClient : IRedisCacheClient
    {
        private readonly RedisConnection redisConnection;
        private readonly IScheduler scheduler;
        private readonly byte[] buffer;
 
        public RedisCacheClient(RedisConnection redisConnection, IScheduler scheduler)
        {
            if (redisConnection == null)
                throw new ArgumentNullException("redisConnection");

            if (scheduler == null)
                throw new ArgumentNullException("scheduler");

            this.redisConnection = redisConnection;
            this.scheduler = scheduler;
            this.buffer = new byte[4096];
        }

        public Task SetValue(string key, string value)
        {
            return SetValue(key, value, null);
        }

        public Task SetValue(string key, string value, TimeSpan? ttl)
        {
            var callback = new TaskCompletionSource<string>();
            var setValueMessage = new RedisSetValueMessage(key, value, ttl);
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

                await transmitter.SendMessage(setValueMessage.ToString(), scheduler).ToTask();
                var response = await receiver.Receive(buffer, scheduler, false).ToTask();

                var ok = RedisParsersModule.OKParser.TryParse(response);

                if (!ok.WasSuccessful)
                {
                    callback.SetException(new ParseException("Unknown GET response: " + response));
                }
                else
                {
                    callback.SetResult("OK");
                }

            });
            return callback.Task;
        }

        public Task<string> GetValue(string key)
        {
            var callback = new TaskCompletionSource<string>();
            var getValueMessage = new RedisGetValueMessage(key);
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

                await transmitter.SendMessage(getValueMessage.ToString(), scheduler).ToTask();
                var response = await receiver.Receive(buffer, scheduler, false).ToTask();

                var result = RedisParsersModule.BulkStringParser.TryParse(response);

                if (!result.WasSuccessful)
                {
                    var nullResponse = RedisParsersModule.NullParser.TryParse(response);

                    if (nullResponse.WasSuccessful)
                    {
                        callback.SetResult(null);
                    }
                    else
                    {
                        callback.SetException(new ParseException("Unknown GET response: " + response));
                    }
                }
                else
                {
                    callback.SetResult(result.Value);
                }
            });
            return callback.Task;
        }

        public Task SetValues(IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            var callback = new TaskCompletionSource<string>();
            var setValuesMessage = new RedisSetValuesMessage(keyValuePairs, callback);
            redisConnection.Inbox.OnNext(async (transmitter, receiver, ex) =>
            {
                if (ex != null)
                {
                    setValuesMessage.Callback.SetException(ex);
                    var tcs = new TaskCompletionSource<bool>();
                    tcs.SetResult(true);
                    await tcs.Task;
                    return;
                }

                await transmitter.SendMessage(setValuesMessage.ToString(), scheduler).ToTask();
                var response = await receiver.Receive(buffer, scheduler, false).ToTask();

                var ok = RedisParsersModule.OKParser.TryParse(response);

                if (!ok.WasSuccessful)
                {
                    setValuesMessage.Callback.SetException(new ParseException("Unknown GET response: " + response));
                }
                else
                {
                    setValuesMessage.Callback.SetResult("OK");
                }

            });
            return callback.Task;
        }

        public Task<IEnumerable<string>> GetValues(IEnumerable<string> keys)
        {
            var callback = new TaskCompletionSource<IEnumerable<string>>();
            var getValuesMessage = new RedisGetValuesMessage(keys);
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

                await transmitter.SendMessage(getValuesMessage.ToString(), scheduler).ToTask();
                var response = await receiver.Receive(buffer, scheduler, false).ToTask();

                var result = RedisParsersModule.ArrayOfBulkStringsParser.TryParse(response);

                if (!result.WasSuccessful)
                {
                    callback.SetException(new ParseException("Unknown MGET respone: " + response));
                }
                else
                {
                    callback.SetResult(result.Value);
                }
            });
            return callback.Task;
        }

        public void Dispose()
        {
            redisConnection.Dispose();
        }
    }
}
