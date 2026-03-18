using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Middleware;
using Umbraco.Cms.Web.Common.Security;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Website.Middleware;

[TestFixture]
public class BasicAuthenticationMiddlewareTests
{
    private Mock<IRuntimeState> _runtimeStateMock = null!;
    private Mock<IBasicAuthService> _basicAuthServiceMock = null!;
    private Mock<IBackOfficeSignInManager> _signInManagerMock = null!;
    private BasicAuthenticationMiddleware _middleware = null!;
    private bool _nextCalled;

    [SetUp]
    public void SetUp()
    {
        _runtimeStateMock = new Mock<IRuntimeState>();
        _runtimeStateMock.Setup(x => x.Level).Returns(RuntimeLevel.Run);

        _basicAuthServiceMock = new Mock<IBasicAuthService>();
        _basicAuthServiceMock.Setup(x => x.IsBasicAuthEnabled()).Returns(true);

        var hostingEnvironmentMock = new Mock<IHostingEnvironment>();
        hostingEnvironmentMock.Setup(x => x.ApplicationVirtualPath).Returns("/");
        hostingEnvironmentMock.Setup(x => x.ToAbsolute(It.IsAny<string>())).Returns<string>(path => path.TrimStart('~'));

        _signInManagerMock = new Mock<IBackOfficeSignInManager>();
        _nextCalled = false;

        _middleware = new BasicAuthenticationMiddleware(
            _runtimeStateMock.Object,
            _basicAuthServiceMock.Object,
            hostingEnvironmentMock.Object);
    }

    /// <summary>
    /// Verifies that the middleware passes through when the runtime level is below Run.
    /// </summary>
    [Test]
    public async Task InvokeAsync_RuntimeLevelBelowRun_PassesThrough()
    {
        _runtimeStateMock.Setup(x => x.Level).Returns(RuntimeLevel.Boot);
        HttpContext context = CreateHttpContext("/some-page");

        await _middleware.InvokeAsync(context, NextDelegate());

        Assert.IsTrue(_nextCalled);
    }

    /// <summary>
    /// Verifies that the middleware passes through when basic auth is disabled.
    /// </summary>
    [Test]
    public async Task InvokeAsync_BasicAuthDisabled_PassesThrough()
    {
        _basicAuthServiceMock.Setup(x => x.IsBasicAuthEnabled()).Returns(false);
        HttpContext context = CreateHttpContext("/some-page");

        await _middleware.InvokeAsync(context, NextDelegate());

        Assert.IsTrue(_nextCalled);
    }

    /// <summary>
    /// Verifies that the middleware passes through for requests to the standalone login page,
    /// preventing an infinite redirect loop.
    /// </summary>
    [Test]
    public async Task InvokeAsync_BasicAuthLoginRoute_PassesThrough()
    {
        HttpContext context = CreateHttpContext("/umbraco/basic-auth/login");

        await _middleware.InvokeAsync(context, NextDelegate());

        Assert.IsTrue(_nextCalled);
    }

    /// <summary>
    /// Verifies that the middleware passes through for requests to the 2FA page.
    /// </summary>
    [Test]
    public async Task InvokeAsync_BasicAuth2faRoute_PassesThrough()
    {
        HttpContext context = CreateHttpContext("/umbraco/basic-auth/2fa");

        await _middleware.InvokeAsync(context, NextDelegate());

        Assert.IsTrue(_nextCalled);
    }

    /// <summary>
    /// Verifies that the middleware passes through when the client has a correct shared secret header.
    /// </summary>
    [Test]
    public async Task InvokeAsync_CorrectSharedSecret_PassesThrough()
    {
        _basicAuthServiceMock.Setup(x => x.HasCorrectSharedSecret(It.IsAny<IHeaderDictionary>())).Returns(true);
        HttpContext context = CreateHttpContext("/some-page");

        await _middleware.InvokeAsync(context, NextDelegate());

        Assert.IsTrue(_nextCalled);
    }

    /// <summary>
    /// Verifies that the middleware passes through when the client IP is allow-listed.
    /// </summary>
    [Test]
    public async Task InvokeAsync_AllowListedIp_PassesThrough()
    {
        _basicAuthServiceMock.Setup(x => x.IsIpAllowListed(It.IsAny<IPAddress>())).Returns(true);
        HttpContext context = CreateHttpContext("/some-page");
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        await _middleware.InvokeAsync(context, NextDelegate());

        Assert.IsTrue(_nextCalled);
    }

