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

        private RedisKeyManager keyManager { get; }
        private IRedisExceptionHandler ExceptionHandler { get;}
        private IDatabase RedisDatabase { get; }
        public RedisWordQuery(IDatabase database,RedisWordQueryConfiguration? configuration = null, IRedisExceptionHandler handler = null)
        {
            this.configuration = configuration ?? RedisWordQueryConfiguration.defaultConfig;
            this.ExceptionHandler = handler;
            this.RedisDatabase = database;
            this.keyManager = new RedisKeyManager(this.configuration);
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
            string redisPKString = keyManager.AdjustedValue(redisPK);
            string queryableValue = searchableValue ?? redisPK;
            byte[] data = embedData ?? Encoding.UTF8.GetBytes(queryableValue);

            if(updateOnExist && isQueryableExists(redisPK))
            {
                string currentQueryableValue = RedisDatabase.HashGet(keyManager.QueryableItemsKey, redisPKString);
                if (currentQueryableValue.Equals(queryableValue))
                {
                    return RedisDatabase.HashSet(keyManager.QueryableItemsDataKey, redisPKString, data);
                }
                else
                {
                    Remove(redisPK);
                }
            }
            var tran = RedisDatabase.CreateTransaction();

            tran.HashSetAsync(keyManager.QueryableItemsDataKey, redisPKString, data);
            tran.HashSetAsync(keyManager.QueryableItemsKey, redisPKString, queryableValue);

            List<string> words = WordsFromString(queryableValue);
            foreach (var word in words)
            {
                List<string> prefixes = CreateAllPrefixesForString(word,configuration.WordIndexingMethod);
                foreach (var subString in prefixes)
                {
                    string sortedSetKey = keyManager.QueryKey(subString);
                    tran.SortedSetAddAsync(sortedSetKey, redisPKString,0);
                }
            }
            bool execute = tran.Execute();
            return execute;
        }
        private bool isQueryableExists(RedisKey redisPK)
        {
            string redisPKString = keyManager.AdjustedValue(redisPK);
            return RedisDatabase.HashExists(keyManager.QueryableItemsKey, redisPKString);
        }

        public bool Remove(RedisKey redisPK)
        {
            string redisPKString = keyManager.AdjustedValue(redisPK);

            string queryableValue = RedisDatabase.HashGet(keyManager.QueryableItemsKey, redisPKString);

            var tran = RedisDatabase.CreateTransaction();

            List<string> words = WordsFromString(queryableValue);
            foreach (var word in words)
            {
                List<string> prefixes = CreateAllPrefixesForString(word, configuration.WordIndexingMethod);
                foreach (var subString in prefixes)
                {
                    string sortedSetKey = keyManager.QueryKey(subString);
                    tran.SortedSetRemoveAsync(sortedSetKey, redisPKString);
                }
            }
            tran.HashDeleteAsync(keyManager.QueryableItemsKey, redisPKString);
            tran.HashDeleteAsync(keyManager.QueryableItemsDataKey, redisPKString);

            bool execute = tran.Execute();
            return execute;
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

            //List<string> words = WordsFromString(queryString);

            //if(words.Count == 0) { return new List<string>(); }

            string sortedSetKey = keyManager.QueryKey(queryString);
            var entries = RedisDatabase.SortedSetRangeByRankWithScores(sortedSetKey, 0, limit-1);
            List<Tuple<string, double>> resultTuples = entries.Select(e => new Tuple<string, double>(e.Element, e.Score)).ToList();

            List<Task<RedisValue>> dataList = new List<Task<RedisValue>>();
            var tran = RedisDatabase.CreateTransaction();
            foreach (var resultTuple in resultTuples)
            {
                dataList.Add(tran.HashGetAsync(keyManager.QueryableItemsDataKey, resultTuple.Item1));
            }
            bool execute = tran.Execute();
            if (!execute)
            {
                throw new TransactionExecuteFailedException("Search");
            }
            Task.WaitAll(dataList.ToArray());
            var results = dataList.Select(d => (d.Result));
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
        private List<string> WordsFromString(string str)
        {
            if (string.IsNullOrEmpty(str)) return new List<string>();
            return new List<string>(str.RemoveSpecialCharacters().Split(' '));
        }
        private List<string> CreateAllPrefixesForString(string str, WordIndexing method)
        {
            List<string> prefixes = new List<string>();

            if (str.Length < configuration.MinQueryLength) { return prefixes; }

            int startingIndex = 0;
            do
            {
                int endingIndex = startingIndex + configuration.MinQueryLength;
                while ((endingIndex <= str.Length) && ((configuration.MaxQueryLength == -1) || (endingIndex <= (startingIndex + configuration.MaxQueryLength))))
                {
                    prefixes.Add(str.Substring(startingIndex, endingIndex - startingIndex));
                    endingIndex++;
                }
                startingIndex++;
            } while (startingIndex <= str.Length - configuration.MinQueryLength && method == WordIndexing.SequentialCombination);
       
            return prefixes;
        }

    }

}
