using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;
using Umbraco.Cms.Infrastructure.HybridCache.Services;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Tests.Benchmarks.Fixtures;

/// <summary>
/// Media equivalent of <see cref="SyntheticPublishedTreeFixture"/>: builds an in-memory published-media
/// stack populated with a synthetic tree so benchmarks can exercise <c>Children()</c> / <c>Descendants()</c>
/// on media without a database. Supports a cold (unseeded) mode backed by a latency-injecting,
/// round-trip-counting repository so the batched read-through can be measured against the reported
/// large-media-library workload.
/// </summary>
internal sealed class SyntheticPublishedMediaTreeFixture
{
    private readonly List<Guid> _allKeys = new();
    private readonly Dictionary<Guid, ContentCacheNode> _nodesByKey = new();
    private Microsoft.Extensions.Caching.Hybrid.HybridCache _hybridCache = null!;
    private MediaCacheService _cacheService = null!;
    private int _singleFetchCount;
    private int _batchFetchCount;

    public IMediaNavigationQueryService NavigationQueryService { get; private set; } = null!;

    public IPublishedMediaStatusFilteringService StatusFilteringService { get; private set; } = null!;

    public IPublishedContent Root { get; private set; } = null!;

    public IReadOnlyList<Guid> AllKeys => _allKeys;

    public Guid RootKey { get; private set; }

    public int SingleFetchCount => _singleFetchCount;

    public int BatchFetchCount => _batchFetchCount;

