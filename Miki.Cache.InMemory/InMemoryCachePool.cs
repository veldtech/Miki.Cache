using System;
using System.Collections.Generic;

namespace Miki.Cache.InMemory
{
	public class InMemoryCachePool : ICachePool
	{
		private Dictionary<string, byte[]> _cache = new Dictionary<string, byte[]>();

		public ICacheClient Get => throw new NotImplementedException();
	}
}
