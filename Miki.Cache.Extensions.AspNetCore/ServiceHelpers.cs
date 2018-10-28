using Miki.Cache;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class ServiceHelpers
	{
		public static IServiceCollection AddCacheClient(this IServiceCollection collection, ICacheClient client)
		{
			return collection.AddSingleton(client);
		}

		public static IServiceCollection AddCacheClient(this IServiceCollection collection, ICachePool pool)
		{
			return collection.AddSingleton(pool)
				.AddScoped((provider) => provider.GetService<ICachePool>().GetAsync().Result);
		}
	}
}
