using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.BackOffice;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Membership;
using Umbraco.Net;
using Umbraco.Web.Security;


namespace Umbraco.Tests.Security
{
    public class UmbracoSecurityStampValidatorTests
    {
        private Mock<IOwinContext> _mockOwinContext;
        private Mock<Umbraco.Web.Security.BackOfficeUserManager<BackOfficeIdentityUser>> _mockUserManager;
        private Mock<BackOfficeSignInManager> _mockSignInManager;

        private AuthenticationTicket _testAuthTicket;
        private CookieAuthenticationOptions _testOptions;
        private BackOfficeIdentityUser _testUser;
        private const string _testAuthType = "cookie";

        [Test]
        public void OnValidateIdentity_When_GetUserIdCallback_Is_Null_Expect_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => UmbracoSecurityStampValidator
                .OnValidateIdentity<BackOfficeSignInManager, Umbraco.Web.Security.BackOfficeUserManager<BackOfficeIdentityUser>, BackOfficeIdentityUser>(
                    TimeSpan.MaxValue, null, null));
        }

        [Test]
        public async Task OnValidateIdentity_When_Validation_Interval_Not_Met_Expect_No_Op()
        {
            var func = UmbracoSecurityStampValidator
                .OnValidateIdentity<BackOfficeSignInManager, Umbraco.Web.Security.BackOfficeUserManager<BackOfficeIdentityUser>, BackOfficeIdentityUser>(
                    TimeSpan.MaxValue, null, identity => throw new Exception());

            _testAuthTicket.Properties.IssuedUtc = DateTimeOffset.UtcNow;

            var context = new CookieValidateIdentityContext(
                _mockOwinContext.Object,
                _testAuthTicket,
                _testOptions);

            await func(context);

            Assert.AreEqual(_testAuthTicket.Identity, context.Identity);
        }

        [Test]
        public void OnValidateIdentity_When_Time_To_Validate_But_No_UserManager_Expect_InvalidOperationException()
        {
            var func = UmbracoSecurityStampValidator
                .OnValidateIdentity<BackOfficeSignInManager, Umbraco.Web.Security.BackOfficeUserManager<BackOfficeIdentityUser>, BackOfficeIdentityUser>(
                    TimeSpan.MinValue, null, identity => throw new Exception());

            _mockOwinContext.Setup(x => x.Get<Umbraco.Web.Security.BackOfficeUserManager<BackOfficeIdentityUser>>(It.IsAny<string>()))
                .Returns((Umbraco.Web.Security.BackOfficeUserManager<BackOfficeIdentityUser>) null);

            var context = new CookieValidateIdentityContext(
                _mockOwinContext.Object,
                _testAuthTicket,
                _testOptions);

            Assert.ThrowsAsync<InvalidOperationException>(async () => await func(context));
        }

        [Test]
        public void OnValidateIdentity_When_Time_To_Validate_But_No_SignInManager_Expect_InvalidOperationException()
        {
            var func = UmbracoSecurityStampValidator
                .OnValidateIdentity<BackOfficeSignInManager, Umbraco.Web.Security.BackOfficeUserManager<BackOfficeIdentityUser>, BackOfficeIdentityUser>(
                    TimeSpan.MinValue, null, identity => throw new Exception());

            _mockOwinContext.Setup(x => x.Get<BackOfficeSignInManager>(It.IsAny<string>()))
                .Returns((BackOfficeSignInManager) null);

            var context = new CookieValidateIdentityContext(
                _mockOwinContext.Object,
                _testAuthTicket,
                _testOptions);

            Assert.ThrowsAsync<InvalidOperationException>(async () => await func(context));
        }

        [Test]
        public async Task OnValidateIdentity_When_Time_To_Validate_And_User_No_Longer_Found_Expect_Rejected()
        {
            var userId = Guid.NewGuid().ToString();
            
            var func = UmbracoSecurityStampValidator
                .OnValidateIdentity<BackOfficeSignInManager, Umbraco.Web.Security.BackOfficeUserManager<BackOfficeIdentityUser>, BackOfficeIdentityUser>(
                    TimeSpan.MinValue, null, identity => userId);

            _mockUserManager.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync((BackOfficeIdentityUser) null);

            var context = new CookieValidateIdentityContext(
                _mockOwinContext.Object,
                _testAuthTicket,
                _testOptions);

            await func(context);

            Assert.IsNull(context.Identity);
            _mockOwinContext.Verify(x => x.Authentication.SignOut(_testAuthType), Times.Once);
        }

        [Test]
        public async Task OnValidateIdentity_When_Time_To_Validate_And_User_Exists_And_Does_Not_Support_SecurityStamps_Expect_Rejected()
        {
            var userId = Guid.NewGuid().ToString();

            var func = UmbracoSecurityStampValidator
                .OnValidateIdentity<BackOfficeSignInManager, Umbraco.Web.Security.BackOfficeUserManager<BackOfficeIdentityUser>, BackOfficeIdentityUser>(
                    TimeSpan.MinValue, null, identity => userId);

            _mockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(_testUser);
            _mockUserManager.Setup(x => x.SupportsUserSecurityStamp).Returns(false);

            var context = new CookieValidateIdentityContext(
                _mockOwinContext.Object,
                _testAuthTicket,
                _testOptions);

            await func(context);

            Assert.IsNull(context.Identity);
            _mockOwinContext.Verify(x => x.Authentication.SignOut(_testAuthType), Times.Once);
        }

        [Test]
        public async Task OnValidateIdentity_When_Time_To_Validate_And_SecurityStamp_Has_Changed_Expect_Rejected()
        {
            var userId = Guid.NewGuid().ToString();

            var func = UmbracoSecurityStampValidator
                .OnValidateIdentity<BackOfficeSignInManager, Umbraco.Web.Security.BackOfficeUserManager<BackOfficeIdentityUser>, BackOfficeIdentityUser>(
                    TimeSpan.MinValue, null, identity => userId);

            _mockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(_testUser);
            _mockUserManager.Setup(x => x.SupportsUserSecurityStamp).Returns(true);
            _mockUserManager.Setup(x => x.GetSecurityStampAsync(_testUser)).ReturnsAsync(Guid.NewGuid().ToString);

            var context = new CookieValidateIdentityContext(
                _mockOwinContext.Object,
                _testAuthTicket,
                _testOptions);

            await func(context);

            Assert.IsNull(context.Identity);
            _mockOwinContext.Verify(x => x.Authentication.SignOut(_testAuthType), Times.Once);
        }

        [Test]
        public async Task OnValidateIdentity_When_Time_To_Validate_And_SecurityStamp_Has_Not_Changed_Expect_No_Change()
        {
            var userId = Guid.NewGuid().ToString();

            var func = UmbracoSecurityStampValidator
                .OnValidateIdentity<BackOfficeSignInManager, Umbraco.Web.Security.BackOfficeUserManager<BackOfficeIdentityUser>, BackOfficeIdentityUser>(
                    TimeSpan.MinValue, null, identity => userId);

            _mockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(_testUser);
            _mockUserManager.Setup(x => x.SupportsUserSecurityStamp).Returns(true);
            _mockUserManager.Setup(x => x.GetSecurityStampAsync(_testUser)).ReturnsAsync(_testUser.SecurityStamp);

            var context = new CookieValidateIdentityContext(
                _mockOwinContext.Object,
                _testAuthTicket,
                _testOptions);

            await func(context);

            Assert.AreEqual(_testAuthTicket.Identity, context.Identity);
        }

        [Test]
        public async Task OnValidateIdentity_When_User_Validated_And_RegenerateIdentityCallback_Present_Expect_User_Refreshed()
        {
            var userId = Guid.NewGuid().ToString();
            var expectedIdentity = new ClaimsIdentity(new List<Claim> {new Claim("sub", "bob")});

            var regenFuncCalled = false;
            Func<BackOfficeSignInManager, Umbraco.Web.Security.BackOfficeUserManager<BackOfficeIdentityUser>, BackOfficeIdentityUser, Task<ClaimsIdentity>> regenFunc =
                (signInManager, userManager, user) =>
                {
                    regenFuncCalled = true;
                    return Task.FromResult(expectedIdentity);
                };

            var func = UmbracoSecurityStampValidator
                .OnValidateIdentity<BackOfficeSignInManager, Umbraco.Web.Security.BackOfficeUserManager<BackOfficeIdentityUser>, BackOfficeIdentityUser>(
                    TimeSpan.MinValue, regenFunc, identity => userId);

            _mockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(_testUser);
            _mockUserManager.Setup(x => x.SupportsUserSecurityStamp).Returns(true);
            _mockUserManager.Setup(x => x.GetSecurityStampAsync(_testUser)).ReturnsAsync(_testUser.SecurityStamp);
            
            var context = new CookieValidateIdentityContext(
                _mockOwinContext.Object,
                _testAuthTicket,
                _testOptions);

            ClaimsIdentity callbackIdentity = null;
            _mockOwinContext.Setup(x => x.Authentication.SignIn(context.Properties, It.IsAny<ClaimsIdentity>()))
                .Callback((AuthenticationProperties props, ClaimsIdentity[] identities) => callbackIdentity = identities.FirstOrDefault())
                .Verifiable();

            await func(context);

            Assert.True(regenFuncCalled);
            Assert.AreEqual(expectedIdentity, callbackIdentity);
            Assert.IsNull(context.Properties.IssuedUtc);
            Assert.IsNull(context.Properties.ExpiresUtc);

            _mockOwinContext.Verify();
        }

        [SetUp]
        public void Setup()
        {
            var mockGlobalSettings = new Mock<IGlobalSettings>();
            mockGlobalSettings.Setup(x => x.DefaultUILanguage).Returns("test");

            _testUser = new BackOfficeIdentityUser(mockGlobalSettings.Object, 2, new List<IReadOnlyUserGroup>())
            {
                UserName = "alice",
                Name = "Alice",
                Email = "alice@umbraco.test",
                SecurityStamp = Guid.NewGuid().ToString()
            };

            _testAuthTicket = new AuthenticationTicket(
                new ClaimsIdentity(
                    new List<Claim> {new Claim("sub", "alice"), new Claim(Constants.Web.SecurityStampClaimType, _testUser.SecurityStamp)},
                    _testAuthType),
                new AuthenticationProperties());
            _testOptions = new CookieAuthenticationOptions { AuthenticationType = _testAuthType };

            _mockUserManager = new Mock<Umbraco.Web.Security.BackOfficeUserManager<BackOfficeIdentityUser>>(
                new Mock<IPasswordConfiguration>().Object,
                new Mock<IIpResolver>().Object,
                new Mock<IUserStore<BackOfficeIdentityUser>>().Object,
                null, null, null, null, null, null, null);
            _mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((BackOfficeIdentityUser) null);
            _mockUserManager.Setup(x => x.SupportsUserSecurityStamp).Returns(false);

            _mockSignInManager = new Mock<BackOfficeSignInManager>(
                _mockUserManager.Object,
                new Mock<IUserClaimsPrincipalFactory<BackOfficeIdentityUser>>().Object,
                new Mock<IAuthenticationManager>().Object,
                new Mock<ILogger>().Object,
                new Mock<IGlobalSettings>().Object,
                new Mock<IOwinRequest>().Object);

            _mockOwinContext = new Mock<IOwinContext>();
            _mockOwinContext.Setup(x => x.Get<Umbraco.Web.Security.BackOfficeUserManager<BackOfficeIdentityUser>>(It.IsAny<string>()))
                .Returns(_mockUserManager.Object);
            _mockOwinContext.Setup(x => x.Get<BackOfficeSignInManager>(It.IsAny<string>()))
                .Returns(_mockSignInManager.Object);

            _mockOwinContext.Setup(x => x.Authentication.SignOut(It.IsAny<string>()));
        }
    }
}
