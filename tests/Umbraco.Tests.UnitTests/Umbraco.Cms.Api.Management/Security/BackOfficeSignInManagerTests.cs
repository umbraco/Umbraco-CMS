// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Security;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Net;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Security;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Security;

[TestFixture]
public class BackOfficeSignInManagerTests
{
    private const string TestProvider = "TestProvider";
    private const string TestEmail = "test@example.com";

    private Mock<BackOfficeUserManager> _userManager = null!;

    [Test]
    public async Task Cannot_Auto_Link_External_Login_When_Auto_Linking_Is_Not_Enabled()
    {
        // Arrange
        BackOfficeSignInManager sut = CreateSut(new ExternalSignInAutoLinkOptions(autoLinkExternalAccount: false));

        // Act
        SignInResult actual = await sut.ExternalLoginSignInAsync(CreateExternalLoginInfo(), false);

        // Assert
        Assert.AreSame(SignInResult.Failed, actual);
    }

    [Test]
    public async Task Cannot_Auto_Link_External_Login_Without_Email_Claim()
    {
        // Arrange
        BackOfficeSignInManager sut = CreateSut();

        // Act
        SignInResult actual = await sut.ExternalLoginSignInAsync(CreateExternalLoginInfo(), false);

        // Assert
        Assert.AreSame(AutoLinkSignInResult.FailedNoEmail, actual);
    }

    [Test]
    public async Task Cannot_Auto_Link_External_Login_Without_Name_Claim()
    {
        // Arrange
        BackOfficeSignInManager sut = CreateSut();

        // The principal carries an email but nothing resolving to ClaimTypes.Name, so auto-linking
        // gets as far as creating a new user and finds no name to create it with.
        ExternalLoginInfo loginInfo = CreateExternalLoginInfoWithEmail();

        // Act
        SignInResult actual = await sut.ExternalLoginSignInAsync(loginInfo, false);

        // Assert
        Assert.AreSame(AutoLinkSignInResult.FailedNoName, actual);
    }

    [Test]
    public async Task Cannot_Auto_Link_External_Login_When_On_Auto_Linking_Callback_Throws()
    {
        // Arrange
        var autoLinkOptions = new ExternalSignInAutoLinkOptions(autoLinkExternalAccount: true)
        {
            OnAutoLinking = (_, _) => throw new InvalidOperationException("callback blew up"),
        };
        BackOfficeSignInManager sut = CreateSut(autoLinkOptions);
        _userManager.Setup(x => x.FindByEmailAsync(TestEmail)).ReturnsAsync(CreateUser());

        // Act
        SignInResult actual = await sut.ExternalLoginSignInAsync(CreateExternalLoginInfoWithEmail(), false);

        // Assert
        Assert.IsInstanceOf<AutoLinkSignInResult>(actual);
        Assert.AreEqual("callback blew up", ((AutoLinkSignInResult)actual).Errors.Single());
    }

    [Test]
    public async Task Cannot_Auto_Link_External_Login_When_On_External_Login_Callback_Refuses()
    {
        // Arrange
        var autoLinkOptions = new ExternalSignInAutoLinkOptions(autoLinkExternalAccount: true)
        {
            OnExternalLogin = (_, _) => false,
        };
        BackOfficeSignInManager sut = CreateSut(autoLinkOptions);
        _userManager.Setup(x => x.FindByEmailAsync(TestEmail)).ReturnsAsync(CreateUser());

        // Act
        SignInResult actual = await sut.ExternalLoginSignInAsync(CreateExternalLoginInfoWithEmail(), false);

        // Assert
        Assert.AreSame(ExternalLoginSignInResult.NotAllowed, actual);
    }

    [Test]
    public async Task Cannot_Auto_Link_External_Login_When_User_Creation_Fails()
    {
        // Arrange
        BackOfficeSignInManager sut = CreateSut();
        _userManager
            .Setup(x => x.CreateAsync(It.IsAny<BackOfficeIdentityUser>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "cannot create" }));

        // Act
        SignInResult actual = await sut.ExternalLoginSignInAsync(
            CreateExternalLoginInfoWithEmail(new Claim(ClaimTypes.Name, "Test User")),
            false);

