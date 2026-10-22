using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Infrastructure.HybridCache;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.PublishedCache.HybridCache;

/// <summary>
/// The synchronous <see cref="DocumentCache"/> lookups must stay on the calling thread: they delegate to the
/// synchronous service members and never wrap the asynchronous ones. A strict mock makes any call to an
/// asynchronous member fail the test.
/// </summary>
[TestFixture]
public class DocumentCacheSyncFastPathTests
{
    [Test]
    public void GetById_ByKey_UsesTheSynchronousServiceLookup()
    {
        IPublishedContent expected = Mock.Of<IPublishedContent>();
        Guid key = Guid.NewGuid();
        var cacheService = new Mock<IDocumentCacheService>(MockBehavior.Strict);
        cacheService.Setup(s => s.GetByKey(key, false)).Returns(expected);

        DocumentCache cache = CreateCache(cacheService);

        Assert.That(cache.GetById(preview: false, contentId: key), Is.SameAs(expected));
        cacheService.Verify(s => s.GetByKey(key, false), Times.Once);
        cacheService.Verify(s => s.GetByKeyAsync(It.IsAny<Guid>(), It.IsAny<bool?>()), Times.Never);
    }

    [Test]
    public void GetById_ByKeyWithoutPreview_LeavesThePreviewDecisionToTheService()
    {
        Guid key = Guid.NewGuid();
        var cacheService = new Mock<IDocumentCacheService>(MockBehavior.Strict);
        cacheService.Setup(s => s.GetByKey(key, null)).Returns((IPublishedContent?)null);

        DocumentCache cache = CreateCache(cacheService);

        Assert.That(cache.GetById(key), Is.Null);
        cacheService.Verify(s => s.GetByKey(key, null), Times.Once);
    }

    [Test]
    public void GetById_ByInt_UsesTheSynchronousServiceLookup()
    {
        IPublishedContent expected = Mock.Of<IPublishedContent>();
        var cacheService = new Mock<IDocumentCacheService>(MockBehavior.Strict);
        cacheService.Setup(s => s.GetById(1234, true)).Returns(expected);
        cacheService.Setup(s => s.GetById(1234, null)).Returns(expected);

        DocumentCache cache = CreateCache(cacheService);

        Assert.Multiple(() =>
        {
            Assert.That(cache.GetById(preview: true, contentId: 1234), Is.SameAs(expected));
            Assert.That(cache.GetById(1234), Is.SameAs(expected));
        });
        cacheService.Verify(s => s.GetByIdAsync(It.IsAny<int>(), It.IsAny<bool?>()), Times.Never);
    }

    private static DocumentCache CreateCache(Mock<IDocumentCacheService> cacheService)
        => new(
            cacheService.Object,
            Mock.Of<IPublishedContentTypeCache>(),
            Mock.Of<IDocumentNavigationQueryService>(),
            Mock.Of<IDocumentUrlService>(),
            new Lazy<IPublishedUrlProvider>(() => Mock.Of<IPublishedUrlProvider>()));
}
