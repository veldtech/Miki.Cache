namespace Miki.Cache.StackExchange
{
    using Miki.Serialization;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using global::StackExchange.Redis;

    public class StackExchangeCacheClient : IExtendedCacheClient, IDistributedLockProvider
	{
		public IConnectionMultiplexer Client { get; }

		private readonly IDatabase database;
		private readonly ISerializer serializer;

		public StackExchangeCacheClient(
            ISerializer serializer, IConnectionMultiplexer connectionMultiplexer)
		{
			Client = connectionMultiplexer;
			this.serializer = serializer;
			database = Client.GetDatabase();
        }

		public Task<bool> ExistsAsync(string key)
			=> database.KeyExistsAsync(key);
		public Task<long> ExistsAsync(IEnumerable<string> keys)
			=> database.KeyExistsAsync(keys
                .Select(x => (RedisKey)x)
                .ToArray());

        public Task ExpiresAsync(string key, TimeSpan time)
            => database.KeyExpireAsync(key, time);
        public Task ExpiresAsync(string key, DateTime date)
            => database.KeyExpireAsync(key, date);

        public async Task<T> GetAsync<T>(string key)
        {
            var x = await database.StringGetAsync(key);
            if (!x.IsNullOrEmpty)
            {
                return serializer.Deserialize<T>(x);
            }
            return default;
        }
        public async Task<IEnumerable<T>> GetAsync<T>(IEnumerable<string> keys)
		{
			var result = await database.StringGetAsync(keys.Select(x => (RedisKey)x).ToArray());
			T[] results = new T[keys.Count()];

			for (int i = 0; i < keys.Count(); i++)
			{
				if (!result[i].IsNullOrEmpty)
				{
					results[i] = serializer.Deserialize<T>(result[i]);
				}
				else
				{
					results[i] = default;
				}
			}

			return results;
		}

		public async Task HashDeleteAsync(string key, string hashKey)
		{
			await database.HashDeleteAsync(key, hashKey);
		}
		public async Task HashDeleteAsync(string key, IEnumerable<string> hashKeys)
		{
			await database.HashDeleteAsync(key, ToRedisValues(hashKeys));
		}

		public async Task<bool> HashExistsAsync(string key, string hashKey)
			=> await database.HashExistsAsync(key, hashKey);
		public async Task<long> HashExistsAsync(string key, IEnumerable<string> hashKeys)
			=> (await HashKeysAsync(key)).Count(hashKeys.Contains);

		public async Task<T> HashGetAsync<T>(string key, string hashKey)
		{
			var response = await database.HashGetAsync(key, hashKey);
			if(response.HasValue)
			{
				return serializer.Deserialize<T>(response);
			}
			return default;
		}
		public async Task<IEnumerable<T>> HashGetAsync<T>(string key, IEnumerable<string> hashKeys)
		{
			RedisValue[] values = await database.HashGetAsync(
				key, ToRedisValues(hashKeys)
			);

			T[] output = new T[values.Length];

			for (int i = 0; i < values.Length; i++)
			{
				if (values[i].HasValue)
				{
					output[i] = serializer.Deserialize<T>(values[i]);
				}
				else
				{
					output[i] = default;
				}
			}

			return output;
		}

        public async Task<IEnumerable<KeyValuePair<string, T>>> HashGetAllAsync<T>(string key)
        {
            var response = await database.HashGetAllAsync(key);
            return response.Select(x 
                => new KeyValuePair<string, T>(
                    x.Name, 
                    serializer.Deserialize<T>(x.Value)));
        }

		public async Task<IEnumerable<string>> HashKeysAsync(string key)
		{
			return ToStringArray(await database.HashKeysAsync(key));
		}

		public async Task<long> HashLengthAsync(string key)
		{
			return await database.HashLengthAsync(key);
		}

		public async Task<IEnumerable<T>> HashValuesAsync<T>(string key)
		{
			var items = await database.HashValuesAsync(key);

            return items
                .Select(x => serializer.Deserialize<T>(x));
		}

        /// <inheritdoc />
        public async ValueTask<T> SortedSetPopAsync<T>(
            string key, Cache.Order order = Cache.Order.Ascending)
        {
            var value = await database.SortedSetPopAsync(
                key, order == Cache.Order.Ascending ? Order.Ascending : Order.Descending);
            if(!value.HasValue)
            {
				return default;
            }

            return serializer.Deserialize<T>(value.Value.Element);
        }

        /// <inheritdoc />
        public async ValueTask SortedSetUpsertAsync<T>(string key, T value, double score)
        {
            await database.SortedSetAddAsync(key, serializer.Serialize<T>(value), score);
        }

        /// <inheritdoc />
        public async ValueTask SortedSetUpsertAsync<T>(string key, IEnumerable<SortedEntry<T>> entries)
        {
            await database.SortedSetAddAsync(
                key, 
                Array.ConvertAll(
                    entries.ToArray(), 
                    entry => new SortedSetEntry(serializer.Serialize(entry.Value), entry.Score)));
        }

        public async Task HashUpsertAsync<T>(string key, string hashKey, T value)
		{
			await database.HashSetAsync(key, hashKey, serializer.Serialize(value));
		}
		public async Task HashUpsertAsync<T>(string key, IEnumerable<KeyValuePair<string, T>> values)
		{
			await database.HashSetAsync(
				key,
                Array.ConvertAll(
					values.ToArray(),
					x => (HashEntry)new KeyValuePair<RedisValue, RedisValue>(x.Key, serializer.Serialize(x.Value))
				)
			);
		}

        public Task RemoveAsync(string key)
		{
			return database.KeyDeleteAsync(key);
		}
        public Task RemoveAsync(IEnumerable<string> keys)
		{
            return database.KeyDeleteAsync(ToRedisKeys(keys));
		}

        public Task UpsertAsync<T>(string key, T value, TimeSpan? expiresIn = null)
		{
            return database.StringSetAsync(
				key,
				serializer.Serialize<T>(value),
				expiresIn
			);
		}

        public Task UpsertAsync<T>(
            IEnumerable<KeyValuePair<string, T>> values, TimeSpan? expiresIn = null)
        {
            int valueCount = values.Count();
			KeyValuePair<RedisKey, RedisValue>[] v = new KeyValuePair<RedisKey, RedisValue>[valueCount];
			for(int i = 0, max = valueCount; i < max; i++)
			{
				v[i] = new KeyValuePair<RedisKey, RedisValue>(
					values.ElementAt(i).Key,
					serializer.Serialize(values.ElementAt(i).Value)
				);
			}
            List<Task> t = new List<Task>();
            t.Add(database.StringSetAsync(v));

			if (expiresIn.HasValue)
			{
				foreach (var kv in values)
				{
                    t.Add(database.KeyExpireAsync(kv.Key, expiresIn.Value));
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

		internal IEnumerable<string> ToStringArray(RedisValue[] values)
		{
			string[] str = new string[values.Length];
			for (int i = 0; i < values.Length; i++)
			{
				str[i] = values[i];
			}
			return str;
		}

        /// <inheritdoc />
        public async ValueTask<IAsyncLock> AcquireLockAsync(string key, CancellationToken token)
        {
            try
            {
                var guid = Guid.NewGuid();

                while(true)
                {
                    if(await database.LockTakeAsync(key, guid.ToString(), TimeSpan.FromMinutes(1)))
                    {
                        return new RedisAsyncLock(key, guid, this);
                    }

					var currentExpiration = await database.StringGetWithExpiryAsync(key);
                    await Task.Delay(currentExpiration.Expiry ?? TimeSpan.FromSeconds(1), token);
                }

            }
            catch(RedisTimeoutException)
            {
            }
        }
    }
}