using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Preview;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;
using Umbraco.Cms.Infrastructure.HybridCache.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.PublishedCache.HybridCache;

/// <summary>
/// Entries evicted by a rebuild are reloaded from the database on their next miss, and that reload reads them through
/// the serializer factory's content type snapshot. The snapshot must therefore be discarded before the entries are
/// evicted, or a miss in between would read through definitions from before the change.
/// </summary>
[TestFixture]
public class CacheServiceRebuildMemoryCacheByContentTypeTests
{
    private const string Cleared = "snapshot cleared";
    private const string Evicted = "entries evicted";

    private List<string> _events = null!;
    private Mock<IContentCacheDataSerializerFactory> _serializerFactory = null!;
    private Mock<Microsoft.Extensions.Caching.Hybrid.HybridCache> _hybridCache = null!;

    [SetUp]
    public void SetUp()
    {
        _events = [];
        _serializerFactory = new Mock<IContentCacheDataSerializerFactory>();
        _serializerFactory.Setup(x => x.ClearTypeCache()).Callback(() => _events.Add(Cleared));
        _hybridCache = new Mock<Microsoft.Extensions.Caching.Hybrid.HybridCache>();
        _hybridCache
            .Setup(x => x.RemoveByTagAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .Callback(() => _events.Add(Evicted))
            .Returns(ValueTask.CompletedTask);
    }

    [Test]
    public async Task Rebuilding_Documents_By_Content_Type_Clears_The_Serializer_Type_Cache_Before_Evicting()
    {
        var cacheFactory = new Mock<IConvertedPublishedContentCacheFactory>();
        cacheFactory
            .Setup(x => x.Create<string, IPublishedContent>(It.IsAny<int?>(), It.IsAny<string>()))
            .Returns(Mock.Of<IConvertedPublishedContentCache<string, IPublishedContent>>());
        var service = new DocumentCacheService(
            Mock.Of<IDatabaseCacheRepository>(),
            Mock.Of<IIdKeyMap>(),
            Mock.Of<ICoreScopeProvider>(),
            _hybridCache.Object,
            Mock.Of<IPublishedContentFactory>(),
            Mock.Of<ICacheNodeFactory>(),
            [],
            new OptionsWrapper<CacheSettings>(new CacheSettings()),
            Mock.Of<IPublishedModelFactory>(),
            Mock.Of<IPreviewService>(),
            Mock.Of<IPublishStatusQueryService>(),
            NullLogger<DocumentCacheService>.Instance,
            cacheFactory.Object,
            Mock.Of<IPreviewAccessEvaluator>(),
            _serializerFactory.Object);

        await service.RebuildMemoryCacheByContentTypeAsync([1]);

        Assert.That(_events, Is.EqualTo(new[] { Cleared, Evicted }));
    }

    [Test]
    public async Task Rebuilding_Media_By_Content_Type_Clears_The_Serializer_Type_Cache_Before_Evicting()
    {
        var cacheFactory = new Mock<IConvertedPublishedContentCacheFactory>();
        cacheFactory
            .Setup(x => x.Create<Guid, IPublishedContent>(It.IsAny<int?>(), It.IsAny<string>()))
            .Returns(Mock.Of<IConvertedPublishedContentCache<Guid, IPublishedContent>>());
        var service = new MediaCacheService(
            Mock.Of<IDatabaseCacheRepository>(),
            Mock.Of<IIdKeyMap>(),
            Mock.Of<ICoreScopeProvider>(),
            _hybridCache.Object,
            Mock.Of<IPublishedContentFactory>(),
            Mock.Of<ICacheNodeFactory>(),
            [],
            Mock.Of<IPublishedModelFactory>(),
            new OptionsWrapper<CacheSettings>(new CacheSettings()),
            NullLogger<MediaCacheService>.Instance,
            cacheFactory.Object,
            _serializerFactory.Object);

        await service.RebuildMemoryCacheByContentTypeAsync([1]);

        Assert.That(_events, Is.EqualTo(new[] { Cleared, Evicted }));
    }

    [Test]
    public async Task Rebuilding_Elements_By_Content_Type_Clears_The_Serializer_Type_Cache_Before_Evicting()
    {
        var cacheFactory = new Mock<IConvertedPublishedContentCacheFactory>();
        cacheFactory
            .Setup(x => x.Create<string, IPublishedElement>(It.IsAny<int?>(), It.IsAny<string>()))
            .Returns(Mock.Of<IConvertedPublishedContentCache<string, IPublishedElement>>());
        var service = new ElementCacheService(
            Mock.Of<IDatabaseCacheRepository>(),
            Mock.Of<ICoreScopeProvider>(),
            _hybridCache.Object,
            Mock.Of<IPublishedContentFactory>(),
            Mock.Of<ICacheNodeFactory>(),
            [],
            Mock.Of<IPublishedModelFactory>(),
            Mock.Of<IPreviewService>(),
            new OptionsWrapper<CacheSettings>(new CacheSettings()),
            NullLogger<ElementCacheService>.Instance,
            cacheFactory.Object,
            Mock.Of<IPreviewAccessEvaluator>(),
            _serializerFactory.Object);

        await service.RebuildMemoryCacheByContentTypeAsync([1]);

        Assert.That(_events, Is.EqualTo(new[] { Cleared, Evicted }));
    }
}
