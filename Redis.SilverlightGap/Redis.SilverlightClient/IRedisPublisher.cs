using System;
using System.Threading.Tasks;

namespace Redis.SilverlightClient
{
    public interface IRedisPublisher
    {
        Task<int> PublishMessage(string channel, string message);
    }
}
