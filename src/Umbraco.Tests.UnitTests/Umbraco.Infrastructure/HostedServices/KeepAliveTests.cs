using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;
using Umbraco.Core.Sync;
using Umbraco.Infrastructure.HostedServices;
using Umbraco.Web;

namespace Umbraco.Tests.UnitTests.Umbraco.Infrastructure.HostedServices
{
    [TestFixture]
    public class KeepAliveTests
    {
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;

        private const string _applicationUrl = "https://mysite.com";

        [Test]
        public void Does_Not_Execute_When_Not_Enabled()
        {
            var sut = CreateKeepAlive(enabled: false);
            sut.ExecuteAsync(null);
            VerifyKeepAliveRequestNotSent();
        }

        [Test]
        public void Does_Not_Execute_When_Server_Role_Is_Replica()
        {
            var sut = CreateKeepAlive(serverRole: ServerRole.Replica);
            sut.ExecuteAsync(null);
            VerifyKeepAliveRequestNotSent();
        }

        [Test]
        public void Does_Not_Execute_When_Server_Role_Is_Unknown()
        {
            var sut = CreateKeepAlive(serverRole: ServerRole.Unknown);
            sut.ExecuteAsync(null);
            VerifyKeepAliveRequestNotSent();
        }

        [Test]
        public void Does_Not_Execute_When_Not_Main_Dom()
        {
            var sut = CreateKeepAlive(isMainDom: false);
            sut.ExecuteAsync(null);
            VerifyKeepAliveRequestNotSent();
        }

        [Test]
        public void Executes_And_Calls_Ping_Url()
        {
            var sut = CreateKeepAlive();
            sut.ExecuteAsync(null);
            VerifyKeepAliveRequestSent();
        }

        private KeepAlive CreateKeepAlive(
            bool enabled = true,
            ServerRole serverRole = ServerRole.Single,
            bool isMainDom = true)
        {
            var settings = new KeepAliveSettings
            {
                DisableKeepAliveTask = !enabled,
            };

            var mockRequestAccessor = new Mock<IRequestAccessor>();
            mockRequestAccessor.Setup(x => x.GetApplicationUrl()).Returns(new Uri(_applicationUrl));

            var mockServerRegistrar = new Mock<IServerRegistrar>();
            mockServerRegistrar.Setup(x => x.GetCurrentServerRole()).Returns(serverRole);

            var mockMainDom = new Mock<IMainDom>();
            mockMainDom.SetupGet(x => x.IsMainDom).Returns(isMainDom);

            var mockScopeProvider = new Mock<IScopeProvider>();
            var mockLogger = new Mock<ILogger<KeepAlive>>();
            var mockProfilingLogger = new Mock<IProfilingLogger>();

            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK))
                .Verifiable();
            _mockHttpMessageHandler.As<IDisposable>().Setup(s => s.Dispose());
            var httpClient = new HttpClient(_mockHttpMessageHandler.Object);

            var mockHttpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            return new KeepAlive(mockRequestAccessor.Object, mockMainDom.Object, Options.Create(settings),
                mockLogger.Object, mockProfilingLogger.Object, mockServerRegistrar.Object, mockHttpClientFactory.Object);
        }

        private void VerifyKeepAliveRequestNotSent()
        {
            VerifyKeepAliveRequestSentTimes(Times.Never());
        }

        private void VerifyKeepAliveRequestSent()
        {
            VerifyKeepAliveRequestSentTimes(Times.Once());
        }

        private void VerifyKeepAliveRequestSentTimes(Times times)
        {
            _mockHttpMessageHandler.Protected().Verify("SendAsync",
                times,
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == $"{_applicationUrl}/api/keepalive/ping"),
                ItExpr.IsAny<CancellationToken>());
        }
    }
}
