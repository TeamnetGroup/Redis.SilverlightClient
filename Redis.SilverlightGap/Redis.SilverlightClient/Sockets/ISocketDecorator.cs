using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Redis.SilverlightClient.Sockets
{
    interface ISocketDecorator : ISocket
    {
        Socket Socket { get; }
    }
}
