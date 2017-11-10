using System;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

namespace StackExchange.Redis.WordQuery
{
    public enum WordIndexing { SequentialOnly, SequentialCombination }

    public interface IRedisWordQueryExceptionHandler
    {

    }
    public interface IRedisPK
    {
        RedisKey RedisPK { get; set; }
    }
    public interface ISerializer
    {
        byte[] Serialize<T>(T value);
        T Deserialize<T>(byte[] value);

    }
    public class SerializerNotFoundException : Exception
    {
        public SerializerNotFoundException() : base("There is no serializer found for search query") { }
    }
    public class TransactionExecuteFailedException : Exception
    {
        public TransactionExecuteFailedException(string message) : base("Cannot execute transaction: "+ message) { }
    }

    public struct RedisWordQueryConfiguration{

        public int MinPrefixLength { get; set; }
        public int MaxPrefixLength { get; set; }
        public bool IsCaseSensitive { get; set; }
        public WordIndexing WordIndexingMethod { get; set; }

        public string ParameterSeperator { get; set; }
        public string ContainerPrefix { get; set; }

        public ISerializer Serializer { get; set; }

        public static RedisWordQueryConfiguration defaultConfig = new RedisWordQueryConfiguration
        {
            MinPrefixLength = 1,
            MaxPrefixLength = -1,
            IsCaseSensitive = false,
            WordIndexingMethod = WordIndexing.SequentialOnly,
            ParameterSeperator = RedisKeyAccessManager.DefaultSeperator,
            ContainerPrefix = RedisKeyAccessManager.DefaultContainerPrefix
        };
    }

    internal class RedisKeyAccessManager
    {
        internal const string DefaultSeperator = ":::";
        internal const string DefaultContainerPrefix = "WQ";
        internal const String QueryableItemsSuffix = "Queryable";
        internal const String QueryableItemsDataSuffix = "QueryableData";
        internal const String QuerySuffix = "Query";

        private string Seperator { get; }
        private string ContainerPrefix { get; }
        public RedisKeyAccessManager(string containerPrefix, string parameterSeperator)
        {
            Seperator = string.IsNullOrEmpty(parameterSeperator) ? DefaultSeperator : parameterSeperator;
            ContainerPrefix = string.IsNullOrEmpty(containerPrefix) ? DefaultContainerPrefix : containerPrefix;
        }
        internal string CompactRedisKey(RedisKey pk, params string[] param)
        {
            string parametersSuffix = "";
            if(param.Length > 0)
            {
                parametersSuffix = Seperator + string.Join(Seperator, param);
            }
            return ContainerPrefix + Seperator + pk.ToString() + parametersSuffix;
        }
        internal string QueryKey(string subString){

            return CompactRedisKey(QuerySuffix, subString);
        }
        internal string QueryableItemsKey { get { return CompactRedisKey(QueryableItemsSuffix); } }
        internal string QueryableItemsDataKey { get { return CompactRedisKey(QueryableItemsDataSuffix); } }

    }
    public class RedisWordQuery
    {
        public RedisWordQueryConfiguration configuration {get;}

        private RedisKeyAccessManager keyManager { get; }
        private IRedisWordQueryExceptionHandler ExceptionHandler { get;}
        private IDatabase RedisDatabase { get; }
        public RedisWordQuery(IDatabase database,RedisWordQueryConfiguration? configuration = null, IRedisWordQueryExceptionHandler handler = null)
        {
            this.configuration = configuration ?? RedisWordQueryConfiguration.defaultConfig;
            this.ExceptionHandler = handler;
            this.RedisDatabase = database;
            this.keyManager = new RedisKeyAccessManager(this.configuration.ContainerPrefix, this.configuration.ParameterSeperator);
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
