using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.AuthorizationStatus;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class UserGroupPermissionServiceTests : UmbracoIntegrationTest
{
    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private IUserGroupPermissionService UserGroupPermissionService => GetRequiredService<IUserGroupPermissionService>();

    private IUserService UserService => GetRequiredService<IUserService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IMediaService MediaService => GetRequiredService<IMediaService>();

    private IElementContainerService ElementContainerService => GetRequiredService<IElementContainerService>();

    private IContent _allowedContentChild;

    private IContent _deniedContent;

    private IMedia _allowedMediaChild;

    private IMedia _deniedMedia;

    private EntityContainer _allowedElementFolderChild;

    private EntityContainer _deniedElementFolder;

    private IUserGroup _restrictedUserGroup;

    private IUser _restrictedUser;

    private IUserGroup _rootAccessUserGroup;

    private IUser _rootAccessUser;

    [SetUp]
    public async Task Setup()
    {
        var contentType = ContentTypeBuilder.CreateSimpleContentType();
        contentType.AllowedTemplates = null;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var allowedContent = ContentBuilder.CreateSimpleContent(contentType, "allowed");
        ContentService.Save(allowedContent);
        _allowedContentChild = ContentBuilder.CreateSimpleContent(contentType, "allowed child", allowedContent.Id);
        ContentService.Save(_allowedContentChild);
        _deniedContent = ContentBuilder.CreateSimpleContent(contentType, "denied");
        ContentService.Save(_deniedContent);

        IMedia allowedMedia = CreateMediaFolder("allowed", Constants.System.Root);
        _allowedMediaChild = CreateMediaFolder("allowed child", allowedMedia.Id);
        _deniedMedia = CreateMediaFolder("denied", Constants.System.Root);

        EntityContainer allowedElementFolder = await CreateElementFolderAsync("allowed", null);
        _allowedElementFolderChild = await CreateElementFolderAsync("allowed child", allowedElementFolder.Key);
        _deniedElementFolder = await CreateElementFolderAsync("denied", null);

        (_restrictedUserGroup, _restrictedUser) = await CreateUserWithStartNodesAsync("restricted", allowedContent.Id, allowedMedia.Id, allowedElementFolder.Id);
        (_rootAccessUserGroup, _rootAccessUser) = await CreateUserWithStartNodesAsync("rootAccess", Constants.System.Root, Constants.System.Root, Constants.System.Root);
    }

    [Test]
    public async Task AuthorizeCreateAsync_Denies_Root_Content_Start_Node_Without_Root_Access()
    {
        UserGroupAuthorizationStatus status = await UserGroupPermissionService.AuthorizeCreateAsync(
            _restrictedUser,
            BuildUserGroup(startContentId: Constants.System.Root));

        Assert.AreEqual(UserGroupAuthorizationStatus.UnauthorizedMissingContentStartNodeAccess, status);
    }

    [Test]
    public async Task AuthorizeCreateAsync_Denies_Root_Media_Start_Node_Without_Root_Access()
    {
        UserGroupAuthorizationStatus status = await UserGroupPermissionService.AuthorizeCreateAsync(
            _restrictedUser,
            BuildUserGroup(startMediaId: Constants.System.Root));

        Assert.AreEqual(UserGroupAuthorizationStatus.UnauthorizedMissingMediaStartNodeAccess, status);
    }

    [Test]
    public async Task AuthorizeCreateAsync_Denies_Content_Start_Node_Outside_Own_Start_Nodes()
    {
        UserGroupAuthorizationStatus status = await UserGroupPermissionService.AuthorizeCreateAsync(
            _restrictedUser,
            BuildUserGroup(startContentId: _deniedContent.Id));

        Assert.AreEqual(UserGroupAuthorizationStatus.UnauthorizedMissingContentStartNodeAccess, status);
    }

    [Test]
    public async Task AuthorizeCreateAsync_Denies_Media_Start_Node_Outside_Own_Start_Nodes()
    {
        UserGroupAuthorizationStatus status = await UserGroupPermissionService.AuthorizeCreateAsync(
            _restrictedUser,
            BuildUserGroup(startMediaId: _deniedMedia.Id));

        Assert.AreEqual(UserGroupAuthorizationStatus.UnauthorizedMissingMediaStartNodeAccess, status);
    }

    [Test]
    public async Task AuthorizeCreateAsync_Allows_Start_Nodes_Within_Own_Start_Nodes()
    {
        UserGroupAuthorizationStatus status = await UserGroupPermissionService.AuthorizeCreateAsync(
            _restrictedUser,
            BuildUserGroup(startContentId: _allowedContentChild.Id, startMediaId: _allowedMediaChild.Id));

        Assert.AreEqual(UserGroupAuthorizationStatus.Success, status);
    }

    [Test]
    public async Task AuthorizeCreateAsync_Allows_Root_Start_Nodes_With_Root_Access()
    {
        UserGroupAuthorizationStatus status = await UserGroupPermissionService.AuthorizeCreateAsync(
            _rootAccessUser,
            BuildUserGroup(startContentId: Constants.System.Root, startMediaId: Constants.System.Root));

        Assert.AreEqual(UserGroupAuthorizationStatus.Success, status);
    }

    [Test]
    public async Task AuthorizeUpdateAsync_Denies_Root_Content_Start_Node_Without_Root_Access()
    {
        IUserGroup userGroup = (await UserGroupService.GetAsync(_restrictedUserGroup.Key))!;
        userGroup.StartContentId = Constants.System.Root;

        UserGroupAuthorizationStatus status = await UserGroupPermissionService.AuthorizeUpdateAsync(_restrictedUser, userGroup);

        Assert.AreEqual(UserGroupAuthorizationStatus.UnauthorizedMissingContentStartNodeAccess, status);
    }

    [Test]
    public async Task AuthorizeUpdateAsync_Denies_Root_Media_Start_Node_Without_Root_Access()
    {
        IUserGroup userGroup = (await UserGroupService.GetAsync(_restrictedUserGroup.Key))!;
        userGroup.StartMediaId = Constants.System.Root;

        UserGroupAuthorizationStatus status = await UserGroupPermissionService.AuthorizeUpdateAsync(_restrictedUser, userGroup);

        Assert.AreEqual(UserGroupAuthorizationStatus.UnauthorizedMissingMediaStartNodeAccess, status);
    }

    [Test]
    public async Task AuthorizeUpdateAsync_Allows_Start_Nodes_Within_Own_Start_Nodes()
    {
        IUserGroup userGroup = (await UserGroupService.GetAsync(_restrictedUserGroup.Key))!;
        userGroup.StartContentId = _allowedContentChild.Id;
        userGroup.StartMediaId = _allowedMediaChild.Id;

        UserGroupAuthorizationStatus status = await UserGroupPermissionService.AuthorizeUpdateAsync(_restrictedUser, userGroup);

        Assert.AreEqual(UserGroupAuthorizationStatus.Success, status);
    }

    [Test]
    public async Task AuthorizeUpdateAsync_Allows_Root_Start_Nodes_With_Root_Access()
    {
        IUserGroup userGroup = (await UserGroupService.GetAsync(_rootAccessUserGroup.Key))!;

        UserGroupAuthorizationStatus status = await UserGroupPermissionService.AuthorizeUpdateAsync(_rootAccessUser, userGroup);

        Assert.AreEqual(UserGroupAuthorizationStatus.Success, status);
    }

    [Test]
    public async Task AuthorizeUpdateAsync_Allows_Unchanged_Root_Start_Nodes_Without_Root_Access()
    {
        IUser narrowedMember = await CreateNarrowedRootAccessGroupMemberAsync();
        IUserGroup userGroup = (await UserGroupService.GetAsync(_rootAccessUserGroup.Key))!;
        userGroup.Description = "Updated description";

        UserGroupAuthorizationStatus status = await UserGroupPermissionService.AuthorizeUpdateAsync(narrowedMember, userGroup);

        Assert.AreEqual(UserGroupAuthorizationStatus.Success, status);
    }

    [Test]
    public async Task AuthorizeUpdateAsync_Denies_Changed_Content_Start_Node_Outside_Own_Start_Nodes()
    {
        IUser narrowedMember = await CreateNarrowedRootAccessGroupMemberAsync();
        IUserGroup userGroup = (await UserGroupService.GetAsync(_rootAccessUserGroup.Key))!;
        userGroup.StartContentId = _deniedContent.Id;

        UserGroupAuthorizationStatus status = await UserGroupPermissionService.AuthorizeUpdateAsync(narrowedMember, userGroup);

        Assert.AreEqual(UserGroupAuthorizationStatus.UnauthorizedMissingContentStartNodeAccess, status);
    }

    [Test]
    public async Task AuthorizeUpdateAsync_Denies_Changed_Media_Start_Node_Outside_Own_Start_Nodes()
    {
        IUser narrowedMember = await CreateNarrowedRootAccessGroupMemberAsync();
        IUserGroup userGroup = (await UserGroupService.GetAsync(_rootAccessUserGroup.Key))!;
        userGroup.StartMediaId = _deniedMedia.Id;

        UserGroupAuthorizationStatus status = await UserGroupPermissionService.AuthorizeUpdateAsync(narrowedMember, userGroup);

        Assert.AreEqual(UserGroupAuthorizationStatus.UnauthorizedMissingMediaStartNodeAccess, status);
    }

    [Test]
    public async Task AuthorizeCreateAsync_Denies_Root_Element_Start_Node_Without_Root_Access()
    {
        UserGroupAuthorizationStatus status = await UserGroupPermissionService.AuthorizeCreateAsync(
            _restrictedUser,
            BuildUserGroup(startElementId: Constants.System.Root));

        Assert.AreEqual(UserGroupAuthorizationStatus.UnauthorizedMissingElementStartNodeAccess, status);
    }

    [Test]
    public async Task AuthorizeCreateAsync_Denies_Element_Start_Node_Outside_Own_Start_Nodes()
    {
        UserGroupAuthorizationStatus status = await UserGroupPermissionService.AuthorizeCreateAsync(
            _restrictedUser,
            BuildUserGroup(startElementId: _deniedElementFolder.Id));

        Assert.AreEqual(UserGroupAuthorizationStatus.UnauthorizedMissingElementStartNodeAccess, status);
    }

    [Test]
    public async Task AuthorizeCreateAsync_Allows_Element_Start_Node_Within_Own_Start_Nodes()
    {
        UserGroupAuthorizationStatus status = await UserGroupPermissionService.AuthorizeCreateAsync(
            _restrictedUser,
            BuildUserGroup(startElementId: _allowedElementFolderChild.Id));

        Assert.AreEqual(UserGroupAuthorizationStatus.Success, status);
    }

    [Test]
    public async Task AuthorizeCreateAsync_Allows_Root_Element_Start_Node_With_Root_Access()
    {
        UserGroupAuthorizationStatus status = await UserGroupPermissionService.AuthorizeCreateAsync(
            _rootAccessUser,
            BuildUserGroup(startElementId: Constants.System.Root));

        Assert.AreEqual(UserGroupAuthorizationStatus.Success, status);
    }

    [Test]
    public async Task AuthorizeUpdateAsync_Denies_Root_Element_Start_Node_Without_Root_Access()
    {
        IUserGroup userGroup = (await UserGroupService.GetAsync(_restrictedUserGroup.Key))!;
        userGroup.StartElementId = Constants.System.Root;

        UserGroupAuthorizationStatus status = await UserGroupPermissionService.AuthorizeUpdateAsync(_restrictedUser, userGroup);

        Assert.AreEqual(UserGroupAuthorizationStatus.UnauthorizedMissingElementStartNodeAccess, status);
    }

    [Test]
    public async Task AuthorizeUpdateAsync_Denies_Changed_Element_Start_Node_Outside_Own_Start_Nodes()
    {
        IUser narrowedMember = await CreateNarrowedRootAccessGroupMemberAsync();
        IUserGroup userGroup = (await UserGroupService.GetAsync(_rootAccessUserGroup.Key))!;
        userGroup.StartElementId = _deniedElementFolder.Id;

        UserGroupAuthorizationStatus status = await UserGroupPermissionService.AuthorizeUpdateAsync(narrowedMember, userGroup);

        Assert.AreEqual(UserGroupAuthorizationStatus.UnauthorizedMissingElementStartNodeAccess, status);
    }

    private async Task<IUser> CreateNarrowedRootAccessGroupMemberAsync()
    {
        IUser user = UserService.CreateUserWithIdentity("narrowed", "narrowed@umbraco.com");
        user.AddGroup(_rootAccessUserGroup.ToReadOnlyGroup());
        user.StartContentIds = [_allowedContentChild.Id];
        user.StartMediaIds = [_allowedMediaChild.Id];
        user.StartElementIds = [_allowedElementFolderChild.Id];
        UserService.Save(user);

        return (await UserService.GetAsync(user.Key))!;
    }

    private IMedia CreateMediaFolder(string name, int parentId)
    {
        IMedia media = MediaService.CreateMedia(name, parentId, Constants.Conventions.MediaTypes.Folder);
        MediaService.Save(media);
        return media;
    }

    private async Task<EntityContainer> CreateElementFolderAsync(string name, Guid? parentKey)
    {
        Attempt<EntityContainer?, EntityContainerOperationStatus> result =
            await ElementContainerService.CreateAsync(null, name, parentKey, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success, $"Could not create the element folder: {result.Status}");
        return result.Result!;
    }

    private async Task<(IUserGroup UserGroup, IUser User)> CreateUserWithStartNodesAsync(string alias, int startContentId, int startMediaId, int startElementId)
    {
        IUserGroup userGroup = new UserGroupBuilder()
            .WithAlias(alias)
            .WithName(alias)
            .WithAllowedSections([Constants.Applications.Content, Constants.Applications.Media, Constants.Applications.Users])
            .WithStartContentId(startContentId)
            .WithStartMediaId(startMediaId)
            .WithStartElementId(startElementId)
            .Build();

        Attempt<IUserGroup, UserGroupOperationStatus> userGroupAttempt =
            await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);
        Assert.IsTrue(userGroupAttempt.Success, $"Could not create the user group: {userGroupAttempt.Status}");

        IUser user = UserService.CreateUserWithIdentity(alias, $"{alias}@umbraco.com");
        user.AddGroup(userGroupAttempt.Result.ToReadOnlyGroup());
        UserService.Save(user);

        return (userGroupAttempt.Result, (await UserService.GetAsync(user.Key))!);
    }

    private static IUserGroup BuildUserGroup(int? startContentId = null, int? startMediaId = null, int? startElementId = null)
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

        return userGroup;
    }
}
