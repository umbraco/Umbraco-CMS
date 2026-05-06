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
public class ContentPermissionServiceTests
{
    private const int ContentNodeId = 5678;
    private const string ContentNodePath = "-1,1234,5678";
    private const int UserStartNodeId = 1234;
    private const string UserStartNodePath = "-1,1234";
    private const int UnrelatedStartNodeId = 9876;

    private Mock<IContentService> _contentServiceMock;
    private Mock<IEntityService> _entityServiceMock;
    private Mock<IUserService> _userServiceMock;
    private Mock<ILanguageService> _languageServiceMock;
    private IContentPermissionService _sut;

    [SetUp]
    public void SetUp()
    {
        _contentServiceMock = new Mock<IContentService>();
        _entityServiceMock = new Mock<IEntityService>();
        _userServiceMock = new Mock<IUserService>();
        _languageServiceMock = new Mock<ILanguageService>();
        _sut = new ContentPermissionService(
            _contentServiceMock.Object,
            _entityServiceMock.Object,
            _userServiceMock.Object,
            AppCaches.Disabled,
            _languageServiceMock.Object);
    }

    [Test]
    public async Task Can_Authorize_Access_By_Path()
    {
        // Arrange
        var contentKey = Guid.NewGuid();
        var user = CreateUser();

        _entityServiceMock
            .Setup(x => x.GetAllPaths(UmbracoObjectTypes.Document, new[] { contentKey }))
            .Returns([CreateTreeEntityPath(contentKey, ContentNodeId, ContentNodePath)]);

        SetupPermissions(user, ContentNodePath, ["A"]);

        // Act
        var result = await _sut.AuthorizeAccessAsync(user, contentKey, "A");

        // Assert
        Assert.That(result, Is.EqualTo(ContentAuthorizationStatus.Success));
    }

    [Test]
    public async Task Cannot_Authorize_Access_When_Content_Not_Found()
    {
        // Arrange
        var contentKey = Guid.NewGuid();
        var user = CreateUser();

        _entityServiceMock
            .Setup(x => x.GetAllPaths(UmbracoObjectTypes.Document, new[] { contentKey }))
            .Returns([]);

        // Act
        var result = await _sut.AuthorizeAccessAsync(user, contentKey, "A");

        // Assert
        Assert.That(result, Is.EqualTo(ContentAuthorizationStatus.NotFound));
    }

    [Test]
    public async Task Cannot_Authorize_Access_Without_Path_Access()
    {
        // Arrange
        var contentKey = Guid.NewGuid();
        var user = CreateUser(startContentId: UnrelatedStartNodeId);

        _entityServiceMock
            .Setup(x => x.GetAllPaths(UmbracoObjectTypes.Document, new[] { contentKey }))
            .Returns([CreateTreeEntityPath(contentKey, ContentNodeId, ContentNodePath)]);

        _entityServiceMock
            .Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
            .Returns([CreateTreeEntityPath(Guid.NewGuid(), UnrelatedStartNodeId, $"-1,{UnrelatedStartNodeId}")]);

        // Act
        var result = await _sut.AuthorizeAccessAsync(user, contentKey, "A");

        // Assert
        Assert.That(result, Is.EqualTo(ContentAuthorizationStatus.UnauthorizedMissingPathAccess));
    }

    [Test]
    public async Task Cannot_Authorize_Access_Without_Required_Permission()
    {
        // Arrange
        var contentKey = Guid.NewGuid();
        var user = CreateUser();

        _entityServiceMock
            .Setup(x => x.GetAllPaths(UmbracoObjectTypes.Document, new[] { contentKey }))
            .Returns([CreateTreeEntityPath(contentKey, ContentNodeId, ContentNodePath)]);

        SetupPermissions(user, ContentNodePath, ["A", "B", "C"]);

        // Act
        var result = await _sut.AuthorizeAccessAsync(user, contentKey, "F");

        // Assert
        Assert.That(result, Is.EqualTo(ContentAuthorizationStatus.UnauthorizedMissingPermissionAccess));
    }

    [Test]
    public async Task Can_Authorize_Access_With_Required_Permission()
    {
        // Arrange
        var contentKey = Guid.NewGuid();
        var user = CreateUser();

        _entityServiceMock
            .Setup(x => x.GetAllPaths(UmbracoObjectTypes.Document, new[] { contentKey }))
            .Returns([CreateTreeEntityPath(contentKey, ContentNodeId, ContentNodePath)]);

        SetupPermissions(user, ContentNodePath, ["A", "F", "C"]);

        // Act
        var result = await _sut.AuthorizeAccessAsync(user, contentKey, "F");

        // Assert
        Assert.That(result, Is.EqualTo(ContentAuthorizationStatus.Success));
    }

