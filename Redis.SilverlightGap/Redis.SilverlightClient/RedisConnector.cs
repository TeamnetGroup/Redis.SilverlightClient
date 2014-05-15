using Redis.SilverlightClient.Sockets;
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
        private readonly Func<ISocketDecorator> socketFactory;
        private readonly Func<ISocketAsyncEventArgsDecorator> socketEventsFactory;

        public RedisConnector(Func<ISocketDecorator> socketFactory, Func<ISocketAsyncEventArgsDecorator> socketEventsFactory)
        {
            if (socketFactory == null)
                throw new ArgumentNullException("socketFactory");

            if (socketEventsFactory == null)
                throw new ArgumentNullException("socketEventsFactory");

            this.socketFactory = socketFactory;
            this.socketEventsFactory = socketEventsFactory;
        }

        public IObservable<ConnectionToken> BuildConnectionToken(string host, int port, IScheduler scheduler)
        {
            return Observable.Create<ConnectionToken>(observer =>
            {
                var disposable = new CompositeDisposable();

                var socket = socketFactory();
                var socketEvent = socketEventsFactory();

                socketEvent.RemoteEndPoint = new DnsEndPoint(host, port);
                socketEvent.SocketClientAccessPolicyProtocol = System.Net.Sockets.SocketClientAccessPolicyProtocol.Tcp;

                var connectionToken = new ConnectionToken(socket, socketEvent);

                var disposableEventSubscription = socketEvent.Completed.ObserveOn(scheduler).Subscribe(_ =>
                {
                    SendNotificationToObserver(observer, socketEvent, connectionToken);
                });

                var disposableActions = scheduler.Schedule(() =>
                {    
                    if (!socket.ConnectAsync(socketEvent))
                    {
                        SendNotificationToObserver(observer, socketEvent, connectionToken);
                    }
                });

                disposable.Add(disposableEventSubscription);
                disposable.Add(disposableActions);

                return disposable;
            });
        }

        private void SendNotificationToObserver(IObserver<ConnectionToken> observer, ISocketAsyncEventArgs socketEvent, ConnectionToken connectionToken)
        {
            if (socketEvent.SocketError == SocketError.Success)
            {
                observer.OnNext(connectionToken);
                observer.OnCompleted();
            }
            else
            {
                observer.OnError(new RedisException(socketEvent.SocketError));
            }
        }
    }
}
