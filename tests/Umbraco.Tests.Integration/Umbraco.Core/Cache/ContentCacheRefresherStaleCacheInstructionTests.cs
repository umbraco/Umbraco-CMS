using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Scoping;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Cache;

/// <summary>
/// Tests that cache refreshers handle stale cache instructions gracefully.
/// This can happen when:
/// 1. Content is saved (cache instruction added to database)
/// 2. Content is deleted
/// 3. Server restarts and processes the stale cache instruction
/// The cache refresher should not throw when the content no longer exists.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class ContentCacheRefresherStaleCacheInstructionTests : UmbracoIntegrationTest
{
    private IContentService ContentService => GetRequiredService<IContentService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private ContentCacheRefresher ContentCacheRefresher => GetRequiredService<ContentCacheRefresher>();

    private MediaCacheRefresher MediaCacheRefresher => GetRequiredService<MediaCacheRefresher>();

    private IMediaService MediaService => GetRequiredService<IMediaService>();

    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private IFileService FileService => GetRequiredService<IFileService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.Services.AddUnique<IServerMessenger, ScopedRepositoryTests.LocalServerMessenger>();
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.AddNotificationHandler<MediaTreeChangeNotification, MediaTreeChangeDistributedCacheNotificationHandler>();
    }

    private IContent CreateAndDeleteContent(string alias)
    {
        var template = TemplateBuilder.CreateTextPageTemplate(alias + "Template");
        FileService.SaveTemplate(template);

        var contentType = ContentTypeBuilder.CreateSimpleContentType(alias, alias, defaultTemplateId: template.Id);
        ContentTypeService.Save(contentType);

        var content = ContentBuilder.CreateSimpleContent(contentType, "Test Content");
        ContentService.Save(content);

        // Delete the content (simulating what happens before server restart)
        ContentService.Delete(content);

        return content;
    }

    /// <summary>
    /// Simulates the scenario where a RefreshBranch cache instruction is processed
    /// for content that has been deleted. This can happen when:
    /// 1. Content is saved/published (RefreshBranch instruction queued)
    /// 2. Content is deleted before the instruction is processed
    /// 3. Server processes the stale instruction on restart
    /// </summary>
    [Test]
    public void ContentCacheRefresher_Handles_Stale_RefreshBranch_Instruction_For_Deleted_Content()
    {
        // Arrange - Create and delete content
        var content = CreateAndDeleteContent("testPage");

        // Act - Simulate processing a stale RefreshBranch cache instruction
        // This is what happens when the server restarts and processes queued instructions
        var stalePayload = new ContentCacheRefresher.JsonPayload
        {
            Id = content.Id,
            Key = content.Key,
            ChangeTypes = TreeChangeTypes.RefreshBranch,
            Blueprint = false,
            PublishedCultures = ["en-US"],
            UnpublishedCultures = null
        };

        // Assert - Should not throw when processing instruction for deleted content
        Assert.DoesNotThrow(() => ContentCacheRefresher.Refresh([stalePayload]));
    }

    /// <summary>
    /// Simulates the scenario where a RefreshNode cache instruction is processed
    /// for content that has been deleted.
    /// </summary>
    [Test]
    public void ContentCacheRefresher_Handles_Stale_RefreshNode_Instruction_For_Deleted_Content()
    {
        // Arrange - Create and delete content
        var content = CreateAndDeleteContent("testPage2");

        // Act - Simulate processing a stale RefreshNode cache instruction
        var stalePayload = new ContentCacheRefresher.JsonPayload
        {
            Id = content.Id,
            Key = content.Key,
            ChangeTypes = TreeChangeTypes.RefreshNode,
            Blueprint = false,
            PublishedCultures = ["en-US"],
            UnpublishedCultures = null
        };

        // Assert - Should not throw
        Assert.DoesNotThrow(() => ContentCacheRefresher.Refresh([stalePayload]));
    }

    /// <summary>
    /// Simulates the scenario where a RefreshBranch cache instruction is processed
    /// for media that has been deleted.
    /// </summary>
    [Test]
    public void MediaCacheRefresher_Handles_Stale_RefreshBranch_Instruction_For_Deleted_Media()
    {
        // Arrange - Create media
        var mediaType = MediaTypeBuilder.CreateSimpleMediaType("testMediaType", "Test Media Type");
        MediaTypeService.Save(mediaType);

        var media = MediaBuilder.CreateSimpleMedia(mediaType, "Test Media", -1);
        MediaService.Save(media);

        var mediaKey = media.Key;
        var mediaId = media.Id;

        // Delete the media
        MediaService.Delete(media);

        // Act - Simulate processing a stale RefreshBranch cache instruction
        var stalePayload = new MediaCacheRefresher.JsonPayload(mediaId, mediaKey, TreeChangeTypes.RefreshBranch);

        // Assert - Should not throw
        Assert.DoesNotThrow(() => MediaCacheRefresher.Refresh([stalePayload]));
    }

    /// <summary>
    /// Simulates processing a cache instruction for a content key that never existed.
    /// This is an edge case but should still be handled gracefully.
    /// </summary>
    [Test]
    public void ContentCacheRefresher_Handles_Instruction_For_NonExistent_Content()
    {
        // Arrange - Use a random key that never existed
        var nonExistentKey = Guid.NewGuid();
        var stalePayload = new ContentCacheRefresher.JsonPayload
        {
            Id = 999999,
            Key = nonExistentKey,
            ChangeTypes = TreeChangeTypes.RefreshBranch,
            Blueprint = false,
            PublishedCultures = ["en-US"],
            UnpublishedCultures = null
        };

        // Act & Assert - Should not throw
        Assert.DoesNotThrow(() => ContentCacheRefresher.Refresh([stalePayload]));
    }
}
