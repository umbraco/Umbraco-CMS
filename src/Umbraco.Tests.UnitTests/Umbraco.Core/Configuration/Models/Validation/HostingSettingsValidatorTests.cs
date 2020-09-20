using NUnit.Framework;
using Umbraco.Core.Configuration.Models.Validation;
using Umbraco.Tests.Common.Builders;

namespace Umbraco.Tests.UnitTests.Umbraco.Core.Configuration.Models.Validation
{
    [TestFixture]
    public class HostingSettingsValidatorTests
    {
        [Test]
        public void ReturnsSuccessResponseForValidConfiguration()
        {
            var validator = new HostingSettingsValidator();
            var options = new HostingSettingsBuilder().Build();
            var result = validator.Validate("settings", options);
            Assert.True(result.Succeeded);
        }

        [Test]
        public void ReturnsFailResponseForConfigurationWithInvalidLocalTempStorageLocationField()
        {
            var validator = new HostingSettingsValidator();
            var options = new HostingSettingsBuilder().WithLocalTempStorageLocation("invalid").Build();
            var result = validator.Validate("settings", options);
            Assert.False(result.Succeeded);
        }
    }
}
