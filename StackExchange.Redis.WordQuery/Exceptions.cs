using System;
using System.Collections.Generic;
using System.Text;

namespace StackExchange.Redis.WordQuery
{
    public class SerializerNotFoundException : Exception
    {
        public SerializerNotFoundException() : base("There is no Serializer found in the RedisWordQueryConfiguration") { }
    }
    public class KeyNameConfigurationNotFoundException : Exception
    {
        public KeyNameConfigurationNotFoundException() : base("There is no KeyNameConfiguration found in the RedisWordQueryConfiguration") { }
    }

    public class CurrentlyPopularityRankingAlgorithmOutOfLimit : Exception
    {
        public CurrentlyPopularityRankingAlgorithmOutOfLimit() : base("Current popularity scores exceeded the redis sorted set score limit") { }
    }
    public class RankingAlgorithmNotFound : Exception
    {
        public RankingAlgorithmNotFound() : base("No ranking algorithm is specified") { }
    }
}
