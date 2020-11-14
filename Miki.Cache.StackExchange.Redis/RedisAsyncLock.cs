namespace Miki.Cache.StackExchange
{
    using System;
    using System.Threading.Tasks;
    using global::StackExchange.Redis;

    internal class RedisAsyncLock : IAsyncLock
    {
        private readonly string key;
        private readonly Guid guid;
        private readonly IDatabase client;

        private bool wasDisposed;

        public RedisAsyncLock(string key, Guid guid, IDatabase client)
        {
            this.key = key;
            this.guid = guid;
            this.client = client;
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if(wasDisposed)
            {
                throw new ObjectDisposedException(key);
            }

            try
            {
                if(!await client.LockReleaseAsync(key, guid.ToString()))
                {
                    throw new InvalidOperationException();
                }
            }
            catch(RedisTimeoutException)
            {
                await DisposeAsync();
            }

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