using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services.PublishStatus;

[TestFixture]
public class PublishedContentStatusFilteringServiceTests
{
    // Matches IDocumentCacheService.TryGetCached so Moq can bind the out parameter.
    private delegate bool TryGetCachedCallback(Guid key, bool preview, out IPublishedContent? content);

    [Test]
    public void FilterAvailable_Invariant_ForNonPreview_YieldsPublishedItems()
    {
        var (sut, items) = SetupInvariant(false);

        var children = sut.FilterAvailable(items.Keys, null).ToArray();
        Assert.That(children, Has.Length.EqualTo(5));
        Assert.Multiple(() =>
        {
            Assert.That(children[0].Id, Is.EqualTo(0));
            Assert.That(children[1].Id, Is.EqualTo(2));
            Assert.That(children[2].Id, Is.EqualTo(4));
            Assert.That(children[3].Id, Is.EqualTo(6));
            Assert.That(children[4].Id, Is.EqualTo(8));
        });
    }

    [Test]
    public void FilterAvailable_Invariant_ForPreview_YieldsUnpublishedItems()
    {
        var (sut, items) = SetupInvariant(true);

        var children = sut.FilterAvailable(items.Keys, null).ToArray();
        Assert.That(children, Has.Length.EqualTo(10));
        for (var i = 0; i < 10; i++)
        {
            Assert.That(children[i].Id, Is.EqualTo(i));
        }
    }

    [TestCase("da-DK", 3)]
    [TestCase("en-US", 4)]
    [TestCase("*", 5)]
    public void FilterAvailable_Variant_ForNonPreview_YieldsPublishedItemsInCulture(string culture, int expectedNumberOfChildren)
    {
        var (sut, items) = SetupVariant(false, culture == Constants.System.InvariantCulture ? "en-US" : culture);

        var children = sut.FilterAvailable(items.Keys, culture).ToArray();
        Assert.That(children, Has.Length.EqualTo(expectedNumberOfChildren));

        // IDs 0 through 3 exist in both en-US and da-DK - only even IDs are published
        Assert.Multiple(() =>
        {
            Assert.That(children[0].Id, Is.EqualTo(0));
            Assert.That(children[1].Id, Is.EqualTo(2));
        });

        // IDs 4 through 6 exist only in en-US - only even IDs are published
        if (culture == "en-US")
        {
            Assert.That(children[2].Id, Is.EqualTo(4));
            Assert.That(children[3].Id, Is.EqualTo(6));
        }

        // IDs 7 through 9 exist only in da-DK - only even IDs are published
        if (culture == "da-DK")
        {
            Assert.That(children[2].Id, Is.EqualTo(8));
        }

        if (culture == Constants.System.InvariantCulture)
        {
            Assert.That(children[2].Id, Is.EqualTo(4));
            Assert.That(children[3].Id, Is.EqualTo(6));
            Assert.That(children[4].Id, Is.EqualTo(8));
        }
    }

    [TestCase("da-DK", 7)]
    [TestCase("en-US", 7)]
    [TestCase("*", 10)]
    public void FilterAvailable_Variant_ForPreview_YieldsUnpublishedItemsInCulture(string culture, int expectedNumberOfChildren)
    {
        var (sut, items) = SetupVariant(true, culture == Constants.System.InvariantCulture ? "en-US" : culture);

        var children = sut.FilterAvailable(items.Keys, culture).ToArray();
        Assert.That(children, Has.Length.EqualTo(expectedNumberOfChildren));

        // IDs 0 through 3 exist in both en-US and da-DK
        Assert.Multiple(() =>
        {
            Assert.That(children[0].Id, Is.EqualTo(0));
            Assert.That(children[1].Id, Is.EqualTo(1));
            Assert.That(children[2].Id, Is.EqualTo(2));
            Assert.That(children[3].Id, Is.EqualTo(3));
        });

        // IDs 4 through 6 exist only in en-US
        if (culture == "en-US")
        {
            Assert.That(children[4].Id, Is.EqualTo(4));
            Assert.That(children[5].Id, Is.EqualTo(5));
            Assert.That(children[6].Id, Is.EqualTo(6));
        }

        // IDs 7 through 9 exist only in da-DK
        if (culture == "da-DK")
        {
            Assert.That(children[4].Id, Is.EqualTo(7));
            Assert.That(children[5].Id, Is.EqualTo(8));
            Assert.That(children[6].Id, Is.EqualTo(9));
        }

        if (culture == Constants.System.InvariantCulture)
        {
            Assert.That(children[4].Id, Is.EqualTo(4));
            Assert.That(children[5].Id, Is.EqualTo(5));
            Assert.That(children[6].Id, Is.EqualTo(6));
            Assert.That(children[7].Id, Is.EqualTo(7));
            Assert.That(children[8].Id, Is.EqualTo(8));
            Assert.That(children[9].Id, Is.EqualTo(9));
        }
    }

