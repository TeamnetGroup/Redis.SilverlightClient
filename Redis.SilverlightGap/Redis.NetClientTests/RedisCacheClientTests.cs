using NUnit.Framework;
using Redis.SilverlightClient;
using Redis.SilverlightClient.Sockets;
using RedisIntegration;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;

namespace Redis.NetClientTests
{
    [TestFixture]
    public class RedisCacheClientTests 
    {
        [Test]
        public async Task CanSetKeyAndGetValue()
        {
            var expected = "CanSetKeyAndGetValue";

            using (var socketConnection = new SocketConnection(TestsSetup.Host, TestsSetup.Port, Scheduler.Immediate))
            {
                var cacheClient = socketConnection.AsCacheClient();
                await cacheClient.SetValue("test1", expected);
                var cacheValue = await cacheClient.GetValue("test1");

                Assert.AreEqual(expected, cacheValue);
            }
        }

        [Test]
        public async Task CanSetMultipleKeysAndGetValues()
        {
            var expected = "CanSetMultipleKeysAndGetValues";
            var map = new Dictionary<string, string>();
            map.Add("test2", expected);
            map.Add("test3", expected);

            using (var socketConnection = new SocketConnection(TestsSetup.Host, TestsSetup.Port, Scheduler.Immediate))
            {
                var cacheClient = socketConnection.AsCacheClient();
                await cacheClient.SetValues(map);
                var cacheValues = await cacheClient.GetValues(map.Keys.ToArray());

                Assert.AreEqual(expected, cacheValues.Take(1).First());
                Assert.AreEqual(expected, cacheValues.Skip(1).Take(1).First());
            }
        }

        [Test]
        public async Task CanSetKeyAndThenDeleteIt()
        {
            var expected = "CanSetKeyAndThenDeleteIt";

            using (var socketConnection = new SocketConnection(TestsSetup.Host, TestsSetup.Port, Scheduler.Immediate))
            {
                var cacheClient = socketConnection.AsCacheClient();
                await cacheClient.SetValue("test4", expected);
                var cacheValue = await cacheClient.GetValue("test4");

                Assert.AreEqual(expected, cacheValue);

                await cacheClient.Del("test4");
                var cacheValueDeleted = await cacheClient.GetValue("test4");

                Assert.AreEqual(null, cacheValueDeleted);
            }
        }

        [Test]
        public async Task CanSetKeysAndThenDeleteAll()
        {
            var expected = "CanSetKeysAndThenDeleteAll";

            using (var socketConnection = new SocketConnection(TestsSetup.Host, TestsSetup.Port, Scheduler.Immediate))
            {
                var cacheClient = socketConnection.AsCacheClient();
                var map = new Dictionary<string, string>();
                map.Add("test5", expected);
                map.Add("test6", expected);

                await cacheClient.SetValues(map);
                await cacheClient.Del(map.Keys.ToArray());

                var cacheValues = await cacheClient.GetValues(map.Keys.ToArray());

                Assert.IsTrue(cacheValues.All(x => x == null));
            }
        }

        [Test]
        public async Task CanSetKeyAndGetText()
        {
            var expected = Enumerable.Repeat("CanSetKeyAndGetText", 1024)
                .Aggregate(
                    new StringBuilder(), 
                    (builder, value) => builder.Append(value),
                    builder => builder.ToString());

            using (var socketConnection = new SocketConnection(TestsSetup.Host, TestsSetup.Port, Scheduler.Immediate))
            {
                var cacheClient = socketConnection.AsCacheClient();
                await cacheClient.SetValue("test7", expected);
                var cacheValue = await cacheClient.GetValue("test7");

                Assert.AreEqual(expected, cacheValue);
            }
        }
    }
}
