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
            this.keyManager = new RedisKeyManager(this.configuration.ContainerPrefix, this.configuration.ParameterSeperator);
        }

        public void Add(RedisKey redisPK, string searchableValue = null, string embedData = null, bool updateOnExist = true, CommandFlags commandFlag = CommandFlags.None)
        {
            string redisPKString = redisPK.ToString();
            string queryableValue = searchableValue ?? redisPK;
            string data = embedData ?? queryableValue;

            if(updateOnExist && isQueryableExists(redisPK))
            {
                string currentQueryableValue = RedisDatabase.HashGet(keyManager.QueryableItemsKey, redisPKString);
                if (currentQueryableValue.Equals(queryableValue))
                {
                    RedisDatabase.HashSet(keyManager.QueryableItemsDataKey, redisPKString, data);
                    return;
                }
                else
                {
                    Remove();
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

        }
        private bool isQueryableExists(RedisKey redisPK)
        {
            return RedisDatabase.HashExists(keyManager.QueryableItemsKey, redisPK.ToString());
        }

        public void AddObject()
        {

        }
        public void Remove()
        {

        }
        public void Update()
        {

        }
        public void DeleteAll()
        {
            
        }

        public List<T> Search<T>(string queryString, int limit = 0, Func<T,bool> filterFunc = null)
        {
            if (configuration.Serializer == null)
            {
                throw new SerializerNotFoundException();
            }
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
        public List<string> CreateAllPrefixesForString(string str, WordIndexing method)
        {
            List<string> prefixes = new List<string>();

            if (str.Length < configuration.MinPrefixLength) { return prefixes; }

            int startingIndex = 0;
            do
            {
                int endingIndex = startingIndex + configuration.MinPrefixLength;
                while ((endingIndex <= str.Length) && ((configuration.MaxPrefixLength == -1) || (endingIndex <= (startingIndex + configuration.MaxPrefixLength))))
                {
                    prefixes.Add(str.Substring(startingIndex, endingIndex - startingIndex));
                    endingIndex++;
                }
                startingIndex++;
            } while (startingIndex <= str.Length - configuration.MinPrefixLength && method == WordIndexing.SequentialCombination);
       
            return prefixes;
        }

    }

}
