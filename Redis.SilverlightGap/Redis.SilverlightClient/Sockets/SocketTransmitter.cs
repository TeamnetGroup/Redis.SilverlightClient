using System;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;

namespace Redis.SilverlightClient.Sockets
{
    public class SocketTransmitter
    {
        public IObservable<Unit> SendMessage(Socket connectedSocket, SocketAsyncEventArgs socketEventArgs, IScheduler scheduler, string message)
        {
            if (connectedSocket == null)
                throw new ArgumentNullException("connectedSocket");

            if (socketEventArgs == null)
                throw new ArgumentNullException("socketEventArgs");

            return Observable.Create<Unit>(observer =>
            {
                var disposable = new CompositeDisposable();
                var buffer = Encoding.UTF8.GetBytes(message);

                var disposableCompletedSubscription = socketEventArgs.CompletedObservable().ObserveOn(scheduler).Subscribe(_ =>
                {
                    SendNotificationToObserver(observer, socketEventArgs);
                });

                var disposableActions = scheduler.Schedule(() =>
                {
                    socketEventArgs.SetBuffer(buffer, 0, buffer.Length);
                    if (!connectedSocket.SendAsync(socketEventArgs))
                    {
                        SendNotificationToObserver(observer, socketEventArgs);
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
