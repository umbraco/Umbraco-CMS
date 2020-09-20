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
        public void ReturnsSuccessResponseForValidConfiguration()
        {
            var validator = new GlobalSettingsValidator();
            var options = BuildGlobalSettings("test@test.com");
            var result = validator.Validate("settings", options);
            Assert.True(result.Succeeded);
        }

        [Test]
        public void ReturnsFailResponseForConfigurationWithInvalidSmtpFromField()
        {
            var validator = new GlobalSettingsValidator();
            var options = BuildGlobalSettings("invalid");
            var result = validator.Validate("settings", options);
            Assert.False(result.Succeeded);
        }

        private static GlobalSettings BuildGlobalSettings(string smtpEmail)
        {
            return new GlobalSettingsBuilder()
                .AddSmtpSettings()
                    .WithFrom(smtpEmail)
                    .Done()
                .Build();
        }
    }
}
