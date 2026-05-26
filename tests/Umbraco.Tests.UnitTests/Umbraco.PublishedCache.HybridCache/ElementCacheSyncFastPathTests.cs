using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.HybridCache;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.PublishedCache.HybridCache;

/// <summary>
/// Tests that <see cref="ElementCache.GetById(bool, Guid)"/> consults
/// <see cref="IElementCacheService.TryGetCached"/> first and skips the async fallback
/// entirely when the L0 cache has the requested element.
/// </summary>
/// <remarks>
/// Mirrors <see cref="DocumentCacheSyncFastPathTests"/>. The dominant element consumer is
/// <c>ElementPickerValueConverter</c>, which runs sync-over-async during property value
/// conversion on the render hot path; this fixture confirms the fast path is taken when the
/// L0 cache is warm and that the async path is only used as a fallback.
/// </remarks>
[TestFixture]
public class ElementCacheSyncFastPathTests
{
    [Test]
    public void GetById_HitsL0ViaTryGetCached_ReturnsCachedAndSkipsAsync()
    {
        IPublishedElement expected = Mock.Of<IPublishedElement>();
        var cacheService = new Mock<IElementCacheService>();
        cacheService
            .Setup(s => s.TryGetCached(It.IsAny<Guid>(), It.IsAny<bool>(), out It.Ref<IPublishedElement?>.IsAny))
            .Returns(new TryGetCachedDelegate((Guid _, bool _, out IPublishedElement? element) =>
            {
                element = expected;
                return true;
            }));

        ElementCache cache = new(cacheService.Object);

        IPublishedElement? actual = cache.GetById(preview: false, key: Guid.NewGuid());

        Assert.That(actual, Is.SameAs(expected));
        cacheService.Verify(
            s => s.GetByKeyAsync(It.IsAny<Guid>(), It.IsAny<bool?>()),
            Times.Never,
            "Async path should not run when TryGetCached hits");
    }

    [Test]
    public void GetById_MissesL0_FallsThroughToAsyncPath()
    {
        IPublishedElement expected = Mock.Of<IPublishedElement>();
        var cacheService = new Mock<IElementCacheService>();
        cacheService
            .Setup(s => s.TryGetCached(It.IsAny<Guid>(), It.IsAny<bool>(), out It.Ref<IPublishedElement?>.IsAny))
            .Returns(new TryGetCachedDelegate((Guid _, bool _, out IPublishedElement? element) =>
            {
                element = null;
                return false;
            }));
        cacheService
            .Setup(s => s.GetByKeyAsync(It.IsAny<Guid>(), It.IsAny<bool?>()))
            .ReturnsAsync(expected);

        ElementCache cache = new(cacheService.Object);

        IPublishedElement? actual = cache.GetById(preview: false, key: Guid.NewGuid());

        Assert.That(actual, Is.SameAs(expected));
        cacheService.Verify(
            s => s.GetByKeyAsync(It.IsAny<Guid>(), It.IsAny<bool?>()),
            Times.Once,
            "Async path runs exactly once on TryGetCached miss");
    }

    // Moq cannot bind directly to ref / out parameters in the lambda overload, so we
    // declare a delegate that matches the TryGetCached signature and pass it explicitly.
    private delegate bool TryGetCachedDelegate(Guid key, bool preview, out IPublishedElement? element);
}
