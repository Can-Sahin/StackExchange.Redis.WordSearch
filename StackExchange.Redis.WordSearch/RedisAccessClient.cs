using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;


namespace StackExchange.Redis.WordSearch
{
    internal class RedisAccessClient
    {
        private RedisKeyComposer keyManager { get; }
        private IDatabase RedisDatabase { get; }

        public RedisAccessClient(IDatabase database, RedisKeyComposer keyManager)
        {
            this.RedisDatabase = database;
            this.keyManager = keyManager;
        }

        internal bool AddQueryableWord(RedisKey redisPK, string word, byte[] data, List<string> partials)
        {
            string redisPKString = keyManager.AdjustedValue(redisPK);

            var tran = RedisDatabase.CreateTransaction();

            tran.HashSetAsync(keyManager.QueryableItemsDataKey, redisPKString, data);
            tran.HashSetAsync(keyManager.QueryableItemsKey, redisPKString, word);

            foreach (var subString in partials)
            {
                string sortedSetKey = keyManager.QueryKey(subString);
                tran.SortedSetAddAsync(sortedSetKey, redisPKString, 0);
            }
            return tran.Execute();

        }
        internal bool RemoveQueryableWord(RedisKey redisPK, List<string> partials)
        {
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
        internal string GetQueryableWord(RedisKey redisPK)
        {
            string redisPKString = keyManager.AdjustedValue(redisPK);
            string currentQueryableValue = RedisDatabase.HashGet(keyManager.QueryableItemsKey, redisPKString);
            return currentQueryableValue;
        }
        public bool SetQueryableWordsData(RedisKey redisPK, byte[] data)
        {
            string redisPKString = keyManager.AdjustedValue(redisPK);
            return RedisDatabase.HashSet(keyManager.QueryableItemsDataKey, redisPKString, data);
        }
        internal bool isQueryableExists(RedisKey redisPK)
        {
            string redisPKString = keyManager.AdjustedValue(redisPK);
            return RedisDatabase.HashExists(keyManager.QueryableItemsKey, redisPKString);
        }

        internal SortedSetEntry[] GetSearchEntries(string queryString, int limit)
        {
            string sortedSetKey = keyManager.QueryKey(queryString);
            var entries = RedisDatabase.SortedSetRangeByRankWithScores(sortedSetKey, 0, limit - 1, Order.Descending);
            return entries;
        }
        internal IEnumerable<RedisValue> GetDataOfQueryablePKs(List<RedisValue> pkList)
        {

            List<Task<RedisValue>> dataList = new List<Task<RedisValue>>();
            var tran = RedisDatabase.CreateTransaction();
            foreach (var redisPK in pkList)
            {
                dataList.Add(tran.HashGetAsync(keyManager.QueryableItemsDataKey, redisPK));
            }
            bool execute = tran.Execute();
            if (!execute)
            {
                //throw new TransactionExecuteFailedException("Search");
                return null;
            }
            Task.WaitAll(dataList.ToArray());
            var results = dataList.Select(d => (d.Result));
            return results;
        }

        internal bool IncrementScore(RedisKey redisPK, List<string> partials, double increment)
        {
            string redisPKString = keyManager.AdjustedValue(redisPK);

            var tran = RedisDatabase.CreateTransaction();

            tran.SortedSetIncrementAsync(keyManager.QueryableItemsRankingKey, redisPKString, increment);
            foreach (var subString in partials)
            {
                string sortedSetKey = keyManager.QueryKey(subString);
                tran.SortedSetIncrementAsync(sortedSetKey, redisPKString, increment);
            }

            return tran.Execute();
        }
        internal long TrimTopRankedQueryables(int size)
        {
            if(size <= 0) return 0;
            return RedisDatabase.SortedSetRemoveRangeByRank(keyManager.QueryableItemsRankingKey,0, -size);
        }
        internal double? CurrentScore(RedisKey redisPK)
        {
            string redisPKString = keyManager.AdjustedValue(redisPK);
            return RedisDatabase.SortedSetScore(keyManager.QueryableItemsRankingKey, redisPKString);
        }
        internal SortedSetEntry[] TopRankedQueryables(int limit = 0)
        {
            var entries = RedisDatabase.SortedSetRangeByRankWithScores(keyManager.QueryableItemsRankingKey, 0, limit - 1, Order.Descending);
            return entries;

        }
    }
}