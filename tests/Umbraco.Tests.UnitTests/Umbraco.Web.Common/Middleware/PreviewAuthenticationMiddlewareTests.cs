// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Middleware;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Middleware;

[TestFixture]
public class PreviewAuthenticationMiddlewareTests
{
    private Mock<IAuthenticationService> _authenticationServiceMock = null!;
    private Mock<IPreviewSessionService> _previewSessionServiceMock = null!;
    private PreviewAuthenticationMiddleware _middleware = null!;
    private bool _nextCalled;

    [SetUp]
    public void SetUp()
    {
        _authenticationServiceMock = new Mock<IAuthenticationService>();
        _previewSessionServiceMock = new Mock<IPreviewSessionService>();
        _nextCalled = false;

        _middleware = new PreviewAuthenticationMiddleware(
            NullLogger<PreviewAuthenticationMiddleware>.Instance,
            _previewSessionServiceMock.Object);
    }

    /// <summary>
    /// Verifies that when the back-office cookie authenticates successfully and yields a verifiable Umbraco
    /// identity, that identity is appended to the principal and the preview session is flagged as active.
    /// </summary>
    [Test]
    public async Task InvokeAsync_BackOfficeAuthenticationSucceedsWithUmbracoIdentity_AddsIdentityAndStartsPreviewSession()
    {
        SetupAuthenticationResult(AuthenticateResult.Success(
            new AuthenticationTicket(CreateBackOfficeIdentityPrincipal(), Constants.Security.BackOfficeAuthenticationType)));

        HttpContext context = CreateHttpContext();

        await _middleware.InvokeAsync(context, NextDelegate());

        Assert.IsTrue(_nextCalled);
        Assert.IsTrue(context.User.Identities.Any(i => i.IsAuthenticated && i.AuthenticationType == Constants.Security.BackOfficeAuthenticationType));
        _previewSessionServiceMock.Verify(x => x.Start(), Times.Once);
    }

    /// <summary>
    /// Verifies that a successful back-office authentication carrying no resolvable Umbraco identity (e.g. a
    /// principal missing the required back-office claims) is a silent no-op: no identity is added and no preview
    /// session is started, but the request still proceeds.
    /// </summary>
    [Test]
    public async Task InvokeAsync_BackOfficeAuthenticationSucceedsWithoutUmbracoIdentity_DoesNotStartPreviewSession()
    {
        SetupAuthenticationResult(AuthenticateResult.Success(
            new AuthenticationTicket(CreateUnverifiableIdentityPrincipal(), "SomeOtherScheme")));

        HttpContext context = CreateHttpContext();

        await _middleware.InvokeAsync(context, NextDelegate());

        Assert.IsTrue(_nextCalled);
        Assert.IsFalse(context.User.Identities.Any(i => i.IsAuthenticated && i.AuthenticationType == Constants.Security.BackOfficeAuthenticationType));
        _previewSessionServiceMock.Verify(x => x.Start(), Times.Never);
    }

    /// <summary>
    /// Verifies that when the back-office cookie fails to authenticate (e.g. it has expired), the request still
    /// proceeds but no preview session is started.
    /// </summary>
    [Test]
    public async Task InvokeAsync_BackOfficeAuthenticationFails_DoesNotStartPreviewSession()
    {
        SetupAuthenticationResult(AuthenticateResult.Fail("The back-office authentication cookie has expired."));

        HttpContext context = CreateHttpContext();

        await _middleware.InvokeAsync(context, NextDelegate());

        Assert.IsTrue(_nextCalled);
        Assert.IsFalse(context.User.Identities.Any(i => i.IsAuthenticated && i.AuthenticationType == Constants.Security.BackOfficeAuthenticationType));
        _previewSessionServiceMock.Verify(x => x.Start(), Times.Never);
    }

    private void SetupAuthenticationResult(AuthenticateResult result) =>
        _authenticationServiceMock
            .Setup(x => x.AuthenticateAsync(It.IsAny<HttpContext>(), Constants.Security.BackOfficeAuthenticationType))
            .ReturnsAsync(result);

    private static ClaimsPrincipal CreateBackOfficeIdentityPrincipal()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "admin"),
            new Claim(ClaimTypes.GivenName, "Admin"),
            new Claim(ClaimTypes.Locality, "en-US"),
            new Claim(Constants.Security.SecurityStampClaimType, "stamp"),
        };
        var identity = new ClaimsIdentity(claims, Constants.Security.BackOfficeAuthenticationType);
        return new ClaimsPrincipal(identity);
    }

    private static ClaimsPrincipal CreateUnverifiableIdentityPrincipal()
    {
        // Missing the required back-office claims (NameIdentifier, Name, GivenName, Locality, SecurityStamp),
        // so GetUmbracoIdentity() cannot resolve a verified back-office identity from it.
        var identity = new ClaimsIdentity("SomeOtherScheme");
        return new ClaimsPrincipal(identity);
    }

    private HttpContext CreateHttpContext()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_authenticationServiceMock.Object);

        var httpContext = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider(),
        };

        httpContext.Request.Path = "/previewed-page";
        httpContext.Request.Headers.Append("Cookie", $"{Constants.Web.PreviewCookieName}={Constants.Web.PreviewCookieValue}");

        return httpContext;
    }

    private RequestDelegate NextDelegate() =>
        _ =>
        {
            _nextCalled = true;
            return Task.CompletedTask;
        };
}
