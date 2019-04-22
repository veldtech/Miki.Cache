using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Miki.Cache.Extensions
{
	internal class HashSet<T> : IHashSet<T>
	{
		private readonly IExtendedCacheClient _cacheClient;
		private readonly string _key;

		public HashSet(IExtendedCacheClient cacheClient, string key)
		{
			_cacheClient = cacheClient;
			_key = key;
		}

		public async Task AddAsync(string key, T value)
		{
			await _cacheClient.HashUpsertAsync(_key, key, value);
		}

		public async Task AddAsync(IEnumerable<KeyValuePair<string, T>> values)
		{
			await _cacheClient.HashUpsertAsync(_key, values);
		}

		public async Task<bool> ExistsAsync(string key)
			=> await _cacheClient.HashExistsAsync(_key, key);

		public async Task<long> ExistsAsync(IEnumerable<string> keys)
			=> await _cacheClient.HashExistsAsync(_key, keys);

		public async Task<IEnumerable<KeyValuePair<string, T>>> GetAllAsync()
			=> await _cacheClient.HashGetAllAsync<T>(_key);

		public async Task<T> GetAsync(string key)
			=> await _cacheClient.HashGetAsync<T>(_key, key);

		public async Task<IEnumerable<T>> GetAsync(IEnumerable<string> key)
			=> await _cacheClient.HashGetAsync<T>(_key, key);

		public async Task<IEnumerable<string>> KeysAsync()
			=> await _cacheClient.HashKeysAsync(_key);

		public async Task<long> LengthAsync()
			=> await _cacheClient.HashLengthAsync(_key);

		public async Task<IEnumerable<T>> ValuesAsync()
			=> await _cacheClient.HashValuesAsync<T>(_key);
	}
}
