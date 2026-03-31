// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

internal sealed partial class UserServiceTests
{
    [Test]
    public async Task GetDocumentPermissionsAsync_Child_Inherits_Parent_Permissions()
    {
        // Arrange
        var userGroup = await CreateTestUserGroup();
        var user = UserService.CreateUserWithIdentity("test1", "test1@test.com");
        user.AddGroup(userGroup.ToReadOnlyGroup());
        UserService.Save(user);

        var contentType = ContentTypeBuilder.CreateSimpleContentType();
        contentType.AllowedTemplates = null;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var parent = ContentBuilder.CreateSimpleContent(contentType);
        ContentService.Save(parent);
        var child = ContentBuilder.CreateSimpleContent(contentType, "child", parent.Id);
        ContentService.Save(child);

        // Set explicit permissions on the parent only
        ContentService.SetPermission(parent, ActionBrowse.ActionLetter, [userGroup.Id]);
        ContentService.SetPermission(parent, ActionDelete.ActionLetter, [userGroup.Id]);

        // Act
        var result = await UserService
            .GetDocumentPermissionsAsync(user.Key, [child.Key]);

        // Assert
        Assert.IsTrue(result.Success);
        var nodePermissions = result.Result.ToArray();
        Assert.AreEqual(1, nodePermissions.Length);
        Assert.AreEqual(child.Key, nodePermissions[0].NodeKey);

        // Child should inherit parent's explicit permissions (Browse + Delete)
        Assert.IsTrue(nodePermissions[0].Permissions.Contains(ActionBrowse.ActionLetter));
        Assert.IsTrue(nodePermissions[0].Permissions.Contains(ActionDelete.ActionLetter));
    }

    [Test]
    public async Task GetDocumentPermissionsAsync_Grandchild_Inherits_Parent_Permissions()
    {
        // Arrange
        var userGroup = await CreateTestUserGroup();
        var user = UserService.CreateUserWithIdentity("test1", "test1@test.com");
        user.AddGroup(userGroup.ToReadOnlyGroup());
        UserService.Save(user);

        var contentType = ContentTypeBuilder.CreateSimpleContentType();
        contentType.AllowedTemplates = null;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var parent = ContentBuilder.CreateSimpleContent(contentType);
        ContentService.Save(parent);
        var child = ContentBuilder.CreateSimpleContent(contentType, "child", parent.Id);
        ContentService.Save(child);
        var grandchild = ContentBuilder.CreateSimpleContent(contentType, "grandchild", child.Id);
        ContentService.Save(grandchild);

        // Set explicit permissions on the parent only
        ContentService.SetPermission(parent, ActionBrowse.ActionLetter, [userGroup.Id]);
        ContentService.SetPermission(parent, ActionMove.ActionLetter, [userGroup.Id]);

        // Act
        var result = await UserService
            .GetDocumentPermissionsAsync(user.Key, [grandchild.Key]);

        // Assert
        Assert.IsTrue(result.Success);
        var nodePermissions = result.Result.ToArray();
        Assert.AreEqual(1, nodePermissions.Length);
        Assert.AreEqual(grandchild.Key, nodePermissions[0].NodeKey);

        // Grandchild should inherit parent's explicit permissions
        Assert.IsTrue(nodePermissions[0].Permissions.Contains(ActionBrowse.ActionLetter));
        Assert.IsTrue(nodePermissions[0].Permissions.Contains(ActionMove.ActionLetter));
    }

    [Test]
    public async Task GetDocumentPermissionsAsync_Returns_Explicit_Permissions_When_Set()
    {
        // Arrange
        var userGroup = await CreateTestUserGroup();
        var user = UserService.CreateUserWithIdentity("test1", "test1@test.com");
        user.AddGroup(userGroup.ToReadOnlyGroup());
        UserService.Save(user);

        var contentType = ContentTypeBuilder.CreateSimpleContentType();
        contentType.AllowedTemplates = null;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var parent = ContentBuilder.CreateSimpleContent(contentType);
        ContentService.Save(parent);
        var child = ContentBuilder.CreateSimpleContent(contentType, "child", parent.Id);
        ContentService.Save(child);

        // Set different explicit permissions on parent and child
        ContentService.SetPermission(parent, ActionBrowse.ActionLetter, [userGroup.Id]);
        ContentService.SetPermission(parent, ActionDelete.ActionLetter, [userGroup.Id]);
        ContentService.SetPermission(child, ActionBrowse.ActionLetter, [userGroup.Id]);
        ContentService.SetPermission(child, ActionMove.ActionLetter, [userGroup.Id]);

        // Act
        var result = await UserService
            .GetDocumentPermissionsAsync(user.Key, [child.Key]);

        // Assert
        Assert.IsTrue(result.Success);
        var nodePermissions = result.Result.ToArray();
        Assert.AreEqual(1, nodePermissions.Length);

        // Child should use its own explicit permissions, not parent's
        Assert.IsTrue(nodePermissions[0].Permissions.Contains(ActionBrowse.ActionLetter));
        Assert.IsTrue(nodePermissions[0].Permissions.Contains(ActionMove.ActionLetter));
        Assert.IsFalse(nodePermissions[0].Permissions.Contains(ActionDelete.ActionLetter));
    }

