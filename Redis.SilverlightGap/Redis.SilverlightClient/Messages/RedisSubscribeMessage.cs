using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Linq;
using System.Collections.ObjectModel;

namespace Redis.SilverlightClient.Messages
{
    public class RedisSubscribeMessage
    {
        private readonly string[] channelNames;

        public RedisSubscribeMessage(string[] channelNames)
        {
            if (channelNames == null)
                throw new ArgumentNullException("channelNames");

            if (channelNames.Length == 0)
                throw new ArgumentException("channelNames");

            this.channelNames = channelNames;
        }

        public string[] ChannelNames { get { return channelNames; } }

        public override string ToString()
        {
            return string.Format("*{0}\r\n$9\r\nSUBSCRIBE\r\n", channelNames.Length + 1)
                + channelNames
                    .Select(channelName =>
                        string.Format("${0}\r\n{1}\r\n", channelName.Length, channelName))
                    .Aggregate((_, __) => _ + __);
        }
    }
}
