using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Members;
using Umbraco.Infrastructure.Members;

namespace Umbraco.Tests.UnitTests.Umbraco.Infrastructure.Members
{
    [TestFixture]
    public class UmbracoMemberIdentityUserManagerTests
    {
        private Mock<IUserStore<UmbracoMembersIdentityUser>> _mockMemberStore;
        private Mock<IOptions<UmbracoMembersIdentityOptions>> _mockIdentityOptions;
        private Mock<IPasswordHasher<UmbracoMembersIdentityUser>> _mockPasswordHasher;
        private Mock<IUserValidator<UmbracoMembersIdentityUser>> _mockUserValidators;
        private Mock<IEnumerable<IPasswordValidator<UmbracoMembersIdentityUser>>> _mockPasswordValidators;
        private Mock<ILookupNormalizer> _mockNormalizer;
        private IdentityErrorDescriber _mockErrorDescriber;
        private Mock<IServiceProvider> _mockServiceProviders;
        private Mock<ILogger<UserManager<UmbracoMembersIdentityUser>>> _mockLogger;
        private Mock<IOptions<MemberPasswordConfigurationSettings>> _mockPasswordConfiguration;

        public UmbracoMembersUserManager<UmbracoMembersIdentityUser> CreateSut()
        {
            _mockMemberStore = new Mock<IUserStore<UmbracoMembersIdentityUser>>();
            _mockIdentityOptions = new Mock<IOptions<UmbracoMembersIdentityOptions>>();

            var idOptions = new UmbracoMembersIdentityOptions { Lockout = { AllowedForNewUsers = false } };
            _mockIdentityOptions.Setup(o => o.Value).Returns(idOptions);
            _mockPasswordHasher = new Mock<IPasswordHasher<UmbracoMembersIdentityUser>>();

            var userValidators = new List<IUserValidator<UmbracoMembersIdentityUser>>();
            _mockUserValidators = new Mock<IUserValidator<UmbracoMembersIdentityUser>>();
            var validator = new Mock<IUserValidator<UmbracoMembersIdentityUser>>();
            userValidators.Add(validator.Object);

            _mockPasswordValidators = new Mock<IEnumerable<IPasswordValidator<UmbracoMembersIdentityUser>>>();
            _mockNormalizer = new Mock<ILookupNormalizer>();
            _mockErrorDescriber = new IdentityErrorDescriber();
            _mockServiceProviders = new Mock<IServiceProvider>();
            _mockLogger = new Mock<ILogger<UserManager<UmbracoMembersIdentityUser>>>();
            _mockPasswordConfiguration = new Mock<IOptions<MemberPasswordConfigurationSettings>>();
            _mockPasswordConfiguration.Setup(x => x.Value).Returns(() =>
                new MemberPasswordConfigurationSettings()
                {
                    
                });

            var pwdValidators = new List<PasswordValidator<UmbracoMembersIdentityUser>>
            {
                new PasswordValidator<UmbracoMembersIdentityUser>()
            };

            var userManager = new UmbracoMembersUserManager<UmbracoMembersIdentityUser>(
                _mockMemberStore.Object,
                _mockIdentityOptions.Object,
                _mockPasswordHasher.Object,
                userValidators,
                pwdValidators,
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                _mockServiceProviders.Object,
                new Mock<ILogger<UserManager<UmbracoMembersIdentityUser>>>().Object,
                _mockPasswordConfiguration.Object);

            validator.Setup(v => v.ValidateAsync(
                    userManager,
                    It.IsAny<UmbracoMembersIdentityUser>()))
                .Returns(Task.FromResult(IdentityResult.Success)).Verifiable();

            return userManager;
        }

        [Test]
        public async Task GivenICreateUser_AndTheIdentityResultFailed_ThenIShouldGetAFailedResultAsync()
        {
            //arrange
            UmbracoMembersUserManager<UmbracoMembersIdentityUser> sut = CreateSut();
            UmbracoMembersIdentityUser fakeUser = new UmbracoMembersIdentityUser()
            {
                PasswordConfig = "testConfig"
            };
            CancellationToken fakeCancellationToken = new CancellationToken() { };
            IdentityError[] identityErrors =
            {
                new IdentityError()
                {
                    Code = "IdentityError1",
                    Description = "There was an identity error when creating a user"
                }
            };

            _mockMemberStore.Setup(x =>
                    x.CreateAsync(fakeUser, fakeCancellationToken))
                .ReturnsAsync(IdentityResult.Failed(identityErrors));

            //act
            IdentityResult identityResult = await sut.CreateAsync(fakeUser);

            //assert
            Assert.IsFalse(identityResult.Succeeded);
            Assert.IsFalse(!identityResult.Errors.Any());

        }


        [Test]
        public async Task GivenICreateUser_AndTheUserIsNull_ThenIShouldGetAFailedResultAsync()
        {
            //arrange
            UmbracoMembersUserManager<UmbracoMembersIdentityUser> sut = CreateSut();
            CancellationToken fakeCancellationToken = new CancellationToken() { };
            IdentityError[] identityErrors =
            {
                new IdentityError()
                {
                    Code = "IdentityError1",
                    Description = "There was an identity error when creating a user"
                }
            };

            _mockMemberStore.Setup(x =>
                    x.CreateAsync(null, fakeCancellationToken))
                .ReturnsAsync(IdentityResult.Failed(identityErrors));

            //act
            var identityResult = new Func<Task<IdentityResult>>(() => sut.CreateAsync(null));


            //assert
            Assert.That(identityResult, Throws.ArgumentNullException);
        }


        [Test]
        public async Task GivenICreateANewUser_AndTheUserIsPopulatedCorrectly_ThenIShouldGetASuccessResultAsync()
        {
            //arrange
            UmbracoMembersUserManager<UmbracoMembersIdentityUser> sut = CreateSut();
            UmbracoMembersIdentityUser fakeUser = new UmbracoMembersIdentityUser()
            {
                PasswordConfig = "testConfig"
            };
            CancellationToken fakeCancellationToken = new CancellationToken() { };
            _mockMemberStore.Setup(x =>
                x.CreateAsync(fakeUser, fakeCancellationToken))
                .ReturnsAsync(IdentityResult.Success);

            //act
            IdentityResult identityResult = await sut.CreateAsync(fakeUser);

            //assert
            Assert.IsTrue(identityResult.Succeeded);
            Assert.IsTrue(!identityResult.Errors.Any());
        }
    }
}
