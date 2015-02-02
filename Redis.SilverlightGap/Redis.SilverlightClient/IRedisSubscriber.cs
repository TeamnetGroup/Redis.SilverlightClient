using Redis.SilverlightClient.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redis.SilverlightClient
{
    public interface IRedisSubscriber
    {
        Task<IObservable<RedisChannelMessage>> Subscribe(params string[] channelNames);
        Task<IObservable<RedisChannelPatternMessage>> PSubscribe(params string[] channelPatterns);
    }
}
