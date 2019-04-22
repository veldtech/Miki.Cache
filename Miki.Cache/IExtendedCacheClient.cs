using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Miki.Cache
{
    public interface IExtendedCacheClient : ICacheClient
    {
		Task HashDeleteAsync(string key, string hashKey);
		Task HashDeleteAsync(string key, IEnumerable<string> hashKeys);

		Task<bool> HashExistsAsync(string key, string hashKey);
		Task<long> HashExistsAsync(string key, IEnumerable<string> hashKeys);

		Task<T> HashGetAsync<T>(string key, string hashKey);
		Task<IEnumerable<T>> HashGetAsync<T>(string key, IEnumerable<string> hashKeys);

		Task<IEnumerable<KeyValuePair<string, T>>> HashGetAllAsync<T>(string key);

		Task<IEnumerable<string>> HashKeysAsync(string key);

		Task<long> HashLengthAsync(string key);

		Task HashUpsertAsync<T>(string key, string hashKey, T value);
		Task HashUpsertAsync<T>(string key, IEnumerable<KeyValuePair<string, T>> values);

		Task<IEnumerable<T>> HashValuesAsync<T>(string key);
	}
}
