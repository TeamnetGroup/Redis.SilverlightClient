using System;
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
        private readonly SocketAsyncEventArgs connectedSocketToken;

        public RedisTransmitter(SocketAsyncEventArgs connectedSocketToken)
        {
            if (connectedSocketToken == null)
                throw new ArgumentNullException("connectionToken");

            this.connectedSocketToken = connectedSocketToken;
        }

        public IObservable<Unit> SendMessage(string message, IScheduler scheduler)
        {
            return Observable.Create<Unit>(observer =>
            {
                var disposable = new CompositeDisposable();
                var buffer = Encoding.UTF8.GetBytes(message);

                var disposableCompletedSubscription = connectedSocketToken.CompletedObservable().ObserveOn(scheduler).Subscribe(_ =>
                {
                    SendNotificationToObserver(observer, connectedSocketToken);
                });

                var disposableActions = scheduler.Schedule(() =>
                {
                    connectedSocketToken.SetBuffer(buffer, 0, buffer.Length);
                    var socket = connectedSocketToken.UserToken as Socket;
                    if (!socket.SendAsync(connectedSocketToken))
                    {
                        SendNotificationToObserver(observer, connectedSocketToken);
                    }
                });

                disposable.Add(disposableCompletedSubscription);
                disposable.Add(disposableActions);

                return disposable;
            });
        }

        private void SendNotificationToObserver(IObserver<Unit> observer, SocketAsyncEventArgs socketEvent)
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
