using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Preview;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;
using Umbraco.Cms.Infrastructure.HybridCache.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class ElementHybridCacheMockTests : UmbracoIntegrationTest
{
    private IPublishedElementCache _mockedCache;
    private Mock<IDatabaseCacheRepository> _mockDatabaseCacheRepository;
    private IElementCacheService _elementCacheService;
    private Element _element;

    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder.AddUmbracoHybridCache();

    [SetUp]
    public async Task SetUp()
    {
        var elementType = ContentTypeBuilder.CreateSimpleElementType();
        await GetRequiredService<IContentTypeService>().CreateAsync(elementType, Constants.Security.SuperUserKey);

        _element = ElementBuilder.CreateSimpleElement(elementType);
        GetRequiredService<IElementService>().Save(_element);

        ContentCacheNode draftTestCacheNode = BuildCacheNode(isDraft: true);
        ContentCacheNode publishedTestCacheNode = BuildCacheNode(isDraft: false);

        _mockDatabaseCacheRepository = new Mock<IDatabaseCacheRepository>();

        _mockDatabaseCacheRepository.Setup(r => r.GetElementSourceAsync(It.IsAny<Guid>(), true))
            .ReturnsAsync(draftTestCacheNode);
        _mockDatabaseCacheRepository.Setup(r => r.GetElementSourceAsync(It.IsAny<Guid>(), false))
            .ReturnsAsync(publishedTestCacheNode);

        _mockDatabaseCacheRepository.Setup(r => r.GetElementSource(It.IsAny<Guid>(), true))
            .Returns(draftTestCacheNode);
        _mockDatabaseCacheRepository.Setup(r => r.GetElementSource(It.IsAny<Guid>(), false))
            .Returns(publishedTestCacheNode);

        _elementCacheService = CreateCacheService(canPreview: true);
        _mockedCache = new ElementCache(_elementCacheService);
    }

    [Test]
    public async Task Synchronous_Lookup_By_Key_Reads_The_Database_Synchronously()
    {
        var hybridCache = GetRequiredService<Microsoft.Extensions.Caching.Hybrid.HybridCache>();
        await hybridCache.RemoveAsync($"{_element.Key}+draft");

        IPublishedElement? element = _mockedCache.GetById(true, _element.Key);

        Assert.AreEqual(_element.Key, element?.Key);
        _mockDatabaseCacheRepository.Verify(x => x.GetElementSource(_element.Key, true), Times.Once);
        _mockDatabaseCacheRepository.Verify(x => x.GetElementSourceAsync(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Never);
    }

    // The synchronous member blocks on this task, so with no distributed cache to wait for it must have completed by
    // the time it is returned. A task that has not means some step of the shared read logic awaited asynchronous
    // work, and synchronous callers are blocking on it again.
    [TestCase(true)]
    [TestCase(false)]
    public async Task Synchronous_Read_Logic_Completes_Synchronously_On_A_Cache_Miss(bool preview)
    {
        var hybridCache = GetRequiredService<Microsoft.Extensions.Caching.Hybrid.HybridCache>();
        await hybridCache.RemoveAsync($"{_element.Key}+draft");
        await hybridCache.RemoveAsync($"{_element.Key}");

        Task<IPublishedElement?> task = ((ElementCacheService)_elementCacheService).GetByKeyWithSynchronousReadsAsync(_element.Key, preview);

        Assert.IsTrue(task.IsCompleted);
        Assert.AreEqual(_element.Key, (await task)?.Key);
    }

    [Test]
    public async Task Synchronous_Read_Logic_Completes_Synchronously_When_Rereading_A_Refused_Draft()
    {
        ElementCacheService refusingCacheService = CreateCacheService(canPreview: false);

        var hybridCache = GetRequiredService<Microsoft.Extensions.Caching.Hybrid.HybridCache>();
        await hybridCache.RemoveAsync($"{_element.Key}+draft");
        await hybridCache.RemoveAsync($"{_element.Key}");

        Task<IPublishedElement?> task = refusingCacheService.GetByKeyWithSynchronousReadsAsync(_element.Key, true);

        Assert.IsTrue(task.IsCompleted);
        Assert.AreEqual(_element.Key, (await task)?.Key);
        _mockDatabaseCacheRepository.Verify(x => x.GetElementSource(_element.Key, false), Times.Once);
    }

    private ElementCacheService CreateCacheService(bool canPreview)
        => new(
            _mockDatabaseCacheRepository.Object,
            GetRequiredService<ICoreScopeProvider>(),
            GetRequiredService<Microsoft.Extensions.Caching.Hybrid.HybridCache>(),
            GetRequiredService<IPublishedContentFactory>(),
            GetRequiredService<ICacheNodeFactory>(),
            Array.Empty<IElementSeedKeyProvider>(),
            GetRequiredService<IPublishedModelFactory>(),
            GetRequiredService<IPreviewSessionService>(),
            new OptionsWrapper<CacheSettings>(new CacheSettings()),
            new NullLogger<ElementCacheService>(),
            new ConvertedPublishedContentCacheFactory(null, new NullLogger<ConvertedPublishedContentCacheFactory>()),
            Mock.Of<IPreviewAccessEvaluator>(evaluator => evaluator.CanPreviewElement(It.IsAny<IPublishedElement>()) == canPreview));

    private ContentCacheNode BuildCacheNode(bool isDraft)
        => new()
        {
            ContentTypeId = _element.ContentTypeId,
            CreatorId = _element.CreatorId,
            CreateDate = _element.CreateDate,
            Id = _element.Id,
            Key = _element.Key,
            SortOrder = 0,
            Data = new ContentData(
                _element.Name,
                null,
                1,
                _element.UpdateDate,
                _element.CreatorId,
                -1,
                isDraft is false,
                new Dictionary<string, PropertyData[]>(),
                null),
            IsDraft = isDraft,
        };
}
