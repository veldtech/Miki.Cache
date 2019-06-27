using Miki.Serialization;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Task<bool> ExistsAsync(string key)
			=> _database.KeyExistsAsync(key);
		public Task<long> ExistsAsync(IEnumerable<string> keys)
			=> _database.KeyExistsAsync(keys
                .Select(x => (RedisKey)x)
                .ToArray());

        public Task ExpiresAsync(string key, TimeSpan time)
            => _database.KeyExpireAsync(key, time);
        public Task ExpiresAsync(string key, DateTime date)
            => _database.KeyExpireAsync(key, date);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<T> GetAsync<T>(string key)
        {
            var x = await _database.StringGetAsync(key);
            if (!x.IsNullOrEmpty)
            {
                return _serializer.Deserialize<T>(x);
            }
            return default(T);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<IEnumerable<T>> GetAsync<T>(IEnumerable<string> keys)
		{
			var result = await _database.StringGetAsync(keys.Select(x => (RedisKey)x).ToArray());
			T[] results = new T[keys.Count()];

			for (int i = 0; i < keys.Count(); i++)
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
		public async Task HashDeleteAsync(string key, IEnumerable<string> hashKeys)
		{
			await _database.HashDeleteAsync(key, ToRedisValues(hashKeys));
		}

		public async Task<bool> HashExistsAsync(string key, string hashKey)
			=> await _database.HashExistsAsync(key, hashKey);
		public async Task<long> HashExistsAsync(string key, IEnumerable<string> hashKeys)
			=> (await HashKeysAsync(key)).Count(x => hashKeys.Contains(x));

		public async Task<T> HashGetAsync<T>(string key, string hashKey)
		{
			var response = await _database.HashGetAsync(key, hashKey);
			if(response.HasValue)
			{
				return _serializer.Deserialize<T>(response);
			}
			return default(T);
		}
		public async Task<IEnumerable<T>> HashGetAsync<T>(string key, IEnumerable<string> hashKeys)
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

        public async Task<IEnumerable<KeyValuePair<string, T>>> HashGetAllAsync<T>(string key)
        {
            var response = await _database.HashGetAllAsync(key);
            return response.Select(x 
                => new KeyValuePair<string, T>(
                    x.Name, 
                    _serializer.Deserialize<T>(x.Value)));
        }

		public async Task<IEnumerable<string>> HashKeysAsync(string key)
		{
			return ToStringArray(await _database.HashKeysAsync(key));
		}

		public async Task<long> HashLengthAsync(string key)
		{
			return await _database.HashLengthAsync(key);
		}

		public async Task<IEnumerable<T>> HashValuesAsync<T>(string key)
		{
			var items = await _database.HashValuesAsync(key);

            return items
                .Select(x => _serializer.Deserialize<T>(x));
		}

		public async Task HashUpsertAsync<T>(string key, string hashKey, T value)
		{
			await _database.HashSetAsync(key, hashKey, _serializer.Serialize<T>(value));
		}
		public async Task HashUpsertAsync<T>(string key, IEnumerable<KeyValuePair<string, T>> values)
		{
			await _database.HashSetAsync(
				key,
				Array.ConvertAll(
					values.ToArray(),
					x => (HashEntry)new KeyValuePair<RedisValue, RedisValue>(x.Key, _serializer.Serialize(x.Value))
				)
			);
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task RemoveAsync(string key)
		{
			return _database.KeyDeleteAsync(key);
		}
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task RemoveAsync(IEnumerable<string> keys)
		{
            return _database.KeyDeleteAsync(ToRedisKeys(keys));
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task UpsertAsync<T>(string key, T value, TimeSpan? expiresIn = null)
		{
            return _database.StringSetAsync(
				key,
				_serializer.Serialize<T>(value),
				expiresIn
			);
		}
        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        public Task UpsertAsync<T>(IEnumerable<KeyValuePair<string, T>> values, TimeSpan? expiresIn = null)
		{
			KeyValuePair<RedisKey, RedisValue>[] v = new KeyValuePair<RedisKey, RedisValue>[values.Count()];
			for(int i = 0, max = values.Count(); i < max; i++)
			{
				v[i] = new KeyValuePair<RedisKey, RedisValue>(
					values.ElementAt(i).Key,
					_serializer.Serialize(values.ElementAt(i).Value)
				);
			}
            List<Task> t = new List<Task>();
            t.Add(_database.StringSetAsync(v));

			if (expiresIn.HasValue)
			{
				foreach (var kv in values)
				{
                    t.Add(_database.KeyExpireAsync(kv.Key, expiresIn.Value));
				}
			}

            return Task.WhenAll(t);
		}

		internal RedisKey[] ToRedisKeys(IEnumerable<string> keys)
		{
			RedisKey[] redisKeys = new RedisKey[keys.Count()];
			for (int i = 0; i < keys.Count(); i++)
			{
				redisKeys[i] = keys.ElementAt(i);
			}
			return redisKeys;
		}

		internal RedisValue[] ToRedisValues(IEnumerable<string> keys)
		{
			RedisValue[] values = new RedisValue[keys.Count()];
			for (int i = 0; i < keys.Count(); i++)
			{
				values[i] = keys.ElementAt(i);
			}
			return values;
		}

		internal IEnumerable<string> ToStringArray(RedisKey[] values)
		{
			string[] str = new string[values.Length];
			for (int i = 0; i < values.Length; i++)
			{
				str[i] = values[i];
			}
			return str;
		}
		internal IEnumerable<string> ToStringArray(RedisValue[] values)
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