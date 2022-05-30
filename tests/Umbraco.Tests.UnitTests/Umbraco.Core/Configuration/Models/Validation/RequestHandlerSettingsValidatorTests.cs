// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Configuration.Models.Validation;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Configuration.Models.Validation;

[TestFixture]
public class RequestHandlerSettingsValidatorTests
{
    [Test]
    public void Returns_Success_ForValid_Configuration()
    {
        var validator = new RequestHandlerSettingsValidator();
        var options = new RequestHandlerSettings();
        var result = validator.Validate("settings", options);
        Assert.True(result.Succeeded);
    }

    [Test]
    public void Returns_Fail_For_Configuration_With_Invalid_ConvertUrlsToAscii_Field()
    {
        var validator = new RequestHandlerSettingsValidator();
        var options = new RequestHandlerSettings { ConvertUrlsToAscii = "invalid" };
        var result = validator.Validate("settings", options);
        Assert.False(result.Succeeded);
    }
}