    [TestCase("da-DK", 1)]
    [TestCase("en-US", 2)]
    [TestCase("*", 3)]
    public void FilterAvailable_Variant_ForNonPreview_YieldsOnlyItemsWithPublishedAncestorPath(string culture, int expectedNumberOfChildren)
    {
        var (sut, items) = SetupVariant(
            false,
            culture == Constants.System.InvariantCulture ? "en-US" : culture,
            (key, _, allItems) => allItems.Keys.IndexOf(key) > 2);

        var children = sut.FilterAvailable(items.Keys, culture).ToArray();
        Assert.That(children, Has.Length.EqualTo(expectedNumberOfChildren));

        // IDs 0 through 3 exist in both en-US and da-DK, but none pass both the published and ancestor-path checks

        // IDs 4 through 6 exist only in en-US - only even IDs are published
        if (culture == "en-US")
        {
            Assert.That(children[0].Id, Is.EqualTo(4));
            Assert.That(children[1].Id, Is.EqualTo(6));
        }

        // IDs 7 through 9 exist only in da-DK - only even IDs are published
        if (culture == "da-DK")
        {
            Assert.That(children[0].Id, Is.EqualTo(8));
        }

        if (culture == Constants.System.InvariantCulture)
        {
            Assert.That(children[0].Id, Is.EqualTo(4));
            Assert.That(children[1].Id, Is.EqualTo(6));
            Assert.That(children[2].Id, Is.EqualTo(8));
        }
    }

    [TestCase("da-DK", 7)]
    [TestCase("en-US", 7)]
    [TestCase("*", 10)]
    public void FilterAvailable_Variant_ForPreview_IgnoresMissingPublishedAncestorPath(string culture, int expectedNumberOfChildren)
    {
        var (sut, items) = SetupVariant(
            true,
            culture == Constants.System.InvariantCulture ? "en-US" : culture,
            (_, _, _) => false);

        var children = sut.FilterAvailable(items.Keys, culture).ToArray();
        Assert.That(children, Has.Length.EqualTo(expectedNumberOfChildren));

        // IDs 0 through 3 exist in both en-US and da-DK
        Assert.Multiple(() =>
        {
            Assert.That(children[0].Id, Is.EqualTo(0));
            Assert.That(children[1].Id, Is.EqualTo(1));
            Assert.That(children[2].Id, Is.EqualTo(2));
            Assert.That(children[3].Id, Is.EqualTo(3));
        });

        // IDs 4 through 6 exist only in en-US
        if (culture == "en-US")
        {
            Assert.That(children[4].Id, Is.EqualTo(4));
            Assert.That(children[5].Id, Is.EqualTo(5));
            Assert.That(children[6].Id, Is.EqualTo(6));
        }

        // IDs 7 through 9 exist only in da-DK
        if (culture == "da-DK")
        {
            Assert.That(children[4].Id, Is.EqualTo(7));
            Assert.That(children[5].Id, Is.EqualTo(8));
            Assert.That(children[6].Id, Is.EqualTo(9));
        }

        if (culture == Constants.System.InvariantCulture)
        {
            Assert.That(children[4].Id, Is.EqualTo(4));
            Assert.That(children[5].Id, Is.EqualTo(5));
            Assert.That(children[6].Id, Is.EqualTo(6));
            Assert.That(children[7].Id, Is.EqualTo(7));
            Assert.That(children[8].Id, Is.EqualTo(8));
            Assert.That(children[9].Id, Is.EqualTo(9));
        }
    }

    [TestCase("da-DK", 4)]
    [TestCase("en-US", 4)]
    [TestCase("*", 5)]
    public void FilterAvailable_MixedVariance_ForNonPreview_YieldsPublishedItemsInCultureOrInvariant(string culture, int expectedNumberOfChildren)
    {
        var (sut, items) = SetupMixedVariance(false, culture == Constants.System.InvariantCulture ? "en-US" : culture);

        var children = sut.FilterAvailable(items.Keys, culture).ToArray();
        Assert.That(children, Has.Length.EqualTo(expectedNumberOfChildren));

        // IDs 0 through 2 are invariant - only even IDs are published
        Assert.Multiple(() =>
        {
            Assert.That(children[0].Id, Is.EqualTo(0));
            Assert.That(children[1].Id, Is.EqualTo(2));
        });

        // IDs 3 through 5 exist in both en-US and da-DK - only even IDs are published
        Assert.Multiple(() =>
        {
            Assert.That(children[2].Id, Is.EqualTo(4));
        });

        // IDs 6 and 7 exist only in en-US - only even IDs are published
        if (culture == "en-US")
        {
            Assert.That(children[3].Id, Is.EqualTo(6));
        }

        // IDs 8 and 9 exist only in da-DK - only even IDs are published
        if (culture == "da-DK")
        {
            Assert.That(children[3].Id, Is.EqualTo(8));
        }

        if (culture == Constants.System.InvariantCulture)
        {
            Assert.That(children[3].Id, Is.EqualTo(6));
            Assert.That(children[4].Id, Is.EqualTo(8));
        }
    }

