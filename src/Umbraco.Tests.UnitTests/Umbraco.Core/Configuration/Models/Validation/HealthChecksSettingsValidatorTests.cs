using NUnit.Framework;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Configuration.Models.Validation;

namespace Umbraco.Tests.UnitTests.Umbraco.Core.Configuration.Models.Validation
{
    [TestFixture]
    public class HealthChecksSettingsValidationTests
    {
        [Test]
        public void Returns_Success_ForValid_Configuration()
        {
            var validator = new HealthChecksSettingsValidator();
            var options = BuildHealthChecksSettings();
            var result = validator.Validate("settings", options);
            Assert.True(result.Succeeded);
        }

        [Test]
        public void Returns_Fail_For_Configuration_With_Invalid_Notification_FirstRunTime()
        {
            var validator = new HealthChecksSettingsValidator();
            var options = BuildHealthChecksSettings(firstRunTime: "25:00");
            var result = validator.Validate("settings", options);
            Assert.False(result.Succeeded);
        }

        private static HealthChecksSettings BuildHealthChecksSettings(string firstRunTime = "12:00")
        {
            return new HealthChecksSettings
            {
                Notification = new HealthChecksSettings.HealthCheckNotificationSettings
                {
                    Enabled = true,
                    FirstRunTime = firstRunTime,
                    PeriodInHours = 1,
                }
            };
        }
    }
}
