using System;
using System.Collections.Generic;
using System.Text;

namespace StackExchange.Redis.WordQuery.Model
{
    public struct RedisWordQueryConfiguration
    {
        public int MinPrefixLength { get; set; }
        public int MaxPrefixLength { get; set; }
        public bool IsCaseSensitive { get; set; }
        public WordIndexing WordIndexingMethod { get; set; }

        public string ParameterSeperator { get; set; }
        public string ContainerPrefix { get; set; }

        public ISerializer Serializer { get; set; }

        public static RedisWordQueryConfiguration defaultConfig = new RedisWordQueryConfiguration
        {
            MinPrefixLength = 1,
            MaxPrefixLength = -1,
            IsCaseSensitive = false,
            WordIndexingMethod = WordIndexing.SequentialOnly,
            ParameterSeperator = RedisKeyManager.DefaultSeperator,
            ContainerPrefix = RedisKeyManager.DefaultContainerPrefix
        };
    }
}
