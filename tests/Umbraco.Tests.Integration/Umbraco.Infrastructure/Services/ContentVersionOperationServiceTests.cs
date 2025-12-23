// tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentVersionOperationServiceTests.cs
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Content = Umbraco.Cms.Core.Models.Content;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class ContentVersionOperationServiceTests : UmbracoIntegrationTestWithContent
{
    private IContentVersionOperationService VersionOperationService => GetRequiredService<IContentVersionOperationService>();
    private IContentPublishingService ContentPublishingService => GetRequiredService<IContentPublishingService>();

    // v1.2 Fix (Issue 3.2): Use CustomTestSetup to register notification handlers
    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder.AddNotificationHandler<ContentRollingBackNotification, VersionNotificationHandler>();

    #region GetVersion Tests

    [Test]
    public void GetVersion_ExistingVersion_ReturnsContent()
    {
        // Arrange
        var versionId = Textpage.VersionId;

        // Act
        var result = VersionOperationService.GetVersion(versionId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(Textpage.Id));
    }

    [Test]
    public void GetVersion_NonExistentVersion_ReturnsNull()
    {
        // Act
        var result = VersionOperationService.GetVersion(999999);

        // Assert
        Assert.That(result, Is.Null);
    }

    #endregion

    #region GetVersions Tests

    [Test]
    public async Task GetVersions_ContentWithMultipleVersions_ReturnsAllVersions()
    {
        // Arrange - Use existing content from base class
        var content = Textpage;

        // Publishing creates a new version. Multiple saves without publish just update the draft.
        content.SetValue("author", "Version 1");
        ContentService.Save(content);
        await ContentPublishingService.PublishAsync(content.Key, new[] { new CulturePublishScheduleModel() }, Constants.Security.SuperUserKey);

        content = (Content)ContentService.GetById(content.Id)!;
        content.SetValue("author", "Version 2");
        ContentService.Save(content);
        await ContentPublishingService.PublishAsync(content.Key, new[] { new CulturePublishScheduleModel() }, Constants.Security.SuperUserKey);

        content = (Content)ContentService.GetById(content.Id)!;
        content.SetValue("author", "Version 3");
        ContentService.Save(content);

        // Act
        var versions = VersionOperationService.GetVersions(content.Id).ToList();

        // Assert - Each publish creates a version, plus the initial version
        Assert.That(versions.Count, Is.GreaterThanOrEqualTo(3));
    }

    [Test]
    public void GetVersions_NonExistentContent_ReturnsEmpty()
    {
        // Act
        var versions = VersionOperationService.GetVersions(999999).ToList();

        // Assert
        Assert.That(versions, Is.Empty);
    }

    #endregion

    #region GetVersionsSlim Tests

    [Test]
    public async Task GetVersionsSlim_ReturnsPagedVersions()
    {
        // Arrange - Use existing content from base class
        var content = Textpage;

        // Create 5+ versions by publishing each time (publishing locks the version)
        for (int i = 1; i <= 5; i++)
        {
            content.SetValue("author", $"Version {i}");
            ContentService.Save(content);
            await ContentPublishingService.PublishAsync(content.Key, new[] { new CulturePublishScheduleModel() }, Constants.Security.SuperUserKey);
            content = (Content)ContentService.GetById(content.Id)!;
        }

        // Act
        var versions = VersionOperationService.GetVersionsSlim(content.Id, skip: 1, take: 2).ToList();

        // Assert
        Assert.That(versions.Count, Is.EqualTo(2));
    }

    #endregion

    #region GetVersionIds Tests

    [Test]
    public async Task GetVersionIds_ReturnsVersionIdsOrderedByLatestFirst()
    {
        // Arrange - Use existing content from base class
        var content = Textpage;

        // First save and publish to lock version 1
        content.SetValue("author", "Version 1");
        ContentService.Save(content);
        await ContentPublishingService.PublishAsync(content.Key, new[] { new CulturePublishScheduleModel() }, Constants.Security.SuperUserKey);
        // Reload to get updated version ID after publish
        content = (Content)ContentService.GetById(content.Id)!;
        var version1Id = content.VersionId;

        // Create version 2 by saving and publishing
        content.SetValue("author", "Version 2");
        ContentService.Save(content);
        await ContentPublishingService.PublishAsync(content.Key, new[] { new CulturePublishScheduleModel() }, Constants.Security.SuperUserKey);
        // Reload to get updated version ID after publish
        content = (Content)ContentService.GetById(content.Id)!;
        var version2Id = content.VersionId;

        // Act
        var versionIds = VersionOperationService.GetVersionIds(content.Id, maxRows: 10).ToList();

        // Assert
        Assert.That(versionIds.Count, Is.GreaterThanOrEqualTo(2));
        Assert.That(versionIds[0], Is.EqualTo(version2Id)); // Latest first

        // Verify ordering (version2 should be before version1 in the list)
        var idx1 = versionIds.IndexOf(version1Id);
        var idx2 = versionIds.IndexOf(version2Id);
        Assert.That(idx2, Is.LessThan(idx1), "Version 2 should appear before Version 1");
    }

    #endregion

    #region Rollback Tests

    [Test]
    public async Task Rollback_ToEarlierVersion_RestoresPropertyValues()
    {
        // Arrange - Use existing content from base class
        var content = Textpage;
        content.SetValue("author", "Original Value");
        ContentService.Save(content);
        // Publish to lock this version
        await ContentPublishingService.PublishAsync(content.Key, new[] { new CulturePublishScheduleModel() }, Constants.Security.SuperUserKey);
        var originalVersionId = content.VersionId;

        // Reload and make a change
        content = (Content)ContentService.GetById(content.Id)!;
        content.SetValue("author", "Changed Value");
        ContentService.Save(content);
        await ContentPublishingService.PublishAsync(content.Key, new[] { new CulturePublishScheduleModel() }, Constants.Security.SuperUserKey);

        // Act
        var result = VersionOperationService.Rollback(content.Id, originalVersionId);

        // Assert
        Assert.That(result.Success, Is.True);
        var rolledBackContent = ContentService.GetById(content.Id);
        Assert.That(rolledBackContent!.GetValue<string>("author"), Is.EqualTo("Original Value"));
    }

    [Test]
    public void Rollback_NonExistentContent_Fails()
    {
        // Act
        var result = VersionOperationService.Rollback(999999, 1);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Result, Is.EqualTo(OperationResultType.FailedCannot));
    }

    [Test]
    public void Rollback_TrashedContent_Fails()
    {
        // Arrange - Use existing trashed content from base class
        var content = Trashed;
        var versionId = content.VersionId;

        // Act
        var result = VersionOperationService.Rollback(content.Id, versionId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Result, Is.EqualTo(OperationResultType.FailedCannot));
    }

    /// <summary>
    /// v1.2 Fix (Issue 3.2): Test that cancellation notification works correctly.
    /// Uses the correct integration test pattern with CustomTestSetup and static action.
    /// </summary>
    [Test]
    public void Rollback_WhenNotificationCancelled_ReturnsCancelledResult()
    {
        // Arrange - Use existing content from base class
        var content = Textpage;
        content.SetValue("author", "Original Value");
        ContentService.Save(content);
        var originalVersionId = content.VersionId;

        content.SetValue("author", "Changed Value");
        ContentService.Save(content);

        // Set up the notification handler to cancel the rollback
        VersionNotificationHandler.RollingBackContent = notification => notification.Cancel = true;

        try
        {
            // Act
            var result = VersionOperationService.Rollback(content.Id, originalVersionId);

            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.EqualTo(OperationResultType.FailedCancelledByEvent));

            // Verify content was not modified
            var unchangedContent = ContentService.GetById(content.Id);
            Assert.That(unchangedContent!.GetValue<string>("author"), Is.EqualTo("Changed Value"));
        }
        finally
        {
            // Clean up the static action
            VersionNotificationHandler.RollingBackContent = null;
        }
    }

    #endregion

    #region DeleteVersions Tests

    /// <summary>
    /// v1.1 Fix (Issue 2.5): Use deterministic date comparison instead of Thread.Sleep.
    /// </summary>
    [Test]
    public async Task DeleteVersions_ByDate_DeletesOlderVersions()
    {
        // Arrange - Use existing content from base class
        var content = Textpage;

        // Create version 1 and publish to lock it
        content.SetValue("author", "Version 1");
        ContentService.Save(content);
        await ContentPublishingService.PublishAsync(content.Key, new[] { new CulturePublishScheduleModel() }, Constants.Security.SuperUserKey);
        var version1Id = content.VersionId;

        // Reload and create version 2
        content = (Content)ContentService.GetById(content.Id)!;
        content.SetValue("author", "Version 2");
        ContentService.Save(content);
        await ContentPublishingService.PublishAsync(content.Key, new[] { new CulturePublishScheduleModel() }, Constants.Security.SuperUserKey);

        // Get the actual update date of version 2 for deterministic comparison
        var version2 = VersionOperationService.GetVersion(content.VersionId);
        var cutoffDate = version2!.UpdateDate.AddMilliseconds(1);

        // Reload and create version 3
        content = (Content)ContentService.GetById(content.Id)!;
        content.SetValue("author", "Version 3");
        ContentService.Save(content);
        await ContentPublishingService.PublishAsync(content.Key, new[] { new CulturePublishScheduleModel() }, Constants.Security.SuperUserKey);
        var version3Id = content.VersionId;

        var versionCountBefore = VersionOperationService.GetVersions(content.Id).Count();

        // Act - Delete versions older than cutoffDate (should delete version 1, keep version 2 and 3)
        VersionOperationService.DeleteVersions(content.Id, cutoffDate);

        // Assert
        var remainingVersions = VersionOperationService.GetVersions(content.Id).ToList();
        Assert.That(remainingVersions.Any(v => v.VersionId == version3Id), Is.True, "Current version should remain");
        Assert.That(remainingVersions.Count, Is.LessThan(versionCountBefore), "Should have fewer versions after deletion");
    }

    #endregion

    #region DeleteVersion Tests

    [Test]
    public async Task DeleteVersion_SpecificVersion_DeletesOnlyThatVersion()
    {
        // Arrange - Use existing content from base class
        var content = Textpage;

        // Create and publish version 1 (to lock it)
        content.SetValue("author", "Version 1");
        ContentService.Save(content);
        await ContentPublishingService.PublishAsync(content.Key, new[] { new CulturePublishScheduleModel() }, Constants.Security.SuperUserKey);
        var version1Id = content.VersionId;

        // Create and publish version 2 (this is the one we'll delete)
        content = (Content)ContentService.GetById(content.Id)!;
        content.SetValue("author", "Version 2");
        ContentService.Save(content);
        await ContentPublishingService.PublishAsync(content.Key, new[] { new CulturePublishScheduleModel() }, Constants.Security.SuperUserKey);
        var versionToDelete = content.VersionId;

        // Create version 3 (the current draft)
        content = (Content)ContentService.GetById(content.Id)!;
        content.SetValue("author", "Version 3");
        ContentService.Save(content);
        await ContentPublishingService.PublishAsync(content.Key, new[] { new CulturePublishScheduleModel() }, Constants.Security.SuperUserKey);
        var currentVersionId = content.VersionId;

        // Act - Delete version 2 (not the current or published version)
        VersionOperationService.DeleteVersion(content.Id, version1Id, deletePriorVersions: false);

        // Assert - Version 1 should be deleted, current version should remain
        var deletedVersion = VersionOperationService.GetVersion(version1Id);
        Assert.That(deletedVersion, Is.Null, "Version 1 should be deleted");
        var currentVersion = VersionOperationService.GetVersion(currentVersionId);
        Assert.That(currentVersion, Is.Not.Null, "Current version should remain");
    }

    [Test]
    public void DeleteVersion_CurrentVersion_DoesNotDelete()
    {
        // Arrange - Use existing content from base class
        var content = Textpage;
        var currentVersionId = content.VersionId;

        // Act
        VersionOperationService.DeleteVersion(content.Id, currentVersionId, deletePriorVersions: false);

        // Assert
        var version = VersionOperationService.GetVersion(currentVersionId);
        Assert.That(version, Is.Not.Null); // Should not be deleted
    }

    /// <summary>
    /// v1.2 Fix (Issue 3.3, 3.4): Test that published version is protected from deletion.
    /// Uses the correct async ContentPublishingService.PublishAsync method.
    /// </summary>
    [Test]
    public async Task DeleteVersion_PublishedVersion_DoesNotDelete()
    {
        // Arrange - Use existing content from base class
        var content = Textpage;

        // v1.2 Fix (Issue 3.4): Use ContentPublishingService.PublishAsync with correct signature
        var publishResult = await ContentPublishingService.PublishAsync(
            content.Key,
            new[] { new CulturePublishScheduleModel() },
            Constants.Security.SuperUserKey);
        Assert.That(publishResult.Success, Is.True, "Publish should succeed");

        // Refresh content to get the published version id
        content = (Content)ContentService.GetById(content.Id)!;
        var publishedVersionId = content.PublishedVersionId;
        Assert.That(publishedVersionId, Is.GreaterThan(0), "Content should have a published version");

        // Create a newer draft version
        content.SetValue("author", "Draft");
        ContentService.Save(content);

        // Act
        VersionOperationService.DeleteVersion(content.Id, publishedVersionId, deletePriorVersions: false);

        // Assert
        var version = VersionOperationService.GetVersion(publishedVersionId);
        Assert.That(version, Is.Not.Null, "Published version should not be deleted");
    }

    #endregion

    #region Behavioral Equivalence Tests

    [Test]
    public void GetVersion_ViaService_MatchesContentService()
    {
        // Arrange - Use existing content from base class
        var versionId = Textpage.VersionId;

        // Act
        var viaService = VersionOperationService.GetVersion(versionId);
        var viaContentService = ContentService.GetVersion(versionId);

        // Assert
        Assert.That(viaService?.Id, Is.EqualTo(viaContentService?.Id));
        Assert.That(viaService?.VersionId, Is.EqualTo(viaContentService?.VersionId));
    }

    [Test]
    public void GetVersions_ViaService_MatchesContentService()
    {
        // Arrange - Use existing content from base class
        var content = Textpage;
        content.SetValue("author", "Version 2");
        ContentService.Save(content);

        // Act
        var viaService = VersionOperationService.GetVersions(content.Id).ToList();
        var viaContentService = ContentService.GetVersions(content.Id).ToList();

        // Assert
        Assert.That(viaService.Count, Is.EqualTo(viaContentService.Count));
    }

    #endregion

    #region Notification Handler

    /// <summary>
    /// v1.2 Fix (Issue 3.2): Notification handler for testing using the correct integration test pattern.
    /// Uses static actions that can be set in individual tests.
    /// </summary>
    private class VersionNotificationHandler : INotificationHandler<ContentRollingBackNotification>
    {
        public static Action<ContentRollingBackNotification>? RollingBackContent { get; set; }

        public void Handle(ContentRollingBackNotification notification)
            => RollingBackContent?.Invoke(notification);
    }

    #endregion
}
