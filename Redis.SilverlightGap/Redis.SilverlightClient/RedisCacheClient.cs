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

namespace Redis.SilverlightClient
{
    internal class RedisCacheClient : IRedisCacheClient, IDisposable
    {
        private readonly SocketConnection socketConnection;
        private readonly byte[] buffer;

        public RedisCacheClient(SocketConnection socketConnection)
        {
            if (socketConnection == null)
                throw new ArgumentNullException("socketConnection");

            this.socketConnection = socketConnection;
            this.buffer = new byte[4096];
        }

        public Task SetValue(string key, string value)
        {
            return SetValue(key, value, null);
        }

        public Task SetValue(string key, string value, TimeSpan? ttl)
        {
            var setValueMessage = new RedisSetValueMessage(key, value, ttl);

            return socketConnection.Connection.Select(connection =>
            {
                var request = connection.Item1.SendMessage(setValueMessage.ToString(), socketConnection.Scheduler);
                var response = connection.Item2.Receive(buffer, socketConnection.Scheduler);

                return request.Zip(response, (_, result) => result).Select(result =>
                {
                    var ok = RedisParsersModule.OKParser.TryParse(result);

                    if(!ok.WasSuccessful)
                        throw new ParseException("Unknown GET response: " + result);

                    return ok.Value;
                });
            }).Merge().ToTask();
        }

        public Task<string> GetValue(string key)
        {
            var getValueMessage = new RedisGetValueMessage(key);

            return socketConnection.Connection.Select(connection =>
            {
                var request = connection.Item1.SendMessage(getValueMessage.ToString(), socketConnection.Scheduler);
                var response = connection.Item2.Receive(buffer, socketConnection.Scheduler);

                return request.Zip(response, (_, result) => result).Select(result =>
                {
                    var getResult = RedisParsersModule.BulkStringParser.TryParse(result);

                    if (!getResult.WasSuccessful)
                    {
                        var nullResult = RedisParsersModule.NullParser.TryParse(result);

                        if (!nullResult.WasSuccessful)
                            throw new ParseException("Unknown GET response: " + result);

                        return null;
                    }

                    return getResult.Value;
                });
            }).Merge().ToTask();
        }

        public Task SetValues(IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            var setValuesMessage = new RedisSetValuesMessage(keyValuePairs);
            
            return socketConnection.Connection.Select(connection =>
            {
                var request = connection.Item1.SendMessage(setValuesMessage.ToString(), socketConnection.Scheduler);
                var response = connection.Item2.Receive(buffer, socketConnection.Scheduler);

                return request.Zip(response, (_, result) => result).Select(result =>
                {
                    var ok = RedisParsersModule.OKParser.TryParse(result);

                    if (!ok.WasSuccessful)
                        throw new ParseException("Unknown GET response: " + result);

                    return ok.Value;
                });
            }).Merge().ToTask();
        }

        public Task<IEnumerable<string>> GetValues(IEnumerable<string> keys)
        {
            var getValuesMessage = new RedisGetValuesMessage(keys);

            return socketConnection.Connection.Select(connection =>
            {
                var request = connection.Item1.SendMessage(getValuesMessage.ToString(), socketConnection.Scheduler);
                var response = connection.Item2.Receive(buffer, socketConnection.Scheduler);

                return request.Zip(response, (_, result) => result).Select(result =>
                {
                    var resultValues = RedisParsersModule.ArrayOfBulkStringsParser.TryParse(result);

                    if (!resultValues.WasSuccessful)
                        new ParseException("Unknown MGET respone: " + result);

                    return resultValues.Value;
                });
            }).Merge().Select(array => array.AsEnumerable()).ToTask();
        }

        public void Dispose()
        {
            socketConnection.Dispose();
        }
    }
}
