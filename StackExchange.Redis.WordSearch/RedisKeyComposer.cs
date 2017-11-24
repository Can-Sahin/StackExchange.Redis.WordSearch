using System;
using System.Collections.Generic;
using System.Text;

namespace StackExchange.Redis.WordSearch
{
    /// <summary>
    /// Redis key names settings
    /// </summary>
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
        string SearchableItemsSuffix { get; }

        /// <summary>
        /// Suffix for hash key of searchable items' data
        /// </summary>
        string SearchableItemsDataSuffix { get; }

        /// <summary>
        /// Suffix for sorted sets of searchable items
        /// </summary>
        string SearchableSuffix { get; }

        /// <summary>
        /// Suffix for sorted set of searchable items' score
        /// </summary>
        string SearchableItemsRankingSuffix { get; }
    }
    
    /// <summary>
    /// Default key names for RedisWordSearch
    /// </summary>
    public class DefaultRedisKeyNameConfiguration : IRedisKeyNameConfiguration
    {
        public string Seperator => "::";
        public string ContainerPrefix => "WS";
        public string SearchableItemsSuffix => "SearchableItems";
        public string SearchableItemsDataSuffix => "SearchableItemsData";
        public string SearchableSuffix => "S";
        public string SearchableItemsRankingSuffix => "SearchableItemsRanking";
    }

    internal class RedisKeyComposer
    {
        public IRedisKeyNameConfiguration KeyName {get;}
        internal bool IsCaseSensitive { get; }
        internal string Seperator { get; }
        internal string ContainerPrefix { get; }
        public RedisKeyComposer(bool isCaseSensitive)
        {
            this.IsCaseSensitive = isCaseSensitive;
        }
        public RedisKeyComposer(RedisWordSearchConfiguration configuration)
        {
            IsCaseSensitive = configuration.IsCaseSensitive;
            KeyName = configuration.KeyNameConfiguration;
        }
        internal string CompactRedisKey(RedisKey pk, params string[] param)
        {
            string parametersSuffix = "";
            if (param.Length > 0)
            {
                parametersSuffix = KeyName.Seperator + string.Join(KeyName.Seperator, param);
            }
            return KeyName.ContainerPrefix + KeyName.Seperator + pk.ToString() + parametersSuffix;
        }
        internal string SearchKey(string subString)
        {
            string finalString = subString;
            if (!IsCaseSensitive)
            {
                finalString = finalString.ToLower();
            }
            return CompactRedisKey(KeyName.SearchableSuffix, finalString);
        }
        internal string SearchableItemsKey { get { return CompactRedisKey(KeyName.SearchableItemsSuffix); } }
        internal string SearchableItemsDataKey { get { return CompactRedisKey(KeyName.SearchableItemsDataSuffix); } }
        internal string SearchableItemsRankingKey { get { return CompactRedisKey(KeyName.SearchableItemsRankingSuffix); } }

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
