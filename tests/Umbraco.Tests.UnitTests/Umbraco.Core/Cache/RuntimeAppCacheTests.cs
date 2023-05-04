// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Threading;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Cache;

public abstract class RuntimeAppCacheTests : AppCacheTests
{
    internal abstract IAppPolicyCache AppPolicyCache { get; }

    [Test]
    [Explicit("Testing for timeouts cannot work on VSTS.")]
    public void Can_Add_And_Expire_Struct_Strongly_Typed_With_Null()
    {
        var now = DateTime.Now;
        AppPolicyCache.Insert("DateTimeTest", () => now, new TimeSpan(0, 0, 0, 0, 200));
        Assert.AreEqual(now, AppCache.GetCacheItem<DateTime>("DateTimeTest"));
        Assert.AreEqual(now, AppCache.GetCacheItem<DateTime?>("DateTimeTest"));

        Thread.Sleep(300); // sleep longer than the cache expiration

        Assert.AreEqual(default(DateTime), AppCache.GetCacheItem<DateTime>("DateTimeTest"));
        Assert.AreEqual(null, AppCache.GetCacheItem<DateTime?>("DateTimeTest"));
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
