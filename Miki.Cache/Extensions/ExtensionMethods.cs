using Miki.Cache.Extensions;
using System;
using System.Text;

namespace Miki.Cache
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Creates an hash set struct for your cache client, exposing values in a more user-friendly way. 
        /// </summary>
        /// <typeparam name="T">Type of value you want to store.</typeparam>
        /// <param name="cacheClient">The cache client reference created.</param>
        /// <param name="key">The key at which the hash set will be stored in the cache.</param>
		public static IHashSet<T> CreateHashSet<T>(this IExtendedCacheClient cacheClient, string key)
		{
			return new HashSet<T>(cacheClient, key);
		}
    }
}