    /// <summary>
    /// Verifies that when RedirectToLoginPage is enabled and no credentials are provided,
    /// the middleware redirects to the standalone login page.
    /// </summary>
    [Test]
    public async Task InvokeAsync_NoCredentials_RedirectToLoginEnabled_RedirectsToStandaloneLogin()
    {
        _basicAuthServiceMock.Setup(x => x.IsRedirectToLoginPageEnabled()).Returns(true);
        HttpContext context = CreateHttpContext("/protected-page");

        await _middleware.InvokeAsync(context, NextDelegate());

        Assert.IsFalse(_nextCalled);
        Assert.That(context.Response.StatusCode, Is.EqualTo(302));
        Assert.That(
            context.Response.Headers.Location.ToString(),
            Does.Contain("basic-auth/login?returnPath="));
    }

    /// <summary>
    /// Verifies that when RedirectToLoginPage is disabled and no credentials are provided,
    /// the middleware returns 401 with a WWW-Authenticate header for the browser Basic popup.
    /// </summary>
    [Test]
    public async Task InvokeAsync_NoCredentials_RedirectToLoginDisabled_Returns401WithWwwAuthenticate()
    {
        _basicAuthServiceMock.Setup(x => x.IsRedirectToLoginPageEnabled()).Returns(false);
        HttpContext context = CreateHttpContext("/protected-page");

        await _middleware.InvokeAsync(context, NextDelegate());

        Assert.IsFalse(_nextCalled);
        Assert.That(context.Response.StatusCode, Is.EqualTo(401));
        Assert.That(context.Response.Headers["WWW-Authenticate"].ToString(), Is.EqualTo("Basic realm=\"Umbraco login\""));
    }

    /// <summary>
    /// Verifies that valid Basic credentials in the Authorization header allow the request through.
    /// </summary>
    [Test]
    public async Task InvokeAsync_ValidBasicCredentials_PassesThrough()
    {
        _signInManagerMock
            .Setup(x => x.PasswordSignInAsync("admin", "pass", false, true))
            .ReturnsAsync(SignInResult.Success);

        HttpContext context = CreateHttpContext("/protected-page", withSignInManager: true);
        AddBasicAuthHeader(context, "admin", "pass");

        await _middleware.InvokeAsync(context, NextDelegate());

        Assert.IsTrue(_nextCalled);
    }

    /// <summary>
    /// Verifies that invalid Basic credentials result in an unauthorized response.
    /// </summary>
    [Test]
    public async Task InvokeAsync_InvalidBasicCredentials_HandleUnauthorized()
    {
        _signInManagerMock
            .Setup(x => x.PasswordSignInAsync("admin", "wrong", false, true))
            .ReturnsAsync(SignInResult.Failed);
        _basicAuthServiceMock.Setup(x => x.IsRedirectToLoginPageEnabled()).Returns(false);

        HttpContext context = CreateHttpContext("/protected-page", withSignInManager: true);
        AddBasicAuthHeader(context, "admin", "wrong");

        await _middleware.InvokeAsync(context, NextDelegate());

        Assert.IsFalse(_nextCalled);
        Assert.That(context.Response.StatusCode, Is.EqualTo(401));
    }

    /// <summary>
    /// Verifies that when Basic credentials require 2FA, the middleware redirects to the 2FA page
    /// even when RedirectToLoginPage is disabled. The browser's Basic popup cannot complete a 2FA flow.
    /// </summary>
    [Test]
    public async Task InvokeAsync_RequiresTwoFactor_RedirectToLoginDisabled_StillRedirectsTo2fa()
    {
        _signInManagerMock
            .Setup(x => x.PasswordSignInAsync("admin", "pass", false, true))
            .ReturnsAsync(SignInResult.TwoFactorRequired);
        _basicAuthServiceMock.Setup(x => x.IsRedirectToLoginPageEnabled()).Returns(false);

        HttpContext context = CreateHttpContext("/protected-page", withSignInManager: true);
        AddBasicAuthHeader(context, "admin", "pass");

        await _middleware.InvokeAsync(context, NextDelegate());

        Assert.IsFalse(_nextCalled);
        Assert.That(context.Response.StatusCode, Is.EqualTo(302));
        Assert.That(
            context.Response.Headers.Location.ToString(),
            Does.Contain("basic-auth/2fa?returnPath="));
    }

