using PortableSprache;
using Redis.SilverlightClient.Parsers;
using System;
using System.Threading.Tasks;

namespace Redis.SilverlightClient.Messages
{
    internal class RedisPublishMessage
    {
        public RedisPublishMessage(string channelName, string message)
        {
            if (string.IsNullOrEmpty(channelName))
                throw new ArgumentException("channelName");

            if (string.IsNullOrEmpty(message))
                throw new ArgumentException("message");

            this.ChannelName = channelName;
            this.Message = message;
        }

        public string ChannelName { get; private set; }
        public string Message { get; private set; }

        public override string ToString()
        {
            return string.Format("*3\r\n$7\r\nPUBLISH\r\n${0}\r\n{1}\r\n${2}\r\n{3}\r\n", ChannelName.Length, ChannelName, Message.Length, Message);
        }
    }
}
