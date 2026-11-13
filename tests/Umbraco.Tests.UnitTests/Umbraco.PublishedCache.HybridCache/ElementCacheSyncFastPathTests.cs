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

    [TestCase(true)]
    [TestCase(false)]
    public void GetByIds_Uses_The_Synchronous_Batched_Lookup(bool preview)
    {
        IPublishedElement expected = Mock.Of<IPublishedElement>();
        Guid[] keys = [Guid.NewGuid(), Guid.NewGuid()];
        var cacheService = new Mock<IElementCacheService>();
        cacheService.Setup(s => s.GetByKeys(keys, preview)).Returns([expected]);

        IReadOnlyList<IPublishedElement> actual = new ElementCache(cacheService.Object).GetByIds(preview, keys);

        Assert.That(actual, Is.EqualTo(new[] { expected }));
        cacheService.Verify(s => s.GetByKeysAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<bool?>()), Times.Never);
        cacheService.Verify(s => s.GetByKey(It.IsAny<Guid>(), It.IsAny<bool?>()), Times.Never);
    }

    [TestCase(true)]
    [TestCase(false)]
    [TestCase(null)]
    public async Task GetByIdsAsync_Uses_The_Asynchronous_Batched_Lookup(bool? preview)
    {
        IPublishedElement expected = Mock.Of<IPublishedElement>();
        Guid[] keys = [Guid.NewGuid(), Guid.NewGuid()];
        var cacheService = new Mock<IElementCacheService>();
        cacheService.Setup(s => s.GetByKeysAsync(keys, preview)).ReturnsAsync([expected]);

        IReadOnlyList<IPublishedElement> actual = await new ElementCache(cacheService.Object).GetByIdsAsync(keys, preview);

        Assert.That(actual, Is.EqualTo(new[] { expected }));
        cacheService.Verify(s => s.GetByKeyAsync(It.IsAny<Guid>(), It.IsAny<bool?>()), Times.Never);
    }

    [Test]
    public async Task Cache_Default_Asynchronous_Batched_Lookup_Returns_Existing_Elements_In_Key_Order()
    {
        IPublishedElement first = Mock.Of<IPublishedElement>();
        IPublishedElement second = Mock.Of<IPublishedElement>();
        Guid[] keys = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
        var cache = new Mock<IPublishedElementCache> { CallBase = true };
        cache.Setup(c => c.GetByIdAsync(keys[0], true)).ReturnsAsync(first);
        cache.Setup(c => c.GetByIdAsync(keys[1], true)).ReturnsAsync((IPublishedElement?)null);
        cache.Setup(c => c.GetByIdAsync(keys[2], true)).ReturnsAsync(second);

        Assert.That(await cache.Object.GetByIdsAsync(keys, true), Is.EqualTo(new[] { first, second }));
    }

    [Test]
    public async Task Service_Default_Batched_Lookup_Returns_Existing_Elements_In_Key_Order()
    {
        IPublishedElement first = Mock.Of<IPublishedElement>();
        IPublishedElement second = Mock.Of<IPublishedElement>();
        Guid[] keys = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
        var cacheService = new Mock<IElementCacheService> { CallBase = true };
        cacheService.Setup(s => s.GetByKeyAsync(keys[0], true)).ReturnsAsync(first);
        cacheService.Setup(s => s.GetByKeyAsync(keys[1], true)).ReturnsAsync((IPublishedElement?)null);
        cacheService.Setup(s => s.GetByKeyAsync(keys[2], true)).ReturnsAsync(second);

        Assert.That(await cacheService.Object.GetByKeysAsync(keys, true), Is.EqualTo(new[] { first, second }));
        Assert.That(cacheService.Object.GetByKeys(keys, true), Is.EqualTo(new[] { first, second }));
    }

    [Test]
    public void Cache_Default_Batched_Lookup_Returns_Existing_Elements_In_Key_Order()
    {
        IPublishedElement first = Mock.Of<IPublishedElement>();
        IPublishedElement second = Mock.Of<IPublishedElement>();
        Guid[] keys = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
        var cache = new Mock<IPublishedElementCache> { CallBase = true };
        cache.Setup(c => c.GetById(false, keys[0])).Returns(first);
        cache.Setup(c => c.GetById(false, keys[1])).Returns((IPublishedElement?)null);
        cache.Setup(c => c.GetById(false, keys[2])).Returns(second);

        Assert.That(cache.Object.GetByIds(false, keys), Is.EqualTo(new[] { first, second }));
    }
}
