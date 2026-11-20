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
/// Tests that the synchronous <see cref="DocumentCache"/> lookups use the synchronous
/// <see cref="IDocumentCacheService"/> members rather than blocking on the asynchronous ones.
/// </summary>
/// <remarks>
/// The synchronous service members return converted (L0) cache hits without entering the asynchronous
/// read logic, and read the database synchronously on a miss.
/// </remarks>
[TestFixture]
public class DocumentCacheSyncFastPathTests
{
    [TestCase(true)]
    [TestCase(false)]
    public void GetById_By_Key_Uses_The_Synchronous_Lookup(bool preview)
    {
        IPublishedContent expected = Mock.Of<IPublishedContent>();
        var key = Guid.NewGuid();
        var cacheService = new Mock<IDocumentCacheService>();
        cacheService.Setup(s => s.GetByKey(key, preview)).Returns(expected);

        IPublishedContent? actual = CreateCache(cacheService).GetById(preview, key);

        Assert.That(actual, Is.SameAs(expected));
        cacheService.Verify(s => s.GetByKeyAsync(It.IsAny<Guid>(), It.IsAny<bool?>()), Times.Never);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void GetById_By_Id_Uses_The_Synchronous_Lookup(bool preview)
    {
        IPublishedContent expected = Mock.Of<IPublishedContent>();
        var cacheService = new Mock<IDocumentCacheService>();
        cacheService.Setup(s => s.GetById(1234, preview)).Returns(expected);

        IPublishedContent? actual = CreateCache(cacheService).GetById(preview, 1234);

        Assert.That(actual, Is.SameAs(expected));
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
