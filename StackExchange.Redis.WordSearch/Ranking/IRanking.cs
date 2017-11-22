namespace StackExchange.Redis.WordSearch
{
    public interface IRankingProvider
    {
        /// <summary>
        /// Max number of items to keep in top ranked items list
        /// </summary>
        int TopRankedSize { get; set; }
    }
    /// <summary>
    /// Cumulative Decay Rate interface. Must know how much to increment the search item's score at any moment
    /// </summary>
    public interface IRelativeDecayRateRanking : IRankingProvider
    {
        /// <summary>
        /// Increment value of search item when requested
        /// </summary>
        /// <param name="multiplierCoefficient">Weight of the increment</param>
        double ScoreToIncrementNow(double multiplierCoefficient = 1);

    }
}