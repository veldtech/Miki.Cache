using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Miki.Serialization.Protobuf;

namespace Miki.Cache.Performance
{
	class Program
	{
		static void Main(string[] args)
		{
			var summary = BenchmarkRunner.Run<SyncVsAsyncRedis>();
			Console.ReadLine();
		}
	}

	[CoreJob]
	[RPlotExporter, RankColumn]
	public class SyncVsAsyncRedis
	{
		ICacheClient client;

		[Params(10, 100, 1000)]
		public int N;

		[GlobalSetup]
		public void Setup()
		{
			client = new StackExchange.StackExchangeCachePool(new ProtobufSerializer(), "localhost").GetAsync().Result;
		}

		[Benchmark]
		public async Task TestAsync()
		{
			for (int i = 0; i < N; i++)
			{
				await Test<string>(client, "test", "test2");
			}
		}

		async Task Test<T>(ICacheClient client, T value, T value2)
		{

			string itemKey = "test";

			await client.UpsertAsync(itemKey, value);

			await client.ExistsAsync(itemKey);

			T x = await client.GetAsync<T>(itemKey);

			await client.RemoveAsync(itemKey);
		}
	}
}
