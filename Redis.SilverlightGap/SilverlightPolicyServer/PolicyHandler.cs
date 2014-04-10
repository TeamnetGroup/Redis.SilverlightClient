using System;
using System.IO;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace SilverlightPolicyServer
{
    static class PolicyHandler
    {
        public const string PolicyRequest = "<policy-file-request/>";

        public static IDisposable HandleClients(TcpListener listener, Func<TcpClient, string, Task> handleClientAccessPolicy, string policyResponse)
        {
            listener.Start();

            var disposableHandler = ThreadPoolScheduler.Instance.ScheduleAsync(async (scheduler, ct) =>
            {
                var disposable = new BooleanDisposable();

                while (!disposable.IsDisposed)
                {
                    var client = await listener.AcceptTcpClientAsync();
                    await handleClientAccessPolicy(client, policyResponse);
                    await scheduler.Yield();
                }

                return disposable;
            });

            var compositeDisposable = new CompositeDisposable();

            compositeDisposable.Add(Disposable.Create(() => listener.Stop()));
            compositeDisposable.Add(disposableHandler);

            return compositeDisposable;
        }

        public static async Task HandleClientAccessPolicy(TcpClient client, string policyResponse)
        {
            using (var clientStream = client.GetStream())
            {
                string request = string.Empty;

                var reader = new StreamReader(clientStream);
                var charArray = new char[2048];
                var length = await reader.ReadAsync(charArray, 0, 2048);
                request = new String(charArray, 0, length);

                if (request == PolicyRequest)
                {
                    var writer = new StreamWriter(clientStream, Encoding.UTF8);
                    writer.AutoFlush = true;
                    await writer.WriteAsync(policyResponse);
                }
            }

            client.Close();
        }
    }
}
