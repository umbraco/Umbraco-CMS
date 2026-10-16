// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.AuthorizationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class UserGroupPermissionServiceTests
{
    private static readonly Guid _userGroupKey = Guid.NewGuid();

    private static readonly Dictionary<int, string> _documentBlueprintContainerPaths = new()
    {
        { 1234, "-1,1234" }, { 9876, "-1,9876" }, { 5555, "-1,9876,5555" },
    };

    [TestCase(Constants.System.Root, UserGroupAuthorizationStatus.UnauthorizedMissingDocumentBlueprintStartNodeAccess)]
    [TestCase(1234, UserGroupAuthorizationStatus.UnauthorizedMissingDocumentBlueprintStartNodeAccess)]
    [TestCase(9876, UserGroupAuthorizationStatus.Success)]
    [TestCase(5555, UserGroupAuthorizationStatus.Success)]
    public async Task Cannot_Update_A_User_Group_To_A_Document_Blueprint_Start_Node_Outside_Own_Access(
        int startDocumentBlueprintId,
        UserGroupAuthorizationStatus expectedStatus)
    {
        IUser user = CreateUserWithDocumentBlueprintStartNode(9876);
        IUserGroup userGroup = CreateUserGroup(startDocumentBlueprintId);

        UserGroupAuthorizationStatus result = await CreateService().AuthorizeUpdateAsync(user, userGroup);

        Assert.AreEqual(expectedStatus, result);
    }

    [TestCase(Constants.System.Root, UserGroupAuthorizationStatus.UnauthorizedMissingDocumentBlueprintStartNodeAccess)]
    [TestCase(1234, UserGroupAuthorizationStatus.UnauthorizedMissingDocumentBlueprintStartNodeAccess)]
    [TestCase(5555, UserGroupAuthorizationStatus.Success)]
    public async Task Cannot_Create_A_User_Group_With_A_Document_Blueprint_Start_Node_Outside_Own_Access(
        int startDocumentBlueprintId,
        UserGroupAuthorizationStatus expectedStatus)
    {
        IUser user = CreateUserWithDocumentBlueprintStartNode(9876);
        IUserGroup userGroup = CreateUserGroup(startDocumentBlueprintId);

        UserGroupAuthorizationStatus result = await CreateService().AuthorizeCreateAsync(user, userGroup);

        Assert.AreEqual(expectedStatus, result);
    }

    [Test]
    public async Task Can_Update_A_User_Group_To_Document_Blueprint_Root_With_Root_Access()
    {
        IUser user = CreateUserWithDocumentBlueprintStartNode(Constants.System.Root);
        IUserGroup userGroup = CreateUserGroup(Constants.System.Root);

        UserGroupAuthorizationStatus result = await CreateService().AuthorizeUpdateAsync(user, userGroup);

        Assert.AreEqual(UserGroupAuthorizationStatus.Success, result);
    }

    private static UserGroupPermissionService CreateService()
    {
        var entityService = new Mock<IEntityService>();
        entityService
            .Setup(x => x.GetAllPaths(UmbracoObjectTypes.DocumentBlueprintContainer, It.IsAny<int[]>()))
            .Returns((UmbracoObjectTypes objectType, int[] ids) => ids
                .Where(_documentBlueprintContainerPaths.ContainsKey)
                .Select(id => new TreeEntityPath { Id = id, Path = _documentBlueprintContainerPaths[id] }));
        entityService
            .Setup(x => x.Get(It.IsAny<int>(), UmbracoObjectTypes.DocumentBlueprintContainer))
            .Returns((int id, UmbracoObjectTypes objectType) =>
                Mock.Of<IEntitySlim>(entity => entity.Id == id && entity.Path == _documentBlueprintContainerPaths[id]));

        return new UserGroupPermissionService(
            Mock.Of<IContentService>(),
            Mock.Of<IMediaService>(),
            entityService.Object,
            AppCaches.Disabled);
    }

    private static IUser CreateUserWithDocumentBlueprintStartNode(int startDocumentBlueprintId)
        => new UserBuilder()
            .WithStartDocumentBlueprintIds([])
            .AddUserGroup()
                .WithKey(_userGroupKey)
                .WithAlias("editor")
                .WithAllowedSections([Constants.Applications.Users])
                .WithStartDocumentBlueprintId(startDocumentBlueprintId)
                .Done()
            .Build();

    private static IUserGroup CreateUserGroup(int startDocumentBlueprintId)
        => new UserGroupBuilder()
            .WithKey(_userGroupKey)
            .WithAlias("editor")
            .WithAllowedSections([Constants.Applications.Users])
            .WithStartDocumentBlueprintId(startDocumentBlueprintId)
            .Build();
}
