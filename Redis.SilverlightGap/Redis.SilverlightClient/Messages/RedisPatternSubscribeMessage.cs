using PortableSprache;
using Redis.SilverlightClient.Parsers;
using System;
using System.Linq;
using System.Net;
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
    public class RedisPatternSubscribeMessage
    {
        internal static readonly Parser<RedisPatternSubscribeMessage> SubscribeMessageParser =
            from arrayOfStrings in RedisParsersModule.ArrayOfBulkStringsParser
                .Where(x => x.Length == 4 && x[0] == "pmessage")
            let patternName = arrayOfStrings[0]
            let channelName = arrayOfStrings[1]
            let content = arrayOfStrings[2]
            select new RedisPatternSubscribeMessage(patternName, channelName, content);

        public RedisPatternSubscribeMessage(string pattern, string channelName, string content)
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
