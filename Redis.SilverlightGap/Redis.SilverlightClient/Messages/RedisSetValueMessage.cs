using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Redis.SilverlightClient.Messages
{
    internal class RedisSetValueMessage
    {
        public RedisSetValueMessage(string key, string value, TaskCompletionSource<string> callback)
            : this(key, value, null, callback) { }

        public RedisSetValueMessage(string key, string value, TimeSpan? ttl, TaskCompletionSource<string> callback)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key");

            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("value");

            if (callback == null)
                throw new ArgumentNullException("callback");

            this.Key = key;
            this.Value = value;
            this.Callback = callback;
            this.TTL = ttl;
        }

        public string Key { get; private set; }
        public string Value { get; private set; }
        public TimeSpan? TTL { get; private set; }
        public TaskCompletionSource<string> Callback { get; private set; }

        public override string ToString()
        {
            return TTL.HasValue ?
                string.Format("*5\r\n$3\r\nSET\r\n${0}\r\n{1}\r\n${2}\r\n{3}\r\n$2\r\nEX\r\n${4}\r\n{5}\r\n", Key.Length, Key, Value.Length, Value, (int)TTL.Value.TotalSeconds.ToString().Length, (int)TTL.Value.TotalSeconds) :
                string.Format("*3\r\n$3\r\nSET\r\n${0}\r\n{1}\r\n${2}\r\n{3}\r\n", Key.Length, Key, Value.Length, Value);
           
        }
    }
}