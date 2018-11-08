using Miki.Serialization;
using Miki.Serialization.Protobuf;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Miki.Cache.Tests
{
	public class CacheClients
	{
		ProtobufSerializer serializer = new ProtobufSerializer();

		[Fact]
		public async Task InMemory()
		{
			// InMemory
			{
				var pool = new InMemory.InMemoryCachePool(serializer);

				await RunTests(await pool.GetAsync());
			}
		}

#if !DISABLE_REDIS
		[Fact]
		public async Task StackExchange()
		{
			// Redis
			{
				var pool = new StackExchange.StackExchangeCachePool(serializer, "localhost");

				await RunTests(await pool.GetAsync());
			}
		}

		[Fact]
		public void StackExchangeSync()
		{
			// Redis
			{
				var pool = new StackExchange.StackExchangeCachePool(serializer, "localhost");

				RunTestsSync(pool.GetAsync().Result);
			}
		}
#endif

		async Task RunTests(ICacheClient client)
		{
			await Test(client, 1, 2);
			await Test(client, "test-string", "other-string");
		}

		void RunTestsSync(ICacheClient client)
		{
			TestSync(client, 1, 2);
			TestSync(client, "test-string", "other-string");
		}

		async Task Test<T>(ICacheClient client, T value, T value2)
		{
			string itemKey = "test";

			await client.UpsertAsync(itemKey, value);

			Assert.True(await client.ExistsAsync(itemKey));

			T i = await client.GetAsync<T>(itemKey);

			Assert.Equal(value, i);

			await client.RemoveAsync(itemKey);

			if (client is IExtendedCacheClient ex)
			{
				string hashKey = "test:hash";

				Assert.False(await client.ExistsAsync(hashKey));
				Assert.False(await ex.HashExistsAsync(hashKey, itemKey));

				Assert.DoesNotContain(await ex.HashKeysAsync(hashKey), x => x == itemKey);
				Assert.DoesNotContain(await ex.HashValuesAsync<T>(hashKey), x => x.Equals(value));
				Assert.DoesNotContain(await ex.HashGetAllAsync<T>(hashKey), x => x.Key == itemKey && x.Value.Equals(value));

				Assert.NotEqual(value, await ex.HashGetAsync<T>(hashKey, itemKey));
				Assert.NotEqual(1, await ex.HashLengthAsync(hashKey));

				await ex.HashUpsertAsync(hashKey, itemKey, value);

				Assert.True(await client.ExistsAsync(hashKey));
				Assert.True(await ex.HashExistsAsync(hashKey, itemKey));

				Assert.Contains(await ex.HashKeysAsync(hashKey), x => x == itemKey);
				Assert.Contains(await ex.HashValuesAsync<T>(hashKey), x => x.Equals(value));
				Assert.Contains(await ex.HashGetAllAsync<T>(hashKey), x => x.Key == itemKey && x.Value.Equals(value));

				Assert.Equal(value, await ex.HashGetAsync<T>(hashKey, itemKey));
				Assert.Equal(1, await ex.HashLengthAsync(hashKey));

				await ex.HashUpsertAsync(hashKey, itemKey, value2);

				Assert.True(await client.ExistsAsync(hashKey));
				Assert.True(await ex.HashExistsAsync(hashKey, itemKey));

				var keys = await ex.HashKeysAsync(hashKey);

				Assert.Contains(keys, x => x == itemKey);

				var values = await ex.HashValuesAsync<T>(hashKey);

				Assert.Contains(values, x => x.Equals(value2));

				Assert.Contains(await ex.HashGetAllAsync<T>(hashKey), x => x.Key == itemKey && x.Value.Equals(value2));

				Assert.Equal(value2, await ex.HashGetAsync<T>(hashKey, itemKey));
				Assert.Equal(1, await ex.HashLengthAsync(hashKey));

				await ex.HashDeleteAsync(hashKey, itemKey);

				Assert.Equal(default(T), await ex.HashGetAsync<T>(hashKey, itemKey));

				await ex.RemoveAsync(hashKey);

			}
		}
		void TestSync<T>(ICacheClient client, T value, T value2)
		{
			string itemKey = "test";

			client.Upsert(itemKey, value);

			Assert.True(client.Exists(itemKey));

			T i = client.Get<T>(itemKey);

			Assert.Equal(value, i);

			client.Remove(itemKey);

			if (client is IExtendedCacheClient ex)
			{
				string hashKey = "test:hash";

				Assert.False(client.Exists(hashKey));
				Assert.False(ex.HashExists(hashKey, itemKey));

				Assert.DoesNotContain(ex.HashKeys(hashKey), x => x == itemKey);
				Assert.DoesNotContain(ex.HashValues<T>(hashKey), x => x.Equals(value));
				Assert.DoesNotContain(ex.HashGetAll<T>(hashKey), x => x.Key == itemKey && x.Value.Equals(value));

				Assert.NotEqual(value, ex.HashGet<T>(hashKey, itemKey));
				Assert.NotEqual(1, ex.HashLength(hashKey));

				ex.HashUpsertAsync(hashKey, itemKey, value);

				Assert.True(client.Exists(hashKey));
				Assert.True(ex.HashExists(hashKey, itemKey));

				Assert.Contains(ex.HashKeys(hashKey), x => x == itemKey);
				Assert.Contains(ex.HashValues<T>(hashKey), x => x.Equals(value));
				Assert.Contains(ex.HashGetAll<T>(hashKey), x => x.Key == itemKey && x.Value.Equals(value));

				Assert.Equal(value, ex.HashGet<T>(hashKey, itemKey));
				Assert.Equal(1, ex.HashLength(hashKey));

				ex.HashUpsertAsync(hashKey, itemKey, value2);

				Assert.True(client.Exists(hashKey));
				Assert.True(ex.HashExists(hashKey, itemKey));

				var keys = ex.HashKeys(hashKey);

				Assert.Contains(keys, x => x == itemKey);

				var values = ex.HashValues<T>(hashKey);

				Assert.Contains(values, x => x.Equals(value2));

				Assert.Contains(ex.HashGetAll<T>(hashKey), x => x.Key == itemKey && x.Value.Equals(value2));

				Assert.Equal(value2, ex.HashGet<T>(hashKey, itemKey));
				Assert.Equal(1, ex.HashLength(hashKey));

				ex.HashDelete(hashKey, itemKey);

				Assert.Equal(default(T), ex.HashGet<T>(hashKey, itemKey));

				ex.Remove(hashKey);

			}
		}
	}
}