// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Web.BackOffice.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.BackOffice.Authorization;

public class ContentPermissionsResourceHandlerTests
{
    private const int NodeId = 1000;

    [Test]
    public async Task Resource_With_Node_Id_With_Permission_Is_Authorized()
    {
        var authHandlerContext = CreateAuthorizationHandlerContext(NodeId, true);
        var sut = CreateHandler(NodeId, new[] { "A" });

        await sut.HandleAsync(authHandlerContext);

        Assert.IsTrue(authHandlerContext.HasSucceeded);
    }

    [Test]
    public async Task Resource_With_Content_With_Permission_Is_Authorized()
    {
        var authHandlerContext = CreateAuthorizationHandlerContext(NodeId);
        var sut = CreateHandler(NodeId, new[] { "A" });

        await sut.HandleAsync(authHandlerContext);

        Assert.IsTrue(authHandlerContext.HasSucceeded);
    }

    [Test]
    public async Task Resource_With_Node_Id_Withou_Permission_Is_Not_Authorized()
    {
        var authHandlerContext = CreateAuthorizationHandlerContext(NodeId, true);
        var sut = CreateHandler(NodeId, new[] { "B" });

        await sut.HandleAsync(authHandlerContext);

        Assert.IsFalse(authHandlerContext.HasSucceeded);
    }

    [Test]
    public async Task Resource_With_Content_Without_Permission_Is_Not_Authorized()
    {
        var authHandlerContext = CreateAuthorizationHandlerContext(NodeId);
        var sut = CreateHandler(NodeId, new[] { "B" });

        await sut.HandleAsync(authHandlerContext);

        Assert.IsFalse(authHandlerContext.HasSucceeded);
    }

    private static AuthorizationHandlerContext CreateAuthorizationHandlerContext(
        int nodeId,
        bool createWithNodeId = false)
    {
        var requirement = new ContentPermissionsResourceRequirement();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>()));
        var content = CreateContent(nodeId);
        var permissions = new List<char> { 'A' }.AsReadOnly();
        var resource = createWithNodeId
            ? new ContentPermissionsResource(content, nodeId, permissions)
            : new ContentPermissionsResource(content, permissions);
        return new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement }, user, resource);
    }

    private static IContent CreateContent(int nodeId)
    {
        var contentType = ContentTypeBuilder.CreateBasicContentType();
        return ContentBuilder.CreateBasicContent(contentType, nodeId);
    }

    private ContentPermissionsResourceHandler CreateHandler(int nodeId, string[] permissionsForPath)
    {
        var mockBackOfficeSecurityAccessor = CreateMockBackOfficeSecurityAccessor();
        var contentPermissions = CreateContentPermissions(nodeId, permissionsForPath);
        return new ContentPermissionsResourceHandler(mockBackOfficeSecurityAccessor.Object, contentPermissions);
    }

    private static Mock<IBackOfficeSecurityAccessor> CreateMockBackOfficeSecurityAccessor()
    {
        var user = CreateUser();
        var mockBackOfficeSecurity = new Mock<IBackOfficeSecurity>();
        mockBackOfficeSecurity.SetupGet(x => x.CurrentUser).Returns(user);
        var mockBackOfficeSecurityAccessor = new Mock<IBackOfficeSecurityAccessor>();
        mockBackOfficeSecurityAccessor.Setup(x => x.BackOfficeSecurity).Returns(mockBackOfficeSecurity.Object);
        return mockBackOfficeSecurityAccessor;
    }

    private static User CreateUser() =>
        new UserBuilder()
            .Build();

    private static ContentPermissions CreateContentPermissions(int nodeId, string[] permissionsForPath)
    {
        var mockUserService = new Mock<IUserService>();

        mockUserService
            .Setup(x => x.GetPermissionsForPath(
                It.IsAny<IUser>(),
                It.Is<string>(y => y == $"{Constants.System.RootString},{nodeId.ToInvariantString()}")))
            .Returns(new EntityPermissionSet(
                nodeId,
                new EntityPermissionCollection(new List<EntityPermission> { new(1, nodeId, permissionsForPath) })));

        var mockContentService = new Mock<IContentService>();
        mockContentService
            .Setup(x => x.GetById(It.Is<int>(y => y == nodeId)))
            .Returns(CreateContent(nodeId));

        var mockEntityService = new Mock<IEntityService>();
        return new ContentPermissions(
            mockUserService.Object,
            mockContentService.Object,
            mockEntityService.Object,
            AppCaches.Disabled);
    }
}
