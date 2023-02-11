// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Editors;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Editors;

[TestFixture]
public class UserEditorAuthorizationHelperTests
{
    [Test]
    public void Admin_Is_Authorized()
    {
        var currentUser = CreateAdminUser();
        var savingUser = CreateUser();

        var contentService = new Mock<IContentService>();
        var mediaService = new Mock<IMediaService>();
        var entityService = new Mock<IEntityService>();

        var authHelper = new UserEditorAuthorizationHelper(
            contentService.Object,
            mediaService.Object,
            entityService.Object,
            AppCaches.Disabled);

        var result = authHelper.IsAuthorized(currentUser, savingUser, new int[0], new int[0], new string[0]);

        Assert.IsTrue(result.Success);
    }

    [Test]
    public void Non_Admin_Cannot_Save_Admin()
    {
        var currentUser = CreateUser();
        var savingUser = CreateAdminUser();

        var contentService = new Mock<IContentService>();
        var mediaService = new Mock<IMediaService>();
        var entityService = new Mock<IEntityService>();

        var authHelper = new UserEditorAuthorizationHelper(
            contentService.Object,
            mediaService.Object,
            entityService.Object,
            AppCaches.Disabled);

        var result = authHelper.IsAuthorized(currentUser, savingUser, new int[0], new int[0], new string[0]);

        Assert.IsFalse(result.Success);
    }

    [Test]
    public void Cannot_Grant_Group_Membership_Without_Being_A_Member()
    {
        var currentUser = CreateUser(true);
        var savingUser = CreateUser();

        var contentService = new Mock<IContentService>();
        var mediaService = new Mock<IMediaService>();
        var entityService = new Mock<IEntityService>();

        var authHelper = new UserEditorAuthorizationHelper(
            contentService.Object,
            mediaService.Object,
            entityService.Object,
            AppCaches.Disabled);

        var result = authHelper.IsAuthorized(currentUser, savingUser, new int[0], new int[0], new[] { "FunGroup" });

        Assert.IsFalse(result.Success);
    }

    [Test]
    public void Can_Grant_Group_Membership_With_Being_A_Member()
    {
        var currentUser = CreateUser(true);
        var savingUser = CreateUser();

        var contentService = new Mock<IContentService>();
        var mediaService = new Mock<IMediaService>();
        var entityService = new Mock<IEntityService>();

        var authHelper = new UserEditorAuthorizationHelper(
            contentService.Object,
            mediaService.Object,
            entityService.Object,
            AppCaches.Disabled);

        var result = authHelper.IsAuthorized(currentUser, savingUser, new int[0], new int[0], new[] { "test" });

        Assert.IsTrue(result.Success);
    }

    [Test]
    [TestCase(Constants.Security.AdminGroupAlias, Constants.Security.AdminGroupAlias, ExpectedResult = true)]
    [TestCase(Constants.Security.AdminGroupAlias, "SomethingElse", ExpectedResult = true)]
    [TestCase(Constants.Security.EditorGroupAlias, Constants.Security.AdminGroupAlias, ExpectedResult = false)]
    [TestCase(Constants.Security.EditorGroupAlias, "SomethingElse", ExpectedResult = false)]
    [TestCase(Constants.Security.EditorGroupAlias, Constants.Security.EditorGroupAlias, ExpectedResult = true)]
    public bool Can_only_add_user_groups_you_are_part_of_yourself_unless_you_are_admin(
        string groupAlias,
        string groupToAdd)
    {
        var currentUser = Mock.Of<IUser>(user => user.Groups == new[]
        {
            new ReadOnlyUserGroup(1, "CurrentUser", "icon-user", null, null, groupAlias, new int[0], new string[0], new string[0], true),
        });
        IUser savingUser = null; // This means it is a new created user

        var contentService = new Mock<IContentService>();
        var mediaService = new Mock<IMediaService>();
        var entityService = new Mock<IEntityService>();

        var authHelper = new UserEditorAuthorizationHelper(
            contentService.Object,
            mediaService.Object,
            entityService.Object,
            AppCaches.Disabled);

        var result = authHelper.IsAuthorized(currentUser, savingUser, new int[0], new int[0], new[] { groupToAdd });

        return result.Success;
    }

