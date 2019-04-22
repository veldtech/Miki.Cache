using Miki.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miki.Cache.InMemory
{
    public class InMemoryCacheClient : IExtendedCacheClient
	{
		private ConcurrentDictionary<string, byte[]> _dictionary;
		private ISerializer _serializer;

		public InMemoryCacheClient(ISerializer serializer)
		{
			_dictionary = new ConcurrentDictionary<string, byte[]>();
			_serializer = serializer;
		}
		internal InMemoryCacheClient(ConcurrentDictionary<string, byte[]> dictionary, ISerializer serializer)
		{
			_dictionary = dictionary;
			_serializer = serializer;
		}

        /// <inheritdoc />
        public async Task<bool> ExistsAsync(string key)
		{
			await Task.Yield();
			return _dictionary.ContainsKey(key);
		}
        public Task<long> ExistsAsync(IEnumerable<string> keys)
        {
            return Task.FromResult(
                (long)keys.Count(x => _dictionary.ContainsKey(x)));
        }

        public Task ExpiresAsync(string key, TimeSpan expiresIn)
        {
            throw new NotImplementedException();
        }
        public Task ExpiresAsync(string key, DateTime expiresAt)
        {
            throw new NotImplementedException();
        }

		public async Task<T> GetAsync<T>(string key)
		{
			if(await ExistsAsync(key))
			{
				return _serializer.Deserialize<T>(_dictionary[key]);
			}
			return default(T);
		}
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

		public async Task HashDeleteAsync(string key, string hashKey)
		{
			if(_dictionary.TryGetValue(key, out byte[] bytes))
			{
				var hash = _serializer.Deserialize<ConcurrentDictionary<string, byte[]>>(bytes);
				hash.TryRemove(hashKey, out _);
				await UpsertAsync(key, hash);
			}
		}
        public async Task HashDeleteAsync(string key, IEnumerable<string> hashKeys)
        {
            foreach (string hKey in hashKeys)
            {
                await HashDeleteAsync(key, hKey);
            }
        }

		public Task<bool> HashExistsAsync(string key, string hashKey)
		{
			if (_dictionary.TryGetValue(key, out byte[] bytes))
			{
				var hash = _serializer.Deserialize<ConcurrentDictionary<string, byte[]>>(bytes);
				return Task.FromResult(hash.ContainsKey(hashKey));
			}
			return Task.FromResult(false);
		}

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

		public Task<IEnumerable<KeyValuePair<string, T>>> HashGetAllAsync<T>(string key)
		{
			if (_dictionary.TryGetValue(key, out byte[] bytes))
			{
				var hash = _serializer.Deserialize<ConcurrentDictionary<string, byte[]>>(bytes);

				return Task.FromResult(
					hash.Select(x 
                        => new KeyValuePair<string, T>(x.Key, _serializer.Deserialize<T>(x.Value)))
                );
			}
			return Task.FromResult<IEnumerable<KeyValuePair<string, T>>>(null);
		}

		public Task<T> HashGetAsync<T>(string key, string hashKey)
		{
			if (_dictionary.TryGetValue(key, out byte[] bytes))
			{
				var hash = _serializer.Deserialize<ConcurrentDictionary<string, byte[]>>(bytes);
				if (hash.TryGetValue(hashKey, out byte[] hashBytes))
				{
					return Task.FromResult(_serializer.Deserialize<T>(hashBytes));
				}
			}
			return Task.FromResult(default(T));
		}

		public async Task<IEnumerable<T>> HashGetAsync<T>(string key, IEnumerable<string> hashKeys)
		{
			List<T> allItems = new List<T>();
			foreach (string hKey in hashKeys)
			{
				allItems.Add(await HashGetAsync<T>(key, hKey));
			}
			return allItems.ToArray();
		}

		public Task<IEnumerable<string>> HashKeysAsync(string key)
		{
			if (_dictionary.TryGetValue(key, out byte[] bytes))
			{
				var hash = _serializer.Deserialize<ConcurrentDictionary<string, byte[]>>(bytes);
				return Task.FromResult(hash.Select(x => x.Key));
			}
			return Task.FromResult(Enumerable.Empty<string>());
		}

		public Task<long> HashLengthAsync(string key)
		{
			if (_dictionary.TryGetValue(key, out byte[] bytes))
			{
				var hash = _serializer.Deserialize<ConcurrentDictionary<string, byte[]>>(bytes);
				return Task.FromResult((long)hash.Count);
			}
			return Task.FromResult(0L);
		}

		public async Task HashUpsertAsync<T>(string key, string hashKey, T value)
		{
			ConcurrentDictionary<string, byte[]> hash = null;
			if (_dictionary.TryGetValue(key, out byte[] bytes))
			{
				hash = _serializer.Deserialize<ConcurrentDictionary<string, byte[]>>(bytes);
				hash.AddOrUpdate(hashKey, _serializer.Serialize(value), (x, y) => _serializer.Serialize(value));
			}
			else
			{
				hash = new ConcurrentDictionary<string, byte[]>();
				hash.AddOrUpdate(hashKey, _serializer.Serialize(value), (x, y) => _serializer.Serialize(value));
			}
			await UpsertAsync(key, hash);
		}

		public async Task HashUpsertAsync<T>(string key, IEnumerable<KeyValuePair<string, T>> values)
		{
			foreach(var value in values)
			{
				await HashUpsertAsync(key, value.Key, value.Value);
			}
		}

		public Task<IEnumerable<T>> HashValuesAsync<T>(string key)
		{
			if (_dictionary.TryGetValue(key, out byte[] bytes))
			{
				var hash = _serializer.Deserialize<ConcurrentDictionary<string, byte[]>>(bytes);

				return Task.FromResult(
					hash
					.Select(x => _serializer.Deserialize<T>(x.Value))
				);
			}
			return Task.FromResult(Enumerable.Empty<T>());
		}

		public async Task RemoveAsync(string key)
		{
			if(await ExistsAsync(key))
			{
				_dictionary.TryRemove(key, out var x);
			}
		}
		public async Task RemoveAsync(IEnumerable<string> keys)
		{
			foreach(var key in keys)
			{
				await RemoveAsync(key);
			}
		}

		public Task UpsertAsync<T>(string key, T value, TimeSpan? expiresIn = null)
		{
			_dictionary.AddOrUpdate(key, _serializer.Serialize(value), (x, y) => _serializer.Serialize(value));
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
