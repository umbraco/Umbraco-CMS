// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Configuration.Models.Extensions
{
    [TestFixture]
    public class HealthCheckSettingsExtensionsTests
    {
        private ICronTabParser CronTabParser => new NCronTabParser();

        [TestCase("30 12 * * *", 30)]
        [TestCase("15 18 * * *", (60 * 6) + 15)]
        [TestCase("0 3 * * *", 60 * 15)]
        [TestCase("0 3 2 * *", (24 * 60 * 1) + (60 * 15))]
        [TestCase("0 6 * * 3", (24 * 60 * 3) + (60 * 18))]
        public void Returns_Notification_Delay_From_Provided_Time(string firstRunTimeCronExpression, int expectedDelayInMinutes)
        {
            var settings = new HealthChecksSettings
            {
                Notification = new HealthChecksNotificationSettings
                {
                    FirstRunTime = firstRunTimeCronExpression,
                }
            };
            var now = new DateTime(2020, 10, 31, 12, 0, 0);
            TimeSpan result = settings.GetNotificationDelay(CronTabParser, now, TimeSpan.Zero);
            Assert.AreEqual(expectedDelayInMinutes, result.TotalMinutes);
        }

        [Test]
        public void Returns_Notification_Delay_From_Default_When_Provided_Time_Too_Close_To_Current_Time()
        {
            var settings = new HealthChecksSettings
            {
                Notification = new HealthChecksNotificationSettings
                {
                    FirstRunTime = "30 12 * * *",
                }
            };
            var now = new DateTime(2020, 10, 31, 12, 25, 0);
            TimeSpan result = settings.GetNotificationDelay(CronTabParser, now, TimeSpan.FromMinutes(10));
            Assert.AreEqual(10, result.TotalMinutes);
        }
    }
}
