using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace Redis.SilverlightClient.Sockets
{
    public class SocketTransmitterReceiver
    {
        public Func<string, IObservable<Unit>> SendMessage { get; private set; }
        public Func<IObservable<string>> ReceiveMessage { get; private set; }

        public SocketTransmitterReceiver(Func<string, IObservable<Unit>> sendMessage, Func<IObservable<string>> receiveMessage)
        {
            if(sendMessage == null)
                throw new ArgumentNullException("sendMessage");

            if(receiveMessage == null)
                throw new ArgumentNullException("receiveMessage");

            SendMessage = sendMessage;
            ReceiveMessage = receiveMessage;
        }
    }
}
