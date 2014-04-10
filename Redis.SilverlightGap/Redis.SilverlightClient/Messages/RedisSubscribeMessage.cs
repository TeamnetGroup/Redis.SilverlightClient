using PortableSprache;
using System.Linq;

namespace Redis.SilverlightClient.Messages
{
    public class RedisSubscribeMessage
    {
        internal static readonly Parser<RedisSubscribeMessage> SubscribeMessageParser =
            from star in Parse.Char('*')
            from numberOfArgs in Parse.Digit.Many()
            from newLine in Parse.String("\r\n")
            from subscribeMessageToken in Parse.String("$7\r\nmessage\r\n")
            from dollar in Parse.Char('$')
            from channelNameLength in Parse.Digit.Many()
            from newLine2 in Parse.String("\r\n")
            from channelName in Parse.AnyChar.Until(Parse.String("\r\n"))
            from dollar2 in Parse.Char('$')
            from contentLength in Parse.Digit.Many()
            from newLine4 in Parse.String("\r\n")
            from content in Parse.AnyChar.Until(Parse.String("\r\n"))
            select new RedisSubscribeMessage(
                new string(channelName.ToArray()), 
                new string(content.ToArray()));

        public RedisSubscribeMessage(string channelName, string content)
        {
            this.ChannelName = channelName;
            this.Content = content;
        }

        public string ChannelName { get; private set; }
        public string Content { get; private set; }
    }
}
