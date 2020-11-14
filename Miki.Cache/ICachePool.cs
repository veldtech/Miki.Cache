namespace Miki.Cache
{
    using System;
    using System.Threading.Tasks;

    [Obsolete("Handle pooling with the provider instead.")]
    public interface ICachePool
    {
		Task<ICacheClient> GetAsync();
    }
}
