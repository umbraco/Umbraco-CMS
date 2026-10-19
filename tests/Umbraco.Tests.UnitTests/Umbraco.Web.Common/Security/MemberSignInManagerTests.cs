using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Net;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Security;

[TestFixture]
public class MemberSignInManagerTests
{
    private const string TestIpAddress = "192.168.1.100";

    private Mock<ILogger<SignInManager<MemberIdentityUser>>> _mockLogger = null!;
    private Mock<MemberManager> _memberManager = null!;
    private Mock<IEventAggregator> _mockEventAggregator = null!;
    private Mock<IIpResolver> _mockIpResolver = null!;

    public UserClaimsPrincipalFactory<MemberIdentityUser> CreateClaimsFactory(MemberManager userMgr)
        => new(userMgr, Options.Create(new IdentityOptions()));

    public MemberSignInManager CreateSut(
        IdentityOptions? identityOptions = null,
        IMemberExternalLoginProviders? externalLoginProviders = null)
    {
        _memberManager = MockMemberManager();
        _mockEventAggregator = new Mock<IEventAggregator>();
        _mockIpResolver = new Mock<IIpResolver>();
        _mockIpResolver.Setup(x => x.GetCurrentRequestIpAddress()).Returns(TestIpAddress);

        // This all needs to be setup because internally aspnet resolves a bunch
        // of services from the HttpContext.RequestServices.
        var serviceProviderFactory = new DefaultServiceProviderFactory();
        var serviceCollection = new ServiceCollection();
        serviceCollection
            .AddLogging()
            .AddAuthentication()
            .AddCookie(IdentityConstants.ApplicationScheme)
            .AddCookie(IdentityConstants.ExternalScheme, o =>
            {
                o.Cookie.Name = IdentityConstants.ExternalScheme;
                o.ExpireTimeSpan = TimeSpan.FromMinutes(5);
            })
            .AddCookie(IdentityConstants.TwoFactorUserIdScheme, o =>
            {
                o.Cookie.Name = IdentityConstants.TwoFactorUserIdScheme;
                o.ExpireTimeSpan = TimeSpan.FromMinutes(5);
            })
            .AddCookie(IdentityConstants.TwoFactorRememberMeScheme, o =>
            {
                o.Cookie.Name = IdentityConstants.TwoFactorRememberMeScheme;
                o.ExpireTimeSpan = TimeSpan.FromMinutes(5);
            });
        var serviceProvider = serviceProviderFactory.CreateServiceProvider(serviceCollection);
        var httpContextFactory = new DefaultHttpContextFactory(serviceProvider);
        var features = new DefaultHttpContext().Features;
        features.Set<IHttpConnectionFeature>(new HttpConnectionFeature { LocalIpAddress = IPAddress.Parse("127.0.0.1") });
        var httpContext = httpContextFactory.Create(features);

        _mockLogger = new Mock<ILogger<SignInManager<MemberIdentityUser>>>();
        return new MemberSignInManager(
            _memberManager.Object,
            Mock.Of<IHttpContextAccessor>(x => x.HttpContext == httpContext),
            CreateClaimsFactory(_memberManager.Object),
            identityOptions is not null
                ? Options.Create(identityOptions)
                : Mock.Of<IOptions<IdentityOptions>>(),
            _mockLogger.Object,
            Mock.Of<IAuthenticationSchemeProvider>(),
            Mock.Of<IUserConfirmation<MemberIdentityUser>>(),
            externalLoginProviders ?? Mock.Of<IMemberExternalLoginProviders>(),
            _mockEventAggregator.Object,
            Mock.Of<IOptions<SecuritySettings>>(x => x.Value == new SecuritySettings()),
            new DictionaryAppCache(),
            _mockIpResolver.Object);
    }

    private static Mock<MemberManager> MockMemberManager()
        => new(
            Mock.Of<IIpResolver>(),
            Mock.Of<IMemberUserStore>(),
            Options.Create(new IdentityOptions()),
            Mock.Of<IPasswordHasher<MemberIdentityUser>>(),
            Enumerable.Empty<IUserValidator<MemberIdentityUser>>(),
            Enumerable.Empty<IPasswordValidator<MemberIdentityUser>>(),
            new MembersErrorDescriber(Mock.Of<ILocalizedTextService>()),
            Mock.Of<IServiceProvider>(),
            Mock.Of<ILogger<UserManager<MemberIdentityUser>>>(),
            new TestOptionsSnapshot<MemberPasswordConfigurationSettings>(new MemberPasswordConfigurationSettings()),
            Mock.Of<IPublicAccessService>(),
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IPublishedModelFactory>());

