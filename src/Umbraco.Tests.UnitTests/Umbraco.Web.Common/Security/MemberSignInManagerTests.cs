using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Net;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Security
{
    [TestFixture]
    public class MemberSignInManagerTests
    {
        private Mock<ILogger<IMemberManager>> _mockLogger;
        private readonly Mock<UserManager<MemberIdentityUser>> _memberManager = MockUserManager<MemberIdentityUser>();
        private readonly Mock<IIpResolver> _mockIpResolver = new Mock<IIpResolver>();

        public MemberSignInManager CreateSut()
        {
            _mockLogger = new Mock<ILogger<IMemberManager>>();
            return new MemberSignInManager(
                    _memberManager.Object,
                    Mock.Of<IIpResolver>(),
                    Mock.Of<IHttpContextAccessor>(),
                    Mock.Of<IUserClaimsPrincipalFactory<MemberIdentityUser>>(),
                    Mock.Of<IOptions<IdentityOptions>>(),
                    Mock.Of<ILogger<SignInManager<MemberIdentityUser>>>(),
                    Mock.Of<IAuthenticationSchemeProvider>(),
                    Mock.Of<IUserConfirmation<MemberIdentityUser>>());
        }
        private static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
        {
            var store = new Mock<IUserStore<TUser>>();
            var mgr = new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
            return mgr;
        }

        [Test]
        public async Task WhenPasswordSignInAsyncIsCalled_AndEverythingIsSetup_ThenASignInResultSucceededShouldBeReturnedAsync()
        {
            //arrange
            var userId = "bo8w3d32q9b98";
            _memberManager.Setup(x => x.GetUserIdAsync(It.IsAny<MemberIdentityUser>())).ReturnsAsync(userId);
            MemberSignInManager sut = CreateSut();
            var fakeUser = new MemberIdentityUser(777)
            {
                UserName = "TestUser",
            };
            var password = "testPassword";
            var lockoutOnFailure = false;
            var isPersistent = true;
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
            _mockIpResolver.Setup(x => x.GetCurrentRequestIpAddress()).Returns("127.0.0.1");

            //act
            SignInResult actual = await sut.PasswordSignInAsync(fakeUser, password, isPersistent, lockoutOnFailure);

            //assert
            Assert.IsFalse(actual.Succeeded);
            //_mockLogger.Verify(x => x.LogInformation("Login attempt failed for username TestUser from IP address 127.0.0.1", null));
        }
    }
}
