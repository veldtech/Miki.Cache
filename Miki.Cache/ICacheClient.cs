using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Miki.Cache
{
    public interface ICacheClient
    {
		bool Exists(string key);
		long Exists(string[] key);
		Task<bool> ExistsAsync(string key);
		Task<long> ExistsAsync(string[] keys);

		T Get<T>(string key);
		T[] Get<T>(string[] keys);
		Task<T> GetAsync<T>(string key);
		Task<T[]> GetAsync<T>(string[] keys);

		void Upsert<T>(string key, T value, TimeSpan? expiresIn = null);
		void Upsert<T>(KeyValuePair<string, T>[] values, TimeSpan? expiresIn = null);
		Task UpsertAsync<T>(string key, T value, TimeSpan? expiresIn = null);
		Task UpsertAsync<T>(KeyValuePair<string, T>[] values, TimeSpan? expiresIn = null);

		void Remove(string key);
		void Remove(string[] keys);
		Task RemoveAsync(string key);
		Task RemoveAsync(string[] keys);
	}
}