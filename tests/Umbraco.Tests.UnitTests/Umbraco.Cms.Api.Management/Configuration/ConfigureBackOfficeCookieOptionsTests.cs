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
using Umbraco.Cms.Api.Common.Security;
using Umbraco.Cms.Api.Management.Configuration;
using Umbraco.Cms.Api.Management.Security;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Net;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Configuration;

[TestFixture]
public class ConfigureBackOfficeCookieOptionsTests
{
    private static readonly DateTimeOffset _now = new(2025, 6, 15, 12, 0, 0, TimeSpan.Zero);
    private Mock<TimeProvider> _timeProviderMock = null!;
    private GlobalSettings _globalSettings = null!;
    private SecuritySettings _securitySettings = null!;
    private Mock<BackOfficeSecurityStampValidator> _mockStampValidator = null!;
    private Mock<IBackOfficeSignInManager> _mockSignInManager = null!;

    [SetUp]
    public void SetUp()
    {
        _timeProviderMock = new Mock<TimeProvider>();
        _timeProviderMock.Setup(tp => tp.GetUtcNow()).Returns(_now);
        _globalSettings = new GlobalSettings { TimeOut = TimeSpan.FromMinutes(60) };
        _securitySettings = new SecuritySettings { KeepUserLoggedIn = false };
        _mockSignInManager = new Mock<IBackOfficeSignInManager>();
        _mockStampValidator = CreateMockStampValidator();
    }

