// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
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
public class MediaPermissionServiceTests
{
    [Test]
    public async Task Access_Allowed_By_Path()
    {
        // Arrange
        var user = CreateUser(9);
        var mediaKey = Guid.NewGuid();
        var entityServiceMock = new Mock<IEntityService>();
        entityServiceMock
            .Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.Is<Guid[]>(keys => keys.Contains(mediaKey))))
            .Returns([Mock.Of<TreeEntityPath>(entity => entity.Key == mediaKey && entity.Path == "-1,1234,5678")]);
        IMediaPermissionService sut = new MediaPermissionService(entityServiceMock.Object, AppCaches.Disabled);

        // Act
        var result = await sut.AuthorizeAccessAsync(user, mediaKey);

        // Assert
        Assert.AreEqual(MediaAuthorizationStatus.Success, result);
    }

    [Test]
    public async Task Returns_Not_Found_When_No_Media_Found()
    {
        // Arrange
        var user = CreateUser(9);
        var mediaKey = Guid.NewGuid();
        var entityServiceMock = new Mock<IEntityService>();
        entityServiceMock
            .Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<Guid[]>()))
            .Returns([]);
        IMediaPermissionService sut = new MediaPermissionService(entityServiceMock.Object, AppCaches.Disabled);

        // Act
        var result = await sut.AuthorizeAccessAsync(user, mediaKey);

        // Assert
        Assert.AreEqual(MediaAuthorizationStatus.NotFound, result);
    }

    [Test]
    public async Task No_Access_By_Path()
    {
        // Arrange
        var user = CreateUser(9, 9876);
        var mediaKey = Guid.NewGuid();
        var entityServiceMock = new Mock<IEntityService>();
        entityServiceMock
            .Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.Is<Guid[]>(keys => keys.Contains(mediaKey))))
            .Returns([Mock.Of<TreeEntityPath>(entity => entity.Key == mediaKey && entity.Path == "-1,1234,5678")]);
        entityServiceMock
            .Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
            .Returns([Mock.Of<TreeEntityPath>(entity => entity.Id == 9876 && entity.Path == "-1,9876")]);
        IMediaPermissionService sut = new MediaPermissionService(entityServiceMock.Object, AppCaches.Disabled);

        // Act
        var result = await sut.AuthorizeAccessAsync(user, mediaKey);

        // Assert
        Assert.AreEqual(MediaAuthorizationStatus.UnauthorizedMissingPathAccess, result);
    }

    [Test]
    public async Task Access_To_Root_By_Path()
    {
        // Arrange
        var user = CreateUser();
        var entityServiceMock = new Mock<IEntityService>();
        var sut = new MediaPermissionService(entityServiceMock.Object, AppCaches.Disabled);

        // Act
        var result = await sut.AuthorizeRootAccessAsync(user);

        // Assert
        Assert.AreEqual(MediaAuthorizationStatus.Success, result);
    }

    [Test]
    public async Task No_Access_To_Root_By_Path()
    {
        // Arrange
        var user = CreateUser(startMediaId: 1234);
        var entityServiceMock = new Mock<IEntityService>();
        entityServiceMock
            .Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
            .Returns([Mock.Of<TreeEntityPath>(entity => entity.Id == 1234 && entity.Path == "-1,1234")]);
        var sut = new MediaPermissionService(entityServiceMock.Object, AppCaches.Disabled);

        // Act
        var result = await sut.AuthorizeRootAccessAsync(user);

        // Assert
        Assert.AreEqual(MediaAuthorizationStatus.UnauthorizedMissingRootAccess, result);
    }

    [Test]
    public async Task Access_To_Recycle_Bin_By_Path()
    {
        // Arrange
        var user = CreateUser();
        var entityServiceMock = new Mock<IEntityService>();
        var sut = new MediaPermissionService(entityServiceMock.Object, AppCaches.Disabled);

        // Act
        var result = await sut.AuthorizeBinAccessAsync(user);

        // Assert
        Assert.AreEqual(MediaAuthorizationStatus.Success, result);
    }

    [Test]
    public async Task No_Access_To_Recycle_Bin_By_Path()
    {
        // Arrange
        var user = CreateUser(startMediaId: 1234);
        var entityServiceMock = new Mock<IEntityService>();
        entityServiceMock
            .Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
            .Returns([Mock.Of<TreeEntityPath>(entity => entity.Id == 1234 && entity.Path == "-1,1234")]);
        var sut = new MediaPermissionService(entityServiceMock.Object, AppCaches.Disabled);

        // Act
        var result = await sut.AuthorizeBinAccessAsync(user);

        // Assert
        Assert.AreEqual(MediaAuthorizationStatus.UnauthorizedMissingBinAccess, result);
    }

    [Test]
    public async Task Empty_Keys_Returns_Success()
    {
        // Arrange
        var user = CreateUser(9);
        var entityServiceMock = new Mock<IEntityService>();
        var sut = new MediaPermissionService(entityServiceMock.Object, AppCaches.Disabled);

        // Act
        var result = await sut.AuthorizeAccessAsync(user, []);

        // Assert
        Assert.AreEqual(MediaAuthorizationStatus.Success, result);
    }

    [Test]
    public async Task Multiple_Keys_All_Accessible_Returns_Success()
    {
        // Arrange
        var user = CreateUser(9);
        var keyA = Guid.NewGuid();
        var keyB = Guid.NewGuid();
        var entityServiceMock = new Mock<IEntityService>();
        entityServiceMock
            .Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<Guid[]>()))
            .Returns(
            [
                Mock.Of<TreeEntityPath>(entity => entity.Key == keyA && entity.Path == "-1,1234"),
                Mock.Of<TreeEntityPath>(entity => entity.Key == keyB && entity.Path == "-1,5678"),
            ]);
        var sut = new MediaPermissionService(entityServiceMock.Object, AppCaches.Disabled);

        // Act
        var result = await sut.AuthorizeAccessAsync(user, [keyA, keyB]);

        // Assert
        Assert.AreEqual(MediaAuthorizationStatus.Success, result);
    }

    [Test]
    public async Task Multiple_Keys_One_Inaccessible_Returns_Unauthorized()
    {
        // Arrange
        var user = CreateUser(9, 1234);
        var keyA = Guid.NewGuid();
        var keyB = Guid.NewGuid();
        var entityServiceMock = new Mock<IEntityService>();
        entityServiceMock
            .Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<Guid[]>()))
            .Returns(
            [
                Mock.Of<TreeEntityPath>(entity => entity.Key == keyA && entity.Path == "-1,1234,5678"),
                Mock.Of<TreeEntityPath>(entity => entity.Key == keyB && entity.Path == "-1,9999"),
            ]);
        entityServiceMock
            .Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
            .Returns([Mock.Of<TreeEntityPath>(entity => entity.Id == 1234 && entity.Path == "-1,1234")]);
        var sut = new MediaPermissionService(entityServiceMock.Object, AppCaches.Disabled);

        // Act
        var result = await sut.AuthorizeAccessAsync(user, [keyA, keyB]);

        // Assert
        Assert.AreEqual(MediaAuthorizationStatus.UnauthorizedMissingPathAccess, result);
    }

    [Test]
    public async Task Filter_Authorized_Access_Returns_Only_Accessible_Keys()
    {
        // Arrange
        var user = CreateUser(9, 1234);
        var keyA = Guid.NewGuid();
        var keyB = Guid.NewGuid();
        var entityServiceMock = new Mock<IEntityService>();
        entityServiceMock
            .Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<Guid[]>()))
            .Returns(
            [
                Mock.Of<TreeEntityPath>(entity => entity.Key == keyA && entity.Path == "-1,1234,5678"),
                Mock.Of<TreeEntityPath>(entity => entity.Key == keyB && entity.Path == "-1,9999"),
            ]);
        entityServiceMock
            .Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
            .Returns([Mock.Of<TreeEntityPath>(entity => entity.Id == 1234 && entity.Path == "-1,1234")]);
        var sut = new MediaPermissionService(entityServiceMock.Object, AppCaches.Disabled);

        // Act
        var result = await sut.FilterAuthorizedAccessAsync(user, [keyA, keyB]);

        // Assert
        CollectionAssert.AreEquivalent(new[] { keyA }, result);
    }

    [Test]
    public async Task Filter_Authorized_Access_Empty_Keys_Returns_Empty()
    {
        // Arrange
        var user = CreateUser(9);
        var entityServiceMock = new Mock<IEntityService>();
        var sut = new MediaPermissionService(entityServiceMock.Object, AppCaches.Disabled);

        // Act
        var result = await sut.FilterAuthorizedAccessAsync(user, []);

        // Assert
        Assert.IsEmpty(result);
    }

    private static IUser CreateUser(int id = 0, int? startMediaId = null) =>
        new UserBuilder()
            .WithId(id)
            .WithStartMediaIds(startMediaId.HasValue ? [startMediaId.Value] : [])
            .AddUserGroup()
                .WithId(1)
                .WithName("admin")
                .WithAlias("admin")
                .Done()
            .Build();
}
