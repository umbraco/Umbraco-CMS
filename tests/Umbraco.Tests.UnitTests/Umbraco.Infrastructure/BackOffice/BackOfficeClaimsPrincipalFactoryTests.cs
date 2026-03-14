// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.BackOffice;

/// <summary>
/// Contains unit tests for the <see cref="BackOfficeClaimsPrincipalFactory"/> class, verifying its behavior and functionality.
/// </summary>
[TestFixture]
public class BackOfficeClaimsPrincipalFactoryTests
{
    /// <summary>
    /// Initializes the test user and mocks required for each test in this class.
    /// This method is called before each test is run to ensure a consistent test environment.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        var globalSettings = new GlobalSettings { DefaultUILanguage = "test" };

        _testUser = new BackOfficeIdentityUser(globalSettings, TestUserId, new List<IReadOnlyUserGroup>())
        {
            UserName = TestUserName,
            Name = TestUserGivenName,
            Email = "bob@umbraco.test",
            SecurityStamp = TestUserSecurityStamp,
            Culture = TestUserCulture,
        };

        _mockUserManager = GetMockedUserManager();
        _mockUserManager.Setup(x => x.GetUserIdAsync(_testUser)).ReturnsAsync(_testUser.Id.ToString);
        _mockUserManager.Setup(x => x.GetUserNameAsync(_testUser)).ReturnsAsync(_testUser.UserName);
        _mockUserManager.Setup(x => x.SupportsUserSecurityStamp).Returns(false);
        _mockUserManager.Setup(x => x.SupportsUserClaim).Returns(false);
        _mockUserManager.Setup(x => x.SupportsUserRole).Returns(false);
    }

    private const int TestUserId = 2;
    private const string TestUserName = "bob";
    private const string TestUserGivenName = "Bob";
    private const string TestUserCulture = "en-US";
    private const string TestUserSecurityStamp = "B6937738-9C17-4C7D-A25A-628A875F5177";
    private BackOfficeIdentityUser _testUser;
    private Mock<UserManager<BackOfficeIdentityUser>> _mockUserManager;

    private static Mock<UserManager<BackOfficeIdentityUser>> GetMockedUserManager()
        => new(new Mock<IUserStore<BackOfficeIdentityUser>>().Object, null, null, null, null, null, null, null, null);

    /// <summary>
    /// Tests that the constructor throws an <see cref="ArgumentNullException"/> when the user manager is null.
    /// </summary>
    [Test]
    public void Ctor_When_UserManager_Is_Null_Expect_ArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => new BackOfficeClaimsPrincipalFactory(
            null,
            new OptionsWrapper<BackOfficeIdentityOptions>(new BackOfficeIdentityOptions()),
            new OptionsWrapper<BackOfficeAuthenticationTypeSettings>(new BackOfficeAuthenticationTypeSettings())));

    /// <summary>
    /// Tests that the constructor throws an <see cref="ArgumentNullException"/> when options are null.
    /// </summary>
    [Test]
    public void Ctor_When_Options_Are_Null_Expect_ArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() =>
            new BackOfficeClaimsPrincipalFactory(GetMockedUserManager().Object, null, new OptionsWrapper<BackOfficeAuthenticationTypeSettings>(new BackOfficeAuthenticationTypeSettings())));

    /// <summary>
    /// Tests that the constructor of <see cref="BackOfficeClaimsPrincipalFactory"/> throws an <see cref="ArgumentException"/> when the options value is null.
    /// </summary>
    [Test]
    public void Ctor_When_Options_Value_Is_Null_Expect_ArgumentException()
        => Assert.Throws<ArgumentException>(() => new BackOfficeClaimsPrincipalFactory(
            GetMockedUserManager().Object,
            new OptionsWrapper<BackOfficeIdentityOptions>(null),
            new OptionsWrapper<BackOfficeAuthenticationTypeSettings>(new BackOfficeAuthenticationTypeSettings())));

    /// <summary>
    /// Tests that CreateAsync throws an ArgumentNullException when the user parameter is null.
    /// </summary>
    [Test]
    public void CreateAsync_When_User_Is_Null_Expect_ArgumentNullException()
    {
        var sut = CreateSut();

        Assert.ThrowsAsync<ArgumentNullException>(async () => await sut.CreateAsync(null));
    }

    /// <summary>
    /// Tests that CreateAsync creates a ClaimsPrincipal with a ClaimsIdentity.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CreateAsync_Should_Create_Principal_With_Claims_Identity()
    {
        var sut = CreateSut();

        var claimsPrincipal = await sut.CreateAsync(_testUser);

        var umbracoBackOfficeIdentity = claimsPrincipal.Identity as ClaimsIdentity;
        Assert.IsNotNull(umbracoBackOfficeIdentity);
    }

    /// <summary>
    /// Verifies that the <c>CreateAsync</c> method adds a claim with the specified type and value to the generated <see cref="ClaimsPrincipal"/>.
    /// </summary>
    /// <param name="expectedClaimType">The claim type expected to be present in the resulting principal.</param>
    /// <param name="expectedClaimValue">The claim value expected to be present in the resulting principal.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [TestCase(ClaimTypes.NameIdentifier, TestUserId)]
    [TestCase(ClaimTypes.Name, TestUserName)]
    public async Task CreateAsync_Should_Include_Claim(string expectedClaimType, object expectedClaimValue)
    {
        var sut = CreateSut();

        var claimsPrincipal = await sut.CreateAsync(_testUser);

        Assert.True(claimsPrincipal.HasClaim(expectedClaimType, expectedClaimValue.ToString()));
        Assert.True(claimsPrincipal.GetUmbracoIdentity().HasClaim(expectedClaimType, expectedClaimValue.ToString()));
    }

    /// <summary>
    /// Tests that when the user manager supports security stamps, the created claims principal contains the security stamp claim.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
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

    /// <summary>
    /// Tests that when roles are supported, the created ClaimsPrincipal contains role claims in the Umbraco identity.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CreateAsync_When_Roles_Supported_Expect_Role_Claims_In_UmbracoIdentity()
    {
        const string expectedClaimType = ClaimTypes.Role;
        const string expectedClaimValue = "b87309fb-4caf-48dc-b45a-2b752d051508";

        _testUser.Roles.Add(new IdentityUserRole<string> { RoleId = expectedClaimValue });
        _mockUserManager.Setup(x => x.SupportsUserRole).Returns(true);
        _mockUserManager.Setup(x => x.GetRolesAsync(_testUser)).ReturnsAsync(new[] { expectedClaimValue });

        var sut = CreateSut();

        var claimsPrincipal = await sut.CreateAsync(_testUser);

        Assert.True(claimsPrincipal.HasClaim(expectedClaimType, expectedClaimValue));
    }

    /// <summary>
    /// Tests that when user claims are supported, the created claims principal contains the expected user claims in the actor.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CreateAsync_When_UserClaims_Supported_Expect_UserClaims_In_Actor()
    {
        const string expectedClaimType = "custom";
        const string expectedClaimValue = "val";

        _testUser.Claims.Add(new IdentityUserClaim<string>
        {
            ClaimType = expectedClaimType,
            ClaimValue = expectedClaimValue,
        });
        _mockUserManager.Setup(x => x.SupportsUserClaim).Returns(true);
        _mockUserManager.Setup(x => x.GetClaimsAsync(_testUser)).ReturnsAsync(
            new List<Claim> { new(expectedClaimType, expectedClaimValue) });

        var sut = CreateSut();

        var claimsPrincipal = await sut.CreateAsync(_testUser);

        Assert.True(claimsPrincipal.GetUmbracoIdentity().HasClaim(expectedClaimType, expectedClaimValue));
    }

    private BackOfficeClaimsPrincipalFactory CreateSut() => new(
        _mockUserManager.Object,
        new OptionsWrapper<BackOfficeIdentityOptions>(new BackOfficeIdentityOptions()),
        new OptionsWrapper<BackOfficeAuthenticationTypeSettings>(new BackOfficeAuthenticationTypeSettings()));
}
