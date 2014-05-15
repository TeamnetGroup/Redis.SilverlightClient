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

namespace Redis.SilverlightClient
{
    internal class RedisTransmitterReceiver
    {
        public RedisTransmitterReceiver(RedisTransmitter transmitter, RedisReceiver receiver)
        {
            if (transmitter == null)
                throw new ArgumentNullException("transmitter");

            if (receiver == null)
                throw new ArgumentNullException("receiver");

            Transmitter = transmitter;
            Receiver = receiver;
        }

        public RedisTransmitter Transmitter { get; private set; }
        public RedisReceiver Receiver { get; private set; }
    }
}
