namespace Miki.Cache.InMemory
{
    using Miki.Serialization;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    
	/// <summary>
	/// InMemory cache client for testing purposes. For real, don't dare to use this in production.
	/// </summary>
    public class InMemoryCacheClient : IExtendedCacheClient, IDistributedLockProvider
	{
		private readonly ConcurrentDictionary<string, byte[]> dictionary;
		private readonly ConcurrentDictionary<string, DateTime?> expireDict;
		private readonly ISerializer serializer;

		public InMemoryCacheClient(ISerializer serializer)
		{
			dictionary = new ConcurrentDictionary<string, byte[]>();
            expireDict = new ConcurrentDictionary<string, DateTime?>();
			this.serializer = serializer;
		}
		internal InMemoryCacheClient(
            ConcurrentDictionary<string, byte[]> dictionary, ISerializer serializer)
		{
			this.dictionary = dictionary;
            expireDict = new ConcurrentDictionary<string, DateTime?>();
            this.serializer = serializer;
		}

		/// <inheritdoc/>
        public Task<bool> ExistsAsync(string key)
		{
            if(!expireDict.TryGetValue(key, out var value))
            {
                return Task.FromResult(dictionary.ContainsKey(key));
			}
            return Task.FromResult(
                (value ?? new DateTime(0)) > DateTime.UtcNow && dictionary.ContainsKey(key));
		}

        /// <inheritdoc/>
		public Task<long> ExistsAsync(IEnumerable<string> keys)
        {
            return Task.FromResult(
                (long)keys.Count(x => dictionary.ContainsKey(x)));
        }

		/// <inheritdoc/>
        public Task ExpiresAsync(string key, TimeSpan expiresIn)
        {
            expireDict.AddOrUpdate(
                key, DateTime.UtcNow + expiresIn, (a, b) => DateTime.UtcNow + expiresIn);
            return Task.CompletedTask;
		}

