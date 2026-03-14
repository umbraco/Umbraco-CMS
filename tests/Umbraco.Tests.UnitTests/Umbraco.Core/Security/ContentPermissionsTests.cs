// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Security;

/// <summary>
/// Contains unit tests that verify the behavior of content permissions in the Umbraco CMS core security module.
/// </summary>
[TestFixture]
public class ContentPermissionsTests
{
    /// <summary>
    /// Verifies that access is granted to a user when the content's path matches the expected value.
    /// Ensures that ContentPermissions.CheckPermissions returns Granted for valid paths.
    /// </summary>
    [Test]
    public void Access_Allowed_By_Path()
    {
        // Arrange
        var user = CreateUser(9);
        var contentMock = new Mock<IContent>();
        contentMock.Setup(c => c.Path).Returns("-1,1234,5678");
        var content = contentMock.Object;
        var contentServiceMock = new Mock<IContentService>();
        contentServiceMock.Setup(x => x.GetById(1234)).Returns(content);
        var contentService = contentServiceMock.Object;
        var entityServiceMock = new Mock<IEntityService>();
        var entityService = entityServiceMock.Object;
        var userServiceMock = new Mock<IUserService>();
        var userService = userServiceMock.Object;
        var contentPermissions = new ContentPermissions(userService, contentService, entityService, AppCaches.Disabled);

        // Act
        var result = contentPermissions.CheckPermissions(1234, user, out IContent _);

        // Assert
        Assert.AreEqual(ContentPermissions.ContentAccess.Granted, result);
    }

    /// <summary>
    /// Tests that the CheckPermissions method returns NotFound when no content is found.
    /// </summary>
    [Test]
    public void No_Content_Found()
    {
        // Arrange
        var user = CreateUser(9);
        var contentMock = new Mock<IContent>();
        contentMock.Setup(c => c.Path).Returns("-1,1234,5678");
        var content = contentMock.Object;
        var contentServiceMock = new Mock<IContentService>();
        contentServiceMock.Setup(x => x.GetById(0)).Returns(content);
        var contentService = contentServiceMock.Object;
        var userServiceMock = new Mock<IUserService>();
        var permissions = new EntityPermissionCollection();
        var permissionSet = new EntityPermissionSet(1234, permissions);
        userServiceMock.Setup(x => x.GetPermissionsForPath(user, "-1,1234,5678")).Returns(permissionSet);
        var userService = userServiceMock.Object;
        var entityServiceMock = new Mock<IEntityService>();
        var entityService = entityServiceMock.Object;
        var contentPermissions = new ContentPermissions(userService, contentService, entityService, AppCaches.Disabled);

        // Act
        var result = contentPermissions.CheckPermissions(1234, user, out IContent _, new[] { "F" }.ToHashSet());

        // Assert
        Assert.AreEqual(ContentPermissions.ContentAccess.NotFound, result);
    }

    /// <summary>
    /// Tests that content access is denied when the user has no permissions by the content path.
    /// </summary>
    [Test]
    public void No_Access_By_Path()
    {
        // Arrange
        var user = CreateUser(9, 9876);
        var contentMock = new Mock<IContent>();
        contentMock.Setup(c => c.Path).Returns("-1,1234,5678");
        var content = contentMock.Object;
        var contentServiceMock = new Mock<IContentService>();
        contentServiceMock.Setup(x => x.GetById(1234)).Returns(content);
        var contentService = contentServiceMock.Object;
        var userServiceMock = new Mock<IUserService>();
        var permissions = new EntityPermissionCollection();
        var permissionSet = new EntityPermissionSet(1234, permissions);
        userServiceMock.Setup(x => x.GetPermissionsForPath(user, "-1,1234")).Returns(permissionSet);
        var userService = userServiceMock.Object;
        var entityServiceMock = new Mock<IEntityService>();
        entityServiceMock.Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
            .Returns(new[] { Mock.Of<TreeEntityPath>(entity => entity.Id == 9876 && entity.Path == "-1,9876") });
        var entityService = entityServiceMock.Object;
        var contentPermissions = new ContentPermissions(userService, contentService, entityService, AppCaches.Disabled);

        // Act
        var result = contentPermissions.CheckPermissions(1234, user, out IContent _, new[] { "F" }.ToHashSet());

        // Assert
        Assert.AreEqual(ContentPermissions.ContentAccess.Denied, result);
    }

