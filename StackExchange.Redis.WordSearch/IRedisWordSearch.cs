using System;
using System.Collections.Generic;
using System.Text;

namespace StackExchange.Redis.WordSearch
{
    /// <summary>
    /// RedisWordSearch interface
    /// </summary>
    public interface IRedisWordSearch
    {
        /// <summary>
        /// Read-only configuration settings
        /// </summary>
        RedisWordSearchConfiguration configuration { get; }

        /// <summary>
        /// Adds search item with string data
        /// </summary>
        /// <param name="redisPK">PrimaryKey(Unique) value in redis</param>
        /// <param name="searchableValue">String to search later</param>
        /// <param name="embedData">String to be retrieved from search</param>
        /// <param name="encoding">Encoding of Data string</param>
        /// <param name="updateOnExist"></param>
        bool Add(RedisKey redisPK, string searchableValue, string embedData, Encoding encoding = null, bool updateOnExist = true);

        /// <summary>
        /// Adds search item with serialized data 
        /// </summary>
        /// <param name="redisPK">PrimaryKey(Unique) value in redis</param>
        /// <param name="searchableValue">String to search later</param>
        /// <param name="embedData"> Serialized data to be retrieved from search</param>
        /// <param name="updateOnExist"></param>
        bool AddObject<T>(RedisKey redisPK, string searchableValue, T embedData, bool updateOnExist = true);

        /// <summary>
        /// Adds search item with raw data 
        /// </summary>
        /// <param name="redisPK">PrimaryKey(Unique) value in redis</param>
        /// <param name="searchableValue">String to search later</param>
        /// <param name="embedData"> Raw data to be retrieved from search</param>
        /// <param name="updateOnExist"></param>  
        bool Add(RedisKey redisPK, string searchableValue = null, byte[] embedData = null, bool updateOnExist = true);

        /// <summary>
        /// Update the search item
        /// </summary>
        /// <param name="redisPK">PrimaryKey(Unique) value of previosuly added item in redis</param>
        /// <param name="searchableValue">Search string to replace</param>
        /// <param name="embedData"> Raw data to replace </param>
        /// <param name="onlyIfExists"></param>
        bool Update(RedisKey redisPK, string searchableValue = null, byte[] embedData = null, bool onlyIfExists = true);

        /// <summary>
        /// Remove the search item
        /// </summary>
        /// <param name="redisPK">PrimaryKey(Unique) value of previosuly added item in redis</param>
        bool Remove(RedisKey redisPK);

        /// <summary>
        /// Search and deserialize for a single result.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="searchString">String to search</param>
        T SearchSingle<T>(string searchString);

        /// <summary>
        /// Search for a string and deserialize the result
        /// </summary>
        /// <param name="searchString">String to search</param>
        /// <param name="limit">Max number of results</param>
        /// <param name="filterFunc">Filter function</param>
        /// <returns>List of serialized results</returns>
        List<T> Search<T>(string searchString, int limit = 0, Func<T, bool> filterFunc = null);

        /// <summary>
        /// Search for a single result.
        /// </summary>
        /// <param name="searchString">String to search</param>
        RedisValue SearchSingle(string searchString);

        /// <summary>
        /// Search for a single result, default if not successfull
        /// </summary>
        /// <param name="searchString">String to search</param>
        /// <param name="defaultValue"></param>
        RedisValue SearchSingleOrDefault(string searchString, RedisValue defaultValue = default(RedisValue));

        /// <summary>
        /// Search for a string
        /// </summary>
        /// <param name="searchString">String to search</param>
        /// <param name="limit">Max number of results</param>
        /// <param name="filterFunc">Filter function</param>
        /// <returns>Enumarable results</returns>
        IEnumerable<RedisValue> Search(string searchString, int limit = 0, Func<RedisValue, bool> filterFunc = null);

        /// <summary>
        /// Tells its RankingPRovider to increment ranking score of the search item.
        /// </summary>
        /// <param name="redisPK">PrimaryKey(Unique) value of previosuly added item in redis</param>
        /// <param name="multiplierCoefficient">Paramater for RankingProvider to use when incrementing</param>
        /// <returns>Incremented Score or null if item doesn't exits</returns>
        double? BoostInRanking(RedisKey redisPK, double multiplierCoefficient = 1);
     
        /// <summary>
        /// Returns deserialized search items in descending order by scores
        /// </summary>
        /// <param name="limit"></param>
        /// <returns>Enumarable results</returns>
        IEnumerable<T> TopRankedSearches<T>(int limit = 0);

        /// <summary>
        /// Returns search items in descending order by scores
        /// </summary>
        /// <param name="limit"></param>
        /// <returns>Enumarable results</returns>
        IEnumerable<RedisValue> TopRankedSearches(int limit = 0);
  
        /// <summary>
        /// Current score of search item. Nil if item doesn't exits
        /// </summary>
        /// <param name="redisPK">PrimaryKey(Unique) value of previosuly added item in redis</param>
        /// <returns>Score or null if item doesn't exits</returns>
        double? CurrentScore(RedisKey redisPK);
        
    }
}