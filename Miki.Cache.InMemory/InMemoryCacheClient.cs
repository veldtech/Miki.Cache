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

		public async Task<bool> ExistsAsync(string key)
		{
			await Task.Yield();
			return _dictionary.ContainsKey(key);
		}
		public async Task<long> ExistsAsync(string[] key)
		{
			await Task.Yield();
			return key.Count(x => _dictionary.ContainsKey(x));
		}

		public async Task<T> GetAsync<T>(string key)
		{
			if(await ExistsAsync(key))
			{
				return _serializer.Deserialize<T>(_dictionary[key]);
			}
			return default(T);
		}
		public async Task<T[]> GetAsync<T>(string[] keys)
		{
			T[] t = new T[keys.Length];
			for (int i = 0; i < keys.Length; i++)
			{
				if(await ExistsAsync(keys[i]))
				{
					t[i] = await GetAsync<T>(keys[i]);
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

		public async Task HashDeleteAsync(string key, string[] hashKeys)
		{
			foreach(string hKey in hashKeys)
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

		public async Task<long> HashExistsAsync(string key, string[] hashKeys)
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

		public Task<KeyValuePair<string, T>[]> HashGetAllAsync<T>(string key)
		{
			if (_dictionary.TryGetValue(key, out byte[] bytes))
			{
				var hash = _serializer.Deserialize<ConcurrentDictionary<string, byte[]>>(bytes);

				return Task.FromResult(
					hash
					.Select(x => new KeyValuePair<string, T>(x.Key, _serializer.Deserialize<T>(x.Value)))
					.ToArray()
				);
			}
			return Task.FromResult(new KeyValuePair<string, T>[0]);
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

		public async Task<T[]> HashGetAsync<T>(string key, string[] hashKeys)
		{
			List<T> allItems = new List<T>();
			foreach (string hKey in hashKeys)
			{
				allItems.Add(await HashGetAsync<T>(key, hKey));
			}
			return allItems.ToArray();
		}

		public Task<string[]> HashKeysAsync(string key)
		{
			if (_dictionary.TryGetValue(key, out byte[] bytes))
			{
				var hash = _serializer.Deserialize<ConcurrentDictionary<string, byte[]>>(bytes);
				return Task.FromResult(hash.Select(x => x.Key).ToArray());
			}
			return Task.FromResult<string[]>(new string[0]);
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

		public async Task HashUpsertAsync<T>(string key, KeyValuePair<string, T>[] values)
		{
			foreach(var value in values)
			{
				await HashUpsertAsync(key, value.Key, value.Value);
			}
		}

		public Task<T[]> HashValuesAsync<T>(string key)
		{
			if (_dictionary.TryGetValue(key, out byte[] bytes))
			{
				var hash = _serializer.Deserialize<ConcurrentDictionary<string, byte[]>>(bytes);

				return Task.FromResult(
					hash
					.Select(x => _serializer.Deserialize<T>(x.Value))
					.ToArray()
				);
			}
			return Task.FromResult(new T[0]);
		}

		public async Task RemoveAsync(string key)
		{
			if(await ExistsAsync(key))
			{
				_dictionary.TryRemove(key, out var x);
			}
		}
		public async Task RemoveAsync(string[] keys)
		{
			foreach(var key in keys)
			{
				await RemoveAsync(key);
			}
		}

		public async Task UpsertAsync<T>(string key, T value, TimeSpan? expiresIn = null)
		{
			await Task.Yield();
			_dictionary.AddOrUpdate(key, _serializer.Serialize(value), (x, y) => _serializer.Serialize(value));
		}
		public async Task UpsertAsync<T>(KeyValuePair<string, T>[] values, TimeSpan? expiresIn = null)
		{
			foreach(var i in values)
			{
				await UpsertAsync(i.Key, i.Value);
			}
		}
	}
}