    public async Task InitialiseAsync(int branchCount, int leafCount, int propertyCount = 10, bool seed = true, int repoLatencyMs = 0)
    {
        IPublishedModelFactory publishedModelFactory = new NoopPublishedModelFactory();
        IVariationContextAccessor variationContextAccessor = new ThreadCultureVariationContextAccessor();
        IPropertyRenderingContextAccessor propertyRenderingContextAccessor = Mock.Of<IPropertyRenderingContextAccessor>();
        IElementsCache elementsCache = new ElementsDictionaryAppCache();
        var converters = new PropertyValueConverterCollection(() => Enumerable.Empty<IPropertyValueConverter>());

        PublishedContentType contentType = BuildTestMediaType(converters, publishedModelFactory, propertyCount);
        Guid contentTypeKey = contentType.Key;

        MediaNavigationService navigationService = BuildNavigationService();
        RootKey = Guid.NewGuid();
        navigationService.Add(RootKey, contentTypeKey, parentKey: null, sortOrder: 0);
        _allKeys.Add(RootKey);
        for (var b = 0; b < branchCount; b++)
        {
            Guid branchKey = Guid.NewGuid();
            navigationService.Add(branchKey, contentTypeKey, RootKey, b);
            _allKeys.Add(branchKey);
            for (var l = 0; l < leafCount; l++)
            {
                Guid leafKey = Guid.NewGuid();
                navigationService.Add(leafKey, contentTypeKey, branchKey, l);
                _allKeys.Add(leafKey);
            }
        }

        var services = new ServiceCollection();
        services.AddLogging();
#pragma warning disable EXTEXP0018
        services.AddHybridCache(opts =>
        {
            opts.MaximumPayloadBytes = 100 * 1024 * 1024;
        }).AddSerializer<ContentCacheNode, HybridCacheSerializer>();
#pragma warning restore EXTEXP0018
        ServiceProvider sp = services.BuildServiceProvider();
        Microsoft.Extensions.Caching.Hybrid.HybridCache hybridCache = sp.GetRequiredService<Microsoft.Extensions.Caching.Hybrid.HybridCache>();

        var contentTypeCacheMock = new Mock<IPublishedContentTypeCache>();
        contentTypeCacheMock.Setup(x => x.Get(PublishedItemType.Media, It.IsAny<int>())).Returns(contentType);
        contentTypeCacheMock.Setup(x => x.Get(PublishedItemType.Media, It.IsAny<Guid>())).Returns(contentType);
        contentTypeCacheMock.Setup(x => x.Get(PublishedItemType.Media, It.IsAny<string>())).Returns(contentType);
        IPublishedContentTypeCache contentTypeCache = contentTypeCacheMock.Object;

        IPublishedContentFactory publishedContentFactory = new PublishedContentFactory(
            elementsCache,
            variationContextAccessor,
            propertyRenderingContextAccessor,
            contentTypeCache);

        // Repository: when NOT seeded, serves nodes from the in-memory map, counting round trips and
        // applying an optional per-call latency so the benchmark can model database round-trip cost.
        var repoMock = new Mock<IDatabaseCacheRepository>();
        repoMock.Setup(r => r.GetMediaSourceAsync(It.IsAny<Guid>()))
            .Returns(async (Guid key) =>
            {
                Interlocked.Increment(ref _singleFetchCount);
                if (repoLatencyMs > 0)
                {
                    await Task.Delay(repoLatencyMs);
                }

                return _nodesByKey.GetValueOrDefault(key);
            });
        repoMock.Setup(r => r.GetMediaSourcesAsync(It.IsAny<IEnumerable<Guid>>()))
            .Returns(async (IEnumerable<Guid> keys) =>
            {
                Interlocked.Increment(ref _batchFetchCount);
                if (repoLatencyMs > 0)
                {
                    await Task.Delay(repoLatencyMs);
                }

                return (IEnumerable<ContentCacheNode>)keys
                    .Select(k => _nodesByKey.GetValueOrDefault(k))
                    .Where(n => n is not null)
                    .Select(n => n!)
                    .ToArray();
            });

        var idKeyMapMock = new Mock<IIdKeyMap>();
        idKeyMapMock.Setup(x => x.GetIdForKey(It.IsAny<Guid>(), It.IsAny<UmbracoObjectTypes>()))
            .Returns((Guid _, UmbracoObjectTypes _) => Attempt.Succeed(1));
        idKeyMapMock.Setup(x => x.GetKeyForId(It.IsAny<int>(), It.IsAny<UmbracoObjectTypes>()))
            .Returns(Attempt.Fail<Guid>());

        var scopeMock = new Mock<ICoreScope>();
        var scopeProviderMock = new Mock<ICoreScopeProvider>();
        scopeProviderMock.Setup(x => x.CreateCoreScope(
                It.IsAny<IsolationLevel>(),
                It.IsAny<RepositoryCacheMode>(),
                It.IsAny<IEventDispatcher>(),
                It.IsAny<IScopedNotificationPublisher>(),
                It.IsAny<bool?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns(scopeMock.Object);

        var cacheService = new MediaCacheService(
            repoMock.Object,
            idKeyMapMock.Object,
            scopeProviderMock.Object,
            hybridCache,
            publishedContentFactory,
            Mock.Of<ICacheNodeFactory>(),
            Enumerable.Empty<IMediaSeedKeyProvider>(),
            publishedModelFactory,
            Options.Create(new CacheSettings()),
            NullLogger<MediaCacheService>.Instance,
            new ConvertedPublishedContentCacheFactory(null, NullLogger<ConvertedPublishedContentCacheFactory>.Instance));

        _hybridCache = hybridCache;
        _cacheService = cacheService;

        foreach (Guid key in _allKeys)
        {
            _nodesByKey[key] = BuildContentCacheNode(key, contentType.Id, propertyCount);
        }

        if (seed)
        {
            foreach (Guid key in _allKeys)
            {
                await hybridCache.SetAsync(key.ToString(), _nodesByKey[key]);
            }
        }

        var mediaCache = new MediaCache(cacheService, contentTypeCache, navigationService);

        NavigationQueryService = navigationService;
        StatusFilteringService = new PublishedMediaStatusFilteringService(mediaCache, cacheService);

        Root = (await cacheService.GetByKeyAsync(RootKey))!;
    }

    public async Task ResetColdAsync()
    {
        _cacheService.ClearConvertedContentCache();
        foreach (Guid key in _allKeys)
        {
            await _hybridCache.RemoveAsync(key.ToString());
        }

        Interlocked.Exchange(ref _singleFetchCount, 0);
        Interlocked.Exchange(ref _batchFetchCount, 0);
    }

    private static PublishedContentType BuildTestMediaType(
        PropertyValueConverterCollection converters,
        IPublishedModelFactory modelFactory,
        int propertyCount)
    {
        var jsonSerializer = new SystemTextConfigurationEditorJsonSerializer(new DefaultJsonSerializerEncoderFactory());
        var dataType = new DataType(new VoidEditor(Mock.Of<IDataValueEditorFactory>()), jsonSerializer) { Id = 1 };
        var dataTypeServiceMock = new Mock<IDataTypeService>();

#pragma warning disable CS0618
        dataTypeServiceMock.Setup(x => x.GetAll()).Returns(new[] { dataType });
#pragma warning restore CS0618

        var factory = new PublishedContentTypeFactory(modelFactory, converters, dataTypeServiceMock.Object);

        IEnumerable<IPublishedPropertyType> CreatePropertyTypes(IPublishedContentType contentType)
        {
            for (var i = 0; i < propertyCount; i++)
            {
                yield return factory.CreatePropertyType(contentType, $"prop{i}", dataType.Id, ContentVariation.Nothing);
            }
        }

        return new PublishedContentType(
            Guid.NewGuid(),
            1000,
            "benchImage",
            PublishedItemType.Media,
            Enumerable.Empty<string>(),
            CreatePropertyTypes,
            ContentVariation.Nothing,
            isElement: false);
    }

    private static MediaNavigationService BuildNavigationService()
        => new(
            Mock.Of<ICoreScopeProvider>(),
            Mock.Of<INavigationRepository>(),
            Mock.Of<IMediaTypeService>());

    private static ContentCacheNode BuildContentCacheNode(Guid key, int contentTypeId, int propertyCount)
    {
        var properties = new Dictionary<string, PropertyData[]>(propertyCount);
        for (var i = 0; i < propertyCount; i++)
        {
            properties[$"prop{i}"] =
            [
                new PropertyData
                {
                    Value = $"value-{i}",
                    Culture = string.Empty,
                    Segment = string.Empty,
                },
            ];
        }

        var data = new ContentData(
            name: $"Media-{key.ToString()[..8]}",
            urlSegment: null,
            versionId: 1,
            versionDate: DateTime.UtcNow,
            writerId: -1,
            templateId: 0,
            published: true,
            properties: properties,
            cultureInfos: null);

        return new ContentCacheNode
        {
            Id = Math.Abs(key.GetHashCode()),
            Key = key,
            SortOrder = 0,
            CreateDate = DateTime.UtcNow,
            CreatorId = -1,
            ContentTypeId = contentTypeId,
            IsDraft = false,
            Data = data,
        };
    }
}
