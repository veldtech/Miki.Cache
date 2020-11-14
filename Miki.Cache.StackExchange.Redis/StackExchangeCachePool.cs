namespace Miki.Cache.StackExchange
{
    using Miki.Serialization;
    using System;
    using System.Threading.Tasks;
    using global::StackExchange.Redis;

    public class StackExchangeCachePool : ICachePool
	{
		private readonly ConfigurationOptions configuration;
		private readonly ISerializer serializer;

		private readonly Lazy<Task<ICacheClient>> factory;

		public StackExchangeCachePool(ISerializer serializer, ConfigurationOptions configuration)
		{
			this.configuration = configuration;
			this.serializer = serializer;

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
