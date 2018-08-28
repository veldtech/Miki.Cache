using Miki.Cache.Extensions;
using System;
using System.Text;

namespace Miki.Cache
{
    public static class ExtensionMethods
    {
		public static IHashSet<T> CreateHashSet<T>(this IExtendedCacheClient cacheClient, string key)
		{
			return new HashSet<T>(cacheClient, key);
		}
    }
}