        /// <inheritdoc/>
		public Task ExpiresAsync(string key, DateTime expiresAt)
        {
			expireDict.AddOrUpdate(key, expiresAt, (a, b) => expiresAt);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
		public async Task<T> GetAsync<T>(string key)
		{
			if(await ExistsAsync(key))
			{
				return serializer.Deserialize<T>(dictionary[key]);
			}
			return default(T);
		}

        /// <inheritdoc/>
		public async Task<IEnumerable<T>> GetAsync<T>(IEnumerable<string> keys)
        {
            T[] t = new T[keys.Count()];
            for (int i = 0; i < keys.Count(); i++)
            {
                if (await ExistsAsync(keys.ElementAtOrDefault(i)))
                {
                    t[i] = await GetAsync<T>(keys.ElementAtOrDefault(i));
                }
                else
                {
                    t[i] = default(T);
                }
            }
            return t;
        }

        /// <inheritdoc/>
		public async Task HashDeleteAsync(string key, string hashKey)
		{
			if(dictionary.TryGetValue(key, out byte[] bytes))
			{
				var hash = serializer.Deserialize<ConcurrentDictionary<string, byte[]>>(bytes);
				hash.TryRemove(hashKey, out _);
				await UpsertAsync(key, hash);
			}
		}

        /// <inheritdoc/>
		public async Task HashDeleteAsync(string key, IEnumerable<string> hashKeys)
        {
            foreach (string hKey in hashKeys)
            {
                await HashDeleteAsync(key, hKey);
            }
        }

        /// <inheritdoc/>
		public Task<bool> HashExistsAsync(string key, string hashKey)
		{
			if (dictionary.TryGetValue(key, out byte[] bytes))
			{
				var hash = serializer.Deserialize<ConcurrentDictionary<string, byte[]>>(bytes);
				return Task.FromResult(hash.ContainsKey(hashKey));
			}
			return Task.FromResult(false);
		}

        /// <inheritdoc/>
		public async Task<long> HashExistsAsync(string key, IEnumerable<string> hashKeys)
		{
			long x = 0;
			foreach(string hKey in hashKeys)
			{
				if(await HashExistsAsync(key, hKey))
				{
					x++;
				}
			}
			return x;
		}

        /// <inheritdoc/>
		public Task<IEnumerable<KeyValuePair<string, T>>> HashGetAllAsync<T>(string key)
		{
			if (dictionary.TryGetValue(key, out byte[] bytes))
			{
				var hash = serializer.Deserialize<ConcurrentDictionary<string, byte[]>>(bytes);
				return Task.FromResult(
					hash.Select(x => new KeyValuePair<string, T>(x.Key, serializer.Deserialize<T>(x.Value)))
                );
			}
			return Task.FromResult(
                Enumerable.Empty<KeyValuePair<string, T>>());
		}

        /// <inheritdoc/>
		public Task<T> HashGetAsync<T>(string key, string hashKey)
		{
			if (dictionary.TryGetValue(key, out byte[] bytes))
			{
				var hash = serializer.Deserialize<ConcurrentDictionary<string, byte[]>>(bytes);
				if (hash.TryGetValue(hashKey, out byte[] hashBytes))
				{
					return Task.FromResult(serializer.Deserialize<T>(hashBytes));
				}
			}
			return Task.FromResult(default(T));
		}

        /// <inheritdoc/>
		public async Task<IEnumerable<T>> HashGetAsync<T>(string key, IEnumerable<string> hashKeys)
		{
			List<T> allItems = new List<T>();
			foreach (string hKey in hashKeys)
			{
				allItems.Add(await HashGetAsync<T>(key, hKey));
			}
			return allItems.ToArray();
		}

        /// <inheritdoc/>
		public Task<IEnumerable<string>> HashKeysAsync(string key)
		{
			if (dictionary.TryGetValue(key, out byte[] bytes))
			{
				var hash = serializer.Deserialize<ConcurrentDictionary<string, byte[]>>(bytes);
				return Task.FromResult(hash.Select(x => x.Key));
			}
			return Task.FromResult(Enumerable.Empty<string>());
		}

        /// <inheritdoc/>
		public Task<long> HashLengthAsync(string key)
		{
			if (dictionary.TryGetValue(key, out byte[] bytes))
			{
				var hash = serializer.Deserialize<ConcurrentDictionary<string, byte[]>>(bytes);
				return Task.FromResult((long)hash.Count);
			}
			return Task.FromResult(0L);
		}

        /// <inheritdoc/>
		public async Task HashUpsertAsync<T>(string key, string hashKey, T value)
		{
			ConcurrentDictionary<string, byte[]> hash = null;
			if (dictionary.TryGetValue(key, out byte[] bytes))
			{
				hash = serializer.Deserialize<ConcurrentDictionary<string, byte[]>>(bytes);
				hash.AddOrUpdate(hashKey, serializer.Serialize(value), (x, y) => serializer.Serialize(value));
			}
			else
			{
				hash = new ConcurrentDictionary<string, byte[]>();
				hash.AddOrUpdate(hashKey, serializer.Serialize(value), (x, y) => serializer.Serialize(value));
			}
			await UpsertAsync(key, hash);
		}

        /// <inheritdoc/>
		public async Task HashUpsertAsync<T>(string key, IEnumerable<KeyValuePair<string, T>> values)
		{
			foreach(var value in values)
			{
				await HashUpsertAsync(key, value.Key, value.Value);
			}
		}

        /// <inheritdoc/>
		public Task<IEnumerable<T>> HashValuesAsync<T>(string key)
		{
			if (dictionary.TryGetValue(key, out byte[] bytes))
			{
				var hash = serializer.Deserialize<ConcurrentDictionary<string, byte[]>>(bytes);

				return Task.FromResult(
					hash
					.Select(x => serializer.Deserialize<T>(x.Value))
				);
			}
			return Task.FromResult(Enumerable.Empty<T>());
		}

        /// <inheritdoc />
        public ValueTask<T> SortedSetPopAsync<T>(string key, Order order = Order.Ascending)
        {
            if(!dictionary.TryGetValue(key, out var bytes))
            {
				return default;
            }

            var list = serializer.Deserialize<List<Tuple<T, double>>>(bytes);
            var value = order == Order.Ascending 
                ? list.OrderBy(x => x.Item2).FirstOrDefault() 
                : list.OrderByDescending(x => x.Item2).FirstOrDefault();

            if(value == null)
            {
                return default;
            }

            list.Remove(value);
            dictionary[key] = serializer.Serialize(list);
            return new ValueTask<T>(value.Item1);
        }

        /// <inheritdoc />
        public ValueTask SortedSetUpsertAsync<T>(string key, T value, double score)
        {
            var tuple = new Tuple<T, double>(value, score);

            var list = new List<Tuple<T, double>>();
            if(dictionary.TryGetValue(key, out var bytes))
			{
				list = serializer.Deserialize<List<Tuple<T, double>>>(bytes);
            }
            list.Add(tuple);

			dictionary[key] = serializer.Serialize(list);
			return default;
		}

        /// <inheritdoc />
        public ValueTask SortedSetUpsertAsync<T>(string key, IEnumerable<SortedEntry<T>> entries)
        {
            var tuples = entries.Select(x => new Tuple<T, double>(x.Value, x.Score));
            
            var list = new List<Tuple<T, double>>();
            if(dictionary.TryGetValue(key, out var bytes))
            {
                list = serializer.Deserialize<List<Tuple<T, double>>>(bytes);
            }
            list.AddRange(tuples);

			dictionary[key] = serializer.Serialize(list);
            return default;
        }

        /// <inheritdoc/>
		public async Task RemoveAsync(string key)
		{
			if(await ExistsAsync(key))
			{
				dictionary.TryRemove(key, out var x);
			}
		}

        /// <inheritdoc/>
		public async Task RemoveAsync(IEnumerable<string> keys)
		{
			foreach(var key in keys)
			{
				await RemoveAsync(key);
			}
		}

        /// <inheritdoc />
        public async ValueTask<IAsyncLock> AcquireLockAsync(string key, CancellationToken token)
        {
            while(true)
            {
                token.ThrowIfCancellationRequested();
                var bytes = dictionary.AddOrUpdate(key, serializer.Serialize(1),
                    (str, val) => serializer.Serialize(serializer.Deserialize<int>(val) + 1));
                
				if(serializer.Deserialize<int>(bytes) == 1)
                {
                    break;
                }

                if(expireDict.TryGetValue(key, out var currentExpiration))
                {
                    await Task.Delay((currentExpiration ?? DateTime.UtcNow) - DateTime.UtcNow, token);
                }
                else
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), token);
                }
            }

            return new InMemoryAsyncLock(key, this);
        }

        public Task UpsertAsync<T>(string key, T value, TimeSpan? expiresIn = null)
		{
			dictionary.AddOrUpdate(key, serializer.Serialize(value), (x, y) => serializer.Serialize(value));
            return Task.CompletedTask;
		}
		public async Task UpsertAsync<T>(IEnumerable<KeyValuePair<string, T>> values, TimeSpan? expiresIn = null)
		{
			foreach(var i in values)
			{
				await UpsertAsync(i.Key, i.Value);
			}
		}
	}
}
