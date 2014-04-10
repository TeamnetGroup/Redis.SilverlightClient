using PortableSprache;
using Redis.SilverlightClient.Parsers;
using System.Linq;

namespace Redis.SilverlightClient.Messages
{
    public class RedisSubscribeMessage
    {
        internal static readonly Parser<RedisSubscribeMessage> SubscribeMessageParser =
            from arrayOfStrings in RedisParsersModule.ArrayOfBulkStringsParser
                .Where(x => x.Length == 3 && x[0] == "message")
            let channelName = arrayOfStrings[1]
            let content = arrayOfStrings[2]
            select new RedisSubscribeMessage(channelName, content);

        public RedisSubscribeMessage(string channelName, string content)
        {
            this.ChannelName = channelName;
            this.Content = content;
        }

        public string ChannelName { get; private set; }
        public string Content { get; private set; }
    }
}
