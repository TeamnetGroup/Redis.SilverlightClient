using Redis.SilverlightClient.Sockets;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;

namespace Redis.SilverlightClient
{
    class RedisTransmitter
    {
        private readonly ConnectionToken connectionToken;

        public RedisTransmitter(ConnectionToken connectionToken)
        {
            if (connectionToken == null)
                throw new ArgumentNullException("connectionToken");

            this.connectionToken = connectionToken;
        }

        public IObservable<Unit> SendMessage(string message, IScheduler scheduler)
        {
            return Observable.Create<Unit>(observer =>
            {
                var disposable = new CompositeDisposable();
                var buffer = Encoding.UTF8.GetBytes(message);
                connectionToken.SocketEvent.SetBuffer(buffer, 0, buffer.Length);

                var disposableCompletedSubscription = connectionToken.SocketEvent.Completed.Subscribe(_ =>
                {
                    SendNotificationToObserver(observer, connectionToken.SocketEvent);
                });

                var disposableActions = scheduler.Schedule(() =>
                {
                    if (!connectionToken.Socket.SendAsync(connectionToken.SocketEvent))
                    {
                        SendNotificationToObserver(observer, connectionToken.SocketEvent);
                    }
                });

                disposable.Add(disposableCompletedSubscription);
                disposable.Add(disposableActions);

                return disposable;
            });
        }

        private void SendNotificationToObserver(IObserver<Unit> observer, ISocketAsyncEventArgs socketEvent)
        {
            if (socketEvent.SocketError == SocketError.Success)
            {
                observer.OnNext(Unit.Default);
                observer.OnCompleted();
            }
            else
            {
                observer.OnError(new RedisException(socketEvent.SocketError));
            }
        }
    }
}
