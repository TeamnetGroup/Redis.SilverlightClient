using System;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Redis.SilverlightClient.Sockets
{
    public class SocketConnection : IDisposable
    {
        private readonly AsyncSubject<SocketTransmitterReceiver> connectionSubject;
        private readonly CompositeDisposable disposables;
        private readonly IScheduler scheduler;

        public SocketConnection(string host, int port, IScheduler scheduler)
        {
            if (string.IsNullOrWhiteSpace(host))
                throw new ArgumentException("host");

            if (port < 4502 || port > 4534)
                throw new ArgumentException("Port must be in range 4502-4534 due to Silverlight network access restrictions");

            if (scheduler == null)
                throw new ArgumentNullException("scheduler");

            this.scheduler = scheduler;
            this.disposables = new CompositeDisposable();

            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var connectArgs = new SocketAsyncEventArgs();
            var sendArgs = new SocketAsyncEventArgs();
            var receiveArgs = new SocketAsyncEventArgs();

            disposables.Add(socket);
            disposables.Add(connectArgs);
            disposables.Add(sendArgs);
            disposables.Add(receiveArgs);

            var socketConnector = new SocketConnector(
                                    () => socket,
                                    () => connectArgs);

            connectionSubject = new AsyncSubject<SocketTransmitterReceiver>();
            var connectorDisposable = socketConnector
                .Connect(host, port, scheduler)
                .Select(connectedSocket =>
                {
                    var transmitter = new SocketTransmitter();
                    var receiver = new SocketReceiver();
                    var buffer = new byte[8192];

                    return new SocketTransmitterReceiver(
                        message => 
                        {
                            if (disposables.IsDisposed)
                                return Observable.Empty<Unit>();
                            else
                                return Observable.Create<Unit>(observer =>
                                    transmitter
                                        .SendMessage(connectedSocket, sendArgs, scheduler, message)
                                        .Subscribe(
                                            observer.OnNext,
                                            ex =>
                                            {
                                                if (disposables.IsDisposed)
                                                    observer.OnCompleted();
                                                else
                                                    observer.OnError(ex);
                                            },
                                            observer.OnCompleted));
                         },
                         () => 
                         {
                             if(disposables.IsDisposed)
                                 return Observable.Empty<string>();
                             else
                                 return Observable.Create<string>(observer =>
                                     receiver
                                         .Receive(connectedSocket, receiveArgs, scheduler, buffer)
                                         .Subscribe(
                                             observer.OnNext,
                                             ex =>
                                             {
                                                 if(disposables.IsDisposed)
                                                     observer.OnCompleted();
                                                 else
                                                     observer.OnError(ex);
                                             },
                                             observer.OnCompleted));
                         },
                         scheduler);

                }).Subscribe(connectionSubject);

            disposables.Add(connectionSubject);
            disposables.Add(connectorDisposable);
        }

        public IObservable<SocketTransmitterReceiver> GetConnection()
        {
            if (disposables.IsDisposed)
                return Observable.Empty<SocketTransmitterReceiver>();
            else
                return connectionSubject; 
        }

        public IScheduler Scheduler
        {
            get { return scheduler; }
        }

        public bool IsDisposed
        {
            get
            {
                return disposables.IsDisposed;
            }
        }

        public void Dispose()
        {
            disposables.Dispose();
        }
    }
}
