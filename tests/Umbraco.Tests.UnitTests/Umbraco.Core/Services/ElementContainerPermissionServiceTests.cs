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
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class ElementContainerPermissionServiceTests
{
    private const int ElementContainerNodeId = 5678;
    private const string ElementContainerNodePath = "-1,1234,5678";
    private const int UserStartNodeId = 1234;
    private const string UserStartNodePath = "-1,1234";
    private const int UnrelatedStartNodeId = 9876;

    private Mock<IEntityService> _entityServiceMock;
    private Mock<IUserService> _userServiceMock;
    private IElementContainerPermissionService _sut;

    [SetUp]
    public void SetUp()
    {
        _entityServiceMock = new Mock<IEntityService>();
        _userServiceMock = new Mock<IUserService>();
        _sut = new ElementContainerPermissionService(
            _entityServiceMock.Object,
            _userServiceMock.Object,
            AppCaches.Disabled);
    }

    [Test]
    public async Task Can_Authorize_Access_By_Path()
    {
        // Arrange
        var containerKey = Guid.NewGuid();
        var user = CreateUser();

        _entityServiceMock
            .Setup(x => x.GetAllPaths(It.Is<IEnumerable<UmbracoObjectTypes>>(t => t.Contains(UmbracoObjectTypes.ElementContainer)), new[] { containerKey }))
            .Returns([CreateTreeEntityPath(containerKey, ElementContainerNodeId, ElementContainerNodePath)]);

        SetupPermissions(user, ElementContainerNodePath, ["A"]);

        // Act
        var result = await _sut.AuthorizeAccessAsync(user, containerKey, "A");

        // Assert
        Assert.That(result, Is.EqualTo(ElementAuthorizationStatus.Success));
    }

    [Test]
    public async Task Cannot_Authorize_Access_When_ElementContainer_Not_Found()
    {
        // Arrange
        var containerKey = Guid.NewGuid();
        var user = CreateUser();

        _entityServiceMock
            .Setup(x => x.GetAllPaths(It.Is<IEnumerable<UmbracoObjectTypes>>(t => t.Contains(UmbracoObjectTypes.ElementContainer)), new[] { containerKey }))
            .Returns([]);

        // Act
        var result = await _sut.AuthorizeAccessAsync(user, containerKey, "A");

        // Assert
        Assert.That(result, Is.EqualTo(ElementAuthorizationStatus.NotFound));
    }

    [Test]
    public async Task Cannot_Authorize_Access_Without_Path_Access()
    {
        // Arrange
        var containerKey = Guid.NewGuid();
        var user = CreateUser(startElementId: UnrelatedStartNodeId);

        _entityServiceMock
            .Setup(x => x.GetAllPaths(It.Is<IEnumerable<UmbracoObjectTypes>>(t => t.Contains(UmbracoObjectTypes.ElementContainer)), new[] { containerKey }))
            .Returns([CreateTreeEntityPath(containerKey, ElementContainerNodeId, ElementContainerNodePath)]);

        _entityServiceMock
            .Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
            .Returns([CreateTreeEntityPath(Guid.NewGuid(), UnrelatedStartNodeId, $"-1,{UnrelatedStartNodeId}")]);

        // Act
        var result = await _sut.AuthorizeAccessAsync(user, containerKey, "A");

        // Assert
        Assert.That(result, Is.EqualTo(ElementAuthorizationStatus.UnauthorizedMissingPathAccess));
    }

    [Test]
    public async Task Cannot_Authorize_Access_Without_Required_Permission()
    {
        // Arrange
        var containerKey = Guid.NewGuid();
        var user = CreateUser();

        _entityServiceMock
            .Setup(x => x.GetAllPaths(It.Is<IEnumerable<UmbracoObjectTypes>>(t => t.Contains(UmbracoObjectTypes.ElementContainer)), new[] { containerKey }))
            .Returns([CreateTreeEntityPath(containerKey, ElementContainerNodeId, ElementContainerNodePath)]);

        SetupPermissions(user, ElementContainerNodePath, ["A", "B", "C"]);

        // Act
        var result = await _sut.AuthorizeAccessAsync(user, containerKey, "F");

        // Assert
        Assert.That(result, Is.EqualTo(ElementAuthorizationStatus.UnauthorizedMissingPermissionAccess));
    }

    [Test]
    public async Task Can_Authorize_Access_With_Required_Permission()
    {
        // Arrange
        var containerKey = Guid.NewGuid();
        var user = CreateUser();

        _entityServiceMock
            .Setup(x => x.GetAllPaths(It.Is<IEnumerable<UmbracoObjectTypes>>(t => t.Contains(UmbracoObjectTypes.ElementContainer)), new[] { containerKey }))
            .Returns([CreateTreeEntityPath(containerKey, ElementContainerNodeId, ElementContainerNodePath)]);

        SetupPermissions(user, ElementContainerNodePath, ["A", "F", "C"]);

        // Act
        var result = await _sut.AuthorizeAccessAsync(user, containerKey, "F");

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

    private void SetupPermissions(IUser user, string path, string[] assignedPermissions)
    {
        var permissions = new EntityPermissionCollection { new(9876, 1234, assignedPermissions.ToHashSet()) };
        var permissionSet = new EntityPermissionSet(1234, permissions);
        _userServiceMock.Setup(x => x.GetPermissionsForPath(user, path)).Returns(permissionSet);
    }

    private static TreeEntityPath CreateTreeEntityPath(Guid key, int id, string path)
        => Mock.Of<TreeEntityPath>(e => e.Key == key && e.Id == id && e.Path == path);
}
