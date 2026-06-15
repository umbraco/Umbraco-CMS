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

        Assert.That(result.Success, Is.True);
        Assert.That(createdUserGroup, Is.Not.Null);
        Assert.That(createdUserGroup.Name, Is.EqualTo(userGroup.Name));
        Assert.That(createdUserGroup.Alias, Is.EqualTo(userGroup.Alias));
        Assert.That(createdUserGroup.Description, Is.EqualTo(userGroup.Description));
        Assert.That(createdUserGroup.Icon, Is.EqualTo(userGroup.Icon));
        Assert.That(createdUserGroup.HasAccessToAllLanguages, Is.EqualTo(userGroup.HasAccessToAllLanguages));
        Assert.That(createdUserGroup.Permissions, Is.EquivalentTo(userGroup.Permissions));
        Assert.That(createdUserGroup.AllowedSections, Is.EquivalentTo(userGroup.AllowedSections));
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

        Assert.That(createResult.Success, Is.True);
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

        Assert.That(updateResult.Success, Is.True);
        Assert.That(updatedUserGroup, Is.Not.Null);
        Assert.That(updatedUserGroup.Name, Is.EqualTo(createdUserGroup.Name));
        Assert.That(updatedUserGroup.Alias, Is.EqualTo(createdUserGroup.Alias));
        Assert.That(updatedUserGroup.Description, Is.EqualTo(createdUserGroup.Description));
        Assert.That(updatedUserGroup.Icon, Is.EqualTo(createdUserGroup.Icon));
        Assert.That(updatedUserGroup.HasAccessToAllLanguages, Is.EqualTo(createdUserGroup.HasAccessToAllLanguages));
        Assert.That(updatedUserGroup.Permissions, Is.EquivalentTo(createdUserGroup.Permissions));
        Assert.That(updatedUserGroup.AllowedSections, Is.EquivalentTo(createdUserGroup.AllowedSections));
    }

    [Test]
    public async Task Cannot_Create_User_Group_With_Name_Equals_Null()
    {
        var userGroup = new UserGroup(ShortStringHelper)
        {
            Name = null
        };

        var result = await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(UserGroupOperationStatus.MissingName));
    }

    [Test]
    public async Task Cannot_Create_User_Group_With_Name_Longer_Than_Max_Length()
    {
        var userGroup = new UserGroup(ShortStringHelper)
        {
            Name = "Sed porttitor lectus nibh. Vivamus magna justo, lacinia eget consectetur sed, convallis at tellus. Vivamus suscipit tortor eget felis porttitor volutpat. Quisque velit nisi, pretium ut lacinia in, elementum id enim."
        };

        var result = await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(UserGroupOperationStatus.NameTooLong));
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

        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(UserGroupOperationStatus.AliasTooLong));
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

        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(UserGroupOperationStatus.NotFound));
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

        Assert.That(result.Success, Is.True);

        result = await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(UserGroupOperationStatus.AlreadyExists));
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
        Assert.That(setupResult.Success, Is.True);

        var newUserGroup = new UserGroup(ShortStringHelper)
        {
            Name = "I have a duplicate alias",
            Alias = alias,
        };
        var result = await UserGroupService.CreateAsync(newUserGroup, Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(UserGroupOperationStatus.DuplicateAlias));
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
        Assert.That(setupResult.Success, Is.True);

        IUserGroup userGroupToUpdate = new UserGroup(ShortStringHelper)
        {
            Name = "I don't have a duplicate alias",
            Alias = "somAlias",
        };
        var creationResult = await UserGroupService.CreateAsync(userGroupToUpdate, Constants.Security.SuperUserKey);
        Assert.That(creationResult.Success, Is.True);


        userGroupToUpdate = creationResult.Result;
        userGroupToUpdate.Name = "Now I have a duplicate alias";
        userGroupToUpdate.Alias = alias;

        var updateResult = await UserGroupService.UpdateAsync(userGroupToUpdate, Constants.Security.SuperUserKey);
        Assert.That(updateResult.Success, Is.False);
        Assert.That(updateResult.Status, Is.EqualTo(UserGroupOperationStatus.DuplicateAlias));
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
        Assert.That(setupResult.Success, Is.True);


        var updateName = "New Name";
        userGroup.Name = updateName;
        var updateResult = await UserGroupService.UpdateAsync(userGroup, Constants.Security.SuperUserKey);
        Assert.That(updateResult.Success, Is.True);
        var updatedGroup = updateResult.Result;
        Assert.That(updatedGroup.Name, Is.EqualTo(updateName));
    }

    [TestCase(Constants.Security.AdminGroupKeyString, "admin")]
    [TestCase(Constants.Security.SensitiveDataGroupKeyString, "sensitiveData")]
    [TestCase(Constants.Security.TranslatorGroupString, "translator")]
    public async Task Cannot_Delete_System_User_Group(string userGroupKeyAsString, string expectedGroupAlias)
    {
        // since we can't use the constants as input, let's make sure we don't get false positives by double checking the group alias
        var key = Guid.Parse(userGroupKeyAsString);
        var userGroup = await UserGroupService.GetAsync(key);
        Assert.That(userGroup, Is.Not.Null);
        Assert.That(userGroup.Alias, Is.EqualTo(expectedGroupAlias));

        var result = await UserGroupService.DeleteAsync(key);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Result, Is.EqualTo(UserGroupOperationStatus.CanNotDeleteIsSystemUserGroup));
    }

    [TestCase( Constants.Security.EditorGroupKeyString, "editor")]
    [TestCase(Constants.Security.WriterGroupKeyString, "writer")]
    public async Task Can_Delete_Non_System_User_Group(string userGroupKeyAsString, string expectedGroupAlias)
    {
        // since we can't use the constants as input, let's make sure we don't get false positives by double checking the group alias
        var key = Guid.Parse(userGroupKeyAsString);
        var userGroup = await UserGroupService.GetAsync(key);
        Assert.That(userGroup, Is.Not.Null);
        Assert.That(userGroup.Alias, Is.EqualTo(expectedGroupAlias));

        var result = await UserGroupService.DeleteAsync(key);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Result, Is.EqualTo(UserGroupOperationStatus.Success));
    }
}
