using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services.PublishStatus;

public partial class PublishedContentStatusFilteringServiceTests
{
    [Test]
    public void FilterDescendants_Invariant_ForNonPreview_YieldsOnlyRoutablePublishedItems()
    {
        var (sut, items) = SetupForDescendantsInvariant(false);

        var descendants = sut.FilterDescendants(items.Keys, null).ToArray();
        Assert.AreEqual(4, descendants.Length);
        Assert.Multiple(() =>
        {
            // 3 is not published
            // 5 is not published, which means 6 should not be returned even though it's published
            // 7 is not published, which means 8 and 9 should not be returned even though they're published
            Assert.AreEqual(0, descendants[0].Id);
            Assert.AreEqual(1, descendants[1].Id);
            Assert.AreEqual(2, descendants[2].Id);
            Assert.AreEqual(4, descendants[3].Id);
        });
    }

    [Test]
    public void FilterDescendants_Invariant_ForPreview_YieldsUnpublishedItems()
    {
        var (sut, items) = SetupForDescendantsInvariant(true);

        var descendants = sut.FilterDescendants(items.Keys, null).ToArray();
        Assert.AreEqual(10, descendants.Length);
        for (var i = 0; i < 10; i++)
        {
            Assert.AreEqual(i, descendants[i].Id);
        }
    }

    // sets up invariant test data:
    // - 10 documents with IDs 0 through 9
    // - divided into three structures of three, where 0 is the root of all - [0 > 1 > 2 > 3], [0 > 4 > 5 > 6] and [0 > 7 > 8 > 9]
    // - 3, 5 and 7 are unpublished, the rest are published
    private (PublishedContentStatusFilteringService PublishedContentStatusFilteringService, Dictionary<Guid, IPublishedContent> Items) SetupForDescendantsInvariant(bool forPreview)
    {
        var contentType = new Mock<IPublishedContentType>();
        contentType.SetupGet(c => c.Variations).Returns(ContentVariation.Nothing);

        var items = new Dictionary<Guid, IPublishedContent>();
        for (var i = 0; i < 10; i++)
        {
            var content = new Mock<IPublishedContent>();

            var key = Guid.NewGuid();
            content.SetupGet(c => c.Key).Returns(key);
            content.SetupGet(c => c.ContentType).Returns(contentType.Object);
            content.SetupGet(c => c.Cultures).Returns(new Dictionary<string, PublishedCultureInfo>());
            content.SetupGet(c => c.Id).Returns(i);

            items[key] = content.Object;
        }

        var publishedContentCache = SetupPublishedContentCache(forPreview, items);
        var previewService = SetupPreviewService(forPreview);
        var publishStatusQueryService = SetupPublishStatusQueryService(items, id => id is not 3 or 5 or 7);
        // TODO KJA: setup IDocumentNavigationQueryService mock for structural query

        var variationContextAccessor = SetupVariantContextAccessor(null);

        return (
            new PublishedContentStatusFilteringService(
                variationContextAccessor,
                publishStatusQueryService,
                previewService,
                publishedContentCache),
            items);
    }
}
