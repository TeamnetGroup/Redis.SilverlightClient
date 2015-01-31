using PortableSprache;
using Redis.SilverlightClient.Parsers;

namespace Redis.SilverlightClient.Messages
{
    public class RedisChannelPatternMessage
    {
        internal static readonly Parser<RedisChannelPatternMessage> RedisChannelPatternMessageParser =
            from arrayOfStrings in RedisParsersModule.ArrayOfBulkStringsParser
                .Where(x => x.Length == 4 && x[0] == "pmessage")
            let patternName = arrayOfStrings[1]
            let channelName = arrayOfStrings[2]
            let content = arrayOfStrings[3]
            select new RedisChannelPatternMessage(patternName, channelName, content);

        public RedisChannelPatternMessage(string pattern, string channelName, string content)
        {
            this.Pattern = pattern;
            this.ChannelName = channelName;
            this.Content = content;
        }

        public string Pattern { get; private set; }
        public string ChannelName { get; private set; }
        public string Content { get; private set; }
    }
}
