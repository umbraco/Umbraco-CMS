using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Attributes;
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
        builder.AddNotificationHandler<ElementTreeChangeNotification, ElementTreeChangeDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
    }

    // Applied via [ConfigureBuilder] to only the batched-delete test, so the tiny batch size doesn't change the
    // number of SQL statements other tests in this fixture issue.
    public static void ConfigureSmallDeleteBatchSize(IUmbracoBuilder builder) =>
        builder.Services.PostConfigure<NuCacheSettings>(options => options.ContentTypeRebuildDeleteBatchSize = 2);

    public static void ConfigureJsonSerializer(IUmbracoBuilder builder) =>
        builder.Services.PostConfigure<NuCacheSettings>(options => options.NuCacheSerializerType = NuCacheSerializerType.JSON);

    private IDatabaseCacheRepository DatabaseCacheRepository => GetRequiredService<IDatabaseCacheRepository>();

    private IDocumentCacheService DocumentCacheService => GetRequiredService<IDocumentCacheService>();

    private ISqlContext SqlContext => GetRequiredService<ISqlContext>();

    private IContentPublishingService ContentPublishingService => GetRequiredService<IContentPublishingService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IElementEditingService ElementEditingService => GetRequiredService<IElementEditingService>();

    private IElementPublishingService ElementPublishingService => GetRequiredService<IElementPublishingService>();

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

    // The synchronous reads must return what their asynchronous counterparts return. The fixture's root is published
    // and its subpages are draft only, so outside preview the subpages are filtered out.
    [TestCase(true)]
    [TestCase(false)]
    public async Task GetDocumentSource_Returns_The_Same_Source_As_GetDocumentSourceAsync(bool preview)
    {
        await ContentPublishingService.PublishAsync(Textpage.Key, [new()], Constants.Security.SuperUserKey);

        using var scope = ScopeProvider.CreateScope(autoComplete: true);
        foreach (Guid key in new[] { Textpage.Key, Subpage.Key })
        {
            ContentCacheNode? expected = await DatabaseCacheRepository.GetDocumentSourceAsync(key, preview);
            ContentCacheNode? actual = DatabaseCacheRepository.GetDocumentSource(key, preview);

            Assert.That(Describe(actual), Is.EqualTo(Describe(expected)), $"Key {key}, preview {preview}");
        }

        Assert.That(DatabaseCacheRepository.GetDocumentSource(Subpage.Key, preview), preview ? Is.Not.Null : Is.Null);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task GetDocumentSources_Returns_The_Same_Sources_As_GetDocumentSourcesAsync(bool preview)
    {
        await ContentPublishingService.PublishAsync(Textpage.Key, [new()], Constants.Security.SuperUserKey);
        var requestedKeys = new[] { Textpage.Key, Subpage.Key, Subpage2.Key };

        using var scope = ScopeProvider.CreateScope(autoComplete: true);
        var expected = (await DatabaseCacheRepository.GetDocumentSourcesAsync(requestedKeys, preview)).Select(Describe).ToList();
        var actual = DatabaseCacheRepository.GetDocumentSources(requestedKeys, preview).Select(Describe).ToList();

        Assert.That(actual, Is.EquivalentTo(expected));
        Assert.That(actual, Has.Count.EqualTo(preview ? requestedKeys.Length : 1));
    }

    [Test]
    public async Task GetMediaSource_Returns_The_Same_Source_As_GetMediaSourceAsync()
    {
        using var scope = ScopeProvider.CreateScope(autoComplete: true);
        ContentCacheNode? expected = await DatabaseCacheRepository.GetMediaSourceAsync(MediaItem1.Key);
        ContentCacheNode? actual = DatabaseCacheRepository.GetMediaSource(MediaItem1.Key);

        Assert.That(actual, Is.Not.Null);
        Assert.That(Describe(actual), Is.EqualTo(Describe(expected)));
    }

    [Test]
    public async Task GetMediaSources_Returns_The_Same_Sources_As_GetMediaSourcesAsync()
    {
        var requestedKeys = new[] { MediaItem1.Key, MediaItem2.Key };

        using var scope = ScopeProvider.CreateScope(autoComplete: true);
        var expected = (await DatabaseCacheRepository.GetMediaSourcesAsync(requestedKeys)).Select(Describe).ToList();
        var actual = DatabaseCacheRepository.GetMediaSources(requestedKeys).Select(Describe).ToList();

        Assert.That(actual, Is.EquivalentTo(expected));
        Assert.That(actual, Has.Count.EqualTo(requestedKeys.Length));
    }

    private static (int Id, Guid Key, bool IsDraft, string? Name)? Describe(ContentCacheNode? node)
        => node is null ? null : (node.Id, node.Key, node.IsDraft, node.Data?.Name);

    [Test]
    public async Task GetDocumentSourceAsync_Without_An_Ambient_Scope_Returns_The_Published_Source()
    {
        await ContentPublishingService.PublishAsync(Textpage.Key, [new()], Constants.Security.SuperUserKey);

        ContentCacheNode? source = await DatabaseCacheRepository.GetDocumentSourceAsync(Textpage.Key);

        Assert.IsNotNull(source);
        Assert.AreEqual(Textpage.Key, source!.Key);
    }

    [Test]
    public async Task GetDocumentSource_Without_An_Ambient_Scope_Returns_The_Published_Source()
    {
        await ContentPublishingService.PublishAsync(Textpage.Key, [new()], Constants.Security.SuperUserKey);

        ContentCacheNode? source = DatabaseCacheRepository.GetDocumentSource(Textpage.Key);

        Assert.IsNotNull(source);
        Assert.AreEqual(Textpage.Key, source!.Key);
    }

    [Test]
    public async Task GetDocumentSourcesAsync_Without_An_Ambient_Scope_Returns_Matching_Sources()
    {
        var requestedKeys = new[] { Textpage.Key, Subpage.Key };

        var sources = (await DatabaseCacheRepository.GetDocumentSourcesAsync(requestedKeys, preview: true)).ToList();

        Assert.That(sources.Select(s => s.Key), Is.EquivalentTo(requestedKeys));
    }

    [Test]
    public async Task GetMediaSourceAsync_Without_An_Ambient_Scope_Returns_The_Source()
    {
        ContentCacheNode? source = await DatabaseCacheRepository.GetMediaSourceAsync(MediaItem1.Key);

        Assert.IsNotNull(source);
        Assert.AreEqual(MediaItem1.Key, source!.Key);
    }

    [Test]
    public void GetMediaSources_Without_An_Ambient_Scope_Returns_Matching_Sources()
    {
        var requestedKeys = new[] { MediaItem1.Key, MediaItem2.Key };

        var sources = DatabaseCacheRepository.GetMediaSources(requestedKeys).ToList();

        Assert.That(sources.Select(s => s.Key), Is.EquivalentTo(requestedKeys));
    }

    [Test]
    public async Task GetMediaSourcesAsync_Without_An_Ambient_Scope_Returns_Matching_Sources()
    {
        var requestedKeys = new[] { MediaItem1.Key, MediaItem2.Key };

        var sources = (await DatabaseCacheRepository.GetMediaSourcesAsync(requestedKeys)).ToList();

        Assert.That(sources.Select(s => s.Key), Is.EquivalentTo(requestedKeys));
    }

    [Test]
    public async Task GetDocumentSources_Without_An_Ambient_Scope_Returns_Matching_Sources()
    {
        await ContentPublishingService.PublishAsync(Textpage.Key, [new()], Constants.Security.SuperUserKey);
        var requestedKeys = new[] { Textpage.Key, Subpage.Key };

        var sources = DatabaseCacheRepository.GetDocumentSources(requestedKeys, preview: true).ToList();

        Assert.That(sources.Select(s => s.Key), Is.EquivalentTo(requestedKeys));
    }

    [Test]
    public void GetMediaSource_Without_An_Ambient_Scope_Returns_The_Source()
    {
        ContentCacheNode? source = DatabaseCacheRepository.GetMediaSource(MediaItem1.Key);

        Assert.IsNotNull(source);
        Assert.AreEqual(MediaItem1.Key, source!.Key);
    }

    [Test]
    public async Task GetDocumentSourceForPublishStatesAsync_Without_An_Ambient_Scope_Returns_Both_States()
    {
        await ContentPublishingService.PublishAsync(Textpage.Key, [new()], Constants.Security.SuperUserKey);

        (ContentCacheNode? draft, ContentCacheNode? published) = await DatabaseCacheRepository.GetDocumentSourceForPublishStatesAsync(Textpage.Key);

        Assert.Multiple(() =>
        {
            Assert.That(draft?.Key, Is.EqualTo(Textpage.Key));
            Assert.That(draft?.IsDraft, Is.True);
            Assert.That(published?.Key, Is.EqualTo(Textpage.Key));
            Assert.That(published?.IsDraft, Is.False);
        });
    }

    [Test]
    public async Task GetDocumentSourceForPublishStates_Without_An_Ambient_Scope_Returns_Both_States()
    {
        await ContentPublishingService.PublishAsync(Textpage.Key, [new()], Constants.Security.SuperUserKey);

        (ContentCacheNode? draft, ContentCacheNode? published) = DatabaseCacheRepository.GetDocumentSourceForPublishStates(Textpage.Key);

        Assert.Multiple(() =>
        {
            Assert.That(draft?.Key, Is.EqualTo(Textpage.Key));
            Assert.That(draft?.IsDraft, Is.True);
            Assert.That(published?.Key, Is.EqualTo(Textpage.Key));
            Assert.That(published?.IsDraft, Is.False);
        });
    }

    [Test]
    public async Task GetDocumentSourceAsync_In_A_Scope_Sees_The_Uncommitted_Changes_Of_The_Scope()
    {
        await ContentPublishingService.PublishAsync(Textpage.Key, [new()], Constants.Security.SuperUserKey);

        using (var scope = ScopeProvider.CreateScope())
        {
            scope.Database.Execute(SqlContext.Sql()
                .Delete<ContentNuDto>()
                .Where<ContentNuDto>(x => x.NodeId == Textpage.Id && x.Published));

            Assert.IsNull(await DatabaseCacheRepository.GetDocumentSourceAsync(Textpage.Key));
        }

        // The scope was not completed, so the published source is back.
        Assert.IsNotNull(await DatabaseCacheRepository.GetDocumentSourceAsync(Textpage.Key));
    }

    [Test]
    public async Task GetElementSourceAsync_Without_An_Ambient_Scope_Returns_The_Published_Source()
    {
        Guid elementKey = await CreateAndPublishElement();

        ContentCacheNode? source = await DatabaseCacheRepository.GetElementSourceAsync(elementKey);

        Assert.IsNotNull(source);
        Assert.AreEqual(elementKey, source!.Key);
    }

    [Test]
    public async Task GetElementSource_Without_An_Ambient_Scope_Returns_The_Published_Source()
    {
        Guid elementKey = await CreateAndPublishElement();

        ContentCacheNode? source = DatabaseCacheRepository.GetElementSource(elementKey);

        Assert.IsNotNull(source);
        Assert.AreEqual(elementKey, source!.Key);
    }

    [Test]
    public async Task GetElementSourcesAsync_Without_An_Ambient_Scope_Returns_Matching_Sources()
    {
        Guid elementKey1 = await CreateAndPublishElement();
        Guid elementKey2 = await CreateAndPublishElement();

        var sources = (await DatabaseCacheRepository.GetElementSourcesAsync([elementKey1, elementKey2])).ToList();

        CollectionAssert.AreEquivalent(new[] { elementKey1, elementKey2 }, sources.Select(x => x.Key));
    }

    [Test]
    public async Task GetElementSourcesAsync_With_More_Keys_Than_The_Parameter_Limit_Returns_Matching_Sources()
    {
        Guid elementKey1 = await CreateAndPublishElement();
        Guid elementKey2 = await CreateAndPublishElement();

        // Unknown keys push the request past SQL Server's 2,100-parameter limit; the matching ones sit in different batches.
        var requestedKeys = new List<Guid> { elementKey1 };
        requestedKeys.AddRange(Enumerable.Range(0, Constants.Sql.MaxParameterCount + 100).Select(_ => Guid.NewGuid()));
        requestedKeys.Add(elementKey2);

        var sources = (await DatabaseCacheRepository.GetElementSourcesAsync(requestedKeys)).ToList();

        CollectionAssert.AreEquivalent(new[] { elementKey1, elementKey2 }, sources.Select(x => x.Key));
    }

    [Test]
    public async Task GetElementSourcesAsync_With_Empty_Keys_Returns_Nothing()
    {
        var sources = await DatabaseCacheRepository.GetElementSourcesAsync(Array.Empty<Guid>());

        Assert.That(sources, Is.Empty);
    }

    [Test]
    public async Task GetElementSourceForPublishStatesAsync_Without_An_Ambient_Scope_Returns_Both_States()
    {
        Guid elementKey = await CreateAndPublishElement();

        (ContentCacheNode? draft, ContentCacheNode? published) = await DatabaseCacheRepository.GetElementSourceForPublishStatesAsync(elementKey);

        Assert.Multiple(() =>
        {
            Assert.That(draft?.IsDraft, Is.True);
            Assert.That(published?.IsDraft, Is.False);
        });
    }

    [Test]
    public async Task GetElementSourceAsync_In_A_Scope_Sees_The_Uncommitted_Changes_Of_The_Scope()
    {
        Guid elementKey = await CreateAndPublishElement();
        ContentCacheNode? source = await DatabaseCacheRepository.GetElementSourceAsync(elementKey);
        int elementId = source!.Id;

        using (var scope = ScopeProvider.CreateScope())
        {
            scope.Database.Execute(SqlContext.Sql()
                .Delete<ContentNuDto>()
                .Where<ContentNuDto>(x => x.NodeId == elementId && x.Published));

            Assert.IsNull(await DatabaseCacheRepository.GetElementSourceAsync(elementKey));
        }

        // The scope was not completed, so the published source is back.
        Assert.IsNotNull(await DatabaseCacheRepository.GetElementSourceAsync(elementKey));
    }

    private async Task<Guid> CreateAndPublishElement()
    {
        IContentType elementType = ContentTypeBuilder.CreateBasicElementType(Guid.NewGuid().ToString("N"));
        elementType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        var createResult = await ElementEditingService.CreateAsync(
            new ElementCreateModel { ContentTypeKey = elementType.Key, Variants = [new() { Name = "Element" }] },
            Constants.Security.SuperUserKey);
        Assert.That(createResult.Success, Is.True);
        Guid elementKey = createResult.Result.Content!.Key;

        var publishResult = await ElementPublishingService.PublishAsync(
            elementKey,
            [new CulturePublishScheduleModel { Culture = "*", Schedule = null }],
            Constants.Security.SuperUserKey);
        Assert.That(publishResult.Success, Is.True);

        return elementKey;
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureSmallDeleteBatchSize))]
    public void Rebuild_Deletes_Stale_Rows_In_Batches_And_Repopulates()
    {
        // Guard: the [ConfigureBuilder] override must be in effect, otherwise the default (large) batch size
        // would delete every row in one statement and this test would no longer exercise the batching loop.
        Assert.That(GetRequiredService<IOptions<NuCacheSettings>>().Value.ContentTypeRebuildDeleteBatchSize, Is.EqualTo(2));

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

    /// <summary>
    ///     Verifies that a cache row saved between the rebuild deleting a content type's rows and re-inserting them
    ///     is kept, rather than overwritten by the rebuild's older data.
    /// </summary>
    /// <remarks>
    ///     The rebuild re-inserts each page of rows with a single bulk insert. That bulk insert only fails when a row
    ///     with the same node id and published flag already exists, which happens when a foreground save writes the
    ///     row after the rebuild has deleted it. On that primary key violation, the rebuild falls back to
    ///     <c>DatabaseCacheRepository.InsertNewRows</c>, which inserts each row with <c>INSERT ... WHERE NOT EXISTS</c>,
    ///     so it skips the saved row and inserts the rest of the page.
    /// </remarks>
    [Test]
    public void Rebuild_Does_Not_Overwrite_Row_Saved_During_Rebuild()
        => AssertRebuildDoesNotOverwriteRowSavedDuringRebuild();

    /// <summary>
    ///     Verifies that a cache row saved during the rebuild is kept when the rows the rebuild inserts have no
    ///     binary data, so the fallback in <c>DatabaseCacheRepository.InsertNewRows</c> has a null value to insert.
    /// </summary>
    /// <remarks>
    ///     The JSON serializer stores no binary data, so the rebuilt rows leave the binary column empty. The fallback
    ///     leaves null columns out of its insert, because some providers cannot convert an untyped null parameter to
    ///     the binary column type.
    /// </remarks>
    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureJsonSerializer))]
    public void Rebuild_Does_Not_Overwrite_Row_Saved_During_Rebuild_When_Binary_Data_Is_Null()
        => AssertRebuildDoesNotOverwriteRowSavedDuringRebuild();

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

    private void AssertRebuildDoesNotOverwriteRowSavedDuringRebuild()
    {
        DocumentCacheService.Rebuild([ContentType.Id]);

        const string foregroundMarker = "FOREGROUND";
        var contentTypeNodeIds = new[] { Textpage.Id, Subpage.Id, Subpage2.Id, Subpage3.Id };
        var written = false;
        using (var scope = ScopeProvider.CreateScope())
        {
            IUmbracoDatabase database = ScopeAccessor.AmbientScope!.Database;
            DatabaseCacheRepository.Rebuild([ContentType.Id], null, null, step =>
            {
                // A save between the rebuild's delete and its inserts writes a row the rebuild then conflicts with.
                if (written is false && CountCacheRows(database, Textpage.Id) == 0)
                {
                    database.Insert(new ContentNuDto { NodeId = Textpage.Id, Published = false, Data = foregroundMarker, Rv = 1 });
                    written = true;
                }

                step();
            });
            scope.Complete();
        }

        using (var scope = ScopeProvider.CreateScope(autoComplete: true))
        {
            List<ContentNuDto> dtos = ScopeAccessor.AmbientScope!.Database.Fetch<ContentNuDto>(
                SqlContext.Sql()
                    .Select<ContentNuDto>()
                    .From<ContentNuDto>()
                    .WhereIn<ContentNuDto>(x => x.NodeId, contentTypeNodeIds));

            Assert.Multiple(() =>
            {
                Assert.That(written, Is.True, "The rebuild never ran a step after deleting the rows.");
                Assert.That(dtos.Select(x => x.NodeId).Distinct(), Is.EquivalentTo(contentTypeNodeIds));
                Assert.That(dtos.Single(x => x.NodeId == Textpage.Id && x.Published is false).Data, Is.EqualTo(foregroundMarker));
            });
        }
    }

    private int CountCacheRows(IUmbracoDatabase database, int nodeId)
        => database.ExecuteScalar<int>(
            SqlContext.Sql()
                .SelectCount()
                .From<ContentNuDto>()
                .Where<ContentNuDto>(x => x.NodeId == nodeId));
}
