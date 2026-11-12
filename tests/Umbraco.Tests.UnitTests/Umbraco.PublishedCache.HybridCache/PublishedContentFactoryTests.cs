using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.PublishedCache.HybridCache;

[TestFixture]
public class PublishedContentFactoryTests
{
    [TestCase(PublishedItemType.Content)]
    [TestCase(PublishedItemType.Element)]
    [TestCase(PublishedItemType.Media)]
    public void CreateDate_And_UpdateDate_With_Unspecified_Kind_Are_Exposed_As_Utc(PublishedItemType itemType)
    {
        var createDate = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Unspecified);
        var versionDate = new DateTime(2026, 2, 3, 4, 5, 6, DateTimeKind.Unspecified);

        IPublishedContentType contentType = new PublishedContentType(
            Guid.NewGuid(),
            1000,
            "test",
            itemType,
            Enumerable.Empty<string>(),
            _ => Enumerable.Empty<IPublishedPropertyType>(),
            ContentVariation.Nothing);

        var contentTypeCacheMock = new Mock<IPublishedContentTypeCache>();
        contentTypeCacheMock
            .Setup(x => x.Get(It.IsAny<PublishedItemType>(), contentType.Id))
            .Returns(contentType);

        var factory = new PublishedContentFactory(
            new ElementsDictionaryAppCache(),
            new ThreadCultureVariationContextAccessor(),
            Mock.Of<IPropertyRenderingContextAccessor>(),
            contentTypeCacheMock.Object);

        var contentCacheNode = new ContentCacheNode
        {
            Id = 1,
            Key = Guid.NewGuid(),
            SortOrder = 0,
            CreateDate = createDate,
            CreatorId = -1,
            ContentTypeId = contentType.Id,
            Data = new ContentData(
                "Test",
                null,
                1,
                versionDate,
                -1,
                null,
                true,
                new Dictionary<string, PropertyData[]>(),
                null),
        };

        IPublishedElement? published = itemType switch
        {
            PublishedItemType.Content => factory.ToIPublishedContent(contentCacheNode, preview: false),
            PublishedItemType.Element => factory.ToIPublishedElement(contentCacheNode, preview: false),
            PublishedItemType.Media => factory.ToIPublishedMedia(contentCacheNode),
            _ => throw new ArgumentOutOfRangeException(nameof(itemType)),
        };

        Assert.That(published, Is.Not.Null);
        Assert.That(published!.CreateDate.Kind, Is.EqualTo(DateTimeKind.Utc));
        Assert.That(published.UpdateDate.Kind, Is.EqualTo(DateTimeKind.Utc));
        Assert.That(published.CreateDate, Is.EqualTo(createDate));
        Assert.That(published.UpdateDate, Is.EqualTo(versionDate));
    }
}
