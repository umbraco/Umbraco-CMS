using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Infrastructure.HybridCache;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.PublishedCache.HybridCache;

/// <summary>
/// Tests that the synchronous <see cref="MediaCache"/> lookups use the synchronous
/// <see cref="IMediaCacheService"/> members rather than blocking on the asynchronous ones.
/// </summary>
/// <remarks>
/// The synchronous service members return converted (L0) cache hits without entering the asynchronous
/// read logic, and read the database synchronously on a miss. <c>FilterAvailable</c> binds its method-group
/// call (<c>candidateKeys.Select(GetById)</c>) to the single-argument <see cref="MediaCache.GetById(Guid)"/>
/// overload, so that overload is covered alongside the two-argument one that delegates to it.
/// </remarks>
[TestFixture]
public class MediaCacheSyncFastPathTests
{
    [Test]
    public void GetById_By_Key_Uses_The_Synchronous_Lookup()
    {
        IPublishedContent expected = Mock.Of<IPublishedContent>();
        var key = Guid.NewGuid();
        Mock<IMediaCacheService> cacheService = CreateCacheService(key, expected);

        IPublishedContent? actual = CreateCache(cacheService).GetById(key);

        Assert.That(actual, Is.SameAs(expected));
        cacheService.Verify(s => s.GetByKeyAsync(It.IsAny<Guid>()), Times.Never);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void GetById_By_Preview_And_Key_Uses_The_Synchronous_Lookup(bool preview)
    {
        IPublishedContent expected = Mock.Of<IPublishedContent>();
        var key = Guid.NewGuid();
        Mock<IMediaCacheService> cacheService = CreateCacheService(key, expected);

        IPublishedContent? actual = CreateCache(cacheService).GetById(preview, key);

        Assert.That(actual, Is.SameAs(expected));
        cacheService.Verify(s => s.GetByKeyAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public void GetById_By_Id_Uses_The_Synchronous_Lookup()
    {
        IPublishedContent expected = Mock.Of<IPublishedContent>();
        var cacheService = new Mock<IMediaCacheService>();
        cacheService.Setup(s => s.GetById(1234)).Returns(expected);

        IPublishedContent? actual = CreateCache(cacheService).GetById(1234);

        Assert.That(actual, Is.SameAs(expected));
        cacheService.Verify(s => s.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    private static Mock<IMediaCacheService> CreateCacheService(Guid key, IPublishedContent expected)
    {
        var cacheService = new Mock<IMediaCacheService>();
        cacheService.Setup(s => s.GetByKey(key)).Returns(expected);
        return cacheService;
    }

    private static MediaCache CreateCache(Mock<IMediaCacheService> cacheService)
        => new(cacheService.Object, Mock.Of<IMediaNavigationQueryService>());
}
