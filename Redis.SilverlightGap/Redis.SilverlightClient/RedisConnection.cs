using System;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace Redis.SilverlightClient
{
    public class RedisConnection
    {
        private readonly Subject<Func<RedisTransmitter, RedisReceiver, Exception, Task>> inboxChannel;
        private readonly IDisposable disposable;
        private readonly IScheduler scheduler;

        public RedisConnection(string host, int port, IScheduler scheduler)
        {
            if (string.IsNullOrWhiteSpace(host))
                throw new ArgumentException("host");

            if (port < 4502 || port > 4534)
                throw new ArgumentException("Port must be in range 4502-4534 due to Silverlight network access restrictions");

            if (scheduler == null)
                throw new ArgumentNullException("scheduler");

            this.scheduler = scheduler;
            inboxChannel = new Subject<Func<RedisTransmitter, RedisReceiver, Exception, Task>>();

            var compositeDisposable = new CompositeDisposable();
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var socketAsyncEventArgs = new SocketAsyncEventArgs();
            var redisConnector = new RedisConnector(
                                    () => socket,
                                    () => socketAsyncEventArgs);

            var cancellationDisposable = new CancellationDisposable();

            var transmitterReceiverObservable = redisConnector
                .BuildConnectionToken(host, port, scheduler)
                .Select(connectionToken =>
                {
                    var transmitter = new RedisTransmitter(connectionToken);
                    var receiver = new RedisReceiver(connectionToken);

                    return new RedisTransmitterReceiver(transmitter, receiver);
                }).ToTask();

            var pipelineDisposable = inboxChannel.ObserveOn(scheduler).Select(message =>
            {
                return Observable.FromAsync(async () =>
                    {
                        Exception catchedException = null;
                        try
                        {
                            var connector = await transmitterReceiverObservable;
                            await message(connector.Transmitter, connector.Receiver, null);
                        }
                        catch (AggregateException exception)
                        {
                            exception.Handle(_ => true);
                            catchedException = exception;
                        }
                        catch (RedisException exception)
                        {
                            catchedException = exception;
                        }

                        if(catchedException != null)
                            await message(null, null, catchedException);
                    });
            }).Merge().Subscribe();

            compositeDisposable.Add(socket);
            compositeDisposable.Add(socketAsyncEventArgs);
            compositeDisposable.Add(pipelineDisposable);
            compositeDisposable.Add(inboxChannel);

            disposable = compositeDisposable;
        }

        internal IObserver<Func<RedisTransmitter, RedisReceiver, Exception, Task>> Inbox
        {
            get { return inboxChannel; }
        }

        public void Dispose()
        {
            disposable.Dispose();
        }
    }
}
