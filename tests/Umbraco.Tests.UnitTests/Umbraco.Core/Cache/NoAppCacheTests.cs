// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Cache;

[TestFixture]
public class NoAppCacheTests
{
    private static IAppPolicyCache AppPolicyCache => NoAppCache.Instance;

    // A cache that stores nothing can never satisfy a re-read, so callers waiting on the in-flight factory must share
    // its result rather than each running the factory again in turn.
    // See: https://github.com/umbraco/Umbraco-CMS/issues/23921
    [Test]
    public async Task Concurrent_Get_With_Async_Factory_Only_Runs_Factory_Once()
    {
        var factoryCalls = 0;
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var calls = Enumerable.Range(0, 10)
            .Select(_ => AppPolicyCache.GetCacheItemAsync<string>(
                "NoAppCacheConcurrentAsyncFactoryTest",
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

    [Test]
    public async Task Sequential_Get_With_Async_Factory_Runs_Factory_Each_Time()
    {
        var factoryCalls = 0;

        for (var i = 0; i < 3; i++)
        {
            await AppPolicyCache.GetCacheItemAsync<string>(
                "NoAppCacheSequentialAsyncFactoryTest",
                () =>
                {
                    Interlocked.Increment(ref factoryCalls);
                    return Task.FromResult<string?>("value");
                },
                TimeSpan.FromMinutes(1));
        }

        Assert.AreEqual(3, factoryCalls);
    }
}