    private static IMemberExternalLoginProviders MockExternalLoginProviders(string authenticationType)
    {
        var options = new MemberExternalLoginProviderOptions
        {
            AutoLinkOptions = new MemberExternalSignInAutoLinkOptions(autoLinkExternalAccount: true),
        };
        var scheme = new MemberExternalLoginProviderScheme(
            new MemberExternalLoginProvider(
                authenticationType,
                Mock.Of<IOptionsMonitor<MemberExternalLoginProviderOptions>>(x => x.Get(authenticationType) == options)),
            new AuthenticationScheme(authenticationType, authenticationType, typeof(IAuthenticationHandler)));

        return Mock.Of<IMemberExternalLoginProviders>(x =>
            x.GetAsync(authenticationType) == Task.FromResult<MemberExternalLoginProviderScheme?>(scheme));
    }

    private static ExternalLoginInfo CreateExternalLoginInfo(string authenticationType, params Claim[] claims)
        => new(
            new ClaimsPrincipal(new ClaimsIdentity(claims)),
            authenticationType,
            "provider-key",
            authenticationType);

    [Test]
    public async Task
        WhenPasswordSignInAsyncIsCalled_AndEverythingIsSetup_ThenASignInResultSucceededShouldBeReturnedAsync()
    {
        // Arrange
        var userId = "bo8w3d32q9b98";
        var sut = CreateSut();
        var fakeUser = new MemberIdentityUser(777) { UserName = "TestUser" };
        var password = "testPassword";
        var lockoutOnFailure = false;
        var isPersistent = true;

        _memberManager.Setup(x => x.GetUserIdAsync(It.IsAny<MemberIdentityUser>())).ReturnsAsync(userId);
        _memberManager.Setup(x => x.GetUserNameAsync(It.IsAny<MemberIdentityUser>())).ReturnsAsync(fakeUser.UserName);
        _memberManager.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(fakeUser);
        _memberManager.Setup(x => x.CheckPasswordAsync(fakeUser, password)).ReturnsAsync(true);
        _memberManager.Setup(x => x.IsEmailConfirmedAsync(fakeUser)).ReturnsAsync(true);
        _memberManager.Setup(x => x.IsLockedOutAsync(fakeUser)).ReturnsAsync(false);

        // Act
        var actual = await sut.PasswordSignInAsync(fakeUser, password, isPersistent, lockoutOnFailure);

        // Assert
        Assert.IsTrue(actual.Succeeded);
    }

    [Test]
    public async Task WhenPasswordSignInAsyncIsCalled_AndTheResultFails_ThenASignInFailedResultShouldBeReturnedAsync()
    {
        // Arrange
        var sut = CreateSut();
        var fakeUser = new MemberIdentityUser(777) { UserName = "TestUser" };
        var password = "testPassword";
        var lockoutOnFailure = false;
        var isPersistent = true;

        // Act
        var actual = await sut.PasswordSignInAsync(fakeUser, password, isPersistent, lockoutOnFailure);

        // Assert
        Assert.IsFalse(actual.Succeeded);
    }

    [Test]
    public async Task Can_Publish_Login_Success_Notification()
    {
        // Arrange
        var memberKey = Guid.NewGuid();
        var sut = CreateSut();
        var fakeUser = new MemberIdentityUser(777) { UserName = "TestUser", Key = memberKey };

        _memberManager.Setup(x => x.GetUserIdAsync(It.IsAny<MemberIdentityUser>())).ReturnsAsync("test-user-id");
        _memberManager.Setup(x => x.GetUserNameAsync(It.IsAny<MemberIdentityUser>())).ReturnsAsync(fakeUser.UserName);
        _memberManager.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(fakeUser);
        _memberManager.Setup(x => x.CheckPasswordAsync(fakeUser, "password")).ReturnsAsync(true);
        _memberManager.Setup(x => x.IsEmailConfirmedAsync(fakeUser)).ReturnsAsync(true);
        _memberManager.Setup(x => x.IsLockedOutAsync(fakeUser)).ReturnsAsync(false);

        // Act
        await sut.PasswordSignInAsync(fakeUser, "password", false, false);

        // Assert
        _mockEventAggregator.Verify(
            x => x.Publish(It.Is<MemberLoginSuccessNotification>(n =>
                n.IpAddress == TestIpAddress && n.MemberKey == memberKey)),
            Times.Once);
    }

    [Test]
    public async Task Can_Publish_Login_Failed_Notification_When_Password_Wrong()
    {
        // Arrange
        var memberKey = Guid.NewGuid();
        var sut = CreateSut();
        var fakeUser = new MemberIdentityUser(777) { UserName = "TestUser", Key = memberKey };

        // Act
        await sut.PasswordSignInAsync(fakeUser, "wrong_password", false, false);

        // Assert
        _mockEventAggregator.Verify(
            x => x.Publish(It.Is<MemberLoginFailedNotification>(n =>
                n.IpAddress == TestIpAddress
                && n.MemberKey == memberKey
                && n.Reason == MemberLoginFailedReason.InvalidCredentials)),
            Times.Once);
    }

