// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Security;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Core.Models;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Tests.Common.Builders;
using Umbraco.Web.Website.Security;
using CoreConstants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Tests.UnitTests.Umbraco.Web.Website.Security
{
    [TestFixture]
    public class UmbracoWebsiteSecurityTests
    {
        [Test]
        public void Can_Create_Registration_Model_With_Default_Member_Type()
        {
            var sut = CreateUmbracoWebsiteSecurity();

            var result = sut.CreateRegistrationModel();
            AssertRegisterModel(result);
        }

        [Test]
        public void Can_Create_Registration_Model_With_Custom_Member_Type()
        {
            const string Alias = "testAlias";
            var sut = CreateUmbracoWebsiteSecurity(Alias);

            var result = sut.CreateRegistrationModel(Alias);
            AssertRegisterModel(result, Alias);
        }

        [Test]
        public void Can_Detected_Logged_In_User()
        {
            var sut = CreateUmbracoWebsiteSecurity();

            var result = sut.IsLoggedIn();
            Assert.IsTrue(result);
        }

        [Test]
        public void Can_Detected_Anonymous_User()
        {
            var sut = CreateUmbracoWebsiteSecurity(isUserAuthenticated: false);

            var result = sut.IsLoggedIn();
            Assert.IsFalse(result);
        }

        private static void AssertRegisterModel(RegisterModel result, string memberTypeAlias = CoreConstants.Conventions.MemberTypes.DefaultAlias)
        {
            Assert.AreEqual(memberTypeAlias, result.MemberTypeAlias);
            Assert.AreEqual(1, result.MemberProperties.Count);

            var firstProperty = result.MemberProperties.First();
            Assert.AreEqual("title", firstProperty.Alias);
            Assert.AreEqual("Title", firstProperty.Name);
            Assert.AreEqual(string.Empty, firstProperty.Value);
        }

        private IUmbracoWebsiteSecurity CreateUmbracoWebsiteSecurity(string memberTypeAlias = CoreConstants.Conventions.MemberTypes.DefaultAlias, bool isUserAuthenticated = true)
        {
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var mockHttpContext = new Mock<HttpContext>();
            var mockIdentity = new Mock<IIdentity>();
            mockIdentity.SetupGet(x => x.IsAuthenticated).Returns(isUserAuthenticated);
            var mockPrincipal = new Mock<ClaimsPrincipal>();
            mockPrincipal.Setup(x => x.Identity).Returns(mockIdentity.Object);
            mockHttpContext.Setup(m => m.User).Returns(mockPrincipal.Object);
            mockHttpContextAccessor.SetupGet(x => x.HttpContext).Returns(mockHttpContext.Object);

            var mockMemberService = new Mock<IMemberService>();

            var mockMemberTypeService = new Mock<IMemberTypeService>();
            mockMemberTypeService
                .Setup(x => x.Get(It.Is<string>(y => y == memberTypeAlias)))
                .Returns(CreateSimpleMemberType(memberTypeAlias));

            var mockShortStringHelper = new Mock<IShortStringHelper>();

            return new UmbracoWebsiteSecurity(mockHttpContextAccessor.Object, mockMemberService.Object, mockMemberTypeService.Object, mockShortStringHelper.Object);
        }

        private IMemberType CreateSimpleMemberType(string alias)
        {
            var memberType = MemberTypeBuilder.CreateSimpleMemberType(alias);
            memberType.SetMemberCanEditProperty("title", true);
            return memberType;
        }
    }
}