    [Test]
    public async Task Cannot_Reset_Timestamps_When_No_Renewal_Triggered()
    {
        // Arrange: validator does nothing (ShouldRenew stays false)
        _mockStampValidator
            .Setup(v => v.ValidateAsync(It.IsAny<CookieValidatePrincipalContext>()))
            .Returns(Task.CompletedTask);

        var originalIssuedUtc = _now.AddMinutes(-5);
        var originalExpiresUtc = _now.AddMinutes(55);

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
    public async Task Can_Reset_Timestamps_When_Validator_Triggers_Renewal()
    {
        // Arrange: validator sets ShouldRenew = true (stamp was valid, principal refreshed)
        _mockStampValidator
            .Setup(v => v.ValidateAsync(It.IsAny<CookieValidatePrincipalContext>()))
            .Callback<CookieValidatePrincipalContext>(ctx => ctx.ShouldRenew = true)
            .Returns(Task.CompletedTask);

        var originalIssuedUtc = _now.AddMinutes(-35);
        var originalExpiresUtc = _now.AddMinutes(25);

        CookieValidatePrincipalContext context = CreateValidatePrincipalContext(originalIssuedUtc, originalExpiresUtc);
        Func<CookieValidatePrincipalContext, Task> onValidatePrincipal = GetOnValidatePrincipal();

        // Act
        await onValidatePrincipal(context);

        // Assert: timestamps should be reset to now + TimeOut
        Assert.Multiple(() =>
        {
            Assert.That(context.ShouldRenew, Is.True);
            Assert.That(context.Properties.IssuedUtc, Is.EqualTo(_now));
            Assert.That(context.Properties.ExpiresUtc, Is.EqualTo(_now.Add(_globalSettings.TimeOut)));
        });
    }

    [Test]
    public async Task Can_Reset_Timestamps_When_KeepUserLoggedIn_Triggers_Renewal()
    {
        // Arrange: KeepUserLoggedIn = true, and timeRemaining < timeElapsed
        _securitySettings.KeepUserLoggedIn = true;

        _mockStampValidator
            .Setup(v => v.ValidateAsync(It.IsAny<CookieValidatePrincipalContext>()))
            .Returns(Task.CompletedTask);

        // Set IssuedUtc far enough in the past that timeRemaining < timeElapsed
        // IssuedUtc = now - 40 min, ExpiresUtc = now + 20 min
        // timeElapsed = 40 min, timeRemaining = 20 min => timeRemaining < timeElapsed => ShouldRenew
        var originalIssuedUtc = _now.AddMinutes(-40);
        var originalExpiresUtc = _now.AddMinutes(20);

        CookieValidatePrincipalContext context = CreateValidatePrincipalContext(originalIssuedUtc, originalExpiresUtc);
        Func<CookieValidatePrincipalContext, Task> onValidatePrincipal = GetOnValidatePrincipal();

        // Act
        await onValidatePrincipal(context);

        // Assert: ShouldRenew set by EnsureTicketRenewalIfKeepUserLoggedIn, timestamps reset
        Assert.Multiple(() =>
        {
            Assert.That(context.ShouldRenew, Is.True);
            Assert.That(context.Properties.IssuedUtc, Is.EqualTo(_now));
            Assert.That(context.Properties.ExpiresUtc, Is.EqualTo(_now.Add(_globalSettings.TimeOut)));
        });
    }

    [Test]
    public async Task Cannot_Renew_Ticket_When_KeepUserLoggedIn_And_Time_Remaining_Exceeds_Time_Elapsed()
    {
        // Arrange: KeepUserLoggedIn = true, but timeRemaining > timeElapsed
        _securitySettings.KeepUserLoggedIn = true;

        _mockStampValidator
            .Setup(v => v.ValidateAsync(It.IsAny<CookieValidatePrincipalContext>()))
            .Returns(Task.CompletedTask);

        // IssuedUtc = now - 10 min, ExpiresUtc = now + 50 min
        // timeElapsed = 10 min, timeRemaining = 50 min => timeRemaining > timeElapsed => no renewal
        var originalIssuedUtc = _now.AddMinutes(-10);
        var originalExpiresUtc = _now.AddMinutes(50);

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

    [TestCase("Strict", SameSiteMode.Strict)]
    [TestCase("strict", SameSiteMode.Strict)]
    [TestCase("None", SameSiteMode.None)]
    [TestCase("Lax", SameSiteMode.Lax)]
    [TestCase("Unspecified", SameSiteMode.Unspecified)]

    // Numeric input is a legitimate spelling of a defined member and must keep working.
    [TestCase("0", SameSiteMode.None)]
    [TestCase("2", SameSiteMode.Strict)]
    public void Can_Configure_SameSite_From_Defined_AuthCookieSameSite_Value(string configured, SameSiteMode expected)
    {
        _securitySettings.AuthCookieSameSite = configured;

        CookieAuthenticationOptions options = ConfigureOptions();

        Assert.That(options.Cookie.SameSite, Is.EqualTo(expected));
    }

    // An out-of-range integer parses successfully but is not a defined SameSiteMode. Left unguarded it
    // reaches SetCookieHeaderValue, which omits the samesite attribute entirely - silently downgrading
    // from the configured default to whatever the browser falls back to. A configuration mistake must
    // fail loudly, exactly as an unrecognised word does.
    [TestCase("42")]
    [TestCase("-7")]
    [TestCase("Stirct")]
    [TestCase("")]
    public void Cannot_Configure_SameSite_From_Undefined_AuthCookieSameSite_Value(string configured)
    {
        _securitySettings.AuthCookieSameSite = configured;

        Assert.Throws<ConfigurationException>(() => ConfigureOptions());
    }

    [Test]
    public async Task Can_Renew_Ticket_For_KeepAlive_Request_When_KeepUserLoggedIn_Is_Disabled()
    {
        // Arrange: nothing else triggers a renewal - no KeepUserLoggedIn, validator does nothing.
        _mockStampValidator
            .Setup(v => v.ValidateAsync(It.IsAny<CookieValidatePrincipalContext>()))
            .Returns(Task.CompletedTask);

        var originalIssuedUtc = _now.AddMinutes(-10);
        var originalExpiresUtc = _now.AddMinutes(50);

        CookieValidatePrincipalContext context = CreateValidatePrincipalContext(
            originalIssuedUtc,
            originalExpiresUtc,
            Paths.BackOfficeApi.KeepAliveEndpoint);
        Func<CookieValidatePrincipalContext, Task> onValidatePrincipal = GetOnValidatePrincipal();

        // Act
        await onValidatePrincipal(context);

        // Assert: the explicit keep-alive renews regardless of KeepUserLoggedIn
        Assert.Multiple(() =>
        {
            Assert.That(context.ShouldRenew, Is.True);
            Assert.That(context.Properties.IssuedUtc, Is.EqualTo(_now));
            Assert.That(context.Properties.ExpiresUtc, Is.EqualTo(_now.Add(_globalSettings.TimeOut)));
        });
    }

    // The renewal is scoped to the keep-alive endpoint on purpose: renewing on every request would stop
    // the SecurityStampValidator ever exceeding its ValidationInterval during active use.
    [Test]
    public async Task Cannot_Renew_Ticket_For_Non_KeepAlive_Request()
    {
        _mockStampValidator
            .Setup(v => v.ValidateAsync(It.IsAny<CookieValidatePrincipalContext>()))
            .Returns(Task.CompletedTask);

        var originalIssuedUtc = _now.AddMinutes(-10);
        var originalExpiresUtc = _now.AddMinutes(50);

        CookieValidatePrincipalContext context = CreateValidatePrincipalContext(
            originalIssuedUtc,
            originalExpiresUtc,
            "/umbraco/management/api/v1/document");
        Func<CookieValidatePrincipalContext, Task> onValidatePrincipal = GetOnValidatePrincipal();

        // Act
        await onValidatePrincipal(context);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(context.ShouldRenew, Is.False);
            Assert.That(context.Properties.IssuedUtc, Is.EqualTo(originalIssuedUtc));
            Assert.That(context.Properties.ExpiresUtc, Is.EqualTo(originalExpiresUtc));
        });
    }

