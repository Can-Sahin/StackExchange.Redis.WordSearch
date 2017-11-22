using System;
using System.Collections.Generic;
using System.Text;

namespace StackExchange.Redis.WordSearch
{

    public enum WordIndexing { SequentialOnly, SequentialCombination }

    public struct RedisWordSearchConfiguration
    {
        public int MinQueryLength { get; set; }
        public int MaxQueryLength { get; set; }
        public bool IsCaseSensitive { get; set; }
        public WordIndexing WordIndexingMethod { get; set; }

        public string ParameterSeperator { get; set; }
        public string ContainerPrefix { get; set; }
        public ICumulativeDecayRateRanking RankingProvider { get; set; }
        private ISerializer _Serializer;
        public ISerializer Serializer
        {
            get
            {
                if (_Serializer == null)
                {
                    throw new SerializerNotFoundException();
                }
                return _Serializer;
            }
            set
            {
                _Serializer = value;
            }
        }
        private IRedisKeyNameConfiguration _KeyNameConfiguration;

        public IRedisKeyNameConfiguration KeyNameConfiguration
        {
            get
            {
                if (_KeyNameConfiguration == null)
                {
                    throw new KeyNameConfigurationNotFoundException();
                }
                return _KeyNameConfiguration;
            }
            set
            {
                _KeyNameConfiguration = value;
            }
        }

        public static RedisWordSearchConfiguration defaultConfig = new RedisWordSearchConfiguration
        {
            MinQueryLength = 1,
            MaxQueryLength = -1,
            IsCaseSensitive = true,
            KeyNameConfiguration = new DefaultRedisKeyNameConfiguration(),
            WordIndexingMethod = WordIndexing.SequentialOnly,
            ParameterSeperator = RedisKeyComposer.DefaultSeperator,
            ContainerPrefix = RedisKeyComposer.DefaultContainerPrefix
        };
    }
}
