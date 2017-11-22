using System;
using System.Collections.Generic;
using System.Text;

namespace StackExchange.Redis.WordSearch
{
    public interface IRedisKeyNameConfiguration
    {
        /// <summary>
        /// ':' like seperator for the key concatenation
        /// </summary>
        string Seperator { get; }

        /// <summary>
        /// Prefix for isolating search environment
        /// </summary>
        string ContainerPrefix { get; }

        /// <summary>
        /// Suffix for hash key of searchable items
        /// </summary>
        string QueryableItemsSuffix { get; }

        /// <summary>
        /// Suffix for hash key of searchable items' data
        /// </summary>
        string QueryableItemsDataSuffix { get; }

        /// <summary>
        /// Suffix for sorted sets of searchable items
        /// </summary>
        string QuerySuffix { get; }

        /// <summary>
        /// Suffix for sorted set of searchable items' score
        /// </summary>
        string QueryableItemsRankingSuffix { get; }
    }
    internal class DefaultRedisKeyNameConfiguration : IRedisKeyNameConfiguration
    {
        public string Seperator => ":::";
        public string ContainerPrefix => "WQ";
        public string QueryableItemsSuffix => "Queryable";
        public string QueryableItemsDataSuffix => "QueryableData";
        public string QuerySuffix => "Query";
        public string QueryableItemsRankingSuffix => "QueryableRanking";
    }
    internal class RedisKeyComposer
    {
        internal const string DefaultSeperator = ":::";
        internal const string DefaultContainerPrefix = "WQ";
        internal const string QueryableItemsSuffix = "Queryable";
        internal const string QueryableItemsDataSuffix = "QueryableData";
        internal const string QuerySuffix = "Query";
        internal const string QueryableItemsRankingSuffix = "QueryableRanking";

        public IRedisKeyNameConfiguration KeyName {get;}
        internal bool IsCaseSensitive { get; }
        internal string Seperator { get; }
        internal string ContainerPrefix { get; }
        public RedisKeyComposer(string containerPrefix, string parameterSeperator, bool isCaseSensitive)
        {
            Seperator = string.IsNullOrEmpty(parameterSeperator) ? DefaultSeperator : parameterSeperator;
            ContainerPrefix = string.IsNullOrEmpty(containerPrefix) ? DefaultContainerPrefix : containerPrefix;
            IsCaseSensitive = isCaseSensitive;
        }
        public RedisKeyComposer(RedisWordSearchConfiguration configuration)
        {
            Seperator = string.IsNullOrEmpty(configuration.ParameterSeperator) ? DefaultSeperator : configuration.ParameterSeperator;
            ContainerPrefix = string.IsNullOrEmpty(configuration.ContainerPrefix) ? DefaultContainerPrefix : configuration.ContainerPrefix;
            IsCaseSensitive = configuration.IsCaseSensitive;
            KeyName = configuration.KeyNameConfiguration;
        }
        internal string CompactRedisKey(RedisKey pk, params string[] param)
        {
            string parametersSuffix = "";
            if (param.Length > 0)
            {
                parametersSuffix = Seperator + string.Join(Seperator, param);
            }
            return ContainerPrefix + Seperator + pk.ToString() + parametersSuffix;
        }
        internal string QueryKey(string subString)
        {
            string finalString = subString;
            if (!IsCaseSensitive)
            {
                finalString = finalString.ToLower();
            }
            return CompactRedisKey(QuerySuffix, finalString);
        }
        internal string QueryableItemsKey { get { return CompactRedisKey(QueryableItemsSuffix); } }
        internal string QueryableItemsDataKey { get { return CompactRedisKey(QueryableItemsDataSuffix); } }
        internal string QueryableItemsRankingKey { get { return CompactRedisKey(QueryableItemsRankingSuffix); } }

        internal string AdjustedValue(RedisKey redisPK)
        {
            string finalValue = redisPK;

            if (!IsCaseSensitive)
            {
                return finalValue = finalValue.ToLower();
            }
            return finalValue;
        }
    }
}
