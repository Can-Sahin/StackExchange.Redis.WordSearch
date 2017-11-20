using StackExchange.Redis.WordQuery.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;


namespace StackExchange.Redis.WordQuery
{
    internal class RedisAccessClient 
    {
        private RedisKeyComposer keyManager { get; }
        private IDatabase RedisDatabase { get; }
        private IRedisExceptionHandler ExceptionHandler { get;}

        public RedisAccessClient(IDatabase database, RedisKeyComposer keyManager, IRedisExceptionHandler handler = null){
            this.ExceptionHandler = handler;
            this.RedisDatabase = database;
            this.keyManager = keyManager;
        }
        internal bool AddQueryableWord(RedisKey redisPK, string word, byte[] data, List<string> partials){
            string redisPKString = keyManager.AdjustedValue(redisPK);

            var tran = RedisDatabase.CreateTransaction();

            tran.HashSetAsync(keyManager.QueryableItemsDataKey, redisPKString, data);
            tran.HashSetAsync(keyManager.QueryableItemsKey, redisPKString, word);

            foreach (var subString in partials)
            {
                string sortedSetKey = keyManager.QueryKey(subString);
                tran.SortedSetAddAsync(sortedSetKey, redisPKString,0);
            }
            return tran.Execute();
        }
        internal bool RemoveQueryableWord(RedisKey redisPK, List<string> partials){
            string redisPKString = keyManager.AdjustedValue(redisPK);

            var tran = RedisDatabase.CreateTransaction();

            foreach (var subString in partials)
            {
                string sortedSetKey = keyManager.QueryKey(subString);
                tran.SortedSetRemoveAsync(sortedSetKey, redisPKString);
            }
            tran.HashDeleteAsync(keyManager.QueryableItemsKey, redisPKString);
            tran.HashDeleteAsync(keyManager.QueryableItemsDataKey, redisPKString);

            return tran.Execute();
        }
        internal string GetQueryableWord(RedisKey redisPK){
            string redisPKString = keyManager.AdjustedValue(redisPK);
            string currentQueryableValue = RedisDatabase.HashGet(keyManager.QueryableItemsKey, redisPKString);
            return currentQueryableValue;
        }
        public bool SetQueryableWordsData(RedisKey redisPK, byte[] data){
            string redisPKString = keyManager.AdjustedValue(redisPK);
            return RedisDatabase.HashSet(keyManager.QueryableItemsDataKey, redisPKString, data);
        }
        internal bool isQueryableExists(RedisKey redisPK)
        {
            string redisPKString = keyManager.AdjustedValue(redisPK);
            return RedisDatabase.HashExists(keyManager.QueryableItemsKey, redisPKString);
        }

        internal List<Tuple<RedisValue, double>> GetSearchEntries(string queryString, int limit)
        {
            string sortedSetKey = keyManager.QueryKey(queryString);
            var entries = RedisDatabase.SortedSetRangeByRankWithScores(sortedSetKey, 0, limit-1);
            return entries.Select(e => new Tuple<RedisValue, double>(e.Element, e.Score)).ToList();;
        }
        internal List<RedisValue> GetDataOfQueryablePKs(List<RedisValue> pkList){

            List<Task<RedisValue>> dataList = new List<Task<RedisValue>>();
            var tran = RedisDatabase.CreateTransaction();
            foreach (var redisPK in pkList)
            {
                dataList.Add(tran.HashGetAsync(keyManager.QueryableItemsDataKey, redisPK));
            }
            bool execute = tran.Execute();
            if (!execute)
            {
                throw new TransactionExecuteFailedException("Search");
            }
            Task.WaitAll(dataList.ToArray());
            var results = dataList.Select(d => (d.Result)).ToList();
            return results;
        }
    }
}