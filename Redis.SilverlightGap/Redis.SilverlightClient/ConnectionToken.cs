using Redis.SilverlightClient.Sockets;
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
    class ConnectionToken
    {
        private readonly ISocketDecorator socket;
        private readonly ISocketAsyncEventArgsDecorator socketEvent;

        public ISocketDecorator Socket { get { return socket; } }
        public ISocketAsyncEventArgsDecorator SocketEvent { get { return socketEvent; } }

        public ConnectionToken(ISocketDecorator socket, ISocketAsyncEventArgsDecorator socketEvent)
        {
            if (socket == null)
                throw new ArgumentNullException("socket");

            if (socketEvent == null)
                throw new ArgumentNullException("socketEvent");

            this.socket = socket;
            this.socketEvent = socketEvent;
        }
    }
}
