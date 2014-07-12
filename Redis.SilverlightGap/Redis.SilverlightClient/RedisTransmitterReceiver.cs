using System;

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
