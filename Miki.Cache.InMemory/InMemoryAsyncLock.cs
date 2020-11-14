namespace Miki.Cache.InMemory
{
    using System;
    using System.Threading.Tasks;

    internal class InMemoryAsyncLock : IAsyncLock
    {
        private readonly string key;
        private readonly InMemoryCacheClient cache;
        private bool wasDisposed;

        internal InMemoryAsyncLock(string key, InMemoryCacheClient cache)
        {
            this.key = key;
            this.cache = cache;
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if(wasDisposed)
            {
                throw new ObjectDisposedException(key);
            }

            await cache.RemoveAsync(key);
            wasDisposed = true;
        }

        /// <inheritdoc />
        public ValueTask ReleaseAsync()
        {
            if(!wasDisposed)
            {
                return DisposeAsync();
            }
            return default;
        }
    }
}