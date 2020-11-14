namespace Miki.Cache
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A distributed lock provider to create multi-system locks.
    /// </summary>
    public interface IDistributedLockProvider : ICacheClient
    {
        /// <summary>
        /// Creates a default distributed lock.
        /// </summary>
        ValueTask<IAsyncLock> AcquireLockAsync(string key, CancellationToken token);
    }
}