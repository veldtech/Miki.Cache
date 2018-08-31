using Miki.Serialization;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miki.Cache.StackExchange
{
	public class StackExchangeCacheClient : IExtendedCacheClient
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
			return await _database.KeyExistsAsync(Array.ConvertAll(keys, (x => (RedisKey)x)));
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
			var result = await _database.StringGetAsync(Array.ConvertAll(keys, (x => (RedisKey)x)));
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

		public async Task HashDeleteAsync(string key, string hashKey)
		{
			await _database.HashDeleteAsync(key, hashKey);
		}
		public async Task HashDeleteAsync(string key, string[] hashKeys)
		{
			await _database.HashDeleteAsync(key, ToRedisValues(hashKeys));
		}

		public async Task<bool> HashExistsAsync(string key, string hashKey)
		{
			return await _database.HashExistsAsync(key, hashKey);
		}
		public async Task<long> HashExistsAsync(string key, string[] hashKeys)
		{
			return (await HashKeysAsync(key)).Count(x => x != null);
		}

		public async Task<T> HashGetAsync<T>(string key, string hashKey)
		{
			var response = await _database.HashGetAsync(key, hashKey);
			if(response.HasValue)
			{
				return _serializer.Deserialize<T>(response);
			}
			return default(T);
		}
		public async Task<T[]> HashGetAsync<T>(string key, string[] hashKeys)
		{
			RedisValue[] values = await _database.HashGetAsync(
				key, ToRedisValues(hashKeys)
			);

			T[] output = new T[values.Length];

			for (int i = 0; i < values.Length; i++)
			{
				if (values[i].HasValue)
				{
					output[i] = _serializer.Deserialize<T>(values[i]);
				}
				else
				{
					output[i] = default(T);
				}
			}

			return output;
		}

		public async Task<string[]> HashKeysAsync(string key)
		{
			return ToStringArray(await _database.HashKeysAsync(key));
		}

		public async Task<long> HashLengthAsync(string key)
		{
			return await _database.HashLengthAsync(key);
		}

		public async Task<T[]> HashValuesAsync<T>(string key)
		{
			var items = await _database.HashValuesAsync(key);

			return items
				.Select(x => _serializer.Deserialize<T>(x))
				.ToArray();
		}

		public async Task<KeyValuePair<string, T>[]> HashGetAllAsync<T>(string key)
		{
			var items = await _database.HashGetAllAsync(key);
			return items
				.Select(x =>  new KeyValuePair<string, T>(x.Name, _serializer.Deserialize<T>(x.Value)))
				.ToArray();
		}

		public async Task HashUpsertAsync<T>(string key, string hashKey, T value)
		{
			await _database.HashSetAsync(key, hashKey, _serializer.Serialize<T>(value));
		}
		public async Task HashUpsertAsync<T>(string key, KeyValuePair<string, T>[] values)
		{
			await _database.HashSetAsync(
				key,
				Array.ConvertAll(
					values,
					x => (HashEntry)new KeyValuePair<RedisValue, RedisValue>(x.Key, _serializer.Serialize(x.Value))
				)
			);
		}

		public async Task RemoveAsync(string key)
		{
			await _database.KeyDeleteAsync(key);
		}
		public async Task RemoveAsync(string[] keys)
		{
			await _database.KeyDeleteAsync(ToRedisKeys(keys));
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

		internal RedisKey[] ToRedisKeys(string[] keys)
		{
			RedisKey[] redisKeys = new RedisKey[keys.Length];
			for (int i = 0; i < keys.Length; i++)
			{
				redisKeys[i] = keys[i];
			}
			return redisKeys;
		}

		internal RedisValue[] ToRedisValues(string[] keys)
		{
			RedisValue[] values = new RedisValue[keys.Length];
			for (int i = 0; i < keys.Length; i++)
			{
				values[i] = keys[i];
			}
			return values;
		}

		internal string[] ToStringArray(RedisKey[] values)
		{
			string[] str = new string[values.Length];
			for (int i = 0; i < values.Length; i++)
			{
				str[i] = values[i];
			}
			return str;
		}
		internal string[] ToStringArray(RedisValue[] values)
		{
			string[] str = new string[values.Length];
			for (int i = 0; i < values.Length; i++)
			{
				str[i] = values[i];
			}
			return str;
		}
	}
}