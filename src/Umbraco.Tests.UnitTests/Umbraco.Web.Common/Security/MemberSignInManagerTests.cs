using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Net;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Security
{
    [TestFixture]
    public class MemberSignInManagerTests
    {
        private MemberUserStore _fakeMemberStore;
        private Mock<IOptions<MemberIdentityOptions>> _mockIdentityOptions;
        private Mock<IPasswordHasher<MemberIdentityUser>> _mockPasswordHasher;
        private Mock<IUserValidator<MemberIdentityUser>> _mockUserValidators;
        private Mock<IEnumerable<IPasswordValidator<MemberIdentityUser>>> _mockPasswordValidators;
        private Mock<ILookupNormalizer> _mockNormalizer;
        private IdentityErrorDescriber _mockErrorDescriber;
        private Mock<IServiceProvider> _mockServiceProviders;
        private Mock<ILogger<IMemberManager>> _mockLogger;
        private Mock<IOptions<MemberPasswordConfigurationSettings>> _mockPasswordConfiguration;
        private MemberManager _userManager;
        private Mock<IIpResolver> _mockIpResolver = new Mock<IIpResolver>();

        public MemberSignInManager CreateSut()
        {
            var _mockMemberService = new Mock<IMemberService>();
            _fakeMemberStore = new MemberUserStore(
                _mockMemberService.Object,
                new UmbracoMapper(new MapDefinitionCollection(new List<IMapDefinition>())),
                new Mock<IScopeProvider>().Object,
                new IdentityErrorDescriber());

            _mockIdentityOptions = new Mock<IOptions<MemberIdentityOptions>>();

            var idOptions = new MemberIdentityOptions { Lockout = { AllowedForNewUsers = false } };
            _mockIdentityOptions.Setup(o => o.Value).Returns(idOptions);
            _mockPasswordHasher = new Mock<IPasswordHasher<MemberIdentityUser>>();

            var userValidators = new List<IUserValidator<MemberIdentityUser>>();
            _mockUserValidators = new Mock<IUserValidator<MemberIdentityUser>>();
            var validator = new Mock<IUserValidator<MemberIdentityUser>>();
            userValidators.Add(validator.Object);

            _mockPasswordValidators = new Mock<IEnumerable<IPasswordValidator<MemberIdentityUser>>>();
            _mockNormalizer = new Mock<ILookupNormalizer>();
            _mockErrorDescriber = new IdentityErrorDescriber();
            _mockServiceProviders = new Mock<IServiceProvider>();
            _mockLogger = new Mock<ILogger<IMemberManager>>();
            _mockPasswordConfiguration = new Mock<IOptions<MemberPasswordConfigurationSettings>>();
            _mockPasswordConfiguration.Setup(x => x.Value).Returns(() =>
                new MemberPasswordConfigurationSettings() { });

            var pwdValidators = new List<PasswordValidator<MemberIdentityUser>>
            {
                new PasswordValidator<MemberIdentityUser>()
            };

            _userManager = new MemberManager(
                _mockIpResolver.Object,
                _fakeMemberStore,
                _mockIdentityOptions.Object,
                _mockPasswordHasher.Object,
                userValidators,
                pwdValidators,
                new BackOfficeIdentityErrorDescriber(),
                _mockServiceProviders.Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<ILogger<UserManager<MemberIdentityUser>>>().Object,
                _mockPasswordConfiguration.Object);

            validator.Setup(v => v.ValidateAsync(
                    _userManager,
                    It.IsAny<MemberIdentityUser>()))
                .Returns(Task.FromResult(IdentityResult.Success)).Verifiable();

            return new MemberSignInManager(
                    _userManager,
                    Mock.Of<IIpResolver>(),
                    Mock.Of<IHttpContextAccessor>(),
                    Mock.Of<IUserClaimsPrincipalFactory<MemberIdentityUser>>(),
                    Mock.Of<IOptions<IdentityOptions>>(),
                    Mock.Of<ILogger<SignInManager<MemberIdentityUser>>>(),
                    Mock.Of<IAuthenticationSchemeProvider>(),
                    Mock.Of<IUserConfirmation<MemberIdentityUser>>());
        }

        [Test]
        public async Task WhenPasswordSignInAsyncIsCalled_AndEverythingIsSetup_ThenASignInResultSucceededShouldBeReturnedAsync()
        {
            //arrange
            MemberSignInManager sut = CreateSut();
            var fakeUser = new MemberIdentityUser(777)
            {
                UserName = "TestUser",
            };
            string password = "testPassword";
            bool lockoutOnfailure = false;
            bool isPersistent = true;

            //act
            SignInResult actual = await sut.PasswordSignInAsync(fakeUser, password, isPersistent, lockoutOnfailure);

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
            string password = "testPassword";
            bool lockoutOnfailure = false;
            bool isPersistent = true;
            _mockIpResolver.Setup(x => x.GetCurrentRequestIpAddress()).Returns("127.0.0.1");

            //act
            SignInResult actual = await sut.PasswordSignInAsync(fakeUser, password, isPersistent, lockoutOnfailure);

            //assert
            Assert.IsFalse(actual.Succeeded);
            //_mockLogger.Verify(x => x.LogInformation("Login attempt failed for username TestUser from IP address 127.0.0.1", null));
        }
    }
}
