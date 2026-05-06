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
public class ElementPermissionServiceTests
{
    private const int ElementNodeId = 5678;
    private const string ElementNodePath = "-1,1234,5678";
    private const int UserStartNodeId = 1234;
    private const string UserStartNodePath = "-1,1234";
    private const int UnrelatedStartNodeId = 9876;

    private Mock<IEntityService> _entityServiceMock;
    private Mock<IUserService> _userServiceMock;
    private Mock<ILanguageService> _languageServiceMock;
    private IElementPermissionService _sut;

    [SetUp]
    public void SetUp()
    {
        _entityServiceMock = new Mock<IEntityService>();
        _userServiceMock = new Mock<IUserService>();
        _languageServiceMock = new Mock<ILanguageService>();
        _sut = new ElementPermissionService(
            _entityServiceMock.Object,
            _userServiceMock.Object,
            AppCaches.Disabled,
            _languageServiceMock.Object);
    }

    [Test]
    public async Task Can_Authorize_Access_By_Path()
    {
        // Arrange
        var elementKey = Guid.NewGuid();
        var user = CreateUser();

        _entityServiceMock
            .Setup(x => x.GetAllPaths(It.Is<IEnumerable<UmbracoObjectTypes>>(t => t.Contains(UmbracoObjectTypes.Element)), new[] { elementKey }))
            .Returns([CreateTreeEntityPath(elementKey, ElementNodeId, ElementNodePath)]);

        SetupPermissions(user, ElementNodePath, ["A"]);

        // Act
        var result = await _sut.AuthorizeAccessAsync(user, elementKey, "A");

        // Assert
        Assert.That(result, Is.EqualTo(ElementAuthorizationStatus.Success));
    }

    [TestCase("F", ElementAuthorizationStatus.Success)]
    [TestCase("X", ElementAuthorizationStatus.UnauthorizedMissingDescendantAccess)]
    public async Task Can_Authorize_Descendants_Access_With_Required_Permission(string permissionToCheck, ElementAuthorizationStatus expectedResult)
    {
        // Arrange
        var containerKey = Guid.NewGuid();
        var user = CreateUser();

        _entityServiceMock
            .Setup(x => x.Get(containerKey, UmbracoObjectTypes.ElementContainer))
            .Returns(new EntitySlim { NodeObjectType = Constants.ObjectTypes.ElementContainer });

        long total = 1;
        _entityServiceMock
            .Setup(x => x.GetPagedDescendants(
                containerKey,
                UmbracoObjectTypes.ElementContainer,
                It.Is<IEnumerable<UmbracoObjectTypes>>(t => t.Contains(UmbracoObjectTypes.Element)),
                It.IsAny<int>(),
                It.IsAny<int>(),
                out total,
                null,
                It.IsAny<Ordering>()
            ))
            .Returns(
            [
                new EntitySlim
                {
                    NodeObjectType = Constants.ObjectTypes.ElementContainer,
                    Id = ElementNodeId,
                    Path = ElementNodePath,
                }
            ]);

        SetupPermissions(user, ElementNodePath, ["A", "F", "C"]);

        // Act
        var result = await _sut.AuthorizeDescendantsAccessAsync(user, containerKey, permissionToCheck);

        // Assert
        Assert.That(result, Is.EqualTo(expectedResult));
    }

    [Test]
    public async Task Cannot_Authorize_Access_When_Element_Not_Found()
    {
        // Arrange
        var elementKey = Guid.NewGuid();
        var user = CreateUser();

        _entityServiceMock
            .Setup(x => x.GetAllPaths(It.Is<IEnumerable<UmbracoObjectTypes>>(t => t.Contains(UmbracoObjectTypes.Element)), new[] { elementKey }))
            .Returns([]);

        // Act
        var result = await _sut.AuthorizeAccessAsync(user, elementKey, "A");

        // Assert
        Assert.That(result, Is.EqualTo(ElementAuthorizationStatus.NotFound));
    }

    [Test]
    public async Task Cannot_Authorize_Descendants_Access_When_Element_Container_Not_Found()
    {
        // Arrange
        var containerKey = Guid.NewGuid();
        var user = CreateUser();

        _entityServiceMock
            .Setup(x => x.Get(containerKey, UmbracoObjectTypes.ElementContainer))
            .Returns((IEntitySlim?)null);

        // Act
        var result = await _sut.AuthorizeDescendantsAccessAsync(user, containerKey, "A");

        // Assert
        Assert.That(result, Is.EqualTo(ElementAuthorizationStatus.NotFound));
    }

