using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Disposables;

namespace SilverlightPolicyServer
{
    public class PolicyService
    {
        IDisposable serviceDisposable;

        public PolicyService()
        {
            serviceDisposable = Disposable.Empty;
        }

        public void Start()
        {
            serviceDisposable.Dispose();

            var policyResponse = File.ReadAllText("ClientAccessPolicy.xml");
            var tcpListener = new TcpListener(IPAddress.Any, 943);
            var policyDisposable = PolicyHandler.HandleClients(tcpListener, PolicyHandler.HandleClientAccessPolicy, policyResponse);

            serviceDisposable = policyDisposable;
        }

        public void Stop()
        {
            serviceDisposable.Dispose();
        }
    }
}