        // Assert
        Assert.IsInstanceOf<AutoLinkSignInResult>(actual);
        Assert.AreEqual("cannot create", ((AutoLinkSignInResult)actual).Errors.Single());
    }

    /// <remarks>
    ///     A user that could not be linked is left behind in an inconsistent state, so the back office
    ///     deletes it again. This differs from members, which are disapproved rather than deleted.
    /// </remarks>
    [Test]
    public async Task Can_Delete_Auto_Linked_User_When_Linking_The_External_Login_Fails()
    {
        // Arrange
        BackOfficeSignInManager sut = CreateSut();
        BackOfficeIdentityUser existingUser = CreateUser();
        _userManager.Setup(x => x.FindByEmailAsync(TestEmail)).ReturnsAsync(existingUser);
        _userManager.Setup(x => x.GetLoginsAsync(existingUser)).ReturnsAsync([]);
        _userManager
            .Setup(x => x.AddLoginAsync(existingUser, It.IsAny<UserLoginInfo>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "cannot link" }));
        _userManager.Setup(x => x.DeleteAsync(existingUser)).ReturnsAsync(IdentityResult.Success);

        // Act
        SignInResult actual = await sut.ExternalLoginSignInAsync(CreateExternalLoginInfoWithEmail(), false);

        // Assert
        Assert.IsInstanceOf<AutoLinkSignInResult>(actual);
        Assert.AreEqual("cannot link", ((AutoLinkSignInResult)actual).Errors.Single());
        _userManager.Verify(x => x.DeleteAsync(existingUser), Times.Once);
    }

    [Test]
    public async Task Can_Report_Both_Failures_When_Deleting_An_Unlinkable_User_Also_Fails()
    {
        // Arrange
        BackOfficeSignInManager sut = CreateSut();
        BackOfficeIdentityUser existingUser = CreateUser();
        _userManager.Setup(x => x.FindByEmailAsync(TestEmail)).ReturnsAsync(existingUser);
        _userManager.Setup(x => x.GetLoginsAsync(existingUser)).ReturnsAsync([]);
        _userManager
            .Setup(x => x.AddLoginAsync(existingUser, It.IsAny<UserLoginInfo>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "cannot link" }));
        _userManager
            .Setup(x => x.DeleteAsync(existingUser))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "cannot delete" }));

        // Act
        SignInResult actual = await sut.ExternalLoginSignInAsync(CreateExternalLoginInfoWithEmail(), false);

        // Assert
        Assert.IsInstanceOf<AutoLinkSignInResult>(actual);
        CollectionAssert.AreEquivalent(
            new[] { "cannot link", "cannot delete" },
            ((AutoLinkSignInResult)actual).Errors);
    }

    private BackOfficeSignInManager CreateSut(ExternalSignInAutoLinkOptions? autoLinkOptions = null)
    {
        _userManager = MockUserManager();

        return new BackOfficeSignInManager(
            _userManager.Object,
            Mock.Of<IHttpContextAccessor>(),
            MockExternalLoginProviders(autoLinkOptions),
            Mock.Of<IUserClaimsPrincipalFactory<BackOfficeIdentityUser>>(),
            Options.Create(new IdentityOptions()),
            Options.Create(new GlobalSettings()),
            Mock.Of<ILogger<SignInManager<BackOfficeIdentityUser>>>(),
            Mock.Of<IAuthenticationSchemeProvider>(),
            Mock.Of<IUserConfirmation<BackOfficeIdentityUser>>(),
            Mock.Of<IEventAggregator>(),
            Options.Create(new SecuritySettings()),
            Options.Create(new BackOfficeAuthenticationTypeSettings()),
            Mock.Of<IRequestCache>());
    }

    private static Mock<BackOfficeUserManager> MockUserManager()
        => new(
            Mock.Of<IIpResolver>(),
            Mock.Of<IUserStore<BackOfficeIdentityUser>>(),
            Options.Create(new BackOfficeIdentityOptions()),
            Mock.Of<IPasswordHasher<BackOfficeIdentityUser>>(),
            Enumerable.Empty<IUserValidator<BackOfficeIdentityUser>>(),
            Enumerable.Empty<IPasswordValidator<BackOfficeIdentityUser>>(),
            new Mock<BackOfficeErrorDescriber>(Mock.Of<ILocalizedTextService>()).Object,
            Mock.Of<IServiceProvider>(),
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<ILogger<UserManager<BackOfficeIdentityUser>>>(),
            Options.Create(new UserPasswordConfigurationSettings()),
            Mock.Of<IEventAggregator>(),
            Mock.Of<IBackOfficeUserPasswordChecker>(),
            Options.Create(new GlobalSettings()));

    private static BackOfficeIdentityUser CreateUser()
        => BackOfficeIdentityUser.CreateNew(new GlobalSettings(), TestEmail, TestEmail, "en-US", "Test User");

    private static IBackOfficeExternalLoginProviders MockExternalLoginProviders(ExternalSignInAutoLinkOptions? autoLinkOptions)
    {
        var options = new BackOfficeExternalLoginProviderOptions
        {
            AutoLinkOptions = autoLinkOptions ?? new ExternalSignInAutoLinkOptions(autoLinkExternalAccount: true),
        };
        var scheme = new BackOfficeExternaLoginProviderScheme(
            new BackOfficeExternalLoginProvider(
                TestProvider,
                Mock.Of<IOptionsMonitor<BackOfficeExternalLoginProviderOptions>>(x => x.Get(TestProvider) == options)),
            new AuthenticationScheme(TestProvider, TestProvider, typeof(IAuthenticationHandler)));

        return Mock.Of<IBackOfficeExternalLoginProviders>(x =>
            x.GetAsync(TestProvider) == Task.FromResult<BackOfficeExternaLoginProviderScheme?>(scheme));
    }

    private static ExternalLoginInfo CreateExternalLoginInfo(params Claim[] claims)
        => new(
            new ClaimsPrincipal(new ClaimsIdentity(claims)),
            TestProvider,
            "provider-key",
            TestProvider);

    private static ExternalLoginInfo CreateExternalLoginInfoWithEmail(params Claim[] claims)
        => CreateExternalLoginInfo([new Claim(ClaimTypes.Email, TestEmail), .. claims]);
}
