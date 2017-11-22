using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace StackExchange.Redis.WordSearch
{
    public static class Extensions
    {
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
        public static IEnumerable<string> AsString(this IEnumerable<RedisValue> searchResults) => searchResults.Select(r => (string)r);
        public static List<string> AsStringList(this IEnumerable<RedisValue> searchResults) => searchResults.Select(r => (string)r).ToList();
        public static IEnumerable<int> AsInt(this IEnumerable<RedisValue> searchResults) => searchResults.Select(r => (int)r);
        public static List<int> AsIntList(this IEnumerable<RedisValue> searchResults) => searchResults.Select(r => (int)r).ToList();
        public static IEnumerable<long> AsLong(this IEnumerable<RedisValue> searchResults) => searchResults.Select(r => (long)r);
        public static List<long> AsLongList(this IEnumerable<RedisValue> searchResults) => searchResults.Select(r => (long)r).ToList();
        public static IEnumerable<double> AsDouble(this IEnumerable<RedisValue> searchResults) => searchResults.Select(r => (double)r);
        public static List<double> AsDoubleList(this IEnumerable<RedisValue> searchResults) => searchResults.Select(r => (double)r).ToList();
        public static IEnumerable<byte[]> AsByte(this IEnumerable<RedisValue> searchResults) => searchResults.Select(r => (byte[])r);
        public static IEnumerable<bool> AsBool(this IEnumerable<RedisValue> searchResults) => searchResults.Select(r => (bool)r);
        public static List<bool> AsBoolList(this IEnumerable<RedisValue> searchResults) => searchResults.Select(r => (bool)r).ToList();


    }
}
