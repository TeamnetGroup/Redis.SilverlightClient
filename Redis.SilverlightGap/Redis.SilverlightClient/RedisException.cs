using System;
using System.Net.Sockets;

namespace Redis.SilverlightClient
{
    public class RedisException : Exception
    {
        public SocketError SocketError { get; private set; }

        public RedisException(SocketError socketError)
        {
            this.SocketError = socketError;
        }

        public override string ToString()
        {
            return SocketError.ToString();
        }
    }
}
