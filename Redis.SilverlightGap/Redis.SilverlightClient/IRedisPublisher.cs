using System;
using System.Threading.Tasks;

namespace Redis.SilverlightClient
{
    public interface IRedisPublisher : IDisposable
    {
        Task<int> PublishMessage(string channel, string message);
    }
}
