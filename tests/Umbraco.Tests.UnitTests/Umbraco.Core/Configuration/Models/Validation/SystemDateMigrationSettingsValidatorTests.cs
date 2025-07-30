// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Configuration.Models.Validation;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Configuration.Models.Validation
{
    [TestFixture]
    public class SystemDateMigrationSettingsValidatorTests
    {
        [Test]
        public void Returns_Success_For_Empty_Configuration()
        {
            var validator = new SystemDateMigrationSettingsValidator();
            SystemDateMigrationSettings options = BuildSystemDateMigrationSettings();
            ValidateOptionsResult result = validator.Validate("settings", options);
            Assert.True(result.Succeeded);
        }

        [Test]
        public void Returns_Success_For_Valid_Configuration()
        {
            var validator = new SystemDateMigrationSettingsValidator();
            SystemDateMigrationSettings options = BuildSystemDateMigrationSettings(localServerTimeZone: "Central European Standard Time");
            ValidateOptionsResult result = validator.Validate("settings", options);
            Assert.True(result.Succeeded);
        }

        [Test]
        public void Returns_Fail_For_Configuration_With_Invalid_LocalServerTimeZone()
        {
            var validator = new SystemDateMigrationSettingsValidator();
            SystemDateMigrationSettings options = BuildSystemDateMigrationSettings(localServerTimeZone: "Invalid Time Zone");
            ValidateOptionsResult result = validator.Validate("settings", options);
            Assert.False(result.Succeeded);
        }

        private static SystemDateMigrationSettings BuildSystemDateMigrationSettings(string? localServerTimeZone = null) =>
            new SystemDateMigrationSettings
            {
                LocalServerTimeZone = localServerTimeZone,
            };
    }
}