    /// <summary>
    /// Tests that access is denied when the user does not have the required permission.
    /// </summary>
    [Test]
    public void No_Access_By_Permission()
    {
        // Arrange
        var user = CreateUser(9);
        var contentMock = new Mock<IContent>();
        contentMock.Setup(c => c.Path).Returns("-1,1234,5678");
        var content = contentMock.Object;
        var contentServiceMock = new Mock<IContentService>();
        contentServiceMock.Setup(x => x.GetById(1234)).Returns(content);
        var contentService = contentServiceMock.Object;
        var userServiceMock = new Mock<IUserService>();
        var permissions = new EntityPermissionCollection { new(9876, 1234, new[] { "A", "B", "C" }.ToHashSet()) };
        var permissionSet = new EntityPermissionSet(1234, permissions);
        userServiceMock.Setup(x => x.GetPermissionsForPath(user, "-1,1234,5678")).Returns(permissionSet);
        var userService = userServiceMock.Object;
        var entityServiceMock = new Mock<IEntityService>();
        var entityService = entityServiceMock.Object;
        var contentPermissions = new ContentPermissions(userService, contentService, entityService, AppCaches.Disabled);

        // Act
        var result = contentPermissions.CheckPermissions(1234, user, out IContent _, new[] { "F" }.ToHashSet());

        // Assert
        Assert.AreEqual(ContentPermissions.ContentAccess.Denied, result);
    }

    /// <summary>
    /// Tests that access is allowed when the user has the required permission.
    /// </summary>
    [Test]
    public void Access_Allowed_By_Permission()
    {
        // Arrange
        var user = CreateUser(9);
        var contentMock = new Mock<IContent>();
        contentMock.Setup(c => c.Path).Returns("-1,1234,5678");
        var content = contentMock.Object;
        var contentServiceMock = new Mock<IContentService>();
        contentServiceMock.Setup(x => x.GetById(1234)).Returns(content);
        var contentService = contentServiceMock.Object;
        var permissions = new EntityPermissionCollection { new(9876, 1234, new[] { "A", "F", "C" }.ToHashSet()) };
        var permissionSet = new EntityPermissionSet(1234, permissions);
        var userServiceMock = new Mock<IUserService>();
        userServiceMock.Setup(x => x.GetPermissionsForPath(user, "-1,1234,5678")).Returns(permissionSet);
        var userService = userServiceMock.Object;
        var entityServiceMock = new Mock<IEntityService>();
        var entityService = entityServiceMock.Object;
        var contentPermissions = new ContentPermissions(userService, contentService, entityService, AppCaches.Disabled);

        // Act
        var result = contentPermissions.CheckPermissions(1234, user, out IContent _, new[] { "F" }.ToHashSet());

        // Assert
        Assert.AreEqual(ContentPermissions.ContentAccess.Granted, result);
    }

    /// <summary>
    /// Tests that access to the root content item by path is granted.
    /// </summary>
    [Test]
    public void Access_To_Root_By_Path()
    {
        // Arrange
        var user = CreateUser();
        var contentServiceMock = new Mock<IContentService>();
        var contentService = contentServiceMock.Object;
        var userServiceMock = new Mock<IUserService>();
        var userService = userServiceMock.Object;
        var entityServiceMock = new Mock<IEntityService>();
        var entityService = entityServiceMock.Object;
        var contentPermissions = new ContentPermissions(userService, contentService, entityService, AppCaches.Disabled);

        // Act
        var result = contentPermissions.CheckPermissions(-1, user, out IContent _);

        // Assert
        Assert.AreEqual(ContentPermissions.ContentAccess.Granted, result);
    }

