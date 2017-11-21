using System;

namespace StackExchange.Redis.WordQuery
{

    public interface IRankingProvider
    {
        double ScoreToIncrementNow(double multiplierCoefficient = 1);
    }
    //From: http://qwerjk.com/posts/surfacing-interesting-content/
    public struct PopularStreamRanking : IRankingProvider
    {
        public long ConstEPOCH {get;} 
        public double HALFLIFE {get;} // Hour

        public double HalfLifeInSeconds
        {
            get { return HALFLIFE * 60 * 60; }
        }
        public PopularStreamRanking(long constEPOCH, double halfLifeInHours){
            this.ConstEPOCH = constEPOCH;
            this.HALFLIFE  = halfLifeInHours;
        }
        public double ScoreToIncrementNow(double multiplierCoefficient = 1)
        {
            var result = multiplierCoefficient * Math.Pow(2, ((DateTimeOffset.UtcNow.ToUnixTimeSeconds() - ConstEPOCH) / HalfLifeInSeconds));
            return multiplierCoefficient * Math.Pow(2, ((DateTimeOffset.UtcNow.ToUnixTimeSeconds() - ConstEPOCH) / HalfLifeInSeconds)); 
        }
        
        
    }
}