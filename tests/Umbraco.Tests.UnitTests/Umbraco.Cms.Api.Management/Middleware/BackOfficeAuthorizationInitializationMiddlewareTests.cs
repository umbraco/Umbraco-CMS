using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Middleware;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Security;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Middleware;

[TestFixture]
public class BackOfficeAuthorizationInitializationMiddlewareTests
{
    private Mock<IRuntimeState> _mockRuntimeState = null!;
    private Mock<IBackOfficeApplicationManager> _mockBackOfficeApplicationManager = null!;
    private BackOfficeAuthorizationInitializationMiddleware _middleware = null!;

    [SetUp]
    public void SetUp()
    {
        _mockRuntimeState = new Mock<IRuntimeState>();
        _mockRuntimeState.Setup(x => x.Level).Returns(RuntimeLevel.Run);

        _mockBackOfficeApplicationManager = new Mock<IBackOfficeApplicationManager>();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(_mockBackOfficeApplicationManager.Object);
        ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

        UmbracoRequestPaths umbracoRequestPaths = CreateUmbracoRequestPaths();
        var webRoutingSettings = Options.Create(new WebRoutingSettings());

        _middleware = new BackOfficeAuthorizationInitializationMiddleware(
            umbracoRequestPaths,
            serviceProvider,
            _mockRuntimeState.Object,
            webRoutingSettings);
    }

    [Test]
    public async Task Registration_Failure_Does_Not_Cache_Host_So_Next_Request_Retries()
    {
        // Arrange — first call throws (simulating database contention during upgrade).
        var callCount = 0;
        _mockBackOfficeApplicationManager
            .Setup(x => x.EnsureBackOfficeApplicationAsync(It.IsAny<IEnumerable<Uri>>(), It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                callCount++;
                if (callCount == 1)
                {
                    throw new Exception("Database is locked");
                }

                return Task.CompletedTask;
            });

        RequestDelegate next = _ => Task.CompletedTask;
        HttpContext context = CreateBackOfficeHttpContext();

        // Act — first request: registration fails.
        try
        {
            await _middleware.InvokeAsync(context, next);
        }
        catch
        {
            // Expected — the middleware lets the exception propagate.
        }

        // Act — second request: should retry (host was NOT cached).
        await _middleware.InvokeAsync(context, next);

        // Assert — EnsureBackOfficeApplicationAsync was called twice (retried after failure).
        Assert.That(callCount, Is.EqualTo(2));
    }

    [Test]
    public async Task Successful_Registration_Caches_Host_So_Subsequent_Requests_Skip()
    {
        // Arrange
        _mockBackOfficeApplicationManager
            .Setup(x => x.EnsureBackOfficeApplicationAsync(It.IsAny<IEnumerable<Uri>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        RequestDelegate next = _ => Task.CompletedTask;
        HttpContext context = CreateBackOfficeHttpContext();

        // Act
        await _middleware.InvokeAsync(context, next);
        await _middleware.InvokeAsync(context, next);

        // Assert — only called once (second request skipped because host was cached)
        _mockBackOfficeApplicationManager.Verify(
            x => x.EnsureBackOfficeApplicationAsync(It.IsAny<IEnumerable<Uri>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Registration_Failure_Does_Not_Leak_Semaphore()
    {
        // Arrange — always throws
        _mockBackOfficeApplicationManager
            .Setup(x => x.EnsureBackOfficeApplicationAsync(It.IsAny<IEnumerable<Uri>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database is locked"));

        RequestDelegate next = _ => Task.CompletedTask;

        // Act — multiple failing requests should not deadlock (semaphore released via finally).
        for (var i = 0; i < 3; i++)
        {
            HttpContext context = CreateBackOfficeHttpContext();
            try
            {
                await _middleware.InvokeAsync(context, next);
            }
            catch
            {
                // Expected
            }
        }

        // Assert — all 3 attempts reached EnsureBackOfficeApplicationAsync (no deadlock).
        _mockBackOfficeApplicationManager.Verify(
            x => x.EnsureBackOfficeApplicationAsync(It.IsAny<IEnumerable<Uri>>(), It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [Test]
    public async Task RuntimeLevel_Below_Upgrade_Skips_Registration()
    {
        // Arrange
        _mockRuntimeState.Setup(x => x.Level).Returns(RuntimeLevel.Install);

        RequestDelegate next = _ => Task.CompletedTask;
        HttpContext context = CreateBackOfficeHttpContext();

        // Act
        await _middleware.InvokeAsync(context, next);

        // Assert
        _mockBackOfficeApplicationManager.Verify(
            x => x.EnsureBackOfficeApplicationAsync(It.IsAny<IEnumerable<Uri>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// Creates an HttpContext with a path that <see cref="UmbracoRequestPaths.IsBackOfficeRequest"/> recognizes.
    /// The Management API path (/umbraco/management/api/) is a backoffice request.
    /// </summary>
    private static HttpContext CreateBackOfficeHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("localhost", 44339);
        context.Request.Path = "/umbraco/management/api/v1/server/status";
        return context;
    }

    private static UmbracoRequestPaths CreateUmbracoRequestPaths()
    {
        var mockHostingEnvironment = new Mock<IHostingEnvironment>();
        mockHostingEnvironment.Setup(x => x.ApplicationVirtualPath).Returns("/");
        mockHostingEnvironment.Setup(x => x.ToAbsolute(It.IsAny<string>()))
            .Returns<string>(path => path.TrimStart('~'));

        var options = Options.Create(new UmbracoRequestPathsOptions());

        return new UmbracoRequestPaths(mockHostingEnvironment.Object, options);
    }
}
