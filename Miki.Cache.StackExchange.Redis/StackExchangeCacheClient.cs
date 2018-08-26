using Miki.Serialization;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miki.Cache.StackExchange
{
	public class StackExchangeCacheClient : ICacheClient
	{
		public IConnectionMultiplexer Client { get; private set; }

		private IDatabase _database;
		private ISerializer _serializer;

		public StackExchangeCacheClient(ISerializer serializer, IConnectionMultiplexer connectionMultiplexer)
		{
			Client = connectionMultiplexer;
			_serializer = serializer;
			_database = Client.GetDatabase();
		}

		public async Task<bool> ExistsAsync(string key)
		{
			return await _database.KeyExistsAsync(key);
		}
		public async Task<long> ExistsAsync(string[] keys)
		{
			return await _database.KeyExistsAsync(FromStringArray(keys));
		}

		public async Task<T> GetAsync<T>(string key)
		{
			var result = await _database.StringGetAsync(key);

			if (!result.IsNullOrEmpty)
			{
				return _serializer.Deserialize<T>(result);
			}

			return default(T);
		}
		public async Task<T[]> GetAsync<T>(string[] keys)
		{
			var result = await _database.StringGetAsync(FromStringArray(keys));
			T[] results = new T[keys.Length];

			for (int i = 0; i < keys.Length; i++)
			{
				if (!result[i].IsNullOrEmpty)
				{
					results[i] = _serializer.Deserialize<T>(result[i]);
				}
				else
				{
					results[i] = default(T);
				}
			}

			return results;
		}

		public async Task RemoveAsync(string key)
		{
			await _database.KeyDeleteAsync(key);
		}
		public async Task RemoveAsync(string[] keys)
		{
			await _database.KeyDeleteAsync(FromStringArray(keys));
		}

		public async Task UpsertAsync<T>(string key, T value, TimeSpan? expiresIn = null)
		{
			await _database.StringSetAsync(
				key,
				_serializer.Serialize<T>(value),
				expiresIn
			);
		}
		public async Task UpsertAsync<T>(KeyValuePair<string, T>[] values, TimeSpan? expiresIn = null)
		{
			KeyValuePair<RedisKey, RedisValue>[] v = new KeyValuePair<RedisKey, RedisValue>[values.Count()];
			for(int i = 0, max = values.Length; i < max; i++)
			{
				v[i] = new KeyValuePair<RedisKey, RedisValue>(
					values[i].Key,
					_serializer.Serialize(values[i].Value)
				);
			}
			await _database.StringSetAsync(v);

			if (expiresIn.HasValue)
			{
				foreach (var kv in values)
				{
					await _database.KeyExpireAsync(kv.Key, expiresIn.Value);
				}
			}
		}

		internal RedisKey[] FromStringArray(string[] keys)
		{
			RedisKey[] redisKeys = new RedisKey[keys.Length];
			for (int i = 0; i < keys.Length; i++)
			{
				redisKeys[i] = keys[i];
			}
			return redisKeys;
		}
	}
}
