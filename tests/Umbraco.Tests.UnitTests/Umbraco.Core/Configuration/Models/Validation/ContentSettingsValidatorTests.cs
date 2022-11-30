// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Configuration.Models.Validation;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Configuration.Models.Validation
{
    [TestFixture]
    public class ContentSettingsValidatorTests
    {
        [Test]
        public void Returns_Success_ForValid_Configuration()
        {
            var validator = new ContentSettingsValidator();
            ContentSettings options = BuildContentSettings();
            ValidateOptionsResult result = validator.Validate("settings", options);
            Assert.True(result.Succeeded);
        }

        [Test]
        public void Returns_Fail_For_Configuration_With_Invalid_Error404Collection_Due_To_Duplicate_Id()
        {
            var validator = new ContentSettingsValidator();
            ContentSettings options = BuildContentSettings(contentXPath: "/aaa/bbb");
            ValidateOptionsResult result = validator.Validate("settings", options);
            Assert.False(result.Succeeded);
        }

        [Test]
        public void Returns_Fail_For_Configuration_With_Invalid_Error404Collection_Due_To_Empty_Culture()
        {
            var validator = new ContentSettingsValidator();
            ContentSettings options = BuildContentSettings(culture: string.Empty);
            ValidateOptionsResult result = validator.Validate("settings", options);
            Assert.False(result.Succeeded);
        }

        [Test]
        public void Returns_Fail_For_Configuration_With_Invalid_AutoFillImageProperties_Collection()
        {
            var validator = new ContentSettingsValidator();
            ContentSettings options = BuildContentSettings(culture: string.Empty);
            ValidateOptionsResult result = validator.Validate("settings", options);
            Assert.False(result.Succeeded);
        }

        private static ContentSettings BuildContentSettings(string culture = "en-US", string contentXPath = "", string autoFillImagePropertyAlias = "testAlias") =>
            new ContentSettings
            {
                Error404Collection = new ContentErrorPage[]
                    {
                        new() { Culture = culture, ContentId = 1, ContentXPath = contentXPath },
                    },
                Imaging = new ContentImagingSettings
                {
                    AutoFillImageProperties = new ImagingAutoFillUploadField[]
                        {
                            new() { Alias = autoFillImagePropertyAlias, WidthFieldAlias = "w", HeightFieldAlias = "h", LengthFieldAlias = "l", ExtensionFieldAlias = "e" },
                        },
                },
            };
    }
}
