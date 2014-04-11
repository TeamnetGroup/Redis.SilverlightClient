using System.Linq;
using PortableSprache;

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
                                            from arrayElement in BulkStringParser.Repeat(arrayLength)
                                            select arrayElement
                                        ).Select(x => x.ToArray());
    }
}
