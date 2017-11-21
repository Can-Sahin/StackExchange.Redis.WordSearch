using System;
using System.Collections.Generic;
using System.Text;

namespace StackExchange.Redis.WordQuery
{
    public class SerializerNotFoundException : Exception
    {
        public SerializerNotFoundException() : base("There is no serializer found for search query") { }
    }
}
