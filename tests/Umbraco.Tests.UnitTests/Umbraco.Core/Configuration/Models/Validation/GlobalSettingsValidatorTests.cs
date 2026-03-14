// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Configuration.Models.Validation;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Configuration.Models.Validation;

/// <summary>
/// Contains unit tests for the <see cref="GlobalSettingsValidator"/> class to verify its validation logic.
/// </summary>
[TestFixture]
public class GlobalSettingsValidatorTests
{
    /// <summary>
    /// Tests that the GlobalSettingsValidator returns success for a valid configuration.
    /// </summary>
    [Test]
    public void Returns_Success_ForValid_Configuration()
    {
        var validator = new GlobalSettingsValidator();
        var options = new GlobalSettings();
        var result = validator.Validate("settings", options);
        Assert.True(result.Succeeded);
    }

    /// <summary>
    /// Tests that the validator fails when the configuration contains an invalid Smtp From field.
    /// </summary>
    [Test]
    public void Returns_Fail_For_Configuration_With_Invalid_SmtpFrom_Field()
    {
        var validator = new GlobalSettingsValidator();
        var options = new GlobalSettings { Smtp = new SmtpSettings { From = "invalid" } };

        var result = validator.Validate("settings", options);
        Assert.False(result.Succeeded);
    }

    /// <summary>
    /// Validates that the configuration fails when the SqlWriteLock timeout is insufficient.
    /// </summary>
    [Test]
    public void Returns_Fail_For_Configuration_With_Insufficient_SqlWriteLockTimeOut()
    {
        var validator = new GlobalSettingsValidator();
        var options = new GlobalSettings { DistributedLockingWriteLockDefaultTimeout = TimeSpan.Parse("00:00:00.099") };

        var result = validator.Validate("settings", options);
        Assert.False(result.Succeeded);
    }

    /// <summary>
    /// Tests that the validator returns success for configuration with a valid SqlWriteLockTimeout.
    /// </summary>
    [Test]
    public void Returns_Success_For_Configuration_With_Valid_SqlWriteLockTimeOut()
    {
        var validator = new GlobalSettingsValidator();
        var options = new GlobalSettings { DistributedLockingWriteLockDefaultTimeout = TimeSpan.Parse("00:00:20") };

        var result = validator.Validate("settings", options);
        Assert.True(result.Succeeded);
    }
}
