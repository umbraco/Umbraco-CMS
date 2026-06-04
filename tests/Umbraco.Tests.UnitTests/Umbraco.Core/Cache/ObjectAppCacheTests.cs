// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics;
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
    // remove the re-added key from the key-tracking set, otherwise key-based operations stop finding the
    // live entry. Regression test for https://github.com/umbraco/Umbraco-CMS/issues/23064.
    [Test]
    public void Replacing_A_Value_Keeps_The_Key_Tracked()
    {
        var timeout = TimeSpan.FromMinutes(10);
        _provider.Insert("key", () => "first", timeout);
        _provider.Insert("key", () => "second", timeout);

        // The replaced entry is evicted on a background thread. On a build with the bug that eviction
        // removes the key from the tracking set - a sticky desync we can detect. On a fixed build nothing
        // changes and the bounded wait simply elapses.
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < TimeSpan.FromSeconds(2) && _provider.SearchByKey("key").Any())
        {
            Thread.Sleep(25);
        }

        Assert.Multiple(() =>
        {
            Assert.That(_provider.Get("key"), Is.EqualTo("second"));
            Assert.That(_provider.SearchByKey("key"), Is.EquivalentTo(new[] { "second" }));
        });

        _provider.ClearByKey("key");
        Assert.That(_provider.Get("key"), Is.Null);
    }
}
