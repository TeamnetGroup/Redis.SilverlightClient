using System;
using System.Net;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
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
    class SocketAsyncEventArgsDecorator : ISocketAsyncEventArgsDecorator
    {
        private readonly SocketAsyncEventArgs socketAsyncEventArgs;

        public SocketAsyncEventArgsDecorator(SocketAsyncEventArgs socketAsyncEventArgs)
        {
            if (socketAsyncEventArgs == null)
                throw new ArgumentNullException("socketAsyncEventArgs");

            this.socketAsyncEventArgs = socketAsyncEventArgs;
        }

        public IObservable<Unit> Completed
        {
            get
            {
                return Observable
                    .FromEventPattern(socketAsyncEventArgs, "Completed")
                    .Select(_ => Unit.Default);
            }
        }

        public EndPoint RemoteEndPoint
        {
            get
            {
                return socketAsyncEventArgs.RemoteEndPoint;
            }
            set
            {
                socketAsyncEventArgs.RemoteEndPoint = value;
            }
        }

        public SocketError SocketError
        {
            get { return socketAsyncEventArgs.SocketError; }
        }

        public int BytesTransferred
        {
            get { return socketAsyncEventArgs.BytesTransferred; }
        }

        public int Offset
        {
            get { return socketAsyncEventArgs.Offset; }
        }

        public byte[] Buffer
        {
            get { return socketAsyncEventArgs.Buffer; }
        }

        public void SetBuffer(byte[] buffer, int offset, int count)
        {
            socketAsyncEventArgs.SetBuffer(buffer, offset, count);
        }

        public SocketClientAccessPolicyProtocol SocketClientAccessPolicyProtocol
        {
            get
            {
                return socketAsyncEventArgs.SocketClientAccessPolicyProtocol;
            }
            set
            {
                socketAsyncEventArgs.SocketClientAccessPolicyProtocol = value;
            }
        }

        public SocketAsyncEventArgs SocketAsyncEventArgs
        {
            get { return socketAsyncEventArgs; }
        }
    }
}
