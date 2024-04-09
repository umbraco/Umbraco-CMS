using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class UserGroupServiceValidationTests : UmbracoIntegrationTest
{
    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private IShortStringHelper ShortStringHelper => GetRequiredService<IShortStringHelper>();

    [Test]
    public async Task Cannot_create_user_group_with_name_equals_null()
    {
        var userGroup = new UserGroup(ShortStringHelper)
        {
            Name = null
        };

        var result = await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(UserGroupOperationStatus.MissingName, result.Status);
    }

    [Test]
    public async Task Cannot_create_user_group_with_name_longer_than_max_length()
    {
        var userGroup = new UserGroup(ShortStringHelper)
        {
            Name = "Sed porttitor lectus nibh. Vivamus magna justo, lacinia eget consectetur sed, convallis at tellus. Vivamus suscipit tortor eget felis porttitor volutpat. Quisque velit nisi, pretium ut lacinia in, elementum id enim."
        };

        var result = await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(UserGroupOperationStatus.NameTooLong, result.Status);
    }

    [Test]
    public async Task Cannot_create_user_group_with_alias_longer_than_max_length()
    {
        var userGroup = new UserGroup(ShortStringHelper)
        {
            Name = "Some Name",
            Alias = "Sed porttitor lectus nibh. Vivamus magna justo, lacinia eget consectetur sed, convallis at tellus. Vivamus suscipit tortor eget felis porttitor volutpat. Quisque velit nisi, pretium ut lacinia in, elementum id enim. Vivamus suscipit tortor eget felis porttitor volutpat. Quisque velit nisi, pretium ut lacinia in, elementum id enim."
        };

        var result = await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(UserGroupOperationStatus.AliasTooLong, result.Status);
    }

    [Test]
    public async Task Cannot_update_non_existing_user_group()
    {
        var userGroup = new UserGroup(ShortStringHelper)
        {
            Name = "Some Name",
            Alias = "someAlias"
        };

        var result = await UserGroupService.UpdateAsync(userGroup, Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(UserGroupOperationStatus.NotFound, result.Status);
    }

    [Test]
    public async Task Cannot_create_existing_user_group()
    {
        var userGroup = new UserGroup(ShortStringHelper)
        {
            Name = "Some Name",
            Alias = "someAlias"
        };

        var result = await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);

        result = await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(UserGroupOperationStatus.AlreadyExists, result.Status);
    }

    [Test]
    public async Task Cannot_create_user_group_with_duplicate_alias()
    {
        var alias = "duplicateAlias";

        var existingUserGroup = new UserGroup(ShortStringHelper)
        {
            Name = "I already exist",
            Alias = alias
        };
        var setupResult = await UserGroupService.CreateAsync(existingUserGroup, Constants.Security.SuperUserKey);
        Assert.IsTrue(setupResult.Success);

        var newUserGroup = new UserGroup(ShortStringHelper)
        {
            Name = "I have a duplicate alias",
            Alias = alias,
        };
        var result = await UserGroupService.CreateAsync(newUserGroup, Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(UserGroupOperationStatus.DuplicateAlias, result.Status);
    }

    [Test]
    public async Task Cannot_update_user_group_with_duplicate_alias()
    {
        var alias = "duplicateAlias";

        var existingUserGroup = new UserGroup(ShortStringHelper)
        {
            Name = "I already exist",
            Alias = alias
        };
        var setupResult = await UserGroupService.CreateAsync(existingUserGroup, Constants.Security.SuperUserKey);
        Assert.IsTrue(setupResult.Success);

        IUserGroup userGroupToUpdate = new UserGroup(ShortStringHelper)
        {
            Name = "I don't have a duplicate alias",
            Alias = "somAlias",
        };
        var creationResult = await UserGroupService.CreateAsync(userGroupToUpdate, Constants.Security.SuperUserKey);
        Assert.IsTrue(creationResult.Success);


        userGroupToUpdate = creationResult.Result;
        userGroupToUpdate.Name = "Now I have a duplicate alias";
        userGroupToUpdate.Alias = alias;

        var updateResult = await UserGroupService.UpdateAsync(userGroupToUpdate, Constants.Security.SuperUserKey);
        Assert.IsFalse(updateResult.Success);
        Assert.AreEqual(UserGroupOperationStatus.DuplicateAlias, updateResult.Status);
    }

    [Test]
    public async Task Can_Update_UserGroup_To_New_Name()
    {
        var userGroup = new UserGroup(ShortStringHelper)
        {
            Name = "Some Name",
            Alias = "someAlias"
        };
        var setupResult = await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);
        Assert.IsTrue(setupResult.Success);


        var updateName = "New Name";
        userGroup.Name = updateName;
        var updateResult = await UserGroupService.UpdateAsync(userGroup, Constants.Security.SuperUserKey);
        Assert.IsTrue(updateResult.Success);
        var updatedGroup = updateResult.Result;
        Assert.AreEqual(updateName, updatedGroup.Name);
    }

    // these keys are not defined as "const" in Constants.Security but as "static readonly", so we have to hardcode
    // them here ("static readonly" can't be used as testcase inputs as they are not constants)
    [TestCase("E5E7F6C8-7F9C-4B5B-8D5D-9E1E5A4F7E4D", "admin")]
    [TestCase("8C6AD70F-D307-4E4A-AF58-72C2E4E9439D", "sensitiveData")]
    [TestCase("F2012E4C-D232-4BD1-8EAE-4384032D97D8", "translator")]
    public async Task Cannot_Delete_System_UserGroups(string userGroupKeyAsString, string expectedGroupAlias)
    {
        // since we can't use the constants as input, let's make sure we don't get false positives by double checking the group alias
        var key = Guid.Parse(userGroupKeyAsString);
        var userGroup = await UserGroupService.GetAsync(key);
        Assert.IsNotNull(userGroup);
        Assert.AreEqual(expectedGroupAlias, userGroup.Alias);

        var result = await UserGroupService.DeleteAsync(key);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(UserGroupOperationStatus.IsSystemUserGroup, result.Result);
    }

    // these keys are not defined as "const" in Constants.Security but as "static readonly", so we have to hardcode
    // them here ("static readonly" can't be used as testcase inputs as they are not constants)
    [TestCase("44DC260E-B4D4-4DD9-9081-EEC5598F1641", "editor")]
    [TestCase("9FC2A16F-528C-46D6-A014-75BF4EC2480C", "writer")]
    public async Task Can_Delete_Non_System_UserGroups(string userGroupKeyAsString, string expectedGroupAlias)
    {
        // since we can't use the constants as input, let's make sure we don't get false positives by double checking the group alias
        var key = Guid.Parse(userGroupKeyAsString);
        var userGroup = await UserGroupService.GetAsync(key);
        Assert.IsNotNull(userGroup);
        Assert.AreEqual(expectedGroupAlias, userGroup.Alias);

        var result = await UserGroupService.DeleteAsync(key);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(UserGroupOperationStatus.Success, result.Result);
    }
}
