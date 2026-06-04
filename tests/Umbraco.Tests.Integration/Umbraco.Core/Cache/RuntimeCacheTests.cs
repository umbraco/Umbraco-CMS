// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Cache;

/// <summary>
/// Integration tests for the runtime cache (<see cref="AppCaches.RuntimeCache" />) as it is wired up in the
/// web pipeline.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.None)]
internal sealed class RuntimeCacheTests : UmbracoIntegrationTest
{
    private static readonly TimeSpan _timeout = TimeSpan.FromMinutes(10);

    private IAppPolicyCache RuntimeCache => AppCaches.RuntimeCache;

    // The integration harness registers AppCaches.NoCache, whose RuntimeCache caches nothing. Override it
    // with the same runtime cache the web pipeline uses (see AppCaches.Create) so these tests exercise the
    // production caching behaviour rather than a no-op.
    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder.Services.AddUnique(_ => new AppCaches(
            new DeepCloneAppCache(new ObjectCacheAppCache()),
            NoAppCache.Instance,
            new IsolatedCaches(_ => new DeepCloneAppCache(new ObjectCacheAppCache()))));

    [Test]
    public void Insert_Then_Get_Returns_Cached_Value()
    {
        RuntimeCache.Insert("key", () => "value", _timeout);

        Assert.That(RuntimeCache.Get("key"), Is.EqualTo("value"));
    }

    [Test]
    public void GetCacheItem_Invokes_Factory_Once_And_Caches_Result()
    {
        var factoryCalls = 0;
        string Factory()
        {
            factoryCalls++;
            return "value";
        }

        var first = RuntimeCache.GetCacheItem("key", Factory, _timeout);
        var second = RuntimeCache.GetCacheItem("key", Factory, _timeout);

        Assert.Multiple(() =>
        {
            Assert.That(first, Is.EqualTo("value"));
            Assert.That(second, Is.EqualTo("value"));
            Assert.That(factoryCalls, Is.EqualTo(1));
        });
    }

    [Test]
    public void Clear_Removes_All_Items()
    {
        RuntimeCache.Insert("a", () => "1", _timeout);
        RuntimeCache.Insert("b", () => "2", _timeout);

        RuntimeCache.Clear();

        Assert.Multiple(() =>
        {
            Assert.That(RuntimeCache.Get("a"), Is.Null);
            Assert.That(RuntimeCache.Get("b"), Is.Null);
        });
    }

    [Test]
    public void Clear_With_Key_Removes_Only_The_Matching_Item()
    {
        RuntimeCache.Insert("a", () => "1", _timeout);
        RuntimeCache.Insert("b", () => "2", _timeout);

        RuntimeCache.Clear("a");

        Assert.Multiple(() =>
        {
            Assert.That(RuntimeCache.Get("a"), Is.Null);
            Assert.That(RuntimeCache.Get("b"), Is.EqualTo("2"));
        });
    }

    [Test]
    public void ClearByKey_Removes_Items_With_Matching_Prefix()
    {
        RuntimeCache.Insert("prefix:a", () => "1", _timeout);
        RuntimeCache.Insert("prefix:b", () => "2", _timeout);
        RuntimeCache.Insert("other", () => "3", _timeout);

        RuntimeCache.ClearByKey("prefix:");

        Assert.Multiple(() =>
        {
            Assert.That(RuntimeCache.Get("prefix:a"), Is.Null);
            Assert.That(RuntimeCache.Get("prefix:b"), Is.Null);
            Assert.That(RuntimeCache.Get("other"), Is.EqualTo("3"));
        });
    }

    [Test]
    public void ClearByRegex_Removes_Matching_Items()
    {
        RuntimeCache.Insert("item-1", () => "1", _timeout);
        RuntimeCache.Insert("item-2", () => "2", _timeout);
        RuntimeCache.Insert("keep", () => "3", _timeout);

        RuntimeCache.ClearByRegex(@"^item-\d$");

        Assert.Multiple(() =>
        {
            Assert.That(RuntimeCache.Get("item-1"), Is.Null);
            Assert.That(RuntimeCache.Get("item-2"), Is.Null);
            Assert.That(RuntimeCache.Get("keep"), Is.EqualTo("3"));
        });
    }

    [Test]
    public void ClearOfType_Removes_Only_Items_Of_That_Type()
    {
        RuntimeCache.Insert("cart", () => new Cart("the-cart"), _timeout);
        RuntimeCache.Insert("text", () => "a string", _timeout);

        RuntimeCache.ClearOfType<Cart>();

        Assert.Multiple(() =>
        {
            Assert.That(RuntimeCache.Get("cart"), Is.Null);
            Assert.That(RuntimeCache.Get("text"), Is.EqualTo("a string"));
        });
    }

    [Test]
    public void SearchByKey_Returns_Items_With_Matching_Prefix()
    {
        RuntimeCache.Insert("prefix:a", () => "1", _timeout);
        RuntimeCache.Insert("prefix:b", () => "2", _timeout);
        RuntimeCache.Insert("other", () => "3", _timeout);

        var matches = RuntimeCache.SearchByKey("prefix:").ToArray();

        Assert.That(matches, Is.EquivalentTo(new[] { "1", "2" }));
    }

    [Test]
    public void ClearByKey_Clears_Entry_After_Value_Is_Replaced()
    {
        RuntimeCache.Insert("key", () => "first", _timeout);

        // Replacing a cached value evicts the previous entry on a background thread. Once that eviction
        // has been processed, ClearByKey must still find and clear the current entry. Regression test for
        // #23064, where the background eviction removed the re-added key from the cache's key-tracking set.
        RuntimeCache.Insert("key", () => "second", _timeout);

        WaitForKeyTrackingDesync("key");

        RuntimeCache.ClearByKey("key");

        Assert.That(RuntimeCache.Get("key"), Is.Null);
    }

    /// <summary>
    /// Waits until the background post-eviction callback has had the chance to run. Before the fix this
    /// leaves the cache in a desynced state (the value is still cached but key-based search no longer
    /// finds it); after the fix the condition never becomes true and the wait simply elapses, with the
    /// calling test's assertions validating the corrected behaviour.
    /// </summary>
    private void WaitForKeyTrackingDesync(string key)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < TimeSpan.FromMilliseconds(500))
        {
            var desynced = RuntimeCache.Get(key) is not null && RuntimeCache.SearchByKey(key).Any() is false;
            if (desynced)
            {
                return;
            }

            Thread.Sleep(25);
        }
    }

    private sealed record Cart(string Name);
}
