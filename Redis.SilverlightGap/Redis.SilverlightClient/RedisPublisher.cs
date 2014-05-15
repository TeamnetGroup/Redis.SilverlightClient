using Redis.SilverlightClient.Parsers;
using Redis.SilverlightClient.Sockets;
using System;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using PortableSprache;
using Redis.SilverlightClient.Messages;
using System.Reactive;
using System.Threading.Tasks;

namespace Redis.SilverlightClient
{
    public class RedisPublisher : IDisposable
    {
        private readonly Subject<RedisPublishMessage> inboxChannel;
        private readonly IDisposable disposable;

        public RedisPublisher(string host, int port, IScheduler scheduler)
        {
            if (string.IsNullOrWhiteSpace(host))
                throw new ArgumentException("host");

            if (port < 4502 || port > 4534)
                throw new ArgumentException("Port must be in range 4502-4534 due to Silverlight network access restrictions");

            if (scheduler == null)
                throw new ArgumentNullException("scheduler");

            inboxChannel = new Subject<RedisPublishMessage>();

            var compositeDisposable = new CompositeDisposable();
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var socketAsyncEventArgs = new SocketAsyncEventArgs();
            var redisConnector = new RedisConnector(
                                    () => new SocketDecorator(socket),
                                    () => new SocketAsyncEventArgsDecorator(socketAsyncEventArgs));

            var cancellationDisposable = new CancellationDisposable();

             var connectTask = new Task<RedisTransmitterReceiver>(() =>
             {
                 var connectionToken = redisConnector.BuildConnectionToken(host, port, scheduler).Wait<ConnectionToken>();
                 var transmitter = new RedisTransmitter(connectionToken);
                 var receiver = new RedisReceiver(connectionToken);

                 return new RedisTransmitterReceiver(transmitter, receiver);
             }, cancellationDisposable.Token);
            
            var buffer = new byte[256];
            var pipelineDisposable = inboxChannel.ObserveOn(scheduler).Subscribe(message =>
            {
                try
                {
                    if (connectTask.Status == TaskStatus.Created)
                    {
                        connectTask.Start();
                    }

                    var transmitterReceiver = connectTask.Result;
                    transmitterReceiver.Transmitter.SendMessage(message.ToString(), scheduler).Wait();
                    var response = transmitterReceiver.Receiver.Receive(buffer, scheduler, false).Wait<string>();
                    var pongs = RedisParsersModule.IntegerParser.TryParse(response);

                    if (!pongs.WasSuccessful)
                        throw new ParseException(string.Format("Invalid integer response for published message: {0}", pongs.Message));

                    message.Callback.SetResult(pongs.Value);
                }
                catch(ParseException exception)
                {
                    message.Callback.SetException(exception);
                }
                catch (RedisException exception)
                {
                    message.Callback.SetException(exception);
                }
            });

            compositeDisposable.Add(Disposable.Create(() => socket.Dispose()));
            compositeDisposable.Add(pipelineDisposable);

            disposable = compositeDisposable;
        }

        public Task<int> PublishMessage(string channel, string message)
        {
            var callback = new TaskCompletionSource<int>();
            var publishMessage = new RedisPublishMessage(channel, message, callback);
            inboxChannel.OnNext(publishMessage);
            return callback.Task;
        }

        public void Dispose()
        {
            inboxChannel.Dispose();
            disposable.Dispose();
        }
    }
}
