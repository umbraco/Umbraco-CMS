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
    private const string BootFailedHtml = "<html><body>BootFailed</body></html>";

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
    public async Task InvokeAsync_WhenLevelIsNotBootFailed_CallsNext(RuntimeLevel level)
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
