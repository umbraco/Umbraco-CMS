// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Configuration.Models.Validation;
using Umbraco.Cms.Core.Logging;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Configuration.Models.Validation
{
/// <summary>
/// Contains unit tests for the <see cref="LoggingSettingsValidator"/> class to ensure correct validation logic for logging settings.
/// </summary>
    [TestFixture]
    public class LoggingSettingsValidatorTests
    {
    /// <summary>
    /// Tests that the LoggingSettingsValidator returns success for a valid configuration.
    /// </summary>
        [Test]
        public void Returns_Success_ForValid_Configuration()
        {
            var validator = new LoggingSettingsValidator();
            LoggingSettings options = BuildLoggingSettings();
            ValidateOptionsResult result = validator.Validate("settings", options);
            Assert.True(result.Succeeded);
        }

    /// <summary>
    /// Tests that the validator fails when the configuration contains invalid file name format arguments.
    /// </summary>
        [Test]
        public void Returns_Fail_For_Configuration_With_Invalid_FileNameFormatArguments()
        {
            var validator = new LoggingSettingsValidator();
            LoggingSettings options = BuildLoggingSettings(fileNameFormatArguments: "MachineName,Invalid");
            ValidateOptionsResult result = validator.Validate("settings", options);
            Assert.False(result.Succeeded);
        }

    /// <summary>
    /// Tests that the validator fails when the configuration contains an invalid file name format.
    /// </summary>
        [Test]
        public void Returns_Fail_For_Configuration_With_Invalid_FileNameFormat()
        {
            var validator = new LoggingSettingsValidator();
            LoggingSettings options = BuildLoggingSettings(fileNameFormat: "InvalidAsTooManyPlaceholders_{0}_{1}");
            ValidateOptionsResult result = validator.Validate("settings", options);
            Assert.False(result.Succeeded);
        }

        private static LoggingSettings BuildLoggingSettings(
            string fileNameFormat = LoggingConfiguration.DefaultLogFileNameFormat,
            string fileNameFormatArguments = LoggingConfiguration.DefaultLogFileNameFormatArguments) =>
            new LoggingSettings
            {
                FileNameFormat = fileNameFormat,
                FileNameFormatArguments = fileNameFormatArguments,
            };
    }
}
