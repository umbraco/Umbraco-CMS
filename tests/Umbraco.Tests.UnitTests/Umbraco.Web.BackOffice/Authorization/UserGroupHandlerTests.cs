// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Web.BackOffice.Authorization;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.BackOffice.Authorization
{
    public class UserGroupHandlerTests
    {
        private const string QueryStringName = "id";

        private const int Group1Id = 1;
        private const string Group1Alias = "group1";
        private const int Group2Id = 2;
        private const string Group2Alias = "group2";
        private const int Group3Id = 3;
        private const string Group3Alias = "group3";

        [Test]
        public async Task Missing_QueryString_Value_Is_Authorized()
        {
            AuthorizationHandlerContext authHandlerContext = CreateAuthorizationHandlerContext();
            UserGroupHandler sut = CreateHandler();

            await sut.HandleAsync(authHandlerContext);

            Assert.IsTrue(authHandlerContext.HasSucceeded);
        }

        [Test]
        public async Task Admin_User_Is_Authorised()
        {
            AuthorizationHandlerContext authHandlerContext = CreateAuthorizationHandlerContext();
            UserGroupHandler sut = CreateHandler(queryStringValue: Group1Id.ToString(), userIsAdmin: true);

            await sut.HandleAsync(authHandlerContext);

            Assert.IsTrue(authHandlerContext.HasSucceeded);
        }

        [Test]
        public async Task User_Matching_Single_Requested_Group_Id_Is_Authorised()
        {
            AuthorizationHandlerContext authHandlerContext = CreateAuthorizationHandlerContext();
            UserGroupHandler sut = CreateHandler(queryStringValue: Group1Id.ToString());

            await sut.HandleAsync(authHandlerContext);

            Assert.IsTrue(authHandlerContext.HasSucceeded);
        }

        [Test]
        public async Task User_Matching_Only_One_Of_Requested_Group_Ids_Is_NOT_Authorised()
        {
            AuthorizationHandlerContext authHandlerContext = CreateAuthorizationHandlerContext();
            UserGroupHandler sut = CreateHandler(queryStringValue: $"{Group1Id},{Group2Id}");

            await sut.HandleAsync(authHandlerContext);

            Assert.IsTrue(authHandlerContext.HasFailed);
        }

        [Test]
        public async Task User_Not_Matching_Single_Requested_Group_Id_Is_Not_Authorised()
        {
            AuthorizationHandlerContext authHandlerContext = CreateAuthorizationHandlerContext();
            UserGroupHandler sut = CreateHandler(queryStringValue: Group2Id.ToString());

            await sut.HandleAsync(authHandlerContext);

            Assert.IsFalse(authHandlerContext.HasSucceeded);
        }

        [Test]
        public async Task User_Not_Matching_Any_Of_Requested_Group_Ids_Is_Not_Authorised()
        {
            AuthorizationHandlerContext authHandlerContext = CreateAuthorizationHandlerContext();
            UserGroupHandler sut = CreateHandler(queryStringValue: $"{Group2Id},{Group3Id}");

            await sut.HandleAsync(authHandlerContext);

            Assert.IsFalse(authHandlerContext.HasSucceeded);
        }

        private static AuthorizationHandlerContext CreateAuthorizationHandlerContext()
        {
            var requirement = new UserGroupRequirement(QueryStringName);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>()));
            object resource = new object();
            return new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement }, user, resource);
        }

        private UserGroupHandler CreateHandler(string queryStringValue = "", bool userIsAdmin = false)
        {
            Mock<IHttpContextAccessor> mockHttpContextAccessor = CreateMockHttpContextAccessor(queryStringValue);

            Mock<IUserService> mockUserService = CreateMockUserService();

            var mockContentService = new Mock<IContentService>();
            var mockMediaService = new Mock<IMediaService>();
            var mockEntityService = new Mock<IEntityService>();

            Mock<IBackOfficeSecurityAccessor> mockBackOfficeSecurityAccessor = CreateMockBackOfficeSecurityAccessor(userIsAdmin);

            return new UserGroupHandler(mockHttpContextAccessor.Object, mockUserService.Object, mockContentService.Object, mockMediaService.Object, mockEntityService.Object, mockBackOfficeSecurityAccessor.Object, AppCaches.Disabled);
        }

        private static Mock<IHttpContextAccessor> CreateMockHttpContextAccessor(string queryStringValue)
        {
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var mockHttpContext = new Mock<HttpContext>();
            var mockHttpRequest = new Mock<HttpRequest>();
            var queryParams = new Dictionary<string, StringValues>
            {
                { QueryStringName, queryStringValue },
            };
            mockHttpRequest.SetupGet(x => x.Query).Returns(new QueryCollection(queryParams));
            mockHttpContext.SetupGet(x => x.Request).Returns(mockHttpRequest.Object);
            mockHttpContextAccessor.SetupGet(x => x.HttpContext).Returns(mockHttpContext.Object);
            return mockHttpContextAccessor;
        }

        private Mock<IUserService> CreateMockUserService()
        {
            var mockUserService = new Mock<IUserService>();
            mockUserService
                .Setup(x => x.GetAllUserGroups(It.Is<int[]>(y => y.Length == 1 && y[0] == Group1Id)))
                .Returns(new List<IUserGroup> { CreateUserGroup(Group1Id, Group1Alias) });
            mockUserService
                .Setup(x => x.GetAllUserGroups(It.Is<int[]>(y => y.Length == 1 && y[0] == Group2Id)))
                .Returns(new List<IUserGroup> { CreateUserGroup(Group2Id, Group2Alias) });
            mockUserService
                .Setup(x => x.GetAllUserGroups(It.Is<int[]>(y => y.Length == 2 && y[0] == Group1Id && y[1] == Group2Id)))
                .Returns(new List<IUserGroup> { CreateUserGroup(Group1Id, Group1Alias), CreateUserGroup(Group2Id, Group2Alias) });
            mockUserService
                .Setup(x => x.GetAllUserGroups(It.Is<int[]>(y => y.Length == 2 && y[0] == Group2Id && y[1] == Group3Id)))
                .Returns(new List<IUserGroup> { CreateUserGroup(Group2Id, Group2Alias), CreateUserGroup(Group3Id, Group3Alias) });
            return mockUserService;
        }

        private static Mock<IBackOfficeSecurityAccessor> CreateMockBackOfficeSecurityAccessor(bool userIsAdmin)
        {
            User user = CreateUser(userIsAdmin);
            var mockBackOfficeSecurity = new Mock<IBackOfficeSecurity>();
            mockBackOfficeSecurity.SetupGet(x => x.CurrentUser).Returns(user);
            var mockBackOfficeSecurityAccessor = new Mock<IBackOfficeSecurityAccessor>();
            mockBackOfficeSecurityAccessor.Setup(x => x.BackOfficeSecurity).Returns(mockBackOfficeSecurity.Object);
            return mockBackOfficeSecurityAccessor;
        }

        private static User CreateUser(bool isAdmin = false) =>
            new UserBuilder()
                .AddUserGroup()
                    .WithAlias(isAdmin ? Constants.Security.AdminGroupAlias : Group1Alias)
                    .Done()
                .Build();

        private IUserGroup CreateUserGroup(int id, string alias) =>
            new UserGroupBuilder()
                .WithId(id)
                .WithAlias(alias)
                .Build();
    }
}
