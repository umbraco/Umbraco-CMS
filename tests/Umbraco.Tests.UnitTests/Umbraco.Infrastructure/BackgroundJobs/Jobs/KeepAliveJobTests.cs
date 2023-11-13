// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs;
using Umbraco.Cms.Infrastructure.HostedServices;
using Umbraco.Cms.Tests.Common;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.BackgroundJobs.Jobs;

[TestFixture]
public class KeepAliveJobTests
{
    private Mock<HttpMessageHandler> _mockHttpMessageHandler;

    private const string ApplicationUrl = "https://mysite.com";

    [Test]
    public async Task Does_Not_Execute_When_Not_Enabled()
    {
        var sut = CreateKeepAlive(false);
        await sut.RunJobAsync();
        VerifyKeepAliveRequestNotSent();
    }


    [Test]
    public async Task Executes_And_Calls_Ping_Url()
    {
        var sut = CreateKeepAlive();
        await sut.RunJobAsync();
        VerifyKeepAliveRequestSent();
    }

    private KeepAliveJob CreateKeepAlive(
        bool enabled = true)
    {
        var settings = new KeepAliveSettings { DisableKeepAliveTask = !enabled };

        var mockHostingEnvironment = new Mock<IHostingEnvironment>();
        mockHostingEnvironment.SetupGet(x => x.ApplicationMainUrl).Returns(new Uri(ApplicationUrl));
        mockHostingEnvironment.Setup(x => x.ToAbsolute(It.IsAny<string>()))
            .Returns((string s) => s.TrimStart('~'));

        var mockScopeProvider = new Mock<IScopeProvider>();
        var mockLogger = new Mock<ILogger<KeepAliveJob>>();
        var mockProfilingLogger = new Mock<IProfilingLogger>();

        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK))
            .Verifiable();
        _mockHttpMessageHandler.As<IDisposable>().Setup(s => s.Dispose());
        var httpClient = new HttpClient(_mockHttpMessageHandler.Object);

        var mockHttpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        return new KeepAliveJob(
            mockHostingEnvironment.Object,
            new TestOptionsMonitor<KeepAliveSettings>(settings),
            mockLogger.Object,
            mockProfilingLogger.Object,
            mockHttpClientFactory.Object);
    }

    private void VerifyKeepAliveRequestNotSent() => VerifyKeepAliveRequestSentTimes(Times.Never());

    private void VerifyKeepAliveRequestSent() => VerifyKeepAliveRequestSentTimes(Times.Once());

    private void VerifyKeepAliveRequestSentTimes(Times times) => _mockHttpMessageHandler.Protected()
        .Verify(
            "SendAsync",
            times,
            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == $"{ApplicationUrl}/api/keepalive/ping"),
            ItExpr.IsAny<CancellationToken>());
}
