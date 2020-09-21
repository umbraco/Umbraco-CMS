using NUnit.Framework;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Configuration.Models.Validation;
using Umbraco.Core.Macros;
using Umbraco.Tests.Common.Builders;

namespace Umbraco.Tests.UnitTests.Umbraco.Core.Configuration.Models.Validation
{
    [TestFixture]
    public class ContentSettingsValidationTests
    {
        [Test]
        public void Returns_Success_ForValid_Configuration()
        {
            var validator = new ContentSettingsValidator();
            var options = BuildContentSettings();
            var result = validator.Validate("settings", options);
            Assert.True(result.Succeeded);
        }

        [Test]
        public void Returns_Fail_For_Configuration_With_Invalid_MacroErrors_Field()
        {
            var validator = new ContentSettingsValidator();
            var options = BuildContentSettings(macroErrors: "invalid");
            var result = validator.Validate("settings", options);
            Assert.False(result.Succeeded);
        }

        [Test]
        public void Returns_Fail_For_Configuration_With_Invalid_Error404Collection_Due_To_Duplicate_Id()
        {
            var validator = new ContentSettingsValidator();
            var options = BuildContentSettings(contentXPath: "/aaa/bbb");
            var result = validator.Validate("settings", options);
            Assert.False(result.Succeeded);
        }

        [Test]
        public void Returns_Fail_For_Configuration_With_Invalid_Error404Collection_Due_To_Empty_Culture()
        {
            var validator = new ContentSettingsValidator();
            var options = BuildContentSettings(culture: string.Empty);
            var result = validator.Validate("settings", options);
            Assert.False(result.Succeeded);
        }

        [Test]
        public void Returns_Fail_For_Configuration_With_Invalid_AutoFillImageProperties_Collection()
        {
            var validator = new ContentSettingsValidator();
            var options = BuildContentSettings(culture: string.Empty);
            var result = validator.Validate("settings", options);
            Assert.False(result.Succeeded);
        }

        private static ContentSettings BuildContentSettings(string macroErrors = "inline", string culture = "en-US", string contentXPath = "", string autoFillImagePropertyAlias = "testAlias")
        {
            return new ContentSettingsBuilder()
                .WithMacroErrors(macroErrors)
                .AddErrorPage()
                    .WithCulture(culture)
                    .WithContentId(1)
                    .WithContentXPath(contentXPath)
                    .Done()
                .AddImaging()
                    .AddAutoFillImageProperty()
                        .WithAlias(autoFillImagePropertyAlias)
                        .Done()
                    .Done()
                .Build();
        }
    }
}
