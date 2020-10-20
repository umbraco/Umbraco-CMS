using NUnit.Framework;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Configuration.Models.Validation;

namespace Umbraco.Tests.UnitTests.Umbraco.Core.Configuration.Models.Validation
{
    [TestFixture]
    public class GlobalSettingsValidationTests
    {
        [Test]
        public void Returns_Success_ForValid_Configuration()
        {
            var validator = new GlobalSettingsValidator();
            var options = BuildGlobalSettings();
            var result = validator.Validate("settings", options);
            Assert.True(result.Succeeded);
        }

        [Test]
        public void Returns_Fail_For_Configuration_With_Invalid_SmtpFrom_Field()
        {
            var validator = new GlobalSettingsValidator();
            var options = BuildGlobalSettings(smtpEmail: "invalid");
            var result = validator.Validate("settings", options);
            Assert.False(result.Succeeded);
        }

        private static GlobalSettings BuildGlobalSettings(string smtpEmail = "test@test.com")
        {
            return new GlobalSettings
            {
                Smtp = new SmtpSettings
                {
                    From = smtpEmail,
                }
            };
        }
    }
}
