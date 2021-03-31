#region Copyright
//=======================================================================================
// Author: Paolo Salvatori
// GitHub: https://github.com/paolosalvatori
//=======================================================================================
// Copyright © 2021 Microsoft Corporation. All rights reserved.
// 
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER 
// EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF 
// MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE. YOU BEAR THE RISK OF USING IT.
//=======================================================================================
#endregion

#region Using Directives

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using StackExchange.Redis;
#endregion

namespace Products.Helpers
{
    public static class StackExchangeExtensions
    {
        #region Public Instance Methods
        public static T Get<T>(this IDatabase cache, string key)
        {
            return Deserialize<T>(cache.StringGet(key));
        }

        public async static Task<T> GetAsync<T>(this IDatabase cache, string key)
        {
            return Deserialize<T>(await cache.StringGetAsync(key));
        }

        public static IEnumerable<T> Get<T>(this IDatabase cache, string[] keys)
        {
            return Deserialize<T>(cache.StringGet(keys.Select(key => (RedisKey)key).ToArray()));
        }

        public async static Task<IEnumerable<T>> GetAsync<T>(this IDatabase cache, string[] keys)
        {
            return Deserialize<T>(await cache.StringGetAsync(keys.Select(key => (RedisKey)key).ToArray()));
        }

        public static object Get(this IDatabase cache, string key)
        {
            return Deserialize<object>(cache.StringGet(key));
        }

        public async static Task<object> GetAsync(this IDatabase cache, string key)
        {
            return Deserialize<object>(await cache.StringGetAsync(key));
        }

        public static object Get(this IDatabase cache, string[] keys)
        {
            return Deserialize<object>(cache.StringGet(keys.Select(key => (RedisKey)key).ToArray()));
        }

        public async static Task<object> GetAsync(this IDatabase cache, string[] keys)
        {
            return Deserialize<object>(await cache.StringGetAsync(keys.Select(key => (RedisKey)key).ToArray()));
        }

        public static void Set(this IDatabase cache, string key, object value)
        {
            cache.StringSet(key, Serialize(value));
        }

        public static Task<bool> SetAsync(this IDatabase cache, string key, object value)
        {
            return cache.StringSetAsync(key, Serialize(value));
        }
        #endregion

        #region Private Static Methods
        private static byte[] Serialize(object o)
        {
            if (o == null)
            {
                return null;
            }

            var binaryFormatter = new BinaryFormatter();
            using (var memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, o);
                byte[] objectDataAsStream = memoryStream.ToArray();
                return objectDataAsStream;
            }
        }

        private static IEnumerable<T> Deserialize<T>(IEnumerable<RedisValue> values)
        {
            return values.Select(v => Deserialize<T>(v));
        }

        private static T Deserialize<T>(byte[] stream)
        {
            if (stream == null)
            {
                return default(T);
            }

            var binaryFormatter = new BinaryFormatter();
            using (var memoryStream = new MemoryStream(stream))
            {
                var result = (T)binaryFormatter.Deserialize(memoryStream);
                return result;
            }
        }
        #endregion
    }
}