    [Test]
    public async Task GetDocumentPermissionsAsync_Grandchild_Inherits_From_Nearest_Ancestor_With_Explicit_Permissions()
    {
        // Arrange
        var userGroup = await CreateTestUserGroup();
        var user = UserService.CreateUserWithIdentity("test1", "test1@test.com");
        user.AddGroup(userGroup.ToReadOnlyGroup());
        UserService.Save(user);

        var contentType = ContentTypeBuilder.CreateSimpleContentType();
        contentType.AllowedTemplates = null;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var parent = ContentBuilder.CreateSimpleContent(contentType);
        ContentService.Save(parent);
        var child = ContentBuilder.CreateSimpleContent(contentType, "child", parent.Id);
        ContentService.Save(child);
        var grandchild = ContentBuilder.CreateSimpleContent(contentType, "grandchild", child.Id);
        ContentService.Save(grandchild);

        // Set different explicit permissions on parent and child
        ContentService.SetPermission(parent, ActionBrowse.ActionLetter, [userGroup.Id]);
        ContentService.SetPermission(parent, ActionDelete.ActionLetter, [userGroup.Id]);
        ContentService.SetPermission(child, ActionBrowse.ActionLetter, [userGroup.Id]);
        ContentService.SetPermission(child, ActionMove.ActionLetter, [userGroup.Id]);

        // Act
        var result = await UserService
            .GetDocumentPermissionsAsync(user.Key, [grandchild.Key]);

        // Assert - grandchild should inherit from child (nearest ancestor), not parent
        Assert.IsTrue(result.Success);
        var nodePermissions = result.Result.ToArray();
        Assert.AreEqual(1, nodePermissions.Length);
        Assert.AreEqual(grandchild.Key, nodePermissions[0].NodeKey);

        Assert.IsTrue(nodePermissions[0].Permissions.Contains(ActionBrowse.ActionLetter));
        Assert.IsTrue(nodePermissions[0].Permissions.Contains(ActionMove.ActionLetter));
        Assert.IsFalse(nodePermissions[0].Permissions.Contains(ActionDelete.ActionLetter));
    }

    [Test]
    public async Task GetDocumentPermissionsAsync_Returns_Default_Permissions_When_No_Explicit_Permissions_Set()
    {
        // Arrange
        var defaultPermissions = new[] { ActionBrowse.ActionLetter, ActionUpdate.ActionLetter }.ToHashSet();
        var userGroup = UserGroupBuilder.CreateUserGroup(permissions: defaultPermissions);
        await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);

        var user = UserService.CreateUserWithIdentity("test1", "test1@test.com");
        user.AddGroup(userGroup.ToReadOnlyGroup());
        UserService.Save(user);

        var contentType = ContentTypeBuilder.CreateSimpleContentType();
        contentType.AllowedTemplates = null;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var parent = ContentBuilder.CreateSimpleContent(contentType);
        ContentService.Save(parent);
        var child = ContentBuilder.CreateSimpleContent(contentType, "child", parent.Id);
        ContentService.Save(child);

        // No explicit permissions set on any node.

        // Act
        var result = await UserService
            .GetDocumentPermissionsAsync(user.Key, [child.Key]);

