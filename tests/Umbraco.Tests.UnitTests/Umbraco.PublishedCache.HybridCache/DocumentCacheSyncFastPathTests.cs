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
/// Tests that <see cref="DocumentCache.GetById(bool, Guid)"/> consults
/// <see cref="IDocumentCacheService.TryGetCached"/> first and skips the async fallback
/// entirely when the L0 cache has the requested item.
/// </summary>
/// <remarks>
/// The async path (<c>GetByKeyAsync</c>) is the slow case — distributed cache + database +
/// factory work. On a warm site we want to confirm the per-key sync calls inside
/// <c>FilterAvailable</c>'s lazy chain take the fast path and never spin up an async state
/// machine. This fixture asserts that contract by mocking the service and verifying call
/// counts.
/// </remarks>
[TestFixture]
public class DocumentCacheSyncFastPathTests
{
    [Test]
    public void GetById_HitsL0ViaTryGetCached_ReturnsCachedAndSkipsAsync()
    {
        IPublishedContent expected = Mock.Of<IPublishedContent>();
        var cacheService = new Mock<IDocumentCacheService>();
        cacheService
            .Setup(s => s.TryGetCached(It.IsAny<Guid>(), It.IsAny<bool>(), out It.Ref<IPublishedContent?>.IsAny))
            .Returns(new TryGetCachedDelegate((Guid _, bool _, out IPublishedContent? content) =>
            {
                content = expected;
                return true;
            }));

        DocumentCache cache = CreateCache(cacheService);

        IPublishedContent? actual = cache.GetById(preview: false, contentId: Guid.NewGuid());

        Assert.That(actual, Is.SameAs(expected));
        cacheService.Verify(
            s => s.GetByKeyAsync(It.IsAny<Guid>(), It.IsAny<bool?>()),
            Times.Never,
            "Async path should not run when TryGetCached hits");
    }

    [Test]
    public void GetById_MissesL0_FallsThroughToAsyncPath()
    {
        IPublishedContent expected = Mock.Of<IPublishedContent>();
        var cacheService = new Mock<IDocumentCacheService>();
        cacheService
            .Setup(s => s.TryGetCached(It.IsAny<Guid>(), It.IsAny<bool>(), out It.Ref<IPublishedContent?>.IsAny))
            .Returns(new TryGetCachedDelegate((Guid _, bool _, out IPublishedContent? content) =>
            {
                content = null;
                return false;
            }));
        cacheService
            .Setup(s => s.GetByKeyAsync(It.IsAny<Guid>(), It.IsAny<bool?>()))
            .ReturnsAsync(expected);

        DocumentCache cache = CreateCache(cacheService);

        IPublishedContent? actual = cache.GetById(preview: false, contentId: Guid.NewGuid());

        Assert.That(actual, Is.SameAs(expected));
        cacheService.Verify(
            s => s.GetByKeyAsync(It.IsAny<Guid>(), It.IsAny<bool?>()),
            Times.Once,
            "Async path runs exactly once on TryGetCached miss");
    }

    private static DocumentCache CreateCache(Mock<IDocumentCacheService> cacheService)
        => new(
            cacheService.Object,
            Mock.Of<IPublishedContentTypeCache>(),
            Mock.Of<IDocumentNavigationQueryService>(),
            Mock.Of<IDocumentUrlService>(),
            new Lazy<IPublishedUrlProvider>(() => Mock.Of<IPublishedUrlProvider>()));

    // Moq cannot bind directly to ref / out parameters in the lambda overload, so we
    // declare a delegate that matches the TryGetCached signature and pass it explicitly.
    private delegate bool TryGetCachedDelegate(Guid key, bool preview, out IPublishedContent? content);
}
