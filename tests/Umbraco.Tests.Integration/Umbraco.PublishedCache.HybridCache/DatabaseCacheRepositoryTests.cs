using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;
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
    }

    private IDatabaseCacheRepository DatabaseCacheRepository => GetRequiredService<IDatabaseCacheRepository>();

    private IContentPublishingService ContentPublishingService => GetRequiredService<IContentPublishingService>();

    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private IMediaService MediaService => GetRequiredService<IMediaService>();

    private IMediaType MediaType { get; set; }

    private IMedia MediaItem1 { get; set; }

    private IMedia MediaItem2 { get; set; }

    private IMedia MediaItem3 { get; set; }

    public override async Task CreateTestDataAsync()
    {
        await base.CreateTestDataAsync();

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
    public async Task GetDocumentSourcesAsync_With_Keys_Returns_Matching_Sources()
    {
        // Arrange — publish the root so we exercise the published branch of the filter, and leave
        // the subpages in draft. Base fixture gives us 4 non-trashed documents.
        await ContentPublishingService.PublishAsync(Textpage.Key, [new()], Constants.Security.SuperUserKey);

        var requestedKeys = new[] { Textpage.Key, Subpage.Key, Subpage2.Key };

        // Act — preview = true so draft-only subpages are also returned.
        using var scope = ScopeProvider.CreateScope(autoComplete: true);
        var sources = (await DatabaseCacheRepository.GetDocumentSourcesAsync(requestedKeys, preview: true)).ToList();

        // Assert — only the three requested nodes come back, ignoring Subpage3 and the trashed node.
        var returnedKeys = sources.Select(s => s.Key).ToHashSet();
        Assert.That(returnedKeys, Is.EquivalentTo(requestedKeys));
    }

    [Test]
    public async Task GetDocumentSourcesAsync_With_Empty_Keys_Returns_Nothing()
    {
        // The batched loop iterates zero groups when the input is empty — no rows should be
        // returned, and the database must not be hit with a malformed `WHERE id IN ()`.
        using var scope = ScopeProvider.CreateScope(autoComplete: true);
        var sources = await DatabaseCacheRepository.GetDocumentSourcesAsync(Array.Empty<Guid>(), preview: true);

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
