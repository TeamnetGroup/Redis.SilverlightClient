using System;
using System.Net.Sockets;
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
        private readonly BehaviorSubject<Tuple<SocketTransmitter, SocketReceiver>> connectionSubject;
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
            var socketAsyncEventArgs = new SocketAsyncEventArgs();
            var socketConnector = new SocketConnector(
                                    () => socket,
                                    () => socketAsyncEventArgs);

            var cancellationDisposable = new CancellationDisposable();

            connectionSubject = new BehaviorSubject<Tuple<SocketTransmitter, SocketReceiver>>(null);
            var connectorDisposable = socketConnector
                .Connect(host, port, scheduler)
                .Select(connectedSocket =>
                {
                    var transmitter = new SocketTransmitter(connectedSocket, new SocketAsyncEventArgs());
                    var receiver = new SocketReceiver(connectedSocket, new SocketAsyncEventArgs());

                    return new Tuple<SocketTransmitter, SocketReceiver>(transmitter, receiver);
                }).Concat(Observable.Never<Tuple<SocketTransmitter, SocketReceiver>>()).Subscribe(connectionSubject);

            compositeDisposable.Add(socket);
            compositeDisposable.Add(socketAsyncEventArgs);
            compositeDisposable.Add(connectionSubject);
            compositeDisposable.Add(connectorDisposable);

            disposable = compositeDisposable;
        }

        public IObservable<Tuple<SocketTransmitter, SocketReceiver>> Connection 
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
