using System;
using System.Net;
using System.Net.Sockets;
using System.Reactive;

namespace Redis.SilverlightClient.Sockets
{
    interface ISocketAsyncEventArgs
    {
        IObservable<Unit> Completed { get; }
        EndPoint RemoteEndPoint { get; set; }
        SocketError SocketError { get; }
        int BytesTransferred { get; }
        int Offset { get; }
        byte[] Buffer { get; }
        void SetBuffer(byte[] buffer, int offset, int count);
        SocketClientAccessPolicyProtocol SocketClientAccessPolicyProtocol { get; set; }
    }
}
