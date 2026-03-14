// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Configuration.Models.Validation;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Configuration.Models.Validation;

/// <summary>
/// Contains unit tests for the <see cref="HealthChecksSettingsValidator"/> class, verifying its validation logic and behavior.
/// </summary>
[TestFixture]
public class HealthChecksSettingsValidatorTests
{
    /// <summary>
    /// Tests that the HealthChecksSettingsValidator returns success for a valid configuration.
    /// </summary>
    [Test]
    public void Returns_Success_ForValid_Configuration()
    {
        var validator = new HealthChecksSettingsValidator(new NCronTabParser());
        var options = BuildHealthChecksSettings();
        var result = validator.Validate("settings", options);
        Assert.True(result.Succeeded);
    }

    /// <summary>
    /// Tests that the validator returns a failure result when the configuration contains an invalid notification FirstRunTime.
    /// </summary>
    [Test]
    public void Returns_Fail_For_Configuration_With_Invalid_Notification_FirstRunTime()
    {
        var validator = new HealthChecksSettingsValidator(new NCronTabParser());
        var options = BuildHealthChecksSettings("0 3 *");
        var result = validator.Validate("settings", options);
        Assert.False(result.Succeeded);
    }

    private static HealthChecksSettings BuildHealthChecksSettings(string firstRunTime = "0 3 * * *") =>
        new()
        {
            Notification = new HealthChecksNotificationSettings
            {
                Enabled = true,
                FirstRunTime = firstRunTime,
                Period = TimeSpan.FromHours(1),
            },
        };
}
