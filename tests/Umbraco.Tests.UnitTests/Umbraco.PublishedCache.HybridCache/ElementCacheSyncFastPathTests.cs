using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.HybridCache;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.PublishedCache.HybridCache;

/// <summary>
/// Tests that the synchronous <see cref="ElementCache"/> lookup uses the synchronous
/// <see cref="IElementCacheService"/> member rather than blocking on the asynchronous one.
/// </summary>
/// <remarks>
/// The synchronous service member returns converted (L0) cache hits without entering the asynchronous
/// read logic, and reads the database synchronously on a miss.
/// </remarks>
[TestFixture]
public class ElementCacheSyncFastPathTests
{
    [TestCase(true)]
    [TestCase(false)]
    public void GetById_Uses_The_Synchronous_Lookup(bool preview)
    {
        IPublishedElement expected = Mock.Of<IPublishedElement>();
        var key = Guid.NewGuid();
        var cacheService = new Mock<IElementCacheService>();
        cacheService.Setup(s => s.GetByKey(key, preview)).Returns(expected);

        IPublishedElement? actual = new ElementCache(cacheService.Object).GetById(preview, key);

        Assert.That(actual, Is.SameAs(expected));
        cacheService.Verify(s => s.GetByKeyAsync(It.IsAny<Guid>(), It.IsAny<bool?>()), Times.Never);
    }
}
