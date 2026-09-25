using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using OpenIddict.Server;
using OpenIddict.Validation;
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

    [TestCase("/.well-known/openid-configuration")]
    [TestCase("/.well-known/jwks")]
    public void GetPathsToHandle_Default_IncludesWellKnownEndpoint(string path)
    {
        Assert.That(PathsToHandle(), Does.Contain(path));
    }

    [Test]
    public void GetPathsToHandle_Default_IncludesBackOfficePath()
    {
        Assert.That(PathsToHandle(), Does.Contain(BackOfficePath));
    }

    [Test]
    public async Task HandleAsync_ServerRequest_OutsideHandledPaths_IsSkipped()
    {
        OpenIddictServerEvents.ProcessRequestContext context = ServerContext();

        await HandlerFor("/my-own-oidc/token").HandleAsync(context);

        Assert.That(context.IsRequestSkipped, Is.True);
    }

    [Test]
    public async Task HandleAsync_ServerRequest_WithinProvidedPath_IsHandled()
    {
        OpenIddictServerEvents.ProcessRequestContext context = ServerContext();

        await HandlerFor("/my-own-oidc/token", "/my-own-oidc/").HandleAsync(context);

        Assert.That(context.IsRequestSkipped, Is.False);
    }

    [Test]
    public async Task HandleAsync_ValidationRequest_WithinProvidedPath_IsHandled()
    {
        OpenIddictValidationEvents.ProcessRequestContext context = ValidationContext();

        await HandlerFor("/my-own-oidc/token", "/my-own-oidc/").HandleAsync(context);

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
    public void GetPathsToHandle_DerivedProvider_AddsToDefaultPaths()
    {
        var paths = new CustomPathsProvider().GetPathsToHandle().ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(paths, Does.Contain(BackOfficePath));
            Assert.That(paths, Does.Contain("/my-own-oidc/"));
        });
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

    private static string[] PathsToHandle()
        => new OpenIddictPathsToHandleProvider().GetPathsToHandle().ToArray();

    private static ProcessRequestContextHandler HandlerFor(string? requestPath, params string[] pathsToHandle)
    {
        var httpContext = new DefaultHttpContext();
        if (requestPath is not null)
        {
            httpContext.Request.Path = requestPath;
        }

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var pathsToHandleProvider = new Mock<IOpenIddictPathsToHandleProvider>();
        pathsToHandleProvider.Setup(x => x.GetPathsToHandle()).Returns(pathsToHandle);

        return new ProcessRequestContextHandler(httpContextAccessor.Object, pathsToHandleProvider.Object);
    }

    private static OpenIddictServerEvents.ProcessRequestContext ServerContext()
        => new(new OpenIddictServerTransaction());

    private static OpenIddictValidationEvents.ProcessRequestContext ValidationContext()
        => new(new OpenIddictValidationTransaction());

    private class CustomPathsProvider : OpenIddictPathsToHandleProvider
    {
        public override IEnumerable<string> GetPathsToHandle()
            => base.GetPathsToHandle().Append("/my-own-oidc/");
    }
}
