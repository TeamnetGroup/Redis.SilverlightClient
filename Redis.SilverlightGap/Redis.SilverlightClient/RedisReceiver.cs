using Redis.SilverlightClient.Sockets;
using System;
using System.Net;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Redis.SilverlightClient
{
    class RedisReceiver
    {
        private readonly ConnectionToken connectionToken;

        public RedisReceiver(ConnectionToken connectionToken)
        {
            if (connectionToken == null)
                throw new ArgumentNullException("connectionToken");

            this.connectionToken = connectionToken;
        }

        public IObservable<string> Receive(byte[] buffer, IScheduler scheduler)
        {
            return Observable.Create<string>(observer =>
                {
                    var disposable = new CompositeDisposable();
                    this.connectionToken.SocketEvent.SetBuffer(buffer, 0, buffer.Length);
                    var subject = new Subject<Unit>();

                    var disposableEventSubscription = connectionToken.SocketEvent.Completed.ObserveOn(scheduler).Subscribe(_ =>
                    {
                        if (SendNotificationToObserver(observer, connectionToken.SocketEvent))
                        {
                            subject.OnNext(Unit.Default);
                        }
                    });

                    var disposableActions = subject.ObserveOn(scheduler).Subscribe(_ =>
                    {
                        if (!connectionToken.Socket.ReceiveAsync(connectionToken.SocketEvent))
                        {
                            if (SendNotificationToObserver(observer, connectionToken.SocketEvent))
                            {
                                subject.OnNext(Unit.Default);
                            }
                        }
                    });

                    subject.OnNext(Unit.Default);

                    disposable.Add(disposableEventSubscription);
                    disposable.Add(disposableActions);

                    return disposable;
                });
        }

        private bool SendNotificationToObserver(IObserver<string> observer, ISocketAsyncEventArgs socketEvent)
        {
            if (socketEvent.SocketError == SocketError.Success)
            {
                observer.OnNext(Encoding.UTF8.GetString(socketEvent.Buffer, 0, socketEvent.BytesTransferred));
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
