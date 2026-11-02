// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Cache;

[TestFixture]
public class ObjectAppCacheTests : RuntimeAppCacheTests
{
    public override void Setup()
    {
        base.Setup();
        _provider = new ObjectCacheAppCache();
    }

    private ObjectCacheAppCache _provider;

    protected override int GetTotalItemCount => _provider.MemoryCache.Count;

    internal override IAppCache AppCache => _provider;

    internal override IAppPolicyCache AppPolicyCache => _provider;

    // Replacing a cached value evicts the previous entry on a background thread. The eviction must not
    // remove the re-added key from the tracked keys, otherwise key-based operations stop finding the
    // live entry. Regression test for https://github.com/umbraco/Umbraco-CMS/issues/23064.
    [Test]
    public void Replacing_A_Value_Keeps_The_Key_Tracked()
    {
        var timeout = TimeSpan.FromMinutes(10);
        _provider.Insert("key", () => "first", timeout);
        _provider.Insert("key", () => "second", timeout);

        WaitForEvictionCallbacks(_provider, 1);

        Assert.Multiple(() =>
        {
            Assert.That(_provider.Get("key"), Is.EqualTo("second"));
            Assert.That(_provider.SearchByKey("key"), Is.EquivalentTo(new[] { "second" }));
        });

        _provider.ClearByKey("key");
        Assert.That(_provider.Get("key"), Is.Null);
    }

    // Re-caching an expired value evicts the expired entry on a background thread. The eviction must not
    // remove the re-added key from the tracked keys, otherwise key-based operations stop finding the
    // live entry. Regression test for https://github.com/umbraco/Umbraco-CMS/issues/24301.
    [Test]
    public void Re_Caching_An_Expired_Value_Keeps_The_Key_Tracked()
    {
        var clock = new TestClock();
        using var cache = new ObjectCacheAppCache(new MemoryCacheOptions { Clock = clock }, NullLoggerFactory.Instance);
        var timeout = TimeSpan.FromMinutes(10);

        cache.Get("key", () => "first", timeout);
        clock.UtcNow += timeout + TimeSpan.FromSeconds(1);
        Assert.That(cache.Get("key", () => "second", timeout), Is.EqualTo("second"));

        WaitForEvictionCallbacks(cache, 1);

        Assert.Multiple(() =>
        {
            Assert.That(cache.Get("key"), Is.EqualTo("second"));
            Assert.That(cache.SearchByKey("key"), Is.EquivalentTo(new[] { "second" }));
        });

        cache.ClearByKey("key");
        Assert.That(cache.Get("key"), Is.Null);
    }

    // Eviction callbacks run on a background thread, so wait for them to complete before asserting on the
    // tracked keys. The timeout only elapses if a callback never runs.
    private static void WaitForEvictionCallbacks(ObjectCacheAppCache cache, int expectedCount)
        => Assert.That(
            SpinWait.SpinUntil(() => cache.CompletedEvictionCallbacks >= expectedCount, TimeSpan.FromSeconds(5)),
            Is.True,
            "Timed out waiting for the eviction callbacks to complete.");

    private sealed class TestClock : ISystemClock
    {
        public DateTimeOffset UtcNow { get; set; } = DateTimeOffset.UtcNow;
    }
}
