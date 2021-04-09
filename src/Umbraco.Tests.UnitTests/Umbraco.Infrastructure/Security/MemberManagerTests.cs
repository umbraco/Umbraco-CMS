using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Net;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Security
{
    [TestFixture]
    public class MemberManagerTests
    {
        private MemberUserStore _fakeMemberStore;
        private Mock<IOptions<MemberIdentityOptions>> _mockIdentityOptions;
        private Mock<IPasswordHasher<MemberIdentityUser>> _mockPasswordHasher;
        private Mock<IMemberService> _mockMemberService;
        private Mock<IUserValidator<MemberIdentityUser>> _mockUserValidators;
        private Mock<IEnumerable<IPasswordValidator<MemberIdentityUser>>> _mockPasswordValidators;
        private Mock<ILookupNormalizer> _mockNormalizer;
        private IdentityErrorDescriber _mockErrorDescriber;
        private Mock<IServiceProvider> _mockServiceProviders;
        private Mock<ILogger<UserManager<MemberIdentityUser>>> _mockLogger;
        private Mock<IOptions<MemberPasswordConfigurationSettings>> _mockPasswordConfiguration;

        public MemberManager CreateSut()
        {
            _mockMemberService = new Mock<IMemberService>();
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
                _fakeMemberStore,
                _mockIdentityOptions.Object,
                _mockPasswordHasher.Object,
                userValidators,
                pwdValidators,
                new BackOfficeIdentityErrorDescriber(),
                _mockServiceProviders.Object,
                new Mock<ILogger<UserManager<MemberIdentityUser>>>().Object,
                _mockPasswordConfiguration.Object);

            validator.Setup(v => v.ValidateAsync(
                    userManager,
                    It.IsAny<MemberIdentityUser>()))
                .Returns(Task.FromResult(IdentityResult.Success)).Verifiable();

            return userManager;
        }

        [Test]
        public async Task GivenICreateUser_AndTheIdentityResultFailed_ThenIShouldGetAFailedResultAsync()
        {
            //arrange
            MemberManager sut = CreateSut();
            MemberIdentityUser fakeUser = new MemberIdentityUser()
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
            MemberManager sut = CreateSut();
            CancellationToken fakeCancellationToken = new CancellationToken() { };
            IdentityError[] identityErrors =
            {
                new IdentityError()
                {
                    Code = "IdentityError1",
                    Description = "There was an identity error when creating a user"
                }
            };

            //act
            var identityResult = new Func<Task<IdentityResult>>(() => sut.CreateAsync(null));


            //assert
            Assert.That(identityResult, Throws.ArgumentNullException);
        }

        [Test]
        public async Task GivenICreateANewUser_AndTheUserIsPopulatedCorrectly_ThenIShouldGetASuccessResultAsync()
        {
            //arrange
            MemberManager sut = CreateSut();
            var fakeUser = new MemberIdentityUser(777)
            {
                UserName = "testUser",
                Email = "test@test.com",
                Name = "Test",
                MemberTypeAlias = "Anything",
                PasswordConfig = "testConfig"
            };

            var builder = new MemberTypeBuilder();
            MemberType memberType = builder.BuildSimpleMemberType();

            IMember fakeMember = new Member(memberType)
            {
                Id = 777
            };

            _mockMemberService.Setup(x => x.CreateMember(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(fakeMember);
            _mockMemberService.Setup(x => x.Save(fakeMember, false));

            //act
            IdentityResult identityResult = await sut.CreateAsync(fakeUser);

            //assert
            Assert.IsTrue(identityResult.Succeeded);
            Assert.IsTrue(!identityResult.Errors.Any());
        }
    }
}
