using Redis.SilverlightClient.Messages;
using Redis.SilverlightClient.Parsers;
using Redis.SilverlightClient.Sockets;
using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace Redis.SilverlightClient
{
    internal class RedisCacheClient : IRedisCacheClient
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

            return socketConnection.GetConnection().Select(connection =>
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
            }).Merge(1).ToTask();
        }

        public Task<string> GetValue(string key)
        {
            var getValueMessage = new RedisGetValueMessage(key);
            string remainder = string.Empty;

            return 
                socketConnection
                    .GetConnection()
                    .Select(connection =>
                        connection.SendMessage(getValueMessage.ToString()))
                    .Merge(1)
                    .Select(_ =>
                        socketConnection
                        .GetConnection()
                        .Select(connection => 
                            connection.ReceiveMessage())
                        .Merge(1)
                        .DoWhile(() => !socketConnection.IsDisposed))
                    .Merge(1).Select(result =>
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
            
            return socketConnection.GetConnection().Select(connection =>
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
            }).Merge(1).ToTask();
        }

        public Task<IEnumerable<string>> GetValues(params string[] keys)
        {
            var getValuesMessage = new RedisGetValuesMessage(keys);
            string remainder = string.Empty;    

            return 
                socketConnection
                    .GetConnection()
                    .Select(connection =>
                        connection.SendMessage(getValuesMessage.ToString()))
                    .Merge(1)
                    .Select(_ =>
                        socketConnection
                            .GetConnection()
                            .Select(connection => 
                                connection.ReceiveMessage())
                            .Merge(1)
                            .DoWhile(() => !socketConnection.IsDisposed))
                    .Merge(1).Select(result =>
                    {
                        remainder += result;
                        var getResult = RedisParsersModule.ArrayOfBulkStringsParser.TryParse(remainder);

                        if (!getResult.WasSuccessful)
                        {
                            return new Tuple<bool, IEnumerable<string>>(false, null);
                        }

                        return new Tuple<bool,IEnumerable<string>>(true, getResult.Value.AsEnumerable());
                    }).Where(x => x.Item1).Take(1).Select(x => x.Item2).ToTask();
        }

        public Task<int> Del(params string[] keys)
        {
            var deleteMessage = new RedisDeleteMessage(keys);

            return socketConnection.GetConnection().Select(connection =>
            {
                var request = connection.SendMessage(deleteMessage.ToString());
                var response = connection.ReceiveMessage();

                return request.Zip(response, (_, result) => result).Select(result =>
                {
                    var pongs = RedisParsersModule.IntegerParser.TryParse(result);

                    if (!pongs.WasSuccessful)
                        throw new ParseException(string.Format("Invalid integer response for published message: {0}", result));

                    return pongs.Value;
                });
            }).Merge(1).ToTask();
        }

        public void Dispose()
        {
            socketConnection.Dispose();
        }
    }
}
