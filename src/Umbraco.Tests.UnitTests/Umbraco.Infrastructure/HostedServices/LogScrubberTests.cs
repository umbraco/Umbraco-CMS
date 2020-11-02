using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Events;
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
        public async Task Does_Not_Execute_When_Server_Role_Is_Replica()
        {
            var sut = CreateLogScrubber(serverRole: ServerRole.Replica);
            await sut.PerformExecuteAsync(null);
            VerifyLogsNotScrubbed();
        }

        [Test]
        public async Task Does_Not_Execute_When_Server_Role_Is_Unknown()
        {
            var sut = CreateLogScrubber(serverRole: ServerRole.Unknown);
            await sut.PerformExecuteAsync(null);
            VerifyLogsNotScrubbed();
        }

        [Test]
        public async Task Does_Not_Execute_When_Not_Main_Dom()
        {
            var sut = CreateLogScrubber(isMainDom: false);
            await sut.PerformExecuteAsync(null);
            VerifyLogsNotScrubbed();
        }

        [Test]
        public async Task Executes_And_Scrubs_Logs()
        {
            var sut = CreateLogScrubber();
            await sut.PerformExecuteAsync(null);
            VerifyLogsScrubbed();
        }

        private LogScrubber CreateLogScrubber(
            ServerRole serverRole = ServerRole.Single,
            bool isMainDom = true)
        {
            var settings = new LoggingSettings
            {
                MaxLogAge = TimeSpan.FromMinutes(_maxLogAgeInMinutes),
            };

            var mockServerRegistrar = new Mock<IServerRegistrar>();
            mockServerRegistrar.Setup(x => x.GetCurrentServerRole()).Returns(serverRole);

            var mockMainDom = new Mock<IMainDom>();
            mockMainDom.SetupGet(x => x.IsMainDom).Returns(isMainDom);

            var mockScope = new Mock<IScope>();
            var mockScopeProvider = new Mock<IScopeProvider>();
            mockScopeProvider
                .Setup(x => x.CreateScope(It.IsAny<IsolationLevel>(), It.IsAny<RepositoryCacheMode>(), It.IsAny<IEventDispatcher>(), It.IsAny<bool?>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(mockScope.Object);
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
