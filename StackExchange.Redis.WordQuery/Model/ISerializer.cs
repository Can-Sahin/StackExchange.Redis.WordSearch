using System;
using System.Collections.Generic;
using System.Text;

namespace StackExchange.Redis.WordQuery.Model
{
    public interface ISerializer
    {
        /// <summary>
        /// Serializes the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns>System.String.</returns>
        byte[] Serialize<T>(T value);
        /// <summary>
        /// Deserializes the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        T Deserialize<T>(byte[] value);
    }
}
