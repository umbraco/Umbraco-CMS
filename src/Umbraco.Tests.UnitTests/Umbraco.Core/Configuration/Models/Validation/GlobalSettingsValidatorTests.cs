using NUnit.Framework;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Configuration.Models.Validation;
using Umbraco.Tests.Common.Builders;

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

        [Test]
        public void Returns_Fail_For_Configuration_With_Invalid_SmtpDeliveryMethod_Field()
        {
            var validator = new GlobalSettingsValidator();
            var options = BuildGlobalSettings(smtpDeliveryMethod: "invalid");
            var result = validator.Validate("settings", options);
            Assert.False(result.Succeeded);
        }

        private static GlobalSettings BuildGlobalSettings(string smtpEmail = "test@test.com", string smtpDeliveryMethod = "Network")
        {
            return new GlobalSettingsBuilder()
                .AddSmtpSettings()
                    .WithFrom(smtpEmail)
                    .WithDeliveryMethod(smtpDeliveryMethod)
                    .Done()
                .Build();
        }
    }
}
