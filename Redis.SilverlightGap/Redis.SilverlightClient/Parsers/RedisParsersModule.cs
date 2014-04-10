using PortableSprache;
using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Redis.SilverlightClient.Parsers
{
    internal static class RedisParsersModule
    {
        public static Parser<string> BulkStringParser =
                                        from dollar in Parse.Char('$')
                                        from length in Parse.Number.Text().Select(int.Parse)
                                        from _ in Parse.String("\r\n")
                                        from content in Parse.AnyChar.Repeat(length)
                                        from __ in Parse.String("\r\n")
                                        select new string(content.ToArray());

        public static Parser<string[]> ArrayOfBulkStringsParser =
                                        (
                                            from star in Parse.Char('*')
                                            from arrayLength in Parse.Number.Text().Select(int.Parse)
                                            from _ in Parse.String("\r\n")
                                            from arrayElement in BulkStringParser.Repeat(arrayLength)
                                            select arrayElement
                                        ).Select(x => x.ToArray());
    }
}
