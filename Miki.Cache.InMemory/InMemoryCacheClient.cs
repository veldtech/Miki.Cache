using Miki.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miki.Cache.InMemory
{
	public class InMemoryCacheClient : ICacheClient
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
