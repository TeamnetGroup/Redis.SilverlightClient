using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redis.NetClientTests
{
    [SetUpFixture]
    public class TestsSetup
    {
        public static readonly int Port = 4525;
        public static readonly string Host = "127.0.0.1";

        [SetUp]
        public void RunBeforeAnyTests()
        {
            RedisIntegration.HostManager.RunInstance(Port);
        }
    }
}
