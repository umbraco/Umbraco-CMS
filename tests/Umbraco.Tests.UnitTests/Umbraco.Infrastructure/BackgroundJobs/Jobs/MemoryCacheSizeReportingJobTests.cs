// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.BackgroundJobs.Jobs;

[TestFixture]
public class MemoryCacheSizeReportingJobTests
{
    [Test]
    public async Task Can_Report_Each_Cache_When_Debug_Enabled()
    {
        Mock<IMemoryCacheSizeReporter> reporterA = CreateReporter("A", 1);
        Mock<IMemoryCacheSizeReporter> reporterB = CreateReporter("B", 2);

        MemoryCacheSizeReportingJob sut = CreateJob(debugEnabled: true, reporterA.Object, reporterB.Object);

        await sut.RunJobAsync(CancellationToken.None);

        reporterA.Verify(x => x.GetApproximateCount(), Times.Once);
        reporterA.Verify(x => x.GetApproximateBytes(), Times.Once);
        reporterB.Verify(x => x.GetApproximateCount(), Times.Once);
        reporterB.Verify(x => x.GetApproximateBytes(), Times.Once);
    }

    [Test]
    public async Task Cannot_Report_When_Debug_Disabled()
    {
        Mock<IMemoryCacheSizeReporter> reporter = CreateReporter("A", 1);

        MemoryCacheSizeReportingJob sut = CreateJob(debugEnabled: false, reporter.Object);

        await sut.RunJobAsync(CancellationToken.None);

        reporter.Verify(x => x.GetApproximateCount(), Times.Never);
        reporter.Verify(x => x.GetApproximateBytes(), Times.Never);
    }

    private static Mock<IMemoryCacheSizeReporter> CreateReporter(string name, long count)
    {
        var reporter = new Mock<IMemoryCacheSizeReporter>();
        reporter.SetupGet(x => x.CacheName).Returns(name);
        reporter.Setup(x => x.GetApproximateCount()).Returns(count);
        return reporter;
    }

    private static MemoryCacheSizeReportingJob CreateJob(bool debugEnabled, params IMemoryCacheSizeReporter[] reporters)
    {
        var logger = new Mock<ILogger<MemoryCacheSizeReportingJob>>();
        logger.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(debugEnabled);
        return new MemoryCacheSizeReportingJob(reporters, logger.Object);
    }
}
