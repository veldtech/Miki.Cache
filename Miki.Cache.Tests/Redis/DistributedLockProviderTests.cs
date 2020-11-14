namespace Miki.Cache.Tests.Redis
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using global::StackExchange.Redis;
    using Miki.Cache.StackExchange;
    using Miki.Serialization.Protobuf;
    using Xunit;

    public class DistributedLockProviderTests
    {
        [Fact]
        public async Task AcquireLockAsyncTest()
        {
            var cacheClient = new StackExchangeCacheClient(
                new ProtobufSerializer(), await ConnectionMultiplexer.ConnectAsync("localhost"));

            var tokenSource = new CancellationTokenSource();
            
            var testLock = await cacheClient.AcquireLockAsync("test:acquire", tokenSource.Token);
            Assert.Equal("1", await cacheClient.Client.GetDatabase().StringGetAsync("test:acquire"));

            await testLock.ReleaseAsync();
            Assert.Null(await cacheClient.GetAsync<string>("test:acquire"));
        }

        [Fact]
        public async Task AcquireLockAsyncTwiceThrowsTimeoutTest()
        {
            var cacheClient = new StackExchangeCacheClient(
                new ProtobufSerializer(), 
                await ConnectionMultiplexer.ConnectAsync("localhost"));

            var tokenSource = new CancellationTokenSource();

            var testLock = await cacheClient.AcquireLockAsync(
                "test:redis:acquire:multiple", tokenSource.Token);

            tokenSource.CancelAfter(1000);
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => cacheClient.AcquireLockAsync(
                        "test:redis:acquire:multiple", tokenSource.Token)
                    .AsTask());

            await testLock.ReleaseAsync();
            Assert.Null(await cacheClient.GetAsync<string>("test:redis:acquire:multiple"));
        }
    }
}
