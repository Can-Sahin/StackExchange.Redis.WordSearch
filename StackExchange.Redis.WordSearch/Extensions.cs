using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace StackExchange.Redis.WordSearch
{
    /// <summary>
    /// Generic Extensions
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Remove all from string but numbers and letters
        /// </summary>
        public static string RemoveSpecialCharacters(this string str)
        {
            if (str == null) return null;
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || char.IsLetter(c) || c == '\'')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
        
        /// <summary>
        /// Cast to string
        /// </summary>
        public static IEnumerable<string> AsString(this IEnumerable<RedisValue> searchResults) => searchResults.Select(r => (string)r);
        
        /// <summary>
        /// Convert to string list
        /// </summary>
        public static List<string> AsStringList(this IEnumerable<RedisValue> searchResults) => searchResults.Select(r => (string)r).ToList();
        /// <summary>
        /// Cast to int
        /// </summary>
        public static IEnumerable<int> AsInt(this IEnumerable<RedisValue> searchResults) => searchResults.Select(r => (int)r);
        
        /// <summary>
        /// Convert to int list
        /// </summary>
        public static List<int> AsIntList(this IEnumerable<RedisValue> searchResults) => searchResults.Select(r => (int)r).ToList();
        
        /// <summary>
        /// Cast to long
        /// </summary>
        public static IEnumerable<long> AsLong(this IEnumerable<RedisValue> searchResults) => searchResults.Select(r => (long)r);
       
        /// <summary>
        /// Convert to long list
        /// </summary>
        public static List<long> AsLongList(this IEnumerable<RedisValue> searchResults) => searchResults.Select(r => (long)r).ToList();
        
        /// <summary>
        /// Cast to double
        /// </summary>
        public static IEnumerable<double> AsDouble(this IEnumerable<RedisValue> searchResults) => searchResults.Select(r => (double)r);

        /// <summary>
        /// Convert to double list
        /// </summary>
        public static List<double> AsDoubleList(this IEnumerable<RedisValue> searchResults) => searchResults.Select(r => (double)r).ToList();

        /// <summary>
        /// Cast to byte array
        /// </summary>
        public static IEnumerable<byte[]> AsByte(this IEnumerable<RedisValue> searchResults) => searchResults.Select(r => (byte[])r);

        /// <summary>
        /// Cast to bool list
        /// </summary>
        public static IEnumerable<bool> AsBool(this IEnumerable<RedisValue> searchResults) => searchResults.Select(r => (bool)r);

        /// <summary>
        /// Convert to bool list
        /// </summary>
        public static List<bool> AsBoolList(this IEnumerable<RedisValue> searchResults) => searchResults.Select(r => (bool)r).ToList();


    }
}
