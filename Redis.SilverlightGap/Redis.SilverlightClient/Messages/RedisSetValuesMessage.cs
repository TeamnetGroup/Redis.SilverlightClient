using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redis.SilverlightClient.Messages
{
    public class RedisSetValuesMessage
    {
        public RedisSetValuesMessage(IEnumerable<KeyValuePair<string, string>> keyValuePairs, TaskCompletionSource<string> callback)
        {
            if (keyValuePairs == null)
                throw new ArgumentNullException("keyValuePairs");

            if (callback == null)
                throw new ArgumentNullException("callback");

            KeyValuePairs = keyValuePairs;
            Callback = callback;
        }

        public IEnumerable<KeyValuePair<string,string>> KeyValuePairs { get; private set; }
        public TaskCompletionSource<string> Callback { get; private set; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            var arrayLength = 2 * KeyValuePairs.Count() + 1;
            builder.AppendFormat("*{0}\r\n$4\r\nMSET\r\n", arrayLength.ToString());

            foreach(var keyValuePair in KeyValuePairs)
                builder.AppendFormat("${0}\r\n{1}\r\n${2}\r\n{3}\r\n", keyValuePair.Key.Length, keyValuePair.Key, keyValuePair.Value.Length, keyValuePair.Value);

            return builder.ToString();
        }
    }
}
