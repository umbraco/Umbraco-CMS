// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Configuration;
using Umbraco.Cms.Api.Management.Security;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Net;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Security;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Configuration;

[TestFixture]
public class ConfigureBackOfficeCookieOptionsTests
{
    private static readonly DateTimeOffset Now = new(2025, 6, 15, 12, 0, 0, TimeSpan.Zero);
    private Mock<TimeProvider> _timeProviderMock = null!;
    private GlobalSettings _globalSettings = null!;
    private SecuritySettings _securitySettings = null!;
    private Mock<BackOfficeSecurityStampValidator> _mockStampValidator = null!;
    private Mock<IBackOfficeSignInManager> _mockSignInManager = null!;

    [SetUp]
    public void SetUp()
    {
        _timeProviderMock = new Mock<TimeProvider>();
        _timeProviderMock.Setup(tp => tp.GetUtcNow()).Returns(Now);
        _globalSettings = new GlobalSettings { TimeOut = TimeSpan.FromMinutes(60) };
        _securitySettings = new SecuritySettings { KeepUserLoggedIn = false };
        _mockSignInManager = new Mock<IBackOfficeSignInManager>();
        _mockStampValidator = CreateMockStampValidator();
    }

    [Test]
    public async Task OnValidatePrincipal_IssuedUtc_Not_Reset_When_No_Renewal_Triggered()
    {
        // Arrange: validator does nothing (ShouldRenew stays false)
        _mockStampValidator
            .Setup(v => v.ValidateAsync(It.IsAny<CookieValidatePrincipalContext>()))
            .Returns(Task.CompletedTask);

        var originalIssuedUtc = Now.AddMinutes(-5);
        var originalExpiresUtc = Now.AddMinutes(55);

        CookieValidatePrincipalContext context = CreateValidatePrincipalContext(originalIssuedUtc, originalExpiresUtc);
        Func<CookieValidatePrincipalContext, Task> onValidatePrincipal = GetOnValidatePrincipal();

        // Act
        await onValidatePrincipal(context);

        // Assert: IssuedUtc should NOT be reset when ShouldRenew was not triggered
        Assert.Multiple(() =>
        {
            Assert.That(context.ShouldRenew, Is.False);
            Assert.That(context.Properties.IssuedUtc, Is.EqualTo(originalIssuedUtc));
            Assert.That(context.Properties.ExpiresUtc, Is.EqualTo(originalExpiresUtc));
        });
    }

    [Test]
    public async Task OnValidatePrincipal_Timestamps_Reset_When_Validator_Triggers_Renewal()
    {
        // Arrange: validator sets ShouldRenew = true (stamp was valid, principal refreshed)
        _mockStampValidator
            .Setup(v => v.ValidateAsync(It.IsAny<CookieValidatePrincipalContext>()))
            .Callback<CookieValidatePrincipalContext>(ctx => ctx.ShouldRenew = true)
            .Returns(Task.CompletedTask);

        var originalIssuedUtc = Now.AddMinutes(-35);
        var originalExpiresUtc = Now.AddMinutes(25);

        CookieValidatePrincipalContext context = CreateValidatePrincipalContext(originalIssuedUtc, originalExpiresUtc);
        Func<CookieValidatePrincipalContext, Task> onValidatePrincipal = GetOnValidatePrincipal();

        // Act
        await onValidatePrincipal(context);

        // Assert: timestamps should be reset to now + TimeOut
        Assert.Multiple(() =>
        {
            Assert.That(context.ShouldRenew, Is.True);
            Assert.That(context.Properties.IssuedUtc, Is.EqualTo(Now));
            Assert.That(context.Properties.ExpiresUtc, Is.EqualTo(Now.Add(_globalSettings.TimeOut)));
        });
    }