    // A Management API request is always JSON, so an unauthenticated one must get a status code rather
    // than a 302 to the HTML login page. The exception is an explicit client_id, which is how Postman
    // and Swagger UI start an authorization code flow and do need the redirect.
    [TestCase("/umbraco/management/api/v1/document", null, 401)]
    [TestCase("/umbraco/management/api/v1/document", "any-client-id", 302)]
    [TestCase("/umbraco/not-management-api", null, 302)]
    public void Can_Answer_Unauthorized_Request_With_Expected_Status(string path, string? clientId, int expectedStatusCode)
    {
        RedirectContext<CookieAuthenticationOptions> context = CreateRedirectContext(path, clientId);

        ConfigureOptions().Events.OnRedirectToLogin(context);

        AssertRedirectOutcome(context, expectedStatusCode);
    }

    [TestCase("/umbraco/management/api/v1/document", null, 403)]
    [TestCase("/umbraco/management/api/v1/document", "any-client-id", 302)]
    [TestCase("/umbraco/not-management-api", null, 302)]
    public void Can_Answer_Forbidden_Request_With_Expected_Status(string path, string? clientId, int expectedStatusCode)
    {
        RedirectContext<CookieAuthenticationOptions> context = CreateRedirectContext(path, clientId);

        ConfigureOptions().Events.OnRedirectToAccessDenied(context);

        AssertRedirectOutcome(context, expectedStatusCode);
    }

    // The X-Requested-With header keeps forcing the status-code branch for non-API paths.
    [Test]
    public void Can_Answer_Unauthorized_Xhr_Request_Outside_Management_Api_With_401()
    {
        RedirectContext<CookieAuthenticationOptions> context = CreateRedirectContext("/umbraco/login", clientId: null);
        context.Request.Headers.XRequestedWith = "XMLHttpRequest";

        ConfigureOptions().Events.OnRedirectToLogin(context);

        AssertRedirectOutcome(context, 401);
    }

    private static void AssertRedirectOutcome(RedirectContext<CookieAuthenticationOptions> context, int expectedStatusCode)
        => Assert.Multiple(() =>
        {
            Assert.That(context.Response.StatusCode, Is.EqualTo(expectedStatusCode));
            Assert.That(context.Response.Headers.Location.ToString(), Is.EqualTo(context.RedirectUri));
        });

    private RedirectContext<CookieAuthenticationOptions> CreateRedirectContext(string path, string? clientId)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = path;
        if (clientId is not null)
        {
            httpContext.Request.QueryString = new QueryString($"?client_id={clientId}");
        }

        var scheme = new AuthenticationScheme(
            Constants.Security.BackOfficeAuthenticationType,
            Constants.Security.BackOfficeAuthenticationType,
            typeof(CookieAuthenticationHandler));

        return new RedirectContext<CookieAuthenticationOptions>(
            httpContext,
            scheme,
            new CookieAuthenticationOptions(),
            new AuthenticationProperties(),
            $"/umbraco/login?ReturnUrl={Uri.EscapeDataString(path)}");
    }

    private Func<CookieValidatePrincipalContext, Task> GetOnValidatePrincipal()
        => ConfigureOptions().Events.OnValidatePrincipal;

    private CookieAuthenticationOptions ConfigureOptions()
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
        return options;
    }

    private CookieValidatePrincipalContext CreateValidatePrincipalContext(
        DateTimeOffset issuedUtc,
        DateTimeOffset expiresUtc,
        string? requestPath = null)
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
        if (requestPath is not null)
        {
            httpContext.Request.Path = requestPath;
        }

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
