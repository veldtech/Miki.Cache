using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Miki.Cache
{
    public interface ICacheClient
    {
        /// <summary>
        /// Checks if the <paramref name="key"/> exists in the cache.
        /// </summary>
		Task<bool> ExistsAsync(string key);

        /// <summary>
        /// Checks if all <paramref name="keys"/> exists in the cache.
        /// </summary>
        /// <returns>The amount of keys that returned true.</returns>
		Task<long> ExistsAsync(IEnumerable<string> keys);

        /// <summary>
        /// Sets a cache expiration span on the key.
        /// </summary>
        Task ExpiresAsync(string key, TimeSpan expiresIn);
        /// <summary>
        /// Sets a cache expiration date on the key.
        /// </summary>
        Task ExpiresAsync(string key, DateTime expiresAt);

        /// <summary>
        /// Gets a value of <typeparamref name="T"/> with <paramref name="key"/> from the cache.
        /// </summary>
		Task<T> GetAsync<T>(string key);
        /// <summary>
        /// Gets values of type <typeparamref name="T"/> with <paramref name="keys"/> from the cache.
        /// </summary>
        Task<IEnumerable<T>> GetAsync<T>(IEnumerable<string> keys);

        /// <summary>
        /// Updates or inserts <paramref name="key"/> with a <paramref name="value"/> of type <typeparamref name="T"/>.
        /// </summary>
		Task UpsertAsync<T>(string key, T value, TimeSpan? expiresIn = null);
        /// <summary>
        /// Updates or inserts multuple <paramref name="values"/> of type <typeparamref name="T"/>.
        /// </summary>
		Task UpsertAsync<T>(IEnumerable<KeyValuePair<string, T>> values, TimeSpan? expiresIn = null);

        /// <summary>
        /// Removes a single <paramref name="key"/> from the cache.
        /// </summary>
		Task RemoveAsync(string key);

        /// <summary>
        /// Removes multiple <paramref name="keys"/> from the cache.
        /// </summary>
        Task RemoveAsync(IEnumerable<string> keys);
	}
}