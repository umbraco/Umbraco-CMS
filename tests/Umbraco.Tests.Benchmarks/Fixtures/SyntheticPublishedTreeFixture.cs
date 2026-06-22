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

    public IDocumentNavigationQueryService NavigationQueryService { get; private set; } = null!;

    public IPublishedContentStatusFilteringService StatusFilteringService { get; private set; } = null!;

    public IPublishedContentCache PublishedContentCache { get; private set; } = null!;

    public IPublishedContent Root { get; private set; } = null!;

    public IPublishedValueFallback PublishedValueFallback { get; } = new NoopPublishedValueFallback();

    public IReadOnlyList<Guid> AllKeys => _allKeys;

    public Guid RootKey { get; private set; }

    public async Task InitialiseAsync(int branchCount, int leafCount, int propertyCount = 10)
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

        var publishStatusMock = new Mock<IDocumentPublishStatusQueryService>();
        publishStatusMock.Setup(x => x.IsPublished(It.IsAny<Guid>(), It.IsAny<string>())).Returns(true);
        publishStatusMock.Setup(x => x.HasPublishedAncestorPath(It.IsAny<Guid>(), It.IsAny<string>())).Returns(true);
        publishStatusMock.Setup(x => x.HasPublishedAncestorPath(It.IsAny<Guid>())).Returns(true);

        // Repository: never called because we pre-seed the cache, but provide a safe stub.
        var repoMock = new Mock<IDatabaseCacheRepository>();
        repoMock.Setup(r => r.GetDocumentSourceAsync(It.IsAny<Guid>(), It.IsAny<bool>()))
            .ReturnsAsync((ContentCacheNode?)null);

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
            NullLogger<DocumentCacheService>.Instance);

        // Seed every node directly so reads stay in-memory and never reach the repository stub.
        foreach (Guid key in _allKeys)
        {
            ContentCacheNode node = BuildContentCacheNode(key, contentType.Id, propertyCount);
            await hybridCache.SetAsync(key.ToString(), node);
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
            documentCache);

        Root = (await cacheService.GetByKeyAsync(RootKey, false))!;
    }

    private static IPublishedContentType BuildTestContentType(
        PropertyValueConverterCollection converters,
        IPublishedModelFactory modelFactory,
        int propertyCount)
    {
        var jsonSerializer = new SystemTextConfigurationEditorJsonSerializer(new DefaultJsonSerializerEncoderFactory());
        var dataType = new DataType(new VoidEditor(Mock.Of<IDataValueEditorFactory>()), jsonSerializer) { Id = 1 };
        var dataTypeServiceMock = new Mock<IDataTypeService>();

        dataTypeServiceMock.Setup(x => x.GetAllAsync(It.IsAny<Guid[]>())).ReturnsAsync(new[] { dataType });

        var factory = new PublishedContentTypeFactory(modelFactory, converters, dataTypeServiceMock.Object, Mock.Of<IIdKeyMap>());

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
