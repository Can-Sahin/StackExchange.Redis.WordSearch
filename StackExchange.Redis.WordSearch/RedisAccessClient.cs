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

        internal bool AddSearchableWord(RedisKey redisPK, string word, byte[] data, List<string> partials)
        {
            string redisPKString = keyManager.AdjustedValue(redisPK);

            var tran = RedisDatabase.CreateTransaction();

            tran.HashSetAsync(keyManager.SearchableItemsDataKey, redisPKString, data);
            tran.HashSetAsync(keyManager.SearchableItemsKey, redisPKString, word);

            foreach (var subString in partials)
            {
                string sortedSetKey = keyManager.SearchKey(subString);
                tran.SortedSetAddAsync(sortedSetKey, redisPKString, 0);
            }
            return tran.Execute();

        }
        internal bool RemoveSearchableWord(RedisKey redisPK, List<string> partials)
        {
            string redisPKString = keyManager.AdjustedValue(redisPK);

            var tran = RedisDatabase.CreateTransaction();

            foreach (var subString in partials)
            {
                string sortedSetKey = keyManager.SearchKey(subString);
                tran.SortedSetRemoveAsync(sortedSetKey, redisPKString);
            }
            tran.HashDeleteAsync(keyManager.SearchableItemsKey, redisPKString);
            tran.HashDeleteAsync(keyManager.SearchableItemsDataKey, redisPKString);

            return tran.Execute();
        }
        internal string GetSearchableWord(RedisKey redisPK)
        {
            string redisPKString = keyManager.AdjustedValue(redisPK);
            string currentSearchableValue = RedisDatabase.HashGet(keyManager.SearchableItemsKey, redisPKString);
            return currentSearchableValue;
        }
        public bool SetSearchableWordsData(RedisKey redisPK, byte[] data)
        {
            string redisPKString = keyManager.AdjustedValue(redisPK);
            return RedisDatabase.HashSet(keyManager.SearchableItemsDataKey, redisPKString, data);
        }
        internal bool isSearchableExists(RedisKey redisPK)
        {
            string redisPKString = keyManager.AdjustedValue(redisPK);
            return RedisDatabase.HashExists(keyManager.SearchableItemsKey, redisPKString);
        }

        internal SortedSetEntry[] GetSearchEntries(string searchString, int limit)
        {
            string sortedSetKey = keyManager.SearchKey(searchString);
            var entries = RedisDatabase.SortedSetRangeByRankWithScores(sortedSetKey, 0, limit - 1, Order.Descending);
            return entries;
        }
        internal IEnumerable<RedisValue> GetDataOfSearchablePKs(List<RedisValue> pkList)
        {

            List<Task<RedisValue>> dataList = new List<Task<RedisValue>>();
            var tran = RedisDatabase.CreateTransaction();
            foreach (var redisPK in pkList)
            {
                dataList.Add(tran.HashGetAsync(keyManager.SearchableItemsDataKey, redisPK));
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

        internal double? IncrementScore(RedisKey redisPK, List<string> partials, double increment)
        {
            string redisPKString = keyManager.AdjustedValue(redisPK);
            Task<double> newScore;
            var tran = RedisDatabase.CreateTransaction();

            newScore = tran.SortedSetIncrementAsync(keyManager.SearchableItemsRankingKey, redisPKString, increment);
            foreach (var subString in partials)
            {
                string sortedSetKey = keyManager.SearchKey(subString);
                tran.SortedSetIncrementAsync(sortedSetKey, redisPKString, increment);
            }

            bool execute = tran.Execute();
            if(!execute){
                return null;
            }
            newScore.Wait();
            return newScore.Result;
        }
        internal long TrimTopRankedSearchables(int size)
        {
            if(size <= 0) return 0;
            return RedisDatabase.SortedSetRemoveRangeByRank(keyManager.SearchableItemsRankingKey,0, -size);
        }
        internal double? CurrentScore(RedisKey redisPK)
        {
            string redisPKString = keyManager.AdjustedValue(redisPK);
            return RedisDatabase.SortedSetScore(keyManager.SearchableItemsRankingKey, redisPKString);
        }
        internal SortedSetEntry[] TopRankedSearchables(int limit = 0)
        {
            var entries = RedisDatabase.SortedSetRangeByRankWithScores(keyManager.SearchableItemsRankingKey, 0, limit - 1, Order.Descending);
            return entries;

        }
    }
}