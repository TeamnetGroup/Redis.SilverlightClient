using System;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Redis.SilverlightClient.Sockets
{
    public class SocketConnector 
    {
        private readonly Func<Socket> socketFactory;
        private readonly Func<SocketAsyncEventArgs> socketEventsFactory;

        public SocketConnector(Func<Socket> socketFactory, Func<SocketAsyncEventArgs> socketEventsFactory)
        {
            if (socketFactory == null)
                throw new ArgumentNullException("socketFactory");

            if (socketEventsFactory == null)
                throw new ArgumentNullException("socketEventsFactory");

            this.socketFactory = socketFactory;
            this.socketEventsFactory = socketEventsFactory;
        }

        public IObservable<Socket> Connect(string host, int port, IScheduler scheduler)
        {
            return Observable.Create<Socket>(observer =>
            {
                var disposable = new CompositeDisposable();

                var socket = socketFactory();
                var socketEvent = socketEventsFactory();

                socketEvent.RemoteEndPoint = new DnsEndPoint(host, port);
                socketEvent.SocketClientAccessPolicyProtocol = System.Net.Sockets.SocketClientAccessPolicyProtocol.Tcp;
                socketEvent.UserToken = socket;

                var disposableEventSubscription = socketEvent.CompletedObservable().ObserveOn(scheduler).Subscribe(_ =>
                {
                    SendNotificationToObserver(observer, socketEvent);
                });

                var disposableActions = scheduler.Schedule(() =>
                {    
                    if (!socket.ConnectAsync(socketEvent))
                    {
                        SendNotificationToObserver(observer, socketEvent);
                    }
                });

                disposable.Add(disposableEventSubscription);
                disposable.Add(disposableActions);

                return disposable;
            });
        }

        private void SendNotificationToObserver(IObserver<Socket> observer, SocketAsyncEventArgs socketEvent)
        {
            if (socketEvent.SocketError == SocketError.Success)
            {
                observer.OnNext((Socket)socketEvent.UserToken);
                observer.OnCompleted();
            }
            else
            {
                observer.OnError(new RedisException(socketEvent.SocketError));
            }
        }
    }
}
