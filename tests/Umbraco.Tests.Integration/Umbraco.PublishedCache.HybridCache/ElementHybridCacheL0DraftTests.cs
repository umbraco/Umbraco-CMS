using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
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

/// <summary>
/// Mirrors <see cref="DocumentHybridCacheL0DraftTests"/> for <see cref="ElementCacheService"/>: only published
/// elements are stored in the in-process (L0) converted cache. A draft (preview) request is never served back
/// from L0 (the read fast path is guarded by preview is false), so storing it there would only waste capacity
/// and leave a stale, non-invalidatable entry.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class ElementHybridCacheL0DraftTests : UmbracoIntegrationTest
{
    private Mock<IDatabaseCacheRepository> _databaseCacheRepository;
    private ElementCacheService _elementCacheService;
    private Element _element;

    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder.AddUmbracoHybridCache();

    private ContentCacheNode BuildNode(bool isDraft)
    {
        var data = new ContentData(
            _element.Name,
            null,
            1,
            _element.UpdateDate,
            _element.CreatorId,
            -1,
            isDraft is false,
            new Dictionary<string, PropertyData[]>
            {
                ["title"] = [new PropertyData { Culture = string.Empty, Segment = string.Empty, Value = "A title" }],
            },
            null);

        return new ContentCacheNode
        {
            ContentTypeId = _element.ContentTypeId,
            CreatorId = _element.CreatorId,
            CreateDate = _element.CreateDate,
            Id = _element.Id,
            Key = _element.Key,
            SortOrder = 0,
            Data = data,
            IsDraft = isDraft,
        };
    }

    [SetUp]
    public async Task SetUp()
    {
        var elementType = ContentTypeBuilder.CreateSimpleElementType();
        await GetRequiredService<IContentTypeService>().CreateAsync(elementType, Constants.Security.SuperUserKey);

        _element = ElementBuilder.CreateSimpleElement(elementType);
        GetRequiredService<IElementService>().Save(_element);

        _databaseCacheRepository = new Mock<IDatabaseCacheRepository>();

        _elementCacheService = new ElementCacheService(
            _databaseCacheRepository.Object,
            GetRequiredService<ICoreScopeProvider>(),
            GetRequiredService<Microsoft.Extensions.Caching.Hybrid.HybridCache>(),
            GetRequiredService<IPublishedContentFactory>(),
            GetRequiredService<ICacheNodeFactory>(),
            Array.Empty<IElementSeedKeyProvider>(),
            GetRequiredService<IPublishedModelFactory>(),
            GetRequiredService<IPreviewService>(),
            GetRequiredService<IOptions<CacheSettings>>(),
            new NullLogger<ElementCacheService>(),
            GetRequiredService<IConvertedPublishedContentCacheFactory>());
    }

    [Test]
    public async Task Cannot_Populate_L0_Cache_From_Draft_Request()
    {
        _databaseCacheRepository.Setup(x => x.GetElementSourceAsync(_element.Key, true)).ReturnsAsync(BuildNode(isDraft: true));
        var reporter = (IMemoryCacheSizeReporter)_elementCacheService;

        IPublishedElement? draft = await _elementCacheService.GetByKeyAsync(_element.Key, preview: true);

        Assert.Multiple(() =>
        {
            Assert.That(draft, Is.Not.Null, "the draft should still resolve (from L1/repository)");
            Assert.That(reporter.GetApproximateCount(), Is.Zero, "a draft request must not populate the L0 cache");
        });
    }

    [Test]
    public async Task Can_Populate_L0_Cache_From_Published_Request()
    {
        _databaseCacheRepository.Setup(x => x.GetElementSourceAsync(_element.Key, false)).ReturnsAsync(BuildNode(isDraft: false));
        var reporter = (IMemoryCacheSizeReporter)_elementCacheService;

        IPublishedElement? published = await _elementCacheService.GetByKeyAsync(_element.Key, preview: false);

        Assert.Multiple(() =>
        {
            Assert.That(published, Is.Not.Null, "the published element should resolve");
            Assert.That(reporter.GetApproximateCount(), Is.EqualTo(1), "a published request should populate the L0 cache");
        });
    }
}
