// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.Membership.Permissions;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

/// <summary>
///     Integration tests for <see cref="IElementPermissionService.GetPermissionsAsync"/> proving equivalence
///     with the legacy <c>IUserService.GetPermissionsForPath</c> algorithm that the
///     <c>ElementPermissionMapper</c> used before being routed through the permission service.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class ElementPermissionServiceTests : UmbracoIntegrationTest
{
    private IElementPermissionService ElementPermissionService => GetRequiredService<IElementPermissionService>();

    private IUserService UserService => GetRequiredService<IUserService>();

    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IElementEditingService ElementEditingService => GetRequiredService<IElementEditingService>();

    private IElementContainerService ElementContainerService => GetRequiredService<IElementContainerService>();

    private IEntityService EntityService => GetRequiredService<IEntityService>();

    [Test]
    public async Task GetPermissionsAsync_Returns_Explicit_Granular_Permissions()
    {
        // Arrange
        var elementKey = await CreateElement();
        var user = await CreateUserInGroup(
            granularPermissions: [ActionElementBrowse.ActionLetter, ActionElementDelete.ActionLetter],
            defaultPermissions: [],
            elementKey: elementKey);

        // Act - resolve through the service and through the legacy path-based algorithm.
        NodePermissions[] viaService = (await ElementPermissionService.GetPermissionsAsync(user, [elementKey])).ToArray();
        ISet<string> viaPath = UserService.GetPermissionsForPath(user, GetPath(elementKey)).GetAllPermissions();

        // Assert
        Assert.That(viaService, Has.Length.EqualTo(1));
        Assert.That(viaService[0].NodeKey, Is.EqualTo(elementKey));
        Assert.That(viaService[0].Permissions, Contains.Item(ActionElementBrowse.ActionLetter));
        Assert.That(viaService[0].Permissions, Contains.Item(ActionElementDelete.ActionLetter));

        // Assert equivalence with the legacy algorithm.
        Assert.That(viaService[0].Permissions, Is.EquivalentTo(viaPath));
    }

    [Test]
    public async Task GetPermissionsAsync_Returns_Default_Permissions_When_No_Granular_Set()
    {
        // Arrange
        var elementKey = await CreateElement();
        var user = await CreateUserInGroup(
            granularPermissions: [],
            defaultPermissions: [ActionElementBrowse.ActionLetter, ActionElementUpdate.ActionLetter],
            elementKey: elementKey);

        // Act
        NodePermissions[] viaService = (await ElementPermissionService.GetPermissionsAsync(user, [elementKey])).ToArray();
        ISet<string> viaPath = UserService.GetPermissionsForPath(user, GetPath(elementKey)).GetAllPermissions();

        // Assert - falls back to the group's default permissions.
        Assert.That(viaService, Has.Length.EqualTo(1));
        Assert.That(viaService[0].NodeKey, Is.EqualTo(elementKey));
        Assert.That(viaService[0].Permissions, Contains.Item(ActionElementBrowse.ActionLetter));
        Assert.That(viaService[0].Permissions, Contains.Item(ActionElementUpdate.ActionLetter));

        Assert.That(viaService[0].Permissions, Is.EquivalentTo(viaPath));
    }

    [Test]
    public async Task GetPermissionsAsync_Returns_Permissions_Granted_At_An_Ancestor_ElementContainer()
    {
        var folderResult = await ElementContainerService.CreateAsync(
            null,
            Guid.NewGuid().ToString(),
            null,
            Constants.Security.SuperUserKey);
        Assert.IsTrue(folderResult.Success, $"Failed to create folder: {folderResult.Status}");
        var folderKey = folderResult.Result!.Key;

        var elementKey = await CreateElement(folderKey);
        var user = await CreateUserInGroupWithContainerGrant(
            granularPermissions: [ActionElementNew.ActionLetter],
            containerKey: folderKey);

        // Act
        NodePermissions[] viaService = (await ElementPermissionService.GetPermissionsAsync(user, [elementKey])).ToArray();
        ISet<string> viaPath = UserService.GetPermissionsForPath(user, GetPath(elementKey)).GetAllPermissions();

        // Assert - the ancestor folder's grant surfaces for the descendant element...
        Assert.That(viaService, Has.Length.EqualTo(1));
        Assert.That(viaService[0].NodeKey, Is.EqualTo(elementKey));
        Assert.That(viaService[0].Permissions, Contains.Item(ActionElementNew.ActionLetter));

        // ...but a verb that was never granted is absent.
        Assert.That(viaService[0].Permissions, Does.Not.Contain(ActionElementDelete.ActionLetter));

        Assert.That(viaService[0].Permissions, Is.EquivalentTo(viaPath));
    }

    private async Task<Guid> CreateElement(Guid? parentKey = null)
    {
        var contentType = ContentTypeBuilder.CreateBasicContentType(
            name: Guid.NewGuid().ToString(),
            alias: Guid.NewGuid().ToString());
        contentType.IsElement = true;
        contentType.AllowedInLibrary = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var result = await ElementEditingService.CreateAsync(
            new ElementCreateModel
            {
                ContentTypeKey = contentType.Key,
                ParentKey = parentKey,
                Variants = [new VariantModel { Name = "Test Element" }],
            },
            Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success, $"Failed to create element with status {result.Status}.");
        return result.Result.Content!.Key;
    }

    private string GetPath(Guid elementKey) => EntityService.Get(elementKey)!.Path;

    private async Task<IUser> CreateUserInGroupWithContainerGrant(string[] granularPermissions, Guid containerKey)
    {
        IGranularPermission[] granular = granularPermissions
            .Select(verb => (IGranularPermission)new ElementContainerGranularPermission { Key = containerKey, Permission = verb })
            .ToArray();

        var userGroup = new UserGroupBuilder()
            .WithName(Guid.NewGuid().ToString())
            .WithAlias(Guid.NewGuid().ToString())
            .WithPermissions(new HashSet<string>())
            .WithGranularPermissions(granular)
            .Build();
        var createGroupResult = await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);
        Assert.IsTrue(createGroupResult.Success, $"Failed to create user group with status {createGroupResult.Status}.");

        var user = UserService.CreateUserWithIdentity(Guid.NewGuid().ToString(), $"{Guid.NewGuid()}@test.com");
        user.AddGroup(userGroup.ToReadOnlyGroup());
        UserService.Save(user);

        return user;
    }

    private async Task<IUser> CreateUserInGroup(string[] granularPermissions, string[] defaultPermissions, Guid elementKey)
    {
        IGranularPermission[] granular = granularPermissions
            .Select(verb => (IGranularPermission)new ElementGranularPermission { Key = elementKey, Permission = verb })
            .ToArray();

        var userGroup = new UserGroupBuilder()
            .WithName(Guid.NewGuid().ToString())
            .WithAlias(Guid.NewGuid().ToString())
            .WithPermissions(defaultPermissions.ToHashSet())
            .WithGranularPermissions(granular)
            .Build();
        var createGroupResult = await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);
        Assert.IsTrue(createGroupResult.Success, $"Failed to create user group with status {createGroupResult.Status}.");

        var user = UserService.CreateUserWithIdentity(Guid.NewGuid().ToString(), $"{Guid.NewGuid()}@test.com");
        user.AddGroup(userGroup.ToReadOnlyGroup());
        UserService.Save(user);

        return user;
    }
}
