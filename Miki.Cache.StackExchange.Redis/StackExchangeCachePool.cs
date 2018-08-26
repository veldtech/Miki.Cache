using Miki.Serialization;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Miki.Cache.StackExchange
{
	public class StackExchangeCachePool : ICachePool
	{
		private readonly ConfigurationOptions _configuration;
		private readonly ISerializer _serializer;

		private Lazy<Task<ICacheClient>> factory;

		public StackExchangeCachePool(ISerializer serializer, ConfigurationOptions configuration)
		{
			_configuration = configuration;
			_serializer = serializer;

			factory = new Lazy<Task<ICacheClient>>(async () =>
			{
				return await Task.Run(async () =>
				{
					var connection = await ConnectionMultiplexer.ConnectAsync(configuration);
					return new StackExchangeCacheClient(serializer, connection);
				});
			});
		}
		public StackExchangeCachePool(ISerializer serializer, string configuration)
			: this(serializer, ConfigurationOptions.Parse(configuration))	
		{
		}

		public async Task<ICacheClient> GetAsync()
		{
			return await factory.Value;
		}
	}
}
