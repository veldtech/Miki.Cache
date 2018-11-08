using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Miki.Cache
{
    public interface IExtendedCacheClient : ICacheClient
    {
		void HashDelete(string key, string hashKey);
		void HashDelete(string key, string[] hashKeys);

		Task HashDeleteAsync(string key, string hashKey);
		Task HashDeleteAsync(string key, string[] hashKeys);

		bool HashExists(string key, string hashKey);
		long HashExists(string key, string[] hashKeys);

		Task<bool> HashExistsAsync(string key, string hashKey);
		Task<long> HashExistsAsync(string key, string[] hashKeys);

		T HashGet<T>(string key, string hashKey);
		T[] HashGet<T>(string key, string[] hashKeys);

		Task<T> HashGetAsync<T>(string key, string hashKey);
		Task<T[]> HashGetAsync<T>(string key, string[] hashKeys);

		KeyValuePair<string, T>[] HashGetAll<T>(string key);

		Task<KeyValuePair<string, T>[]> HashGetAllAsync<T>(string key);

		string[] HashKeys(string key);

		Task<string[]> HashKeysAsync(string key);

		long HashLength(string key);

		Task<long> HashLengthAsync(string key);

		void HashUpsert<T>(string key, string hashKey, T value);
		void HashUpsert<T>(string key, KeyValuePair<string, T>[] values);

		Task HashUpsertAsync<T>(string key, string hashKey, T value);
		Task HashUpsertAsync<T>(string key, KeyValuePair<string, T>[] values);

		T[] HashValues<T>(string key);

		Task<T[]> HashValuesAsync<T>(string key);
	}
}
