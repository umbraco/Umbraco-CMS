// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Cache;

public abstract class RuntimeAppCacheTests : AppCacheTests
{
    internal abstract IAppPolicyCache AppPolicyCache { get; }

    [Test]
    public void Can_Add_Struct_Strongly_Typed_With_Null()
    {
        var now = DateTime.Now;

        // Long enough that the entry cannot expire while the assertions below run.
        AppPolicyCache.Insert("DateTimeTest", () => now, TimeSpan.FromMinutes(5));

        Assert.Multiple(() =>
        {
            Assert.AreEqual(now, AppCache.GetCacheItem<DateTime>("DateTimeTest"));
            Assert.AreEqual(now, AppCache.GetCacheItem<DateTime?>("DateTimeTest"));
        });
    }

    [Test]
    public void Can_Expire_Struct_Strongly_Typed_With_Null()
    {
        AppPolicyCache.Insert("DateTimeTest", () => DateTime.Now, TimeSpan.FromMilliseconds(20));

        // Expiry is observed lazily on read, so the test must poll.
        Assert.That(
            () => AppCache.GetCacheItem<DateTime>("DateTimeTest"),
            Is.EqualTo(default(DateTime)).After(5000, 20),
            "The cache entry did not expire.");
        Assert.IsNull(AppCache.GetCacheItem<DateTime?>("DateTimeTest"));
    }

    [Test]
    public async Task Can_Get_With_Async_Factory()
    {
        var value = await AppPolicyCache.GetCacheItemAsync("AsyncFactoryGetTest", async () => await GetValueAsync(5), TimeSpan.FromMilliseconds(100));
        Assert.AreEqual(50, value);
    }

    [Test]
    public async Task Can_Insert_With_Async_Factory()
    {
        await AppPolicyCache.InsertCacheItemAsync("AsyncFactoryInsertTest", async () => await GetValueAsync(10), TimeSpan.FromMilliseconds(100));
        var value = AppPolicyCache.GetCacheItem<int>("AsyncFactoryInsertTest");
        Assert.AreEqual(100, value);
    }

    [Test]
    public void Concurrent_Get_With_Factory_Only_Runs_Factory_Once()
    {
        var factoryCalls = 0;

        Parallel.For(0, 10, _ => AppPolicyCache.GetCacheItem(
            "ConcurrentSyncFactoryTest",
            () =>
            {
                Interlocked.Increment(ref factoryCalls);
                Thread.Sleep(50);
                return "value";
            },
            TimeSpan.FromMinutes(1)));

        Assert.AreEqual(1, factoryCalls);
    }

    // Concurrent misses on the same key must only run the factory once, as the synchronous GetCacheItem does.
    // https://github.com/umbraco/Umbraco-CMS/issues/23921
    [Test]
    public async Task Concurrent_Get_With_Async_Factory_Only_Runs_Factory_Once()
    {
        var factoryCalls = 0;
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        // Each call runs synchronously until its first await, so by the time the gate opens every caller has
        // either entered the factory or is waiting to.
        var calls = Enumerable.Range(0, 10)
            .Select(_ => AppPolicyCache.GetCacheItemAsync<string>(
                "ConcurrentAsyncFactoryTest",
                async () =>
                {
                    Interlocked.Increment(ref factoryCalls);
                    await gate.Task;
                    return "value";
                },
                TimeSpan.FromMinutes(1)))
            .ToArray();

        gate.SetResult();
        var values = await Task.WhenAll(calls);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, factoryCalls);
            Assert.That(values, Is.All.EqualTo("value"));
        });
    }

    // A null result is never stored, so callers waiting on the in-flight factory must share its result rather than
    // re-reading the cache and each running the factory again in turn.
    // See: https://github.com/umbraco/Umbraco-CMS/issues/23921
    [Test]
    public async Task Concurrent_Get_With_Async_Factory_Returning_Null_Only_Runs_Factory_Once()
    {
        var factoryCalls = 0;
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var calls = Enumerable.Range(0, 10)
            .Select(_ => AppPolicyCache.GetCacheItemAsync<string>(
                "ConcurrentAsyncNullFactoryTest",
                async () =>
                {
                    Interlocked.Increment(ref factoryCalls);
                    await gate.Task;
                    return null;
                },
                TimeSpan.FromMinutes(1)))
            .ToArray();

        gate.SetResult();
        var values = await Task.WhenAll(calls);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, factoryCalls);
            Assert.That(values, Is.All.Null);
        });
    }

    [Test]
    public async Task Get_With_Async_Factory_Does_Not_Block_Other_Keys()
    {
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var blocked = AppPolicyCache.GetCacheItemAsync<string>(
            "AsyncFactoryBlockedKeyTest",
            async () =>
            {
                await gate.Task;
                return "blocked";
            },
            TimeSpan.FromMinutes(1));

        var other = await AppPolicyCache.GetCacheItemAsync<string>(
            "AsyncFactoryOtherKeyTest",
            () => Task.FromResult<string?>("other"),
            TimeSpan.FromMinutes(1));

        Assert.AreEqual("other", other);
        Assert.IsFalse(blocked.IsCompleted);

        gate.SetResult();
        Assert.AreEqual("blocked", await blocked);
    }

    [Test]
    public async Task Get_With_Async_Factory_Releases_Key_When_Factory_Throws()
    {
        Assert.ThrowsAsync<InvalidOperationException>(() => AppPolicyCache.GetCacheItemAsync<string>(
            "AsyncFactoryThrowsTest",
            () => throw new InvalidOperationException(),
            TimeSpan.FromMinutes(1)));

        var value = await AppPolicyCache.GetCacheItemAsync<string>(
            "AsyncFactoryThrowsTest",
            () => Task.FromResult<string?>("value"),
            TimeSpan.FromMinutes(1));

        Assert.AreEqual("value", value);
    }

    // A factory that requests its own key would otherwise wait on the lock it is running under. Throw instead,
    // as the synchronous GetCacheItem does.
    [Test]
    public async Task Get_With_Async_Factory_Throws_When_Factory_Requests_Its_Own_Key()
    {
        var call = AppPolicyCache.GetCacheItemAsync<string>(
            "AsyncFactoryRecursiveTest",
            async () => await AppPolicyCache.GetCacheItemAsync<string>(
                "AsyncFactoryRecursiveTest",
                () => Task.FromResult<string?>("inner"),
                TimeSpan.FromMinutes(1)),
            TimeSpan.FromMinutes(1));

        var completed = await Task.WhenAny(call, Task.Delay(TimeSpan.FromSeconds(5)));

        Assert.AreSame(call, completed, "Recursive call deadlocked.");
        Assert.ThrowsAsync<InvalidOperationException>(() => call);
    }

    [Test]
    public async Task Get_With_Async_Factory_Allows_Factory_To_Request_Other_Keys()
    {
        var value = await AppPolicyCache.GetCacheItemAsync<string>(
            "AsyncFactoryOuterKeyTest",
            async () => await AppPolicyCache.GetCacheItemAsync<string>(
                "AsyncFactoryInnerKeyTest",
                () => Task.FromResult<string?>("inner"),
                TimeSpan.FromMinutes(1)),
            TimeSpan.FromMinutes(1));

        Assert.AreEqual("inner", value);
    }

    private static async Task<int> GetValueAsync(int value)
    {
        await Task.Delay(10);
        return value * 10;
    }
}
