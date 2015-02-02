using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redis.SilverlightClient.Messages
{
    public class RedisDeleteMessage
    {
        public RedisDeleteMessage(params string[] keys)
        {
            if (keys == null)
                throw new ArgumentNullException("keys");

            if (keys.Length == 0)
                throw new ArgumentException("keys");

            Keys = keys;
        }

        public string[] Keys { get; private set; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            var arrayLength = Keys.Count() + 1;
            builder.AppendFormat("*{0}\r\n$3\r\nDEL\r\n", arrayLength.ToString());

            foreach(var key in Keys)
                builder.AppendFormat("${0}\r\n{1}\r\n", key.Length, key);

            return builder.ToString();
        }
    }
}
