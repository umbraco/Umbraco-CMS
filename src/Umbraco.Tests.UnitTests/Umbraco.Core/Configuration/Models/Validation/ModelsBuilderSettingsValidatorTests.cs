using NUnit.Framework;
using Umbraco.Core.Configuration.Models.Validation;
using Umbraco.Tests.Common.Builders;

namespace Umbraco.Tests.UnitTests.Umbraco.Core.Configuration.Models.Validation
{
    [TestFixture]
    public class ModelsBuilderSettingsValidatorTests
    {
        [Test]
        public void Returns_Success_ForValid_Configuration()
        {
            var validator = new ModelsBuilderSettingsValidator();
            var options = new ModelsBuilderSettingsBuilder().Build();
            var result = validator.Validate("settings", options);
            Assert.True(result.Succeeded);
        }

        [Test]
        public void Returns_Fail_For_Configuration_With_Invalid_ModelsMode_Field()
        {
            var validator = new ModelsBuilderSettingsValidator();
            var options = new ModelsBuilderSettingsBuilder().WithModelsMode("invalid").Build();
            var result = validator.Validate("settings", options);
            Assert.False(result.Succeeded);
        }
    }
}