    [Test]
    public void Can_Add_Another_Content_Start_Node_On_User_With_Access()
    {
        var nodePaths = new Dictionary<int, string>
        {
            { 1234, "-1,1234" }, { 9876, "-1,9876" }, { 5555, "-1,9876,5555" }, { 4567, "-1,4567" },
        };

        var currentUser = CreateUser(startContentIds: new[] { 9876 });
        var savingUser = CreateUser(startContentIds: new[] { 1234 });

        var contentService = new Mock<IContentService>();
        contentService.Setup(x => x.GetById(It.IsAny<int>()))
            .Returns((int id) => Mock.Of<IContent>(content => content.Path == nodePaths[id]));
        var mediaService = new Mock<IMediaService>();
        var entityService = new Mock<IEntityService>();
        entityService.Setup(service => service.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
            .Returns((UmbracoObjectTypes objType, int[] ids) =>
                ids.Select(x => new TreeEntityPath { Path = nodePaths[x], Id = x }));

        var authHelper = new UserEditorAuthorizationHelper(
            contentService.Object,
            mediaService.Object,
            entityService.Object,
            AppCaches.Disabled);

        // adding 5555 which currentUser has access to since it's a child of 9876 ... adding is still ok even though currentUser doesn't have access to 1234
        var result = authHelper.IsAuthorized(currentUser, savingUser, new[] { 1234, 5555 }, new int[0], new string[0]);

        Assert.IsTrue(result.Success);
    }

    [Test]
    public void Can_Remove_Content_Start_Node_On_User_Without_Access()
    {
        var nodePaths = new Dictionary<int, string>
        {
            { 1234, "-1,1234" }, { 9876, "-1,9876" }, { 5555, "-1,9876,5555" }, { 4567, "-1,4567" },
        };

        var currentUser = CreateUser(startContentIds: new[] { 9876 });
        var savingUser = CreateUser(startContentIds: new[] { 1234, 4567 });

        var contentService = new Mock<IContentService>();
        contentService.Setup(x => x.GetById(It.IsAny<int>()))
            .Returns((int id) => Mock.Of<IContent>(content => content.Path == nodePaths[id]));
        var mediaService = new Mock<IMediaService>();
        var entityService = new Mock<IEntityService>();
        entityService.Setup(service => service.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
            .Returns((UmbracoObjectTypes objType, int[] ids) =>
                ids.Select(x => new TreeEntityPath { Path = nodePaths[x], Id = x }));

        var authHelper = new UserEditorAuthorizationHelper(
            contentService.Object,
            mediaService.Object,
            entityService.Object,
            AppCaches.Disabled);

        // removing 4567 start node even though currentUser doesn't have acces to it ... removing is ok
        var result = authHelper.IsAuthorized(currentUser, savingUser, new[] { 1234 }, new int[0], new string[0]);

        Assert.IsTrue(result.Success);
    }

    [Test]
    public void Cannot_Add_Content_Start_Node_On_User_Without_Access()
    {
        var nodePaths = new Dictionary<int, string>
        {
            { 1234, "-1,1234" }, { 9876, "-1,9876" }, { 5555, "-1,9876,5555" }, { 4567, "-1,4567" },
        };

        var currentUser = CreateUser(startContentIds: new[] { 9876 });
        var savingUser = CreateUser();

        var contentService = new Mock<IContentService>();
        contentService.Setup(x => x.GetById(It.IsAny<int>()))
            .Returns((int id) => Mock.Of<IContent>(content => content.Path == nodePaths[id]));
        var mediaService = new Mock<IMediaService>();
        var entityService = new Mock<IEntityService>();
        entityService.Setup(service => service.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
            .Returns((UmbracoObjectTypes objType, int[] ids) =>
                ids.Select(x => new TreeEntityPath { Path = nodePaths[x], Id = x }));

        var authHelper = new UserEditorAuthorizationHelper(
            contentService.Object,
            mediaService.Object,
            entityService.Object,
            AppCaches.Disabled);

        // adding 1234 but currentUser doesn't have access to it ... nope
        var result = authHelper.IsAuthorized(currentUser, savingUser, new[] { 1234 }, new int[0], new string[0]);

        Assert.IsFalse(result.Success);
    }

    [Test]
    public void Can_Add_Content_Start_Node_On_User_With_Access()
    {
        var nodePaths = new Dictionary<int, string>
        {
            { 1234, "-1,1234" }, { 9876, "-1,9876" }, { 5555, "-1,9876,5555" }, { 4567, "-1,4567" },
        };

        var currentUser = CreateUser(startContentIds: new[] { 9876 });
        var savingUser = CreateUser();

        var contentService = new Mock<IContentService>();
        contentService.Setup(x => x.GetById(It.IsAny<int>()))
            .Returns((int id) => Mock.Of<IContent>(content => content.Path == nodePaths[id]));
        var mediaService = new Mock<IMediaService>();
        var entityService = new Mock<IEntityService>();
        entityService.Setup(service => service.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
            .Returns((UmbracoObjectTypes objType, int[] ids) =>
                ids.Select(x => new TreeEntityPath { Path = nodePaths[x], Id = x }));

        var authHelper = new UserEditorAuthorizationHelper(
            contentService.Object,
            mediaService.Object,
            entityService.Object,
            AppCaches.Disabled);

        // adding 5555 which currentUser has access to since it's a child of 9876 ... ok
        var result = authHelper.IsAuthorized(currentUser, savingUser, new[] { 5555 }, new int[0], new string[0]);

        Assert.IsTrue(result.Success);
    }

    [Test]
    public void Cannot_Add_Media_Start_Node_On_User_Without_Access()
    {
        var nodePaths = new Dictionary<int, string>
        {
            { 1234, "-1,1234" }, { 9876, "-1,9876" }, { 5555, "-1,9876,5555" }, { 4567, "-1,4567" },
        };

        var currentUser = CreateUser(startMediaIds: new[] { 9876 });
        var savingUser = CreateUser();

        var contentService = new Mock<IContentService>();
        var mediaService = new Mock<IMediaService>();
        mediaService.Setup(x => x.GetById(It.IsAny<int>()))
            .Returns((int id) => Mock.Of<IMedia>(content => content.Path == nodePaths[id]));
        var entityService = new Mock<IEntityService>();
        entityService.Setup(service => service.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
            .Returns((UmbracoObjectTypes objType, int[] ids) =>
                ids.Select(x => new TreeEntityPath { Path = nodePaths[x], Id = x }));

        var authHelper = new UserEditorAuthorizationHelper(
            contentService.Object,
            mediaService.Object,
            entityService.Object,
            AppCaches.Disabled);

        // adding 1234 but currentUser doesn't have access to it ... nope
        var result = authHelper.IsAuthorized(currentUser, savingUser, new int[0], new[] { 1234 }, new string[0]);

        Assert.IsFalse(result.Success);
    }

    [Test]
    public void Can_Add_Media_Start_Node_On_User_With_Access()
    {
        var nodePaths = new Dictionary<int, string>
        {
            { 1234, "-1,1234" }, { 9876, "-1,9876" }, { 5555, "-1,9876,5555" }, { 4567, "-1,4567" },
        };

        var currentUser = CreateUser(startMediaIds: new[] { 9876 });
        var savingUser = CreateUser();

        var contentService = new Mock<IContentService>();
        var mediaService = new Mock<IMediaService>();
        mediaService.Setup(x => x.GetById(It.IsAny<int>()))
            .Returns((int id) => Mock.Of<IMedia>(content => content.Path == nodePaths[id]));
        var entityService = new Mock<IEntityService>();
        entityService.Setup(service => service.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
            .Returns((UmbracoObjectTypes objType, int[] ids) =>
                ids.Select(x => new TreeEntityPath { Path = nodePaths[x], Id = x }));

        var authHelper = new UserEditorAuthorizationHelper(
            contentService.Object,
            mediaService.Object,
            entityService.Object,
            AppCaches.Disabled);

        // adding 5555 which currentUser has access to since it's a child of 9876 ... ok
        var result = authHelper.IsAuthorized(currentUser, savingUser, new int[0], new[] { 5555 }, new string[0]);

        Assert.IsTrue(result.Success);
    }

    [Test]
    public void Can_Add_Another_Media_Start_Node_On_User_With_Access()
    {
        var nodePaths = new Dictionary<int, string>
        {
            { 1234, "-1,1234" }, { 9876, "-1,9876" }, { 5555, "-1,9876,5555" }, { 4567, "-1,4567" },
        };

        var currentUser = CreateUser(startMediaIds: new[] { 9876 });
        var savingUser = CreateUser(startMediaIds: new[] { 1234 });

        var contentService = new Mock<IContentService>();
        var mediaService = new Mock<IMediaService>();
        mediaService.Setup(x => x.GetById(It.IsAny<int>()))
            .Returns((int id) => Mock.Of<IMedia>(content => content.Path == nodePaths[id]));
        var entityService = new Mock<IEntityService>();
        entityService.Setup(service => service.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
            .Returns((UmbracoObjectTypes objType, int[] ids) =>
                ids.Select(x => new TreeEntityPath { Path = nodePaths[x], Id = x }));

        var authHelper = new UserEditorAuthorizationHelper(
            contentService.Object,
            mediaService.Object,
            entityService.Object,
            AppCaches.Disabled);

        // adding 5555 which currentUser has access to since it's a child of 9876 ... adding is still ok even though currentUser doesn't have access to 1234
        var result = authHelper.IsAuthorized(currentUser, savingUser, new int[0], new[] { 1234, 5555 }, new string[0]);

        Assert.IsTrue(result.Success);
    }

    [Test]
    public void Can_Remove_Media_Start_Node_On_User_Without_Access()
    {
        var nodePaths = new Dictionary<int, string>
        {
            { 1234, "-1,1234" }, { 9876, "-1,9876" }, { 5555, "-1,9876,5555" }, { 4567, "-1,4567" },
        };

        var currentUser = CreateUser(startMediaIds: new[] { 9876 });
        var savingUser = CreateUser(startMediaIds: new[] { 1234, 4567 });

        var contentService = new Mock<IContentService>();
        var mediaService = new Mock<IMediaService>();
        mediaService.Setup(x => x.GetById(It.IsAny<int>()))
            .Returns((int id) => Mock.Of<IMedia>(content => content.Path == nodePaths[id]));
        var entityService = new Mock<IEntityService>();
        entityService.Setup(service => service.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
            .Returns((UmbracoObjectTypes objType, int[] ids) =>
                ids.Select(x => new TreeEntityPath { Path = nodePaths[x], Id = x }));

        var authHelper = new UserEditorAuthorizationHelper(
            contentService.Object,
            mediaService.Object,
            entityService.Object,
            AppCaches.Disabled);

        // removing 4567 start node even though currentUser doesn't have acces to it ... removing is ok
        var result = authHelper.IsAuthorized(currentUser, savingUser, new int[0], new[] { 1234 }, new string[0]);

        Assert.IsTrue(result.Success);
    }

    private static IUser CreateUser(bool withGroup = false, int[] startContentIds = null, int[] startMediaIds = null)
    {
        var builder = new UserBuilder()
            .WithStartContentIds(startContentIds ?? new int[0])
            .WithStartMediaIds(startMediaIds ?? new int[0]);
        if (withGroup)
        {
            builder = (UserBuilder)builder
                .AddUserGroup()
                .WithName("Test")
                .WithAlias("test")
                .Done();
        }

        return builder.Build();
    }

    private static IUser CreateAdminUser() =>
        new UserBuilder()
            .AddUserGroup()
            .WithId(1)
            .WithName("Admin")
            .WithAlias(Constants.Security.AdminGroupAlias)
            .Done()
            .Build();
}
