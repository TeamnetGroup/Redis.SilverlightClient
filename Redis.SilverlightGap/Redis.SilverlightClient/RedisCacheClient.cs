using PortableSprache;
using Redis.SilverlightClient.Messages;
using Redis.SilverlightClient.Parsers;
using Redis.SilverlightClient.Sockets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace Redis.SilverlightClient
{
    internal class RedisCacheClient : IRedisCacheClient, IDisposable
    {
        private readonly SocketConnection socketConnection;

        public RedisCacheClient(SocketConnection socketConnection)
        {
            if (socketConnection == null)
                throw new ArgumentNullException("socketConnection");

            this.socketConnection = socketConnection;
        }

        public Task SetValue(string key, string value)
        {
            return SetValue(key, value, null);
        }

        public Task SetValue(string key, string value, TimeSpan? ttl)
        {
            var setValueMessage = new RedisSetValueMessage(key, value, ttl);

            return socketConnection.Connection.Take(1).Select(connection =>
            {
                var request = connection.SendMessage(setValueMessage.ToString());
                var response = connection.ReceiveMessage();

                return request.Zip(response, (_, result) => result).Select(result =>
                {
                    var ok = RedisParsersModule.OKParser.TryParse(result);

                    if(!ok.WasSuccessful)
                        throw new ParseException("Unknown SET response: " + result);

                    return ok.Value;
                });
            }).Merge().ToTask();
        }

        public Task<string> GetValue(string key)
        {
            var getValueMessage = new RedisGetValueMessage(key);
            string remainder = string.Empty;

            return socketConnection.Connection.Take(1).Select(connection =>
            {
                return connection.SendMessage(getValueMessage.ToString()).Select(_ => connection);;
            }).Merge(1).Select(connection =>
            {
                return connection.ReceiveMessage().Repeat();
            }).Merge(1).Select(result =>
            {
                remainder += result;
                var getResult = RedisParsersModule.BulkStringParser.Or(RedisParsersModule.NullParser).TryParse(remainder);

                if (!getResult.WasSuccessful)
                {
                    return new Tuple<bool, string>(false, null);
                }
                return new Tuple<bool, string>(true, getResult.Value);
            }).Where(x => x.Item1).Take(1).Select(x => x.Item2).ToTask();
        }

        public Task SetValues(IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            var setValuesMessage = new RedisSetValuesMessage(keyValuePairs);
            
            return socketConnection.Connection.Take(1).Select(connection =>
            {
                var request = connection.SendMessage(setValuesMessage.ToString());
                var response = connection.ReceiveMessage();

                return request.Zip(response, (_, result) => result).Select(result =>
                {
                    var ok = RedisParsersModule.OKParser.TryParse(result);

                    if (!ok.WasSuccessful)
                        throw new ParseException("Unknown MSET response: " + result);

                    return ok.Value;
                });
            }).Merge().ToTask();
        }

        public Task<IEnumerable<string>> GetValues(IEnumerable<string> keys)
        {
            var getValuesMessage = new RedisGetValuesMessage(keys);
            string remainder = string.Empty;    

            return socketConnection.Connection.Take(1).Select(connection =>
            {
                return connection.SendMessage(getValuesMessage.ToString()).Select(_ => connection);
            }).Merge(1).Select(connection =>
            {
                return connection.ReceiveMessage().Repeat();
            }).Merge(1).Select(result =>
            {
                remainder += result;
                var getResult = RedisParsersModule.ArrayOfBulkStringsParser.TryParse(remainder);

                if (!getResult.WasSuccessful)
                {
                    return null;
                }

                return getResult.Value.AsEnumerable();
            }).Where(x => x != null).Take(1).ToTask();
        }

        public void Dispose()
        {
            socketConnection.Dispose();
        }
    }
}