    [TestCase("da-DK", 8)]
    [TestCase("en-US", 8)]
    [TestCase("*", 10)]
    public void FilterAvailable_MixedVariance_ForPreview_YieldsPublishedItemsInCultureOrInvariant(string culture, int expectedNumberOfChildren)
    {
        var (sut, items) = SetupMixedVariance(true, culture == Constants.System.InvariantCulture ? "en-US" : culture);

        var children = sut.FilterAvailable(items.Keys, culture).ToArray();
        Assert.That(children, Has.Length.EqualTo(expectedNumberOfChildren));

        // IDs 0 through 2 are invariant
        Assert.Multiple(() =>
        {
            Assert.That(children[0].Id, Is.EqualTo(0));
            Assert.That(children[1].Id, Is.EqualTo(1));
            Assert.That(children[2].Id, Is.EqualTo(2));
        });

        // IDs 3 through 5 exist in both en-US and da-DK
        Assert.Multiple(() =>
        {
            Assert.That(children[3].Id, Is.EqualTo(3));
            Assert.That(children[4].Id, Is.EqualTo(4));
            Assert.That(children[5].Id, Is.EqualTo(5));
        });

        // IDs 6 and 7 exist only in en-US
        if (culture == "en-US")
        {
            Assert.That(children[6].Id, Is.EqualTo(6));
            Assert.That(children[7].Id, Is.EqualTo(7));
        }

        // IDs 8 and 9 exist only in da-DK
        if (culture == "da-DK")
        {
            Assert.That(children[6].Id, Is.EqualTo(8));
            Assert.That(children[7].Id, Is.EqualTo(9));
        }

        if (culture == Constants.System.InvariantCulture)
        {
            Assert.That(children[6].Id, Is.EqualTo(6));
            Assert.That(children[7].Id, Is.EqualTo(7));
            Assert.That(children[8].Id, Is.EqualTo(8));
            Assert.That(children[9].Id, Is.EqualTo(9));
        }
    }

