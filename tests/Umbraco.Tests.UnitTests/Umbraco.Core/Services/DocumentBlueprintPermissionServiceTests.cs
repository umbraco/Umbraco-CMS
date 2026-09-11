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
public class DocumentBlueprintPermissionServiceTests
{
    [Test]
    public async Task Access_Allowed_By_Path()
    {
        var user = CreateUser();
        var blueprintKey = Guid.NewGuid();
        var entityServiceMock = new Mock<IEntityService>();
        entityServiceMock
            .Setup(x => x.GetAllPaths(It.IsAny<IEnumerable<UmbracoObjectTypes>>(), It.Is<Guid[]>(keys => keys.Contains(blueprintKey))))
            .Returns([Mock.Of<TreeEntityPath>(entity => entity.Key == blueprintKey && entity.Path == "-1,1234,5678")]);
        IDocumentBlueprintPermissionService sut = new DocumentBlueprintPermissionService(entityServiceMock.Object, AppCaches.Disabled);

        DocumentBlueprintAuthorizationStatus result = await sut.AuthorizeAccessAsync(user, blueprintKey);

        Assert.AreEqual(DocumentBlueprintAuthorizationStatus.Success, result);
    }

    [Test]
    public async Task No_Access_By_Path()
    {
        var user = CreateUser(startDocumentBlueprintId: 9876);
        var blueprintKey = Guid.NewGuid();
        var entityServiceMock = new Mock<IEntityService>();
        entityServiceMock
            .Setup(x => x.GetAllPaths(It.IsAny<IEnumerable<UmbracoObjectTypes>>(), It.Is<Guid[]>(keys => keys.Contains(blueprintKey))))
            .Returns([Mock.Of<TreeEntityPath>(entity => entity.Key == blueprintKey && entity.Path == "-1,1234,5678")]);
        entityServiceMock
            .Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
            .Returns([Mock.Of<TreeEntityPath>(entity => entity.Id == 9876 && entity.Path == "-1,9876")]);
        IDocumentBlueprintPermissionService sut = new DocumentBlueprintPermissionService(entityServiceMock.Object, AppCaches.Disabled);

        DocumentBlueprintAuthorizationStatus result = await sut.AuthorizeAccessAsync(user, blueprintKey);

        Assert.AreEqual(DocumentBlueprintAuthorizationStatus.UnauthorizedMissingPathAccess, result);
    }

    [Test]
    public async Task Access_Allowed_Within_The_Start_Node()
    {
        var user = CreateUser(startDocumentBlueprintId: 1234);
        var blueprintKey = Guid.NewGuid();
        var entityServiceMock = new Mock<IEntityService>();
        entityServiceMock
            .Setup(x => x.GetAllPaths(It.IsAny<IEnumerable<UmbracoObjectTypes>>(), It.Is<Guid[]>(keys => keys.Contains(blueprintKey))))
            .Returns([Mock.Of<TreeEntityPath>(entity => entity.Key == blueprintKey && entity.Path == "-1,1234,5678")]);
        entityServiceMock
            .Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
            .Returns([Mock.Of<TreeEntityPath>(entity => entity.Id == 1234 && entity.Path == "-1,1234")]);
        IDocumentBlueprintPermissionService sut = new DocumentBlueprintPermissionService(entityServiceMock.Object, AppCaches.Disabled);

        DocumentBlueprintAuthorizationStatus result = await sut.AuthorizeAccessAsync(user, blueprintKey);

        Assert.AreEqual(DocumentBlueprintAuthorizationStatus.Success, result);
    }

    [Test]
    public async Task Returns_Not_Found_When_Nothing_Resolves()
    {
        var user = CreateUser();
        var entityServiceMock = new Mock<IEntityService>();
        entityServiceMock
            .Setup(x => x.GetAllPaths(It.IsAny<IEnumerable<UmbracoObjectTypes>>(), It.IsAny<Guid[]>()))
            .Returns([]);
        IDocumentBlueprintPermissionService sut = new DocumentBlueprintPermissionService(entityServiceMock.Object, AppCaches.Disabled);

        DocumentBlueprintAuthorizationStatus result = await sut.AuthorizeAccessAsync(user, Guid.NewGuid());

        Assert.AreEqual(DocumentBlueprintAuthorizationStatus.NotFound, result);
    }

