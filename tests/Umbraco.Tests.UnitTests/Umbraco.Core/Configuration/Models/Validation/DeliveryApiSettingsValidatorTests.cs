// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Configuration.Models.Validation;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Configuration.Models.Validation;

/// <summary>
/// Contains unit tests for the <see cref="DeliveryApiSettingsValidator"/> class.
/// </summary>
[TestFixture]
public class DeliveryApiSettingsValidatorTests
{
    private Mock<ILogger<DeliveryApiSettingsValidator>> _loggerMock;

    /// <summary>
    /// Sets up the test environment before each test.
    /// </summary>
    [SetUp]
    public void SetUp() => _loggerMock = new Mock<ILogger<DeliveryApiSettingsValidator>>();

    /// <summary>
    /// Tests that the validator returns success when the configuration contains only an allow list.
    /// </summary>
    [Test]
    public void Returns_Success_For_Configuration_With_Only_AllowList()
    {
        var validator = new DeliveryApiSettingsValidator(_loggerMock.Object);
        var options = new DeliveryApiSettings
        {
            AllowedContentTypeAliases = new HashSet<string> { "content1", "content2" },
        };

        ValidateOptionsResult result = validator.Validate("settings", options);

        Assert.IsTrue(result.Succeeded);
        VerifyNoWarningLogged();
    }

    /// <summary>
    /// Tests that the validator returns success when the configuration contains only a disallow list.
    /// </summary>
    [Test]
    public void Returns_Success_For_Configuration_With_Only_DisallowList()
    {
        var validator = new DeliveryApiSettingsValidator(_loggerMock.Object);
        var options = new DeliveryApiSettings
        {
            DisallowedContentTypeAliases = new HashSet<string> { "content1", "content2" },
        };

        ValidateOptionsResult result = validator.Validate("settings", options);

        Assert.IsTrue(result.Succeeded);
        VerifyNoWarningLogged();
    }

    /// <summary>
    /// Tests that the validator returns success when the configuration contains no overlapping lists.
    /// </summary>
    [Test]
    public void Returns_Success_For_Configuration_With_No_Overlapping_Lists()
    {
        var validator = new DeliveryApiSettingsValidator(_loggerMock.Object);
        var options = new DeliveryApiSettings
        {
            AllowedContentTypeAliases = new HashSet<string> { "content1", "content2" },
            DisallowedContentTypeAliases = new HashSet<string> { "content3", "content4" },
        };

        ValidateOptionsResult result = validator.Validate("settings", options);

        Assert.IsTrue(result.Succeeded);
        VerifyNoWarningLogged();
    }

    /// <summary>
    /// Tests that <see cref="DeliveryApiSettingsValidator"/> returns success but logs a warning when the same content type alias appears in both the allowed and disallowed lists.
    /// </summary>
    [Test]
    public void Returns_Success_But_Logs_Warning_For_Overlapping_Lists()
    {
        var validator = new DeliveryApiSettingsValidator(_loggerMock.Object);
        var options = new DeliveryApiSettings
        {
            AllowedContentTypeAliases = new HashSet<string> { "content1", "content2" },
            DisallowedContentTypeAliases = new HashSet<string> { "content1", "content3" },
        };

        ValidateOptionsResult result = validator.Validate("settings", options);

        Assert.IsTrue(result.Succeeded);

        VerifyWarningLogged();
    }

    private void VerifyWarningLogged()
    {
        var warningCount = GetWarningLogCount();
        Assert.AreEqual(1, warningCount);
    }

    private void VerifyNoWarningLogged()
    {
        var warningCount = GetWarningLogCount();
        Assert.AreEqual(0, warningCount);
    }

    private int GetWarningLogCount() =>
        _loggerMock.Invocations
            .Count(invocation =>
                invocation.Method.Name == nameof(ILogger.Log) &&
                invocation.Arguments.OfType<LogLevel>().Any(level => level == LogLevel.Warning));
}
