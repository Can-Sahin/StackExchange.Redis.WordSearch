using System;
using System.Collections.Generic;
using System.Linq;

namespace StackExchange.Redis.WordQuery
{
    public interface IRankingProvider
    {
        int TopRankedSize { get; set; }
    }
    public interface ICumulativeDecayRateRanking : IRankingProvider
    {
        double ScoreToIncrementNow(double multiplierCoefficient = 1);

    }

    internal class CumulativeDecayRateRankingHandler
    {
        private ICumulativeDecayRateRanking rankingClient { get; }
        private RedisAccessClient redis { get; }
        public CumulativeDecayRateRankingHandler(RedisAccessClient redisClient, ICumulativeDecayRateRanking rankingAlgorithm = null)
        {
            this.redis = redisClient;
            this.rankingClient = rankingAlgorithm;
        }
        internal double? BoostInRanking(RedisKey redisPK, List<string> partials, double multiplierCoefficient)
        {
            if (rankingClient == null) throw new RankingAlgorithmNotFound();
            double scoreToIncrement = rankingClient.ScoreToIncrementNow(multiplierCoefficient);
            bool success = redis.IncrementScore(redisPK, partials, scoreToIncrement);
            if (!success) return null;
            if (rankingClient.TopRankedSize > 0)
            {
                redis.TrimTopRankedQueryables(rankingClient.TopRankedSize);
            }
            return scoreToIncrement;
        }
        public IEnumerable<RedisValue> TopRankedSearches(int limit = 0)
        {
            List<RedisValue> pkList = redis.TopRankedQueryables(limit).Select(r => r.Element).ToList();
            var results = redis.GetDataOfQueryablePKs(pkList);
            return results;
        }

        public double? CurrentScore(RedisKey redisPK)
        {
            return redis.CurrentScore(redisPK);
        }

    }

    //From: http://qwerjk.com/posts/surfacing-interesting-content/
    public struct CurrentlyPopularRanking : ICumulativeDecayRateRanking
    {
        public long ConstEPOCH { get; }
        public double HALFLIFE { get; } // Hour

        public int TopRankedSize { get; set; }
        public double HalfLifeInSeconds
        {
            get { return HALFLIFE * 60 * 60; }
        }
        public CurrentlyPopularRanking(long constEPOCH, double halfLifeInHours, int topRankedSize = -1)
        {
            this.ConstEPOCH = constEPOCH;
            this.HALFLIFE = halfLifeInHours;
            this.TopRankedSize = topRankedSize;
        }
        public double ScoreToIncrementNow(double multiplierCoefficient = 1)
        {
            double power = (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - ConstEPOCH) / HalfLifeInSeconds;
            if (power + (int)(multiplierCoefficient / 2) >= 52)
            {
                Console.WriteLine("Ranking scores are at bit limit for sorted set scores. Consider migrating!!!!!");
            }
            double increment = multiplierCoefficient * Math.Pow(2, power);
            if (increment > 9007199254740992) // 2^53
            {
                throw new CurrentlyPopularityRankingAlgorithmOutOfLimit();
            }
            return increment;
        }


    }
}