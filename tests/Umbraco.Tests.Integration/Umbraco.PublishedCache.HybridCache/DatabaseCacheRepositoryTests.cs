using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class DatabaseCacheRepositoryTests : UmbracoIntegrationTestWithContent
{
    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.AddNotificationHandler<MediaTreeChangeNotification, MediaTreeChangeDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();

        // Force several delete batches over the handful of fixture documents so the batched delete loop is exercised.
        builder.Services.PostConfigure<NuCacheSettings>(options => options.ContentTypeRebuildDeleteBatchSize = 2);
    }

    private IDatabaseCacheRepository DatabaseCacheRepository => GetRequiredService<IDatabaseCacheRepository>();

    private IDocumentCacheService DocumentCacheService => GetRequiredService<IDocumentCacheService>();

    private ISqlContext SqlContext => GetRequiredService<ISqlContext>();

    private IContentPublishingService ContentPublishingService => GetRequiredService<IContentPublishingService>();

    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private IMediaService MediaService => GetRequiredService<IMediaService>();

    private IMediaType MediaType { get; set; }

    private IMedia MediaItem1 { get; set; }

    private IMedia MediaItem2 { get; set; }

    private IMedia MediaItem3 { get; set; }

    public override void CreateTestData()
    {
        base.CreateTestData();

        // Add a few media items so the media-side methods have something to read.
        MediaType = MediaTypeService.Get("image")!;
        MediaItem1 = new MediaBuilder().WithName("Image 1").WithMediaType(MediaType).Build();
        MediaItem2 = new MediaBuilder().WithName("Image 2").WithMediaType(MediaType).Build();
        MediaItem3 = new MediaBuilder().WithName("Image 3").WithMediaType(MediaType).Build();
        MediaService.Save(MediaItem1);
        MediaService.Save(MediaItem2);
        MediaService.Save(MediaItem3);
    }

    [Test]
    public async Task GetContentSourcesAsync_With_Keys_Returns_Matching_Sources()
    {
        // Arrange — publish the root so we exercise the published branch of the filter, and leave
        // the subpages in draft. Base fixture gives us 4 non-trashed documents.
        await ContentPublishingService.PublishAsync(Textpage.Key, [new()], Constants.Security.SuperUserKey);

        var requestedKeys = new[] { Textpage.Key, Subpage.Key, Subpage2.Key };

        // Act — preview = true so draft-only subpages are also returned.
        using var scope = ScopeProvider.CreateScope(autoComplete: true);
        var sources = (await DatabaseCacheRepository.GetContentSourcesAsync(requestedKeys, preview: true)).ToList();

        // Assert — only the three requested nodes come back, ignoring Subpage3 and the trashed node.
        var returnedKeys = sources.Select(s => s.Key).ToHashSet();
        Assert.That(returnedKeys, Is.EquivalentTo(requestedKeys));
    }

    [Test]
    public async Task GetContentSourcesAsync_With_Empty_Keys_Returns_Nothing()
    {
        // The batched loop iterates zero groups when the input is empty — no rows should be
        // returned, and the database must not be hit with a malformed `WHERE id IN ()`.
        using var scope = ScopeProvider.CreateScope(autoComplete: true);
        var sources = await DatabaseCacheRepository.GetContentSourcesAsync(Array.Empty<Guid>(), preview: true);

        Assert.That(sources, Is.Empty);
    }

    [Test]
    public async Task GetMediaSourcesAsync_With_Keys_Returns_Matching_Sources()
    {
        var requestedKeys = new[] { MediaItem1.Key, MediaItem2.Key };

        using var scope = ScopeProvider.CreateScope(autoComplete: true);
        var sources = (await DatabaseCacheRepository.GetMediaSourcesAsync(requestedKeys)).ToList();

        var returnedKeys = sources.Select(s => s.Key).ToHashSet();
        Assert.That(returnedKeys, Is.EquivalentTo(requestedKeys));
    }

    [Test]
    public async Task GetMediaSourcesAsync_With_Empty_Keys_Returns_Nothing()
    {
        using var scope = ScopeProvider.CreateScope(autoComplete: true);
        var sources = await DatabaseCacheRepository.GetMediaSourcesAsync(Array.Empty<Guid>());

        Assert.That(sources, Is.Empty);
    }

    [Test]
    public void Rebuild_Deletes_Stale_Rows_In_Batches_And_Repopulates()
    {
        // Arrange — populate the cache for the document type, then mark every row for the type as stale.
        // If the batched delete failed to remove a row, the repopulation's insert-where-not-exists would
        // skip it and the stale marker would survive — so this proves the delete actually runs (with a
        // batch size of 2 forcing multiple batches over the fixture's documents).
        DocumentCacheService.Rebuild([ContentType.Id]);

        const string staleMarker = "STALE";
        var contentTypeNodeIds = new[] { Textpage.Id, Subpage.Id, Subpage2.Id, Subpage3.Id };

        using (var scope = ScopeProvider.CreateScope())
        {
            ScopeAccessor.AmbientScope!.Database.Execute(
                SqlContext.Sql()
                    .Update<ContentNuDto>(u => u.Set(x => x.Data, staleMarker))
                    .WhereIn<ContentNuDto>(x => x.NodeId, contentTypeNodeIds));
            scope.Complete();
        }

        // Act
        DocumentCacheService.Rebuild([ContentType.Id]);

        // Assert — every document of the type has a refreshed (non-stale) row, and none was left behind.
        using (var scope = ScopeProvider.CreateScope(autoComplete: true))
        {
            var dtos = ScopeAccessor.AmbientScope!.Database.Fetch<ContentNuDto>(
                SqlContext.Sql()
                    .Select<ContentNuDto>()
                    .From<ContentNuDto>()
                    .WhereIn<ContentNuDto>(x => x.NodeId, contentTypeNodeIds));

            Assert.Multiple(() =>
            {
                Assert.That(dtos, Is.Not.Empty, "Expected the cache to be repopulated for the content type.");
                Assert.That(
                    dtos.Select(x => x.NodeId).Distinct(),
                    Is.EquivalentTo(contentTypeNodeIds),
                    "Every document of the type should have a cache row after the rebuild.");
                Assert.That(
                    dtos.Any(x => x.Data == staleMarker),
                    Is.False,
                    "The batched delete should have removed the stale rows before repopulation.");
            });
        }
    }

    [Test]
    public void GetContentByContentTypeKey_With_Keys_Returns_Matching_Nodes()
    {
        // Exercises the batched ForNodes helpers (GetContentMetadataForNodes,
        // GetPropertyDataForNodes, etc.) used by the rebuild/document-type lookup pipeline.
        using var scope = ScopeProvider.CreateScope(autoComplete: true);
        var nodes = DatabaseCacheRepository
            .GetContentByContentTypeKey([ContentType.Key], ContentCacheDataSerializerEntityType.Document)
            .ToList();

        // The 4 non-trashed documents created by the fixture must all surface.
        var returnedKeys = nodes.Select(n => n.Key).ToHashSet();
        Assert.That(returnedKeys, Does.Contain(Textpage.Key));
        Assert.That(returnedKeys, Does.Contain(Subpage.Key));
        Assert.That(returnedKeys, Does.Contain(Subpage2.Key));
        Assert.That(returnedKeys, Does.Contain(Subpage3.Key));
    }
}
