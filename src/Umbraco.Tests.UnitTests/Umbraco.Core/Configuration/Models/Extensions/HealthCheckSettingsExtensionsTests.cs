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
        public void Returns_Notification_Delay_From_Provided_Time()
        {
            var settings = new HealthChecksSettings
            {
                Notification = new HealthChecksNotificationSettings
                {
                    FirstRunTime = "1230",
                }
            };
            var now = DateTime.Now.Date.AddHours(12);
            var result = settings.GetNotificationDelay(now, TimeSpan.Zero);
            Assert.AreEqual(30, result.Minutes);
        }

        [Test]
        public void Returns_Notification_Delay_From_Default_When_Provided_Time_Too_Close_To_Current_Time()
        {
            var settings = new HealthChecksSettings
            {
                Notification = new HealthChecksNotificationSettings
                {
                    FirstRunTime = "1230",
                }
            };
            var now = DateTime.Now.Date.AddHours(12).AddMinutes(25);
            var result = settings.GetNotificationDelay(now, TimeSpan.FromMinutes(10));
            Assert.AreEqual(10, result.Minutes);
        }
    }
}
