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
using Umbraco.Cms.Core.Routing;
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
/// Builds an in-memory published-content stack populated with a synthetic tree of a given shape,
/// so benchmarks can exercise <c>Children()</c> / <c>Descendants()</c> without a database.
/// </summary>
internal sealed class SyntheticPublishedTreeFixture
{
    private readonly List<Guid> _allKeys = new();
    private readonly Dictionary<Guid, ContentCacheNode> _nodesByKey = new();
    private Microsoft.Extensions.Caching.Hybrid.HybridCache _hybridCache = null!;
    private DocumentCacheService _cacheService = null!;
    private int _singleFetchCount;
    private int _batchFetchCount;

    public IDocumentNavigationQueryService NavigationQueryService { get; private set; } = null!;

    public IPublishedContentStatusFilteringService StatusFilteringService { get; private set; } = null!;

    public IPublishedContentCache PublishedContentCache { get; private set; } = null!;

    public IPublishedContent Root { get; private set; } = null!;

    public IPublishedValueFallback PublishedValueFallback { get; } = new NoopPublishedValueFallback();

    public IReadOnlyList<Guid> AllKeys => _allKeys;

    public Guid RootKey { get; private set; }

    /// <summary>Gets the number of single-item repository fetches issued since the last <see cref="ResetColdAsync"/>.</summary>
    public int SingleFetchCount => _singleFetchCount;

    /// <summary>Gets the number of batched repository fetches issued since the last <see cref="ResetColdAsync"/>.</summary>
    public int BatchFetchCount => _batchFetchCount;

