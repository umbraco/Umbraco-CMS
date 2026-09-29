using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Infrastructure.HybridCache;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.PublishedCache.HybridCache;

/// <summary>
/// Tests that <see cref="MediaCache"/> consults <see cref="IMediaCacheService.TryGetCached"/>
/// first and skips the async fallback entirely when the L0 cache has the requested item.
/// </summary>
/// <remarks>
/// The async path (<c>GetByKeyAsync</c>) is the slow case — id/key map lookup + distributed
/// cache + database + factory work. On a warm site we want the per-key sync calls inside
/// <c>FilterAvailable</c>'s lazy chain to take the fast path. Because <c>FilterAvailable</c>
/// binds its method-group call (<c>candidateKeys.Select(GetById)</c>) to the single-argument
/// <see cref="MediaCache.GetById(Guid)"/> overload, this fixture asserts the fast path fires
/// for that overload (and the two-argument overload that delegates to it), not only the
/// two-argument one.
/// </remarks>
[TestFixture]
public class MediaCacheSyncFastPathTests
{
    [Test]
    public void GetById_Guid_HitsL0ViaTryGetCached_ReturnsCachedAndSkipsAsync()
    {
        IPublishedContent expected = Mock.Of<IPublishedContent>();
        var cacheService = CreateHitCacheService(expected);

        MediaCache cache = CreateCache(cacheService);

        IPublishedContent? actual = cache.GetById(Guid.NewGuid());

        Assert.That(actual, Is.SameAs(expected));
        cacheService.Verify(
            s => s.GetByKeyAsync(It.IsAny<Guid>()),
            Times.Never,
            "Async path should not run when TryGetCached hits");
    }

    [Test]
    public void GetById_PreviewGuid_HitsL0ViaTryGetCached_ReturnsCachedAndSkipsAsync()
    {
        IPublishedContent expected = Mock.Of<IPublishedContent>();
        var cacheService = CreateHitCacheService(expected);

        MediaCache cache = CreateCache(cacheService);

        IPublishedContent? actual = cache.GetById(preview: true, contentId: Guid.NewGuid());

        Assert.That(actual, Is.SameAs(expected));
        cacheService.Verify(
            s => s.GetByKeyAsync(It.IsAny<Guid>()),
            Times.Never,
            "Async path should not run when TryGetCached hits");
    }

    [Test]
    public void GetById_Guid_MissesL0_FallsThroughToAsyncPath()
    {
        IPublishedContent expected = Mock.Of<IPublishedContent>();
        var cacheService = new Mock<IMediaCacheService>();
        cacheService
            .Setup(s => s.TryGetCached(It.IsAny<Guid>(), out It.Ref<IPublishedContent?>.IsAny))
            .Returns(new TryGetCachedDelegate((Guid _, out IPublishedContent? content) =>
            {
                content = null;
                return false;
            }));
        cacheService
            .Setup(s => s.GetByKeyAsync(It.IsAny<Guid>()))
            .ReturnsAsync(expected);

        MediaCache cache = CreateCache(cacheService);

        IPublishedContent? actual = cache.GetById(Guid.NewGuid());

        Assert.That(actual, Is.SameAs(expected));
        cacheService.Verify(
            s => s.GetByKeyAsync(It.IsAny<Guid>()),
            Times.Once,
            "Async path runs exactly once on TryGetCached miss");
    }

    private static Mock<IMediaCacheService> CreateHitCacheService(IPublishedContent expected)
    {
        var cacheService = new Mock<IMediaCacheService>();
        cacheService
            .Setup(s => s.TryGetCached(It.IsAny<Guid>(), out It.Ref<IPublishedContent?>.IsAny))
            .Returns(new TryGetCachedDelegate((Guid _, out IPublishedContent? content) =>
            {
                content = expected;
                return true;
            }));
        return cacheService;
    }

    private static MediaCache CreateCache(Mock<IMediaCacheService> cacheService)
        => new(
            cacheService.Object,
            Mock.Of<IPublishedContentTypeCache>(),
            Mock.Of<IMediaNavigationQueryService>());

    // Moq cannot bind directly to ref / out parameters in the lambda overload, so we
    // declare a delegate that matches the TryGetCached signature and pass it explicitly.
    private delegate bool TryGetCachedDelegate(Guid key, out IPublishedContent? content);
}
