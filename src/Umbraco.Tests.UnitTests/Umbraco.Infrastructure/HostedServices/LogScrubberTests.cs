using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Core.Sync;
using Umbraco.Infrastructure.HostedServices;

namespace Umbraco.Tests.UnitTests.Umbraco.Infrastructure.HostedServices
{
    [TestFixture]
    public class LogScrubberTests
    {
        private Mock<IAuditService> _mockAuditService;

        const int _maxLogAgeInMinutes = 60;

        [Test]
        public void Does_Not_Execute_When_Server_Role_Is_Replica()
        {
            var sut = CreateLogScrubber(serverRole: ServerRole.Replica);
            sut.ExecuteAsync(null);
            VerifyLogsNotScrubbed();
        }

        [Test]
        public void Does_Not_Execute_When_Server_Role_Is_Unknown()
        {
            var sut = CreateLogScrubber(serverRole: ServerRole.Unknown);
            sut.ExecuteAsync(null);
            VerifyLogsNotScrubbed();
        }

        [Test]
        public void Does_Not_Execute_When_Not_Main_Dom()
        {
            var sut = CreateLogScrubber(isMainDom: false);
            sut.ExecuteAsync(null);
            VerifyLogsNotScrubbed();
        }

        [Test]
        public void Executes_And_Srubs_Logs()
        {
            var sut = CreateLogScrubber();
            sut.ExecuteAsync(null);
            VerifyLogsScrubbed();
        }

        private LogScrubber CreateLogScrubber(
            ServerRole serverRole = ServerRole.Single,
            bool isMainDom = true)
        {
            var settings = new LoggingSettings
            {
                MaxLogAge = _maxLogAgeInMinutes,
            };

            var mockServerRegistrar = new Mock<IServerRegistrar>();
            mockServerRegistrar.Setup(x => x.GetCurrentServerRole()).Returns(serverRole);

            var mockMainDom = new Mock<IMainDom>();
            mockMainDom.SetupGet(x => x.IsMainDom).Returns(isMainDom);

            var mockScopeProvider = new Mock<IScopeProvider>();
            var mockLogger = new Mock<ILogger<LogScrubber>>();
            var mockProfilingLogger = new Mock<IProfilingLogger>();

            _mockAuditService = new Mock<IAuditService>();

            return new LogScrubber(mockMainDom.Object, mockServerRegistrar.Object, _mockAuditService.Object,
                Options.Create(settings), mockScopeProvider.Object, mockLogger.Object, mockProfilingLogger.Object);
        }

        private void VerifyLogsNotScrubbed()
        {
            VerifyLogsScrubbed(Times.Never());
        }

        private void VerifyLogsScrubbed()
        {
            VerifyLogsScrubbed(Times.Once());
        }

        private void VerifyLogsScrubbed(Times times)
        {
            _mockAuditService.Verify(x => x.CleanLogs(It.Is<int>(y => y == _maxLogAgeInMinutes)), times);
        }
    }
}