    [Test]
    public async Task Cannot_Authorize_Access_Without_Path_Access()
    {
        // Arrange
        var elementKey = Guid.NewGuid();
        var user = CreateUser(startElementId: UnrelatedStartNodeId);

        _entityServiceMock
            .Setup(x => x.GetAllPaths(It.Is<IEnumerable<UmbracoObjectTypes>>(t => t.Contains(UmbracoObjectTypes.Element)), new[] { elementKey }))
            .Returns([CreateTreeEntityPath(elementKey, ElementNodeId, ElementNodePath)]);

        _entityServiceMock
            .Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
            .Returns([CreateTreeEntityPath(Guid.NewGuid(), UnrelatedStartNodeId, $"-1,{UnrelatedStartNodeId}")]);

        // Act
        var result = await _sut.AuthorizeAccessAsync(user, elementKey, "A");

        // Assert
        Assert.That(result, Is.EqualTo(ElementAuthorizationStatus.UnauthorizedMissingPathAccess));
    }

    [Test]
    public async Task Cannot_Authorize_Access_Without_Required_Permission()
    {
        // Arrange
        var elementKey = Guid.NewGuid();
        var user = CreateUser();

        _entityServiceMock
            .Setup(x => x.GetAllPaths(It.Is<IEnumerable<UmbracoObjectTypes>>(t => t.Contains(UmbracoObjectTypes.Element)), new[] { elementKey }))
            .Returns([CreateTreeEntityPath(elementKey, ElementNodeId, ElementNodePath)]);

        SetupPermissions(user, ElementNodePath, ["A", "B", "C"]);

        // Act
        var result = await _sut.AuthorizeAccessAsync(user, elementKey, "F");

        // Assert
        Assert.That(result, Is.EqualTo(ElementAuthorizationStatus.UnauthorizedMissingPermissionAccess));
    }

    [Test]
    public async Task Can_Authorize_Access_With_Required_Permission()
    {
        // Arrange
        var elementKey = Guid.NewGuid();
        var user = CreateUser();

        _entityServiceMock
            .Setup(x => x.GetAllPaths(It.Is<IEnumerable<UmbracoObjectTypes>>(t => t.Contains(UmbracoObjectTypes.Element)), new[] { elementKey }))
            .Returns([CreateTreeEntityPath(elementKey, ElementNodeId, ElementNodePath)]);

        SetupPermissions(user, ElementNodePath, ["A", "F", "C"]);

        // Act
        var result = await _sut.AuthorizeAccessAsync(user, elementKey, "F");

        // Assert
        Assert.That(result, Is.EqualTo(ElementAuthorizationStatus.Success));
    }

    [Test]
    public async Task Cannot_Authorize_Access_With_Empty_Keys()
    {
        // Arrange
        var user = CreateUser();

        // Act
        var result = await _sut.AuthorizeAccessAsync(user, [], new HashSet<string> { "A" });

        // Assert
        Assert.That(result, Is.EqualTo(ElementAuthorizationStatus.NotFound));
    }

    [Test]
    public async Task Can_Authorize_Root_Access_By_Path()
    {
        // Arrange
        var user = CreateUser();

        SetupPermissions(user, Constants.System.RootString, ["A"]);

        // Act
        var result = await _sut.AuthorizeRootAccessAsync(user, "A");

        // Assert
        Assert.That(result, Is.EqualTo(ElementAuthorizationStatus.Success));
    }

    [Test]
    public async Task Cannot_Authorize_Root_Access_By_Path()
    {
        // Arrange
        var user = CreateUser(startElementId: UserStartNodeId);

        _entityServiceMock
            .Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
            .Returns([CreateTreeEntityPath(Guid.NewGuid(), UserStartNodeId, UserStartNodePath)]);

        // Act
        var result = await _sut.AuthorizeRootAccessAsync(user, "A");

        // Assert
        Assert.That(result, Is.EqualTo(ElementAuthorizationStatus.UnauthorizedMissingRootAccess));
    }

    [Test]
    public async Task Cannot_Authorize_Root_Access_By_Permission()
    {
        // Arrange
        var user = CreateUser();

        SetupPermissions(user, Constants.System.RootString, ["A"]);

        // Act
        var result = await _sut.AuthorizeRootAccessAsync(user, "B");

        // Assert
        Assert.That(result, Is.EqualTo(ElementAuthorizationStatus.UnauthorizedMissingPermissionAccess));
    }

    [Test]
    public async Task Can_Authorize_Bin_Access_By_Path()
    {
        // Arrange
        var user = CreateUser();

        SetupPermissions(user, Constants.System.RecycleBinElementString, ["A"]);

        // Act
        var result = await _sut.AuthorizeBinAccessAsync(user, "A");

        // Assert
        Assert.That(result, Is.EqualTo(ElementAuthorizationStatus.Success));
    }

    [Test]
    public async Task Cannot_Authorize_Bin_Access_By_Path()
    {
        // Arrange
        var user = CreateUser(startElementId: UserStartNodeId);

        _entityServiceMock
            .Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
            .Returns([CreateTreeEntityPath(Guid.NewGuid(), UserStartNodeId, UserStartNodePath)]);

        // Act
        var result = await _sut.AuthorizeBinAccessAsync(user, "A");

        // Assert
        Assert.That(result, Is.EqualTo(ElementAuthorizationStatus.UnauthorizedMissingBinAccess));
    }

