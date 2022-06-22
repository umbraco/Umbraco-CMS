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
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.BackOffice.Authorization
{
    public class ContentPermissionsQueryStringHandlerTests
    {
        private const string QueryStringName = "id";
        private const int NodeId = 1000;
        private static readonly Guid s_nodeGuid = Guid.NewGuid();
        private static readonly Udi s_nodeUdi = UdiParser.Parse($"umb://document/{s_nodeGuid.ToString().ToLowerInvariant().Replace("-", string.Empty)}");

        [Test]
        public async Task Node_Id_From_Requirement_With_Permission_Is_Authorized()
        {
            AuthorizationHandlerContext authHandlerContext = CreateAuthorizationHandlerContext(NodeId);
            Mock<IHttpContextAccessor> mockHttpContextAccessor = CreateMockHttpContextAccessor();
            ContentPermissionsQueryStringHandler sut = CreateHandler(mockHttpContextAccessor.Object, NodeId, new string[] { "A" });

            await sut.HandleAsync(authHandlerContext);

            Assert.IsTrue(authHandlerContext.HasSucceeded);
        }

        [Test]
        public async Task Node_Id_From_Requirement_Without_Permission_Is_Not_Authorized()
        {
            AuthorizationHandlerContext authHandlerContext = CreateAuthorizationHandlerContext(NodeId);
            Mock<IHttpContextAccessor> mockHttpContextAccessor = CreateMockHttpContextAccessor();
            ContentPermissionsQueryStringHandler sut = CreateHandler(mockHttpContextAccessor.Object, NodeId, new string[] { "B" });

            await sut.HandleAsync(authHandlerContext);

            Assert.IsFalse(authHandlerContext.HasSucceeded);
            AssertContentCached(mockHttpContextAccessor);
        }

        [Test]
        public async Task Node_Id_Missing_From_Requirement_And_QueryString_Is_Authorized()
        {
            AuthorizationHandlerContext authHandlerContext = CreateAuthorizationHandlerContext();
            Mock<IHttpContextAccessor> mockHttpContextAccessor = CreateMockHttpContextAccessor(queryStringName: "xxx");
            ContentPermissionsQueryStringHandler sut = CreateHandler(mockHttpContextAccessor.Object, NodeId, new string[] { "A" });

            await sut.HandleAsync(authHandlerContext);

            Assert.IsTrue(authHandlerContext.HasSucceeded);
        }

        [Test]
        public async Task Node_Integer_Id_From_QueryString_With_Permission_Is_Authorized()
        {
            AuthorizationHandlerContext authHandlerContext = CreateAuthorizationHandlerContext();
            Mock<IHttpContextAccessor> mockHttpContextAccessor = CreateMockHttpContextAccessor(queryStringValue: NodeId.ToString());
            ContentPermissionsQueryStringHandler sut = CreateHandler(mockHttpContextAccessor.Object, NodeId, new string[] { "A" });

            await sut.HandleAsync(authHandlerContext);

            Assert.IsTrue(authHandlerContext.HasSucceeded);
            AssertContentCached(mockHttpContextAccessor);
        }

        [Test]
        public async Task Node_Integer_Id_From_QueryString_Without_Permission_Is_Not_Authorized()
        {
            AuthorizationHandlerContext authHandlerContext = CreateAuthorizationHandlerContext();
            Mock<IHttpContextAccessor> mockHttpContextAccessor = CreateMockHttpContextAccessor(queryStringValue: NodeId.ToString());
            ContentPermissionsQueryStringHandler sut = CreateHandler(mockHttpContextAccessor.Object, NodeId, new string[] { "B" });

            await sut.HandleAsync(authHandlerContext);

            Assert.IsFalse(authHandlerContext.HasSucceeded);
            AssertContentCached(mockHttpContextAccessor);
        }

        [Test]
        public async Task Node_Udi_Id_From_QueryString_With_Permission_Is_Authorized()
        {
            AuthorizationHandlerContext authHandlerContext = CreateAuthorizationHandlerContext();
            Mock<IHttpContextAccessor> mockHttpContextAccessor = CreateMockHttpContextAccessor(queryStringValue: s_nodeUdi.ToString());
            ContentPermissionsQueryStringHandler sut = CreateHandler(mockHttpContextAccessor.Object, NodeId, new string[] { "A" });

            await sut.HandleAsync(authHandlerContext);

            Assert.IsTrue(authHandlerContext.HasSucceeded);
            AssertContentCached(mockHttpContextAccessor);
        }

        [Test]
        public async Task Node_Udi_Id_From_QueryString_Without_Permission_Is_Not_Authorized()
        {
            AuthorizationHandlerContext authHandlerContext = CreateAuthorizationHandlerContext();
            Mock<IHttpContextAccessor> mockHttpContextAccessor = CreateMockHttpContextAccessor(queryStringValue: s_nodeUdi.ToString());
            ContentPermissionsQueryStringHandler sut = CreateHandler(mockHttpContextAccessor.Object, NodeId, new string[] { "B" });

            await sut.HandleAsync(authHandlerContext);

            Assert.IsFalse(authHandlerContext.HasSucceeded);
            AssertContentCached(mockHttpContextAccessor);
        }

        [Test]
        public async Task Node_Guid_Id_From_QueryString_With_Permission_Is_Authorized()
        {
            AuthorizationHandlerContext authHandlerContext = CreateAuthorizationHandlerContext();
            Mock<IHttpContextAccessor> mockHttpContextAccessor = CreateMockHttpContextAccessor(queryStringValue: s_nodeGuid.ToString());
            ContentPermissionsQueryStringHandler sut = CreateHandler(mockHttpContextAccessor.Object, NodeId, new string[] { "A" });

            await sut.HandleAsync(authHandlerContext);

            Assert.IsTrue(authHandlerContext.HasSucceeded);
            AssertContentCached(mockHttpContextAccessor);
        }

        [Test]
        public async Task Node_Guid_Id_From_QueryString_Without_Permission_Is_Not_Authorized()
        {
            AuthorizationHandlerContext authHandlerContext = CreateAuthorizationHandlerContext();
            Mock<IHttpContextAccessor> mockHttpContextAccessor = CreateMockHttpContextAccessor(queryStringValue: s_nodeGuid.ToString());
            ContentPermissionsQueryStringHandler sut = CreateHandler(mockHttpContextAccessor.Object, NodeId, new string[] { "B" });

            await sut.HandleAsync(authHandlerContext);

            Assert.IsFalse(authHandlerContext.HasSucceeded);
            AssertContentCached(mockHttpContextAccessor);
        }

        [Test]
        public async Task Node_Invalid_Id_From_QueryString_Is_Authorized()
        {
            AuthorizationHandlerContext authHandlerContext = CreateAuthorizationHandlerContext();
            Mock<IHttpContextAccessor> mockHttpContextAccessor = CreateMockHttpContextAccessor(queryStringValue: "invalid");
            ContentPermissionsQueryStringHandler sut = CreateHandler(mockHttpContextAccessor.Object, NodeId, new string[] { "A" });

            await sut.HandleAsync(authHandlerContext);

            Assert.IsTrue(authHandlerContext.HasSucceeded);
        }

        private static AuthorizationHandlerContext CreateAuthorizationHandlerContext(int? nodeId = null)
        {
            const char Permission = 'A';
            ContentPermissionsQueryStringRequirement requirement = nodeId.HasValue
                ? new ContentPermissionsQueryStringRequirement(nodeId.Value, Permission)
                : new ContentPermissionsQueryStringRequirement(Permission, QueryStringName);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>()));
            object resource = new object();
            return new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement }, user, resource);
        }

        private static Mock<IHttpContextAccessor> CreateMockHttpContextAccessor(string queryStringName = QueryStringName, string queryStringValue = "")
        {
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var mockHttpContext = new Mock<HttpContext>();
            var mockHttpRequest = new Mock<HttpRequest>();
            var queryParams = new Dictionary<string, StringValues>
            {
                { queryStringName, queryStringValue },
            };
            mockHttpRequest.SetupGet(x => x.Query).Returns(new QueryCollection(queryParams));
            mockHttpContext.SetupGet(x => x.Request).Returns(mockHttpRequest.Object);
            mockHttpContext.SetupGet(x => x.Items).Returns(new Dictionary<object, object>());
            mockHttpContextAccessor.SetupGet(x => x.HttpContext).Returns(mockHttpContext.Object);
            return mockHttpContextAccessor;
        }

        private ContentPermissionsQueryStringHandler CreateHandler(IHttpContextAccessor httpContextAccessor, int nodeId, string[] permissionsForPath)
        {
            Mock<IBackOfficeSecurityAccessor> mockBackOfficeSecurityAccessor = CreateMockBackOfficeSecurityAccessor();
            Mock<IEntityService> mockEntityService = CreateMockEntityService();
            ContentPermissions contentPermissions = CreateContentPermissions(mockEntityService.Object, nodeId, permissionsForPath);
            return new ContentPermissionsQueryStringHandler(mockBackOfficeSecurityAccessor.Object, httpContextAccessor, mockEntityService.Object, contentPermissions);
        }

        private static Mock<IEntityService> CreateMockEntityService()
        {
            var mockEntityService = new Mock<IEntityService>();
            mockEntityService
                .Setup(x => x.GetId(It.Is<Udi>(y => y == s_nodeUdi)))
                .Returns(Attempt<int>.Succeed(NodeId));
            mockEntityService
                .Setup(x => x.GetId(It.Is<Guid>(y => y == s_nodeGuid), It.Is<UmbracoObjectTypes>(y => y == UmbracoObjectTypes.Document)))
                .Returns(Attempt<int>.Succeed(NodeId));
            return mockEntityService;
        }

        private static Mock<IBackOfficeSecurityAccessor> CreateMockBackOfficeSecurityAccessor()
        {
            User user = CreateUser();
            var mockBackOfficeSecurity = new Mock<IBackOfficeSecurity>();
            mockBackOfficeSecurity.SetupGet(x => x.CurrentUser).Returns(user);
            var mockBackOfficeSecurityAccessor = new Mock<IBackOfficeSecurityAccessor>();
            mockBackOfficeSecurityAccessor.Setup(x => x.BackOfficeSecurity).Returns(mockBackOfficeSecurity.Object);
            return mockBackOfficeSecurityAccessor;
        }

        private static User CreateUser() =>
            new UserBuilder()
                .Build();

        private static ContentPermissions CreateContentPermissions(IEntityService entityService, int nodeId, string[] permissionsForPath)
        {
            var mockUserService = new Mock<IUserService>();

            mockUserService
                .Setup(x => x.GetPermissionsForPath(It.IsAny<IUser>(), It.Is<string>(y => y == $"{Constants.System.RootString},{nodeId.ToInvariantString()}")))
                .Returns(new EntityPermissionSet(nodeId, new EntityPermissionCollection(new List<EntityPermission> { new EntityPermission(1, nodeId, permissionsForPath) })));

            var mockContentService = new Mock<IContentService>();
            mockContentService
                .Setup(x => x.GetById(It.Is<int>(y => y == nodeId)))
                .Returns(CreateContent(nodeId));

            return new ContentPermissions(mockUserService.Object, mockContentService.Object, entityService, AppCaches.Disabled);
        }

        private static IContent CreateContent(int nodeId)
        {
            ContentType contentType = ContentTypeBuilder.CreateBasicContentType();
            return ContentBuilder.CreateBasicContent(contentType, nodeId);
        }

        private static void AssertContentCached(Mock<IHttpContextAccessor> mockHttpContextAccessor) =>
            Assert.AreEqual(NodeId, ((IContent)mockHttpContextAccessor.Object.HttpContext.Items[typeof(IContent).ToString()]).Id);
    }
}
