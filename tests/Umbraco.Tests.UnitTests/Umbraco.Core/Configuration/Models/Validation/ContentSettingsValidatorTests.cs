// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Configuration.Models.Validation;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Configuration.Models.Validation
{
/// <summary>
/// Contains unit tests for validating content settings using the <see cref="ContentSettingsValidator"/> class.
/// </summary>
    [TestFixture]
    public class ContentSettingsValidatorTests
    {
    /// <summary>
    /// Tests that the ContentSettingsValidator returns success for a valid configuration.
    /// </summary>
        [Test]
        public void Returns_Success_ForValid_Configuration()
        {
            var validator = new ContentSettingsValidator();
            ContentSettings options = BuildContentSettings();
            ValidateOptionsResult result = validator.Validate("settings", options);
            Assert.True(result.Succeeded);
        }

    /// <summary>
    /// Tests that the ContentSettingsValidator fails validation when the Error404Collection contains an entry with an empty culture.
    /// </summary>
        [Test]
        public void Returns_Fail_For_Configuration_With_Invalid_Error404Collection_Due_To_Empty_Culture()
        {
            var validator = new ContentSettingsValidator();
            ContentSettings options = BuildContentSettings(culture: string.Empty);
            ValidateOptionsResult result = validator.Validate("settings", options);
            Assert.False(result.Succeeded);
        }

    /// <summary>
    /// Tests that the validator returns a failure result when the configuration contains an invalid AutoFillImageProperties collection.
    /// </summary>
        [Test]
        public void Returns_Fail_For_Configuration_With_Invalid_AutoFillImageProperties_Collection()
        {
            var validator = new ContentSettingsValidator();
            ContentSettings options = BuildContentSettings(culture: string.Empty);
            ValidateOptionsResult result = validator.Validate("settings", options);
            Assert.False(result.Succeeded);
        }

        private static ContentSettings BuildContentSettings(string culture = "en-US", string autoFillImagePropertyAlias = "testAlias") =>
            new ContentSettings
            {
                Error404Collection =
                [
                    new() { Culture = culture, ContentId = 1 },
                ],
                Imaging =
                {
                    AutoFillImageProperties =
                    {
                        new() { Alias = autoFillImagePropertyAlias, WidthFieldAlias = "w", HeightFieldAlias = "h", LengthFieldAlias = "l", ExtensionFieldAlias = "e" },
                    },
                },
            };
    }
}
