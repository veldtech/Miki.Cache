using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Miki.Cache.Extensions
{
    public interface IHashSet<T>
    {
		Task AddAsync(string key, T value);
		Task AddAsync(KeyValuePair<string, T>[] values);

		Task<bool> ExistsAsync(string key);
		Task<long> ExistsAsync(string[] keys);

		Task<long> LengthAsync();

		Task<T> GetAsync(string key);
		Task<T[]> GetAsync(string[] key);

		Task<KeyValuePair<string, T>[]> GetAllAsync();

		Task<string[]> KeysAsync();

		Task<T[]> ValuesAsync();
	}
}
