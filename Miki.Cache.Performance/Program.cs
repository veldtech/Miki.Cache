using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Miki.Serialization;
using Miki.Serialization.Protobuf;

namespace Miki.Cache.Performance
{
    class TestSerializer : ISerializer
    {
        public T Deserialize<T>(byte[] data)
        {
            if (data == null)
                return default(T);
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(data))
            {
                object obj = bf.Deserialize(ms);
                return (T)obj;
            }
        }

        public byte[] Serialize<T>(T obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
    }

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
			client = new StackExchange.StackExchangeCachePool(new TestSerializer(), "localhost").GetAsync().Result;
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
