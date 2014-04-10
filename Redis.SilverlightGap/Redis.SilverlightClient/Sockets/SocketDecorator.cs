using System;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Redis.SilverlightClient.Sockets
{
    class SocketDecorator : ISocketDecorator
    {
        private readonly Socket socket;

        public SocketDecorator(Socket socket)
        {
            if (socket == null)
                throw new ArgumentNullException("socket");

            this.socket = socket;
        }

        public bool ConnectAsync(ISocketAsyncEventArgsDecorator socketEvent)
        {
            return socket.ConnectAsync(socketEvent.SocketAsyncEventArgs);
        }

        public bool SendAsync(ISocketAsyncEventArgsDecorator socketEvent)
        {
            return socket.SendAsync(socketEvent.SocketAsyncEventArgs);
        }

        public bool ReceiveAsync(ISocketAsyncEventArgsDecorator socketEvent)
        {
            return socket.ReceiveAsync(socketEvent.SocketAsyncEventArgs);
        }

        public Socket Socket
        {
            get { return this.socket; }
        }
    }
}