    [Test]
    public async Task Empty_Keys_Returns_Success()
    {
        var user = CreateUser();
        var entityServiceMock = new Mock<IEntityService>();
        IDocumentBlueprintPermissionService sut = new DocumentBlueprintPermissionService(entityServiceMock.Object, AppCaches.Disabled);

        DocumentBlueprintAuthorizationStatus result = await sut.AuthorizeAccessAsync(user, []);

        Assert.AreEqual(DocumentBlueprintAuthorizationStatus.Success, result);
    }

    [Test]
    public async Task Access_To_Root()
    {
        var user = CreateUser();
        var entityServiceMock = new Mock<IEntityService>();
        IDocumentBlueprintPermissionService sut = new DocumentBlueprintPermissionService(entityServiceMock.Object, AppCaches.Disabled);

        DocumentBlueprintAuthorizationStatus result = await sut.AuthorizeRootAccessAsync(user);

        Assert.AreEqual(DocumentBlueprintAuthorizationStatus.Success, result);
    }

    [Test]
    public async Task No_Access_To_Root_When_Scoped_To_A_Container()
    {
        var user = CreateUser(startDocumentBlueprintId: 1234);
        var entityServiceMock = new Mock<IEntityService>();
        entityServiceMock
            .Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
            .Returns([Mock.Of<TreeEntityPath>(entity => entity.Id == 1234 && entity.Path == "-1,1234")]);
        IDocumentBlueprintPermissionService sut = new DocumentBlueprintPermissionService(entityServiceMock.Object, AppCaches.Disabled);

        DocumentBlueprintAuthorizationStatus result = await sut.AuthorizeRootAccessAsync(user);

        Assert.AreEqual(DocumentBlueprintAuthorizationStatus.UnauthorizedMissingRootAccess, result);
    }

    [Test]
    public async Task No_Access_To_Root_Without_A_Start_Node()
    {
        var user = CreateUserWithoutAccess();
        var entityServiceMock = new Mock<IEntityService>();
        IDocumentBlueprintPermissionService sut = new DocumentBlueprintPermissionService(entityServiceMock.Object, AppCaches.Disabled);

        DocumentBlueprintAuthorizationStatus result = await sut.AuthorizeRootAccessAsync(user);

        Assert.AreEqual(DocumentBlueprintAuthorizationStatus.UnauthorizedMissingRootAccess, result);
    }

    [Test]
    public async Task No_Access_To_A_Blueprint_Without_A_Start_Node()
    {
        var user = CreateUserWithoutAccess();
        var blueprintKey = Guid.NewGuid();
        var entityServiceMock = new Mock<IEntityService>();
        entityServiceMock
            .Setup(x => x.GetAllPaths(It.IsAny<IEnumerable<UmbracoObjectTypes>>(), It.Is<Guid[]>(keys => keys.Contains(blueprintKey))))
            .Returns([Mock.Of<TreeEntityPath>(entity => entity.Key == blueprintKey && entity.Path == "-1,1234")]);
        IDocumentBlueprintPermissionService sut = new DocumentBlueprintPermissionService(entityServiceMock.Object, AppCaches.Disabled);

        DocumentBlueprintAuthorizationStatus result = await sut.AuthorizeAccessAsync(user, blueprintKey);

        Assert.AreEqual(DocumentBlueprintAuthorizationStatus.UnauthorizedMissingPathAccess, result);
    }

    private static User CreateUser(int? startDocumentBlueprintId = null)
    {
        UserBuilder builder = new UserBuilder().WithId(0);

        if (startDocumentBlueprintId.HasValue)
        {
            builder = (UserBuilder)builder.WithStartDocumentBlueprintId(startDocumentBlueprintId.Value);
        }

        return builder
            .AddUserGroup()
                .WithId(1)
                .WithName("admin")
                .WithAlias("admin")
                .WithStartDocumentBlueprintId(startDocumentBlueprintId ?? Constants.System.Root)
                .Done()
            .Build();
    }

    /// <summary>
    /// Builds a user whose groups grant no document blueprint start node at all, which is what denies
    /// access outright. The builders default every start node to the root, and a user level assignment
    /// only ever overrides a group one, so this cannot be expressed through them.
    /// </summary>
    private static User CreateUserWithoutAccess()
    {
        User user = new UserBuilder().WithId(0).WithStartDocumentBlueprintIds([]).Build();
        user.ClearGroups();
        user.AddGroup(Mock.Of<IReadOnlyUserGroup>(x =>
            x.Id == 1 &&
            x.Alias == "editor" &&
            x.StartDocumentBlueprintId == null));

        return user;
    }
}
