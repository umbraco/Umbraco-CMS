// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Configuration.Models.Validation;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Configuration.Models.Validation;

[TestFixture]
public class SecuritySettingsValidatorTests
{
    [Test]
    public void Returns_Success_ForValid_Configuration()
    {
        var validator = new SecuritySettingsValidator();
        var options = new SecuritySettings();
        var result = validator.Validate("settings", options);
        Assert.True(result.Succeeded);
    }

    [TestCase(false, false, true)]
    [TestCase(false, true, false)]
    [TestCase(true, false, true)]
    [TestCase(true, true, true)]
    public void Returns_Correct_Response_For_Configuration_With_Member_Require_Unique_Email(bool memberRequireUniqueEmail, bool usernameIsEmail, bool expected)
    {
        var validator = new SecuritySettingsValidator();
        var options = new SecuritySettings { MemberRequireUniqueEmail = memberRequireUniqueEmail, UsernameIsEmail = usernameIsEmail };

        var result = validator.Validate("settings", options);
        Assert.AreEqual(expected, result.Succeeded);
    }
}
