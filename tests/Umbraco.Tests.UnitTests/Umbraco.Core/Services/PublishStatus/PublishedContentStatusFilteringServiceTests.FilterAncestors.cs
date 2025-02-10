using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services.PublishStatus;

public partial class PublishedContentStatusFilteringServiceTests
{
    [Test]
    public void FilterAncestors_Invariant_ForNonPreview_ItemWithUnpublishedAncestor_YieldsNoAncestors()
    {
        var (sut, items) = SetupForAncestorsInvariant(false);

        // document keys must be passed in reverse (down-top) order, as is expected by the ancestor filtering
        var ancestors = sut.FilterAncestors(items.Keys.Reverse(), null).ToArray();
        // the furthest descendant has several unpublished ancestors, so this should yield no published ancestors
        Assert.AreEqual(0, ancestors.Length);
    }

    [Test]
    public void FilterAncestors_Invariant_ForNonPreview_ClosestPublishedDescendant_YieldsAllItems()
    {
        var (sut, items) = SetupForAncestorsInvariant(false);

        // document keys must be passed in reverse (down-top) order, as is expected by the ancestor filtering
        var ancestors = sut.FilterAncestors(items.Keys.Take(3).Reverse(), null).ToArray();
        // items 2, 1 and 0 are all published
        Assert.AreEqual(3, ancestors.Length);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, ancestors[2].Id);
            Assert.AreEqual(1, ancestors[1].Id);
            Assert.AreEqual(2, ancestors[0].Id);
        });
    }

    [Test]
    public void FilterAncestors_Invariant_ForPreview_YieldsUnpublishedItems()
    {
        var (sut, items) = SetupForAncestorsInvariant(true);

        // document keys must be passed in reverse (down-top) order, as is expected by the ancestor filtering
        var ancestors = sut.FilterAncestors(items.Keys.Reverse(), null).ToArray();
        Assert.AreEqual(10, ancestors.Length);
        for (var i = 0; i < 10; i++)
        {
            var expectedId = 9 - i;
            Assert.AreEqual(expectedId, ancestors[i].Id);
        }
    }

    // sets up invariant test data:
    // - 10 documents with IDs 0 through 9
    // - each document is a child to the previous one - that is, [0 > 1 > 2 > 3 > 4 > 5 > 6 > 7 > 8 > 9]
    // - 3, 5 and 7 are unpublished, the rest are published
    private (PublishedContentStatusFilteringService PublishedContentStatusFilteringService, Dictionary<Guid, IPublishedContent> Items) SetupForAncestorsInvariant(bool forPreview)
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
        var publishStatusQueryService = SetupPublishStatusQueryService(items, id => new[] { 3, 5, 7 }.Contains(id) is false);
        Guid? mockParentKey;
        var documentNavigationQueryService = new Mock<IDocumentNavigationQueryService>();
        documentNavigationQueryService
            .Setup(d => d.TryGetParentKey(It.IsAny<Guid>(), out mockParentKey))
            .Returns(true)
            .Callback(new TryGetParentKeyCallback((Guid childKey, out Guid? parentKey) =>
            {
                var id = items[childKey].Id;
                parentKey = id is 0
                    ? null
                    : items.Values.First(item => item.Id == id - 1).Key;
            }));

        var variationContextAccessor = SetupVariantContextAccessor(null);

        return (
            new PublishedContentStatusFilteringService(
                variationContextAccessor,
                publishStatusQueryService,
                documentNavigationQueryService.Object,
                previewService,
                publishedContentCache),
            items);
    }
}
