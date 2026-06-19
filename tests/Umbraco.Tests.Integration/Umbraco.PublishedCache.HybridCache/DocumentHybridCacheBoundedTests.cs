using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class DocumentHybridCacheBoundedTests : UmbracoIntegrationTestWithContentEditing
{
    private const int MaximumItems = 5;

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        // Wire the content-change cache refreshers so published content is retrievable (see the integration
        // test CLAUDE.md), and bound the L0 converted-content cache so eviction is exercised end-to-end.
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
        builder.Services.Configure<CacheSettings>(options => options.Entry.Document.MaximumLocalCacheItems = MaximumItems);
    }

    private IPublishedContentCache PublishedContentHybridCache => GetRequiredService<IPublishedContentCache>();

    private IContentPublishingService ContentPublishingService => GetRequiredService<IContentPublishingService>();

    private IDocumentCacheService DocumentCacheService => GetRequiredService<IDocumentCacheService>();

    [Test]
    public async Task Can_Evict_And_Still_Serve_Content_When_L0_Is_Bounded()
    {
        const int pageCount = 30;
        var keys = await CreatePublishedPagesAsync(pageCount);

        // Populate the L0 cache by requesting every page through the published cache.
        foreach (Guid key in keys)
        {
            Assert.That(await PublishedContentHybridCache.GetByIdAsync(key), Is.Not.Null);
        }

        var reporter = (IMemoryCacheSizeReporter)DocumentCacheService;
        TestContext.Out.WriteLine($"L0 entry count after requesting {pageCount} pages (max {MaximumItems}): {reporter.GetApproximateCount()}");

        // The bounded L0 cache evicted: it did not retain all the requested items. (Eviction is applied
        // asynchronously by the eviction policy, so poll for the count to settle.)
        Assert.That(
            () => reporter.GetApproximateCount(),
            Is.LessThanOrEqualTo(MaximumItems).After(5000, 200),
            "the bounded L0 cache should evict down to its configured maximum rather than retain every requested item");

        // ...and every page still resolves correctly after eviction (cache miss -> re-fetch -> re-convert).
        foreach (Guid key in keys)
        {
            IPublishedContent? content = await PublishedContentHybridCache.GetByIdAsync(key);
            Assert.That(content, Is.Not.Null, $"content '{key}' should still resolve after eviction");
        }
    }

    private async Task<List<Guid>> CreatePublishedPagesAsync(int count)
    {
        var keys = new List<Guid>();
        for (var i = 0; i < count; i++)
        {
            ContentCreateModel page = ContentEditingBuilder.CreateSimpleContent(ContentType.Key, $"Bounded Page {i}");
            var created = await ContentEditingService.CreateAsync(page, Constants.Security.SuperUserKey);
            Assert.IsTrue(created.Success);

            Guid key = page.Key!.Value;
            var published = await ContentPublishingService.PublishAsync(key, [new CulturePublishScheduleModel()], Constants.Security.SuperUserKey);
            Assert.IsTrue(published.Success);

            keys.Add(key);
        }

        return keys;
    }
}