    [Test]
    public async Task OnValidatePrincipal_Timestamps_Reset_When_KeepUserLoggedIn_Triggers_Renewal()
    {
        // Arrange: KeepUserLoggedIn = true, and timeRemaining < timeElapsed
        _securitySettings.KeepUserLoggedIn = true;

        _mockStampValidator
            .Setup(v => v.ValidateAsync(It.IsAny<CookieValidatePrincipalContext>()))
            .Returns(Task.CompletedTask);

        // Set IssuedUtc far enough in the past that timeRemaining < timeElapsed
        // IssuedUtc = now - 40 min, ExpiresUtc = now + 20 min
        // timeElapsed = 40 min, timeRemaining = 20 min => timeRemaining < timeElapsed => ShouldRenew
        var originalIssuedUtc = Now.AddMinutes(-40);
        var originalExpiresUtc = Now.AddMinutes(20);

        CookieValidatePrincipalContext context = CreateValidatePrincipalContext(originalIssuedUtc, originalExpiresUtc);
        Func<CookieValidatePrincipalContext, Task> onValidatePrincipal = GetOnValidatePrincipal();

        // Act
        await onValidatePrincipal(context);

        // Assert: ShouldRenew set by EnsureTicketRenewalIfKeepUserLoggedIn, timestamps reset
        Assert.Multiple(() =>
        {
            Assert.That(context.ShouldRenew, Is.True);
            Assert.That(context.Properties.IssuedUtc, Is.EqualTo(Now));
            Assert.That(context.Properties.ExpiresUtc, Is.EqualTo(Now.Add(_globalSettings.TimeOut)));
        });
    }

    [Test]
    public async Task OnValidatePrincipal_No_Renewal_When_KeepUserLoggedIn_But_TimeRemaining_Greater_Than_TimeElapsed()
    {
        // Arrange: KeepUserLoggedIn = true, but timeRemaining > timeElapsed
        _securitySettings.KeepUserLoggedIn = true;

        _mockStampValidator
            .Setup(v => v.ValidateAsync(It.IsAny<CookieValidatePrincipalContext>()))
            .Returns(Task.CompletedTask);

        // IssuedUtc = now - 10 min, ExpiresUtc = now + 50 min
        // timeElapsed = 10 min, timeRemaining = 50 min => timeRemaining > timeElapsed => no renewal
        var originalIssuedUtc = Now.AddMinutes(-10);
        var originalExpiresUtc = Now.AddMinutes(50);

        CookieValidatePrincipalContext context = CreateValidatePrincipalContext(originalIssuedUtc, originalExpiresUtc);
        Func<CookieValidatePrincipalContext, Task> onValidatePrincipal = GetOnValidatePrincipal();

        // Act
        await onValidatePrincipal(context);

        // Assert: ShouldRenew stays false, timestamps unchanged
        Assert.Multiple(() =>
        {
            Assert.That(context.ShouldRenew, Is.False);
            Assert.That(context.Properties.IssuedUtc, Is.EqualTo(originalIssuedUtc));
            Assert.That(context.Properties.ExpiresUtc, Is.EqualTo(originalExpiresUtc));
        });
    }

    private Func<CookieValidatePrincipalContext, Task> GetOnValidatePrincipal()
    {
        var sut = new ConfigureBackOfficeCookieOptions(
            Options.Create(_securitySettings),
            Options.Create(_globalSettings),
            Mock.Of<IRuntimeState>(x => x.Level == RuntimeLevel.Run),
            CreateMockDataProtectionProvider(),
            Mock.Of<IUserService>(),
            Mock.Of<IIpResolver>(),
            _timeProviderMock.Object);

        var options = new CookieAuthenticationOptions();
        sut.Configure(Constants.Security.BackOfficeAuthenticationType, options);
        return options.Events.OnValidatePrincipal;
    }

    private CookieValidatePrincipalContext CreateValidatePrincipalContext(
        DateTimeOffset issuedUtc,
        DateTimeOffset expiresUtc)
    {
        ClaimsPrincipal principal = CreateBackOfficePrincipal();

        var properties = new AuthenticationProperties
        {
            IssuedUtc = issuedUtc,
            ExpiresUtc = expiresUtc,
        };

        var ticket = new AuthenticationTicket(principal, properties, Constants.Security.BackOfficeAuthenticationType);

        var services = new ServiceCollection();
        services.AddSingleton(_mockStampValidator.Object);
        services.AddSingleton(_mockSignInManager.Object);
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };

        var scheme = new AuthenticationScheme(
            Constants.Security.BackOfficeAuthenticationType,
            Constants.Security.BackOfficeAuthenticationType,
            typeof(CookieAuthenticationHandler));

