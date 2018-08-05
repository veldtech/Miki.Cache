using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Miki.Cache.StackExchange
{
	public class StackExchangeCachePool : ICachePool
	{
		private readonly ConfigurationOptions _configuration;
		private readonly ISerializer _serializer;

		private Lazy<ICacheClient> factory;

		public StackExchangeCachePool(ISerializer serializer, ConfigurationOptions configuration)
		{
			_configuration = configuration;
			_serializer = serializer;

			factory = new Lazy<ICacheClient>(() =>
			{
				return new StackExchangeCacheClient(serializer, ConnectionMultiplexer.Connect(configuration));
			});
		}
		public StackExchangeCachePool(ISerializer serializer, string configuration)
			: this(serializer, ConfigurationOptions.Parse(configuration))	
		{
		}

		public ICacheClient Get => factory.Value;
	}
}
