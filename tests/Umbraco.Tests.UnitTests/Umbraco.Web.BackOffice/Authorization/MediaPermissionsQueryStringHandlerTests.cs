// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
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

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.BackOffice.Authorization;

public class MediaPermissionsQueryStringHandlerTests
{
    private const string QueryStringName = "id";
    private const int NodeId = 1000;
    private static readonly Guid s_nodeGuid = Guid.NewGuid();

    private static readonly Udi s_nodeUdi =
        UdiParser.Parse($"umb://document/{s_nodeGuid.ToString().ToLowerInvariant().Replace("-", string.Empty)}");

    [Test]
    public async Task Node_Id_Missing_From_QueryString_Is_Authorized()
    {
        var authHandlerContext = CreateAuthorizationHandlerContext();
        var mockHttpContextAccessor = CreateMockHttpContextAccessor("xxx");
        var sut = CreateHandler(mockHttpContextAccessor.Object, NodeId);

        await sut.HandleAsync(authHandlerContext);

        Assert.IsTrue(authHandlerContext.HasSucceeded);
    }

    [Test]
    public async Task Node_Integer_Id_From_QueryString_With_Permission_Is_Authorized()
    {
        var authHandlerContext = CreateAuthorizationHandlerContext();
        var mockHttpContextAccessor = CreateMockHttpContextAccessor(queryStringValue: NodeId.ToString());
        var sut = CreateHandler(mockHttpContextAccessor.Object, NodeId);

        await sut.HandleAsync(authHandlerContext);

        Assert.IsTrue(authHandlerContext.HasSucceeded);
        AssertMediaCached(mockHttpContextAccessor);
    }

    [Test]
    public async Task Node_Integer_Id_From_QueryString_Without_Permission_Is_Not_Authorized()
    {
        var authHandlerContext = CreateAuthorizationHandlerContext();
        var mockHttpContextAccessor = CreateMockHttpContextAccessor(queryStringValue: NodeId.ToString());
        var sut = CreateHandler(mockHttpContextAccessor.Object, NodeId, 1001);

        await sut.HandleAsync(authHandlerContext);

        Assert.IsFalse(authHandlerContext.HasSucceeded);
        AssertMediaCached(mockHttpContextAccessor);
    }

    [Test]
    public async Task Node_Udi_Id_From_QueryString_With_Permission_Is_Authorized()
    {
        var authHandlerContext = CreateAuthorizationHandlerContext();
        var mockHttpContextAccessor = CreateMockHttpContextAccessor(queryStringValue: s_nodeUdi.ToString());
        var sut = CreateHandler(mockHttpContextAccessor.Object, NodeId);

        await sut.HandleAsync(authHandlerContext);

        Assert.IsTrue(authHandlerContext.HasSucceeded);
        AssertMediaCached(mockHttpContextAccessor);
    }

    [Test]
    public async Task Node_Udi_Id_From_QueryString_Without_Permission_Is_Not_Authorized()
    {
        var authHandlerContext = CreateAuthorizationHandlerContext();
        var mockHttpContextAccessor = CreateMockHttpContextAccessor(queryStringValue: s_nodeUdi.ToString());
        var sut = CreateHandler(mockHttpContextAccessor.Object, NodeId, 1001);

        await sut.HandleAsync(authHandlerContext);

        Assert.IsFalse(authHandlerContext.HasSucceeded);
        AssertMediaCached(mockHttpContextAccessor);
    }

    [Test]
    public async Task Node_Guid_Id_From_QueryString_With_Permission_Is_Authorized()
    {
        var authHandlerContext = CreateAuthorizationHandlerContext();
        var mockHttpContextAccessor = CreateMockHttpContextAccessor(queryStringValue: s_nodeGuid.ToString());
        var sut = CreateHandler(mockHttpContextAccessor.Object, NodeId);

        await sut.HandleAsync(authHandlerContext);

        Assert.IsTrue(authHandlerContext.HasSucceeded);
        AssertMediaCached(mockHttpContextAccessor);
    }