    [Test]
    public void FilterAvailable_IsLazy_WarmCache_ShortCircuitsWithoutBatching()
    {
        var (sut, items, _, batchedKeys, serviceMock) = SetupCounting(forPreview: true, warm: true);

        IPublishedContent? first = sut.FilterAvailable(items.Keys, null).FirstOrDefault();

        Assert.That(first, Is.Not.Null);

        // An all-L0-hit chunk must be served synchronously — the batched read is never engaged.
        Assert.That(batchedKeys, Is.Empty);
        serviceMock.Verify(
            s => s.GetByKeysAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<bool?>()),
            Times.Never);
    }

    [Test]
    public void FilterAvailable_IsLazy_FirstOrDefaultMaterialisesOnlyOneItem()
    {
        var (sut, items, _, batchedKeys, _) = SetupCounting(forPreview: true, warm: false);

        IPublishedContent? first = sut.FilterAvailable(items.Keys, null).FirstOrDefault();

        Assert.That(first, Is.Not.Null);

        // Slow-start's first chunk is a single key, so exactly one item is materialised.
        Assert.That(batchedKeys, Has.Count.EqualTo(1));
    }

    [Test]
    public void FilterAvailable_IsLazy_TakeMaterialisesOnlyRequestedItems()
    {
        var (sut, items, _, batchedKeys, _) = SetupCounting(forPreview: true, warm: false);

        IPublishedContent[] taken = sut.FilterAvailable(items.Keys, null).Take(3).ToArray();

        Assert.That(taken, Has.Length.EqualTo(3));

        // Chunks of 1 then 2 cover the three requested items; the remaining seven are never materialised.
        Assert.That(batchedKeys, Has.Count.EqualTo(3));
    }

    [Test]
    public void FilterAvailable_IsLazy_NonPreviewTakeShortCircuitsPublishStatusQueries()
    {
        var (sut, items, statusMock, batchedKeys, _) = SetupCounting(forPreview: false, warm: false);

        IPublishedContent[] taken = sut.FilterAvailable(items.Keys, null).Take(3).ToArray();

        Assert.That(taken, Has.Length.EqualTo(3));

        // Only the keys passing the publish-status filter are materialised, so exactly three.
        Assert.That(batchedKeys, Has.Count.EqualTo(3));

        // Publish status must short-circuit before the full candidate set is enumerated.
        statusMock.Verify(
            s => s.IsPublished(It.IsAny<Guid>(), It.IsAny<string>()),
            Times.AtMost(items.Count - 1));
    }

    [Test]
    public void FilterAvailable_IsLazy_FullEnumerationMaterialisesAllItemsInFewBatches()
    {
        var (sut, items, _, batchedKeys, serviceMock) = SetupCounting(forPreview: true, warm: false);

        IPublishedContent[] all = sut.FilterAvailable(items.Keys, null).ToArray();

        Assert.That(all, Has.Length.EqualTo(items.Count));

        // Every item is materialised exactly once...
        Assert.That(batchedKeys, Has.Count.EqualTo(items.Count));
        Assert.That(batchedKeys.Distinct().Count(), Is.EqualTo(items.Count));

        // ...but collapsed into a handful of batched reads rather than one per item.
        serviceMock.Verify(
            s => s.GetByKeysAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<bool?>()),
            Times.AtMost(5));
    }

    // Sets up invariant data with mocks exposed so tests can verify materialisation behaviour.
    // - 10 invariant documents with IDs 0 through 9
    // - even IDs are published, odd are not (relevant only for non-preview)
    // When warm, TryGetCached serves every key from L0; otherwise every key misses L0 and is routed
    // through the batched GetByKeysAsync, whose requested keys are recorded in the returned list.
    private static (
        PublishedContentStatusFilteringService Service,
        Dictionary<Guid, IPublishedContent> Items,
        Mock<IDocumentPublishStatusQueryService> StatusMock,
        List<Guid> BatchedKeys,
        Mock<IDocumentCacheService> ServiceMock)
        SetupCounting(bool forPreview, bool warm)
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

        var batchedKeys = new List<Guid>();
        var serviceMock = new Mock<IDocumentCacheService>();
        serviceMock
            .Setup(s => s.TryGetCached(It.IsAny<Guid>(), It.IsAny<bool>(), out It.Ref<IPublishedContent?>.IsAny))
            .Returns(new TryGetCachedCallback((Guid key, bool _, out IPublishedContent? content) =>
            {
                if (warm && items.TryGetValue(key, out IPublishedContent? item))
                {
                    content = item;
                    return true;
                }

                content = null;
                return false;
            }));
        serviceMock
            .Setup(s => s.GetByKeysAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<bool?>()))
            .ReturnsAsync((IReadOnlyCollection<Guid> keys, bool? _) =>
            {
                batchedKeys.AddRange(keys);
                return (IReadOnlyList<IPublishedContent>)keys
                    .Select(k => items.TryGetValue(k, out IPublishedContent? item) ? item : null)
                    .WhereNotNull()
                    .ToArray();
            });

        var statusMock = new Mock<IDocumentPublishStatusQueryService>();
        statusMock
            .Setup(s => s.IsPublished(It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns((Guid key, string _) => items.TryGetValue(key, out IPublishedContent? item) && item.Id % 2 == 0);
        statusMock
            .Setup(s => s.HasPublishedAncestorPath(It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(true);

        var previewSessionService = SetupPreviewService(forPreview);

        var variationContextAccessor = new Mock<IVariationContextAccessor>();
        variationContextAccessor.SetupGet(v => v.VariationContext).Returns(new VariationContext(null));

        var service = new PublishedContentStatusFilteringService(
            variationContextAccessor.Object,
            statusMock.Object,
            previewSessionService,
            Mock.Of<IPublishedContentCache>(),
            serviceMock.Object);

        return (service, items, statusMock, batchedKeys, serviceMock);
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

        var documentCacheService = SetupDocumentCacheService(items);
        var previewSessionService = SetupPreviewService(forPreview);
        var publishStatusQueryService = SetupPublishStatusQueryService(items);
        var variationContextAccessor = SetupVariantContextAccessor(null);

        return (
            new PublishedContentStatusFilteringService(
                variationContextAccessor,
                publishStatusQueryService,
                previewSessionService,
                Mock.Of<IPublishedContentCache>(),
                documentCacheService),
            items);
    }

    // sets up variant test data:
    // - 10 documents with IDs 0 through 9
    // - IDs 0 through 3 exist in both en-US and da-DK
    // - IDs 4 through 6 exist only in en-US
    // - IDs 7 through 9 exist only in da-DK
    // - even IDs (0, 2, ...) are published, odd are unpublished
    private (PublishedContentStatusFilteringService PublishedContentStatusFilteringService, Dictionary<Guid, IPublishedContent> Items) SetupVariant(bool forPreview, string requestCulture, Func<Guid, string, Dictionary<Guid, IPublishedContent>, bool>? hasPublishedAncestorPath = null)
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

        var documentCacheService = SetupDocumentCacheService(items);
        var previewSessionService = SetupPreviewService(forPreview);
        var publishStatusQueryService = SetupPublishStatusQueryService(items, hasPublishedAncestorPath);
        var variationContextAccessor = SetupVariantContextAccessor(requestCulture);

        return (
            new PublishedContentStatusFilteringService(
                variationContextAccessor,
                publishStatusQueryService,
                previewSessionService,
                Mock.Of<IPublishedContentCache>(),
                documentCacheService),
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

        var documentCacheService = SetupDocumentCacheService(items);
        var previewSessionService = SetupPreviewService(forPreview);
        var publishStatusQueryService = SetupPublishStatusQueryService(items);
        var variationContextAccessor = SetupVariantContextAccessor(requestCulture);

        return (
            new PublishedContentStatusFilteringService(
                variationContextAccessor,
                publishStatusQueryService,
                previewSessionService,
                Mock.Of<IPublishedContentCache>(),
                documentCacheService),
            items);
    }

    private IDocumentPublishStatusQueryService SetupPublishStatusQueryService(Dictionary<Guid, IPublishedContent> items, Func<Guid, string, Dictionary<Guid, IPublishedContent>, bool>? hasPublishedAncestorPath = null)
        => SetupPublishStatusQueryService(items, id => id % 2 == 0, hasPublishedAncestorPath);

    private IDocumentPublishStatusQueryService SetupPublishStatusQueryService(Dictionary<Guid, IPublishedContent> items, Func<int, bool> idIsPublished, Func<Guid, string, Dictionary<Guid, IPublishedContent>, bool>? hasPublishedAncestorPath = null)
    {
        var publishStatusQueryService = new Mock<IDocumentPublishStatusQueryService>();
        publishStatusQueryService
            .Setup(p => p.IsPublished(It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns((Guid key, string culture) => items
                                                       .TryGetValue(key, out var item)
                                                   && idIsPublished(item.Id)
                                                   && (culture == Constants.System.InvariantCulture || item.ContentType.VariesByCulture() is false || item.Cultures.ContainsKey(culture)));
        publishStatusQueryService
            .Setup(s => s.HasPublishedAncestorPath(It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns((Guid key, string culture) => hasPublishedAncestorPath?.Invoke(key, culture, items) ?? true);
        return publishStatusQueryService.Object;
    }

    private static IPreviewSessionService SetupPreviewService(bool forPreview)
    {
        var previewSessionService = new Mock<IPreviewSessionService>();
        previewSessionService.Setup(p => p.IsActive()).Returns(forPreview);
        return previewSessionService.Object;
    }

    private IVariationContextAccessor SetupVariantContextAccessor(string? requestCulture)
    {
        var variationContextAccessor = new Mock<IVariationContextAccessor>();
        variationContextAccessor.SetupGet(v => v.VariationContext).Returns(new VariationContext(requestCulture));
        return variationContextAccessor.Object;
    }

    // Routes every key through the batched GetByKeysAsync (L0 always misses), returning the matching
    // items in order. This isolates the filtering service from the cache-tier logic under test elsewhere.
    private static IDocumentCacheService SetupDocumentCacheService(Dictionary<Guid, IPublishedContent> items)
    {
        var serviceMock = new Mock<IDocumentCacheService>();
        serviceMock
            .Setup(s => s.TryGetCached(It.IsAny<Guid>(), It.IsAny<bool>(), out It.Ref<IPublishedContent?>.IsAny))
            .Returns(new TryGetCachedCallback((Guid _, bool _, out IPublishedContent? content) =>
            {
                content = null;
                return false;
            }));
        serviceMock
            .Setup(s => s.GetByKeysAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<bool?>()))
            .ReturnsAsync((IReadOnlyCollection<Guid> keys, bool? _) =>
                (IReadOnlyList<IPublishedContent>)keys
                    .Select(k => items.TryGetValue(k, out IPublishedContent? item) ? item : null)
                    .WhereNotNull()
                    .ToArray());
        return serviceMock.Object;
    }
}
