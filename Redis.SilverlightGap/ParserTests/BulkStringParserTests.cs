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

        [Fact]
        public void ParsingMalformedBulkStringMessageFails()
        {
            var malformedMessage = "not a real bulk string";

            var parser = Redis.SilverlightClient.Parsers.RedisParsersModule.BulkStringParser;

            Assert.Throws<ParseException>(() =>
                parser.Parse(malformedMessage));
        }

        [Fact]
        public void MessageLengthMustMatch()
        {
            var malformedMessage = "$4\r\nlong test message\r\n";

            var parser = Redis.SilverlightClient.Parsers.RedisParsersModule.BulkStringParser;

            Assert.Throws<ParseException>(() => parser.Parse(malformedMessage));
        }
    }
}
