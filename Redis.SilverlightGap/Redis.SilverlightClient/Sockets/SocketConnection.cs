using System;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace Redis.SilverlightClient.Sockets
{
    public class SocketConnection : IDisposable
    {
        private readonly BehaviorSubject<SocketTransmitterReceiver> connectionSubject;
        private readonly IDisposable disposable;
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
            var compositeDisposable = new CompositeDisposable();

            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var connectArgs = new SocketAsyncEventArgs();
            var sendArgs = new SocketAsyncEventArgs();
            var receiveArgs = new SocketAsyncEventArgs();

            compositeDisposable.Add(socket);
            compositeDisposable.Add(connectArgs);
            compositeDisposable.Add(sendArgs);
            compositeDisposable.Add(receiveArgs);

            var socketConnector = new SocketConnector(
                                    () => socket,
                                    () => connectArgs);

            connectionSubject = new BehaviorSubject<SocketTransmitterReceiver>(null);
            var connectorDisposable = socketConnector
                .Connect(host, port, scheduler)
                .Select(connectedSocket =>
                {
                    var transmitter = new SocketTransmitter();
                    var receiver = new SocketReceiver();
                    var buffer = new byte[4096];

                    return new SocketTransmitterReceiver(
                         message => transmitter.SendMessage(connectedSocket, sendArgs, scheduler, message),
                         () => receiver.Receive(connectedSocket, receiveArgs, scheduler, buffer));

                }).Concat(Observable.Never<SocketTransmitterReceiver>()).Subscribe(connectionSubject);

            compositeDisposable.Add(connectionSubject);
            compositeDisposable.Add(connectorDisposable);

            disposable = compositeDisposable;
        }

        public IObservable<SocketTransmitterReceiver> Connection 
        {
            get { return connectionSubject.Where(x => x != null).ObserveOn(scheduler); }
        }

        public IScheduler Scheduler
        {
            get { return scheduler; }
        }

        public void Dispose()
        {
            disposable.Dispose();
        }
    }
}
