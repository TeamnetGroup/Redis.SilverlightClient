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
    public class RedisSubscribeMessage
    {
        public RedisSubscribeMessage(string channelName)
        {
            if (string.IsNullOrEmpty(channelName))
                throw new ArgumentException("channelName");

            this.ChannelName = channelName;
        }

        public string ChannelName { get; private set; }

        public override string ToString()
        {
            return string.Format("*2\r\n$9\r\nSUBSCRIBE\r\n${0}\r\n{1}\r\n", ChannelName.Length, ChannelName);
        }
    }
}
