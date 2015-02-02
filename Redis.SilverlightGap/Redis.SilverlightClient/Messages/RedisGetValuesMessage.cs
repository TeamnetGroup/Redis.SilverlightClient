using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Redis.SilverlightClient.Messages
{
    public class RedisGetValuesMessage
    {
        public RedisGetValuesMessage(params string[] keys)
        {
            if (keys == null)
                throw new ArgumentNullException("keys");

            if (keys.Length == 0)
                throw new ArgumentException("keys");

            this.Keys = keys;
        }

        public string[] Keys { get; private set; }
        public string Value { get; private set; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            var arrayLength = Keys.Length + 1;
            builder.AppendFormat("*{0}\r\n$4\r\nMGET\r\n", arrayLength.ToString());

            foreach (var key in Keys)
                builder.AppendFormat("${0}\r\n{1}\r\n", key.Length, key);

            return builder.ToString();
        }
    }
}
