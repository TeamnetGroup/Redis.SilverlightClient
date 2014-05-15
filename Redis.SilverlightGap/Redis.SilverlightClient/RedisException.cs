using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

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