    /// <summary>
    /// Verifies that a user is granted access to the recycle bin when checking permissions by its path identifier.
    /// This test ensures that the <see cref="ContentPermissions.CheckPermissions"/> method returns <c>Granted</c> for the recycle bin node.
    /// </summary>
    [Test]
    public void Access_To_Recycle_Bin_By_Path()
    {
        // Arrange
        var user = CreateUser();
        var contentServiceMock = new Mock<IContentService>();
        var contentService = contentServiceMock.Object;
        var userServiceMock = new Mock<IUserService>();
        var userService = userServiceMock.Object;
        var entityServiceMock = new Mock<IEntityService>();
        var entityService = entityServiceMock.Object;
        var contentPermissions = new ContentPermissions(userService, contentService, entityService, AppCaches.Disabled);

        // Act
        var result = contentPermissions.CheckPermissions(-20, user, out IContent _);

        // Assert
        Assert.AreEqual(ContentPermissions.ContentAccess.Granted, result);
    }

    /// <summary>
    /// Tests that a user has no access to the recycle bin when checked by path.
    /// </summary>
    [Test]
    public void No_Access_To_Recycle_Bin_By_Path()
    {
        // Arrange
        var user = CreateUser(startContentId: 1234);
        var contentServiceMock = new Mock<IContentService>();
        var contentService = contentServiceMock.Object;
        var userServiceMock = new Mock<IUserService>();
        var userService = userServiceMock.Object;
        var entityServiceMock = new Mock<IEntityService>();
        entityServiceMock.Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
            .Returns(new[] { Mock.Of<TreeEntityPath>(entity => entity.Id == 1234 && entity.Path == "-1,1234") });
        var entityService = entityServiceMock.Object;
        var contentPermissions = new ContentPermissions(userService, contentService, entityService, AppCaches.Disabled);

        // Act
        var result = contentPermissions.CheckPermissions(-20, user, out IContent _);

        // Assert
        Assert.AreEqual(ContentPermissions.ContentAccess.Denied, result);
    }

    /// <summary>
    /// Tests that access to the root content item by path is denied.
    /// </summary>
    [Test]
    public void No_Access_To_Root_By_Path()
    {
        // Arrange
        var user = CreateUser(startContentId: 1234);

        var contentServiceMock = new Mock<IContentService>();
        var contentService = contentServiceMock.Object;
        var userServiceMock = new Mock<IUserService>();
        var userService = userServiceMock.Object;
        var entityServiceMock = new Mock<IEntityService>();
        entityServiceMock.Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
            .Returns(new[] { Mock.Of<TreeEntityPath>(entity => entity.Id == 1234 && entity.Path == "-1,1234") });
        var entityService = entityServiceMock.Object;
        var contentPermissions = new ContentPermissions(userService, contentService, entityService, AppCaches.Disabled);

        // Act
        var result = contentPermissions.CheckPermissions(-1, user, out IContent _);

        // Assert
        Assert.AreEqual(ContentPermissions.ContentAccess.Denied, result);
    }

    /// <summary>
    /// Tests that access to the root content node is granted based on the specified permission.
    /// </summary>
    [Test]
    public void Access_To_Root_By_Permission()
    {
        // Arrange
        var user = CreateUser();

        var userServiceMock = new Mock<IUserService>();
        var permissions = new EntityPermissionCollection { new(9876, 1234, new[] { "A" }.ToHashSet()) };
        var permissionSet = new EntityPermissionSet(1234, permissions);
        userServiceMock.Setup(x => x.GetPermissionsForPath(user, "-1")).Returns(permissionSet);
        var contentServiceMock = new Mock<IContentService>();
        var contentService = contentServiceMock.Object;
        var userService = userServiceMock.Object;
        var entityServiceMock = new Mock<IEntityService>();
        var entityService = entityServiceMock.Object;
        var contentPermissions = new ContentPermissions(userService, contentService, entityService, AppCaches.Disabled);

        // Act
        var result = contentPermissions.CheckPermissions(-1, user, out IContent _, new[] { "A" }.ToHashSet());

        // Assert
        Assert.AreEqual(ContentPermissions.ContentAccess.Granted, result);
    }

