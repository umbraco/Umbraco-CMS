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
internal sealed class UserGroupServiceValidationTests : UmbracoIntegrationTest
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

    [TestCase(Constants.Security.AdminGroupKeyString, "admin")]
    [TestCase(Constants.Security.SensitiveDataGroupKeyString, "sensitiveData")]
    [TestCase(Constants.Security.TranslatorGroupString, "translator")]
    public async Task Cannot_Delete_System_UserGroups(string userGroupKeyAsString, string expectedGroupAlias)
    {
        // since we can't use the constants as input, let's make sure we don't get false positives by double checking the group alias
        var key = Guid.Parse(userGroupKeyAsString);
        var userGroup = await UserGroupService.GetAsync(key);
        Assert.IsNotNull(userGroup);
        Assert.AreEqual(expectedGroupAlias, userGroup.Alias);

        var result = await UserGroupService.DeleteAsync(key);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(UserGroupOperationStatus.CanNotDeleteIsSystemUserGroup, result.Result);
    }

    [TestCase( Constants.Security.EditorGroupKeyString, "editor")]
    [TestCase(Constants.Security.WriterGroupKeyString, "writer")]
    public async Task Can_Delete_Non_System_UserGroups(string userGroupKeyAsString, string expectedGroupAlias)
    {
        // since we can't use the constants as input, let's make sure we don't get false positives by double checking the group alias
        var key = Guid.Parse(userGroupKeyAsString);
        var userGroup = await UserGroupService.GetAsync(key);
        Assert.IsNotNull(userGroup);
        Assert.AreEqual(expectedGroupAlias, userGroup.Alias);

        var result = await UserGroupService.DeleteAsync(key);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(result.Result,UserGroupOperationStatus.Success);
    }
}
