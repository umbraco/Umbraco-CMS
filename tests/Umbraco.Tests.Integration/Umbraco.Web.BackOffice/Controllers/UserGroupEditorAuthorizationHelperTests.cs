// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Web.BackOffice.Controllers;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Web.BackOffice.Controllers;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class UserGroupEditorAuthorizationHelperTests : UmbracoIntegrationTestWithContent
{
    private IUserService UserService => GetRequiredService<IUserService>();

    private IMediaService MediaService => GetRequiredService<IMediaService>();

    private IContent _deniedContent;

    private IMedia _allowedMedia;

    private IMedia _allowedMediaChild;

    private IMedia _deniedMedia;

    private IUser _restrictedUser;

    private IUser _rootAccessUser;

    public override void CreateTestData()
    {
        base.CreateTestData();

        _deniedContent = ContentBuilder.CreateSimpleContent(ContentType, "denied");
        ContentService.Save(_deniedContent);

        _allowedMedia = CreateMediaFolder("allowed", Constants.System.Root);
        _allowedMediaChild = CreateMediaFolder("allowed child", _allowedMedia.Id);
        _deniedMedia = CreateMediaFolder("denied", Constants.System.Root);

        _restrictedUser = CreateUserWithStartNodes("restricted", Textpage.Id, _allowedMedia.Id);
        _rootAccessUser = CreateUserWithStartNodes("rootAccess", Constants.System.Root, Constants.System.Root);
    }

    [Test]
    public void Cannot_Change_Content_Start_Node_To_Root_Without_Root_Access()
    {
        Attempt<string?> result = CreateHelper().AuthorizeStartNodeChanges(
            _restrictedUser,
            Textpage.Id,
            Constants.System.Root,
            _allowedMedia.Id,
            _allowedMedia.Id);

        Assert.IsFalse(result.Success);
    }

    [Test]
    public void Cannot_Change_Media_Start_Node_To_Root_Without_Root_Access()
    {
        Attempt<string?> result = CreateHelper().AuthorizeStartNodeChanges(
            _restrictedUser,
            Textpage.Id,
            Textpage.Id,
            _allowedMedia.Id,
            Constants.System.Root);

        Assert.IsFalse(result.Success);
    }

    [Test]
    public void Cannot_Set_Root_Start_Nodes_On_New_User_Group_Without_Root_Access()
    {
        Attempt<string?> result = CreateHelper().AuthorizeStartNodeChanges(
            _restrictedUser,
            null,
            Constants.System.Root,
            null,
            Constants.System.Root);

        Assert.IsFalse(result.Success);
    }

    [Test]
    public void Cannot_Change_Start_Nodes_Outside_Own_Start_Nodes()
    {
        Attempt<string?> contentResult = CreateHelper().AuthorizeStartNodeChanges(
            _restrictedUser,
            Textpage.Id,
            _deniedContent.Id,
            _allowedMedia.Id,
            _allowedMedia.Id);
        Attempt<string?> mediaResult = CreateHelper().AuthorizeStartNodeChanges(
            _restrictedUser,
            Textpage.Id,
            Textpage.Id,
            _allowedMedia.Id,
            _deniedMedia.Id);

        Assert.IsFalse(contentResult.Success);
        Assert.IsFalse(mediaResult.Success);
    }

    [Test]
    public void Can_Change_Start_Nodes_Within_Own_Start_Nodes()
    {
        Attempt<string?> result = CreateHelper().AuthorizeStartNodeChanges(
            _restrictedUser,
            Textpage.Id,
            Subpage.Id,
            _allowedMedia.Id,
            _allowedMediaChild.Id);

        Assert.IsTrue(result.Success, result.Result);
    }

    [Test]
    public void Can_Keep_Unchanged_Root_Start_Nodes_Without_Root_Access()
    {
        Attempt<string?> result = CreateHelper().AuthorizeStartNodeChanges(
            _restrictedUser,
            Constants.System.Root,
            Constants.System.Root,
            Constants.System.Root,
            Constants.System.Root);

        Assert.IsTrue(result.Success, result.Result);
    }

    [Test]
    public void Can_Change_Start_Nodes_To_Root_With_Root_Access()
    {
        Attempt<string?> result = CreateHelper().AuthorizeStartNodeChanges(
            _rootAccessUser,
            Textpage.Id,
            Constants.System.Root,
            _allowedMedia.Id,
            Constants.System.Root);

        Assert.IsTrue(result.Success, result.Result);
    }

    private UserGroupEditorAuthorizationHelper CreateHelper() =>
        new(
            UserService,
            ContentService,
            MediaService,
            GetRequiredService<IEntityService>(),
            GetRequiredService<AppCaches>());

    private IMedia CreateMediaFolder(string name, int parentId)
    {
        IMedia media = MediaService.CreateMedia(name, parentId, Constants.Conventions.MediaTypes.Folder);
        MediaService.Save(media);
        return media;
    }

    private IUser CreateUserWithStartNodes(string alias, int startContentId, int startMediaId)
    {
        IUserGroup userGroup = new UserGroupBuilder()
            .WithAlias(alias)
            .WithName(alias)
            .WithAllowedSections(new[] { Constants.Applications.Content, Constants.Applications.Media, Constants.Applications.Users })
            .WithStartContentId(startContentId)
            .WithStartMediaId(startMediaId)
            .Build();
        UserService.Save(userGroup);

        IUser user = UserService.CreateUserWithIdentity(alias, $"{alias}@umbraco.com");
        user.AddGroup(userGroup.ToReadOnlyGroup());
        UserService.Save(user);

        return UserService.GetUserById(user.Id)!;
    }
}
