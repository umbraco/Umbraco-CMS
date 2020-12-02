using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Umbraco.Tests.Common.Builders;
using Umbraco.Web.BackOffice.Authorization;
using Umbraco.Web.Common.Security;

namespace Umbraco.Tests.UnitTests.Umbraco.Web.BackOffice.Authorization
{
    public class DenyLocalLoginHandlerTests
    {
        [Test]
        public async Task With_Deny_Local_Login_Is_Not_Authorized()
        {
            var authHandlerContext = CreateAuthorizationHandlerContext();
            var sut = CreateHandler(denyLocalLogin: true);

            await sut.HandleAsync(authHandlerContext);

            Assert.IsFalse(authHandlerContext.HasSucceeded);
        }

        [Test]
        public async Task Without_Deny_Local_Login_Is_Authorized()
        {
            var authHandlerContext = CreateAuthorizationHandlerContext();
            var sut = CreateHandler();

            await sut.HandleAsync(authHandlerContext);

            Assert.IsTrue(authHandlerContext.HasSucceeded);
        }

        private static AuthorizationHandlerContext CreateAuthorizationHandlerContext()
        {
            var requirement = new DenyLocalLoginRequirement();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>()));
            var resource = new object();
            return new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement }, user, resource);
        }

        private DenyLocalLoginHandler CreateHandler(bool denyLocalLogin = false)
        {
            var mockBackOfficeExternalLoginProviders = CreateMockBackOfficeExternalLoginProviders(denyLocalLogin);

            return new DenyLocalLoginHandler(mockBackOfficeExternalLoginProviders.Object);
        }

        private static Mock<IBackOfficeExternalLoginProviders> CreateMockBackOfficeExternalLoginProviders(bool denyLocalLogin)
        {
            var mockBackOfficeExternalLoginProviders = new Mock<IBackOfficeExternalLoginProviders>();
            mockBackOfficeExternalLoginProviders.Setup(x => x.HasDenyLocalLogin()).Returns(denyLocalLogin);
            return mockBackOfficeExternalLoginProviders;
        }
    }
}
