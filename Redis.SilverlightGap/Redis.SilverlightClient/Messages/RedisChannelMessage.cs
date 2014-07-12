using PortableSprache;
using Redis.SilverlightClient.Parsers;
using System.Linq;

namespace Redis.SilverlightClient.Messages
{
    public class RedisChannelMessage
    {
        internal static readonly Parser<RedisChannelMessage> SubscribeMessageParser =
            from arrayOfStrings in RedisParsersModule.ArrayOfBulkStringsParser
                .Where(x => x.Length == 3 && x[0] == "message")
            let channelName = arrayOfStrings[1]
            let content = arrayOfStrings[2]
            select new RedisChannelMessage(channelName, content);

        public RedisChannelMessage(string channelName, string content)
        {
            this.ChannelName = channelName;
            this.Content = content;
        }

        public string ChannelName { get; private set; }
        public string Content { get; private set; }
    }
}
