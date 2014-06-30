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
    public class RedisGetValueMessage
    {
        public RedisGetValueMessage(string key, TaskCompletionSource<string> callback)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key");

            if (callback == null)
                throw new ArgumentNullException("callback");

            this.Key = key;
            this.Callback = callback;
        }

        public string Key { get; private set; }
        public string Value { get; private set; }
        public TaskCompletionSource<string> Callback { get; private set; }

        public override string ToString()
        {
            return string.Format("*2\r\n$3\r\nGET\r\n${0}\r\n{1}\r\n", Key.Length, Key);
        }
    }
}
