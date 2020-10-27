using NUnit.Framework;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Configuration.Models.Validation;

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

        private static ContentSettings BuildContentSettings(string culture = "en-US", string contentXPath = "", string autoFillImagePropertyAlias = "testAlias")
        {
            return new ContentSettings
            {
                Error404Collection = new ContentErrorPage[]
                {
                    new ContentErrorPage { Culture = culture, ContentId = 1, ContentXPath = contentXPath },
                },
                Imaging = new ContentImagingSettings
                {
                    AutoFillImageProperties = new ImagingAutoFillUploadField[]
                    {
                        new ImagingAutoFillUploadField { Alias = autoFillImagePropertyAlias, WidthFieldAlias = "w", HeightFieldAlias = "h", LengthFieldAlias = "l", ExtensionFieldAlias = "e" }
                    }
                }
            };
        }
    }
}