    [Test]
    public async Task Node_Guid_Id_From_QueryString_Without_Permission_Is_Not_Authorized()
    {
        var authHandlerContext = CreateAuthorizationHandlerContext();
        var mockHttpContextAccessor = CreateMockHttpContextAccessor(queryStringValue: s_nodeGuid.ToString());
        var sut = CreateHandler(mockHttpContextAccessor.Object, NodeId, 1001);

        await sut.HandleAsync(authHandlerContext);

        Assert.IsFalse(authHandlerContext.HasSucceeded);
        AssertMediaCached(mockHttpContextAccessor);
    }

    [Test]
    public async Task Node_Invalid_Id_From_QueryString_Is_Authorized()
    {
        var authHandlerContext = CreateAuthorizationHandlerContext();
        var mockHttpContextAccessor = CreateMockHttpContextAccessor(queryStringValue: "invalid");
        var sut = CreateHandler(mockHttpContextAccessor.Object, NodeId);

        await sut.HandleAsync(authHandlerContext);

        Assert.IsTrue(authHandlerContext.HasSucceeded);
    }

    private static AuthorizationHandlerContext CreateAuthorizationHandlerContext()
    {
        var requirement = new MediaPermissionsQueryStringRequirement(QueryStringName);
        var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>()));
        var resource = new object();
        return new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement }, user, resource);
    }

    private static Mock<IHttpContextAccessor> CreateMockHttpContextAccessor(
        string queryStringName = QueryStringName,
        string queryStringValue = "")
    {
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var mockHttpContext = new Mock<HttpContext>();
        var mockHttpRequest = new Mock<HttpRequest>();
        var queryParams = new Dictionary<string, StringValues> { { queryStringName, queryStringValue } };
        mockHttpRequest.SetupGet(x => x.Query).Returns(new QueryCollection(queryParams));
        mockHttpContext.SetupGet(x => x.Request).Returns(mockHttpRequest.Object);
        mockHttpContext.SetupGet(x => x.Items).Returns(new Dictionary<object, object>());
        mockHttpContextAccessor.SetupGet(x => x.HttpContext).Returns(mockHttpContext.Object);
        return mockHttpContextAccessor;
    }

    private MediaPermissionsQueryStringHandler CreateHandler(
        IHttpContextAccessor httpContextAccessor,
        int nodeId,
        int startMediaId = -1)
    {
        var mockBackOfficeSecurityAccessor = CreateMockBackOfficeSecurityAccessor(startMediaId);
        var mockEntityService = CreateMockEntityService();
        var mediaPermissions = CreateMediaPermissions(mockEntityService.Object, nodeId);
        return new MediaPermissionsQueryStringHandler(
            mockBackOfficeSecurityAccessor.Object,
            httpContextAccessor,
            mockEntityService.Object,
            mediaPermissions);
    }

    private static Mock<IEntityService> CreateMockEntityService()
    {
        var mockEntityService = new Mock<IEntityService>();
        mockEntityService
            .Setup(x => x.GetId(It.Is<Udi>(y => y == s_nodeUdi)))
            .Returns(Attempt<int>.Succeed(NodeId));
        mockEntityService
            .Setup(x => x.GetId(
                It.Is<Guid>(y => y == s_nodeGuid),
                It.Is<UmbracoObjectTypes>(y => y == UmbracoObjectTypes.Document)))
            .Returns(Attempt<int>.Succeed(NodeId));
        return mockEntityService;
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

    private static MediaPermissions CreateMediaPermissions(IEntityService entityService, int nodeId)
    {
        var mockMediaService = new Mock<IMediaService>();
        mockMediaService
            .Setup(x => x.GetById(It.Is<int>(y => y == nodeId)))
            .Returns(CreateMedia(nodeId));

        return new MediaPermissions(mockMediaService.Object, entityService, AppCaches.Disabled);
    }

    private static IMedia CreateMedia(int nodeId)
    {
        var mediaType = MediaTypeBuilder.CreateSimpleMediaType("image", "Image");
        return MediaBuilder.CreateSimpleMedia(mediaType, "Test image", -1, nodeId);
    }

    private static void AssertMediaCached(Mock<IHttpContextAccessor> mockHttpContextAccessor) =>
        Assert.AreEqual(
            NodeId,
            ((IMedia)mockHttpContextAccessor.Object.HttpContext.Items[typeof(IMedia).ToString()]).Id);
}
