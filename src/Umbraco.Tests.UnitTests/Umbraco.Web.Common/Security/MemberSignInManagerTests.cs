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
using Umbraco.Cms.Core.Net;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Security
{
    [TestFixture]
    public class MemberSignInManagerTests
    {
        private Mock<IUserStore<MemberIdentityUser>> _mockMemberStore;
        private Mock<IOptions<MemberIdentityOptions>> _mockIdentityOptions;
        private Mock<IPasswordHasher<MemberIdentityUser>> _mockPasswordHasher;
        private Mock<IUserValidator<MemberIdentityUser>> _mockUserValidators;
        private Mock<IEnumerable<IPasswordValidator<MemberIdentityUser>>> _mockPasswordValidators;
        private Mock<ILookupNormalizer> _mockNormalizer;
        private IdentityErrorDescriber _mockErrorDescriber;
        private Mock<IServiceProvider> _mockServiceProviders;
        private Mock<ILogger<UserManager<MemberIdentityUser>>> _mockLogger;
        private Mock<IOptions<MemberPasswordConfigurationSettings>> _mockPasswordConfiguration;

        public MemberSignInManager CreateSut()
        {
            _mockMemberStore = new Mock<IUserStore<MemberIdentityUser>>();
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
            _mockLogger = new Mock<ILogger<UserManager<MemberIdentityUser>>>();
            _mockPasswordConfiguration = new Mock<IOptions<MemberPasswordConfigurationSettings>>();
            _mockPasswordConfiguration.Setup(x => x.Value).Returns(() =>
                new MemberPasswordConfigurationSettings()
                {

                });

            var pwdValidators = new List<PasswordValidator<MemberIdentityUser>>
            {
                new PasswordValidator<MemberIdentityUser>()
            };

            var userManager = new MemberManager(
                new Mock<IIpResolver>().Object,
                _mockMemberStore.Object,
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
                    userManager,
                    It.IsAny<MemberIdentityUser>()))
                .Returns(Task.FromResult(IdentityResult.Success)).Verifiable();

            return new MemberSignInManager(
                    userManager,
                    Mock.Of<IHttpContextAccessor>(),
                    Mock.Of<IUserClaimsPrincipalFactory<MemberIdentityUser>>(),
                    Mock.Of<IOptions<IdentityOptions>>(),
                    Mock.Of<ILogger<SignInManager<MemberIdentityUser>>>(),
                    Mock.Of<IAuthenticationSchemeProvider>(),
                    Mock.Of<IUserConfirmation<MemberIdentityUser>>());
        }

        [Test]
        public async Task WhenPasswordSignInAsyncIsCalled_AndEverythingIsSetup_ThenASignInResultShouldBeReturnedAsync()
        {
            //arrange
            MemberSignInManager sut = CreateSut();
            var fakeUser = new MemberIdentityUser(777)
            {
            };
            string password = null;
            bool lockoutOnfailure = false;
            bool isPersistent = false;

            //act
            SignInResult actual = await sut.PasswordSignInAsync(fakeUser, password, isPersistent, lockoutOnfailure);

            //assert
            Assert.IsTrue(actual.Succeeded);
        }
    }
}
