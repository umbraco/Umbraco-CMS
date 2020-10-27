using System;
using NUnit.Framework;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Configuration.Models.Extensions;

namespace Umbraco.Tests.UnitTests.Umbraco.Core.Configuration.Models.Extensions
{
    [TestFixture]
    public class HealthCheckSettingsExtensionsTests
    {
        [Test]
        public void Returns_Notification_Period_In_Milliseconds()
        {
            var settings = new HealthChecksSettings
            {
                Notification = new HealthChecksSettings.HealthCheckNotificationSettings
                {
                    PeriodInHours = 2,
                }
            };
            var result = settings.GetNotificationPeriodInMilliseconds();
            Assert.AreEqual(7200000, result);
        }

        [Test]
        public void Returns_Notification_Delay_In_Milliseconds()
        {
            var settings = new HealthChecksSettings
            {
                Notification = new HealthChecksSettings.HealthCheckNotificationSettings
                {
                    FirstRunTime = "1230",
                }
            };
            var now = DateTime.Now.Date.AddHours(12);
            var result = settings.GetNotificationDelayInMilliseconds(now, 0);
            Assert.AreEqual(1800000, result);
        }
    }
}
