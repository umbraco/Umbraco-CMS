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
        // Integer keys are used deliberately: the W-TinyLFU admission policy estimates access frequency with a
        // count-min sketch keyed off the entry's hash code, and .NET randomizes string.GetHashCode() per
        // process. With string keys an unlucky per-process seed can collide a cold item's sketch buckets with a
        // hot item's, inflating its estimate enough to evict the hot entry that is still cycling through
        // probation — a green-locally / red-on-CI flake. Integer hash codes are stable across processes, so the
        // sketch (and therefore this assertion) is deterministic. The high access count gives the hot set a wide
        // frequency margin so the outcome is unambiguous; the behaviour under test — frequently requested content
        // surviving a one-off scan of more-recently-touched items, which a plain LRU would NOT do — does not
        // depend on the exact numbers, so this still guards the behaviour if the backing cache is swapped later.
        var cache = new ConvertedPublishedContentCache<int>(maximumItems: 10);

        int[] hot = [0, 1, 2];

        foreach (int key in hot)
        {
            cache.Set(key, Content(), 10);
        }

        // Establish a high access frequency for the hot set. Maintenance runs between rounds (as it would on
        // a live site) so the accesses are recorded as frequency rather than dropped from the read buffer.
        for (var round = 0; round < 30; round++)
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