    [Test]
    public async Task Can_Publish_Login_Failed_Notification_When_User_Not_Found()
    {
        // Arrange
        var sut = CreateSut();

        // FindByNameAsync returns null by default on the fresh mock, so the
        // string overload of PasswordSignInAsync calls HandleSignIn with a null user.

        // Act
        await sut.PasswordSignInAsync("nonexistent_user", "password", false, false);

        // Assert
        _mockEventAggregator.Verify(
            x => x.Publish(It.Is<MemberLoginFailedNotification>(n =>
                n.IpAddress == TestIpAddress
                && n.MemberKey == null
                && n.Reason == MemberLoginFailedReason.MemberNotFound)),
            Times.Once);
    }

    [Test]
    public async Task Can_Publish_Login_Failed_Notification_When_Locked_Out()
    {
        // Arrange
        var memberKey = Guid.NewGuid();
        var sut = CreateSut();
        var fakeUser = new MemberIdentityUser(777) { UserName = "TestUser", Key = memberKey };

        _memberManager.Setup(x => x.SupportsUserLockout).Returns(true);
        _memberManager.Setup(x => x.IsLockedOutAsync(fakeUser)).ReturnsAsync(true);

        // Act
        await sut.PasswordSignInAsync(fakeUser, "password", false, false);

        // Assert
        _mockEventAggregator.Verify(
            x => x.Publish(It.Is<MemberLoginFailedNotification>(n =>
                n.IpAddress == TestIpAddress
                && n.MemberKey == memberKey
                && n.Reason == MemberLoginFailedReason.LockedOut)),
            Times.Once);
    }

    [Test]
    public async Task Can_Publish_Login_Failed_Notification_When_Not_Allowed()
    {
        // Arrange
        var memberKey = Guid.NewGuid();
        var identityOptions = new IdentityOptions { SignIn = { RequireConfirmedEmail = true } };
        var sut = CreateSut(identityOptions);
        var fakeUser = new MemberIdentityUser(777) { UserName = "TestUser", Key = memberKey };

        // IsEmailConfirmedAsync returns false by default on the mock,
        // so CanSignInAsync returns false → PreSignInCheck returns NotAllowed.

        // Act
        await sut.PasswordSignInAsync(fakeUser, "password", false, false);

        // Assert
        _mockEventAggregator.Verify(
            x => x.Publish(It.Is<MemberLoginFailedNotification>(n =>
                n.IpAddress == TestIpAddress
                && n.MemberKey == memberKey
                && n.Reason == MemberLoginFailedReason.NotAllowed)),
            Times.Once);
    }

    [Test]
    public async Task Cannot_Auto_Link_External_Login_Without_Email_Claim()
    {
        // Arrange
        const string provider = "TestProvider";
        var sut = CreateSut(externalLoginProviders: MockExternalLoginProviders(provider));

        // Act
        var actual = await sut.ExternalLoginSignInAsync(CreateExternalLoginInfo(provider), false);

        // Assert
        Assert.AreSame(MemberSignInManager.AutoLinkSignInResult.FailedNoEmail, actual);
    }

    [Test]
    public async Task Cannot_Auto_Link_External_Login_Without_Name_Claim()
    {
        // Arrange
        const string provider = "TestProvider";
        var sut = CreateSut(externalLoginProviders: MockExternalLoginProviders(provider));

        // The principal carries an email but nothing resolving to ClaimTypes.Name, so auto-linking
        // gets as far as creating a new member and finds no name to create it with.
        var loginInfo = CreateExternalLoginInfo(provider, new Claim(ClaimTypes.Email, "test@example.com"));

        // Act
        var actual = await sut.ExternalLoginSignInAsync(loginInfo, false);

        // Assert
        Assert.AreSame(MemberSignInManager.AutoLinkSignInResult.FailedNoName, actual);
    }

    [Test]
    public async Task Can_Publish_Logout_Success_Notification()
    {
        // Arrange
        var memberKey = Guid.NewGuid();
        var sut = CreateSut();
        var fakeUser = new MemberIdentityUser(777) { UserName = "TestUser", Key = memberKey };

        _memberManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(fakeUser);

        // Act
        await sut.SignOutAsync();

        // Assert
        _mockEventAggregator.Verify(
            x => x.Publish(It.Is<MemberLogoutSuccessNotification>(n =>
                n.IpAddress == TestIpAddress && n.MemberKey == memberKey)),
            Times.Once);
    }

    [Test]
    public async Task Cannot_Publish_Logout_Notification_When_User_Not_Found()
    {
        // Arrange
        var sut = CreateSut();

        // GetUserAsync returns null by default on the fresh mock.

        // Act
        await sut.SignOutAsync();

        // Assert
        _mockEventAggregator.Verify(
            x => x.Publish(It.IsAny<MemberLogoutSuccessNotification>()),
            Times.Never);
    }
}
