// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Web.BackOffice.Authorization;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.BackOffice.Authorization
{
    public class SectionHandlerTests
    {
        [Test]
        public async Task Unauthorized_User_Is_Not_Authorized()
        {
            AuthorizationHandlerContext authHandlerContext = CreateAuthorizationHandlerContext();
            SectionHandler sut = CreateHandler();

            await sut.HandleAsync(authHandlerContext);

            Assert.IsFalse(authHandlerContext.HasSucceeded);
        }

        [Test]
        public async Task User_With_Section_Access_Is_Authorized()
        {
            AuthorizationHandlerContext authHandlerContext = CreateAuthorizationHandlerContext();
            SectionHandler sut = CreateHandler(userIsAuthorized: true, userCanAccessContentSection: true);

            await sut.HandleAsync(authHandlerContext);

            Assert.IsTrue(authHandlerContext.HasSucceeded);
        }

        [Test]
        public async Task User_Without_Section_Access_Is_Not_Authorized()
        {
            AuthorizationHandlerContext authHandlerContext = CreateAuthorizationHandlerContext();
            SectionHandler sut = CreateHandler(userIsAuthorized: true);

            await sut.HandleAsync(authHandlerContext);

            Assert.IsFalse(authHandlerContext.HasSucceeded);
        }

        private static AuthorizationHandlerContext CreateAuthorizationHandlerContext()
        {
            var requirement = new SectionRequirement(Constants.Applications.Content, Constants.Applications.Media);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>()));
            object resource = new object();
            return new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement }, user, resource);
        }

        private SectionHandler CreateHandler(bool userIsAuthorized = false, bool userCanAccessContentSection = false)
        {
            Mock<IBackOfficeSecurityAccessor> mockBackOfficeSecurityAccessor = CreateMockBackOfficeSecurityAccessor(userIsAuthorized, userCanAccessContentSection);

            return new SectionHandler(mockBackOfficeSecurityAccessor.Object);
        }

        private static Mock<IBackOfficeSecurityAccessor> CreateMockBackOfficeSecurityAccessor(bool userIsAuthorized, bool userCanAccessContentSection)
        {
            User user = CreateUser();
            var mockBackOfficeSecurity = new Mock<IBackOfficeSecurity>();
            mockBackOfficeSecurity.SetupGet(x => x.CurrentUser).Returns(userIsAuthorized ? user : null);
            mockBackOfficeSecurity
                .Setup(x => x.UserHasSectionAccess(Constants.Applications.Content, It.Is<IUser>(y => y.Username == user.Username)))
                .Returns(userCanAccessContentSection);
            mockBackOfficeSecurity
                .Setup(x => x.UserHasSectionAccess(Constants.Applications.Media, It.Is<IUser>(y => y.Username == user.Username)))
                .Returns(false);
            var mockBackOfficeSecurityAccessor = new Mock<IBackOfficeSecurityAccessor>();
            mockBackOfficeSecurityAccessor.Setup(x => x.BackOfficeSecurity).Returns(mockBackOfficeSecurity.Object);
            return mockBackOfficeSecurityAccessor;
        }

        private static User CreateUser() =>
            new UserBuilder()
                .Build();
    }
}
