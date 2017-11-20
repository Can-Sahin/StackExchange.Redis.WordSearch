using System;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using StackExchange.Redis.WordQuery.Model;

namespace StackExchange.Redis.WordQuery
{
    public enum WordIndexing { SequentialOnly, SequentialCombination }

    public class RedisWordQuery
    {
        public RedisWordQueryConfiguration configuration {get;}
 
        private RedisAccessClient redis { get; }

        public RedisWordQuery(IDatabase database,RedisWordQueryConfiguration? configuration = null, IRedisExceptionHandler handler = null)
        {
            this.configuration = configuration ?? RedisWordQueryConfiguration.defaultConfig;
            this.keyManager = new RedisKeyComposer(this.configuration);
        }
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

            if(updateOnExist && redis.isQueryableExists(redisPK))
            {
                string currentQueryableValue = redis.GetQueryableWord(redisPK);
                if (currentQueryableValue.Equals(queryableWord))
                {
                    return redis.SetQueryableWordsData(redisPK,data);
                }
                else
                {
                    Remove(redisPK);
                }
            }
            List<string> partials = CreateAllPartialsForWord(queryableWord,configuration.WordIndexingMethod).ToList();
            return redis.AddQueryableWord(redisPK,queryableWord,data,partials);
        }

        public bool Remove(RedisKey redisPK)
        {
            string queryableWord = redis.GetQueryableWord(redisPK);
            List<string> partials = CreateAllPartialsForWord(queryableWord,configuration.WordIndexingMethod).ToList();

            return redis.RemoveQueryableWord(redisPK,partials);
        }
        public List<T> Search<T>(string queryString, int limit = 0, Func<T,bool> filterFunc = null)
        {
            var searchResults = Search(queryString, limit);
            var results = searchResults.Select(d => configuration.Serializer.Deserialize<T>(d));
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

        public List<RedisValue> Search(string queryString, int limit = 0, Func<RedisValue, bool> filterFunc = null)
        {
            List<Tuple<RedisValue, double>> searchResults = redis.GetSearchEntries(queryString,limit);
            var results = redis.GetDataOfQueryablePKs(searchResults.Select(r =>r.Item1).ToList());

            if(filterFunc != null)
            {
                var filteredResults = results.Where(r => !filterFunc(r));
                return filteredResults.ToList();
            }
            else
            {
                return results.ToList();
            }
        }

        private List<string> CreateAllPartialsForWord(string word, WordIndexing method)
        {
            if (string.IsNullOrEmpty(word)) return new List<string>();
            string queryWord = word.RemoveSpecialCharacters().Replace(" ",String.Empty);

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

    }

}
