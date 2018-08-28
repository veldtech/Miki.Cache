using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Miki.Cache.Extensions
{
	internal class HashSet<T> : IHashSet<T>
	{
		IExtendedCacheClient _cacheClient;
		string _key;

		public HashSet(IExtendedCacheClient cacheClient, string key)
		{
			_cacheClient = cacheClient;
		}

		public async Task AddAsync(string key, T value)
		{
			await _cacheClient.HashUpsertAsync(_key, key, value);
		}

		public async Task AddAsync(KeyValuePair<string, T>[] values)
		{
			await _cacheClient.HashUpsertAsync(_key, values);
		}

		public async Task<bool> ExistsAsync(string key)
			=> await _cacheClient.HashExistsAsync(_key, key);

		public async Task<long> ExistsAsync(string[] keys)
			=> await _cacheClient.HashExistsAsync(_key, keys);

		public async Task<KeyValuePair<string, T>[]> GetAllAsync()
			=> await _cacheClient.HashGetAllAsync<T>(_key);

		public async Task<T> GetAsync(string key)
			=> await _cacheClient.HashGetAsync<T>(_key, key);

		public async Task<T[]> GetAsync(string[] key)
			=> await _cacheClient.HashGetAsync<T>(_key, key);

		public async Task<string[]> KeysAsync()
			=> await _cacheClient.HashKeysAsync(_key);

		public async Task<long> LengthAsync()
			=> await _cacheClient.HashLengthAsync(_key);

		public async Task<T[]> ValuesAsync()
		{
			throw new NotImplementedException();
		}
	}
}
