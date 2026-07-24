// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.Membership.Permissions;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

/// <summary>
///     Integration tests for <see cref="IElementContainerPermissionService.GetPermissionsAsync"/>, the
///     concrete implementation's path-based resolution scoped to <see cref="UmbracoObjectTypes.ElementContainer"/>.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class ElementContainerPermissionServiceTests : UmbracoIntegrationTest
{
    private IElementContainerPermissionService ElementContainerPermissionService => GetRequiredService<IElementContainerPermissionService>();

    private IUserService UserService => GetRequiredService<IUserService>();

    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private IElementContainerService ElementContainerService => GetRequiredService<IElementContainerService>();

    [Test]
    public async Task GetPermissionsAsync_Returns_Mixed_Container_And_Element_Granular_Permissions()
    {
        var folderKey = await CreateFolder();
        var user = await CreateUserInGroup(
            granularPermissions: [ActionElementContainerNew.ActionLetter, ActionElementNew.ActionLetter],
            defaultPermissions: [],
            folderKey: folderKey);

        // Act
        NodePermissions[] result = (await ElementContainerPermissionService.GetPermissionsAsync(user, [folderKey])).ToArray();

        // Assert
        Assert.That(result, Has.Length.EqualTo(1));
        Assert.That(result[0].NodeKey, Is.EqualTo(folderKey));
        Assert.That(
            result[0].Permissions,
            Is.EquivalentTo((string[])[ActionElementContainerNew.ActionLetter, ActionElementNew.ActionLetter]));
    }

    [Test]
    public async Task GetPermissionsAsync_Returns_Default_Permissions_When_No_Granular_Set()
    {
        // Arrange
        var folderKey = await CreateFolder();
        var user = await CreateUserInGroup(
            granularPermissions: [],
            defaultPermissions: [ActionElementContainerBrowse.ActionLetter],
            folderKey: folderKey);

        // Act
        NodePermissions[] result = (await ElementContainerPermissionService.GetPermissionsAsync(user, [folderKey])).ToArray();

        // Assert - falls back to the group's default permissions.
        Assert.That(result, Has.Length.EqualTo(1));
        Assert.That(result[0].NodeKey, Is.EqualTo(folderKey));
        Assert.That(result[0].Permissions, Is.EquivalentTo(new[] { ActionElementContainerBrowse.ActionLetter }));
    }

    [Test]
    public async Task GetPermissionsAsync_Cascades_Grant_To_A_Nested_Subfolder()
    {
        var parentFolderKey = await CreateFolder();
        var subfolderKey = await CreateFolder(parentFolderKey);

        var user = await CreateUserInGroup(
            granularPermissions: [ActionElementContainerNew.ActionLetter],
            defaultPermissions: [],
            folderKey: parentFolderKey);

        // Act
        NodePermissions[] result = (await ElementContainerPermissionService.GetPermissionsAsync(user, [subfolderKey])).ToArray();

        // Assert - the parent's grant surfaces for the subfolder...
        Assert.That(result, Has.Length.EqualTo(1));
        Assert.That(result[0].NodeKey, Is.EqualTo(subfolderKey));
        Assert.That(result[0].Permissions, Contains.Item(ActionElementContainerNew.ActionLetter));

        // ...but a verb that was never granted is absent.
        Assert.That(result[0].Permissions, Does.Not.Contain(ActionElementContainerDelete.ActionLetter));
    }

    private async Task<Guid> CreateFolder(Guid? parentKey = null)
    {
        var result = await ElementContainerService.CreateAsync(null, Guid.NewGuid().ToString(), parentKey, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success, $"Failed to create folder: {result.Status}");
        return result.Result!.Key;
    }

    private async Task<IUser> CreateUserInGroup(string[] granularPermissions, string[] defaultPermissions, Guid folderKey)
    {
        IGranularPermission[] granular = granularPermissions
            .Select(verb => (IGranularPermission)new ElementContainerGranularPermission { Key = folderKey, Permission = verb })
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
