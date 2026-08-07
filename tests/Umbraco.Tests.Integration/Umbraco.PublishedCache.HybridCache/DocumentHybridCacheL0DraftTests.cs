using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DependencyInjection;
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
internal sealed class DocumentHybridCacheL0DraftTests : UmbracoIntegrationTestWithContentEditing
{
    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        // Wire the content-change cache refreshers so published/draft content is retrievable (see the
        // integration test CLAUDE.md). L0 is left unbounded (no MaximumLocalCacheItems) — the default.
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
    }

    private IDocumentCacheService DocumentCacheService => GetRequiredService<IDocumentCacheService>();

    private IContentPublishingService ContentPublishingService => GetRequiredService<IContentPublishingService>();

    [Test]
    public async Task Cannot_Populate_L0_Cache_From_Draft_Request()
    {
        Guid key = await CreatePublishedPageAsync();
        var reporter = (IMemoryCacheSizeReporter)DocumentCacheService;

        // A draft request is never served back from L0 (the L0 read fast path is guarded by preview is false),
        // so storing it there would only waste capacity and leave a stale, non-invalidatable entry.
        IPublishedContent? draft = await DocumentCacheService.GetByKeyAsync(key, preview: true);

        Assert.Multiple(() =>
        {
            Assert.That(draft, Is.Not.Null, "the draft should still resolve (from L1/repository)");
            Assert.That(reporter.GetApproximateCount(), Is.Zero, "a draft request must not populate the L0 cache");
        });
    }

    [Test]
    public async Task Can_Populate_L0_Cache_From_Published_Request()
    {
        Guid key = await CreatePublishedPageAsync();
        var reporter = (IMemoryCacheSizeReporter)DocumentCacheService;

        IPublishedContent? published = await DocumentCacheService.GetByKeyAsync(key, preview: false);

        Assert.Multiple(() =>
        {
            Assert.That(published, Is.Not.Null, "the published content should resolve");
            Assert.That(reporter.GetApproximateCount(), Is.EqualTo(1), "a published request should populate the L0 cache");
        });
    }

    private async Task<Guid> CreatePublishedPageAsync()
    {
        ContentCreateModel page = ContentEditingBuilder.CreateSimpleContent(ContentType.Key, "L0 Draft Test Page");
        var created = await ContentEditingService.CreateAsync(page, Constants.Security.SuperUserKey);
        Assert.IsTrue(created.Success);

        Guid key = page.Key!.Value;
        var published = await ContentPublishingService.PublishAsync(key, [new CulturePublishScheduleModel()], Constants.Security.SuperUserKey);
        Assert.IsTrue(published.Success);

        return key;
    }
}
