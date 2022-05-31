// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.HostedServices;
using Umbraco.Cms.Tests.Common;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.HostedServices;

[TestFixture]
public class LogScrubberTests
{
    private Mock<IAuditService> _mockAuditService;

    private const int MaxLogAgeInMinutes = 60;

    [Test]
    public async Task Does_Not_Execute_When_Server_Role_Is_Subscriber()
    {
        var sut = CreateLogScrubber(ServerRole.Subscriber);
        await sut.PerformExecuteAsync(null);
        VerifyLogsNotScrubbed();
    }

    [Test]
    public async Task Does_Not_Execute_When_Server_Role_Is_Unknown()
    {
        var sut = CreateLogScrubber(ServerRole.Unknown);
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
        var settings = new LoggingSettings { MaxLogAge = TimeSpan.FromMinutes(MaxLogAgeInMinutes) };

        var mockServerRegistrar = new Mock<IServerRoleAccessor>();
        mockServerRegistrar.Setup(x => x.CurrentServerRole).Returns(serverRole);

        var mockMainDom = new Mock<IMainDom>();
        mockMainDom.SetupGet(x => x.IsMainDom).Returns(isMainDom);

        var mockScope = new Mock<IScope>();
        var mockScopeProvider = new Mock<ICoreScopeProvider>();
        mockScopeProvider
            .Setup(x => x.CreateCoreScope(
                It.IsAny<IsolationLevel>(),
                It.IsAny<RepositoryCacheMode>(),
                It.IsAny<IEventDispatcher>(),
                It.IsAny<IScopedNotificationPublisher>(),
                It.IsAny<bool?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns(mockScope.Object);
        var mockLogger = new Mock<ILogger<LogScrubber>>();
        var mockProfilingLogger = new Mock<IProfilingLogger>();

        _mockAuditService = new Mock<IAuditService>();

        return new LogScrubber(
            mockMainDom.Object,
            mockServerRegistrar.Object,
            _mockAuditService.Object,
            new TestOptionsMonitor<LoggingSettings>(settings),
            mockScopeProvider.Object,
            mockLogger.Object,
            mockProfilingLogger.Object);
    }

    private void VerifyLogsNotScrubbed() => VerifyLogsScrubbed(Times.Never());

    private void VerifyLogsScrubbed() => VerifyLogsScrubbed(Times.Once());

    private void VerifyLogsScrubbed(Times times) =>
        _mockAuditService.Verify(x => x.CleanLogs(It.Is<int>(y => y == MaxLogAgeInMinutes)), times);
}
