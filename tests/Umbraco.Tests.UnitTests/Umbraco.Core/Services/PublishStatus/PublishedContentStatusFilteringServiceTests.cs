using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services.PublishStatus;

[TestFixture]
public partial class PublishedContentStatusFilteringServiceTests
{
    [Test]
    public void FilterAvailable_Invariant_ForNonPreview_YieldsPublishedItems()
    {
        var (sut, items) = SetupInvariant(false);

        var children = sut.FilterAvailable(items.Keys, null).ToArray();
        Assert.AreEqual(5, children.Length);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, children[0].Id);
            Assert.AreEqual(2, children[1].Id);
            Assert.AreEqual(4, children[2].Id);
            Assert.AreEqual(6, children[3].Id);
            Assert.AreEqual(8, children[4].Id);
        });
    }

    [Test]
    public void FilterAvailable_Invariant_ForPreview_YieldsUnpublishedItems()
    {
        var (sut, items) = SetupInvariant(true);

        var children = sut.FilterAvailable(items.Keys, null).ToArray();
        Assert.AreEqual(10, children.Length);
        for (var i = 0; i < 10; i++)
        {
            Assert.AreEqual(i, children[i].Id);
        }
    }

    [TestCase("da-DK", 3)]
    [TestCase("en-US", 4)]
    public void FilterAvailable_Variant_ForNonPreview_YieldsPublishedItemsInCulture(string culture, int expectedNumberOfChildren)
    {
        var (sut, items) = SetupVariant(false, culture);

        var children = sut.FilterAvailable(items.Keys, culture).ToArray();
        Assert.AreEqual(expectedNumberOfChildren, children.Length);

        // IDs 0 through 3 exist in both en-US and da-DK - only even IDs are published
        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, children[0].Id);
            Assert.AreEqual(2, children[1].Id);
        });

        // IDs 4 through 6 exist only in en-US - only even IDs are published
        if (culture == "en-US")
        {
            Assert.AreEqual(4, children[2].Id);
            Assert.AreEqual(6, children[3].Id);
        }

        // IDs 7 through 9 exist only in da-DK - only even IDs are published
        if (culture == "da-DK")
        {
            Assert.AreEqual(8, children[2].Id);
        }
    }

    [TestCase("da-DK")]
    [TestCase("en-US")]
    public void FilterAvailable_Variant_ForPreview_YieldsUnpublishedItemsInCulture(string culture)
    {
        var (sut, items) = SetupVariant(true, culture);

        var children = sut.FilterAvailable(items.Keys, culture).ToArray();
        Assert.AreEqual(7, children.Length);

        // IDs 0 through 3 exist in both en-US and da-DK
        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, children[0].Id);
            Assert.AreEqual(1, children[1].Id);
            Assert.AreEqual(2, children[2].Id);
            Assert.AreEqual(3, children[3].Id);
        });

        // IDs 4 through 6 exist only in en-US
        if (culture == "en-US")
        {
            Assert.AreEqual(4, children[4].Id);
            Assert.AreEqual(5, children[5].Id);
            Assert.AreEqual(6, children[6].Id);
        }

        // IDs 7 through 9 exist only in da-DK
        if (culture == "da-DK")
        {
            Assert.AreEqual(7, children[4].Id);
            Assert.AreEqual(8, children[5].Id);
            Assert.AreEqual(9, children[6].Id);
        }
    }

    [TestCase("da-DK")]
    [TestCase("en-US")]
    public void FilterAvailable_MixedVariance_ForNonPreview_YieldsPublishedItemsInCultureOrInvariant(string culture)
    {
        var (sut, items) = SetupMixedVariance(false, culture);

        var children = sut.FilterAvailable(items.Keys, culture).ToArray();
        Assert.AreEqual(4, children.Length);

        // IDs 0 through 2 are invariant - only even IDs are published
        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, children[0].Id);
            Assert.AreEqual(2, children[1].Id);
        });

        // IDs 3 through 5 exist in both en-US and da-DK - only even IDs are published
        Assert.Multiple(() =>
        {
            Assert.AreEqual(4, children[2].Id);
        });

        // IDs 6 and 7 exist only in en-US - only even IDs are published
        if (culture == "en-US")
        {
            Assert.AreEqual(6, children[3].Id);
        }

        // IDs 8 and 9 exist only in da-DK - only even IDs are published
        if (culture == "da-DK")
        {
            Assert.AreEqual(8, children[3].Id);
        }
    }

    [TestCase("da-DK")]
    [TestCase("en-US")]
    public void FilterAvailable_MixedVariance_FoPreview_YieldsPublishedItemsInCultureOrInvariant(string culture)
    {
        var (sut, items) = SetupMixedVariance(true, culture);

        var children = sut.FilterAvailable(items.Keys, culture).ToArray();
        Assert.AreEqual(8, children.Length);

        // IDs 0 through 2 are invariant
        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, children[0].Id);
            Assert.AreEqual(1, children[1].Id);
            Assert.AreEqual(2, children[2].Id);
        });

        // IDs 3 through 5 exist in both en-US and da-DK
        Assert.Multiple(() =>
        {
            Assert.AreEqual(3, children[3].Id);
            Assert.AreEqual(4, children[4].Id);
            Assert.AreEqual(5, children[5].Id);
        });

        // IDs 6 and 7 exist only in en-US
        if (culture == "en-US")
        {
            Assert.AreEqual(6, children[6].Id);
            Assert.AreEqual(7, children[7].Id);
        }

        // IDs 8 and 9 exist only in da-DK
        if (culture == "da-DK")
        {
            Assert.AreEqual(8, children[6].Id);
            Assert.AreEqual(9, children[7].Id);
        }
    }

    // sets up invariant test data:
    // - 10 documents with IDs 0 through 9
    // - even IDs (0, 2, ...) are published, odd are unpublished
    private (PublishedContentStatusFilteringService PublishedContentStatusFilteringService, Dictionary<Guid, IPublishedContent> Items) SetupInvariant(bool forPreview)
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
        var publishStatusQueryService = SetupPublishStatusQueryService(items);
        var variationContextAccessor = SetupVariantContextAccessor(null);

        return (
            new PublishedContentStatusFilteringService(
                variationContextAccessor,
                publishStatusQueryService,
                previewService,
                publishedContentCache),
            items);
    }

    // sets up variant test data:
    // - 10 documents with IDs 0 through 9
    // - IDs 0 through 3 exist in both en-US and da-DK
    // - IDs 4 through 6 exist only in en-US
    // - IDs 7 through 9 exist only in da-DK
    // - even IDs (0, 2, ...) are published, odd are unpublished
    private (PublishedContentStatusFilteringService PublishedContentStatusFilteringService, Dictionary<Guid, IPublishedContent> Items) SetupVariant(bool forPreview, string requestCulture)
    {
        var contentType = new Mock<IPublishedContentType>();
        contentType.SetupGet(c => c.Variations).Returns(ContentVariation.Culture);

        var items = new Dictionary<Guid, IPublishedContent>();
        for (var i = 0; i < 10; i++)
        {
            var content = new Mock<IPublishedContent>();

            var key = Guid.NewGuid();
            string[] cultures = i <= 3
                ? ["da-DK", "en-US"]
                : i <= 6
                    ? ["en-US"]
                    : ["da-DK"];
            var cultureDictionary = cultures.ToDictionary(culture => culture, culture => new PublishedCultureInfo(culture, culture, $"{i}-{culture}", DateTime.MinValue));
            content.SetupGet(c => c.Key).Returns(key);
            content.SetupGet(c => c.ContentType).Returns(contentType.Object);
            content.SetupGet(c => c.Cultures).Returns(cultureDictionary);
            content.SetupGet(c => c.Id).Returns(i);

            items[key] = content.Object;
        }

        var publishedContentCache = SetupPublishedContentCache(forPreview, items);
        var previewService = SetupPreviewService(forPreview);
        var publishStatusQueryService = SetupPublishStatusQueryService(items);
        var variationContextAccessor = SetupVariantContextAccessor(requestCulture);

        return (
            new PublishedContentStatusFilteringService(
                variationContextAccessor,
                publishStatusQueryService,
                previewService,
                publishedContentCache),
            items);
    }

    // sets up mixed variant test data:
    // - 10 documents with IDs 0 through 9
    // - IDs 0 through 2 are invariant
    // - IDs 3 through 5 exist in both en-US and da-DK
    // - IDs 6 and 7 exist only in en-US
    // - IDs 8 and 9 exist only in da-DK
    // - even IDs (0, 2, ...) are published, odd are unpublished
    private (PublishedContentStatusFilteringService PublishedContentStatusFilteringService, Dictionary<Guid, IPublishedContent> Items) SetupMixedVariance(bool forPreview, string requestCulture)
    {
        var invariantContentType = new Mock<IPublishedContentType>();
        invariantContentType.SetupGet(c => c.Variations).Returns(ContentVariation.Nothing);

        var variantContentType = new Mock<IPublishedContentType>();
        variantContentType.SetupGet(c => c.Variations).Returns(ContentVariation.Culture);

        var items = new Dictionary<Guid, IPublishedContent>();
        for (var i = 0; i < 10; i++)
        {
            var content = new Mock<IPublishedContent>();
            var contentType = i <= 2
                ? invariantContentType
                : variantContentType;

            var key = Guid.NewGuid();
            string[] cultures = i <= 2
                ? []
                : i <= 5
                    ? ["da-DK", "en-US"]
                    : i <= 7
                        ? ["en-US"]
                        : ["da-DK"];
            var cultureDictionary = cultures.ToDictionary(culture => culture, culture => new PublishedCultureInfo(culture, culture, $"{i}-{culture}", DateTime.MinValue));
            content.SetupGet(c => c.Key).Returns(key);
            content.SetupGet(c => c.ContentType).Returns(contentType.Object);
            content.SetupGet(c => c.Cultures).Returns(cultureDictionary);
            content.SetupGet(c => c.Id).Returns(i);

            items[key] = content.Object;
        }

        var publishedContentCache = SetupPublishedContentCache(forPreview, items);
        var previewService = SetupPreviewService(forPreview);
        var publishStatusQueryService = SetupPublishStatusQueryService(items);
        var variationContextAccessor = SetupVariantContextAccessor(requestCulture);

        return (
            new PublishedContentStatusFilteringService(
                variationContextAccessor,
                publishStatusQueryService,
                previewService,
                publishedContentCache),
            items);
    }

    private IPublishStatusQueryService SetupPublishStatusQueryService(Dictionary<Guid, IPublishedContent> items)
        => SetupPublishStatusQueryService(items, id => id % 2 == 0);

    private IPublishStatusQueryService SetupPublishStatusQueryService(Dictionary<Guid, IPublishedContent> items, Func<int, bool> idIsPublished)
    {
        var publishStatusQueryService = new Mock<IPublishStatusQueryService>();
        publishStatusQueryService
            .Setup(p => p.IsDocumentPublished(It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns((Guid key, string culture) => items
                                                       .TryGetValue(key, out var item)
                                                   && idIsPublished(item.Id)
                                                   && (item.ContentType.VariesByCulture() is false || item.Cultures.ContainsKey(culture)));
        return publishStatusQueryService.Object;
    }

    private IPreviewService SetupPreviewService(bool forPreview)
    {
        var previewService = new Mock<IPreviewService>();
        previewService.Setup(p => p.IsInPreview()).Returns(forPreview);
        return previewService.Object;
    }

    private IVariationContextAccessor SetupVariantContextAccessor(string? requestCulture)
    {
        var variationContextAccessor = new Mock<IVariationContextAccessor>();
        variationContextAccessor.SetupGet(v => v.VariationContext).Returns(new VariationContext(requestCulture));
        return variationContextAccessor.Object;
    }

    private IPublishedContentCache SetupPublishedContentCache(bool forPreview, Dictionary<Guid, IPublishedContent> items)
    {
        var publishedContentCache = new Mock<IPublishedContentCache>();
        publishedContentCache
            .Setup(c => c.GetById(forPreview, It.IsAny<Guid>()))
            .Returns((bool preview, Guid key) => items.TryGetValue(key, out var item) ? item : null);
        return publishedContentCache.Object;
    }
}
