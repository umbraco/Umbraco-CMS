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
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

/// <summary>
/// Tests that the HybridCache gracefully handles an inconsistent database state where
/// umbracoDocument.published = 1 but no umbracoDocumentVersion row has published = 1.
/// See https://github.com/umbraco/Umbraco-CMS/issues/22293.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class DocumentHybridCacheInconsistentStateTests : UmbracoIntegrationTestWithContent
{
    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
    }

    private IPublishedContentCache PublishedContentCache => GetRequiredService<IPublishedContentCache>();

    private IContentPublishingService ContentPublishingService => GetRequiredService<IContentPublishingService>();

    [Test]
    public async Task GetByIdAsync_Returns_Null_For_Node_With_Inconsistent_Published_State()
    {
        // Arrange: Publish the Textpage, then corrupt its published version flag.
        await ContentPublishingService.PublishAsync(Textpage.Key, [new()], Constants.Security.SuperUserKey);
        await CorruptPublishedVersionFlag(Textpage.Id, Textpage.Key);

        // Act: Request the published content — should NOT throw ArgumentNullException.
        var publishedContent = await PublishedContentCache.GetByIdAsync(Textpage.Key);

        // Assert: Should return null (gracefully skip the corrupt node), not crash.
        Assert.That(publishedContent, Is.Null);
    }

    [Test]
    public async Task GetByIdAsync_Draft_Still_Works_For_Node_With_Inconsistent_Published_State()
    {
        // Arrange: Publish the Textpage, then corrupt its published version flag.
        await ContentPublishingService.PublishAsync(Textpage.Key, [new()], Constants.Security.SuperUserKey);
        await CorruptPublishedVersionFlag(Textpage.Id, Textpage.Key);

        // Act: Request the draft content — draft path should be unaffected.
        var draftContent = await PublishedContentCache.GetByIdAsync(Textpage.Key, true);

        // Assert: Draft should still be accessible.
        Assert.That(draftContent, Is.Not.Null);
        Assert.That(draftContent!.Key, Is.EqualTo(Textpage.Key));
    }

    [Test]
    public async Task Other_Published_Nodes_Unaffected_By_Single_Corrupt_Node()
    {
        // Arrange: Publish both Textpage and Subpage, then corrupt only Subpage.
        await ContentPublishingService.PublishBranchAsync(
            Textpage.Key,
            [],
            PublishBranchFilter.All,
            Constants.Security.SuperUserKey,
            false);

        // Verify Subpage is published before corrupting.
        var subpageBefore = await PublishedContentCache.GetByIdAsync(Subpage.Key);
        Assert.That(subpageBefore, Is.Not.Null, "Subpage should be published before corruption");

        await CorruptPublishedVersionFlag(Subpage.Id, Subpage.Key);

        // Act: Request a different published node (Textpage) — should work fine.
        var textPage = await PublishedContentCache.GetByIdAsync(Textpage.Key);

        // Assert: Textpage is unaffected by Subpage's corrupt state.
        Assert.That(textPage, Is.Not.Null);
        Assert.That(textPage!.Key, Is.EqualTo(Textpage.Key));
    }

    [Test]
    public async Task GetByContentType_Skips_Corrupt_Node_Without_Crashing()
    {
        // Arrange: Publish both Textpage and Subpage, then corrupt only Textpage.
        await ContentPublishingService.PublishBranchAsync(
            Textpage.Key,
            [],
            PublishBranchFilter.All,
            Constants.Security.SuperUserKey,
            false);

        await CorruptPublishedVersionFlag(Textpage.Id, Textpage.Key);

        // Act: GetContentByContentTypeKey iterates all nodes of a content type via
        // CreateContentNodeKit. A corrupt node should not crash the entire enumeration.
        var databaseCacheRepository = GetRequiredService<IDatabaseCacheRepository>();

        // Assert: Should not throw — corrupt node should be skipped.
        Assert.DoesNotThrow(() =>
        {
            using var scope = ScopeProvider.CreateScope(autoComplete: true);
            var nodes = databaseCacheRepository.GetContentByContentTypeKey(
                [ContentType.Key],
                ContentCacheDataSerializerEntityType.Document);

            // Force full enumeration (it uses yield return).
            _ = nodes.ToList();
        });
    }

    /// <summary>
    /// Publishes the Textpage and then corrupts the database state by clearing the published flag
    /// from all umbracoDocumentVersion rows for that node, while leaving umbracoDocument.published = 1.
    /// This reproduces the exact inconsistency described in issue #22293.
    /// </summary>
    private async Task CorruptPublishedVersionFlag(int nodeId, Guid nodeKey)
    {
        // Clear the HybridCache so the next request hits the database.
        var hybridCache = GetRequiredService<Microsoft.Extensions.Caching.Hybrid.HybridCache>();
        await hybridCache.RemoveAsync($"{nodeKey}");
        await hybridCache.RemoveAsync($"{nodeKey}+draft");

        // Corrupt: set published = 0 on all DocumentVersion rows for this node,
        // leaving umbracoDocument.published = 1 (orphaned published flag).
        using (var scope = ScopeProvider.CreateScope(autoComplete: true))
        {
            var db = ScopeAccessor.AmbientScope!.Database;
            var sql = ScopeAccessor.AmbientScope.SqlContext.Sql();

            // Find the version ID(s) that are currently marked as published.
            var findSql = sql
                .Select<DocumentVersionDto>(x => x.Id)
                .From<DocumentVersionDto>()
                .InnerJoin<ContentVersionDto>()
                .On<DocumentVersionDto, ContentVersionDto>((dv, cv) => dv.Id == cv.Id)
                .Where<ContentVersionDto>(x => x.NodeId == nodeId)
                .Where<DocumentVersionDto>(x => x.Published == true);

            var publishedVersionIds = db.Fetch<int>(findSql);

            // Verify we had a published version before corrupting.
            Assert.That(
                publishedVersionIds,
                Has.Count.GreaterThan(0),
                "Expected at least one published version before corruption");

            // Clear the published flag on those versions
            foreach (var versionId in publishedVersionIds)
            {
                var updateSql = ScopeAccessor.AmbientScope.SqlContext.Sql()
                    .Update<DocumentVersionDto>(u => u.Set(x => x.Published, false))
                    .Where<DocumentVersionDto>(x => x.Id == versionId);

                db.Execute(updateSql);
            }

            // Verify the corruption: umbracoDocument.published should still be 1
            var documentSql = ScopeAccessor.AmbientScope.SqlContext.Sql()
                .Select<DocumentDto>(x => x.Published)
                .From<DocumentDto>()
                .Where<DocumentDto>(x => x.NodeId == nodeId);

            var isPublished = db.ExecuteScalar<bool>(documentSql);
            Assert.That(
                isPublished,
                Is.True,
                "umbracoDocument.published should still be 1 after corruption");

            // Verify no published version exists
            var checkSql = ScopeAccessor.AmbientScope.SqlContext.Sql()
                .SelectCount()
                .From<DocumentVersionDto>()
                .InnerJoin<ContentVersionDto>()
                .On<DocumentVersionDto, ContentVersionDto>((dv, cv) => dv.Id == cv.Id)
                .Where<ContentVersionDto>(x => x.NodeId == nodeId)
                .Where<DocumentVersionDto>(x => x.Published == true);

            var publishedCount = db.ExecuteScalar<int>(checkSql);
            Assert.That(
                publishedCount,
                Is.EqualTo(0),
                "No umbracoDocumentVersion should have published = 1 after corruption");
        }

        // Clear cache again to ensure the corrupted state is read fresh from DB
        await hybridCache.RemoveAsync($"{nodeKey}");
        await hybridCache.RemoveAsync($"{nodeKey}+draft");
    }
}
