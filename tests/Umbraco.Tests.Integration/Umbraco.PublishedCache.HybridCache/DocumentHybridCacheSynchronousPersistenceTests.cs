// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

/// <summary>
/// The published-content cache rows are written from inside the content transaction, under the content tree
/// lock. Until the lock wait and the notification pipeline are asynchronous, every await there needs a second
/// thread-pool thread while the first is blocked, so the save path must reach the database through the
/// synchronous repository members only. These tests fail when a save or publish routes through an asynchronous one.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class DocumentHybridCacheSynchronousPersistenceTests : UmbracoIntegrationTestWithContent
{
    private IPublishedContentCache PublishedContentCache => GetRequiredService<IPublishedContentCache>();

    private RecordingDatabaseCacheRepository Repository => (RecordingDatabaseCacheRepository)GetRequiredService<IDatabaseCacheRepository>();

    private IDocumentUrlService DocumentUrlService => GetRequiredService<IDocumentUrlService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

        // Run the post-commit refresher in-process, as the real application does.
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.AddNotificationHandler<ContentPublishingNotification, RedirectTrackingHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
        builder.Services.AddUnique<IDatabaseCacheRepository>(sp =>
            new RecordingDatabaseCacheRepository(ActivatorUtilities.CreateInstance<DatabaseCacheRepository>(sp)));
    }

    [Test]
    public void Publish_WritesTheCacheRows_ThroughTheSynchronousRepositoryMembers()
    {
        Repository.Reset();

        Assert.That(ContentService.Publish(Textpage, ["*"]).Success, Is.True);

        Assert.Multiple(() =>
        {
            Assert.That(Repository.SynchronousWrites, Is.GreaterThan(0), "The publish must write its cache rows.");
            Assert.That(Repository.AsynchronousWrites, Is.Zero, "No asynchronous repository member may run inside the content transaction.");
            Assert.That(PublishedContentCache.GetById(Textpage.Key), Is.Not.Null, "The published document must be served from the cache after the commit.");
        });
    }

    [Test]
    public void Unpublish_RemovesThePublishedRow_ThroughTheSynchronousRepositoryMembers_AndClearsTheMemoryCache()
    {
        Assert.That(ContentService.Publish(Textpage, ["*"]).Success, Is.True);
        Assert.That(PublishedContentCache.GetById(Textpage.Key), Is.Not.Null);
        Repository.Reset();

        Assert.That(ContentService.Unpublish(Textpage).Success, Is.True);

        Assert.Multiple(() =>
        {
            Assert.That(Repository.AsynchronousWrites, Is.Zero);
            Assert.That(Repository.SynchronousWrites, Is.GreaterThan(0));
            Assert.That(PublishedContentCache.GetById(Textpage.Key), Is.Null, "The published entry must be gone once the unpublish has committed.");
        });
    }

    [Test]
    public void Trash_DeletesTheRows_ThroughTheSynchronousRepositoryMembers_AndClearsTheMemoryCache()
    {
        Assert.That(ContentService.Publish(Textpage, ["*"]).Success, Is.True);
        Repository.Reset();

        Assert.That(ContentService.MoveToRecycleBin(Textpage).Success, Is.True);

        Assert.Multiple(() =>
        {
            Assert.That(Repository.AsynchronousWrites, Is.Zero);
            Assert.That(Repository.SynchronousWrites, Is.GreaterThan(0));
            Assert.That(PublishedContentCache.GetById(Textpage.Key), Is.Null);
        });
    }

    /// <summary>
    /// The redirect tracker runs on the publishing notification, inside the content transaction. Content that has
    /// never been published has no route to preserve, so the tracker must not look it up in the published content
    /// cache: that lookup misses every tier and reads through to the database on the asynchronous pipeline while
    /// the content tree lock is held.
    /// </summary>
    [Test]
    public void FirstPublish_WithRedirectTracking_DoesNotReadThroughThePublishedCache()
    {
        DocumentUrlService.InitAsync(false, CancellationToken.None).GetAwaiter().GetResult();
        Repository.Reset();

        Assert.That(ContentService.Publish(Textpage, ["*"]).Success, Is.True);

        Assert.That(Repository.AsynchronousReads, Is.Zero, "No published-cache read-through may run inside the content transaction for content without a published route.");
    }

    [Test]
    public void DraftSave_DoesNotRouteThroughAsynchronousRepositoryMembers()
    {
        Repository.Reset();

        IContent draft = ContentService.GetById(Textpage.Id)!;
        draft.Name = "Renamed";
        ContentService.Save(draft);

        Assert.Multiple(() =>
        {
            Assert.That(Repository.AsynchronousWrites, Is.Zero);
            Assert.That(Repository.AsynchronousReads, Is.Zero, "The post-commit memory refresh must read synchronously as well.");
        });
    }
}
