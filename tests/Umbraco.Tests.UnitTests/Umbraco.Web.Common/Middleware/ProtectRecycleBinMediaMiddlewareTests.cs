// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Middleware;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Middleware;

[TestFixture]
public class ProtectRecycleBinMediaMiddlewareTests
{
    private const string TrashedMediaSuffix = ".deleted";
    private const string MediaSectionAlias = "media";

    private Mock<IUserService> _userServiceMock = null!;
    private Mock<IAuthenticationService> _authServiceMock = null!;
    private ContentSettings _contentSettings = null!;
    private ProtectRecycleBinMediaMiddleware _middleware = null!;

    [SetUp]
    public void SetUp()
    {
        _userServiceMock = new Mock<IUserService>();
        _authServiceMock = new Mock<IAuthenticationService>();
        _contentSettings = new ContentSettings { EnableMediaRecycleBinProtection = true };

        var optionsMonitorMock = new Mock<IOptionsMonitor<ContentSettings>>();
        optionsMonitorMock.Setup(x => x.CurrentValue).Returns(_contentSettings);

        _middleware = new ProtectRecycleBinMediaMiddleware(_userServiceMock.Object, optionsMonitorMock.Object);
    }

    private HttpContext CreateHttpContext(string? path)
    {
        var services = new ServiceCollection();
        services.AddSingleton(_authServiceMock.Object);

        var httpContext = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider(),
        };

        httpContext.Request.Path = path;