        return new CookieValidatePrincipalContext(
            httpContext,
            scheme,
            new CookieAuthenticationOptions(),
            ticket);
    }

    private static ClaimsPrincipal CreateBackOfficePrincipal()
    {
        var identity = new ClaimsIdentity(
            Constants.Security.BackOfficeAuthenticationType,
            ClaimTypes.Name,
            ClaimTypes.Role);

        // Add required back office claims (see ClaimsIdentityExtensions.RequiredBackOfficeClaimTypes)
        identity.AddClaims(
        [
            new Claim(ClaimTypes.NameIdentifier, "1234", ClaimValueTypes.String, Constants.Security.BackOfficeAuthenticationType),
            new Claim(ClaimTypes.Name, "admin@example.com", ClaimValueTypes.String, Constants.Security.BackOfficeAuthenticationType),
            new Claim(ClaimTypes.GivenName, "Admin", ClaimValueTypes.String, Constants.Security.BackOfficeAuthenticationType),
            new Claim(ClaimTypes.Locality, "en-US", ClaimValueTypes.String, Constants.Security.BackOfficeAuthenticationType),
            new Claim(Constants.Security.SecurityStampClaimType, Guid.NewGuid().ToString(), ClaimValueTypes.String, Constants.Security.BackOfficeAuthenticationType),
            new Claim(Constants.Security.SessionIdClaimType, Guid.NewGuid().ToString(), ClaimValueTypes.String, Constants.Security.BackOfficeAuthenticationType),
        ]);

        return new ClaimsPrincipal(identity);
    }

    private static IDataProtectionProvider CreateMockDataProtectionProvider()
    {
        var mockProtector = new Mock<IDataProtector>();
        mockProtector
            .Setup(p => p.CreateProtector(It.IsAny<string>()))
            .Returns(mockProtector.Object);

        var mockProvider = new Mock<IDataProtectionProvider>();
        mockProvider
            .Setup(p => p.CreateProtector(It.IsAny<string>()))
            .Returns(mockProtector.Object);

        return mockProvider.Object;
    }

    private static Mock<BackOfficeSecurityStampValidator> CreateMockStampValidator()
    {
        // Build up the mock chain needed by BackOfficeSecurityStampValidator's constructor.
        // None of the inner dependencies are actually called - only ValidateAsync is invoked
        // (and it's overridden by Moq).
        BackOfficeUserManager userManager = CreateMockUserManager();
        BackOfficeSignInManager signInManager = CreateMockSignInManager(userManager);

        var mockValidator = new Mock<BackOfficeSecurityStampValidator>(
            Options.Create(new BackOfficeSecurityStampValidatorOptions()),
            signInManager,
            Mock.Of<ILoggerFactory>());

        return mockValidator;
    }

    private static BackOfficeUserManager CreateMockUserManager()
    {
        var mockTextService = Mock.Of<ILocalizedTextService>();
        var errorDescriber = new Mock<BackOfficeErrorDescriber>(mockTextService);

        var mock = new Mock<BackOfficeUserManager>(
            Mock.Of<IIpResolver>(),
            Mock.Of<IUserStore<BackOfficeIdentityUser>>(),
            Options.Create(new BackOfficeIdentityOptions()),
            Mock.Of<IPasswordHasher<BackOfficeIdentityUser>>(),
            Enumerable.Empty<IUserValidator<BackOfficeIdentityUser>>(),
            Enumerable.Empty<IPasswordValidator<BackOfficeIdentityUser>>(),
            errorDescriber.Object,
            Mock.Of<IServiceProvider>(),
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<ILogger<UserManager<BackOfficeIdentityUser>>>(),
            Options.Create(new UserPasswordConfigurationSettings()),
            Mock.Of<IEventAggregator>(),
            Mock.Of<IBackOfficeUserPasswordChecker>(),
            Options.Create(new GlobalSettings()));

        return mock.Object;
    }

    private static BackOfficeSignInManager CreateMockSignInManager(BackOfficeUserManager userManager)
    {
        var mock = new Mock<BackOfficeSignInManager>(
            userManager,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IBackOfficeExternalLoginProviders>(),
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

        return mock.Object;
    }
}
