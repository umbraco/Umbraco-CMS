using System.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class DocumentUrlServiceTests
{
    #region ConvertToCacheModel Tests

    [Test]
    public void ConvertToCacheModel_Converts_Single_Document_With_Single_Segment_To_Expected_Cache_Model()
    {
        var documentKey = Guid.NewGuid();
        var segments = new List<PublishedDocumentUrlSegment>
        {
            new()
            {
                DocumentKey = documentKey,
                IsDraft = false,
                IsPrimary = true,
                LanguageId = 1,
                UrlSegment = "test-segment",
            },
        };
        var cacheModels = DocumentUrlService.ConvertToCacheModel(segments).ToList();

        Assert.AreEqual(1, cacheModels.Count);
        Assert.AreEqual(documentKey, cacheModels[0].Key.DocumentKey);
        Assert.AreEqual(1, cacheModels[0].Key.LanguageId);
        Assert.IsFalse(cacheModels[0].Key.IsDraft);
        Assert.AreEqual("test-segment", cacheModels[0].Cache.PrimarySegment);
        Assert.IsNull(cacheModels[0].Cache.AlternateSegments);
    }

    [Test]
    public void ConvertToCacheModel_Converts_Multiple_Documents_With_Single_Segment_To_Expected_Cache_Model()
    {
        var documentKey1 = Guid.NewGuid();
        var documentKey2 = Guid.NewGuid();
        var segments = new List<PublishedDocumentUrlSegment>
        {
            new()
            {
                DocumentKey = documentKey1,
                IsDraft = false,
                IsPrimary = true,
                LanguageId = 1,
                UrlSegment = "test-segment",
            },
            new()
            {
                DocumentKey = documentKey2,
                IsDraft = false,
                IsPrimary = true,
                LanguageId = 1,
                UrlSegment = "test-segment-2",
            },
        };
        var cacheModels = DocumentUrlService.ConvertToCacheModel(segments).ToList();

        Assert.AreEqual(2, cacheModels.Count);

        var model1 = cacheModels.First(m => m.Key.DocumentKey == documentKey1);
        var model2 = cacheModels.First(m => m.Key.DocumentKey == documentKey2);

        Assert.AreEqual(1, model1.Key.LanguageId);
        Assert.AreEqual(1, model2.Key.LanguageId);
        Assert.AreEqual("test-segment", model1.Cache.PrimarySegment);
        Assert.AreEqual("test-segment-2", model2.Cache.PrimarySegment);
        Assert.IsNull(model1.Cache.AlternateSegments);
        Assert.IsNull(model2.Cache.AlternateSegments);
    }

    [Test]
    public void ConvertToCacheModel_Converts_Single_Document_With_Multiple_Segments_To_Expected_Cache_Model()
    {
        var documentKey = Guid.NewGuid();
        var segments = new List<PublishedDocumentUrlSegment>
        {
            new()
            {
                DocumentKey = documentKey,
                IsDraft = false,
                IsPrimary = true,
                LanguageId = 1,
                UrlSegment = "test-segment",
            },
            new()
            {
                DocumentKey = documentKey,
                IsDraft = false,
                IsPrimary = false,
                LanguageId = 1,
                UrlSegment = "test-segment-2",
            },
        };
        var cacheModels = DocumentUrlService.ConvertToCacheModel(segments).ToList();

        Assert.AreEqual(1, cacheModels.Count);
        Assert.AreEqual(documentKey, cacheModels[0].Key.DocumentKey);
        Assert.AreEqual(1, cacheModels[0].Key.LanguageId);
        Assert.AreEqual("test-segment", cacheModels[0].Cache.PrimarySegment);
        Assert.IsNotNull(cacheModels[0].Cache.AlternateSegments);
        Assert.AreEqual(1, cacheModels[0].Cache.AlternateSegments!.Length);
        Assert.AreEqual("test-segment-2", cacheModels[0].Cache.AlternateSegments[0]);
    }

    [Test]
    public void ConvertToCacheModel_Performance_Test()
    {
        const int NumberOfSegments = 1;
        var segments = Enumerable.Range(0, NumberOfSegments)
            .Select((x, i) => new PublishedDocumentUrlSegment
            {
                DocumentKey = Guid.NewGuid(),
                IsDraft = false,
                IsPrimary = true,
                LanguageId = 1,
                UrlSegment = $"test-segment-{x + 1}",
            });
        var cacheModels = DocumentUrlService.ConvertToCacheModel(segments).ToList();

        Assert.AreEqual(NumberOfSegments, cacheModels.Count);

        // Benchmarking (for NumberOfSegments = 50000):
        //  - Initial implementation (15.4): ~28s
        //  - Previous implementation (versions 15.5 through 17.0, optimized algorithm): ~75ms
        //  - Current implementation (17.1+, refactored for memory optimization, same performance as previous): ~75ms
    }

    [Test]
    public void ConvertToCacheModel_Handles_Null_LanguageId_For_Invariant_Content()
    {
        var documentKey = Guid.NewGuid();
        var segments = new List<PublishedDocumentUrlSegment>
        {
            new()
            {
                DocumentKey = documentKey,
                IsDraft = false,
                IsPrimary = true,
                LanguageId = null, // Invariant content uses NULL
                UrlSegment = "test-segment",
            },
        };
        var cacheModels = DocumentUrlService.ConvertToCacheModel(segments).ToList();

        Assert.AreEqual(1, cacheModels.Count);
        Assert.AreEqual(documentKey, cacheModels[0].Key.DocumentKey);
        Assert.IsNull(cacheModels[0].Key.LanguageId, "Invariant content should have NULL LanguageId");
        Assert.IsFalse(cacheModels[0].Key.IsDraft);
        Assert.AreEqual("test-segment", cacheModels[0].Cache.PrimarySegment);
        Assert.IsNull(cacheModels[0].Cache.AlternateSegments);
    }

    [Test]
    public void ConvertToCacheModel_Handles_Mixed_Invariant_And_Variant_Content()
    {
        var invariantDocKey = Guid.NewGuid();
        var variantDocKey = Guid.NewGuid();
        var segments = new List<PublishedDocumentUrlSegment>
        {
            new()
            {
                DocumentKey = invariantDocKey,
                IsDraft = false,
                IsPrimary = true,
                LanguageId = null, // Invariant content
                UrlSegment = "invariant-segment",
            },
            new()
            {
                DocumentKey = variantDocKey,
                IsDraft = false,
                IsPrimary = true,
                LanguageId = 1, // Variant content
                UrlSegment = "variant-segment",
            },
        };
        var cacheModels = DocumentUrlService.ConvertToCacheModel(segments).ToList();

        Assert.AreEqual(2, cacheModels.Count);

        var invariantModel = cacheModels.First(m => m.Key.DocumentKey == invariantDocKey);
        var variantModel = cacheModels.First(m => m.Key.DocumentKey == variantDocKey);

        Assert.IsNull(invariantModel.Key.LanguageId, "Invariant content should have NULL LanguageId");
        Assert.AreEqual(1, variantModel.Key.LanguageId, "Variant content should have specific LanguageId");
        Assert.AreEqual("invariant-segment", invariantModel.Cache.PrimarySegment);
        Assert.AreEqual("variant-segment", variantModel.Cache.PrimarySegment);
    }

    #endregion

    #region CreateOrUpdateUrlSegmentsAsync Tests

    /// <summary>
    /// Creates a DocumentUrlService for testing with mocked dependencies.
    /// </summary>
    private static (DocumentUrlService Service, Mock<IDocumentUrlRepository> Repository) CreateDocumentUrlServiceWithMocks(
        UrlSegmentProviderCollection urlSegmentProviderCollection,
        IEnumerable<ILanguage> languages)
    {
        var loggerMock = Mock.Of<ILogger<DocumentUrlService>>();
        var documentUrlRepositoryMock = new Mock<IDocumentUrlRepository>();
        var documentRepositoryMock = Mock.Of<IDocumentRepository>();
        var globalSettingsMock = Options.Create(new GlobalSettings());
        var webRoutingSettingsMock = Options.Create(new WebRoutingSettings());
        var contentServiceMock = Mock.Of<IContentService>();

        var languageServiceMock = new Mock<ILanguageService>();
        languageServiceMock.Setup(x => x.GetAllAsync()).ReturnsAsync(languages);

        var keyValueServiceMock = Mock.Of<IKeyValueService>();
        var idKeyMapMock = Mock.Of<IIdKeyMap>();
        var documentNavigationQueryServiceMock = Mock.Of<IDocumentNavigationQueryService>();
        var publishStatusQueryServiceMock = Mock.Of<IPublishStatusQueryService>();
        var domainCacheServiceMock = Mock.Of<IDomainCacheService>();
        var defaultCultureAccessorMock = Mock.Of<IDefaultCultureAccessor>();

        var scopeContextMock = new Mock<IScopeContext>();
        var coreScopeMock = new Mock<ICoreScope>();
        coreScopeMock.Setup(x => x.Complete());

        var coreScopeProviderMock = new Mock<ICoreScopeProvider>();
        coreScopeProviderMock.Setup(x => x.CreateCoreScope(
                It.IsAny<IsolationLevel>(),
                It.IsAny<RepositoryCacheMode>(),
                It.IsAny<IEventDispatcher?>(),
                It.IsAny<IScopedNotificationPublisher?>(),
                It.IsAny<bool?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns(coreScopeMock.Object);
        coreScopeProviderMock.Setup(x => x.Context).Returns(scopeContextMock.Object);

        var service = new DocumentUrlService(
            loggerMock,
            documentUrlRepositoryMock.Object,
            documentRepositoryMock,
            coreScopeProviderMock.Object,
            globalSettingsMock,
            webRoutingSettingsMock,
            urlSegmentProviderCollection,
            contentServiceMock,
            new DefaultShortStringHelper(new DefaultShortStringHelperConfig()),
            languageServiceMock.Object,
            keyValueServiceMock,
            idKeyMapMock,
            documentNavigationQueryServiceMock,
            publishStatusQueryServiceMock,
            domainCacheServiceMock,
            defaultCultureAccessorMock);

        return (service, documentUrlRepositoryMock);
    }

    /// <summary>
    /// Creates a mock IContent with the specified configuration.
    /// </summary>
    private static Mock<IContent> CreateMockContent(Guid key, bool variesByCulture, bool isPublished, bool isTrashed = false)
    {
        // IContent.ContentType returns ISimpleContentType, and VariesByCulture() is an extension method
        // that checks the Variations property, so we need to mock Variations correctly.
        var contentTypeMock = new Mock<ISimpleContentType>();
        contentTypeMock.Setup(x => x.Variations).Returns(variesByCulture ? ContentVariation.Culture : ContentVariation.Nothing);

        var contentMock = new Mock<IContent>();
        contentMock.Setup(x => x.Key).Returns(key);
        contentMock.Setup(x => x.ContentType).Returns(contentTypeMock.Object);
        contentMock.Setup(x => x.Published).Returns(isPublished);
        contentMock.Setup(x => x.Trashed).Returns(isTrashed);
        contentMock.Setup(x => x.Name).Returns("Test Content");

        return contentMock;
    }

    /// <summary>
    /// Creates a mock ILanguage.
    /// </summary>
    private static ILanguage CreateMockLanguage(int id, string isoCode)
    {
        var languageMock = new Mock<ILanguage>();
        languageMock.Setup(x => x.Id).Returns(id);
        languageMock.Setup(x => x.IsoCode).Returns(isoCode);
        return languageMock.Object;
    }

    /// <summary>
    /// Creates a URL segment provider that returns a fixed segment for all cultures.
    /// </summary>
    private static IUrlSegmentProvider CreateFixedSegmentProvider(string segment)
    {
        var providerMock = new Mock<IUrlSegmentProvider>();
        providerMock.Setup(x => x.GetUrlSegment(It.IsAny<IContentBase>(), It.IsAny<bool>(), It.IsAny<string?>()))
            .Returns(segment);
        providerMock.Setup(x => x.AllowAdditionalSegments).Returns(false);
        return providerMock.Object;
    }

    /// <summary>
    /// Creates a URL segment provider that returns different segments based on culture.
    /// </summary>
    private static IUrlSegmentProvider CreateCultureSpecificSegmentProvider(Dictionary<string?, string> cultureToSegment)
    {
        var providerMock = new Mock<IUrlSegmentProvider>();
        providerMock.Setup(x => x.GetUrlSegment(It.IsAny<IContentBase>(), It.IsAny<bool>(), It.IsAny<string?>()))
            .Returns<IContentBase, bool, string?>((_, _, culture) =>
                cultureToSegment.TryGetValue(culture, out var segment) ? segment : "default-segment");
        providerMock.Setup(x => x.AllowAdditionalSegments).Returns(false);
        return providerMock.Object;
    }

    [Test]
    public async Task CreateOrUpdateUrlSegmentsAsync_VariantContent_Stores_PerLanguage()
    {
        // Arrange
        var languages = new List<ILanguage>
        {
            CreateMockLanguage(1, "en-US"),
            CreateMockLanguage(2, "fr-FR"),
        };

        var urlSegmentProvider = CreateFixedSegmentProvider("test-segment");
        var urlSegmentProviderCollection = new UrlSegmentProviderCollection(() => [urlSegmentProvider]);

        var (service, repositoryMock) = CreateDocumentUrlServiceWithMocks(urlSegmentProviderCollection, languages);

        var documentKey = Guid.NewGuid();
        var contentMock = CreateMockContent(documentKey, variesByCulture: true, isPublished: true);

        // Set up variant content - needs culture-specific published info
        var publishCultureInfos = new ContentCultureInfosCollection();
        publishCultureInfos.AddOrUpdate("en-US", "English Name", DateTime.Now);
        publishCultureInfos.AddOrUpdate("fr-FR", "French Name", DateTime.Now);
        contentMock.Setup(x => x.PublishCultureInfos).Returns(publishCultureInfos);

        List<PublishedDocumentUrlSegment>? savedSegments = null;
        repositoryMock.Setup(x => x.Save(It.IsAny<IEnumerable<PublishedDocumentUrlSegment>>()))
            .Callback<IEnumerable<PublishedDocumentUrlSegment>>(segments => savedSegments = segments.ToList());

        // Act
        await service.CreateOrUpdateUrlSegmentsAsync([contentMock.Object]);

        // Assert
        repositoryMock.Verify(x => x.Save(It.IsAny<IEnumerable<PublishedDocumentUrlSegment>>()), Times.Once);

        Assert.That(savedSegments, Is.Not.Null);
        Assert.That(savedSegments, Has.Count.GreaterThan(0), "Should have saved URL segments");

        // Verify all saved segments have specific language IDs (not NULL)
        Assert.That(
            savedSegments!.All(s => s.LanguageId.HasValue),
            Is.True,
            "Variant content should have specific LanguageId values (not NULL)");

        // Should have segments for both languages
        var languageIds = savedSegments.Select(s => s.LanguageId).Distinct().ToList();
        Assert.That(languageIds, Does.Contain(1), "Should have segments for en-US (language ID 1)");
        Assert.That(languageIds, Does.Contain(2), "Should have segments for fr-FR (language ID 2)");
    }

    [Test]
    public async Task CreateOrUpdateUrlSegmentsAsync_InvariantContent_WithIdenticalSegments_Stores_NullLanguageId()
    {
        // Arrange
        var languages = new List<ILanguage>
        {
            CreateMockLanguage(1, "en-US"),
            CreateMockLanguage(2, "fr-FR"),
        };

        // Provider returns same segment for all cultures
        var urlSegmentProvider = CreateFixedSegmentProvider("same-segment");
        var urlSegmentProviderCollection = new UrlSegmentProviderCollection(() => [urlSegmentProvider]);

        var (service, repositoryMock) = CreateDocumentUrlServiceWithMocks(urlSegmentProviderCollection, languages);

        var documentKey = Guid.NewGuid();
        var contentMock = CreateMockContent(documentKey, variesByCulture: false, isPublished: true);

        List<PublishedDocumentUrlSegment>? savedSegments = null;
        repositoryMock.Setup(x => x.Save(It.IsAny<IEnumerable<PublishedDocumentUrlSegment>>()))
            .Callback<IEnumerable<PublishedDocumentUrlSegment>>(segments => savedSegments = segments.ToList());

        // Act
        await service.CreateOrUpdateUrlSegmentsAsync([contentMock.Object]);

        // Assert
        repositoryMock.Verify(x => x.Save(It.IsAny<IEnumerable<PublishedDocumentUrlSegment>>()), Times.Once);

        Assert.That(savedSegments, Is.Not.Null);
        Assert.That(savedSegments, Has.Count.GreaterThan(0), "Should have saved URL segments");

        // Verify all saved segments have NULL language ID
        Assert.That(
            savedSegments!.All(s => s.LanguageId is null),
            Is.True,
            "Invariant content with identical segments should have NULL LanguageId");
    }

    [Test]
    public async Task CreateOrUpdateUrlSegmentsAsync_InvariantContent_WithDifferentSegments_Stores_PerLanguage()
    {
        // Arrange
        var languages = new List<ILanguage>
        {
            CreateMockLanguage(1, "en-US"),
            CreateMockLanguage(2, "fr-FR"),
        };

        // Provider returns different segments for different cultures
        var cultureToSegment = new Dictionary<string?, string>
        {
            { "en-US", "english-segment" },
            { "fr-FR", "french-segment" },
        };
        var urlSegmentProvider = CreateCultureSpecificSegmentProvider(cultureToSegment);
        var urlSegmentProviderCollection = new UrlSegmentProviderCollection(() => [urlSegmentProvider]);

        var (service, repositoryMock) = CreateDocumentUrlServiceWithMocks(urlSegmentProviderCollection, languages);

        var documentKey = Guid.NewGuid();
        var contentMock = CreateMockContent(documentKey, variesByCulture: false, isPublished: true);

        List<PublishedDocumentUrlSegment>? savedSegments = null;
        repositoryMock.Setup(x => x.Save(It.IsAny<IEnumerable<PublishedDocumentUrlSegment>>()))
            .Callback<IEnumerable<PublishedDocumentUrlSegment>>(segments => savedSegments = segments.ToList());

        // Act
        await service.CreateOrUpdateUrlSegmentsAsync([contentMock.Object]);

        // Assert
        repositoryMock.Verify(x => x.Save(It.IsAny<IEnumerable<PublishedDocumentUrlSegment>>()), Times.Once);

        Assert.That(savedSegments, Is.Not.Null);
        Assert.That(savedSegments, Has.Count.GreaterThan(0), "Should have saved URL segments");

        // Verify all saved segments have specific language IDs (not NULL)
        Assert.That(
            savedSegments!.All(s => s.LanguageId.HasValue),
            Is.True,
            "Invariant content with different segments should have specific LanguageId values (not NULL)");

        // Should have segments for both languages
        var languageIds = savedSegments.Select(s => s.LanguageId).Distinct().ToList();
        Assert.That(languageIds, Does.Contain(1), "Should have segments for en-US (language ID 1)");
        Assert.That(languageIds, Does.Contain(2), "Should have segments for fr-FR (language ID 2)");
    }

    [Test]
    public async Task CreateOrUpdateUrlSegmentsAsync_InvariantContent_WithSingleLanguage_Stores_NullLanguageId()
    {
        // Arrange - only one language
        var languages = new List<ILanguage>
        {
            CreateMockLanguage(1, "en-US"),
        };

        var urlSegmentProvider = CreateFixedSegmentProvider("test-segment");
        var urlSegmentProviderCollection = new UrlSegmentProviderCollection(() => [urlSegmentProvider]);

        var (service, repositoryMock) = CreateDocumentUrlServiceWithMocks(urlSegmentProviderCollection, languages);

        var documentKey = Guid.NewGuid();
        var contentMock = CreateMockContent(documentKey, variesByCulture: false, isPublished: true);

        List<PublishedDocumentUrlSegment>? savedSegments = null;
        repositoryMock.Setup(x => x.Save(It.IsAny<IEnumerable<PublishedDocumentUrlSegment>>()))
            .Callback<IEnumerable<PublishedDocumentUrlSegment>>(segments => savedSegments = segments.ToList());

        // Act
        await service.CreateOrUpdateUrlSegmentsAsync([contentMock.Object]);

        // Assert
        repositoryMock.Verify(x => x.Save(It.IsAny<IEnumerable<PublishedDocumentUrlSegment>>()), Times.Once);

        Assert.That(savedSegments, Is.Not.Null);
        Assert.That(savedSegments, Has.Count.GreaterThan(0), "Should have saved URL segments");

        // Verify all saved segments have NULL language ID (single language optimization)
        Assert.That(
            savedSegments!.All(s => s.LanguageId is null),
            Is.True,
            "Invariant content with single language should have NULL LanguageId");
    }

    [Test]
    public async Task CreateOrUpdateUrlSegmentsAsync_EmptyDocuments_DoesNotCallRepository()
    {
        // Arrange
        var languages = new List<ILanguage> { CreateMockLanguage(1, "en-US") };

        var urlSegmentProvider = CreateFixedSegmentProvider("test-segment");
        var urlSegmentProviderCollection = new UrlSegmentProviderCollection(() => [urlSegmentProvider]);

        var (service, repositoryMock) = CreateDocumentUrlServiceWithMocks(urlSegmentProviderCollection, languages);

        // Act
        await service.CreateOrUpdateUrlSegmentsAsync(Array.Empty<IContent>());

        // Assert
        repositoryMock.Verify(x => x.Save(It.IsAny<IEnumerable<PublishedDocumentUrlSegment>>()), Times.Never);
    }

    [Test]
    public async Task CreateOrUpdateUrlSegmentsAsync_TrashedInvariantContent_DoesNotStoreSegments()
    {
        // Arrange
        var languages = new List<ILanguage>
        {
            CreateMockLanguage(1, "en-US"),
            CreateMockLanguage(2, "fr-FR"),
        };

        var urlSegmentProvider = CreateFixedSegmentProvider("test-segment");
        var urlSegmentProviderCollection = new UrlSegmentProviderCollection(() => [urlSegmentProvider]);

        var (service, repositoryMock) = CreateDocumentUrlServiceWithMocks(urlSegmentProviderCollection, languages);

        var documentKey = Guid.NewGuid();
        var contentMock = CreateMockContent(documentKey, variesByCulture: false, isPublished: false, isTrashed: true);

        List<PublishedDocumentUrlSegment>? savedSegments = null;
        repositoryMock.Setup(x => x.Save(It.IsAny<IEnumerable<PublishedDocumentUrlSegment>>()))
            .Callback<IEnumerable<PublishedDocumentUrlSegment>>(segments => savedSegments = segments.ToList());

        // Act
        await service.CreateOrUpdateUrlSegmentsAsync([contentMock.Object]);

        // Assert - trashed content should not have any segments saved (cache is cleared instead)
        // The repository.Save is still called but with no segments that should be cached
        repositoryMock.Verify(x => x.Save(It.IsAny<IEnumerable<PublishedDocumentUrlSegment>>()), Times.Never);
    }

    #endregion
}