    [Test]
    public async Task Can_Authorize_Access_With_Empty_Keys()
    {
        // Arrange
        var user = CreateUser();

        // Act
        var result = await _sut.AuthorizeAccessAsync(user, [], new HashSet<string> { "A" });

        // Assert
        Assert.That(result, Is.EqualTo(ContentAuthorizationStatus.Success));
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
        Assert.That(result, Is.EqualTo(ContentAuthorizationStatus.Success));
    }

    [Test]
    public async Task Cannot_Authorize_Root_Access_By_Path()
    {
        // Arrange
        var user = CreateUser(startContentId: UserStartNodeId);

        _entityServiceMock
            .Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
            .Returns([CreateTreeEntityPath(Guid.NewGuid(), UserStartNodeId, UserStartNodePath)]);

        // Act
        var result = await _sut.AuthorizeRootAccessAsync(user, "A");

        // Assert
        Assert.That(result, Is.EqualTo(ContentAuthorizationStatus.UnauthorizedMissingRootAccess));
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
        Assert.That(result, Is.EqualTo(ContentAuthorizationStatus.UnauthorizedMissingPermissionAccess));
    }

    [Test]
    public async Task Can_Authorize_Bin_Access_By_Path()
    {
        // Arrange
        var user = CreateUser();

        SetupPermissions(user, Constants.System.RecycleBinContentString, ["A"]);

        // Act
        var result = await _sut.AuthorizeBinAccessAsync(user, "A");

        // Assert
        Assert.That(result, Is.EqualTo(ContentAuthorizationStatus.Success));
    }

    [Test]
    public async Task Cannot_Authorize_Bin_Access_By_Path()
    {
        // Arrange
        var user = CreateUser(startContentId: UserStartNodeId);

        _entityServiceMock
            .Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
            .Returns([CreateTreeEntityPath(Guid.NewGuid(), UserStartNodeId, UserStartNodePath)]);

        // Act
        var result = await _sut.AuthorizeBinAccessAsync(user, "A");

        // Assert
        Assert.That(result, Is.EqualTo(ContentAuthorizationStatus.UnauthorizedMissingBinAccess));
    }

    [Test]
    public async Task Cannot_Authorize_Bin_Access_By_Permission()
    {
        // Arrange
        var user = CreateUser();

        SetupPermissions(user, Constants.System.RecycleBinContentString, ["A"]);

        // Act
        var result = await _sut.AuthorizeBinAccessAsync(user, "B");

        // Assert
        Assert.That(result, Is.EqualTo(ContentAuthorizationStatus.UnauthorizedMissingPermissionAccess));
    }

    [Test]
    public async Task Can_Authorize_Culture_Access_When_Group_Has_All_Languages()
    {
        // Arrange
        var user = CreateUserWithAllLanguageAccess();

        // Act
        var result = await _sut.AuthorizeCultureAccessAsync(user, new HashSet<string> { "en-US", "da-DK" });

        // Assert
        Assert.That(result, Is.EqualTo(ContentAuthorizationStatus.Success));
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
        Assert.That(result, Is.EqualTo(ContentAuthorizationStatus.Success));
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
        Assert.That(result, Is.EqualTo(ContentAuthorizationStatus.UnauthorizedMissingCulture));
    }

    private static IUser CreateUser(int id = 0, int? startContentId = null, bool withUserGroup = true)
    {
        var builder = new UserBuilder()
            .WithId(id)
            .WithStartContentIds(startContentId.HasValue ? [startContentId.Value] : []);

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
        userGroupMock.Setup(x => x.StartContentId).Returns(-1);
        userGroupMock.Setup(x => x.StartMediaId).Returns(-1);

        var userMock = new Mock<IUser>();
        userMock.Setup(x => x.Groups).Returns([userGroupMock.Object]);
        return userMock.Object;
    }

    private static IUser CreateUserWithLanguageAccess(int languageId)
    {
        var userGroupMock = new Mock<IReadOnlyUserGroup>();
        userGroupMock.Setup(x => x.HasAccessToAllLanguages).Returns(false);
        userGroupMock.Setup(x => x.AllowedLanguages).Returns(new HashSet<int> { languageId });
        userGroupMock.Setup(x => x.StartContentId).Returns(-1);
        userGroupMock.Setup(x => x.StartMediaId).Returns(-1);

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
