using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Redis.SilverlightClient
{
    public interface IRedisCacheClient 
    {
        Task SetValue(string key, string value);
        Task SetValue(string key, string value, TimeSpan? ttl);
        Task<string> GetValue(string key);
        Task SetValues(IEnumerable<KeyValuePair<string, string>> keyValuePairs);
        Task<IEnumerable<string>> GetValues(IEnumerable<string> keys);
    }
}
