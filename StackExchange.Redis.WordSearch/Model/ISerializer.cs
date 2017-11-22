using System;
using System.Collections.Generic;
using System.Text;

namespace StackExchange.Redis.WordSearch
{
    public interface ISerializer
    {
        /// <summary>
        /// Serializes the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns>System.Byte</returns>
        byte[] Serialize<T>(T value);
        /// <summary>
        /// Deserializes the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        T Deserialize<T>(byte[] value);
    }
}
