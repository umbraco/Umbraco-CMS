// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

/// <summary>
///     Integration tests for <see cref="IContentPermissionService.GetPermissionsAsync"/> proving equivalence
///     with <see cref="IUserService.GetDocumentPermissionsAsync"/> when using the shipped implementation.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class ContentPermissionServiceTests : UmbracoIntegrationTest
{
    private IContentPermissionService ContentPermissionService => GetRequiredService<IContentPermissionService>();

    private IUserService UserService => GetRequiredService<IUserService>();

    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    [Test]
    public async Task GetPermissionsAsync_Child_Inherits_Parent_Permissions()
    {
        // Arrange
        var (user, userGroup) = await CreateTestUserAndGroup();

        var contentType = ContentTypeBuilder.CreateSimpleContentType();
        contentType.AllowedTemplates = null;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var parent = ContentBuilder.CreateSimpleContent(contentType);
        ContentService.Save(parent);
        var child = ContentBuilder.CreateSimpleContent(contentType, "child", parent.Id);
        ContentService.Save(child);

        ContentService.SetPermission(parent, ActionBrowse.ActionLetter, [userGroup.Id]);
        ContentService.SetPermission(parent, ActionDelete.ActionLetter, [userGroup.Id]);

        // Act - call both services
        NodePermissions[] viaPermissionService = (await ContentPermissionService.GetPermissionsAsync(user, [child.Key])).ToArray();
        Attempt<IEnumerable<NodePermissions>, UserOperationStatus> viaUserService = await UserService.GetDocumentPermissionsAsync(user.Key, [child.Key]);

        // Assert - results are identical
        Assert.That(viaPermissionService, Has.Length.EqualTo(1));
        Assert.That(viaPermissionService[0].NodeKey, Is.EqualTo(child.Key));
        Assert.That(viaPermissionService[0].Permissions, Contains.Item(ActionBrowse.ActionLetter));
        Assert.That(viaPermissionService[0].Permissions, Contains.Item(ActionDelete.ActionLetter));

        // Assert equivalence with IUserService path
        Assert.That(viaUserService.Success, Is.True);
        NodePermissions[] userServiceResult = viaUserService.Result.ToArray();
        Assert.That(viaPermissionService[0].Permissions, Is.EquivalentTo(userServiceResult[0].Permissions));
    }

    [Test]
    public async Task GetPermissionsAsync_Grandchild_Inherits_Parent_Permissions()
    {
        // Arrange
        var (user, userGroup) = await CreateTestUserAndGroup();

        var contentType = ContentTypeBuilder.CreateSimpleContentType();
        contentType.AllowedTemplates = null;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var parent = ContentBuilder.CreateSimpleContent(contentType);
        ContentService.Save(parent);
        var child = ContentBuilder.CreateSimpleContent(contentType, "child", parent.Id);
        ContentService.Save(child);
        var grandchild = ContentBuilder.CreateSimpleContent(contentType, "grandchild", child.Id);
        ContentService.Save(grandchild);

        ContentService.SetPermission(parent, ActionBrowse.ActionLetter, [userGroup.Id]);
        ContentService.SetPermission(parent, ActionMove.ActionLetter, [userGroup.Id]);

        // Act
        NodePermissions[] viaPermissionService = (await ContentPermissionService.GetPermissionsAsync(user, [grandchild.Key])).ToArray();
        Attempt<IEnumerable<NodePermissions>, UserOperationStatus> viaUserService = await UserService.GetDocumentPermissionsAsync(user.Key, [grandchild.Key]);

        // Assert
        Assert.That(viaPermissionService, Has.Length.EqualTo(1));
        Assert.That(viaPermissionService[0].Permissions, Contains.Item(ActionBrowse.ActionLetter));
        Assert.That(viaPermissionService[0].Permissions, Contains.Item(ActionMove.ActionLetter));

        Assert.That(viaUserService.Success, Is.True);
        Assert.That(viaPermissionService[0].Permissions, Is.EquivalentTo(viaUserService.Result.First().Permissions));
    }

    [Test]
    public async Task GetPermissionsAsync_Returns_Explicit_Permissions_When_Set()
    {
        // Arrange
        var (user, userGroup) = await CreateTestUserAndGroup();

        var contentType = ContentTypeBuilder.CreateSimpleContentType();
        contentType.AllowedTemplates = null;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var parent = ContentBuilder.CreateSimpleContent(contentType);
        ContentService.Save(parent);
        var child = ContentBuilder.CreateSimpleContent(contentType, "child", parent.Id);
        ContentService.Save(child);

        // Different permissions on parent and child
        ContentService.SetPermission(parent, ActionBrowse.ActionLetter, [userGroup.Id]);
        ContentService.SetPermission(parent, ActionDelete.ActionLetter, [userGroup.Id]);
        ContentService.SetPermission(child, ActionBrowse.ActionLetter, [userGroup.Id]);
        ContentService.SetPermission(child, ActionMove.ActionLetter, [userGroup.Id]);

        // Act
        NodePermissions[] viaPermissionService = (await ContentPermissionService.GetPermissionsAsync(user, [child.Key])).ToArray();
        Attempt<IEnumerable<NodePermissions>, UserOperationStatus> viaUserService = await UserService.GetDocumentPermissionsAsync(user.Key, [child.Key]);

        // Assert - child uses its own explicit permissions
        Assert.That(viaPermissionService, Has.Length.EqualTo(1));
        Assert.That(viaPermissionService[0].Permissions, Contains.Item(ActionBrowse.ActionLetter));
        Assert.That(viaPermissionService[0].Permissions, Contains.Item(ActionMove.ActionLetter));
        Assert.That(viaPermissionService[0].Permissions, Does.Not.Contain(ActionDelete.ActionLetter));

        Assert.That(viaUserService.Success, Is.True);
        Assert.That(viaPermissionService[0].Permissions, Is.EquivalentTo(viaUserService.Result.First().Permissions));
    }

    [Test]
    public async Task GetPermissionsAsync_Grandchild_Inherits_From_Nearest_Ancestor()
    {
        // Arrange
        var (user, userGroup) = await CreateTestUserAndGroup();

        var contentType = ContentTypeBuilder.CreateSimpleContentType();
        contentType.AllowedTemplates = null;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var parent = ContentBuilder.CreateSimpleContent(contentType);
        ContentService.Save(parent);
        var child = ContentBuilder.CreateSimpleContent(contentType, "child", parent.Id);
        ContentService.Save(child);
        var grandchild = ContentBuilder.CreateSimpleContent(contentType, "grandchild", child.Id);
        ContentService.Save(grandchild);

        ContentService.SetPermission(parent, ActionBrowse.ActionLetter, [userGroup.Id]);
        ContentService.SetPermission(parent, ActionDelete.ActionLetter, [userGroup.Id]);
        ContentService.SetPermission(child, ActionBrowse.ActionLetter, [userGroup.Id]);
        ContentService.SetPermission(child, ActionMove.ActionLetter, [userGroup.Id]);

        // Act
        NodePermissions[] viaPermissionService = (await ContentPermissionService.GetPermissionsAsync(user, [grandchild.Key])).ToArray();
        Attempt<IEnumerable<NodePermissions>, UserOperationStatus> viaUserService = await UserService.GetDocumentPermissionsAsync(user.Key, [grandchild.Key]);

        // Assert - grandchild inherits from child (nearest ancestor), not parent
        Assert.That(viaPermissionService, Has.Length.EqualTo(1));
        Assert.That(viaPermissionService[0].Permissions, Contains.Item(ActionBrowse.ActionLetter));
        Assert.That(viaPermissionService[0].Permissions, Contains.Item(ActionMove.ActionLetter));
        Assert.That(viaPermissionService[0].Permissions, Does.Not.Contain(ActionDelete.ActionLetter));

        Assert.That(viaUserService.Success, Is.True);
        Assert.That(viaPermissionService[0].Permissions, Is.EquivalentTo(viaUserService.Result.First().Permissions));
    }

    [Test]
    public async Task GetPermissionsAsync_Returns_Default_Permissions_When_No_Explicit_Set()
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
        NodePermissions[] viaPermissionService = (await ContentPermissionService.GetPermissionsAsync(user, [child.Key])).ToArray();
        Attempt<IEnumerable<NodePermissions>, UserOperationStatus> viaUserService = await UserService.GetDocumentPermissionsAsync(user.Key, [child.Key]);

        // Assert - should get the group's default permissions
        Assert.That(viaPermissionService, Has.Length.EqualTo(1));
        Assert.That(viaPermissionService[0].NodeKey, Is.EqualTo(child.Key));
        Assert.That(viaPermissionService[0].Permissions, Contains.Item(ActionBrowse.ActionLetter));
        Assert.That(viaPermissionService[0].Permissions, Contains.Item(ActionUpdate.ActionLetter));

        Assert.That(viaUserService.Success, Is.True);
        Assert.That(viaPermissionService[0].Permissions, Is.EquivalentTo(viaUserService.Result.First().Permissions));
    }

    [Test]
    public async Task GetPermissionsAsync_Returns_Empty_For_Empty_Keys()
    {
        // Arrange
        var (user, _) = await CreateTestUserAndGroup();

        // Act
        NodePermissions[] result = (await ContentPermissionService.GetPermissionsAsync(user, [])).ToArray();

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetPermissionsAsync_Returns_Empty_For_Unknown_Content()
    {
        // Arrange
        var (user, _) = await CreateTestUserAndGroup();

        // Act
        NodePermissions[] result = (await ContentPermissionService.GetPermissionsAsync(user, [Guid.NewGuid()])).ToArray();

        // Assert
        Assert.That(result, Is.Empty);
    }

    private async Task<(IUser User, IUserGroup UserGroup)> CreateTestUserAndGroup()
    {
        var userGroup = UserGroupBuilder.CreateUserGroup();
        await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);

        var user = UserService.CreateUserWithIdentity("test1", "test1@test.com");
        user.AddGroup(userGroup.ToReadOnlyGroup());
        UserService.Save(user);

        return (user, userGroup);
    }
}
