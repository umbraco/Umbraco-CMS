using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

/// <summary>
/// The published-element cache rows are written from inside the element transaction, under the element tree
/// lock. Until the lock wait and the notification pipeline are asynchronous, every await there needs a second
/// thread-pool thread while the first is blocked, so the save path must reach the database through the
/// synchronous repository members only. These tests fail when a save or publish routes through an asynchronous one.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class ElementHybridCacheSynchronousPersistenceTests : UmbracoIntegrationTest
{
    private IPublishedElementCache PublishedElementCache => GetRequiredService<IPublishedElementCache>();

    private RecordingDatabaseCacheRepository Repository => (RecordingDatabaseCacheRepository)GetRequiredService<IDatabaseCacheRepository>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IElementEditingService ElementEditingService => GetRequiredService<IElementEditingService>();

    private IElementPublishingService ElementPublishingService => GetRequiredService<IElementPublishingService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddUmbracoHybridCache();

        // Run the post-commit refresher in-process, as the real application does.
        builder.AddNotificationHandler<ElementTreeChangeNotification, ElementTreeChangeDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
        builder.Services.AddUnique<IDatabaseCacheRepository>(sp =>
            new RecordingDatabaseCacheRepository(ActivatorUtilities.CreateInstance<DatabaseCacheRepository>(sp)));
    }

    [Test]
    public async Task Publish_WritesTheCacheRows_ThroughTheSynchronousRepositoryMembers()
    {
        Guid elementKey = await CreateElement();
        Repository.Reset();

        await Publish(elementKey);

        Assert.Multiple(() =>
        {
            Assert.That(Repository.SynchronousWrites, Is.GreaterThan(0), "The publish must write its cache rows.");
            Assert.That(Repository.AsynchronousWrites, Is.Zero, "No asynchronous repository member may run inside the element transaction.");
            Assert.That(PublishedElementCache.GetById(false, elementKey), Is.Not.Null, "The published element must be served from the cache after the commit.");
        });
    }

    [Test]
    public async Task Unpublish_RemovesThePublishedRow_ThroughTheSynchronousRepositoryMembers_AndClearsTheMemoryCache()
    {
        Guid elementKey = await CreateElement();
        await Publish(elementKey);
        Assert.That(PublishedElementCache.GetById(false, elementKey), Is.Not.Null);
        Repository.Reset();

        var unpublishResult = await ElementPublishingService.UnpublishAsync(elementKey, null, Constants.Security.SuperUserKey);
        Assert.That(unpublishResult.Success, Is.True);

        Assert.Multiple(() =>
        {
            Assert.That(Repository.AsynchronousWrites, Is.Zero);
            Assert.That(Repository.SynchronousWrites, Is.GreaterThan(0));
            Assert.That(PublishedElementCache.GetById(false, elementKey), Is.Null, "The published entry must be gone once the unpublish has committed.");
        });
    }

    [Test]
    public async Task Trash_DeletesTheRows_ThroughTheSynchronousRepositoryMembers_AndClearsTheMemoryCache()
    {
        Guid elementKey = await CreateElement();
        await Publish(elementKey);
        Repository.Reset();

        var trashResult = await ElementEditingService.MoveToRecycleBinAsync(elementKey, Constants.Security.SuperUserKey);
        Assert.That(trashResult.Success, Is.True);

        Assert.Multiple(() =>
        {
            Assert.That(Repository.AsynchronousWrites, Is.Zero);
            Assert.That(Repository.SynchronousWrites, Is.GreaterThan(0));
            Assert.That(PublishedElementCache.GetById(false, elementKey), Is.Null);
        });
    }

    [Test]
    public async Task DraftSave_DoesNotRouteThroughAsynchronousRepositoryMembers()
    {
        Guid elementKey = await CreateElement();
        await Publish(elementKey);
        Repository.Reset();

        var updateResult = await ElementEditingService.UpdateAsync(
            elementKey,
            new ElementUpdateModel { Variants = [new() { Name = "Renamed" }] },
            Constants.Security.SuperUserKey);
        Assert.That(updateResult.Success, Is.True);

        Assert.Multiple(() =>
        {
            Assert.That(Repository.AsynchronousWrites, Is.Zero);
            Assert.That(Repository.AsynchronousReads, Is.Zero, "The post-commit memory refresh must read synchronously as well.");
        });
    }

    private async Task<Guid> CreateElement()
    {
        IContentType elementType = ContentTypeBuilder.CreateBasicElementType(Guid.NewGuid().ToString("N"));
        elementType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        var createResult = await ElementEditingService.CreateAsync(
            new ElementCreateModel { ContentTypeKey = elementType.Key, Variants = [new() { Name = "Element" }] },
            Constants.Security.SuperUserKey);
        Assert.That(createResult.Success, Is.True);
        return createResult.Result.Content!.Key;
    }

    private async Task Publish(Guid elementKey)
    {
        var publishResult = await ElementPublishingService.PublishAsync(
            elementKey,
            [new CulturePublishScheduleModel { Culture = "*", Schedule = null }],
            Constants.Security.SuperUserKey);
        Assert.That(publishResult.Success, Is.True);
    }
}
