using NUnit.Framework;
using Umbraco.Core.Configuration.Models.Validation;
using Umbraco.Tests.Common.Builders;

namespace Umbraco.Tests.UnitTests.Umbraco.Core.Configuration.Models.Validation
{
    [TestFixture]
    public class RequestHandlerSettingsValidatorTests
    {
        [Test]
        public void Returns_Success_ForValid_Configuration()
        {
            var validator = new RequestHandlerSettingsValidator();
            var options = new RequestHandlerSettingsBuilder().Build();
            var result = validator.Validate("settings", options);
            Assert.True(result.Succeeded);
        }

        [Test]
        public void Returns_Fail_For_Configuration_With_Invalid_ConvertUrlsToAscii_Field()
        {
            var validator = new RequestHandlerSettingsValidator();
            var options = new RequestHandlerSettingsBuilder().WithConvertUrlsToAscii("invalid").Build();
            var result = validator.Validate("settings", options);
            Assert.False(result.Succeeded);
        }
    }
}
