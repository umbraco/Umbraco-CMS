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
using Umbraco.Core.Models.Membership;
using Umbraco.Extensions;

namespace Umbraco.Tests.UnitTests.Umbraco.Core.BackOffice
{
    [TestFixture]
    public class BackOfficeClaimsPrincipalFactoryTests
    {
        private BackOfficeIdentityUser _testUser;
        private Mock<UserManager<BackOfficeIdentityUser>> _mockUserManager;

        [Test]
        public void Ctor_When_UserManager_Is_Null_Expect_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new BackOfficeClaimsPrincipalFactory<BackOfficeIdentityUser>(
                null,
                new OptionsWrapper<IdentityOptions>(new IdentityOptions())));
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
                new OptionsWrapper<IdentityOptions>(null)));
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

        [Test]
        public async Task CreateAsync_Should_Create_NameId()
        {
            const string expectedClaimType = ClaimTypes.NameIdentifier;
            var expectedClaimValue = _testUser.Id.ToString();

            var sut = CreateSut();

            var claimsPrincipal = await sut.CreateAsync(_testUser);

            Assert.True(claimsPrincipal.HasClaim(expectedClaimType, expectedClaimValue));
            Assert.True(claimsPrincipal.GetUmbracoIdentity().Actor.HasClaim(expectedClaimType, expectedClaimValue));
        }

        [Test]
        public async Task CreateAsync_Should_Create_Name()
        {
            const string expectedClaimType = ClaimTypes.Name;
            var expectedClaimValue = _testUser.UserName;

            var sut = CreateSut();

            var claimsPrincipal = await sut.CreateAsync(_testUser);

            Assert.True(claimsPrincipal.HasClaim(expectedClaimType, expectedClaimValue));
            Assert.True(claimsPrincipal.GetUmbracoIdentity().Actor.HasClaim(expectedClaimType, expectedClaimValue));
        }

        [Test]
        public async Task CreateAsync_Should_Create_IdentityProvider()
        {
            const string expectedClaimType = "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider";
            const string expectedClaimValue = "ASP.NET Identity";

            var sut = CreateSut();

            var claimsPrincipal = await sut.CreateAsync(_testUser);

            Assert.True(claimsPrincipal.HasClaim(expectedClaimType, expectedClaimValue));
            Assert.True(claimsPrincipal.GetUmbracoIdentity().Actor.HasClaim(expectedClaimType, expectedClaimValue));
        }

        [Test]
        public async Task CreateAsync_When_SecurityStamp_Supported_Expect_SecurityStamp_Claim()
        {
            const string expectedClaimType = Constants.Web.SecurityStampClaimType;
            var expectedClaimValue = _testUser.SecurityStamp;

            _mockUserManager.Setup(x => x.SupportsUserSecurityStamp).Returns(true);
            _mockUserManager.Setup(x => x.GetSecurityStampAsync(_testUser)).ReturnsAsync(_testUser.SecurityStamp);

            var sut = CreateSut();

            var claimsPrincipal = await sut.CreateAsync(_testUser);
            
            Assert.True(claimsPrincipal.HasClaim(expectedClaimType, expectedClaimValue));
            Assert.True(claimsPrincipal.GetUmbracoIdentity().Actor.HasClaim(expectedClaimType, expectedClaimValue));
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

            Assert.True(claimsPrincipal.GetUmbracoIdentity().Actor.HasClaim(expectedClaimType, expectedClaimValue));
        }

        [SetUp]
        public void Setup()
        {
            var mockGlobalSettings = new Mock<IGlobalSettings>();
            mockGlobalSettings.Setup(x => x.DefaultUILanguage).Returns("test");

            _testUser = new BackOfficeIdentityUser(mockGlobalSettings.Object, 2, new List<IReadOnlyUserGroup>())
            {
                UserName = "bob",
                Name = "Bob",
                Email = "bob@umbraco.test",
                SecurityStamp = "B6937738-9C17-4C7D-A25A-628A875F5177"
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
                new OptionsWrapper<IdentityOptions>(new IdentityOptions()));
        }
    }
}
