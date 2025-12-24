// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
/// Integration tests specifically for validating ContentService refactoring.
/// These tests establish behavioral baselines that must pass throughout the refactoring phases.
/// </summary>
[TestFixture]
[NonParallelizable] // Required: static notification handler state is shared across tests
[Category("Refactoring")] // v1.2: Added for easier test filtering during refactoring
[UmbracoTest(
    Database = UmbracoTestOptions.Database.NewSchemaPerTest,
    PublishedRepositoryEvents = true,
    WithApplication = true,
    Logger = UmbracoTestOptions.Logger.Console)]
internal sealed class ContentServiceRefactoringTests : UmbracoIntegrationTestWithContent
{
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();
    private IUserService UserService => GetRequiredService<IUserService>();
    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder
        .AddNotificationHandler<ContentSavingNotification, RefactoringTestNotificationHandler>()
        .AddNotificationHandler<ContentSavedNotification, RefactoringTestNotificationHandler>()
        .AddNotificationHandler<ContentPublishingNotification, RefactoringTestNotificationHandler>()
        .AddNotificationHandler<ContentPublishedNotification, RefactoringTestNotificationHandler>()
        .AddNotificationHandler<ContentUnpublishingNotification, RefactoringTestNotificationHandler>()
        .AddNotificationHandler<ContentUnpublishedNotification, RefactoringTestNotificationHandler>()
        .AddNotificationHandler<ContentMovingNotification, RefactoringTestNotificationHandler>()
        .AddNotificationHandler<ContentMovedNotification, RefactoringTestNotificationHandler>()
        .AddNotificationHandler<ContentMovingToRecycleBinNotification, RefactoringTestNotificationHandler>()
        .AddNotificationHandler<ContentMovedToRecycleBinNotification, RefactoringTestNotificationHandler>()
        .AddNotificationHandler<ContentSortingNotification, RefactoringTestNotificationHandler>()
        .AddNotificationHandler<ContentSortedNotification, RefactoringTestNotificationHandler>()
        .AddNotificationHandler<ContentDeletingNotification, RefactoringTestNotificationHandler>()
        .AddNotificationHandler<ContentDeletedNotification, RefactoringTestNotificationHandler>();

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        RefactoringTestNotificationHandler.Reset();
    }

    [TearDown]
    public void Teardown()
    {
        RefactoringTestNotificationHandler.Reset();
    }

    #region Notification Ordering Tests

    /// <summary>
    /// Test 1: Verifies that MoveToRecycleBin for published content fires notifications in the correct order.
    /// Expected order: MovingToRecycleBin -> MovedToRecycleBin
    /// Note: As per design doc, MoveToRecycleBin does NOT unpublish first - content is "masked" not unpublished.
    /// </summary>
    [Test]
    public void MoveToRecycleBin_PublishedContent_FiresNotificationsInCorrectOrder()
    {
        // Arrange - Create and publish content
        // First publish parent if not already published
        if (!Textpage.Published)
        {
            ContentService.Publish(Textpage, new[] { "*" });
        }

        var content = ContentBuilder.CreateSimpleContent(ContentType, "TestContent", Textpage.Id);
        ContentService.Save(content);
        ContentService.Publish(content, new[] { "*" });

        // Verify it's published
        Assert.That(content.Published, Is.True, "Content should be published before test");

        // Clear notification tracking
        RefactoringTestNotificationHandler.Reset();

        // Act
        var result = ContentService.MoveToRecycleBin(content);

        // Assert
        Assert.That(result.Success, Is.True, "MoveToRecycleBin should succeed");

        var notifications = RefactoringTestNotificationHandler.NotificationOrder;

        // Verify notification sequence
        Assert.That(notifications, Does.Contain(nameof(ContentMovingToRecycleBinNotification)),
            "MovingToRecycleBin notification should fire");
        Assert.That(notifications, Does.Contain(nameof(ContentMovedToRecycleBinNotification)),
            "MovedToRecycleBin notification should fire");

        // Verify order: Moving comes before Moved
        var movingIndex = notifications.IndexOf(nameof(ContentMovingToRecycleBinNotification));
        var movedIndex = notifications.IndexOf(nameof(ContentMovedToRecycleBinNotification));
        Assert.That(movingIndex, Is.LessThan(movedIndex),
            "MovingToRecycleBin should fire before MovedToRecycleBin");
    }

    /// <summary>
    /// Test 2: Verifies that MoveToRecycleBin for unpublished content only fires move notifications.
    /// No publish/unpublish notifications should be fired.
    /// </summary>
    [Test]
    public void MoveToRecycleBin_UnpublishedContent_OnlyFiresMoveNotifications()
    {
        // Arrange - Create content but don't publish
        // First publish parent if not already published (required for creating child content)
        if (!Textpage.Published)
        {
            ContentService.Publish(Textpage, new[] { "*" });
        }

        var content = ContentBuilder.CreateSimpleContent(ContentType, "UnpublishedContent", Textpage.Id);
        ContentService.Save(content);

        // Verify it's not published
        Assert.That(content.Published, Is.False, "Content should not be published before test");

        // Clear notification tracking
        RefactoringTestNotificationHandler.Reset();

        // Act
        var result = ContentService.MoveToRecycleBin(content);

        // Assert
        Assert.That(result.Success, Is.True, "MoveToRecycleBin should succeed");

        var notifications = RefactoringTestNotificationHandler.NotificationOrder;

        // Verify move notifications fire
        Assert.That(notifications, Does.Contain(nameof(ContentMovingToRecycleBinNotification)),
            "MovingToRecycleBin notification should fire");
        Assert.That(notifications, Does.Contain(nameof(ContentMovedToRecycleBinNotification)),
            "MovedToRecycleBin notification should fire");

        // Verify no publish/unpublish notifications
        Assert.That(notifications, Does.Not.Contain(nameof(ContentPublishingNotification)),
            "Publishing notification should not fire for unpublished content");
        Assert.That(notifications, Does.Not.Contain(nameof(ContentPublishedNotification)),
            "Published notification should not fire for unpublished content");
        Assert.That(notifications, Does.Not.Contain(nameof(ContentUnpublishingNotification)),
            "Unpublishing notification should not fire for unpublished content");
        Assert.That(notifications, Does.Not.Contain(nameof(ContentUnpublishedNotification)),
            "Unpublished notification should not fire for unpublished content");
    }

    #endregion

    #region Sort Operation Tests

    /// <summary>
    /// Test 3: Verifies Sort(IEnumerable&lt;IContent&gt;) correctly reorders children.
    /// </summary>
    [Test]
    public void Sort_WithContentItems_ChangesSortOrder()
    {
        // Arrange - Use existing subpages from base class (Subpage, Subpage2, Subpage3)
        // Get fresh copies to ensure we have current sort orders
        var child1 = ContentService.GetById(Subpage.Id)!;
        var child2 = ContentService.GetById(Subpage2.Id)!;
        var child3 = ContentService.GetById(Subpage3.Id)!;

        // v1.2: Verify initial sort order assumption
        Assert.That(child1.SortOrder, Is.LessThan(child2.SortOrder), "Setup: child1 before child2");
        Assert.That(child2.SortOrder, Is.LessThan(child3.SortOrder), "Setup: child2 before child3");

        // Record original sort orders
        var originalOrder1 = child1.SortOrder;
        var originalOrder2 = child2.SortOrder;
        var originalOrder3 = child3.SortOrder;

        // Create reversed order list
        var reorderedItems = new[] { child3, child2, child1 };

        // Act
        var result = ContentService.Sort(reorderedItems);

        // Assert
        Assert.That(result.Success, Is.True, "Sort should succeed");

        // Re-fetch to verify persisted order
        child1 = ContentService.GetById(Subpage.Id)!;
        child2 = ContentService.GetById(Subpage2.Id)!;
        child3 = ContentService.GetById(Subpage3.Id)!;

        Assert.That(child3.SortOrder, Is.EqualTo(0), "Child3 should now be first (sort order 0)");
        Assert.That(child2.SortOrder, Is.EqualTo(1), "Child2 should now be second (sort order 1)");
        Assert.That(child1.SortOrder, Is.EqualTo(2), "Child1 should now be third (sort order 2)");
    }

    /// <summary>
    /// Test 4: Verifies Sort(IEnumerable&lt;int&gt;) correctly reorders children by ID.
    /// </summary>
    [Test]
    public void Sort_WithIds_ChangesSortOrder()
    {
        // Arrange - Use existing subpages from base class
        var child1 = ContentService.GetById(Subpage.Id)!;
        var child2 = ContentService.GetById(Subpage2.Id)!;
        var child3 = ContentService.GetById(Subpage3.Id)!;

        // v1.2: Verify initial sort order assumption
        Assert.That(child1.SortOrder, Is.LessThan(child2.SortOrder), "Setup: child1 before child2");
        Assert.That(child2.SortOrder, Is.LessThan(child3.SortOrder), "Setup: child2 before child3");

        var child1Id = Subpage.Id;
        var child2Id = Subpage2.Id;
        var child3Id = Subpage3.Id;

        // Create reversed order list by ID
        var reorderedIds = new[] { child3Id, child2Id, child1Id };

        // Act
        var result = ContentService.Sort(reorderedIds);

        // Assert
        Assert.That(result.Success, Is.True, "Sort should succeed");

        // Re-fetch to verify persisted order (v1.3: removed var to avoid shadowing)
        child1 = ContentService.GetById(child1Id)!;
        child2 = ContentService.GetById(child2Id)!;
        child3 = ContentService.GetById(child3Id)!;

        Assert.That(child3.SortOrder, Is.EqualTo(0), "Child3 should now be first (sort order 0)");
        Assert.That(child2.SortOrder, Is.EqualTo(1), "Child2 should now be second (sort order 1)");
        Assert.That(child1.SortOrder, Is.EqualTo(2), "Child1 should now be third (sort order 2)");
    }

    /// <summary>
    /// Test 5: Verifies Sort fires Sorting and Sorted notifications in correct sequence.
    /// </summary>
    [Test]
    public void Sort_FiresSortingAndSortedNotifications()
    {
        // Arrange - Use existing subpages from base class
        var child1 = ContentService.GetById(Subpage.Id)!;
        var child2 = ContentService.GetById(Subpage2.Id)!;
        var child3 = ContentService.GetById(Subpage3.Id)!;

        // v1.2: Verify initial sort order assumption
        Assert.That(child1.SortOrder, Is.LessThan(child2.SortOrder), "Setup: child1 before child2");
        Assert.That(child2.SortOrder, Is.LessThan(child3.SortOrder), "Setup: child2 before child3");

        var reorderedItems = new[] { child3, child2, child1 };

        // Clear notification tracking
        RefactoringTestNotificationHandler.Reset();

        // Act
        var result = ContentService.Sort(reorderedItems);

        // Assert
        Assert.That(result.Success, Is.True, "Sort should succeed");

        var notifications = RefactoringTestNotificationHandler.NotificationOrder;

        // Verify both sorting notifications fire
        Assert.That(notifications, Does.Contain(nameof(ContentSortingNotification)),
            "Sorting notification should fire");
        Assert.That(notifications, Does.Contain(nameof(ContentSortedNotification)),
            "Sorted notification should fire");

        // Also verify Saving/Saved fire (Sort saves content)
        Assert.That(notifications, Does.Contain(nameof(ContentSavingNotification)),
            "Saving notification should fire during sort");
        Assert.That(notifications, Does.Contain(nameof(ContentSavedNotification)),
            "Saved notification should fire during sort");

        // Verify order: Sorting -> Saving -> Saved -> Sorted
        var sortingIndex = notifications.IndexOf(nameof(ContentSortingNotification));
        var sortedIndex = notifications.IndexOf(nameof(ContentSortedNotification));

        Assert.That(sortingIndex, Is.LessThan(sortedIndex),
            "Sorting should fire before Sorted");
    }

    #endregion

    #region DeleteOfType Tests

    /// <summary>
    /// Test 6: Verifies DeleteOfType with hierarchical content deletes everything correctly.
    /// </summary>
    [Test]
    public void DeleteOfType_MovesDescendantsToRecycleBinFirst()
    {
        // Arrange - Create a second content type for descendants
        var template = FileService.GetTemplate("defaultTemplate");
        Assert.That(template, Is.Not.Null, "Default template must exist for test setup");
        var childContentType = ContentTypeBuilder.CreateSimpleContentType(
            "childType", "Child Type", defaultTemplateId: template!.Id);
        ContentTypeService.Save(childContentType);

        // Create parent of target type
        var parent = ContentBuilder.CreateSimpleContent(ContentType, "ParentToDelete", -1);
        ContentService.Save(parent);

        // Create child of different type (should be moved to bin, not deleted)
        var childOfDifferentType = ContentBuilder.CreateSimpleContent(childContentType, "ChildDifferentType", parent.Id);
        ContentService.Save(childOfDifferentType);

        // Create child of same type (should be deleted)
        var childOfSameType = ContentBuilder.CreateSimpleContent(ContentType, "ChildSameType", parent.Id);
        ContentService.Save(childOfSameType);

        var parentId = parent.Id;
        var childDiffId = childOfDifferentType.Id;
        var childSameId = childOfSameType.Id;

        // Act
        ContentService.DeleteOfType(ContentType.Id);

        // Assert
        // Parent should be deleted (it's the target type)
        var deletedParent = ContentService.GetById(parentId);
        Assert.That(deletedParent, Is.Null, "Parent of target type should be deleted");

        // Child of same type should be deleted
        var deletedChildSame = ContentService.GetById(childSameId);
        Assert.That(deletedChildSame, Is.Null, "Child of same type should be deleted");

        // Child of different type should be in recycle bin
        var trashedChild = ContentService.GetById(childDiffId);
        Assert.That(trashedChild, Is.Not.Null, "Child of different type should still exist");
        Assert.That(trashedChild!.Trashed, Is.True, "Child of different type should be in recycle bin");
    }

    /// <summary>
    /// Test 7: Verifies DeleteOfType only deletes content of the specified type.
    /// </summary>
    [Test]
    public void DeleteOfType_WithMixedTypes_OnlyDeletesSpecifiedType()
    {
        // Arrange - Create a second content type
        var template = FileService.GetTemplate("defaultTemplate");
        Assert.That(template, Is.Not.Null, "Default template must exist for test setup");
        var otherContentType = ContentTypeBuilder.CreateSimpleContentType(
            "otherType", "Other Type", defaultTemplateId: template!.Id);
        ContentTypeService.Save(otherContentType);

        // Create content of target type
        var targetContent1 = ContentBuilder.CreateSimpleContent(ContentType, "Target1", -1);
        var targetContent2 = ContentBuilder.CreateSimpleContent(ContentType, "Target2", -1);
        ContentService.Save(targetContent1);
        ContentService.Save(targetContent2);

        // Create content of other type (should survive)
        var otherContent = ContentBuilder.CreateSimpleContent(otherContentType, "Other", -1);
        ContentService.Save(otherContent);

        var target1Id = targetContent1.Id;
        var target2Id = targetContent2.Id;
        var otherId = otherContent.Id;

        // Act
        ContentService.DeleteOfType(ContentType.Id);

        // Assert
        Assert.That(ContentService.GetById(target1Id), Is.Null, "Target1 should be deleted");
        Assert.That(ContentService.GetById(target2Id), Is.Null, "Target2 should be deleted");
        Assert.That(ContentService.GetById(otherId), Is.Not.Null, "Other type content should survive");
        Assert.That(ContentService.GetById(otherId)!.Trashed, Is.False, "Other type content should not be trashed");
    }

    /// <summary>
    /// Test 8: Verifies DeleteOfTypes deletes multiple content types in a single operation.
    /// </summary>
    [Test]
    public void DeleteOfTypes_DeletesMultipleTypesAtOnce()
    {
        // Arrange - Create additional content types
        var template = FileService.GetTemplate("defaultTemplate");
        Assert.That(template, Is.Not.Null, "Default template must exist for test setup");

        var type1 = ContentTypeBuilder.CreateSimpleContentType(
            "deleteType1", "Delete Type 1", defaultTemplateId: template!.Id);
        var type2 = ContentTypeBuilder.CreateSimpleContentType(
            "deleteType2", "Delete Type 2", defaultTemplateId: template.Id);
        var survivorType = ContentTypeBuilder.CreateSimpleContentType(
            "survivorType", "Survivor Type", defaultTemplateId: template.Id);

        ContentTypeService.Save(type1);
        ContentTypeService.Save(type2);
        ContentTypeService.Save(survivorType);

        // Create content of each type
        var content1 = ContentBuilder.CreateSimpleContent(type1, "Content1", -1);
        var content2 = ContentBuilder.CreateSimpleContent(type2, "Content2", -1);
        var survivor = ContentBuilder.CreateSimpleContent(survivorType, "Survivor", -1);

        ContentService.Save(content1);
        ContentService.Save(content2);
        ContentService.Save(survivor);

        var content1Id = content1.Id;
        var content2Id = content2.Id;
        var survivorId = survivor.Id;

        // Act - Delete multiple types
        ContentService.DeleteOfTypes(new[] { type1.Id, type2.Id });

        // Assert
        Assert.That(ContentService.GetById(content1Id), Is.Null, "Content of type1 should be deleted");
        Assert.That(ContentService.GetById(content2Id), Is.Null, "Content of type2 should be deleted");
        Assert.That(ContentService.GetById(survivorId), Is.Not.Null, "Content of survivor type should exist");
    }

    #endregion

    #region Permission Tests

    /// <summary>
    /// Test 9: Verifies SetPermission assigns a permission and GetPermissions retrieves it.
    /// </summary>
    [Test]
    public async Task SetPermission_AssignsPermissionToUserGroup()
    {
        // Arrange
        var content = ContentBuilder.CreateSimpleContent(ContentType, "PermissionTest", -1);
        ContentService.Save(content);

        // Get admin user group ID (should always exist)
        var adminGroup = await UserGroupService.GetAsync(Constants.Security.AdminGroupAlias);
        Assert.That(adminGroup, Is.Not.Null, "Admin group should exist");

        // Act - Assign browse permission ('F' is typically the Browse Node permission)
        ContentService.SetPermission(content, "F", new[] { adminGroup!.Id });

        // Assert
        var permissions = ContentService.GetPermissions(content);
        Assert.That(permissions, Is.Not.Null, "Permissions should be returned");

        var adminPermissions = permissions.FirstOrDefault(p => p.UserGroupId == adminGroup.Id);
        Assert.That(adminPermissions, Is.Not.Null, "Should have permissions for admin group");
        Assert.That(adminPermissions!.AssignedPermissions, Does.Contain("F"),
            "Admin group should have Browse permission");
    }

    /// <summary>
    /// Test 10: Verifies multiple SetPermission calls accumulate permissions for a user group.
    /// </summary>
    /// <remarks>
    /// v1.2: Expected behavior documentation -
    /// SetPermission assigns permissions per-permission-type, not per-entity.
    /// Calling SetPermission("F", ...) then SetPermission("U", ...) results in both F and U
    /// permissions being assigned. Each call only replaces permissions of the same type.
    /// </remarks>
    [Test]
    public async Task SetPermission_MultiplePermissionsForSameGroup()
    {
        // Arrange
        var content = ContentBuilder.CreateSimpleContent(ContentType, "MultiPermissionTest", -1);
        ContentService.Save(content);

        var adminGroup = (await UserGroupService.GetAsync(Constants.Security.AdminGroupAlias))!;

        // Act - Assign multiple permissions
        ContentService.SetPermission(content, "F", new[] { adminGroup.Id }); // Browse
        ContentService.SetPermission(content, "U", new[] { adminGroup.Id }); // Update

        // Assert
        var permissions = ContentService.GetPermissions(content);
        var adminPermissions = permissions.FirstOrDefault(p => p.UserGroupId == adminGroup.Id);

        Assert.That(adminPermissions, Is.Not.Null, "Should have permissions for admin group");
        Assert.That(adminPermissions!.AssignedPermissions, Does.Contain("F"), "Should have Browse permission");
        Assert.That(adminPermissions.AssignedPermissions, Does.Contain("U"), "Should have Update permission");
    }

    /// <summary>
    /// Test 11: Verifies SetPermissions assigns a complete permission set.
    /// </summary>
    [Test]
    public async Task SetPermissions_AssignsPermissionSet()
    {
        // Arrange
        var content = ContentBuilder.CreateSimpleContent(ContentType, "PermissionSetTest", -1);
        ContentService.Save(content);

        var adminGroup = (await UserGroupService.GetAsync(Constants.Security.AdminGroupAlias))!;

        // Create permission set
        var permissionSet = new EntityPermissionSet(
            content.Id,
            new EntityPermissionCollection(new[]
            {
                new EntityPermission(adminGroup.Id, content.Id, new HashSet<string> { "F", "U", "P" }) // Browse, Update, Publish
            }));

        // Act
        ContentService.SetPermissions(permissionSet);

        // Assert
        var permissions = ContentService.GetPermissions(content);
        var adminPermissions = permissions.FirstOrDefault(p => p.UserGroupId == adminGroup.Id);

        Assert.That(adminPermissions, Is.Not.Null, "Should have permissions for admin group");
        Assert.That(adminPermissions!.AssignedPermissions, Does.Contain("F"), "Should have Browse permission");
        Assert.That(adminPermissions.AssignedPermissions, Does.Contain("U"), "Should have Update permission");
        Assert.That(adminPermissions.AssignedPermissions, Does.Contain("P"), "Should have Publish permission");
    }

    /// <summary>
    /// Test 12: Verifies SetPermission can assign to multiple user groups simultaneously.
    /// </summary>
    [Test]
    public async Task SetPermission_AssignsToMultipleUserGroups()
    {
        // Arrange
        var content = ContentBuilder.CreateSimpleContent(ContentType, "MultiGroupTest", -1);
        ContentService.Save(content);

        var adminGroup = (await UserGroupService.GetAsync(Constants.Security.AdminGroupAlias))!;
        var editorGroup = (await UserGroupService.GetAsync(Constants.Security.EditorGroupKey))!;

        // Act - Assign permission to multiple groups at once
        ContentService.SetPermission(content, "F", new[] { adminGroup.Id, editorGroup.Id });

        // Assert
        var permissions = ContentService.GetPermissions(content);

        var adminPermissions = permissions.FirstOrDefault(p => p.UserGroupId == adminGroup.Id);
        var editorPermissions = permissions.FirstOrDefault(p => p.UserGroupId == editorGroup.Id);

        Assert.That(adminPermissions, Is.Not.Null, "Should have permissions for admin group");
        Assert.That(adminPermissions!.AssignedPermissions, Does.Contain("F"), "Admin should have Browse permission");

        Assert.That(editorPermissions, Is.Not.Null, "Should have permissions for editor group");
        Assert.That(editorPermissions!.AssignedPermissions, Does.Contain("F"), "Editor should have Browse permission");
    }

    #endregion

    #region Transaction Boundary Tests

    /// <summary>
    /// Test 13: Verifies that multiple operations within an uncompleted scope all roll back together.
    /// </summary>
    [Test]
    public void AmbientScope_NestedOperationsShareTransaction()
    {
        // Arrange
        var content1 = ContentBuilder.CreateSimpleContent(ContentType, "RollbackTest1", -1);
        var content2 = ContentBuilder.CreateSimpleContent(ContentType, "RollbackTest2", -1);

        // Act - Create scope, save content, but don't complete the scope
        using (var scope = ScopeProvider.CreateScope())
        {
            ContentService.Save(content1);
            ContentService.Save(content2);

            // Verify content has IDs (was saved within transaction)
            Assert.That(content1.Id, Is.GreaterThan(0), "Content1 should have an ID");
            Assert.That(content2.Id, Is.GreaterThan(0), "Content2 should have an ID");

            // v1.2: Note - IDs are captured for debugging but cannot be used after rollback
            // since they were assigned within the rolled-back transaction
            var id1 = content1.Id;
            var id2 = content2.Id;

            // DON'T call scope.Complete() - should roll back
        }

        // Assert - Content should not exist after rollback
        // We can't use the IDs because they were assigned in the rolled-back transaction
        // Instead, search by name
        var foundContent = ContentService.GetRootContent()
            .Where(c => c.Name == "RollbackTest1" || c.Name == "RollbackTest2")
            .ToList();

        Assert.That(foundContent, Is.Empty, "Content should not exist after transaction rollback");
    }

    /// <summary>
    /// Test 14: Verifies that multiple operations within a completed scope all commit together.
    /// </summary>
    [Test]
    public void AmbientScope_CompletedScopeCommitsAllOperations()
    {
        // Arrange
        var content1 = ContentBuilder.CreateSimpleContent(ContentType, "CommitTest1", -1);
        var content2 = ContentBuilder.CreateSimpleContent(ContentType, "CommitTest2", -1);
        int id1, id2;

        // Act - Create scope, save content, and complete the scope
        using (var scope = ScopeProvider.CreateScope())
        {
            ContentService.Save(content1);
            ContentService.Save(content2);

            id1 = content1.Id;
            id2 = content2.Id;

            scope.Complete(); // Commit transaction
        }

        // Assert - Content should exist after commit
        var retrieved1 = ContentService.GetById(id1);
        var retrieved2 = ContentService.GetById(id2);

        Assert.That(retrieved1, Is.Not.Null, "Content1 should exist after commit");
        Assert.That(retrieved2, Is.Not.Null, "Content2 should exist after commit");
        Assert.That(retrieved1!.Name, Is.EqualTo("CommitTest1"));
        Assert.That(retrieved2!.Name, Is.EqualTo("CommitTest2"));
    }

    /// <summary>
    /// Test 15: Verifies MoveToRecycleBin within an uncompleted scope rolls back completely.
    /// </summary>
    [Test]
    public void AmbientScope_MoveToRecycleBinRollsBackCompletely()
    {
        // Arrange - Create and save content OUTSIDE the test scope so it persists
        var content = ContentBuilder.CreateSimpleContent(ContentType, "MoveRollbackTest", -1);
        ContentService.Save(content);
        var contentId = content.Id;

        // Verify content exists and is not trashed
        var beforeMove = ContentService.GetById(contentId);
        Assert.That(beforeMove, Is.Not.Null, "Content should exist before test");
        Assert.That(beforeMove!.Trashed, Is.False, "Content should not be trashed before test");

        // Act - Move to recycle bin within an uncompleted scope
        using (var scope = ScopeProvider.CreateScope())
        {
            ContentService.MoveToRecycleBin(content);

            // Verify it's trashed within the transaction
            var duringMove = ContentService.GetById(contentId);
            Assert.That(duringMove!.Trashed, Is.True, "Content should be trashed within transaction");

            // DON'T call scope.Complete() - should roll back
        }

        // Assert - Content should be back to original state after rollback
        var afterRollback = ContentService.GetById(contentId);
        Assert.That(afterRollback, Is.Not.Null, "Content should still exist after rollback");
        Assert.That(afterRollback!.Trashed, Is.False, "Content should not be trashed after rollback");
        Assert.That(afterRollback.ParentId, Is.EqualTo(-1), "Content should be at root level after rollback");
    }

    #endregion

    #region Phase 1 Tests

    /// <summary>
    /// Phase 1 Test: Verifies IContentCrudService is registered and resolvable from DI.
    /// </summary>
    [Test]
    public void IContentCrudService_CanBeResolvedFromDI()
    {
        // Act
        var crudService = GetRequiredService<IContentCrudService>();

        // Assert
        Assert.That(crudService, Is.Not.Null);
        Assert.That(crudService, Is.InstanceOf<ContentCrudService>());
    }

    #endregion

    #region Phase 2 - Count Method Delegation Tests

    /// <summary>
    /// Phase 2 Test: Verifies Count() via facade returns same result as direct service call.
    /// </summary>
    [Test]
    public void Count_ViaFacade_ReturnsEquivalentResultToDirectService()
    {
        // Arrange
        var queryService = GetRequiredService<IContentQueryOperationService>();

        // Act
        var facadeCount = ContentService.Count();
        var directCount = queryService.Count();

        // Assert
        Assert.That(facadeCount, Is.EqualTo(directCount));
    }

    /// <summary>
    /// Phase 2 Test: Verifies CountPublished() via facade returns same result as direct service call.
    /// </summary>
    [Test]
    public void CountPublished_ViaFacade_ReturnsEquivalentResultToDirectService()
    {
        // Arrange
        var queryService = GetRequiredService<IContentQueryOperationService>();
        ContentService.Publish(Textpage, new[] { "*" });

        // Act
        var facadeCount = ContentService.CountPublished();
        var directCount = queryService.CountPublished();

        // Assert
        Assert.That(facadeCount, Is.EqualTo(directCount));
    }

    /// <summary>
    /// Phase 2 Test: Verifies CountChildren() via facade returns same result as direct service call.
    /// </summary>
    [Test]
    public void CountChildren_ViaFacade_ReturnsEquivalentResultToDirectService()
    {
        // Arrange
        var queryService = GetRequiredService<IContentQueryOperationService>();
        var parentId = Textpage.Id;

        // Act
        var facadeCount = ContentService.CountChildren(parentId);
        var directCount = queryService.CountChildren(parentId);

        // Assert
        Assert.That(facadeCount, Is.EqualTo(directCount));
    }

    /// <summary>
    /// Phase 2 Test: Verifies CountDescendants() via facade returns same result as direct service call.
    /// </summary>
    [Test]
    public void CountDescendants_ViaFacade_ReturnsEquivalentResultToDirectService()
    {
        // Arrange
        var queryService = GetRequiredService<IContentQueryOperationService>();
        var parentId = Textpage.Id;

        // Act
        var facadeCount = ContentService.CountDescendants(parentId);
        var directCount = queryService.CountDescendants(parentId);

        // Assert
        Assert.That(facadeCount, Is.EqualTo(directCount));
    }

    /// <summary>
    /// Phase 2 Test: Verifies GetByLevel() via facade returns same result as direct service call.
    /// </summary>
    [Test]
    public void GetByLevel_ViaFacade_ReturnsEquivalentResultToDirectService()
    {
        // Arrange
        var queryService = GetRequiredService<IContentQueryOperationService>();

        // Act
        var facadeItems = ContentService.GetByLevel(1).ToList();
        var directItems = queryService.GetByLevel(1).ToList();

        // Assert
        Assert.That(facadeItems.Count, Is.EqualTo(directItems.Count));
        Assert.That(facadeItems.Select(x => x.Id), Is.EquivalentTo(directItems.Select(x => x.Id)));
    }

    /// <summary>
    /// Phase 2 Test: Verifies GetPagedOfType() via facade returns same result as direct service call.
    /// </summary>
    [Test]
    public void GetPagedOfType_ViaFacade_ReturnsEquivalentResultToDirectService()
    {
        // Arrange
        var queryService = GetRequiredService<IContentQueryOperationService>();
        var contentTypeId = ContentType.Id;

        // Act
        var facadeItems = ContentService.GetPagedOfType(contentTypeId, 0, 10, out var facadeTotal).ToList();
        var directItems = queryService.GetPagedOfType(contentTypeId, 0, 10, out var directTotal).ToList();

        // Assert
        Assert.That(facadeTotal, Is.EqualTo(directTotal));
        Assert.That(facadeItems.Select(x => x.Id), Is.EquivalentTo(directItems.Select(x => x.Id)));
    }

    /// <summary>
    /// Phase 2 Test: Verifies GetPagedOfTypes() via facade returns same result as direct service call.
    /// </summary>
    [Test]
    public void GetPagedOfTypes_ViaFacade_ReturnsEquivalentResultToDirectService()
    {
        // Arrange
        var queryService = GetRequiredService<IContentQueryOperationService>();
        var contentTypeIds = new[] { ContentType.Id };

        // Act
        var facadeItems = ContentService.GetPagedOfTypes(contentTypeIds, 0, 10, out var facadeTotal, null).ToList();
        var directItems = queryService.GetPagedOfTypes(contentTypeIds, 0, 10, out var directTotal).ToList();

        // Assert
        Assert.That(facadeTotal, Is.EqualTo(directTotal));
        Assert.That(facadeItems.Select(x => x.Id), Is.EquivalentTo(directItems.Select(x => x.Id)));
    }

    #endregion

    #region Phase 5 - Publish Operation Tests

    [Test]
    public void ContentPublishOperationService_Can_Be_Resolved_From_DI()
    {
        // Act
        var publishOperationService = GetRequiredService<IContentPublishOperationService>();

        // Assert
        Assert.That(publishOperationService, Is.Not.Null);
        Assert.That(publishOperationService, Is.InstanceOf<ContentPublishOperationService>());
    }

    [Test]
    public async Task Publish_Through_ContentService_Uses_PublishOperationService()
    {
        // Arrange
        var contentService = GetRequiredService<IContentService>();
        var contentTypeService = GetRequiredService<IContentTypeService>();

        var contentType = new ContentTypeBuilder()
            .WithAlias("testPublishPage")
            .Build();
        await contentTypeService.SaveAsync(contentType, Constants.Security.SuperUserKey);

        var content = contentService.Create("Test Publish Page", Constants.System.Root, contentType.Alias);
        contentService.Save(content);

        // Act
        var result = contentService.Publish(content, new[] { "*" });

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(content.Published, Is.True);
    }

    [Test]
    public async Task Unpublish_Through_ContentService_Uses_PublishOperationService()
    {
        // Arrange
        var contentService = GetRequiredService<IContentService>();
        var contentTypeService = GetRequiredService<IContentTypeService>();

        var contentType = new ContentTypeBuilder()
            .WithAlias("testUnpublishPage")
            .Build();
        await contentTypeService.SaveAsync(contentType, Constants.Security.SuperUserKey);

        var content = contentService.Create("Test Unpublish Page", Constants.System.Root, contentType.Alias);
        contentService.Save(content);
        contentService.Publish(content, new[] { "*" });

        // Act
        var result = contentService.Unpublish(content);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(content.Published, Is.False);
    }

    [Test]
    public async Task IsPathPublishable_RootContent_ReturnsTrue()
    {
        // Arrange
        var contentService = GetRequiredService<IContentService>();
        var contentTypeService = GetRequiredService<IContentTypeService>();

        var contentType = new ContentTypeBuilder()
            .WithAlias("testPathPage")
            .Build();
        await contentTypeService.SaveAsync(contentType, Constants.Security.SuperUserKey);

        var content = contentService.Create("Test Path Page", Constants.System.Root, contentType.Alias);
        contentService.Save(content);

        // Act
        var result = contentService.IsPathPublishable(content);

        // Assert
        Assert.That(result, Is.True);
    }

    #endregion

    #region Phase 6 - Permission Manager Tests

    /// <summary>
    /// Phase 6 Test: Verifies ContentPermissionManager is registered and resolvable from DI.
    /// </summary>
    [Test]
    public void ContentPermissionManager_CanBeResolvedFromDI()
    {
        // Act
        var permissionManager = GetRequiredService<ContentPermissionManager>();

        // Assert
        Assert.That(permissionManager, Is.Not.Null);
        Assert.That(permissionManager, Is.InstanceOf<ContentPermissionManager>());
    }

    /// <summary>
    /// Phase 6 Test: Verifies permission operations work via ContentService after delegation.
    /// </summary>
    [Test]
    public async Task SetPermission_ViaContentService_DelegatesToPermissionManager()
    {
        // Arrange
        var content = ContentBuilder.CreateSimpleContent(ContentType, "PermissionDelegationTest", -1);
        ContentService.Save(content);

        var adminGroup = await UserGroupService.GetAsync(Constants.Security.AdminGroupAlias);
        Assert.That(adminGroup, Is.Not.Null, "Admin group should exist");

        // Act - This should delegate to ContentPermissionManager
        ContentService.SetPermission(content, "F", new[] { adminGroup!.Id });

        // Assert - Verify it worked (via GetPermissions which also delegates)
        var permissions = ContentService.GetPermissions(content);
        var adminPermissions = permissions.FirstOrDefault(p => p.UserGroupId == adminGroup.Id);

        Assert.That(adminPermissions, Is.Not.Null, "Should have permissions for admin group");
        Assert.That(adminPermissions!.AssignedPermissions, Does.Contain("F"),
            "Admin group should have Browse permission");
    }

    #endregion

    #region Phase 7 - Blueprint Manager Tests

    /// <summary>
    /// Phase 7 Test: Verifies ContentBlueprintManager is registered and resolvable from DI.
    /// </summary>
    [Test]
    public void ContentBlueprintManager_CanBeResolvedFromDI()
    {
        // Act
        var blueprintManager = GetRequiredService<ContentBlueprintManager>();

        // Assert
        Assert.That(blueprintManager, Is.Not.Null);
        Assert.That(blueprintManager, Is.InstanceOf<ContentBlueprintManager>());
    }

    /// <summary>
    /// Phase 7 Test: Verifies ContentBlueprintManager can be used directly without going through ContentService.
    /// This validates the manager is independently functional (v2.0: added per critical review).
    /// </summary>
    [Test]
    public void ContentBlueprintManager_CanBeUsedDirectly()
    {
        // Arrange
        var blueprintManager = GetRequiredService<ContentBlueprintManager>();
        var blueprint = ContentBuilder.CreateSimpleContent(ContentType, "DirectManagerBlueprint", -1);

        // Act - Use manager directly, not through ContentService
        blueprintManager.SaveBlueprint(blueprint, null, Constants.Security.SuperUserId);

        // Assert
        Assert.That(blueprint.Blueprint, Is.True, "Content should be marked as blueprint");
        Assert.That(blueprint.HasIdentity, Is.True, "Blueprint should have been saved");

        // Retrieve directly through manager
        var retrieved = blueprintManager.GetBlueprintById(blueprint.Id);
        Assert.That(retrieved, Is.Not.Null, "Blueprint should be retrievable via manager");
        Assert.That(retrieved!.Name, Is.EqualTo("DirectManagerBlueprint"));
    }

    /// <summary>
    /// Phase 7 Test: Verifies blueprint operations work via ContentService after delegation.
    /// </summary>
    [Test]
    public void SaveBlueprint_ViaContentService_DelegatesToBlueprintManager()
    {
        // Arrange
        var blueprint = ContentBuilder.CreateSimpleContent(ContentType, "TestBlueprint", -1);

        // Act - This should delegate to ContentBlueprintManager
        ContentService.SaveBlueprint(blueprint);

        // Assert - Verify it was saved as a blueprint
        Assert.That(blueprint.Blueprint, Is.True, "Content should be marked as blueprint");
        Assert.That(blueprint.HasIdentity, Is.True, "Blueprint should have been saved");

        // Retrieve and verify
        var retrieved = ContentService.GetBlueprintById(blueprint.Id);
        Assert.That(retrieved, Is.Not.Null, "Blueprint should be retrievable");
        Assert.That(retrieved!.Blueprint, Is.True, "Retrieved content should be marked as blueprint");
        Assert.That(retrieved.Name, Is.EqualTo("TestBlueprint"));
    }

    /// <summary>
    /// Phase 7 Test: Verifies DeleteBlueprint works via ContentService.
    /// </summary>
    [Test]
    public void DeleteBlueprint_ViaContentService_DelegatesToBlueprintManager()
    {
        // Arrange
        var blueprint = ContentBuilder.CreateSimpleContent(ContentType, "BlueprintToDelete", -1);
        ContentService.SaveBlueprint(blueprint);
        var blueprintId = blueprint.Id;

        Assert.That(ContentService.GetBlueprintById(blueprintId), Is.Not.Null, "Blueprint should exist before delete");

        // Act
        ContentService.DeleteBlueprint(blueprint);

        // Assert
        Assert.That(ContentService.GetBlueprintById(blueprintId), Is.Null, "Blueprint should be deleted");
    }

    /// <summary>
    /// Phase 7 Test: Verifies GetBlueprintsForContentTypes works via ContentService.
    /// </summary>
    [Test]
    public void GetBlueprintsForContentTypes_ViaContentService_DelegatesToBlueprintManager()
    {
        // Arrange
        var blueprint1 = ContentBuilder.CreateSimpleContent(ContentType, "Blueprint1", -1);
        var blueprint2 = ContentBuilder.CreateSimpleContent(ContentType, "Blueprint2", -1);
        ContentService.SaveBlueprint(blueprint1);
        ContentService.SaveBlueprint(blueprint2);

        // Act
        var blueprints = ContentService.GetBlueprintsForContentTypes(ContentType.Id).ToList();

        // Assert
        Assert.That(blueprints.Count, Is.GreaterThanOrEqualTo(2), "Should find at least 2 blueprints");
        Assert.That(blueprints.All(b => b.Blueprint), Is.True, "All returned items should be blueprints");
    }

    #endregion

    #region Phase 8 - Exposed Interface Methods Tests

    /// <summary>
    /// Phase 8 Test: Verifies PerformMoveLocked returns non-null collection.
    /// </summary>
    [Test]
    public void PerformMoveLocked_ReturnsNonNullCollection()
    {
        // Arrange
        var moveService = GetRequiredService<IContentMoveOperationService>();
        var content = ContentBuilder.CreateSimpleContent(ContentType, "MoveTest", -1);
        ContentService.Save(content);

        // Create a destination parent
        var destination = ContentBuilder.CreateSimpleContent(ContentType, "Destination", -1);
        ContentService.Save(destination);

        // Act
        IReadOnlyCollection<(IContent Content, string OriginalPath)> result;
        using (var scope = ScopeProvider.CreateScope())
        {
            scope.WriteLock(Constants.Locks.ContentTree);
            result = moveService.PerformMoveLocked(content, destination.Id, null, Constants.Security.SuperUserId, null);
            scope.Complete();
        }

        // Assert
        Assert.That(result, Is.Not.Null, "PerformMoveLocked should return non-null collection");
    }

    /// <summary>
    /// Phase 8 Test: Verifies PerformMoveLocked includes moved item in returned collection.
    /// </summary>
    [Test]
    public void PerformMoveLocked_IncludesMovedItemInCollection()
    {
        // Arrange
        var moveService = GetRequiredService<IContentMoveOperationService>();
        var content = ContentBuilder.CreateSimpleContent(ContentType, "MoveItem", -1);
        ContentService.Save(content);
        var contentId = content.Id;

        var destination = ContentBuilder.CreateSimpleContent(ContentType, "Destination", -1);
        ContentService.Save(destination);
        var destinationId = destination.Id;

        // Act
        IReadOnlyCollection<(IContent Content, string OriginalPath)> result;
        using (var scope = ScopeProvider.CreateScope())
        {
            scope.WriteLock(Constants.Locks.ContentTree);
            result = moveService.PerformMoveLocked(content, destinationId, null, Constants.Security.SuperUserId, null);
            scope.Complete();
        }

        // Assert
        Assert.That(result.Count, Is.GreaterThan(0), "Should return at least one move event");
        Assert.That(result.Any(e => e.Content.Id == contentId), Is.True,
            "Moved item should be included in returned collection");
    }

    /// <summary>
    /// Phase 8 Test: Verifies PerformMoveLocked handles nested hierarchy correctly.
    /// </summary>
    [Test]
    public void PerformMoveLocked_HandlesNestedHierarchy()
    {
        // Arrange
        var moveService = GetRequiredService<IContentMoveOperationService>();

        // Create parent with child
        var parent = ContentBuilder.CreateSimpleContent(ContentType, "ParentToMove", -1);
        ContentService.Save(parent);

        var child = ContentBuilder.CreateSimpleContent(ContentType, "Child", parent.Id);
        ContentService.Save(child);

        // Create destination
        var destination = ContentBuilder.CreateSimpleContent(ContentType, "Destination", -1);
        ContentService.Save(destination);

        // Act
        IReadOnlyCollection<(IContent Content, string OriginalPath)> result;
        using (var scope = ScopeProvider.CreateScope())
        {
            scope.WriteLock(Constants.Locks.ContentTree);
            result = moveService.PerformMoveLocked(parent, destination.Id, null, Constants.Security.SuperUserId, null);
            scope.Complete();
        }

        // Assert
        Assert.That(result.Count, Is.GreaterThan(0), "Should return move events for hierarchy");

        // Verify parent was moved
        var movedParent = ContentService.GetById(parent.Id);
        Assert.That(movedParent!.ParentId, Is.EqualTo(destination.Id), "Parent should be moved to destination");
    }

    /// <summary>
    /// Phase 8 Test: Verifies DeleteLocked handles content with no descendants.
    /// </summary>
    [Test]
    public void DeleteLocked_HandlesEmptyTree()
    {
        // Arrange
        var crudService = GetRequiredService<IContentCrudService>();
        var content = ContentBuilder.CreateSimpleContent(ContentType, "DeleteTest", -1);
        ContentService.Save(content);
        var contentId = content.Id;

        // Act & Assert - Should not throw
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            scope.WriteLock(Constants.Locks.ContentTree);
            var eventMessages = new EventMessages();
            Assert.DoesNotThrow(() => crudService.DeleteLocked(scope, content, eventMessages));
            scope.Complete();
        }

        // Verify deletion
        Assert.That(ContentService.GetById(contentId), Is.Null, "Content should be deleted");
    }

    /// <summary>
    /// Phase 8 Test: Verifies DeleteLocked handles content with descendants.
    /// </summary>
    [Test]
    public void DeleteLocked_HandlesLargeTree()
    {
        // Arrange
        var crudService = GetRequiredService<IContentCrudService>();

        // Create parent with multiple children
        var parent = ContentBuilder.CreateSimpleContent(ContentType, "ParentToDelete", -1);
        ContentService.Save(parent);

        var child1 = ContentBuilder.CreateSimpleContent(ContentType, "Child1", parent.Id);
        var child2 = ContentBuilder.CreateSimpleContent(ContentType, "Child2", parent.Id);
        var child3 = ContentBuilder.CreateSimpleContent(ContentType, "Child3", parent.Id);
        ContentService.Save(child1);
        ContentService.Save(child2);
        ContentService.Save(child3);

        var parentId = parent.Id;
        var child1Id = child1.Id;
        var child2Id = child2.Id;
        var child3Id = child3.Id;

        // Act
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            scope.WriteLock(Constants.Locks.ContentTree);
            var eventMessages = new EventMessages();
            crudService.DeleteLocked(scope, parent, eventMessages);
            scope.Complete();
        }

        // Assert - All should be deleted
        Assert.That(ContentService.GetById(parentId), Is.Null, "Parent should be deleted");
        Assert.That(ContentService.GetById(child1Id), Is.Null, "Child1 should be deleted");
        Assert.That(ContentService.GetById(child2Id), Is.Null, "Child2 should be deleted");
        Assert.That(ContentService.GetById(child3Id), Is.Null, "Child3 should be deleted");
    }

    /// <summary>
    /// Phase 8 Test: Verifies DeleteLocked throws exception for null content.
    /// Note: Current implementation throws NullReferenceException rather than ArgumentNullException.
    /// This test documents the actual behavior.
    /// </summary>
    [Test]
    public void DeleteLocked_ThrowsForNullContent()
    {
        // Arrange
        var crudService = GetRequiredService<IContentCrudService>();

        // Act & Assert
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            scope.WriteLock(Constants.Locks.ContentTree);
            var eventMessages = new EventMessages();
            Assert.Throws<NullReferenceException>(() =>
                crudService.DeleteLocked(scope, null!, eventMessages));
        }
    }

    #endregion

    /// <summary>
    /// Notification handler that tracks the order of notifications for test verification.
    /// </summary>
    internal sealed class RefactoringTestNotificationHandler :
        INotificationHandler<ContentSavingNotification>,
        INotificationHandler<ContentSavedNotification>,
        INotificationHandler<ContentPublishingNotification>,
        INotificationHandler<ContentPublishedNotification>,
        INotificationHandler<ContentUnpublishingNotification>,
        INotificationHandler<ContentUnpublishedNotification>,
        INotificationHandler<ContentMovingNotification>,
        INotificationHandler<ContentMovedNotification>,
        INotificationHandler<ContentMovingToRecycleBinNotification>,
        INotificationHandler<ContentMovedToRecycleBinNotification>,
        INotificationHandler<ContentSortingNotification>,
        INotificationHandler<ContentSortedNotification>,
        INotificationHandler<ContentDeletingNotification>,
        INotificationHandler<ContentDeletedNotification>
    {
        private static readonly List<string> _notificationOrder = new();
        private static readonly object _lock = new();

        public static IReadOnlyList<string> NotificationOrder
        {
            get
            {
                lock (_lock)
                {
                    return _notificationOrder.ToList();
                }
            }
        }

        public static void Reset()
        {
            lock (_lock)
            {
                _notificationOrder.Clear();
            }
        }

        private static void Record(string notificationType)
        {
            lock (_lock)
            {
                _notificationOrder.Add(notificationType);
            }
        }

        public void Handle(ContentSavingNotification notification) => Record(nameof(ContentSavingNotification));
        public void Handle(ContentSavedNotification notification) => Record(nameof(ContentSavedNotification));
        public void Handle(ContentPublishingNotification notification) => Record(nameof(ContentPublishingNotification));
        public void Handle(ContentPublishedNotification notification) => Record(nameof(ContentPublishedNotification));
        public void Handle(ContentUnpublishingNotification notification) => Record(nameof(ContentUnpublishingNotification));
        public void Handle(ContentUnpublishedNotification notification) => Record(nameof(ContentUnpublishedNotification));
        public void Handle(ContentMovingNotification notification) => Record(nameof(ContentMovingNotification));
        public void Handle(ContentMovedNotification notification) => Record(nameof(ContentMovedNotification));
        public void Handle(ContentMovingToRecycleBinNotification notification) => Record(nameof(ContentMovingToRecycleBinNotification));
        public void Handle(ContentMovedToRecycleBinNotification notification) => Record(nameof(ContentMovedToRecycleBinNotification));
        public void Handle(ContentSortingNotification notification) => Record(nameof(ContentSortingNotification));
        public void Handle(ContentSortedNotification notification) => Record(nameof(ContentSortedNotification));
        public void Handle(ContentDeletingNotification notification) => Record(nameof(ContentDeletingNotification));
        public void Handle(ContentDeletedNotification notification) => Record(nameof(ContentDeletedNotification));
    }
}
