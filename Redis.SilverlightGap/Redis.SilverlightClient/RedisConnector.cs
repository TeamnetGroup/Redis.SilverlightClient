using System;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Redis.SilverlightClient
{
    class RedisConnector
    {
        private readonly Func<Socket> socketFactory;
        private readonly Func<SocketAsyncEventArgs> socketEventsFactory;

        public RedisConnector(Func<Socket> socketFactory, Func<SocketAsyncEventArgs> socketEventsFactory)
        {
            if (socketFactory == null)
                throw new ArgumentNullException("socketFactory");

            if (socketEventsFactory == null)
                throw new ArgumentNullException("socketEventsFactory");

            this.socketFactory = socketFactory;
            this.socketEventsFactory = socketEventsFactory;
        }

        public IObservable<SocketAsyncEventArgs> BuildConnectionToken(string host, int port, IScheduler scheduler)
        {
            return Observable.Create<SocketAsyncEventArgs>(observer =>
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

        private void SendNotificationToObserver(IObserver<SocketAsyncEventArgs> observer, SocketAsyncEventArgs socketEvent)
        {
            if (socketEvent.SocketError == SocketError.Success)
            {
                observer.OnNext(socketEvent);
                observer.OnCompleted();
            }
            else
            {
                observer.OnError(new RedisException(socketEvent.SocketError));
            }
        }
    }
}
