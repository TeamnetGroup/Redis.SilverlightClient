using System.Net.Sockets;
namespace Redis.SilverlightClient.Sockets
{
    interface ISocket
    {
        bool ConnectAsync(ISocketAsyncEventArgsDecorator socketEvent);
        bool SendAsync(ISocketAsyncEventArgsDecorator socketEvent);
        bool ReceiveAsync(ISocketAsyncEventArgsDecorator socketEvent);
    }
}
