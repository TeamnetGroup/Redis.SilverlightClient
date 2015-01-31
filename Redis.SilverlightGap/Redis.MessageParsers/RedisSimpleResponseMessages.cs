using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Redis.MessageParsers
{
    public class RedisSimpleResponseMessages
    {
        private RedisSimpleResponseMessages() { }

        public static readonly RedisSimpleResponseMessages OK = new RedisSimpleResponseMessages();
    }
}