    /// <summary>
    /// Verifies that when Basic credentials require 2FA and RedirectToLoginPage is enabled,
    /// the middleware redirects to the 2FA page.
    /// </summary>
    [Test]
    public async Task InvokeAsync_RequiresTwoFactor_RedirectToLoginEnabled_RedirectsTo2fa()
    {
        _signInManagerMock
            .Setup(x => x.PasswordSignInAsync("admin", "pass", false, true))
            .ReturnsAsync(SignInResult.TwoFactorRequired);
        _basicAuthServiceMock.Setup(x => x.IsRedirectToLoginPageEnabled()).Returns(true);

        HttpContext context = CreateHttpContext("/protected-page", withSignInManager: true);
        AddBasicAuthHeader(context, "admin", "pass");

        await _middleware.InvokeAsync(context, NextDelegate());

        Assert.IsFalse(_nextCalled);
        Assert.That(context.Response.StatusCode, Is.EqualTo(302));
        Assert.That(
            context.Response.Headers.Location.ToString(),
            Does.Contain("basic-auth/2fa?returnPath="));
    }

    /// <summary>
    /// Verifies that when the backoffice auth scheme is not registered (AddCore() only),
    /// the middleware does not throw and proceeds to handle the request.
    /// </summary>
    [Test]
    public async Task InvokeAsync_NoBackOfficeAuthScheme_DoesNotThrow_HandleUnauthorized()
    {
        _basicAuthServiceMock.Setup(x => x.IsRedirectToLoginPageEnabled()).Returns(false);
        HttpContext context = CreateHttpContext("/protected-page");

        // No auth scheme registered, no Basic header — should return 401 without throwing
        await _middleware.InvokeAsync(context, NextDelegate());

        Assert.IsFalse(_nextCalled);
        Assert.That(context.Response.StatusCode, Is.EqualTo(401));
    }

    /// <summary>
    /// Verifies that when IBackOfficeSignInManager is not registered and Basic credentials
    /// are provided, the middleware treats it as unauthorized.
    /// </summary>
    [Test]
    public async Task InvokeAsync_BasicCredentials_NoSignInManager_HandleUnauthorized()
    {
        _basicAuthServiceMock.Setup(x => x.IsRedirectToLoginPageEnabled()).Returns(false);

        // Create context without sign-in manager
        HttpContext context = CreateHttpContext("/protected-page", withSignInManager: false);
        AddBasicAuthHeader(context, "admin", "pass");

        await _middleware.InvokeAsync(context, NextDelegate());

        Assert.IsFalse(_nextCalled);
        Assert.That(context.Response.StatusCode, Is.EqualTo(401));
    }

    /// <summary>
    /// Verifies that when no authentication services are registered (AddCore().AddWebsite() only),
    /// the middleware does not throw and falls through to HandleUnauthorized gracefully.
    /// </summary>
    [Test]
    public async Task InvokeAsync_NoAuthenticationServicesRegistered_DoesNotThrow_HandleUnauthorized()
    {
        _basicAuthServiceMock.Setup(x => x.IsRedirectToLoginPageEnabled()).Returns(false);

        // Create context without authentication services (no IAuthenticationSchemeProvider)
        var services = new ServiceCollection();
        services.AddLogging();
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };
        httpContext.Request.Path = "/protected-page";
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("localhost");

        await _middleware.InvokeAsync(httpContext, NextDelegate());

        Assert.IsFalse(_nextCalled);
        Assert.That(httpContext.Response.StatusCode, Is.EqualTo(401));
    }

    /// <summary>
    /// Verifies that the return path in the login redirect is URL-encoded.
    /// </summary>
    [Test]
    public async Task InvokeAsync_RedirectToLogin_ReturnPathIsUrlEncoded()
    {
        _basicAuthServiceMock.Setup(x => x.IsRedirectToLoginPageEnabled()).Returns(true);
        HttpContext context = CreateHttpContext("/page");
        context.Request.QueryString = new QueryString("?foo=bar&baz=1");

        await _middleware.InvokeAsync(context, NextDelegate());

        var location = context.Response.Headers.Location.ToString();
        Assert.That(location, Does.Contain("returnPath=%2Fpage%3Ffoo%3Dbar%26baz%3D1"));
    }

    private HttpContext CreateHttpContext(string path, bool withSignInManager = false)
    {
        var services = new ServiceCollection();
        services.AddAuthentication();
        services.AddLogging();

        if (withSignInManager)
        {
            services.AddSingleton(_signInManagerMock.Object);
        }

        ServiceProvider serviceProvider = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };
        httpContext.Request.Path = path;
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("localhost");

        return httpContext;
    }

    private static void AddBasicAuthHeader(HttpContext context, string username, string password)
    {
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
        context.Request.Headers.Authorization = $"Basic {credentials}";
    }

    private RequestDelegate NextDelegate() =>
        _ =>
            {
                _nextCalled = true;
                return Task.CompletedTask;
            };
}
