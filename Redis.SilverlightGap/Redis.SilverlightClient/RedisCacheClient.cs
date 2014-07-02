using PortableSprache;
using Redis.SilverlightClient.Messages;
using Redis.SilverlightClient.Parsers;
using Redis.SilverlightClient.Sockets;
using System;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Threading.Tasks;
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
    public class RedisCacheClient : IDisposable
    {
        private readonly Subject<object> inboxChannel;
        private readonly IDisposable disposable;

        public RedisCacheClient(string host, int port) : this(host, port, Scheduler.Default) { }

        public RedisCacheClient(string host, int port, IScheduler scheduler)
        {
            if (string.IsNullOrWhiteSpace(host))
                throw new ArgumentException("host");

            if (port < 4502 || port > 4534)
                throw new ArgumentException("Port must be in range 4502-4534 due to Silverlight network access restrictions");

            if (scheduler == null)
                throw new ArgumentNullException("scheduler");

            inboxChannel = new Subject<object>();
            
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

                    var getValueMessage = message as RedisGetValueMessage;

                    if (getValueMessage != null)
                    {
                        var result = RedisParsersModule.BulkStringParser.TryParse(response);

                        if (!result.WasSuccessful)
                        {
                            var nullResponse = RedisParsersModule.NullParser.TryParse(response);

                            if (nullResponse.WasSuccessful)
                            {
                                getValueMessage.Callback.SetResult(null);
                            }
                            else
                            {
                                getValueMessage.Callback.SetException(
                                    new ParseException(
                                        "Unknown GET response: " + response));
                            }
                        }
                        else
                        {
                            getValueMessage.Callback.SetResult(result.Value);
                        }

                        return;
                    }

                    var setValueMessage = message as RedisSetValueMessage;

                    if (setValueMessage != null)
                    {
                        var ok = RedisParsersModule.OKParser.TryParse(response);

                        if (!ok.WasSuccessful)
                        {
                            setValueMessage.Callback.SetException(
                                    new ParseException(
                                        "Unknown SET response: " + response));
                        }

                        setValueMessage.Callback.SetResult("OK");

                        return;
                    }
                }
                catch (AggregateException exception)
                {
                    HandleException(exception, message);
                }
                catch (RedisException exception)
                {
                    HandleException(exception, message);
                }
            });

            compositeDisposable.Add(Disposable.Create(() => socket.Dispose()));
            compositeDisposable.Add(pipelineDisposable);

            disposable = compositeDisposable;
        }

        private void HandleException(Exception exception, object message)
        {
            var getValueMessage = message as RedisGetValueMessage;

            if (getValueMessage != null)
            {
                getValueMessage.Callback.SetException(exception);
                return;
            }

            var setValueMessage = message as RedisSetValueMessage;

            if (setValueMessage != null)
            {
                setValueMessage.Callback.SetException(exception);
                return;
            }
        }

        public Task SetValue(string key, string value)
        {
            var callback = new TaskCompletionSource<string>();
            var setValueMessage = new RedisSetValueMessage(key, value, callback);
            inboxChannel.OnNext(setValueMessage);
            return callback.Task;
        }

        public Task SetValue(string key, string value, TimeSpan ttl)
        {
            var callback = new TaskCompletionSource<string>();
            var setValueMessage = new RedisSetValueMessage(key, value, ttl, callback);
            inboxChannel.OnNext(setValueMessage);
            return callback.Task;
        }

        public Task<string> GetValue(string key)
        {
            var callback = new TaskCompletionSource<string>();
            var getValueMessage = new RedisGetValueMessage(key, callback);
            inboxChannel.OnNext(getValueMessage);
            return callback.Task;
        }

        public void Dispose()
        {
            inboxChannel.Dispose();
            disposable.Dispose();
        }
    }
}
