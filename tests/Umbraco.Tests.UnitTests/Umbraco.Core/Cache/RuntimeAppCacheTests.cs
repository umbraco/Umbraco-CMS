// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Cache;

public abstract class RuntimeAppCacheTests : AppCacheTests
{
    internal abstract IAppPolicyCache AppPolicyCache { get; }

    // The cached and the expired half of this used to be one test that inserted with a 20 millisecond
    // expiration and then asserted the value was still cached. Those assertions raced the expiration and
    // failed with default(DateTime) whenever the agent stalled for longer than that between two adjacent
    // statements. They are now two tests: neither the caching nor the expiring one depends on how long the
    // machine takes to get from one line to the next.
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

        // Poll for the expiration instead of sleeping for a fixed time and assuming that outlasted it.
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

    private static async Task<int> GetValueAsync(int value)
    {
        await Task.Delay(10);
        return value * 10;
    }
}
