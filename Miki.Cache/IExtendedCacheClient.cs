using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Miki.Cache
{
    public interface IExtendedCacheClient : ICacheClient
    {
		Task HashDeleteAsync(string key, string hashKey);
		Task HashDeleteAsync(string key, string[] hashKeys);

		Task<bool> HashExistsAsync(string key, string hashKey);
		Task<long> HashExistsAsync(string key, string[] hashKeys);

		Task<T> HashGetAsync<T>(string key, string hashKey);
		Task<T[]> HashGetAsync<T>(string key, string[] hashKeys);

		Task<KeyValuePair<string, T>[]> HashGetAllAsync<T>(string key);

		Task<string[]> HashKeysAsync(string key);

		Task<long> HashLengthAsync(string key);

		Task HashUpsertAsync<T>(string key, string hashKey, T value);
		Task HashUpsertAsync<T>(string key, KeyValuePair<string, T>[] values);

		Task<T[]> HashValuesAsync<T>(string key);
	}
}
