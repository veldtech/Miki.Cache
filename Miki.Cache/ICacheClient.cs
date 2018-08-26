using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Miki.Cache
{
    public interface ICacheClient
    {
		Task<bool> ExistsAsync(string key);
		Task<long> ExistsAsync(string[] keys);

		Task<T> GetAsync<T>(string key);
		Task<T[]> GetAsync<T>(string[] keys);

		Task UpsertAsync<T>(string key, T value, TimeSpan? expiresIn = null);
		Task UpsertAsync<T>(KeyValuePair<string, T>[] values, TimeSpan? expiresIn = null);

		Task RemoveAsync(string key);
		Task RemoveAsync(string[] keys);
	}
}