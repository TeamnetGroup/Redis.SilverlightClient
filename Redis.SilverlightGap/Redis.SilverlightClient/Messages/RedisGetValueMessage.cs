using System;
using System.Windows.Input;

namespace Redis.SilverlightClient.Messages
{
    public class RedisGetValueMessage
    {
        public RedisGetValueMessage(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key");

            this.Key = key;
        }

        public string Key { get; private set; }
        public string Value { get; private set; }

        public override string ToString()
        {
            return string.Format("*2\r\n$3\r\nGET\r\n${0}\r\n{1}\r\n", Key.Length, Key);
        }
    }
}
