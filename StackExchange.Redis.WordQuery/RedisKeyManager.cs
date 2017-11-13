using StackExchange.Redis.WordQuery.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace StackExchange.Redis.WordQuery
{
    internal class RedisKeyManager
    {
        internal const string DefaultSeperator = ":::";
        internal const string DefaultContainerPrefix = "WQ";
        internal const String QueryableItemsSuffix = "Queryable";
        internal const String QueryableItemsDataSuffix = "QueryableData";
        internal const String QuerySuffix = "Query";

        private bool IsCaseSensitive { get; }
        private string Seperator { get; }
        private string ContainerPrefix { get; }
        public RedisKeyManager(string containerPrefix, string parameterSeperator, bool isCaseSensitive)
        {
            Seperator = string.IsNullOrEmpty(parameterSeperator) ? DefaultSeperator : parameterSeperator;
            ContainerPrefix = string.IsNullOrEmpty(containerPrefix) ? DefaultContainerPrefix : containerPrefix;
            IsCaseSensitive = isCaseSensitive;
        }
        public RedisKeyManager(RedisWordQueryConfiguration configuration)
        {
            Seperator = string.IsNullOrEmpty(configuration.ParameterSeperator) ? DefaultSeperator : configuration.ParameterSeperator;
            ContainerPrefix = string.IsNullOrEmpty(configuration.ContainerPrefix) ? DefaultContainerPrefix : configuration.ContainerPrefix;
            IsCaseSensitive = configuration.IsCaseSensitive;
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
