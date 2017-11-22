using System;
using System.Collections.Generic;
using System.Linq;

namespace StackExchange.Redis.WordSearch
{
    internal class CumulativeDecayRateRankingHandler
    {
        private IRelativeDecayRateRanking rankingClient { get; }
        private RedisAccessClient redis { get; }
        public CumulativeDecayRateRankingHandler(RedisAccessClient redisClient, IRelativeDecayRateRanking rankingAlgorithm = null)
        {
            this.redis = redisClient;
            this.rankingClient = rankingAlgorithm;
        }
        internal double? BoostInRanking(RedisKey redisPK, List<string> partials, double multiplierCoefficient)
        {
            if (rankingClient == null) throw new RankingAlgorithmNotFound();
            double scoreToIncrement = rankingClient.ScoreToIncrementNow(multiplierCoefficient);
            double? newScore = redis.IncrementScore(redisPK, partials, scoreToIncrement);
            if (!newScore.HasValue) return null;
            if (rankingClient.TopRankedSize > 0)
            {
                redis.TrimTopRankedSearchables(rankingClient.TopRankedSize);
            }
            return scoreToIncrement;
        }
        
        public IEnumerable<RedisValue> TopRankedSearches(int limit = 0)
        {
            List<RedisValue> pkList = redis.TopRankedSearchables(limit).Select(r => r.Element).ToList();
            var results = redis.GetDataOfSearchablePKs(pkList);
            return results;
        }

        public double? CurrentScore(RedisKey redisPK)
        {
            return redis.CurrentScore(redisPK);
        }

    }
 
}