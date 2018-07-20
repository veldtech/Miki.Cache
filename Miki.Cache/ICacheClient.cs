using System;
using System.Threading.Tasks;

namespace Miki.Cache
{
    public interface ICacheClient
    {
		Task<bool> ExistsAsync(string key);

		Task<T> GetAsync<T>(string key);

		Task UpsertAsync<T>(string key, T value, TimeSpan? expiresIn = null);

		Task RemoveAsync(string key);
	}
}