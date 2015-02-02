using System;
using System.Linq;

namespace Redis.SilverlightClient.Messages
{
    public class RedisPatternSubscribeMessage
    {
        private readonly string[] channelPatterns;

        public RedisPatternSubscribeMessage(string[] channelPatterns)
        {
            if (channelPatterns == null || channelPatterns.Length == 0)
                throw new ArgumentException("channelPatterns");

            this.channelPatterns = channelPatterns;
        }

        public string[] ChannelPatterns { get { return channelPatterns; } }

        public override string ToString()
        {
            return string.Format("*{0}\r\n$10\r\nPSUBSCRIBE\r\n", channelPatterns.Length + 1)
                + channelPatterns
                    .Select(channelName =>
                        string.Format("${0}\r\n{1}\r\n", channelName.Length, channelName))
                    .Aggregate((_, __) => _ + __);
        }
    }
}
