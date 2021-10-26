// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Configuration.Models.Validation;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Configuration.Models.Validation
{
    [TestFixture]
    public class GlobalSettingsValidatorTests
    {
        [Test]
        public void Returns_Success_ForValid_Configuration()
        {
            var validator = new GlobalSettingsValidator();
            var options = new GlobalSettings();
            ValidateOptionsResult result = validator.Validate("settings", options);
            Assert.True(result.Succeeded);
        }

        [Test]
        public void Returns_Fail_For_Configuration_With_Invalid_SmtpFrom_Field()
        {
            var validator = new GlobalSettingsValidator();
            var options = new GlobalSettings
            {
                Smtp = new SmtpSettings
                {
                    From = "invalid",
                }
            };

            ValidateOptionsResult result = validator.Validate("settings", options);
            Assert.False(result.Succeeded);
        }
    }
}