    /// <summary>
    /// Tests that access to the root content is denied when the user does not have the required permission.
    /// </summary>
    [Test]
    public void No_Access_To_Root_By_Permission()
    {
        // Arrange
        var user = CreateUser(withUserGroup: false);

        var userServiceMock = new Mock<IUserService>();
        var permissions = new EntityPermissionCollection { new(9876, 1234, new[] { "A" }.ToHashSet()) };
        var permissionSet = new EntityPermissionSet(1234, permissions);
        userServiceMock.Setup(x => x.GetPermissionsForPath(user, "-1")).Returns(permissionSet);
        var userService = userServiceMock.Object;
        var entityServiceMock = new Mock<IEntityService>();
        var entityService = entityServiceMock.Object;
        var contentServiceMock = new Mock<IContentService>();
        var contentService = contentServiceMock.Object;
        var contentPermissions = new ContentPermissions(userService, contentService, entityService, AppCaches.Disabled);

        // Act
        var result = contentPermissions.CheckPermissions(-1, user, out IContent _, new[] { "B" }.ToHashSet());

        // Assert
        Assert.AreEqual(ContentPermissions.ContentAccess.Denied, result);
    }

    /// <summary>
    /// Tests that access to the recycle bin is granted based on the appropriate permission.
    /// </summary>
    [Test]
    public void Access_To_Recycle_Bin_By_Permission()
    {
        // Arrange
        var user = CreateUser();

        var userServiceMock = new Mock<IUserService>();
        var permissions = new EntityPermissionCollection { new(9876, 1234, new[] { "A" }.ToHashSet()) };
        var permissionSet = new EntityPermissionSet(-20, permissions);

        userServiceMock.Setup(x => x.GetPermissionsForPath(user, "-20")).Returns(permissionSet);
        var userService = userServiceMock.Object;
        var entityServiceMock = new Mock<IEntityService>();
        var entityService = entityServiceMock.Object;
        var contentServiceMock = new Mock<IContentService>();
        var contentService = contentServiceMock.Object;
        var contentPermissions = new ContentPermissions(userService, contentService, entityService, AppCaches.Disabled);

        // Act
        var result = contentPermissions.CheckPermissions(-20, user, out IContent _, new[] { "A" }.ToHashSet());

        // Assert
        Assert.AreEqual(ContentPermissions.ContentAccess.Granted, result);
    }

    /// <summary>
    /// Tests that access to the recycle bin is denied when the user lacks the required permission.
    /// </summary>
    [Test]
    public void No_Access_To_Recycle_Bin_By_Permission()
    {
        // Arrange
        var user = CreateUser(withUserGroup: false);

        var userServiceMock = new Mock<IUserService>();
        var permissions = new EntityPermissionCollection { new(9876, 1234, new[] { "A" }.ToHashSet()) };
        var permissionSet = new EntityPermissionSet(1234, permissions);
        userServiceMock.Setup(x => x.GetPermissionsForPath(user, "-20")).Returns(permissionSet);
        var userService = userServiceMock.Object;
        var entityServiceMock = new Mock<IEntityService>();
        var entityService = entityServiceMock.Object;
        var contentServiceMock = new Mock<IContentService>();
        var contentService = contentServiceMock.Object;
        var contentPermissions = new ContentPermissions(userService, contentService, entityService, AppCaches.Disabled);

        // Act
        var result = contentPermissions.CheckPermissions(-20, user, out IContent _, new[] { "B" }.ToHashSet());

        // Assert
        Assert.AreEqual(ContentPermissions.ContentAccess.Denied, result);
    }

    private IUser CreateUser(int id = 0, int? startContentId = null, bool withUserGroup = true)
    {
        var builder = new UserBuilder()
            .WithId(id)
            .WithStartContentIds(startContentId.HasValue ? new[] { startContentId.Value } : new int[0]);
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
}
