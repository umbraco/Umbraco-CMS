using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.BackOffice;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Extensions;
using Umbraco.Tests.Common.Builders;

namespace Umbraco.Tests.UnitTests.Umbraco.Core.BackOffice
{
    [TestFixture]
    public class BackOfficeClaimsPrincipalFactoryTests
    {
        private const int _testUserId = 2;
        private const string _testUserName = "bob";
        private const string _testUserGivenName = "Bob";
        private const string _testUserCulture = "en-US";
        private const string _testUserSecurityStamp = "B6937738-9C17-4C7D-A25A-628A875F5177";
        private BackOfficeIdentityUser _testUser;
        private Mock<UserManager<BackOfficeIdentityUser>> _mockUserManager;

        [Test]
        public void Ctor_When_UserManager_Is_Null_Expect_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new BackOfficeClaimsPrincipalFactory<BackOfficeIdentityUser>(
                null,
                new OptionsWrapper<BackOfficeIdentityOptions>(new BackOfficeIdentityOptions())));
        }

        [Test]
        public void Ctor_When_Options_Are_Null_Expect_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new BackOfficeClaimsPrincipalFactory<BackOfficeIdentityUser>(
                new Mock<UserManager<BackOfficeIdentityUser>>(new Mock<IUserStore<BackOfficeIdentityUser>>().Object,
                    null, null, null, null, null, null, null, null).Object,
                null));
        }

        [Test]
        public void Ctor_When_Options_Value_Is_Null_Expect_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new BackOfficeClaimsPrincipalFactory<BackOfficeIdentityUser>(
                new Mock<UserManager<BackOfficeIdentityUser>>(new Mock<IUserStore<BackOfficeIdentityUser>>().Object,
                    null, null, null, null, null, null, null, null).Object,
                new OptionsWrapper<BackOfficeIdentityOptions>(null)));
        }

        [Test]
        public void CreateAsync_When_User_Is_Null_Expect_ArgumentNullException()
        {
            var sut = CreateSut();

            Assert.ThrowsAsync<ArgumentNullException>(async () => await sut.CreateAsync(null));
        }

        [Test]
        public async Task CreateAsync_Should_Create_Principal_With_Umbraco_Identity()
        {
            var sut = CreateSut();

            var claimsPrincipal = await sut.CreateAsync(_testUser);

            var umbracoBackOfficeIdentity = claimsPrincipal.Identity as UmbracoBackOfficeIdentity;
            Assert.IsNotNull(umbracoBackOfficeIdentity);
        }

        [TestCase(ClaimTypes.NameIdentifier, _testUserId)]
        [TestCase(ClaimTypes.Name, _testUserName)]
        public async Task CreateAsync_Should_Include_Claim(string expectedClaimType, object expectedClaimValue)
        {
            var sut = CreateSut();

            var claimsPrincipal = await sut.CreateAsync(_testUser);

            Assert.True(claimsPrincipal.HasClaim(expectedClaimType, expectedClaimValue.ToString()));
            Assert.True(claimsPrincipal.GetUmbracoIdentity().HasClaim(expectedClaimType, expectedClaimValue.ToString()));
        }

        [Test]
        public async Task CreateAsync_When_SecurityStamp_Supported_Expect_SecurityStamp_Claim()
        {
            const string expectedClaimType = Constants.Security.SecurityStampClaimType;
            var expectedClaimValue = _testUser.SecurityStamp;

            _mockUserManager.Setup(x => x.SupportsUserSecurityStamp).Returns(true);
            _mockUserManager.Setup(x => x.GetSecurityStampAsync(_testUser)).ReturnsAsync(_testUser.SecurityStamp);

            var sut = CreateSut();

            var claimsPrincipal = await sut.CreateAsync(_testUser);

            Assert.True(claimsPrincipal.HasClaim(expectedClaimType, expectedClaimValue));
            Assert.True(claimsPrincipal.GetUmbracoIdentity().HasClaim(expectedClaimType, expectedClaimValue));
        }

        [Test]
        public async Task CreateAsync_When_Roles_Supported_Expect_Role_Claims_In_UmbracoIdentity()
        {
            const string expectedClaimType = ClaimTypes.Role;
            const string expectedClaimValue = "b87309fb-4caf-48dc-b45a-2b752d051508";

            _testUser.Roles.Add(new global::Umbraco.Core.Models.Identity.IdentityUserRole<string>{RoleId = expectedClaimValue});
            _mockUserManager.Setup(x => x.SupportsUserRole).Returns(true);
            _mockUserManager.Setup(x => x.GetRolesAsync(_testUser)).ReturnsAsync(new[] {expectedClaimValue});

            var sut = CreateSut();

            var claimsPrincipal = await sut.CreateAsync(_testUser);

            Assert.True(claimsPrincipal.HasClaim(expectedClaimType, expectedClaimValue));
        }

        [Test]
        public async Task CreateAsync_When_UserClaims_Supported_Expect_UserClaims_In_Actor()
        {
            const string expectedClaimType = "custom";
            const string expectedClaimValue = "val";

            _testUser.Claims.Add(new global::Umbraco.Core.Models.Identity.IdentityUserClaim<int> {ClaimType = expectedClaimType, ClaimValue = expectedClaimValue});
            _mockUserManager.Setup(x => x.SupportsUserClaim).Returns(true);
            _mockUserManager.Setup(x => x.GetClaimsAsync(_testUser)).ReturnsAsync(
                new List<Claim> {new Claim(expectedClaimType, expectedClaimValue)});

            var sut = CreateSut();

            var claimsPrincipal = await sut.CreateAsync(_testUser);

            Assert.True(claimsPrincipal.GetUmbracoIdentity().HasClaim(expectedClaimType, expectedClaimValue));
        }

        [SetUp]
        public void Setup()
        {
            var globalSettings = new GlobalSettings { DefaultUILanguage = "test" };

            _testUser = new BackOfficeIdentityUser(globalSettings, _testUserId, new List<IReadOnlyUserGroup>())
            {
                UserName = _testUserName,
                Name = _testUserGivenName,
                Email = "bob@umbraco.test",
                SecurityStamp = _testUserSecurityStamp,
                Culture = _testUserCulture
            };

            _mockUserManager = new Mock<UserManager<BackOfficeIdentityUser>>(new Mock<IUserStore<BackOfficeIdentityUser>>().Object,
                null, null, null, null, null, null, null, null);
            _mockUserManager.Setup(x => x.GetUserIdAsync(_testUser)).ReturnsAsync(_testUser.Id.ToString);
            _mockUserManager.Setup(x => x.GetUserNameAsync(_testUser)).ReturnsAsync(_testUser.UserName);
            _mockUserManager.Setup(x => x.SupportsUserSecurityStamp).Returns(false);
            _mockUserManager.Setup(x => x.SupportsUserClaim).Returns(false);
            _mockUserManager.Setup(x => x.SupportsUserRole).Returns(false);
        }

        private BackOfficeClaimsPrincipalFactory<BackOfficeIdentityUser> CreateSut()
        {
            return new BackOfficeClaimsPrincipalFactory<BackOfficeIdentityUser>(_mockUserManager.Object,
                new OptionsWrapper<BackOfficeIdentityOptions>(new BackOfficeIdentityOptions()));
        }
    }
}
