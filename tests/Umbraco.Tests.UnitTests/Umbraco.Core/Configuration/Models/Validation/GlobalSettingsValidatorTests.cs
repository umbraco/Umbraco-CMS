// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Configuration.Models.Validation;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Configuration.Models.Validation;

[TestFixture]
public class GlobalSettingsValidatorTests
{
    [Test]
    public void Returns_Success_ForValid_Configuration()
    {
        var validator = new GlobalSettingsValidator();
        var options = new GlobalSettings();
        var result = validator.Validate("settings", options);
        Assert.True(result.Succeeded);
    }

    [Test]
    public void Returns_Fail_For_Configuration_With_Invalid_SmtpFrom_Field()
    {
        var validator = new GlobalSettingsValidator();
        var options = new GlobalSettings { Smtp = new SmtpSettings { From = "invalid" } };

        var result = validator.Validate("settings", options);
        Assert.False(result.Succeeded);
    }

    [Test]
    public void Returns_Fail_For_Configuration_With_Insufficient_SqlWriteLockTimeOut()
    {
        var validator = new GlobalSettingsValidator();
        var options = new GlobalSettings { DistributedLockingWriteLockDefaultTimeout = TimeSpan.Parse("00:00:00.099") };

        var result = validator.Validate("settings", options);
        Assert.False(result.Succeeded);
    }

    [Test]
    public void Returns_Success_For_Configuration_With_Valid_SqlWriteLockTimeOut()
    {
        var validator = new GlobalSettingsValidator();
        var options = new GlobalSettings { DistributedLockingWriteLockDefaultTimeout = TimeSpan.Parse("00:00:20") };

        var result = validator.Validate("settings", options);
        Assert.True(result.Succeeded);
    }

    [Test]
    public void Returns_Fail_For_Configuration_With_TimeOut_Exceeding_Browser_Max()
    {
        var validator = new GlobalSettingsValidator();
        var options = new GlobalSettings { TimeOut = TimeSpan.FromDays(25) };

        var result = validator.Validate("settings", options);
        Assert.False(result.Succeeded);
    }

    [Test]
    public void Returns_Success_For_Configuration_With_Valid_TimeOut()
    {
        var validator = new GlobalSettingsValidator();
        var options = new GlobalSettings { TimeOut = TimeSpan.FromHours(12) };

        var result = validator.Validate("settings", options);
        Assert.True(result.Succeeded);
    }

    [Test]
    public void Returns_Fail_For_Configuration_With_Negative_DatabaseCommandTimeout()
    {
        var validator = new GlobalSettingsValidator();
        var options = new GlobalSettings { DatabaseCommandTimeout = TimeSpan.FromSeconds(-1) };

        var result = validator.Validate("settings", options);
        Assert.False(result.Succeeded);
    }

    [Test]
    public void Returns_Fail_For_Configuration_With_Negative_DatabaseConnectTimeout()
    {
        var validator = new GlobalSettingsValidator();
        var options = new GlobalSettings { DatabaseConnectTimeout = TimeSpan.FromSeconds(-1) };

        var result = validator.Validate("settings", options);
        Assert.False(result.Succeeded);
    }

    [Test]
    public void Returns_Success_For_Configuration_With_Zero_DatabaseTimeouts()
    {
        // Zero is meaningful: it configures no limit at all.
        var validator = new GlobalSettingsValidator();
        var options = new GlobalSettings
        {
            DatabaseCommandTimeout = TimeSpan.Zero,
            DatabaseConnectTimeout = TimeSpan.Zero,
        };

        var result = validator.Validate("settings", options);
        Assert.True(result.Succeeded);
    }

    [Test]
    public void Returns_Success_For_Configuration_With_Valid_DatabaseTimeouts()
    {
        var validator = new GlobalSettingsValidator();
        var options = new GlobalSettings
        {
            DatabaseCommandTimeout = TimeSpan.FromMinutes(5),
            DatabaseConnectTimeout = TimeSpan.FromSeconds(30),
        };

        var result = validator.Validate("settings", options);
        Assert.True(result.Succeeded);
    }
}
