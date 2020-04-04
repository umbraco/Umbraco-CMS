using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Umbraco.Web.Models.Identity;
using Umbraco.Web.Security;

namespace Umbraco.Tests.Security
{
    [TestFixture]
    public class BackOfficeClaimsPrincipalFactoryTests
    {
        private BackOfficeIdentityUser _testUser;
        private Mock<UserManager<BackOfficeIdentityUser>> _mockUserManager;

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
            var sut = CreateSut();

            var claimsPrincipal = await sut.CreateAsync(_testUser);

            Assert.True(claimsPrincipal.HasClaim(ClaimTypes.NameIdentifier, _testUser.Id.ToString()));
        }

        [Test]
        public async Task CreateAsync_Should_Create_IdentityProvider()
        {
            var sut = CreateSut();

            var claimsPrincipal = await sut.CreateAsync(_testUser);

            Assert.True(claimsPrincipal.HasClaim(
                "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider",
                "ASP.NET Identity"));
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
