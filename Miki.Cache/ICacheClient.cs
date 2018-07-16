using System;
using System.Threading.Tasks;

namespace Miki.Cache
{
    public interface ICacheClient
    {
		Task<T> GetAsync<T>(string key);
	
		Task UpsertAsync<T>(string key, T value);

		Task RemoveAsync(string key);
	}
}
