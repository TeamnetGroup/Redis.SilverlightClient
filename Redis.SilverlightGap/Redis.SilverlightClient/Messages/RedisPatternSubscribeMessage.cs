using System;
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
        public RedisPatternSubscribeMessage(string channelPattern)
        {
            if (string.IsNullOrEmpty(channelPattern))
                throw new ArgumentException("channelPattern");

            this.ChannelPattern = channelPattern;
        }

        public string ChannelPattern { get; private set; }

        public override string ToString()
        {
            return string.Format("*2\r\n$10\r\nPSUBSCRIBE\r\n${0}\r\n{1}\r\n", ChannelPattern.Length, ChannelPattern);
        }
    }
}
