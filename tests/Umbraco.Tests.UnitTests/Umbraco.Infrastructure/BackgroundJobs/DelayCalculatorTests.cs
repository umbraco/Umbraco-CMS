// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Infrastructure.BackgroundJobs;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.BackgroundJobs;

[TestFixture]
public class DelayCalculatorTests
{
    [TestCase("30 12 * * *", 30)]
    [TestCase("15 18 * * *", (60 * 6) + 15)]
    [TestCase("0 3 * * *", 60 * 15)]
    [TestCase("0 3 2 * *", (24 * 60 * 1) + (60 * 15))]
    [TestCase("0 6 * * 3", (24 * 60 * 3) + (60 * 18))]
    public void GetDelay_Returns_Delay_From_CronTab(string firstRunTime, int expectedDelayInMinutes)
    {
        var cronTabParser = new NCronTabParser();
        var logger = Mock.Of<ILogger>();
        var now = new DateTime(2020, 10, 31, 12, 0, 0);

        TimeSpan result = DelayCalculator.GetDelay(firstRunTime, cronTabParser, logger, now, TimeSpan.Zero);

        Assert.AreEqual(expectedDelayInMinutes, result.TotalMinutes);
    }

    [Test]
    public void GetDelay_Returns_Default_When_CronTab_Too_Close_To_Current_Time()
    {
        var cronTabParser = new NCronTabParser();
        var logger = Mock.Of<ILogger>();
        var now = new DateTime(2020, 10, 31, 12, 25, 0);
        var defaultDelay = TimeSpan.FromMinutes(10);

        TimeSpan result = DelayCalculator.GetDelay("30 12 * * *", cronTabParser, logger, now, defaultDelay);

        Assert.AreEqual(defaultDelay.TotalMinutes, result.TotalMinutes);
    }

    [Test]
    public void GetDelay_Returns_Default_When_FirstRunTime_Is_Empty()
    {
        var cronTabParser = new NCronTabParser();
        var logger = Mock.Of<ILogger>();
        var now = new DateTime(2020, 10, 31, 12, 0, 0);
        var defaultDelay = TimeSpan.FromMinutes(3);

        TimeSpan result = DelayCalculator.GetDelay(string.Empty, cronTabParser, logger, now, defaultDelay);

        Assert.AreEqual(defaultDelay, result);
    }

    [Test]
    public void GetDelay_Logs_Warning_And_Returns_Default_When_CronTab_Is_Invalid()
    {
        var cronTabParser = new NCronTabParser();
        var logger = new Mock<ILogger>();
        var now = new DateTime(2020, 10, 31, 12, 25, 0);
        var defaultDelay = TimeSpan.FromMinutes(10);

        TimeSpan result = DelayCalculator.GetDelay("invalid", cronTabParser, logger.Object, now, defaultDelay);

        Assert.AreEqual(defaultDelay, result);
        logger.Verify(
            l => l.Log(
                It.Is<LogLevel>(y => y == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Test]
    public void GetDelay_With_TimeProvider_Uses_Provider_Time()
    {
        var cronTabParser = new NCronTabParser();
        var logger = Mock.Of<ILogger>();
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2020, 10, 31, 12, 0, 0, TimeSpan.Zero));

        // "30 12 * * *" = 12:30 daily. From 12:00, that's 30 minutes.
        TimeSpan result = DelayCalculator.GetDelay("30 12 * * *", cronTabParser, logger, timeProvider, TimeSpan.Zero);

        Assert.AreEqual(30, result.TotalMinutes);
    }
}
