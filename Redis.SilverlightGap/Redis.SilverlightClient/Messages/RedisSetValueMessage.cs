using System;
using System.Windows.Input;

namespace Redis.SilverlightClient.Messages
{
    internal class RedisSetValueMessage
    {
        public RedisSetValueMessage(string key, string value)
            : this(key, value, null) { }

        public RedisSetValueMessage(string key, string value, TimeSpan? ttl)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key");

            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("value");

            this.Key = key;
            this.Value = value;
            this.TTL = ttl;
        }

        public string Key { get; private set; }
        public string Value { get; private set; }
        public TimeSpan? TTL { get; private set; }

        public override string ToString()
        {
            return TTL.HasValue ?
                string.Format("*5\r\n$3\r\nSET\r\n${0}\r\n{1}\r\n${2}\r\n{3}\r\n$2\r\nEX\r\n${4}\r\n{5}\r\n", Key.Length, Key, Value.Length, Value, (int)TTL.Value.TotalSeconds.ToString().Length, (int)TTL.Value.TotalSeconds) :
                string.Format("*3\r\n$3\r\nSET\r\n${0}\r\n{1}\r\n${2}\r\n{3}\r\n", Key.Length, Key, Value.Length, Value);
           
        }
    }
}