namespace Miki.Cache
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    
    /// <summary>
    /// A more feature-rich, but harder to implement cache client.
    /// </summary>
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

        /// <summary>
        /// Removes the first object from either the front or back based on <see cref="Order"/>.
        /// </summary>
        /// <param name="key">Collection key.</param>
        /// <param name="order"></param>
        ValueTask<T> SortedSetPopAsync<T>(string key, Order order = Order.Ascending);

        ValueTask SortedSetUpsertAsync<T>(string key, T value, double score);
        ValueTask SortedSetUpsertAsync<T>(string key, IEnumerable<SortedEntry<T>> entries);
    }
}
