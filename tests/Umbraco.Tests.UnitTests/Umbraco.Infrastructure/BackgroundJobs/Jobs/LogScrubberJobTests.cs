// Copyright (c) Umbraco.
// See LICENSE for more details.

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
using Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs;
using Umbraco.Cms.Infrastructure.HostedServices;
using Umbraco.Cms.Tests.Common;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.BackgroundJobs.Jobs;

[TestFixture]
public class LogScrubberJobTests
{
    private Mock<IAuditService> _mockAuditService;

    private const int MaxLogAgeInMinutes = 60;

    [Test]
    public async Task Executes_And_Scrubs_Logs()
    {
        var sut = CreateLogScrubber();
        await sut.RunJobAsync();
        VerifyLogsScrubbed();
    }

    private LogScrubberJob CreateLogScrubber()
    {
        var settings = new LoggingSettings { MaxLogAge = TimeSpan.FromMinutes(MaxLogAgeInMinutes) };

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
        var mockLogger = new Mock<ILogger<LogScrubberJob>>();
        var mockProfilingLogger = new Mock<IProfilingLogger>();

        _mockAuditService = new Mock<IAuditService>();

        return new LogScrubberJob(
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