        return httpContext;
    }

    private static ClaimsPrincipal CreatePrincipalWithUserKey(Guid userKey)
    {
        var claims = new[] { new Claim("sub", userKey.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    private static ClaimsPrincipal CreatePrincipalWithoutUserKey()
    {
        var identity = new ClaimsIdentity([], "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    private void SetupAuthenticationSuccess(ClaimsPrincipal principal) =>
        _authServiceMock
            .Setup(x => x.AuthenticateAsync(It.IsAny<HttpContext>(), Constants.Security.BackOfficeExposedAuthenticationType))
            .ReturnsAsync(AuthenticateResult.Success(new AuthenticationTicket(principal, "TestScheme")));

    private void SetupAuthenticationFailure() =>
        _authServiceMock
            .Setup(x => x.AuthenticateAsync(It.IsAny<HttpContext>(), Constants.Security.BackOfficeExposedAuthenticationType))
            .ReturnsAsync(AuthenticateResult.Fail("Authentication failed"));

    private static Mock<IUser> CreateUserMock(params string[] allowedSections)
    {
        var userMock = new Mock<IUser>();
        userMock.Setup(x => x.AllowedSections).Returns(allowedSections);
        return userMock;
    }

    [Test]
    public async Task InvokeAsync_WhenProtectionDisabled_CallsNext()
    {
        // Arrange
        _contentSettings.EnableMediaRecycleBinProtection = false;
        var httpContext = CreateHttpContext($"/media/test{TrashedMediaSuffix}.jpg");
        var nextCalled = false;

        // Act
        await _middleware.InvokeAsync(httpContext, _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        // Assert
        Assert.That(nextCalled, Is.True);
    }

    [Test]
    public async Task InvokeAsync_WhenPathIsNull_CallsNext()
    {
        // Arrange
        var httpContext = CreateHttpContext(null);
        var nextCalled = false;

        // Act
        await _middleware.InvokeAsync(httpContext, _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        // Assert
        Assert.That(nextCalled, Is.True);
    }

    [Test]
    public async Task InvokeAsync_WhenPathIsEmpty_CallsNext()
    {
        // Arrange
        var httpContext = CreateHttpContext(string.Empty);
        var nextCalled = false;

        // Act
        await _middleware.InvokeAsync(httpContext, _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        // Assert
        Assert.That(nextCalled, Is.True);
    }

    [Test]
    public async Task InvokeAsync_WhenPathDoesNotStartWithMedia_CallsNext()
    {
        // Arrange
        var httpContext = CreateHttpContext($"/content/test{TrashedMediaSuffix}.jpg");
        var nextCalled = false;

        // Act
        await _middleware.InvokeAsync(httpContext, _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        // Assert
        Assert.That(nextCalled, Is.True);
    }

    [Test]
    public async Task InvokeAsync_WhenPathDoesNotContainTrashedSuffix_CallsNext()
    {
        // Arrange
        var httpContext = CreateHttpContext("/media/test.jpg");
        var nextCalled = false;

        // Act
        await _middleware.InvokeAsync(httpContext, _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        // Assert
        Assert.That(nextCalled, Is.True);
    }

    [Test]
    public async Task InvokeAsync_WhenAuthenticationFails_Returns401()
    {
        // Arrange
        var httpContext = CreateHttpContext($"/media/test{TrashedMediaSuffix}.jpg");
        SetupAuthenticationFailure();

        // Act
        await _middleware.InvokeAsync(httpContext, _ => Task.CompletedTask);

        // Assert
        Assert.That(httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
    }

    [Test]
    public async Task InvokeAsync_WhenUserKeyClaimMissing_Returns403()
    {
        // Arrange
        var httpContext = CreateHttpContext($"/media/test{TrashedMediaSuffix}.jpg");
        SetupAuthenticationSuccess(CreatePrincipalWithoutUserKey());

        // Act
        await _middleware.InvokeAsync(httpContext, _ => Task.CompletedTask);

        // Assert
        Assert.That(httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
    }

    [Test]
    public async Task InvokeAsync_WhenUserNotFound_Returns403()
    {
        // Arrange
        var userKey = Guid.NewGuid();
        var httpContext = CreateHttpContext($"/media/test{TrashedMediaSuffix}.jpg");
        SetupAuthenticationSuccess(CreatePrincipalWithUserKey(userKey));
        _userServiceMock.Setup(x => x.GetAsync(userKey)).ReturnsAsync((IUser?)null);

        // Act
        await _middleware.InvokeAsync(httpContext, _ => Task.CompletedTask);

        // Assert
        Assert.That(httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
    }

    [Test]
    public async Task InvokeAsync_WhenUserDoesNotHaveMediaAccess_Returns403()
    {
        // Arrange
        var userKey = Guid.NewGuid();
        var httpContext = CreateHttpContext($"/media/test{TrashedMediaSuffix}.jpg");
        SetupAuthenticationSuccess(CreatePrincipalWithUserKey(userKey));

        var userMock = CreateUserMock("content", "settings");
        _userServiceMock.Setup(x => x.GetAsync(userKey)).ReturnsAsync(userMock.Object);

        // Act
        await _middleware.InvokeAsync(httpContext, _ => Task.CompletedTask);

        // Assert
        Assert.That(httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
    }

    [Test]
    public async Task InvokeAsync_WhenUserHasMediaAccess_CallsNext()
    {
        // Arrange
        var userKey = Guid.NewGuid();
        var httpContext = CreateHttpContext($"/media/test{TrashedMediaSuffix}.jpg");
        SetupAuthenticationSuccess(CreatePrincipalWithUserKey(userKey));

        var userMock = CreateUserMock(MediaSectionAlias, "content");
        _userServiceMock.Setup(x => x.GetAsync(userKey)).ReturnsAsync(userMock.Object);

        var nextCalled = false;

        // Act
        await _middleware.InvokeAsync(httpContext, _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        // Assert
        Assert.That(nextCalled, Is.True);
    }

    [Test]
    public async Task InvokeAsync_WhenUserHasOnlyMediaAccess_CallsNext()
    {
        // Arrange
        var userKey = Guid.NewGuid();
        var httpContext = CreateHttpContext($"/media/test{TrashedMediaSuffix}.jpg");
        SetupAuthenticationSuccess(CreatePrincipalWithUserKey(userKey));

        var userMock = CreateUserMock(MediaSectionAlias);
        _userServiceMock.Setup(x => x.GetAsync(userKey)).ReturnsAsync(userMock.Object);

        var nextCalled = false;

        // Act
        await _middleware.InvokeAsync(httpContext, _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        // Assert
        Assert.That(nextCalled, Is.True);
    }
}
