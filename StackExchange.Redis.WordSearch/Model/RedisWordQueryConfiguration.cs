using System;
using System.Collections.Generic;
using System.Text;

namespace StackExchange.Redis.WordSearch
{

    /// <summary>
    /// Strategy to strip words for searching
    /// </summary>
    public enum WordIndexing { SequentialOnly, SequentialCombination }

    /// <summary>
    /// RedisWordSearch Settings
    /// </summary>
    public struct RedisWordSearchConfiguration
    {
        /// <summary>
        /// Min string length to search
        /// </summary>
        public int MinSearchLength { get; set; }

        /// <summary>
        /// Max string length to search
        /// </summary>
        public int MaxSearchLength { get; set; }

        /// <summary>
        /// Lowercase or uppercase senstivity for searches
        /// </summary>
        public bool IsCaseSensitive { get; set; }

        /// <summary>
        /// Strategy to strip words for searching
        /// </summary>
        public WordIndexing WordIndexingMethod { get; set; }

        /// <summary>
        /// Ranking algorithm provider
        /// </summary>
        public IRelativeDecayRateRanking RankingProvider { get; set; }
        private ISerializer _Serializer;

        /// <summary>
        /// Serializer for search data
        /// </summary>
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

        /// <summary>
        /// Redis key names settings
        /// </summary>
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

        /// <summary>
        /// Default configuration values
        /// </summary>
        public static RedisWordSearchConfiguration defaultConfig = new RedisWordSearchConfiguration
        {
            MinSearchLength = 1,
            MaxSearchLength = -1,
            IsCaseSensitive = true,
            KeyNameConfiguration = new DefaultRedisKeyNameConfiguration(),
            WordIndexingMethod = WordIndexing.SequentialOnly
        };
    }
}
