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

    private static async Task<int> GetValueAsync(int value)
    {
        await Task.Delay(10);
        return value * 10;
    }
}
