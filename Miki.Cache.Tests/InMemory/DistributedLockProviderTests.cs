namespace Miki.Cache.Tests.InMemory
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Miki.Cache.InMemory;
    using Miki.Serialization.Protobuf;
    using Xunit;

    public class DistributedLockProviderTests
    {
        [Fact]
        public async Task AcquireLockAsyncTest()
        {
            var cacheClient = new InMemoryCacheClient(new ProtobufSerializer());
            var tokenSource = new CancellationTokenSource();
            
            var testLock = await cacheClient.AcquireLockAsync("test:InMemory:acquire", tokenSource.Token);
            Assert.Equal(1, await cacheClient.GetAsync<int>("test:InMemory:acquire"));

            await testLock.ReleaseAsync();
            Assert.Null(await cacheClient.GetAsync<string>("test:InMemory:acquire"));
        }

        [Fact]
        public async Task AcquireLockAsyncTwiceThrowsTimeoutTest()
        {
            var cacheClient = new InMemoryCacheClient(new ProtobufSerializer());
            var tokenSource = new CancellationTokenSource();

            var testLock = await cacheClient.AcquireLockAsync(
                "test:acquire:multiple", tokenSource.Token);

            tokenSource.CancelAfter(1000);
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => cacheClient.AcquireLockAsync(
                        "test:acquire:multiple", tokenSource.Token)
                    .AsTask());

            await testLock.ReleaseAsync();
            Assert.Null(await cacheClient.GetAsync<string>("test:acquire:multiple"));
        }
    }
}
