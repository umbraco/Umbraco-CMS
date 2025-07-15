// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Configuration.Models.Validation;
using Umbraco.Cms.Core.Logging;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Configuration.Models.Validation
{
    [TestFixture]
    public class LoggingSettingsValidatorTests
    {
        [Test]
        public void Returns_Success_ForValid_Configuration()
        {
            var validator = new LoggingSettingsValidator();
            LoggingSettings options = BuildLoggingSettings();
            ValidateOptionsResult result = validator.Validate("settings", options);
            Assert.True(result.Succeeded);
        }

        [Test]
        public void Returns_Fail_For_Configuration_With_Invalid_FileNameFormatArguments()
        {
            var validator = new LoggingSettingsValidator();
            LoggingSettings options = BuildLoggingSettings(fileNameFormatArguments: "MachineName,Invalid");
            ValidateOptionsResult result = validator.Validate("settings", options);
            Assert.False(result.Succeeded);
        }

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
