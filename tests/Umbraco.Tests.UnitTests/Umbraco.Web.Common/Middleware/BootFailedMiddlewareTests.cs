// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Middleware;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Middleware;

[TestFixture]
public class BootFailedMiddlewareTests
{
    private const string UpgradeFailedHtml = "<html><body>UpgradeFailed</body></html>";
    private const string BootFailedHtml = "<html><body>BootFailed</body></html>";

    [Test]
    public async Task InvokeAsync_WhenUpgradeFailed_Returns503()
    {
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(RuntimeLevel.UpgradeFailed);

        await middleware.InvokeAsync(context, _ => Task.CompletedTask);

        Assert.That(context.Response.StatusCode, Is.EqualTo(StatusCodes.Status503ServiceUnavailable));
    }

    [Test]
    public async Task InvokeAsync_WhenUpgradeFailed_WritesUpgradeFailedContent()
    {
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(RuntimeLevel.UpgradeFailed, upgradeFailedContent: UpgradeFailedHtml);

        await middleware.InvokeAsync(context, _ => Task.CompletedTask);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body, Encoding.UTF8).ReadToEndAsync();
        Assert.That(body, Is.EqualTo(UpgradeFailedHtml));
    }

    [Test]
    public async Task InvokeAsync_WhenUpgradeFailed_DoesNotCallNext()
    {
        var context = CreateHttpContext();
        var nextCalled = false;
        var middleware = CreateMiddleware(RuntimeLevel.UpgradeFailed);

        await middleware.InvokeAsync(context, _ => { nextCalled = true; return Task.CompletedTask; });

        Assert.That(nextCalled, Is.False);
    }

    [Test]
    public async Task InvokeAsync_WhenUpgradeFailed_AndNoFileExists_Still503()
    {
        // When UpgradeFailed.html is absent, still respond 503 (no content body).
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(RuntimeLevel.UpgradeFailed, upgradeFailedContent: null);

        await middleware.InvokeAsync(context, _ => Task.CompletedTask);

        Assert.That(context.Response.StatusCode, Is.EqualTo(StatusCodes.Status503ServiceUnavailable));
    }

    [Test]
    public async Task InvokeAsync_WhenBootFailed_NonDebug_Returns500()
    {
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(RuntimeLevel.BootFailed, bootFailedContent: BootFailedHtml);

        await middleware.InvokeAsync(context, _ => Task.CompletedTask);

        Assert.That(context.Response.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }

    [Test]
    public async Task InvokeAsync_WhenBootFailed_DebugMode_RethrowsException()
    {
        var context = CreateHttpContext();
        var bootException = new BootFailedException("Test failure");
        var middleware = CreateMiddleware(RuntimeLevel.BootFailed, isDebug: true, bootFailedException: bootException);

        Assert.ThrowsAsync<BootFailedException>(() => middleware.InvokeAsync(context, _ => Task.CompletedTask));
    }

    [TestCase(RuntimeLevel.Run)]
    [TestCase(RuntimeLevel.Upgrading)]
    [TestCase(RuntimeLevel.Upgrade)]
    public async Task InvokeAsync_WhenLevelIsNeitherBootFailedNorUpgradeFailed_CallsNext(RuntimeLevel level)
    {
        var context = CreateHttpContext();
        var nextCalled = false;
        var middleware = CreateMiddleware(level);

        await middleware.InvokeAsync(context, _ => { nextCalled = true; return Task.CompletedTask; });

        Assert.That(nextCalled, Is.True);
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static BootFailedMiddleware CreateMiddleware(
        RuntimeLevel level,
        string? upgradeFailedContent = null,
        string? bootFailedContent = null,
        bool isDebug = false,
        BootFailedException? bootFailedException = null)
    {
        var runtimeState = new Mock<IRuntimeState>();
        runtimeState.SetupGet(x => x.Level).Returns(level);
        runtimeState.SetupGet(x => x.BootFailedException).Returns(bootFailedException);

        var hostingEnv = Mock.Of<IHostingEnvironment>(e => e.IsDebugMode == isDebug);

        var fileProvider = new Mock<IFileProvider>();

        // Custom override path (e.g. config/errors/) — always absent in these tests.
        fileProvider
            .Setup(x => x.GetFileInfo(It.Is<string>(p => p.StartsWith("config/"))))
            .Returns(Mock.Of<IFileInfo>(f => f.Exists == false));

        SetupFile(fileProvider, "umbraco/views/errors/UpgradeFailed.html", upgradeFailedContent);
        SetupFile(fileProvider, "umbraco/views/errors/BootFailed.html", bootFailedContent);

        var webHostEnv = Mock.Of<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>(
            e => e.WebRootFileProvider == fileProvider.Object);

        return new BootFailedMiddleware(runtimeState.Object, hostingEnv, webHostEnv);
    }

    private static void SetupFile(Mock<IFileProvider> provider, string path, string? content)
    {
        if (content is null)
        {
            provider.Setup(x => x.GetFileInfo(path))
                .Returns(Mock.Of<IFileInfo>(f => f.Exists == false));
            return;
        }

        var fileInfo = new Mock<IFileInfo>();
        fileInfo.SetupGet(x => x.Exists).Returns(true);
        fileInfo.Setup(x => x.CreateReadStream())
            .Returns(new MemoryStream(Encoding.UTF8.GetBytes(content)));
        provider.Setup(x => x.GetFileInfo(path)).Returns(fileInfo.Object);
    }
}
