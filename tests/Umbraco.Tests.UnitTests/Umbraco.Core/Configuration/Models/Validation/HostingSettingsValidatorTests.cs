// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Configuration.Models.Validation;
using Umbraco.Cms.Core.Factories;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Configuration.Models.Validation;

[TestFixture]
public class HostingSettingsValidatorTests
{
    [Test]
    public void Returns_Success_For_Default_Configuration()
    {
        var validator = new HostingSettingsValidator();
        var options = new HostingSettings();
        var result = validator.Validate("settings", options);
        Assert.True(result.Succeeded);
    }

    [Test]
    public void Returns_Success_For_Configuration_With_No_SiteName()
    {
        var validator = new HostingSettingsValidator();
        var options = new HostingSettings { SiteName = null };
        var result = validator.Validate("settings", options);
        Assert.True(result.Succeeded);
    }

    [Test]
    public void Returns_Success_For_Configuration_With_Short_SiteName()
    {
        var validator = new HostingSettingsValidator();
        var options = new HostingSettings { SiteName = "site1" };
        var result = validator.Validate("settings", options);
        Assert.True(result.Succeeded);
    }

    [Test]
    public void Returns_Fail_For_Configuration_With_SiteName_That_Exceeds_Max_Length()
    {
        var validator = new HostingSettingsValidator();
        var options = new HostingSettings { SiteName = new string('x', MachineInfoFactory.MaxMachineIdentifierLength) };
        var result = validator.Validate("settings", options);
        Assert.False(result.Succeeded);
    }
}
