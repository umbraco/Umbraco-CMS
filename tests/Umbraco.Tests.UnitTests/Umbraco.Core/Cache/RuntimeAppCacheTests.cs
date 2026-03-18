// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Cache;

/// <summary>
/// Contains unit tests for verifying the behavior and functionality of <see cref="IAppPolicyCache"/> implementations (e.g. <see cref="ObjectCacheAppCache"/>).
/// </summary>
public abstract class RuntimeAppCacheTests : AppCacheTests
{
    internal abstract IAppPolicyCache AppPolicyCache { get; }

    /// <summary>
    /// Tests that a struct value can be added to the cache and correctly expires,
    /// including verifying behavior when retrieving as a nullable struct.
    /// </summary>
    [Test]
    public void Can_Add_And_Expire_Struct_Strongly_Typed_With_Null()
    {
        var now = DateTime.Now;
        AppPolicyCache.Insert("DateTimeTest", () => now, new TimeSpan(0, 0, 0, 0, 20));
        var cachedDateTime = AppCache.GetCacheItem<DateTime>("DateTimeTest");
        var cachedDateTimeNullable = AppCache.GetCacheItem<DateTime?>("DateTimeTest");
        Assert.AreEqual(now, cachedDateTime);
        Assert.AreEqual(now, cachedDateTimeNullable);

        Thread.Sleep(30); // sleep longer than the cache expiration

        cachedDateTime = AppCache.GetCacheItem<DateTime>("DateTimeTest");
        cachedDateTimeNullable = AppCache.GetCacheItem<DateTime?>("DateTimeTest");
        Assert.AreEqual(default(DateTime), cachedDateTime);
        Assert.AreEqual(null, cachedDateTimeNullable);
    }

    /// <summary>
    /// Tests that the cache can retrieve a value using an asynchronous factory method.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Can_Get_With_Async_Factory()
    {
        var value = await AppPolicyCache.GetCacheItemAsync("AsyncFactoryGetTest", async () => await GetValueAsync(5), TimeSpan.FromMilliseconds(100));
        Assert.AreEqual(50, value);
    }

    /// <summary>
    /// Tests that an item can be inserted into the cache using an asynchronous factory method.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
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
