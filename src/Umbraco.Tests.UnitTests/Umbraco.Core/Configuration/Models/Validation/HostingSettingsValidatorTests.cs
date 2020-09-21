using NUnit.Framework;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Configuration.Models.Validation;

namespace Umbraco.Tests.UnitTests.Umbraco.Core.Configuration.Models.Validation
{
    [TestFixture]
    public class HostingSettingsValidatorTests
    {
        [Test]
        public void Returns_Success_ForValid_Configuration()
        {
            var validator = new HostingSettingsValidator();
            var options = new HostingSettings();
            var result = validator.Validate("settings", options);
            Assert.True(result.Succeeded);
        }

        [Test]
        public void Returns_Fail_For_Configuration_With_Invalid_LocalTempStorage_Field()
        {
            var validator = new HostingSettingsValidator();
            var options = new HostingSettings { LocalTempStorageLocation = "invalid" };
            var result = validator.Validate("settings", options);
            Assert.False(result.Succeeded);
        }
    }
}
