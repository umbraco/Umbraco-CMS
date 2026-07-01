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
        var cache = new ConvertedPublishedContentCache<string, IPublishedContent>();

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
        var cache = new ConvertedPublishedContentCache<string, IPublishedContent>();

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
        var cache = new ConvertedPublishedContentCache<string, IPublishedContent>();

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
        var cache = new ConvertedPublishedContentCache<string, IPublishedContent>();

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
    public void Can_Track_Element_Values()
    {
        var cache = new ConvertedPublishedContentCache<string, IPublishedElement>();

        cache.Set("a", Element(), 100);

        Assert.Multiple(() =>
        {
            Assert.That(cache.Count, Is.EqualTo(1));
            Assert.That(cache.ApproximateSizeInBytes, Is.EqualTo(100));
            Assert.That(cache.TryGet("a", out IPublishedElement? element), Is.True);
            Assert.That(element, Is.Not.Null);
        });
    }

    private static IPublishedContent Content() => new Mock<IPublishedContent>().Object;

    private static IPublishedElement Element() => new Mock<IPublishedElement>().Object;

    private static IPublishedContent ContentOfType(int contentTypeId)
    {
        var contentType = new Mock<IPublishedContentType>();
        contentType.SetupGet(x => x.Id).Returns(contentTypeId);
        var content = new Mock<IPublishedContent>();
        content.SetupGet(x => x.ContentType).Returns(contentType.Object);
        return content.Object;
    }
}
