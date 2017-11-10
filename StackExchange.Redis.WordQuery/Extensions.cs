using System;
using System.Collections.Generic;
using System.Text;

namespace StackExchange.Redis.WordQuery
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
    }
}
