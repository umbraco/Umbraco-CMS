// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Web.BackOffice.Authorization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.BackOffice.Authorization;

public class MediaPermissionsResourceHandlerTests
{
    private const int NodeId = 1000;

    [Test]
    public async Task Resource_With_Node_Id_With_Permission_Is_Authorized()
    {
        var authHandlerContext = CreateAuthorizationHandlerContext(NodeId, true);
        var sut = CreateHandler(NodeId);

        await sut.HandleAsync(authHandlerContext);

        Assert.IsTrue(authHandlerContext.HasSucceeded);
    }

    [Test]
    public async Task Resource_With_Media_With_Permission_Is_Authorized()
    {
        var authHandlerContext = CreateAuthorizationHandlerContext(NodeId);
        var sut = CreateHandler(NodeId);

        await sut.HandleAsync(authHandlerContext);

        Assert.IsTrue(authHandlerContext.HasSucceeded);
    }

    [Test]
    public async Task Resource_With_Node_Id_Withou_Permission_Is_Not_Authorized()
    {
        var authHandlerContext = CreateAuthorizationHandlerContext(NodeId, true);
        var sut = CreateHandler(NodeId, 1001);

        await sut.HandleAsync(authHandlerContext);

        Assert.IsFalse(authHandlerContext.HasSucceeded);
    }

    [Test]
    public async Task Resource_With_Media_Without_Permission_Is_Not_Authorized()
    {
        var authHandlerContext = CreateAuthorizationHandlerContext(NodeId);
        var sut = CreateHandler(NodeId, 1001);

        await sut.HandleAsync(authHandlerContext);

        Assert.IsFalse(authHandlerContext.HasSucceeded);
    }

    private static AuthorizationHandlerContext CreateAuthorizationHandlerContext(
        int nodeId,
        bool createWithNodeId = false)
    {
        var requirement = new MediaPermissionsResourceRequirement();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>()));
        var media = CreateMedia(nodeId);
        var resource = createWithNodeId
            ? new MediaPermissionsResource(nodeId)
            : new MediaPermissionsResource(media);
        return new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement }, user, resource);
    }

    private static IMedia CreateMedia(int nodeId)
    {
        var mediaType = MediaTypeBuilder.CreateSimpleMediaType("image", "Image");
        return MediaBuilder.CreateSimpleMedia(mediaType, "Test image", -1, nodeId);
    }

    private MediaPermissionsResourceHandler CreateHandler(int nodeId, int startMediaId = -1)
    {
        var mockBackOfficeSecurityAccessor = CreateMockBackOfficeSecurityAccessor(startMediaId);
        var contentPermissions = CreateMediaPermissions(nodeId);
        return new MediaPermissionsResourceHandler(mockBackOfficeSecurityAccessor.Object, contentPermissions);
    }

    private static Mock<IBackOfficeSecurityAccessor> CreateMockBackOfficeSecurityAccessor(int startMediaId)
    {
        var user = CreateUser(startMediaId);
        var mockBackOfficeSecurity = new Mock<IBackOfficeSecurity>();
        mockBackOfficeSecurity.SetupGet(x => x.CurrentUser).Returns(user);
        var mockBackOfficeSecurityAccessor = new Mock<IBackOfficeSecurityAccessor>();
        mockBackOfficeSecurityAccessor.Setup(x => x.BackOfficeSecurity).Returns(mockBackOfficeSecurity.Object);
        return mockBackOfficeSecurityAccessor;
    }

    private static User CreateUser(int startMediaId) =>
        new UserBuilder()
            .WithStartMediaId(startMediaId)
            .Build();

    private static MediaPermissions CreateMediaPermissions(int nodeId)
    {
        var mockMediaService = new Mock<IMediaService>();
        mockMediaService
            .Setup(x => x.GetById(It.Is<int>(y => y == nodeId)))
            .Returns(CreateMedia(nodeId));

        var mockEntityService = new Mock<IEntityService>();
        return new MediaPermissions(mockMediaService.Object, mockEntityService.Object, AppCaches.Disabled);
    }
}