    public async Task InitialiseAsync(int branchCount, int leafCount, int propertyCount = 10, bool seed = true, int repoLatencyMs = 0)
    {
        IPublishedModelFactory publishedModelFactory = new NoopPublishedModelFactory();
        IVariationContextAccessor variationContextAccessor = new ThreadCultureVariationContextAccessor();
        IPropertyRenderingContextAccessor propertyRenderingContextAccessor = Mock.Of<IPropertyRenderingContextAccessor>();
        IElementsCache elementsCache = new ElementsDictionaryAppCache();
        var converters = new PropertyValueConverterCollection(() => Enumerable.Empty<IPropertyValueConverter>());

        IPublishedContentType contentType = BuildTestContentType(converters, publishedModelFactory, propertyCount);
        Guid contentTypeKey = contentType.Key;

        DocumentNavigationService navigationService = BuildNavigationService();
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

        // HybridCache requires a service provider for registration; everything else is wired by hand.
        // Logging must be registered because HybridCacheSerializer takes an ILogger<T> dependency.
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

        // Always return our single test content type.
        var contentTypeCacheMock = new Mock<IPublishedContentTypeCache>();
        contentTypeCacheMock.Setup(x => x.Get(PublishedItemType.Content, It.IsAny<int>())).Returns(contentType);
        contentTypeCacheMock.Setup(x => x.Get(PublishedItemType.Content, It.IsAny<Guid>())).Returns(contentType);
        contentTypeCacheMock.Setup(x => x.Get(PublishedItemType.Content, It.IsAny<string>())).Returns(contentType);
        IPublishedContentTypeCache contentTypeCache = contentTypeCacheMock.Object;

        IPublishedContentFactory publishedContentFactory = new PublishedContentFactory(
            elementsCache,
            variationContextAccessor,
            propertyRenderingContextAccessor,
            contentTypeCache);

        var publishStatusMock = new Mock<IPublishStatusQueryService>();
        publishStatusMock.Setup(x => x.IsDocumentPublished(It.IsAny<Guid>(), It.IsAny<string>())).Returns(true);
        publishStatusMock.Setup(x => x.HasPublishedAncestorPath(It.IsAny<Guid>(), It.IsAny<string>())).Returns(true);
        publishStatusMock.Setup(x => x.HasPublishedAncestorPath(It.IsAny<Guid>())).Returns(true);

        // Repository: when seeded, never called (everything is pre-seeded into HybridCache below).
        // When NOT seeded, it serves nodes from the in-memory map, counting round trips and applying
        // an optional per-call latency so the benchmark can model database round-trip cost — the single
        // vs batched read count is what distinguishes the cold per-key path from the batched one.
        var repoMock = new Mock<IDatabaseCacheRepository>();
        repoMock.Setup(r => r.GetContentSourceAsync(It.IsAny<Guid>(), It.IsAny<bool>()))
            .Returns(async (Guid key, bool _) =>
            {
                Interlocked.Increment(ref _singleFetchCount);
                if (repoLatencyMs > 0)
                {
                    await Task.Delay(repoLatencyMs);
                }

                return _nodesByKey.GetValueOrDefault(key);
            });
        repoMock.Setup(r => r.GetContentSourcesAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<bool>()))
            .Returns(async (IEnumerable<Guid> keys, bool _) =>
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

        var previewMock = new Mock<IPreviewService>();
        previewMock.Setup(x => x.IsInPreview()).Returns(false);

        var idKeyMapMock = new Mock<IIdKeyMap>();
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

        var cacheService = new DocumentCacheService(
            repoMock.Object,
            idKeyMapMock.Object,
            scopeProviderMock.Object,
            hybridCache,
            publishedContentFactory,
            Mock.Of<ICacheNodeFactory>(),
            Enumerable.Empty<IDocumentSeedKeyProvider>(),
            Options.Create(new CacheSettings()),
            publishedModelFactory,
            previewMock.Object,
            publishStatusMock.Object,
            NullLogger<DocumentCacheService>.Instance,
            new ConvertedPublishedContentCacheFactory(null, NullLogger<ConvertedPublishedContentCacheFactory>.Instance));

        _hybridCache = hybridCache;
        _cacheService = cacheService;

        // Build every node up front so the repository stub can serve them in the cold (unseeded) mode.
        foreach (Guid key in _allKeys)
        {
            _nodesByKey[key] = BuildContentCacheNode(key, contentType.Id, propertyCount);
        }

        // When seeding, prime HybridCache (L1) directly so reads stay in-memory and never reach the
        // repository. When cold, leave L1 empty so reads fall through to the (batched) repository.
        if (seed)
        {
            foreach (Guid key in _allKeys)
            {
                await hybridCache.SetAsync(key.ToString(), _nodesByKey[key]);
            }
        }

        var documentCache = new DocumentCache(
            cacheService,
            contentTypeCache,
            navigationService,
            Mock.Of<IDocumentUrlService>(),
            new Lazy<IPublishedUrlProvider>(() => Mock.Of<IPublishedUrlProvider>()));

        PublishedContentCache = documentCache;
        NavigationQueryService = navigationService;

        StatusFilteringService = new PublishedContentStatusFilteringService(
            variationContextAccessor,
            publishStatusMock.Object,
            previewMock.Object,
            documentCache,
            cacheService);

        Root = (await cacheService.GetByKeyAsync(RootKey, false))!;
    }

    /// <summary>
    /// Evicts the tree from the converted (L0) and HybridCache (L1) tiers and resets the fetch counters,
    /// so the next traversal is measured cold. Intended for a benchmark <c>[IterationSetup]</c>.
    /// </summary>
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

    private static PublishedContentType BuildTestContentType(
        PropertyValueConverterCollection converters,
        IPublishedModelFactory modelFactory,
        int propertyCount)
    {
        var jsonSerializer = new SystemTextConfigurationEditorJsonSerializer(new DefaultJsonSerializerEncoderFactory());
        var dataType = new DataType(new VoidEditor(Mock.Of<IDataValueEditorFactory>()), jsonSerializer) { Id = 1 };
        var dataTypeServiceMock = new Mock<IDataTypeService>();

        // PublishedContentTypeFactory.GetDataType calls the synchronous GetAll() overload (the obsolete
        // params int[] one), so we must set up that one rather than the new GetAllAsync.
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
            "benchPage",
            PublishedItemType.Content,
            Enumerable.Empty<string>(),
            CreatePropertyTypes,
            ContentVariation.Nothing,
            isElement: false);
    }

    private static DocumentNavigationService BuildNavigationService()
        => new(
            Mock.Of<ICoreScopeProvider>(),
            Mock.Of<INavigationRepository>(),
            Mock.Of<IContentTypeService>());

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
            name: $"Node-{key.ToString()[..8]}",
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
