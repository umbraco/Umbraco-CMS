using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OpenIddict.Server;
using OpenIddict.Validation;
using Umbraco.Cms.Api.Common.Configuration;
using Umbraco.Cms.Api.Common.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Common.DependencyInjection;

[TestFixture]
internal class ProcessRequestContextHandlerTests
{
    private static readonly string BackOfficePath = Constants.System.DefaultUmbracoPath
        .TrimStart(Constants.CharArrays.Tilde)
        .EnsureStartsWith('/')
        .EnsureEndsWith('/');

    private static IEnumerable<string> CorePathRequests()
    {
        yield return BackOfficePath + "management/api/v1/security/back-office/token";
        yield return "/.well-known/openid-configuration";
        yield return "/.well-known/jwks";
    }

    [TestCaseSource(nameof(CorePathRequests))]
    public async Task HandleAsync_CorePath_IsHandled(string requestPath)
    {
        OpenIddictServerEvents.ProcessRequestContext context = ServerContext();

        await HandlerFor(requestPath).HandleAsync(context);

        Assert.That(context.IsRequestSkipped, Is.False);
    }

    [TestCaseSource(nameof(CorePathRequests))]
    public async Task HandleAsync_CorePath_IsHandled_WhenAdditionalPathsAreConfigured(string requestPath)
    {
        OpenIddictServerEvents.ProcessRequestContext context = ServerContext();

        await HandlerFor(requestPath, AdditionalPaths("/my-own-oidc/")).HandleAsync(context);

        Assert.That(context.IsRequestSkipped, Is.False);
    }

    [Test]
    public async Task HandleAsync_ServerRequest_OutsideHandledPaths_IsSkipped()
    {
        OpenIddictServerEvents.ProcessRequestContext context = ServerContext();

        await HandlerFor("/my-own-oidc/token").HandleAsync(context);

        Assert.That(context.IsRequestSkipped, Is.True);
    }

    [Test]
    public async Task HandleAsync_ServerRequest_WithinAdditionalPath_IsHandled()
    {
        OpenIddictServerEvents.ProcessRequestContext context = ServerContext();

        await HandlerFor("/my-own-oidc/token", AdditionalPaths("/my-own-oidc/")).HandleAsync(context);

        Assert.That(context.IsRequestSkipped, Is.False);
    }

    [Test]
    public async Task HandleAsync_ValidationRequest_OutsideHandledPaths_IsSkipped()
    {
        OpenIddictValidationEvents.ProcessRequestContext context = ValidationContext();

        await HandlerFor("/my-own-oidc/token").HandleAsync(context);

        Assert.That(context.IsRequestSkipped, Is.True);
    }

    [Test]
    public async Task HandleAsync_ValidationRequest_WithinAdditionalPath_IsHandled()
    {
        OpenIddictValidationEvents.ProcessRequestContext context = ValidationContext();

        await HandlerFor("/my-own-oidc/token", AdditionalPaths("/my-own-oidc/")).HandleAsync(context);

        Assert.That(context.IsRequestSkipped, Is.False);
    }

    [TestCase("")]
    [TestCase(null)]
    public async Task HandleAsync_NullOrEmptyAdditionalPath_IsIgnored(string? emptyPath)
    {
        OpenIddictServerEvents.ProcessRequestContext context = ServerContext();

        await HandlerFor("/some-page", AdditionalPaths(emptyPath!)).HandleAsync(context);

        Assert.That(context.IsRequestSkipped, Is.True);
    }

    // The zero-width case guards against a culture-sensitive comparison, which ignores the character and so
    // treats the value as an empty prefix that matches every request.
    [TestCase("/my-own-oidc/", "/other/my-own-oidc/token")]
    [TestCase("\u200B", "/some-page")]
    public async Task HandleAsync_RequestNotStartingWithAdditionalPath_IsSkipped(string additionalPath, string requestPath)
    {
        OpenIddictServerEvents.ProcessRequestContext context = ServerContext();

        await HandlerFor(requestPath, AdditionalPaths(additionalPath)).HandleAsync(context);

        Assert.That(context.IsRequestSkipped, Is.True);
    }

    [TestCase("/package-a/token")]
    [TestCase("/package-b/token")]
    public async Task HandleAsync_AdditionalPathsFromEveryConfiguration_AreHandled(string requestPath)
    {
        var services = new ServiceCollection();
        services.AddSingleton(HttpContextAccessorFor(requestPath));
        services.Configure<OpenIddictAdditionalPathsOptions>(o => o.PathsToHandle.Add("/package-a/"));
        services.Configure<OpenIddictAdditionalPathsOptions>(o => o.PathsToHandle.Add("/package-b/"));
        services.AddSingleton<ProcessRequestContextHandler>();
        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        OpenIddictServerEvents.ProcessRequestContext context = ServerContext();

        await serviceProvider.GetRequiredService<ProcessRequestContextHandler>().HandleAsync(context);

        Assert.That(context.IsRequestSkipped, Is.False);
    }

    [Test]
    public async Task HandleAsync_RequestWithoutPath_IsHandled()
    {
        // The handler cannot tell whether an unresolved path is one of ours, so it defers to OpenIddict
        // rather than skipping a request that may need processing.
        OpenIddictServerEvents.ProcessRequestContext context = ServerContext();

        await HandlerFor(null).HandleAsync(context);

        Assert.That(context.IsRequestSkipped, Is.False);
    }

    private static OpenIddictAdditionalPathsOptions AdditionalPaths(params string[] additionalPaths)
    {
        var options = new OpenIddictAdditionalPathsOptions();
        foreach (var path in additionalPaths)
        {
            options.PathsToHandle.Add(path);
        }

        return options;
    }

    private static ProcessRequestContextHandler HandlerFor(string? requestPath, OpenIddictAdditionalPathsOptions? options = null)
        => new(HttpContextAccessorFor(requestPath), Options.Create(options ?? new OpenIddictAdditionalPathsOptions()));

    private static IHttpContextAccessor HttpContextAccessorFor(string? requestPath)
    {
        var httpContext = new DefaultHttpContext();
        if (requestPath is not null)
        {
            httpContext.Request.Path = requestPath;
        }

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        return httpContextAccessor.Object;
    }

    private static OpenIddictServerEvents.ProcessRequestContext ServerContext()
        => new(new OpenIddictServerTransaction());

    private static OpenIddictValidationEvents.ProcessRequestContext ValidationContext()
        => new(new OpenIddictValidationTransaction());
}
