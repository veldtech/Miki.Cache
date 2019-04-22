using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Miki.Cache.Extensions
{
    public interface IHashSet<T>
    {
		Task AddAsync(string key, T value);
		Task AddAsync(IEnumerable<KeyValuePair<string, T>> values);

		Task<bool> ExistsAsync(string key);
		Task<long> ExistsAsync(IEnumerable<string> keys);

		Task<long> LengthAsync();

		Task<T> GetAsync(string key);
		Task<IEnumerable<T>> GetAsync(IEnumerable<string> key);

		Task<IEnumerable<KeyValuePair<string, T>>> GetAllAsync();

		Task<IEnumerable<string>> KeysAsync();

		Task<IEnumerable<T>> ValuesAsync();
	}
}
