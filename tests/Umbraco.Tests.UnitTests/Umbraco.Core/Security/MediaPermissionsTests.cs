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
/// Contains unit tests for verifying media permissions functionality.
/// </summary>
[TestFixture]
public class MediaPermissionsTests
{
    /// <summary>
    /// Tests that access is allowed for a user based on the media path.
    /// </summary>
    [Test]
    public void Access_Allowed_By_Path()
    {
        // Arrange
        var user = CreateUser(9);
        var mediaMock = new Mock<IMedia>();
        mediaMock.Setup(m => m.Path).Returns("-1,1234,5678");
        var media = mediaMock.Object;
        var mediaServiceMock = new Mock<IMediaService>();
        mediaServiceMock.Setup(x => x.GetById(1234)).Returns(media);
        var mediaService = mediaServiceMock.Object;
        var entityServiceMock = new Mock<IEntityService>();
        var entityService = entityServiceMock.Object;
        var mediaPermissions = new MediaPermissions(mediaService, entityService, AppCaches.Disabled);

        // Act
        var result = mediaPermissions.CheckPermissions(user, 1234, out _);

        // Assert
        Assert.AreEqual(MediaPermissions.MediaAccess.Granted, result);
    }

    /// <summary>
    /// Tests that the CheckPermissions method returns NotFound when no media is found.
    /// </summary>
    [Test]
    public void Returns_Not_Found_When_No_Media_Found()
    {
        // Arrange
        var user = CreateUser(9);
        var mediaMock = new Mock<IMedia>();
        mediaMock.Setup(m => m.Path).Returns("-1,1234,5678");
        var media = mediaMock.Object;
        var mediaServiceMock = new Mock<IMediaService>();
        mediaServiceMock.Setup(x => x.GetById(0)).Returns(media);
        var mediaService = mediaServiceMock.Object;
        var entityServiceMock = new Mock<IEntityService>();
        var entityService = entityServiceMock.Object;
        var mediaPermissions = new MediaPermissions(mediaService, entityService, AppCaches.Disabled);

        // Act/assert
        var result = mediaPermissions.CheckPermissions(user, 1234, out _);
        Assert.AreEqual(MediaPermissions.MediaAccess.NotFound, result);
    }

    /// <summary>
    /// Tests that access is denied when the user does not have permission by media path.
    /// </summary>
    [Test]
    public void No_Access_By_Path()
    {
        // Arrange
        var user = CreateUser(9, 9876);
        var mediaMock = new Mock<IMedia>();
        mediaMock.Setup(m => m.Path).Returns("-1,1234,5678");
        var media = mediaMock.Object;
        var mediaServiceMock = new Mock<IMediaService>();
        mediaServiceMock.Setup(x => x.GetById(1234)).Returns(media);
        var mediaService = mediaServiceMock.Object;
        var entityServiceMock = new Mock<IEntityService>();
        entityServiceMock.Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
            .Returns(new[] { Mock.Of<TreeEntityPath>(entity => entity.Id == 9876 && entity.Path == "-1,9876") });
        var entityService = entityServiceMock.Object;
        var mediaPermissions = new MediaPermissions(mediaService, entityService, AppCaches.Disabled);

        // Act
        var result = mediaPermissions.CheckPermissions(user, 1234, out _);

        // Assert
        Assert.AreEqual(MediaPermissions.MediaAccess.Denied, result);
    }

    /// <summary>
    /// Tests that access to the root media item by path is granted.
    /// </summary>
    [Test]
    public void Access_To_Root_By_Path()
    {
        // Arrange
        var user = CreateUser();
        var mediaServiceMock = new Mock<IMediaService>();
        var mediaService = mediaServiceMock.Object;
        var entityServiceMock = new Mock<IEntityService>();
        var entityService = entityServiceMock.Object;
        var mediaPermissions = new MediaPermissions(mediaService, entityService, AppCaches.Disabled);

        // Act
        var result = mediaPermissions.CheckPermissions(user, -1, out _);

        // Assert
        Assert.AreEqual(MediaPermissions.MediaAccess.Granted, result);
    }

    /// <summary>
    /// Tests that a user has no access to the root media item when checked by path.
    /// </summary>
    [Test]
    public void No_Access_To_Root_By_Path()
    {
        // Arrange
        var user = CreateUser(startMediaId: 1234);
        var mediaServiceMock = new Mock<IMediaService>();
        var mediaService = mediaServiceMock.Object;
        var entityServiceMock = new Mock<IEntityService>();
        entityServiceMock.Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
            .Returns(new[] { Mock.Of<TreeEntityPath>(entity => entity.Id == 1234 && entity.Path == "-1,1234") });
        var entityService = entityServiceMock.Object;
        var mediaPermissions = new MediaPermissions(mediaService, entityService, AppCaches.Disabled);

        // Act
        var result = mediaPermissions.CheckPermissions(user, -1, out _);

        // Assert
        Assert.AreEqual(MediaPermissions.MediaAccess.Denied, result);
    }

    /// <summary>
    /// Tests that access to the Recycle Bin by path is granted.
    /// </summary>
    [Test]
    public void Access_To_Recycle_Bin_By_Path()
    {
        // Arrange
        var user = CreateUser();
        var mediaServiceMock = new Mock<IMediaService>();
        var mediaService = mediaServiceMock.Object;
        var entityServiceMock = new Mock<IEntityService>();
        var entityService = entityServiceMock.Object;
        var mediaPermissions = new MediaPermissions(mediaService, entityService, AppCaches.Disabled);

        // Act
        var result = mediaPermissions.CheckPermissions(user, -21, out _);

        // Assert
        Assert.AreEqual(MediaPermissions.MediaAccess.Granted, result);
    }

    /// <summary>
    /// Tests that a user has no access to the recycle bin when checked by path.
    /// </summary>
    [Test]
    public void No_Access_To_Recycle_Bin_By_Path()
    {
        // Arrange
        var user = CreateUser(startMediaId: 1234);
        var mediaServiceMock = new Mock<IMediaService>();
        var mediaService = mediaServiceMock.Object;
        var entityServiceMock = new Mock<IEntityService>();
        entityServiceMock.Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
            .Returns(new[] { Mock.Of<TreeEntityPath>(entity => entity.Id == 1234 && entity.Path == "-1,1234") });
        var entityService = entityServiceMock.Object;
        var mediaPermissions = new MediaPermissions(mediaService, entityService, AppCaches.Disabled);

        // Act
        var result = mediaPermissions.CheckPermissions(user, -21, out _);

        // Assert
        Assert.AreEqual(MediaPermissions.MediaAccess.Denied, result);
    }

    private IUser CreateUser(int id = 0, int? startMediaId = null) =>
        new UserBuilder()
            .WithId(id)
            .WithStartMediaIds(startMediaId.HasValue ? new[] { startMediaId.Value } : new int[0])
            .AddUserGroup()
            .WithId(1)
            .WithName("admin")
            .WithAlias("admin")
            .Done()
            .Build();
}
