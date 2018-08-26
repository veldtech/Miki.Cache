using Miki.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Miki.Cache.InMemory
{
	public class InMemoryCachePool : ICachePool
	{
		private readonly ConcurrentDictionary<string, byte[]> _cache = new ConcurrentDictionary<string, byte[]>();
		private readonly ISerializer _serializer;

		public InMemoryCachePool(ISerializer serializer)
		{
			_serializer = serializer;
		}

		public async Task<ICacheClient> GetAsync()
		{
			await Task.Yield();
			return new InMemoryCacheClient(_cache, _serializer);
		}
	}
}
