using StackExchange.Redis;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Miki.Cache.StackExchange
{
	public class StackExchangeCacheClient : ICacheClient
	{
		public IConnectionMultiplexer Client { get; private set; }

		private ISerializer serializer;

		public StackExchangeCacheClient(ISerializer serializer, IConnectionMultiplexer connectionMultiplexer)
		{
			Client = connectionMultiplexer;
			this.serializer = serializer;
		}

		public async Task<T> GetAsync<T>(string key)
		{
			IDatabase database = Client.GetDatabase();

			var result = await database.StringGetAsync(key);

			if (result.HasValue)
			{
				return serializer.Deserialize<T>(result);
			}

			return default(T);
		}

		public async Task RemoveAsync(string key)
		{
			IDatabase database = Client.GetDatabase();
			await database.KeyDeleteAsync(key);
		}

		public async Task UpsertAsync<T>(string key, T value)
		{
			IDatabase database = Client.GetDatabase();
			await database.StringSetAsync(
				key,
				serializer.Serialize<T>(value)
			);
		}
	}
}