    [Test]
    public async Task Cannot_Authorize_Bin_Access_By_Permission()
    {
        // Arrange
        var user = CreateUser();

        SetupPermissions(user, Constants.System.RecycleBinElementString, ["A"]);

        // Act
        var result = await _sut.AuthorizeBinAccessAsync(user, "B");

        // Assert
        Assert.That(result, Is.EqualTo(ElementAuthorizationStatus.UnauthorizedMissingPermissionAccess));
    }

    [Test]
    public async Task Can_Authorize_Culture_Access_When_Group_Has_All_Languages()
    {
        // Arrange
        var user = CreateUserWithAllLanguageAccess();

        // Act
        var result = await _sut.AuthorizeCultureAccessAsync(user, new HashSet<string> { "en-US", "da-DK" });

        // Assert
        Assert.That(result, Is.EqualTo(ElementAuthorizationStatus.Success));
    }

    [Test]
    public async Task Can_Authorize_Culture_Access_When_User_Has_Language()
    {
        // Arrange
        var languageId = 1;
        var user = CreateUserWithLanguageAccess(languageId);

        _languageServiceMock
            .Setup(x => x.GetIsoCodesByIdsAsync(It.Is<ICollection<int>>(ids => ids.Contains(languageId))))
            .ReturnsAsync(["en-US"]);

        // Act
        var result = await _sut.AuthorizeCultureAccessAsync(user, new HashSet<string> { "en-US" });

        // Assert
        Assert.That(result, Is.EqualTo(ElementAuthorizationStatus.Success));
    }

    [Test]
    public async Task Cannot_Authorize_Culture_Access_When_User_Lacks_Language()
    {
        // Arrange
        var languageId = 1;
        var user = CreateUserWithLanguageAccess(languageId);

        _languageServiceMock
            .Setup(x => x.GetIsoCodesByIdsAsync(It.Is<ICollection<int>>(ids => ids.Contains(languageId))))
            .ReturnsAsync(["en-US"]);

        // Act
        var result = await _sut.AuthorizeCultureAccessAsync(user, new HashSet<string> { "da-DK" });

        // Assert
        Assert.That(result, Is.EqualTo(ElementAuthorizationStatus.UnauthorizedMissingCulture));
    }

    private static IUser CreateUser(int id = 0, int? startElementId = null, bool withUserGroup = true)
    {
        var builder = new UserBuilder()
            .WithId(id)
            .WithStartElementIds(startElementId.HasValue ? [startElementId.Value] : []);

        if (withUserGroup)
        {
            builder = builder
                .AddUserGroup()
                    .WithId(1)
                    .WithName("admin")
                    .WithAlias("admin")
                .Done();
        }

        return builder.Build();
    }

    private static IUser CreateUserWithAllLanguageAccess()
    {
        var userGroupMock = new Mock<IReadOnlyUserGroup>();
        userGroupMock.Setup(x => x.HasAccessToAllLanguages).Returns(true);
        userGroupMock.Setup(x => x.AllowedLanguages).Returns(new HashSet<int>());
        // userGroupMock.Setup(x => x.StartContentId).Returns(-1);
        // userGroupMock.Setup(x => x.StartMediaId).Returns(-1);
        userGroupMock.Setup(x => x.StartElementId).Returns(-1);

        var userMock = new Mock<IUser>();
        userMock.Setup(x => x.Groups).Returns([userGroupMock.Object]);
        return userMock.Object;
    }

    private static IUser CreateUserWithLanguageAccess(int languageId)
    {
        var userGroupMock = new Mock<IReadOnlyUserGroup>();
        userGroupMock.Setup(x => x.HasAccessToAllLanguages).Returns(false);
        userGroupMock.Setup(x => x.AllowedLanguages).Returns(new HashSet<int> { languageId });
        // userGroupMock.Setup(x => x.StartContentId).Returns(-1);
        // userGroupMock.Setup(x => x.StartMediaId).Returns(-1);
        userGroupMock.Setup(x => x.StartElementId).Returns(-1);

        var userMock = new Mock<IUser>();
        userMock.Setup(x => x.Groups).Returns([userGroupMock.Object]);
        return userMock.Object;
    }

    private void SetupPermissions(IUser user, string path, string[] assignedPermissions)
    {
        var permissions = new EntityPermissionCollection { new(9876, 1234, assignedPermissions.ToHashSet()) };
        var permissionSet = new EntityPermissionSet(1234, permissions);
        _userServiceMock.Setup(x => x.GetPermissionsForPath(user, path)).Returns(permissionSet);
    }

    private static TreeEntityPath CreateTreeEntityPath(Guid key, int id, string path)
        => Mock.Of<TreeEntityPath>(e => e.Key == key && e.Id == id && e.Path == path);
}
