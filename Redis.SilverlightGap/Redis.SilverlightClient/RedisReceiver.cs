using System;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace Redis.SilverlightClient
{
    class RedisReceiver
    {
        private readonly SocketAsyncEventArgs connectedSocketToken;

        public RedisReceiver(SocketAsyncEventArgs connectedSocketToken)
        {
            if (connectedSocketToken == null || connectedSocketToken.UserToken == null)
                throw new ArgumentNullException("connectedSocketToken");

            this.connectedSocketToken = connectedSocketToken;
        }

        public IObservable<string> Receive(byte[] buffer, IScheduler scheduler, bool repeat)
        {
            return Observable.Create<string>(observer =>
                {
                    var disposable = new CompositeDisposable();
                    var subject = new Subject<Unit>();

                    var disposableEventSubscription = connectedSocketToken.CompletedObservable().ObserveOn(scheduler).Subscribe(_ =>
                    {
                        if (SendNotificationToObserver(observer, connectedSocketToken, repeat))
                        {
                            if (repeat)
                            {
                                subject.OnNext(Unit.Default);
                            }
                        }
                    });

                    var disposableActions = subject.ObserveOn(scheduler).Subscribe(_ =>
                    {
                        connectedSocketToken.SetBuffer(buffer, 0, buffer.Length);
                        var socket = connectedSocketToken.UserToken as Socket;
                        if (!socket.ReceiveAsync(connectedSocketToken))
                        {
                            if (SendNotificationToObserver(observer, connectedSocketToken, repeat))
                            {
                                if (repeat)
                                {
                                    subject.OnNext(Unit.Default);
                                }
                            }
                        }
                    });

                    subject.OnNext(Unit.Default);

                    disposable.Add(disposableEventSubscription);
                    disposable.Add(disposableActions);
                    disposable.Add(subject);

                    return disposable;
                });
        }

        private bool SendNotificationToObserver(IObserver<string> observer, SocketAsyncEventArgs socketEvent, bool repeat)
        {
            if (socketEvent.SocketError == SocketError.Success)
            {
                observer.OnNext(Encoding.UTF8.GetString(socketEvent.Buffer, 0, socketEvent.BytesTransferred));
                if (!repeat)
                    observer.OnCompleted();
                return true;
            }
            else
            {
                observer.OnError(new RedisException(socketEvent.SocketError));
                return false;
            }
        }
    }
}
