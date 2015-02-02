using System.Linq;
using PortableSprache;
using Redis.MessageParsers;

namespace Redis.SilverlightClient.Parsers
{
    public static class RedisParsersModule
    {
        public static Parser<string> BulkStringParser =
                                        from dollar in Parse.Char('$')
                                        from length in Parse.Number.Select(int.Parse)
                                        from _ in Parse.String("\r\n")
                                        from content in Parse.AnyChar.Repeat(length)
                                        from __ in Parse.String("\r\n")
                                        select new string(content.ToArray());

        public static Parser<string[]> ArrayOfBulkStringsParser =
                                        (
                                            from star in Parse.Char('*')
                                            from arrayLength in Parse.Number.Select(int.Parse)
                                            from _ in Parse.String("\r\n")
                                            from arrayElement in BulkStringParser.Or(NullParser).Repeat(arrayLength)
                                            select arrayElement
                                        ).Select(x => x.ToArray());

        public static Parser<string[]> SubscriptionMessageParser =
                                        (
                                            from star in Parse.Char('*')
                                            from arrayLength in Parse.Number.Select(int.Parse)
                                            from _ in Parse.String("\r\n")
                                            from arrayElement in BulkStringParser.Repeat(arrayLength - 1)
                                            from channelSubscribed in IntegerParser
                                            from __ in Parse.String("\r\n")
                                            select arrayElement
                                        ).Select(x => x.ToArray());

        public static Parser<int> IntegerParser =
                                    from colon in Parse.Char(':')
                                    from integer in Parse.Number.Select(int.Parse)
                                    select integer;

        public static Parser<RedisSimpleResponseMessages> OKParser =
                                        from plus in Parse.String("+OK")
                                        from _ in Parse.String("\r\n")
                                        select RedisSimpleResponseMessages.OK;

        public static Parser<string> NullParser =
                                        from dollar in Parse.Char('$')
                                        from minus in Parse.Char('-')
                                        from one in Parse.Char('1')
                                        from _ in Parse.String("\r\n")
                                        select null as string;
    }
}
