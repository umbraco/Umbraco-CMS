// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Infrastructure.HybridCache.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.HybridCache;

[TestFixture]
public class ConvertedPublishedContentCacheTests
{
    [Test]
    public void Can_Track_Count_And_Bytes_On_Set_And_Remove()
    {
        var cache = new ConvertedPublishedContentCache<string>();

        cache.Set("a", Content(), 100);
        cache.Set("b", Content(), 50);

        Assert.Multiple(() =>
        {
            Assert.That(cache.Count, Is.EqualTo(2));
            Assert.That(cache.ApproximateSizeInBytes, Is.EqualTo(150));
        });

        cache.Remove("a");

        Assert.Multiple(() =>
        {
            Assert.That(cache.Count, Is.EqualTo(1));
            Assert.That(cache.ApproximateSizeInBytes, Is.EqualTo(50));
        });
    }

    [Test]
    public void Can_Adjust_Bytes_When_Overwriting_Existing_Key()
    {
        var cache = new ConvertedPublishedContentCache<string>();

        cache.Set("a", Content(), 100);
        cache.Set("a", Content(), 30);

        Assert.Multiple(() =>
        {
            Assert.That(cache.Count, Is.EqualTo(1));
            Assert.That(cache.ApproximateSizeInBytes, Is.EqualTo(30));
        });
    }

    [Test]
    public void Can_Reset_Bytes_On_Clear()
    {
        var cache = new ConvertedPublishedContentCache<string>();

        cache.Set("a", Content(), 100);
        cache.Clear();

        Assert.Multiple(() =>
        {
            Assert.That(cache.Count, Is.EqualTo(0));
            Assert.That(cache.ApproximateSizeInBytes, Is.EqualTo(0));
        });
    }

    [Test]
    public void Can_Remove_Matching_Entries_With_RemoveWhere()
    {
        var cache = new ConvertedPublishedContentCache<string>();

        cache.Set("a", ContentOfType(1), 100);
        cache.Set("b", ContentOfType(2), 40);

        cache.RemoveWhere(content => content.ContentType.Id == 1);

        Assert.Multiple(() =>
        {
            Assert.That(cache.Count, Is.EqualTo(1));
            Assert.That(cache.ApproximateSizeInBytes, Is.EqualTo(40));
        });
    }

    [Test]
    public void Can_Bound_Entry_Count_When_Maximum_Configured()
    {
        var cache = new ConvertedPublishedContentCache<string>(maximumItems: 5);

        for (var i = 0; i < 100; i++)
        {
            cache.Set($"key-{i}", Content(), 10);
        }

        cache.RunPendingMaintenance();

        Assert.That(cache.Count, Is.LessThanOrEqualTo(5));
    }

    [Test]
    public void Can_Retain_Frequently_Accessed_Items_Under_Scan_Pressure()
    {
        // Guards the scan-resistance property: frequently requested content survives a one-off scan of
        // more-recently-touched items, which a plain LRU would NOT do (it keeps the most recent N, so a scan
        // larger than the cache evicts the whole hot set).
        //
        // The hot set is driven into W-TinyLFU's protected region, whose entries are not eviction candidates
        // until that region itself overflows — which three items in a ten-entry cache never cause — so survival
        // is guaranteed by structure rather than by the marginal frequency comparison for window/probation
        // residents (which depends on count-min sketch hashing and buffer behaviour). Int keys keep that hashing
        // stable across processes, as .NET randomizes string.GetHashCode().
        var cache = new ConvertedPublishedContentCache<int>(maximumItems: 10);

        int[] hot = [0, 1, 2];

        foreach (int key in hot)
        {
            cache.Set(key, Content(), 10);
        }

        // Push the hot items out of the single-slot admission window into the main region. Total held is
        // 3 hot + 4 decoys = 7 < 10, so nothing is evicted yet.
        for (var decoy = -4; decoy < 0; decoy++)
        {
            cache.Set(decoy, Content(), 10);
        }

        cache.RunPendingMaintenance();

        // Read the hot items so each is promoted from probation into the protected region.
        for (var round = 0; round < 5; round++)
        {
            foreach (int key in hot)
            {
                cache.TryGet(key, out _);
            }

            cache.RunPendingMaintenance();
        }

        // A one-off "scan" of other items, each requested once — these are touched more recently than the
        // hot set, so under a plain LRU they would evict it.
        for (var i = 100; i < 130; i++)
        {
            cache.Set(i, Content(), 10);
        }

        cache.RunPendingMaintenance();

        Assert.Multiple(() =>
        {
            foreach (int key in hot)
            {
                Assert.That(cache.TryGet(key, out _), Is.True, $"frequently accessed '{key}' should survive the scan");
            }

            Assert.That(cache.Count, Is.LessThanOrEqualTo(10));
        });
    }

    [Test]
    public void Can_Remove_And_Clear_When_Bounded()
    {
        var cache = new ConvertedPublishedContentCache<string>(maximumItems: 10);

        cache.Set("a", ContentOfType(1), 10);
        cache.Set("b", ContentOfType(2), 10);
        cache.RunPendingMaintenance();

        cache.RemoveWhere(content => content.ContentType.Id == 1);
        cache.RunPendingMaintenance();
        Assert.That(cache.TryGet("a", out _), Is.False);

        cache.Clear();
        cache.RunPendingMaintenance();
        Assert.That(cache.Count, Is.EqualTo(0));
    }

    private static IPublishedContent Content() => new Mock<IPublishedContent>().Object;

    private static IPublishedContent ContentOfType(int contentTypeId)
    {
        var contentType = new Mock<IPublishedContentType>();
        contentType.SetupGet(x => x.Id).Returns(contentTypeId);
        var content = new Mock<IPublishedContent>();
        content.SetupGet(x => x.ContentType).Returns(contentType.Object);
        return content.Object;
    }
}