        // Assert - child should get the group's default permissions.
        Assert.IsTrue(result.Success);
        var nodePermissions = result.Result.ToArray();
        Assert.AreEqual(1, nodePermissions.Length);
        Assert.AreEqual(child.Key, nodePermissions[0].NodeKey);
        Assert.AreEqual(2, nodePermissions[0].Permissions.Count);
        Assert.IsTrue(nodePermissions[0].Permissions.Contains(ActionBrowse.ActionLetter));
        Assert.IsTrue(nodePermissions[0].Permissions.Contains(ActionUpdate.ActionLetter));
    }

    [Test]
    public async Task GetDocumentPermissionsAsync_Child_Permissions_Do_Not_Influence_Parent()
    {
        // Arrange
        var defaultPermissions = new[] { ActionBrowse.ActionLetter, ActionUpdate.ActionLetter }.ToHashSet();
        var userGroup = UserGroupBuilder.CreateUserGroup(permissions: defaultPermissions);
        await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);

        var user = UserService.CreateUserWithIdentity("test1", "test1@test.com");
        user.AddGroup(userGroup.ToReadOnlyGroup());
        UserService.Save(user);

        var contentType = ContentTypeBuilder.CreateSimpleContentType();
        contentType.AllowedTemplates = null;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var parent = ContentBuilder.CreateSimpleContent(contentType);
        ContentService.Save(parent);
        var child = ContentBuilder.CreateSimpleContent(contentType, "child", parent.Id);
        ContentService.Save(child);

        // Set explicit permissions on the child only
        ContentService.SetPermission(child, ActionDelete.ActionLetter, [userGroup.Id]);
        ContentService.SetPermission(child, ActionMove.ActionLetter, [userGroup.Id]);

        // Act - query permissions for both parent and child
        var result = await UserService
            .GetDocumentPermissionsAsync(user.Key, [parent.Key, child.Key]);

        // Assert
        Assert.IsTrue(result.Success);
        var nodePermissions = result.Result.ToArray();
        Assert.AreEqual(2, nodePermissions.Length);

        var parentPermissions = nodePermissions.Single(x => x.NodeKey == parent.Key);
        var childPermissions = nodePermissions.Single(x => x.NodeKey == child.Key);

        // Parent should have default permissions only, not influenced by child's explicit permissions
        Assert.AreEqual(2, parentPermissions.Permissions.Count);
        Assert.IsTrue(parentPermissions.Permissions.Contains(ActionBrowse.ActionLetter));
        Assert.IsTrue(parentPermissions.Permissions.Contains(ActionUpdate.ActionLetter));

        // Child should have its own explicit permissions
        Assert.AreEqual(2, childPermissions.Permissions.Count);
        Assert.IsTrue(childPermissions.Permissions.Contains(ActionDelete.ActionLetter));
        Assert.IsTrue(childPermissions.Permissions.Contains(ActionMove.ActionLetter));
    }

    [Test]
    public async Task GetDocumentPermissionsAsync_Returns_Failure_For_Unknown_User()
    {
        // Arrange
        var contentType = ContentTypeBuilder.CreateSimpleContentType();
        contentType.AllowedTemplates = null;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var content = ContentBuilder.CreateSimpleContent(contentType);
        ContentService.Save(content);

        // Act
        var result = await UserService
            .GetDocumentPermissionsAsync(Guid.NewGuid(), [content.Key]);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(UserOperationStatus.UserNotFound, result.Status);
    }

    [Test]
    public async Task ReplaceUserGroupPermissions_Replaces_Existing_Permissions()
    {
        // Arrange
        var userGroup = await CreateTestUserGroup();
        var user = UserService.CreateUserWithIdentity("test1", "test1@test.com");
        user.AddGroup(userGroup.ToReadOnlyGroup());
        UserService.Save(user);

        var contentType = ContentTypeBuilder.CreateSimpleContentType();
        contentType.AllowedTemplates = null;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var content = ContentBuilder.CreateSimpleContent(contentType);
        ContentService.Save(content);

        // Set initial permissions
        var initialPermissions = new HashSet<string> { ActionBrowse.ActionLetter, ActionDelete.ActionLetter };
        UserService.ReplaceUserGroupPermissions(userGroup.Id, initialPermissions, content.Id);

        // Act - replace with different permissions
        var newPermissions = new HashSet<string> { ActionMove.ActionLetter, ActionUpdate.ActionLetter };
        UserService.ReplaceUserGroupPermissions(userGroup.Id, newPermissions, content.Id);

        // Assert - verify only the new permissions exist
        var result = await UserService.GetDocumentPermissionsAsync(user.Key, [content.Key]);

        Assert.IsTrue(result.Success);
        var nodePermissions = result.Result.ToArray();
        Assert.AreEqual(1, nodePermissions.Length);

        // Old permissions should be gone
        Assert.IsFalse(nodePermissions[0].Permissions.Contains(ActionBrowse.ActionLetter));
        Assert.IsFalse(nodePermissions[0].Permissions.Contains(ActionDelete.ActionLetter));

        // New permissions should be set
        Assert.IsTrue(nodePermissions[0].Permissions.Contains(ActionMove.ActionLetter));
        Assert.IsTrue(nodePermissions[0].Permissions.Contains(ActionUpdate.ActionLetter));
    }

    [Test]
    public async Task ReplaceUserGroupPermissions_Sets_Permissions_On_Multiple_Entities()
    {
        // Arrange
        var userGroup = await CreateTestUserGroup();
        var user = UserService.CreateUserWithIdentity("test1", "test1@test.com");
        user.AddGroup(userGroup.ToReadOnlyGroup());
        UserService.Save(user);

        var contentType = ContentTypeBuilder.CreateSimpleContentType();
        contentType.AllowedTemplates = null;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var content1 = ContentBuilder.CreateSimpleContent(contentType);
        ContentService.Save(content1);
        var content2 = ContentBuilder.CreateSimpleContent(contentType, "second");
        ContentService.Save(content2);

        var permissions = new HashSet<string> { ActionBrowse.ActionLetter, ActionMove.ActionLetter };

        // Act
        UserService.ReplaceUserGroupPermissions(userGroup.Id, permissions, content1.Id, content2.Id);

        // Assert - verify permissions on both content items
        var result = await UserService.GetDocumentPermissionsAsync(user.Key, [content1.Key, content2.Key]);

        Assert.IsTrue(result.Success);
        var nodePermissions = result.Result.ToArray();
        Assert.AreEqual(2, nodePermissions.Length);

        var perms1 = nodePermissions.Single(x => x.NodeKey == content1.Key);
        var perms2 = nodePermissions.Single(x => x.NodeKey == content2.Key);

        Assert.IsTrue(perms1.Permissions.Contains(ActionBrowse.ActionLetter));
        Assert.IsTrue(perms1.Permissions.Contains(ActionMove.ActionLetter));
        Assert.IsTrue(perms2.Permissions.Contains(ActionBrowse.ActionLetter));
        Assert.IsTrue(perms2.Permissions.Contains(ActionMove.ActionLetter));
    }

    [Test]
    public async Task AssignUserGroupPermission_Sets_Permission_On_Multiple_Entities()
    {
        // Arrange
        var userGroup = await CreateTestUserGroup();
        var user = UserService.CreateUserWithIdentity("test1", "test1@test.com");
        user.AddGroup(userGroup.ToReadOnlyGroup());
        UserService.Save(user);

        var contentType = ContentTypeBuilder.CreateSimpleContentType();
        contentType.AllowedTemplates = null;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var content1 = ContentBuilder.CreateSimpleContent(contentType);
        ContentService.Save(content1);
        var content2 = ContentBuilder.CreateSimpleContent(contentType, "second");
        ContentService.Save(content2);

        // Act
        UserService.AssignUserGroupPermission(userGroup.Id, ActionBrowse.ActionLetter, content1.Id, content2.Id);

        // Assert - verify permissions on both content items
        var result = await UserService.GetDocumentPermissionsAsync(user.Key, [content1.Key, content2.Key]);

        Assert.IsTrue(result.Success);
        var nodePermissions = result.Result.ToArray();
        Assert.AreEqual(2, nodePermissions.Length);

        var perms1 = nodePermissions.Single(x => x.NodeKey == content1.Key);
        var perms2 = nodePermissions.Single(x => x.NodeKey == content2.Key);

        Assert.IsTrue(perms1.Permissions.Contains(ActionBrowse.ActionLetter));
        Assert.IsTrue(perms2.Permissions.Contains(ActionBrowse.ActionLetter));
    }

    [Test]
    public async Task AssignUserGroupPermission_Adds_To_Existing_Permissions()
    {
        // Arrange
        var userGroup = await CreateTestUserGroup();
        var user = UserService.CreateUserWithIdentity("test1", "test1@test.com");
        user.AddGroup(userGroup.ToReadOnlyGroup());
        UserService.Save(user);

        var contentType = ContentTypeBuilder.CreateSimpleContentType();
        contentType.AllowedTemplates = null;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var content = ContentBuilder.CreateSimpleContent(contentType);
        ContentService.Save(content);

        // Set initial permission
        UserService.AssignUserGroupPermission(userGroup.Id, ActionBrowse.ActionLetter, content.Id);

        // Act - assign an additional permission
        UserService.AssignUserGroupPermission(userGroup.Id, ActionDelete.ActionLetter, content.Id);

        // Assert - both permissions should exist
        var result = await UserService.GetDocumentPermissionsAsync(user.Key, [content.Key]);

        Assert.IsTrue(result.Success);
        var nodePermissions = result.Result.ToArray();
        Assert.AreEqual(1, nodePermissions.Length);
        Assert.IsTrue(nodePermissions[0].Permissions.Contains(ActionBrowse.ActionLetter));
        Assert.IsTrue(nodePermissions[0].Permissions.Contains(ActionDelete.ActionLetter));
    }
}
