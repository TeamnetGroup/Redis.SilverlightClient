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
    interface ISocketAsyncEventArgsDecorator : ISocketAsyncEventArgs
    {
        SocketAsyncEventArgs SocketAsyncEventArgs { get; }
    }
}
