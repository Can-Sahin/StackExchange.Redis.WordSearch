using System;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

namespace StackExchange.Redis.WordSearch
{
    public class RedisWordSearch : IRedisWordSearch
    {

        public RedisWordSearchConfiguration configuration { get; }
        private CumulativeDecayRateRankingHandler rankingHandler {get;}

        private RedisAccessClient redis { get; }

        public RedisWordSearch(IDatabase database, RedisWordSearchConfiguration? configuration = null)
        {
            this.configuration = configuration ?? RedisWordSearchConfiguration.defaultConfig;
            this.redis = new RedisAccessClient(database, new RedisKeyComposer(this.configuration));
            this.rankingHandler = new CumulativeDecayRateRankingHandler(this.redis,this.configuration.RankingProvider);
        }
        #region CRUD
        
        public bool Add(RedisKey redisPK, string searchableValue, string embedData, Encoding encoding = null, bool updateOnExist = true)
        {
            Encoding selectedEncoding = encoding ?? Encoding.UTF8;
            return Add(redisPK, searchableValue, selectedEncoding.GetBytes(embedData), updateOnExist);
        }
        public bool AddObject<T>(RedisKey redisPK, string searchableValue, T embedData, bool updateOnExist = true)
        {
            var serialized = configuration.Serializer.Serialize(embedData);
            bool add = Add(redisPK, searchableValue, serialized, updateOnExist);
            return add;
        }
        public bool Add(RedisKey redisPK, string searchableValue = null, byte[] embedData = null, bool updateOnExist = true)
        {
            string queryableWord = searchableValue ?? redisPK;
            byte[] data = embedData ?? Encoding.UTF8.GetBytes(queryableWord);

            if (updateOnExist && redis.isQueryableExists(redisPK))
            {
                string currentQueryableValue = redis.GetQueryableWord(redisPK);
                if (currentQueryableValue.Equals(queryableWord))
                {
                    redis.SetQueryableWordsData(redisPK, data);
                    return true;
                }
                else
                {
                    Remove(redisPK);
                }
            }
            List<string> partials = CreateAllPartialsForWord(queryableWord, configuration.WordIndexingMethod).ToList();
            return redis.AddQueryableWord(redisPK, queryableWord, data, partials);
        }
        public bool Update(RedisKey redisPK, string searchableValue = null, byte[] embedData = null, bool onlyIfExists = true)
        {
            if (onlyIfExists && !redis.isQueryableExists(redisPK))
            {
                return false;
            }
            return Add(redisPK, searchableValue, embedData);
        }
        public bool Remove(RedisKey redisPK)
        {
            string queryableWord = redis.GetQueryableWord(redisPK);
            List<string> partials = CreateAllPartialsForWord(queryableWord, configuration.WordIndexingMethod).ToList();

            return redis.RemoveQueryableWord(redisPK, partials);
        }
        #endregion

        #region Search
        public T SearchSingle<T>(string queryString)
        {
            var result = configuration.Serializer.Deserialize<T>(SearchSingle(queryString));
            return result;
        }

        public List<T> Search<T>(string queryString, int limit = 0, Func<T, bool> filterFunc = null)
        {
            var searchResults = Search(queryString, limit).AsByte();
            var results = searchResults.Select(r => configuration.Serializer.Deserialize<T>(r));
            if (filterFunc != null)
            {
                var filteredResults = results.Where(r => !filterFunc(r));
                return filteredResults.ToList();
            }
            else
            {
                return results.ToList();
            }
        }

        public RedisValue SearchSingle(string queryString)
        {
            var searchResults = Search(queryString, 2);
            if (searchResults.Count() != 1)
            {
                throw new InvalidOperationException("Sequence contains more than one matching element");
            }
            var result = searchResults.First();
            return result;
        }
        public RedisValue SearchSingleOrDefault(string queryString, RedisValue defaultValue = default(RedisValue))
        {
            var searchResults = Search(queryString, 2);
            if (searchResults.Count() != 1)
            {
                return defaultValue;
            }
            var result = searchResults.First();
            return result;
        }

        public IEnumerable<RedisValue> Search(string queryString, int limit = 0, Func<RedisValue, bool> filterFunc = null)
        {
            List<RedisValue> pkList = redis.GetSearchEntries(queryString, limit).Select(r => r.Element).ToList();
            var results = redis.GetDataOfQueryablePKs(pkList);

            if (filterFunc != null)
            {
                var filteredResults = results.Where(r => !filterFunc(r));
                return filteredResults;
            }
            else
            {
                return results;
            }
        }

        public double? BoostInRanking(RedisKey redisPK, double multiplierCoefficient = 1)
        {
            if (configuration.RankingProvider == null || !redis.isQueryableExists(redisPK))
            {
                return null;
            }
            string queryableWord = redis.GetQueryableWord(redisPK);
            if (string.IsNullOrEmpty(queryableWord))
            {
                return null;
            }
            List<string> partials = CreateAllPartialsForWord(queryableWord, configuration.WordIndexingMethod).ToList();
            
            return rankingHandler.BoostInRanking(redisPK,partials,multiplierCoefficient);
        }

        public IEnumerable<T> TopRankedSearches<T>(int limit = 0)
        {
            var topRankedResults = TopRankedSearches(limit).AsByte();
            var results = topRankedResults.Select(r => configuration.Serializer.Deserialize<T>(r));
            return results;
        }

        public IEnumerable<RedisValue> TopRankedSearches(int limit = 0)
        {
            return rankingHandler.TopRankedSearches(limit) ?? Enumerable.Empty<RedisValue>();
        }

        public double? CurrentScore(RedisKey redisPK)
        {
            return rankingHandler.CurrentScore(redisPK);
        }
        #endregion
        
        #region Private
        private List<string> CreateAllPartialsForWord(string word, WordIndexing method)
        {
            if (string.IsNullOrEmpty(word)) return new List<string>();
            string queryWord = word.RemoveSpecialCharacters().Replace(" ", String.Empty);

            List<string> prefixes = new List<string>();

            if (queryWord.Length < configuration.MinQueryLength) { return prefixes; }

            int startingIndex = 0;
            do
            {
                int endingIndex = startingIndex + configuration.MinQueryLength;
                while ((endingIndex <= queryWord.Length) && ((configuration.MaxQueryLength == -1) || (endingIndex <= (startingIndex + configuration.MaxQueryLength))))
                {
                    prefixes.Add(queryWord.Substring(startingIndex, endingIndex - startingIndex));
                    endingIndex++;
                }
                startingIndex++;
            } while (startingIndex <= queryWord.Length - configuration.MinQueryLength && method == WordIndexing.SequentialCombination);

            return prefixes;
        }
        #endregion

    }

}
