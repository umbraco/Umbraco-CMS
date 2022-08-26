// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Infrastructure.HostedServices;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.HostedServices;

[TestFixture]
public class RecurringHostedServiceBaseTests
{
    [TestCase("30 12 * * *", 30)]
    [TestCase("15 18 * * *", (60 * 6) + 15)]
    [TestCase("0 3 * * *", 60 * 15)]
    [TestCase("0 3 2 * *", (24 * 60 * 1) + (60 * 15))]
    [TestCase("0 6 * * 3", (24 * 60 * 3) + (60 * 18))]
    public void Returns_Notification_Delay_From_Provided_Time(string firstRunTime, int expectedDelayInMinutes)
    {
        var cronTabParser = new NCronTabParser();
        var logger = Mock.Of<ILogger>();
        var now = new DateTime(2020, 10, 31, 12, 0, 0);
        var result = RecurringHostedServiceBase.GetDelay(firstRunTime, cronTabParser, logger, now, TimeSpan.Zero);
        Assert.AreEqual(expectedDelayInMinutes, result.TotalMinutes);
    }

    [Test]
    public void Returns_Notification_Delay_From_Default_When_Provided_Time_Too_Close_To_Current_Time()
    {
        var firstRunTime = "30 12 * * *";
        var cronTabParser = new NCronTabParser();
        var logger = Mock.Of<ILogger>();
        var now = new DateTime(2020, 10, 31, 12, 25, 0);
        var defaultDelay = TimeSpan.FromMinutes(10);
        var result = RecurringHostedServiceBase.GetDelay(firstRunTime, cronTabParser, logger, now, defaultDelay);
        Assert.AreEqual(defaultDelay.TotalMinutes, result.TotalMinutes);
    }

    [Test]
    public void Logs_And_Returns_Notification_Delay_From_Default_When_Provided_Time_Is_Not_Valid()
    {
        var firstRunTime = "invalid";
        var cronTabParser = new NCronTabParser();
        var logger = new Mock<ILogger>();
        var now = new DateTime(2020, 10, 31, 12, 25, 0);
        var defaultDelay = TimeSpan.FromMinutes(10);
        var result = RecurringHostedServiceBase.GetDelay(firstRunTime, cronTabParser, logger.Object, now, defaultDelay);
        Assert.AreEqual(defaultDelay, result);

        logger.Verify(
            logger => logger.Log(
                It.Is<LogLevel>(y => y == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
