// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Scoping;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, PublishedRepositoryEvents = true)]
internal sealed class MediaCacheServiceTests : UmbracoIntegrationTestWithContent
{
    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddUmbracoHybridCache();
        builder.Services.AddUnique<IServerMessenger, ScopedRepositoryTests.LocalServerMessenger>();

        // Use JSON to allow easier verification of data.
        builder.Services.PostConfigure<NuCacheSettings>(options => options.NuCacheSerializerType = NuCacheSerializerType.JSON);
    }

    private ISqlContext SqlContext => GetRequiredService<ISqlContext>();

    private IMediaCacheService MediaCacheService => GetRequiredService<IMediaCacheService>();

    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private IMediaService MediaService => GetRequiredService<IMediaService>();

    private IMediaType MediaType { get; set; }

    private IMedia MediaItem { get; set; }

    public override void CreateTestData()
    {
        base.CreateTestData();

        MediaType = MediaTypeService.Get("image")!;

        // Create and Save Media "MediaItem" based on "image" media type
        MediaItem = new MediaBuilder()
            .WithName("Test Media Item")
            .WithMediaType(MediaType)
            .Build();
        MediaService.Save(MediaItem);
    }

    [Test]
    public void Rebuild_Creates_Media_Database_Cache_Records_For_Media_Type()
    {
        // Arrange - media is created in Setup()

        // Act - Call Rebuild for the media type
        MediaCacheService.Rebuild([MediaType.Id]);

        // Assert - Verify cmsContentNu table has records for the media item
        using (var scope = ScopeProvider.CreateScope(autoComplete: true))
        {
            var selectSql = SqlContext.Sql()
                .Select<ContentNuDto>()
                .From<ContentNuDto>();

            var dtos = ScopeAccessor.AmbientScope!.Database.Fetch<ContentNuDto>(selectSql);

            // Verify specific media items have cache entries
            var nodeIds = dtos.Select(d => d.NodeId).ToList();
            Assert.That(nodeIds, Does.Contain(MediaItem.Id), "MediaItem should have cache entry");

            // Verify cache data is not empty
            var mediaItemDto = dtos.First(d => d.NodeId == MediaItem.Id);
            Assert.That(mediaItemDto.Data, Is.Not.Null.And.Not.Empty, "Cache data should not be empty");

            // Verify the cached data is correct across refactorings and optimizations.
            const string ExpectedJson = "{\"pd\":{\"umbracoFile\":[],\"umbracoWidth\":[],\"umbracoHeight\":[],\"umbracoBytes\":[],\"umbracoExtension\":[]},\"cd\":{},\"us\":\"test-media-item\"}";
            Assert.That(mediaItemDto.Data, Is.EqualTo(ExpectedJson), "Cache data does not match expected JSON");
        }
    }

    [Test]
    public void Rebuild_Replaces_Existing_Media_Database_Cache_Records()
    {
        // Arrange - First rebuild to create initial records
        MediaCacheService.Rebuild([MediaType.Id]);

        // Get initial data
        string initialData;
        using (var scope = ScopeProvider.CreateScope(autoComplete: true))
        {
            var selectSql = SqlContext.Sql()
                .Select<ContentNuDto>()
                .From<ContentNuDto>()
                .Where<ContentNuDto>(x => x.NodeId == MediaItem.Id);
            var dto = ScopeAccessor.AmbientScope!.Database.Fetch<ContentNuDto>(selectSql).FirstOrDefault();
            Assert.That(dto, Is.Not.Null);
            initialData = dto!.Data!;
        }

        // Modify content
        MediaItem.Name += " - Modified";
        MediaService.Save(MediaItem);

        // Act - Rebuild again
        MediaCacheService.Rebuild([MediaType.Id]);

        // Assert - Verify record was updated (not duplicated)
        using (var scope = ScopeProvider.CreateScope(autoComplete: true))
        {
            var selectSql = SqlContext.Sql()
                .Select<ContentNuDto>()
                .From<ContentNuDto>()
                .Where<ContentNuDto>(x => x.NodeId == MediaItem.Id);
            var dtos = ScopeAccessor.AmbientScope!.Database.Fetch<ContentNuDto>(selectSql);

            // Should have exactly one draft record (no duplicates)
            Assert.That(dtos, Has.Count.EqualTo(1), "Should have exactly one draft cache record");

            // Data should be different (updated)
            var updatedData = dtos[0].Data;
            Assert.That(updatedData, Does.Contain("test-media-item-modified"), "Cache data should contain the modified name");
        }
    }

    [Test]
    public async Task Rebuild_Includes_Composed_Properties_In_Cache()
    {
        // Arrange - Create a composition media type with a custom property
        var compositionType = new MediaTypeBuilder()
            .WithAlias("mediaComposition")
            .WithName("Media Composition")
            .AddPropertyGroup()
                .WithName("Metadata")
                .WithAlias("metadata")
                .WithSortOrder(1)
                .AddPropertyType()
                    .WithPropertyEditorAlias(Cms.Core.Constants.PropertyEditors.Aliases.TextBox)
                    .WithValueStorageType(ValueStorageType.Nvarchar)
                    .WithAlias("altText")
                    .WithName("Alt Text")
                    .WithSortOrder(1)
                    .Done()
                .Done()
            .Build();
        await MediaTypeService.CreateAsync(compositionType, Constants.Security.SuperUserKey);

        // Create a media type that uses the composition
        var composedMediaType = new MediaTypeBuilder()
            .WithAlias("composedImage")
            .WithName("Composed Image")
            .WithMediaPropertyGroup()
            .AddPropertyGroup()
                .WithName("Extra")
                .WithAlias("extra")
                .WithSortOrder(2)
                .AddPropertyType()
                    .WithPropertyEditorAlias(Cms.Core.Constants.PropertyEditors.Aliases.TextBox)
                    .WithValueStorageType(ValueStorageType.Nvarchar)
                    .WithAlias("caption")
                    .WithName("Caption")
                    .WithSortOrder(1)
                    .Done()
                .Done()
            .Build();

        // Add the composition to the media type
        composedMediaType.AddContentType(compositionType);
        await MediaTypeService.CreateAsync(composedMediaType, Constants.Security.SuperUserKey);

        // Create media using the composed type
        var composedMedia = new MediaBuilder()
            .WithName("Composed Media Item")
            .WithMediaType(composedMediaType)
            .Build();
        MediaService.Save(composedMedia);

        // Act - Rebuild the cache for the composed media type
        MediaCacheService.Rebuild([composedMediaType.Id]);

        // Assert - Verify the cache includes properties from both the media type AND its composition
        using (var scope = ScopeProvider.CreateScope(autoComplete: true))
        {
            var selectSql = SqlContext.Sql()
                .Select<ContentNuDto>()
                .From<ContentNuDto>()
                .Where<ContentNuDto>(x => x.NodeId == composedMedia.Id);

            var dto = ScopeAccessor.AmbientScope!.Database.Fetch<ContentNuDto>(selectSql).FirstOrDefault();
            Assert.That(dto, Is.Not.Null, "Composed media should have a cache entry");
            Assert.That(dto!.Data, Is.Not.Null.And.Not.Empty, "Cache data should not be empty");

            // Verify the cached data includes properties from the composition (altText)
            Assert.That(dto.Data, Does.Contain("\"altText\":[]"), "Cache should include altText from composition");

            // Verify the cached data includes direct properties (caption)
            Assert.That(dto.Data, Does.Contain("\"caption\":[]"), "Cache should include caption from direct type");

            // Verify the cached data includes the media property group properties
            Assert.That(dto.Data, Does.Contain("\"umbracoFile\":[]"), "Cache should include umbracoFile from media property group");
        }
    }
}
