using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Net;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Security
{
    [TestFixture]
    public class MemberSignInManagerTests
    {
        private Mock<ILogger<SignInManager<MemberIdentityUser>>> _mockLogger;
        private readonly Mock<MemberManager> _memberManager = MockMemberManager();

        public UserClaimsPrincipalFactory<MemberIdentityUser> CreateClaimsFactory(MemberManager userMgr)
            => new UserClaimsPrincipalFactory<MemberIdentityUser>(userMgr, Options.Create(new IdentityOptions()));

        public MemberSignInManager CreateSut()
        {
            // This all needs to be setup because internally aspnet resolves a bunch
            // of services from the HttpContext.RequestServices.
            var serviceProviderFactory = new DefaultServiceProviderFactory();
            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddLogging()
                .AddAuthentication()
                .AddCookie(IdentityConstants.ApplicationScheme);
            IServiceProvider serviceProvider = serviceProviderFactory.CreateServiceProvider(serviceCollection);
            var httpContextFactory = new DefaultHttpContextFactory(serviceProvider);
            IFeatureCollection features = new DefaultHttpContext().Features;
            features.Set<IHttpConnectionFeature>(new HttpConnectionFeature
            {
                LocalIpAddress = IPAddress.Parse("127.0.0.1")
            });
            HttpContext httpContext = httpContextFactory.Create(features);

            _mockLogger = new Mock<ILogger<SignInManager<MemberIdentityUser>>>();
            return new MemberSignInManager(
                    _memberManager.Object,
                    Mock.Of<IHttpContextAccessor>(x => x.HttpContext == httpContext),
                    CreateClaimsFactory(_memberManager.Object),
                    Mock.Of<IOptions<IdentityOptions>>(),
                    _mockLogger.Object,
                    Mock.Of<IAuthenticationSchemeProvider>(),
                    Mock.Of<IUserConfirmation<MemberIdentityUser>>());
        }
        private static Mock<MemberManager> MockMemberManager()
            => new Mock<MemberManager>(
                    Mock.Of<IIpResolver>(),
                    Mock.Of<IMemberUserStore>(),
                    Options.Create(new IdentityOptions()),
                    Mock.Of<IPasswordHasher<MemberIdentityUser>>(),
                    Enumerable.Empty<IUserValidator<MemberIdentityUser>>(),
                    Enumerable.Empty<IPasswordValidator<MemberIdentityUser>>(),
                    new MembersErrorDescriber(),
                    Mock.Of<IServiceProvider>(),
                    Mock.Of<ILogger<UserManager<MemberIdentityUser>>>(),
                    Options.Create(new MemberPasswordConfigurationSettings()),
                    Mock.Of<IPublicAccessService>(),
                    Mock.Of<IHttpContextAccessor>());

        [Test]
        public async Task WhenPasswordSignInAsyncIsCalled_AndEverythingIsSetup_ThenASignInResultSucceededShouldBeReturnedAsync()
        {
            //arrange
            var userId = "bo8w3d32q9b98";            
            MemberSignInManager sut = CreateSut();
            var fakeUser = new MemberIdentityUser(777)
            {
                UserName = "TestUser",
            };
            var password = "testPassword";
            var lockoutOnFailure = false;
            var isPersistent = true;

            _memberManager.Setup(x => x.GetUserIdAsync(It.IsAny<MemberIdentityUser>())).ReturnsAsync(userId);
            _memberManager.Setup(x => x.GetUserNameAsync(It.IsAny<MemberIdentityUser>())).ReturnsAsync(fakeUser.UserName);
            _memberManager.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(fakeUser);
            _memberManager.Setup(x => x.CheckPasswordAsync(fakeUser, password)).ReturnsAsync(true);
            _memberManager.Setup(x => x.IsEmailConfirmedAsync(fakeUser)).ReturnsAsync(true);
            _memberManager.Setup(x => x.IsLockedOutAsync(fakeUser)).ReturnsAsync(false);

            //act
            SignInResult actual = await sut.PasswordSignInAsync(fakeUser, password, isPersistent, lockoutOnFailure);

            //assert
            Assert.IsTrue(actual.Succeeded);
        }

        [Test]
        public async Task WhenPasswordSignInAsyncIsCalled_AndTheResultFails_ThenASignInFailedResultShouldBeReturnedAsync()
        {
            //arrange
            MemberSignInManager sut = CreateSut();
            var fakeUser = new MemberIdentityUser(777)
            {
                UserName = "TestUser",
            };
            var password = "testPassword";
            var lockoutOnFailure = false;
            var isPersistent = true;

            //act
            SignInResult actual = await sut.PasswordSignInAsync(fakeUser, password, isPersistent, lockoutOnFailure);

            //assert
            Assert.IsFalse(actual.Succeeded);
        }
    }
}
