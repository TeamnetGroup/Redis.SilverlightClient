using System;
using System.Net.Sockets;
using System.Reactive.Linq;

namespace Redis.SilverlightClient.Sockets
{
    public static class SocketAsyncEventArgsExtensions
    {
        public static IObservable<SocketAsyncEventArgs> CompletedObservable(this SocketAsyncEventArgs eventArgs)
        {
            return Observable
                    .FromEventPattern<SocketAsyncEventArgs>(
                        h => eventArgs.Completed += h,
                        h => eventArgs.Completed -= h)
                    .Select(x => x.EventArgs);
        }
    }
}
