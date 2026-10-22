// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
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

    private IDocumentCacheService DocumentCacheService => GetRequiredService<IDocumentCacheService>();

    private RecordingDatabaseCacheRepository Repository => (RecordingDatabaseCacheRepository)GetRequiredService<IDatabaseCacheRepository>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

        // Run the post-commit refresher in-process, as the real application does.
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();

        // The redirect tracker looks the document up in the published cache from inside the publishing transaction.
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
    public void Unpublish_RemovesThePublishedRow_ThroughTheSynchronousRepositoryMembers_AndClearsTheMemoryCacheAfterCommit()
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
    public void Trash_DeletesTheRows_ThroughTheSynchronousRepositoryMembers_AndClearsTheMemoryCacheAfterCommit()
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

    [Test]
    public void FirstPublish_DoesNotReadThePublishedCacheAsynchronously()
    {
        Repository.Reset();

        Assert.That(ContentService.Publish(Textpage, ["*"]).Success, Is.True);

        Assert.That(Repository.AsynchronousReads, Is.Zero, "The redirect tracker must not read through an asynchronous member inside the transaction.");
    }

    [Test]
    public async Task Republish_LooksUpThePublishedContentSynchronously_WhenItIsNotInMemory()
    {
        Assert.That(ContentService.Publish(Textpage, ["*"]).Success, Is.True);
        await DocumentCacheService.RemoveFromMemoryCacheAsync(Textpage.Key);
        Repository.Reset();

        IContent content = ContentService.GetById(Textpage.Id)!;
        Assert.That(ContentService.Publish(content, ["*"]).Success, Is.True);

        Assert.Multiple(() =>
        {
            Assert.That(Repository.AsynchronousReads, Is.Zero);
            Assert.That(Repository.SynchronousReads, Is.GreaterThan(0), "The redirect tracker's lookup must read the database on the calling thread.");
        });
    }

    [Test]
    public async Task GetById_OnAMemoryMiss_ReadsTheDatabaseSynchronously_AndThenServesFromMemory()
    {
        Assert.That(ContentService.Publish(Textpage, ["*"]).Success, Is.True);
        await DocumentCacheService.RemoveFromMemoryCacheAsync(Textpage.Key);
        Repository.Reset();

        IPublishedContent? first = PublishedContentCache.GetById(Textpage.Id);
        IPublishedContent? second = PublishedContentCache.GetById(Textpage.Id);

        Assert.Multiple(() =>
        {
            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.SameAs(first), "The second lookup must be served from the converted-content cache.");
            Assert.That(Repository.SynchronousReads, Is.EqualTo(1));
            Assert.That(Repository.AsynchronousReads, Is.Zero);
        });
    }

    [Test]
    public void GetById_OnAConvertedCacheMiss_IsServedFromTheLocalHybridCache()
    {
        Assert.That(ContentService.Publish(Textpage, ["*"]).Success, Is.True);
        Assert.That(PublishedContentCache.GetById(Textpage.Id), Is.Not.Null);
        DocumentCacheService.ClearConvertedContentCache();
        Repository.Reset();

        Assert.Multiple(() =>
        {
            Assert.That(PublishedContentCache.GetById(Textpage.Id), Is.Not.Null);
            Assert.That(Repository.SynchronousReads, Is.Zero, "An in-process HybridCache hit must not reach the database.");
            Assert.That(Repository.AsynchronousReads, Is.Zero);
        });
    }

    [Test]
    public void GetById_ReturnsNull_ForUnpublishedContent_WithoutAsynchronousReads()
    {
        Repository.Reset();

        Assert.Multiple(() =>
        {
            Assert.That(PublishedContentCache.GetById(Textpage.Id), Is.Null);
            Assert.That(Repository.AsynchronousReads, Is.Zero);
        });
    }

    /// <summary>
    /// Forwards every call to the real repository and counts whether the save path used the synchronous or the
    /// asynchronous members. The synchronous members must be forwarded explicitly: the interface defaults bridge to
    /// the asynchronous ones, which would hide the distinction.
    /// </summary>
    private sealed class RecordingDatabaseCacheRepository : IDatabaseCacheRepository
    {
        private readonly IDatabaseCacheRepository _inner;

        public RecordingDatabaseCacheRepository(IDatabaseCacheRepository inner) => _inner = inner;

        public int SynchronousWrites { get; private set; }

        public int AsynchronousWrites { get; private set; }

        public int AsynchronousReads { get; private set; }

        public int SynchronousReads { get; private set; }

        public void Reset()
        {
            SynchronousWrites = 0;
            AsynchronousWrites = 0;
            AsynchronousReads = 0;
            SynchronousReads = 0;
        }

        public void DeleteContentItem(int id)
        {
            SynchronousWrites++;
            _inner.DeleteContentItem(id);
        }

        public void RefreshContent(ContentCacheNode contentCacheNode)
        {
            SynchronousWrites++;
            _inner.RefreshContent(contentCacheNode);
        }

        public void RefreshMedia(ContentCacheNode contentCacheNode)
        {
            SynchronousWrites++;
            _inner.RefreshMedia(contentCacheNode);
        }

        public void RemovePublishedContent(int id)
        {
            SynchronousWrites++;
            _inner.RemovePublishedContent(id);
        }

        public (ContentCacheNode? Draft, ContentCacheNode? Published) GetContentSourceForPublishStates(Guid key)
        {
            SynchronousReads++;
            return _inner.GetContentSourceForPublishStates(key);
        }

        public ContentCacheNode? GetContentSource(Guid key, bool preview = false)
        {
            SynchronousReads++;
            return _inner.GetContentSource(key, preview);
        }

        public Task DeleteContentItemAsync(int id)
        {
            AsynchronousWrites++;
            return _inner.DeleteContentItemAsync(id);
        }

        public Task RefreshContentAsync(ContentCacheNode contentCacheNode)
        {
            AsynchronousWrites++;
            return _inner.RefreshContentAsync(contentCacheNode);
        }

        public Task RefreshMediaAsync(ContentCacheNode contentCacheNode)
        {
            AsynchronousWrites++;
            return _inner.RefreshMediaAsync(contentCacheNode);
        }

        public Task RemovePublishedContentAsync(int id)
        {
            AsynchronousWrites++;
            return _inner.RemovePublishedContentAsync(id);
        }

        public Task<(ContentCacheNode? Draft, ContentCacheNode? Published)> GetContentSourceForPublishStatesAsync(Guid key)
        {
            AsynchronousReads++;
            return _inner.GetContentSourceForPublishStatesAsync(key);
        }

        public Task<ContentCacheNode?> GetContentSourceAsync(Guid key, bool preview = false)
        {
            AsynchronousReads++;
            return _inner.GetContentSourceAsync(key, preview);
        }

        public Task<IEnumerable<ContentCacheNode>> GetContentSourcesAsync(IEnumerable<Guid> keys, bool preview = false)
            => _inner.GetContentSourcesAsync(keys, preview);

        public Task<ContentCacheNode?> GetMediaSourceAsync(Guid key)
            => _inner.GetMediaSourceAsync(key);

        public Task<IEnumerable<ContentCacheNode>> GetMediaSourcesAsync(IEnumerable<Guid> keys)
            => _inner.GetMediaSourcesAsync(keys);

        public IEnumerable<ContentCacheNode> GetContentByContentTypeKey(IEnumerable<Guid> keys, ContentCacheDataSerializerEntityType entityType)
            => _inner.GetContentByContentTypeKey(keys, entityType);

        public IEnumerable<Guid> GetDocumentKeysByContentTypeKeys(IEnumerable<Guid> keys, bool published = false)
            => _inner.GetDocumentKeysByContentTypeKeys(keys, published);

        public IEnumerable<(Guid Key, bool IsDraft)> GetDocumentKeysWithPublishedStatus(IEnumerable<Guid> contentTypeKeys)
            => _inner.GetDocumentKeysWithPublishedStatus(contentTypeKeys);

        public IEnumerable<Guid> GetMediaKeysByContentTypeKeys(IEnumerable<Guid> mediaTypeKeys)
            => _inner.GetMediaKeysByContentTypeKeys(mediaTypeKeys);

        public void Rebuild(IReadOnlyCollection<int>? contentTypeIds, IReadOnlyCollection<int>? mediaTypeIds, Action<Action>? executeStep)
            => _inner.Rebuild(contentTypeIds, mediaTypeIds, executeStep);
    }
}
