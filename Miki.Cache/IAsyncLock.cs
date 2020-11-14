namespace Miki.Cache
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Asynchronous lock for distributed systems.
    /// </summary>
    public interface IAsyncLock : IAsyncDisposable
    {
        /// <summary>
        /// Unlocks the lock.
        /// </summary>
        ValueTask ReleaseAsync();
    }
}
