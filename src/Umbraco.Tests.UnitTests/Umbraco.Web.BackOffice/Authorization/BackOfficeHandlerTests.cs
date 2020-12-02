using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Security;
using Umbraco.Tests.Common.Builders;
using Umbraco.Web.BackOffice.Authorization;

namespace Umbraco.Tests.UnitTests.Umbraco.Web.BackOffice.Authorization
{
    public class BackOfficeHandlerTests
    {
        [Test]
        public async Task Runtime_State_Install_Is_Authorized()
        {
            var authHandlerContext = CreateAuthorizationHandlerContext();
            var sut = CreateHandler(runtimeLevel: RuntimeLevel.Install);

            await sut.HandleAsync(authHandlerContext);

            Assert.IsTrue(authHandlerContext.HasSucceeded);
        }

        [Test]
        public async Task Runtime_State_Upgrade_Is_Authorized()
        {
            var authHandlerContext = CreateAuthorizationHandlerContext();
            var sut = CreateHandler(runtimeLevel: RuntimeLevel.Upgrade);

            await sut.HandleAsync(authHandlerContext);

            Assert.IsTrue(authHandlerContext.HasSucceeded);
        }

        [Test]
        public async Task Non_Validated_User_Is_Not_Authorized()
        {
            var authHandlerContext = CreateAuthorizationHandlerContext();
            var sut = CreateHandler();

            await sut.HandleAsync(authHandlerContext);

            Assert.IsFalse(authHandlerContext.HasSucceeded);
        }

        [Test]
        public async Task Validated_User_Is_Not_Authorized_When_Not_Approved_And_Approval_Required()
        {
            var authHandlerContext = CreateAuthorizationHandlerContext(requireApproval: true);
            var sut = CreateHandler(requireApproval: true, isAuthenticated: true);

            await sut.HandleAsync(authHandlerContext);

            Assert.IsFalse(authHandlerContext.HasSucceeded);
        }

        [Test]
        public async Task Validated_User_Is_Authorized_When_Not_Approved_And_Approval_Not_Required()
        {
            var authHandlerContext = CreateAuthorizationHandlerContext();
            var sut = CreateHandler(isAuthenticated: true);

            await sut.HandleAsync(authHandlerContext);

            Assert.IsTrue(authHandlerContext.HasSucceeded);
        }

        [Test]
        public async Task Validated_User_Is_Authorized_When_Approved_And_Approval_Required()
        {
            var authHandlerContext = CreateAuthorizationHandlerContext(requireApproval: true);
            var sut = CreateHandler(requireApproval: true, isAuthenticated: true, isApproved: true);

            await sut.HandleAsync(authHandlerContext);

            Assert.IsTrue(authHandlerContext.HasSucceeded);
        }

        private static AuthorizationHandlerContext CreateAuthorizationHandlerContext(bool requireApproval = false)
        {
            var requirement = new BackOfficeRequirement(requireApproval);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>()));
            var resource = new object();
            return new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement }, user, resource);
        }

        private BackOfficeHandler CreateHandler(RuntimeLevel runtimeLevel = RuntimeLevel.Run, bool requireApproval = false, bool isAuthenticated = false, bool isApproved = false)
        {
            var mockBackOfficeSecurityAccessor = CreateMockBackOfficeSecurityAccessor(requireApproval, isAuthenticated, isApproved);
            var mockRuntimeState = CreateMockRuntimeState(runtimeLevel);
            return new BackOfficeHandler(mockBackOfficeSecurityAccessor.Object, mockRuntimeState.Object);
        }

        private static Mock<IBackOfficeSecurityAccessor> CreateMockBackOfficeSecurityAccessor(bool requireApproval, bool isAuthenticated, bool isApproved)
        {
            var user = new UserBuilder()
                .Build();
            var mockBackOfficeSecurityAccessor = new Mock<IBackOfficeSecurityAccessor>();
            var mockBackOfficeSecurity = new Mock<IBackOfficeSecurity>();
            mockBackOfficeSecurity
                .Setup(x => x.ValidateCurrentUser(It.Is<bool>(y => y == false), It.Is<bool>(y => y == requireApproval)))
                .Returns(isAuthenticated
                    ? !requireApproval || (requireApproval && isApproved)
                        ? ValidateRequestAttempt.Success
                        : ValidateRequestAttempt.FailedNoPrivileges
                    : ValidateRequestAttempt.FailedNoPrivileges);
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
