using System;
using System.Collections.Generic;
using System.Text;

namespace StackExchange.Redis.WordQuery
{
    public interface IRedisWordQuery
    {
        /// <summary>
        /// Read-only configuration settings
        /// </summary>
        /// <returns></returns>
        RedisWordQueryConfiguration configuration { get; }

        /// <summary>
        /// Adds string to be searched
        /// </summary>
        /// <param name="redisPK">PrimaryKey(Unique) value in redis</param>
        /// <param name="searchableValue">String to search later</param>
        /// <param name="embedData">String to be retrieved from search</param>
        /// <param name="encoding">Encoding of Data string</param>
        /// <param name="updateOnExist"></param>
        /// <returns></returns>      

        bool Add(RedisKey redisPK, string searchableValue, string embedData, Encoding encoding = null, bool updateOnExist = true);
        /// <summary>
        /// Adds string to be searched with serialized data 
        /// </summary>
        /// <param name="redisPK">PrimaryKey(Unique) value in redis</param>
        /// <param name="searchableValue">String to search later</param>
        /// <param name="embedData"> Serialized data to be retrieved from search</param>
        /// <param name="updateOnExist"></param>
        /// <returns></returns>
        bool AddObject<T>(RedisKey redisPK, string searchableValue, T embedData, bool updateOnExist = true);

        /// <summary>
        /// Adds string to be searched with raw data 
        /// </summary>
        /// <param name="redisPK">PrimaryKey(Unique) value in redis</param>
        /// <param name="searchableValue">String to search later</param>
        /// <param name="embedData"> Raw data to be retrieved from search</param>
        /// <param name="updateOnExist"></param>  
        /// <returns></returns>        

        bool Add(RedisKey redisPK, string searchableValue = null, byte[] embedData = null, bool updateOnExist = true);

        /// <summary>
        /// Updates string to be searched
        /// </summary>
        /// <param name="redisPK">PrimaryKey(Unique) value of previosuly added item in redis</param>
        /// <param name="searchableValue">Search string to replace</param>
        /// <param name="embedData"> Raw data to replace </param>
        /// <param name="onlyIfExists"></param>
        /// <returns></returns>    
        bool Update(RedisKey redisPK, string searchableValue = null, byte[] embedData = null, bool onlyIfExists = true);

        /// <summary>
        /// Removes primary key from searchables
        /// </summary>
        /// <param name="redisPK">PrimaryKey(Unique) value of previosuly added item in redis</param>
        /// <returns></returns>
        bool Remove(RedisKey redisPK);

        /// <summary>
        /// Search and deserialize for a single result.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryString">String to search</param>
        T SearchSingle<T>(string queryString);

        /// <summary>
        /// Search for a string and deserialize the result
        /// </summary>
        /// <param name="queryString">String to search</param>
        /// <param name="limit">Max number of results</param>
        /// <param name="filterFunc">Filter function</param>
        /// <returns>List of serialized results</returns>
        List<T> Search<T>(string queryString, int limit = 0, Func<T, bool> filterFunc = null);

        /// <summary>
        /// Search for a single result.
        /// </summary>
        /// <param name="queryString">String to search</param>
        RedisValue SearchSingle(string queryString);

        /// <summary>
        /// Search for a single result, default if not successfull
        /// </summary>
        /// <param name="queryString">String to search</param>
        /// <param name="defaultValue"></param>
        RedisValue SearchSingleOrDefault(string queryString, RedisValue defaultValue = default(RedisValue));

        /// <summary>
        /// Search for a string
        /// </summary>
        /// <param name="queryString">String to search</param>
        /// <param name="limit">Max number of results</param>
        /// <param name="filterFunc">Filter function</param>
        /// <returns>Enumarable results</returns>
        IEnumerable<RedisValue> Search(string queryString, int limit = 0, Func<RedisValue, bool> filterFunc = null);

    }
}