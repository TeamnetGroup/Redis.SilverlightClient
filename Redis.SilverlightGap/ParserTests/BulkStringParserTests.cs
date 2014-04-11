using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PortableSprache;

using Xunit;

namespace ParserTests
{
    public class BulkStringParserTests
    {
        [Fact]
        public void CanParseRedisBulkString()
        {
            var message = "$4\r\ntest\r\n";
            var parser = Redis.SilverlightClient.Parsers.RedisParsersModule.BulkStringParser;

            var result = parser.Parse(message);

            Assert.Equal("test", result);
        }
    }
}
