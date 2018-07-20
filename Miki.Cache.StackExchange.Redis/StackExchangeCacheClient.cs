using StackExchange.Redis;
using System;
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

		public async Task<T> GetAsync<T>(string key)
		{
			var result = await _database.StringGetAsync(key);

			if (result.HasValue)
			{
				return _serializer.Deserialize<T>(result);
			}

			return default(T);
		}

		public async Task RemoveAsync(string key)
		{
			await _database.KeyDeleteAsync(key);
		}

		public async Task UpsertAsync<T>(string key, T value, TimeSpan? expiresIn = null)
		{
			await _database.StringSetAsync(
				key,
				_serializer.Serialize<T>(value),
				expiresIn
			);
		}
	}
}
