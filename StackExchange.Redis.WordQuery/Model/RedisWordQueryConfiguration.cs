using System;
using System.Collections.Generic;
using System.Text;

namespace StackExchange.Redis.WordQuery
{
    public struct RedisWordQueryConfiguration
    {
        public int MinQueryLength { get; set; }
        public int MaxQueryLength { get; set; }
        public bool IsCaseSensitive { get; set; }
        public WordIndexing WordIndexingMethod { get; set; }

        public string ParameterSeperator { get; set; }
        public string ContainerPrefix { get; set; }

        private ISerializer _Serializer;
        public ISerializer Serializer {
            get {
                if(_Serializer == null)
                {
                    throw new SerializerNotFoundException();
                }
                return _Serializer;
            }
            set {
                _Serializer = value;
            }
        }

        public static RedisWordQueryConfiguration defaultConfig = new RedisWordQueryConfiguration
        {
            MinQueryLength = 1,
            MaxQueryLength = -1,
            IsCaseSensitive = true,
            WordIndexingMethod = WordIndexing.SequentialOnly,
            ParameterSeperator = RedisKeyComposer.DefaultSeperator,
            ContainerPrefix = RedisKeyComposer.DefaultContainerPrefix
        };
    }
}
