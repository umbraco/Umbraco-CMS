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
internal sealed class UserGroupServiceTests : UmbracoIntegrationTest
{
    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private IShortStringHelper ShortStringHelper => GetRequiredService<IShortStringHelper>();

    [Test]
    public async Task Can_Create_User_Group()
    {
        var allowedSections = new[] { "content", "media", "settings" };
        var userGroup = new UserGroup(ShortStringHelper)
        {
            Name = "Some Name",
            Alias = "someAlias",
            Description = "This is a test user group description",
            Icon = "icon-users",
            HasAccessToAllLanguages = true,
            Permissions = new HashSet<string> { "A", "B", "C" }
        };

        foreach (var allowedSection in allowedSections)
        {
            userGroup.AddAllowedSection(allowedSection);
        }

        var result = await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);
        var createdUserGroup = await UserGroupService.GetAsync(result.Result.Key);

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(createdUserGroup);
        Assert.AreEqual(userGroup.Name, createdUserGroup.Name);
        Assert.AreEqual(userGroup.Alias, createdUserGroup.Alias);
        Assert.AreEqual(userGroup.Description, createdUserGroup.Description);
        Assert.AreEqual(userGroup.Icon, createdUserGroup.Icon);
        Assert.AreEqual(userGroup.HasAccessToAllLanguages, createdUserGroup.HasAccessToAllLanguages);
        CollectionAssert.AreEquivalent(userGroup.Permissions, createdUserGroup.Permissions);
        CollectionAssert.AreEquivalent(userGroup.AllowedSections, createdUserGroup.AllowedSections);
    }

    [Test]
    public async Task Can_Update_User_Group()
    {
        var allowedSections = new[] { "content", "media", "settings" };
        var userGroup = new UserGroup(ShortStringHelper)
        {
            Name = "Some Name",
            Alias = "someAlias",
            Description = "This is a test user group description",
            Icon = "icon-users",
            HasAccessToAllLanguages = true,
            Permissions = new HashSet<string> { "A", "B", "C" }
        };

        foreach (var allowedSection in allowedSections)
        {
            userGroup.AddAllowedSection(allowedSection);
        }

        var createResult = await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);
        var createdUserGroup = await UserGroupService.GetAsync(createResult.Result.Key);

        Assert.IsTrue(createResult.Success);
        createdUserGroup.Name = "Updated Name";
        createdUserGroup.Alias = "updatedAlias";
        createdUserGroup.Description = "Updated description";
        createdUserGroup.Icon = "icon-user";
        createdUserGroup.HasAccessToAllLanguages = false;
        createdUserGroup.Permissions = new HashSet<string> { "X", "Y", "Z" };
        createdUserGroup.ClearAllowedSections();
        createdUserGroup.AddAllowedSection("users");

        var updateResult = await UserGroupService.UpdateAsync(createdUserGroup, Constants.Security.SuperUserKey);
        var updatedUserGroup = await UserGroupService.GetAsync(updateResult.Result.Key);

        Assert.IsTrue(updateResult.Success);
        Assert.IsNotNull(updatedUserGroup);
        Assert.AreEqual(createdUserGroup.Name, updatedUserGroup.Name);
        Assert.AreEqual(createdUserGroup.Alias, updatedUserGroup.Alias);
        Assert.AreEqual(createdUserGroup.Description, updatedUserGroup.Description);
        Assert.AreEqual(createdUserGroup.Icon, updatedUserGroup.Icon);
        Assert.AreEqual(createdUserGroup.HasAccessToAllLanguages, updatedUserGroup.HasAccessToAllLanguages);
        CollectionAssert.AreEquivalent(createdUserGroup.Permissions, updatedUserGroup.Permissions);
        CollectionAssert.AreEquivalent(createdUserGroup.AllowedSections, updatedUserGroup.AllowedSections);
    }

    [Test]
    public async Task Cannot_Create_User_Group_With_Name_Equals_Null()
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
    public async Task Cannot_Create_User_Group_With_Name_Longer_Than_Max_Length()
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
    public async Task Cannot_Create_User_Group_With_Alias_Longer_Than_Max_Length()
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
    public async Task Cannot_Update_Non_Existing_User_Group()
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
    public async Task Cannot_Create_Existing_User_Group()
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
    public async Task Cannot_Create_User_Group_With_Duplicate_Alias()
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
    public async Task Cannot_Update_User_Group_With_Duplicate_Alias()
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
    public async Task Can_Update_User_Group_To_New_Name()
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
    public async Task Cannot_Delete_System_User_Group(string userGroupKeyAsString, string expectedGroupAlias)
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
    public async Task Can_Delete_Non_System_User_Group(string userGroupKeyAsString, string expectedGroupAlias)
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
