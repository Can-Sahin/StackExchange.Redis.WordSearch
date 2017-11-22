using System;

namespace StackExchange.Redis.WordSearch
{
       /// <summary>
    /// Currently popular ranking algorithm
    /// From: http://qwerjk.com/posts/surfacing-interesting-content/
    /// </summary>
    public struct CurrentlyPopularRanking : IRelativeDecayRateRanking
    {
        /// <summary>
        /// EPOCH for anchoring the half life. Check documentation
        /// </summary>
        public long ConstEPOCH { get; }

        /// <summary>
        /// Half life of popularity algorithm in hours
        /// </summary>
        public double HALFLIFE { get; } // Hour

        /// <summary>
        /// Max number of items to keep in top ranked items list
        /// </summary>
        public int TopRankedSize { get; set; }

        private double HalfLifeInSeconds
        {
            get { return HALFLIFE * 60 * 60; }
        }
        /// <summary>
        /// Default contstructor
        /// </summary>
        /// <param name="constEPOCH"></param>
        /// <param name="halfLifeInHours"></param>
        /// <param name="topRankedSize"></param>
        public CurrentlyPopularRanking(long constEPOCH, double halfLifeInHours, int topRankedSize = -1)
        {
            this.ConstEPOCH = constEPOCH;
            this.HALFLIFE = halfLifeInHours;
            this.TopRankedSize = topRankedSize;
        }
        /// <summary>
        /// Calculates current score to increment
        /// </summary>
        /// <param name="multiplierCoefficient">Weight of the score</param>
        /// <returns></returns>
        public double ScoreToIncrementNow(double multiplierCoefficient = 1)
        {
            double power = (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - ConstEPOCH) / HalfLifeInSeconds;
            double increment = multiplierCoefficient * Math.Pow(2, power);
            if (Double.IsInfinity(increment))
            {
                throw new CurrentPopularityRankingAlgorithmOutOfLimit();
            }
            return increment;
        }
    }
}