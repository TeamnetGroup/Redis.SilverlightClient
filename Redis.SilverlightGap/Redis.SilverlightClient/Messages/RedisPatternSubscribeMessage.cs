using PortableSprache;
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
            from star in Parse.Char('*')
            from numberOfArgs in Parse.Digit.Many()
            from newLine1 in Parse.String("\r\n")
            from subscribeMessageToken in Parse.String("$8\r\npmessage\r\n")
            from dollar in Parse.Char('$')
            from patternNameLength in Parse.Digit.Many()
            from newLine2 in Parse.String("\r\n")
            from patternName in Parse.AnyChar.Until(Parse.String("\r\n"))
            from dollar2 in Parse.Char('$')
            from channelNameLength in Parse.Digit.Many()
            from newLine3 in Parse.String("\r\n")
            from channelName in Parse.AnyChar.Until(Parse.String("\r\n"))
            from dollar3 in Parse.Char('$')
            from contentLength in Parse.Digit.Many()
            from newLine4 in Parse.String("\r\n")
            from content in Parse.AnyChar.Until(Parse.String("\r\n"))
            select new RedisPatternSubscribeMessage(
                new string(patternName.ToArray()),
                new string(channelName.ToArray()), 
                new string(content.ToArray()));

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
