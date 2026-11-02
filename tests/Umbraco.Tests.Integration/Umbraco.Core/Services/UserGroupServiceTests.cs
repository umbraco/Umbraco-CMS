using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class UserGroupServiceTests : UmbracoIntegrationTest
{
    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private IUserService UserService => GetRequiredService<IUserService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IMediaService MediaService => GetRequiredService<IMediaService>();

    private IElementContainerService ElementContainerService => GetRequiredService<IElementContainerService>();

    private IElementEditingService ElementEditingService => GetRequiredService<IElementEditingService>();

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

    [Test]
    public async Task Cannot_Create_User_Group_With_Root_Content_Start_Node_Without_Root_Access()
    {
        StartNodeScenario scenario = await CreateStartNodeScenarioAsync();

        var result = await UserGroupService.CreateAsync(
            BuildUserGroup(startContentId: Constants.System.Root),
            scenario.RestrictedUser.Key);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(UserGroupOperationStatus.Unauthorized, result.Status);
    }

    [Test]
    public async Task Cannot_Create_User_Group_With_Root_Media_Start_Node_Without_Root_Access()
    {
        StartNodeScenario scenario = await CreateStartNodeScenarioAsync();

        var result = await UserGroupService.CreateAsync(
            BuildUserGroup(startMediaId: Constants.System.Root),
            scenario.RestrictedUser.Key);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(UserGroupOperationStatus.Unauthorized, result.Status);
    }

    [TestCase(int.MaxValue)]
    [TestCase(Constants.System.RecycleBinElement)]
    public async Task Cannot_Create_User_Group_With_Element_Start_Node_That_Is_Not_An_Element_Folder(int startElementId)
    {
        var result = await UserGroupService.CreateAsync(
            BuildUserGroup(startElementId: startElementId),
            Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(UserGroupOperationStatus.ElementStartNodeKeyNotFound, result.Status);
    }

    [TestCase(int.MaxValue)]
    [TestCase(Constants.System.RecycleBinElement)]
    public async Task Cannot_Update_User_Group_With_Element_Start_Node_That_Is_Not_An_Element_Folder(int startElementId)
    {
        var createResult = await UserGroupService.CreateAsync(BuildUserGroup(), Constants.Security.SuperUserKey);
        Assert.IsTrue(createResult.Success, $"Could not create the user group: {createResult.Status}");
        var userGroup = await UserGroupService.GetAsync(createResult.Result.Key);
        userGroup!.StartElementId = startElementId;

        var result = await UserGroupService.UpdateAsync(userGroup, Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(UserGroupOperationStatus.ElementStartNodeKeyNotFound, result.Status);
        Assert.IsNull((await UserGroupService.GetAsync(userGroup.Key))!.StartElementId);
    }

    [TestCase(int.MaxValue)]
    [TestCase(Constants.System.RecycleBinContent)]
    public async Task Cannot_Create_User_Group_With_Document_Blueprint_Start_Node_That_Is_Not_A_Document_Blueprint_Folder(int startDocumentBlueprintId)
    {
        var result = await UserGroupService.CreateAsync(
            BuildUserGroup(startDocumentBlueprintId: startDocumentBlueprintId),
            Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(UserGroupOperationStatus.DocumentBlueprintStartNodeKeyNotFound, result.Status);
    }

    [TestCase(int.MaxValue)]
    [TestCase(Constants.System.RecycleBinContent)]
    public async Task Cannot_Update_User_Group_With_Document_Blueprint_Start_Node_That_Is_Not_A_Document_Blueprint_Folder(int startDocumentBlueprintId)
    {
        var createResult = await UserGroupService.CreateAsync(BuildUserGroup(), Constants.Security.SuperUserKey);
        Assert.IsTrue(createResult.Success, $"Could not create the user group: {createResult.Status}");
        var userGroup = await UserGroupService.GetAsync(createResult.Result.Key);
        userGroup!.StartDocumentBlueprintId = startDocumentBlueprintId;

        var result = await UserGroupService.UpdateAsync(userGroup, Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(UserGroupOperationStatus.DocumentBlueprintStartNodeKeyNotFound, result.Status);
        Assert.IsNull((await UserGroupService.GetAsync(userGroup.Key))!.StartDocumentBlueprintId);
    }

    [Test]
    public async Task Cannot_Create_User_Group_With_Element_As_Element_Start_Node()
    {
        var elementId = await CreateElementAsync();

        var result = await UserGroupService.CreateAsync(
            BuildUserGroup(startElementId: elementId),
            Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(UserGroupOperationStatus.ElementStartNodeKeyNotFound, result.Status);
    }

    [Test]
    public async Task Cannot_Update_User_Group_With_Element_As_Element_Start_Node()
    {
        var elementId = await CreateElementAsync();
        var createResult = await UserGroupService.CreateAsync(BuildUserGroup(), Constants.Security.SuperUserKey);
        Assert.IsTrue(createResult.Success, $"Could not create the user group: {createResult.Status}");
        var userGroup = await UserGroupService.GetAsync(createResult.Result.Key);
        userGroup!.StartElementId = elementId;

        var result = await UserGroupService.UpdateAsync(userGroup, Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(UserGroupOperationStatus.ElementStartNodeKeyNotFound, result.Status);
        Assert.IsNull((await UserGroupService.GetAsync(userGroup.Key))!.StartElementId);
    }

    [Test]
    public async Task Cannot_Create_User_Group_With_Root_Element_Start_Node_Without_Root_Access()
    {
        StartNodeScenario scenario = await CreateStartNodeScenarioAsync();

        var result = await UserGroupService.CreateAsync(
            BuildUserGroup(startElementId: Constants.System.Root),
            scenario.RestrictedUser.Key);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(UserGroupOperationStatus.Unauthorized, result.Status);
    }

    [Test]
    public async Task Can_Create_User_Group_With_Start_Nodes_Within_Own_Start_Nodes()
    {
        StartNodeScenario scenario = await CreateStartNodeScenarioAsync();

        var result = await UserGroupService.CreateAsync(
            BuildUserGroup(startContentId: scenario.AllowedContentChild.Id, startMediaId: scenario.AllowedMediaChild.Id, startElementId: scenario.AllowedElementFolderChild.Id),
            scenario.RestrictedUser.Key);

        Assert.IsTrue(result.Success, $"Could not create the user group: {result.Status}");
    }

    [Test]
    public async Task Cannot_Update_Own_User_Group_To_Root_Content_Start_Node_Without_Root_Access()
    {
        StartNodeScenario scenario = await CreateStartNodeScenarioAsync();
        var userGroup = await UserGroupService.GetAsync(scenario.RestrictedUserGroup.Key);
        var originalStartContentId = userGroup!.StartContentId;
        userGroup.StartContentId = Constants.System.Root;

        var result = await UserGroupService.UpdateAsync(userGroup, scenario.RestrictedUser.Key);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(UserGroupOperationStatus.Unauthorized, result.Status);
        Assert.AreEqual(originalStartContentId, (await UserGroupService.GetAsync(userGroup.Key))!.StartContentId);
    }

    [Test]
    public async Task Cannot_Update_Own_User_Group_To_Root_Media_Start_Node_Without_Root_Access()
    {
        StartNodeScenario scenario = await CreateStartNodeScenarioAsync();
        var userGroup = await UserGroupService.GetAsync(scenario.RestrictedUserGroup.Key);
        var originalStartMediaId = userGroup!.StartMediaId;
        userGroup.StartMediaId = Constants.System.Root;

        var result = await UserGroupService.UpdateAsync(userGroup, scenario.RestrictedUser.Key);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(UserGroupOperationStatus.Unauthorized, result.Status);
        Assert.AreEqual(originalStartMediaId, (await UserGroupService.GetAsync(userGroup.Key))!.StartMediaId);
    }

    [Test]
    public async Task Cannot_Update_Own_User_Group_To_Root_Element_Start_Node_Without_Root_Access()
    {
        StartNodeScenario scenario = await CreateStartNodeScenarioAsync();
        var userGroup = await UserGroupService.GetAsync(scenario.RestrictedUserGroup.Key);
        var originalStartElementId = userGroup!.StartElementId;
        userGroup.StartElementId = Constants.System.Root;

        var result = await UserGroupService.UpdateAsync(userGroup, scenario.RestrictedUser.Key);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(UserGroupOperationStatus.Unauthorized, result.Status);
        Assert.AreEqual(originalStartElementId, (await UserGroupService.GetAsync(userGroup.Key))!.StartElementId);
    }

    [Test]
    public async Task Can_Update_User_Group_With_Unchanged_Root_Start_Nodes_Without_Root_Access()
    {
        StartNodeScenario scenario = await CreateStartNodeScenarioAsync();
        var rootAccessUserGroup = new UserGroupBuilder()
            .WithAlias("rootAccess")
            .WithName("Root Access")
            .WithAllowedSections([Constants.Applications.Content, Constants.Applications.Media, Constants.Applications.Users])
            .WithStartContentId(Constants.System.Root)
            .WithStartMediaId(Constants.System.Root)
            .WithStartElementId(Constants.System.Root)
            .Build();
        var createResult = await UserGroupService.CreateAsync(rootAccessUserGroup, Constants.Security.SuperUserKey);
        Assert.IsTrue(createResult.Success, $"Could not create the user group: {createResult.Status}");

        var narrowedMember = UserService.CreateUserWithIdentity("narrowed", "narrowed@umbraco.com");
        narrowedMember.AddGroup(createResult.Result.ToReadOnlyGroup());
        narrowedMember.StartContentIds = [scenario.AllowedContentChild.Id];
        narrowedMember.StartMediaIds = [scenario.AllowedMediaChild.Id];
        narrowedMember.StartElementIds = [scenario.AllowedElementFolderChild.Id];
        UserService.Save(narrowedMember);

        var userGroup = await UserGroupService.GetAsync(createResult.Result.Key);
        userGroup!.Description = "Updated description";

        var result = await UserGroupService.UpdateAsync(userGroup, narrowedMember.Key);

        Assert.IsTrue(result.Success, $"Could not update the user group: {result.Status}");
        Assert.AreEqual("Updated description", (await UserGroupService.GetAsync(userGroup.Key))!.Description);
    }

    private async Task<StartNodeScenario> CreateStartNodeScenarioAsync()
    {
        var contentType = ContentTypeBuilder.CreateSimpleContentType();
        contentType.AllowedTemplates = null;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var allowedContent = ContentBuilder.CreateSimpleContent(contentType, "allowed");
        ContentService.Save(allowedContent);
        var allowedContentChild = ContentBuilder.CreateSimpleContent(contentType, "allowed child", allowedContent.Id);
        ContentService.Save(allowedContentChild);

        var allowedMedia = MediaService.CreateMedia("allowed", Constants.System.Root, Constants.Conventions.MediaTypes.Folder);
        MediaService.Save(allowedMedia);
        var allowedMediaChild = MediaService.CreateMedia("allowed child", allowedMedia.Id, Constants.Conventions.MediaTypes.Folder);
        MediaService.Save(allowedMediaChild);

        var allowedElementFolder = await CreateElementFolderAsync("allowed", null);
        var allowedElementFolderChild = await CreateElementFolderAsync("allowed child", allowedElementFolder.Key);

        var userGroup = new UserGroupBuilder()
            .WithAlias("restricted")
            .WithName("Restricted")
            .WithAllowedSections([Constants.Applications.Content, Constants.Applications.Media, Constants.Applications.Users])
            .WithStartContentId(allowedContent.Id)
            .WithStartMediaId(allowedMedia.Id)
            .WithStartElementId(allowedElementFolder.Id)
            .Build();
        var userGroupResult = await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);
        Assert.IsTrue(userGroupResult.Success, $"Could not create the user group: {userGroupResult.Status}");

        var user = UserService.CreateUserWithIdentity("restricted", "restricted@umbraco.com");
        user.AddGroup(userGroupResult.Result.ToReadOnlyGroup());
        UserService.Save(user);

        return new StartNodeScenario(userGroupResult.Result, user, allowedContentChild, allowedMediaChild, allowedElementFolderChild);
    }

    private async Task<int> CreateElementAsync()
    {
        var elementType = new ContentTypeBuilder()
            .WithAlias("testElement")
            .WithName("Test Element")
            .WithIsElement(true)
            .WithAllowedInLibrary(true)
            .Build();
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        var result = await ElementEditingService.CreateAsync(
            new ElementCreateModel
            {
                ContentTypeKey = elementType.Key,
                ParentKey = null,
                Variants = [new VariantModel { Name = "Element" }],
            },
            Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success, $"Could not create the element: {result.Status}");
        return result.Result!.Content!.Id;
    }

    private async Task<EntityContainer> CreateElementFolderAsync(string name, Guid? parentKey)
    {
        var result = await ElementContainerService.CreateAsync(null, name, parentKey, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success, $"Could not create the element folder: {result.Status}");
        return result.Result!;
    }

    private IUserGroup BuildUserGroup(
        int? startContentId = null,
        int? startMediaId = null,
        int? startElementId = null,
        int? startDocumentBlueprintId = null)
    {
        var alias = $"group{Guid.NewGuid():N}";
        IUserGroup userGroup = new UserGroupBuilder()
            .WithAlias(alias)
            .WithName(alias)
            .WithAllowedSections([Constants.Applications.Content])
            .Build();

        userGroup.StartContentId = startContentId;
        userGroup.StartMediaId = startMediaId;
        userGroup.StartElementId = startElementId;
        userGroup.StartDocumentBlueprintId = startDocumentBlueprintId;

        return userGroup;
    }

    private sealed record StartNodeScenario(
        IUserGroup RestrictedUserGroup,
        IUser RestrictedUser,
        IContent AllowedContentChild,
        IMedia AllowedMediaChild,
        EntityContainer AllowedElementFolderChild);
}
