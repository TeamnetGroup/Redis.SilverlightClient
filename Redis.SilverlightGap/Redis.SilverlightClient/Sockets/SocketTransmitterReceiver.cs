using System;
using System.Reactive;
using System.Reactive.Concurrency;

namespace Redis.SilverlightClient.Sockets
{
    public class SocketTransmitterReceiver
    {
        public Func<string, IObservable<Unit>> SendMessage { get; private set; }
        public Func<IObservable<string>> ReceiveMessage { get; private set; }
        public IScheduler Scheduler { get; private set; }

        public SocketTransmitterReceiver(Func<string, IObservable<Unit>> sendMessage, Func<IObservable<string>> receiveMessage, IScheduler scheduler)
        {
            if(sendMessage == null)
                throw new ArgumentNullException("sendMessage");

            if(receiveMessage == null)
                throw new ArgumentNullException("receiveMessage");

            if (scheduler == null)
                throw new ArgumentNullException("scheduler");

            SendMessage = sendMessage;
            ReceiveMessage = receiveMessage;
            Scheduler = scheduler;
        }
    }
}
