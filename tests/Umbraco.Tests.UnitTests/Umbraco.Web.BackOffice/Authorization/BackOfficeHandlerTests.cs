// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Web.BackOffice.Authorization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.BackOffice.Authorization
{
    public class BackOfficeHandlerTests
    {
        [Test]
        public async Task Runtime_State_Install_Is_Authorized()
        {
            AuthorizationHandlerContext authHandlerContext = CreateAuthorizationHandlerContext();
            BackOfficeHandler sut = CreateHandler(runtimeLevel: RuntimeLevel.Install);

            await sut.HandleAsync(authHandlerContext);

            Assert.IsTrue(authHandlerContext.HasSucceeded);
        }

        [Test]
        public async Task Runtime_State_Upgrade_Is_Authorized()
        {
            AuthorizationHandlerContext authHandlerContext = CreateAuthorizationHandlerContext();
            BackOfficeHandler sut = CreateHandler(runtimeLevel: RuntimeLevel.Upgrade);

            await sut.HandleAsync(authHandlerContext);

            Assert.IsTrue(authHandlerContext.HasSucceeded);
        }

        [Test]
        public async Task Unauthenticated_User_Is_Not_Authorized()
        {
            AuthorizationHandlerContext authHandlerContext = CreateAuthorizationHandlerContext();
            BackOfficeHandler sut = CreateHandler();

            await sut.HandleAsync(authHandlerContext);

            Assert.IsFalse(authHandlerContext.HasSucceeded);
        }

        [Test]
        public async Task Authenticated_User_Is_Not_Authorized_When_Not_Approved_And_Approval_Required()
        {
            AuthorizationHandlerContext authHandlerContext = CreateAuthorizationHandlerContext(requireApproval: true);
            BackOfficeHandler sut = CreateHandler(currentUserIsAuthenticated: true);

            await sut.HandleAsync(authHandlerContext);

            Assert.IsFalse(authHandlerContext.HasSucceeded);
        }

        [Test]
        public async Task Authenticated_User_Is_Authorized_When_Not_Approved_And_Approval_Not_Required()
        {
            AuthorizationHandlerContext authHandlerContext = CreateAuthorizationHandlerContext();
            BackOfficeHandler sut = CreateHandler(currentUserIsAuthenticated: true);

            await sut.HandleAsync(authHandlerContext);

            Assert.IsTrue(authHandlerContext.HasSucceeded);
        }

        [Test]
        public async Task Authenticated_User_Is_Authorized_When_Approved_And_Approval_Required()
        {
            AuthorizationHandlerContext authHandlerContext = CreateAuthorizationHandlerContext(requireApproval: true);
            BackOfficeHandler sut = CreateHandler(currentUserIsAuthenticated: true, currentUserIsApproved: true);

            await sut.HandleAsync(authHandlerContext);

            Assert.IsTrue(authHandlerContext.HasSucceeded);
        }

        private static AuthorizationHandlerContext CreateAuthorizationHandlerContext(bool requireApproval = false)
        {
            var requirement = new BackOfficeRequirement(requireApproval);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>()));
            object resource = new object();
            return new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement }, user, resource);
        }

        private BackOfficeHandler CreateHandler(RuntimeLevel runtimeLevel = RuntimeLevel.Run, bool currentUserIsAuthenticated = false, bool currentUserIsApproved = false)
        {
            Mock<IBackOfficeSecurityAccessor> mockBackOfficeSecurityAccessor = CreateMockBackOfficeSecurityAccessor(currentUserIsAuthenticated, currentUserIsApproved);
            Mock<IRuntimeState> mockRuntimeState = CreateMockRuntimeState(runtimeLevel);
            return new BackOfficeHandler(mockBackOfficeSecurityAccessor.Object, mockRuntimeState.Object);
        }

        private static Mock<IBackOfficeSecurityAccessor> CreateMockBackOfficeSecurityAccessor(bool currentUserIsAuthenticated, bool currentUserIsApproved)
        {
            global::Umbraco.Cms.Core.Models.Membership.User user = new UserBuilder()
                .WithIsApproved(currentUserIsApproved)
                .Build();
            var mockBackOfficeSecurityAccessor = new Mock<IBackOfficeSecurityAccessor>();
            var mockBackOfficeSecurity = new Mock<IBackOfficeSecurity>();
            mockBackOfficeSecurity.Setup(x => x.IsAuthenticated()).Returns(currentUserIsAuthenticated);
            if (currentUserIsAuthenticated)
            {
                mockBackOfficeSecurity.Setup(x => x.CurrentUser).Returns(user);
            }

            mockBackOfficeSecurityAccessor = new Mock<IBackOfficeSecurityAccessor>();
            mockBackOfficeSecurityAccessor.Setup(x => x.BackOfficeSecurity).Returns(mockBackOfficeSecurity.Object);
            return mockBackOfficeSecurityAccessor;
        }

        private static Mock<IRuntimeState> CreateMockRuntimeState(RuntimeLevel runtimeLevel)
        {
            var mockRuntimeState = new Mock<IRuntimeState>();
            mockRuntimeState.SetupGet(x => x.Level).Returns(runtimeLevel);
            return mockRuntimeState;
        }
    }
}
