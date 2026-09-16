// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Configuration.Models.Validation;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Configuration.Models.Validation;

[TestFixture]
public class SecuritySettingsValidatorTests
{
    [TestCase("Strict")]
    [TestCase("strict")]
    [TestCase("None")]
    [TestCase("Lax")]
    [TestCase("Unspecified")]
    [TestCase("0")]
    [TestCase("2")]
    public void Can_Validate_Defined_AuthCookieSameSite_Value(string configured)
    {
        ValidateOptionsResult result = Validate(new SecuritySettings { AuthCookieSameSite = configured });

        Assert.That(result.Failed, Is.False, result.FailureMessage);
    }

    [TestCase("42")]
    [TestCase("-7")]
    [TestCase("Stirct")]
    [TestCase("")]
    public void Cannot_Validate_Undefined_AuthCookieSameSite_Value(string configured)
    {
        ValidateOptionsResult result = Validate(new SecuritySettings { AuthCookieSameSite = configured });

        Assert.That(result.Failed, Is.True);
        Assert.That(result.FailureMessage, Does.Contain("Umbraco:CMS:Security:AuthCookieSameSite"));
    }

    // Umbraco.Core cannot reference Microsoft.AspNetCore.Http.SameSiteMode, so the validator mirrors its
    // members as strings. This pins the mirror to the real enum: a member added or renamed upstream fails
    // here rather than silently becoming un-configurable.
    [Test]
    public void Can_Validate_Every_Member_Of_The_Real_SameSiteMode_Enum()
    {
        Assert.Multiple(() =>
        {
            foreach (SameSiteMode mode in Enum.GetValues<SameSiteMode>())
            {
                var byName = Validate(new SecuritySettings { AuthCookieSameSite = mode.ToString() });
                Assert.That(byName.Failed, Is.False, $"SameSiteMode.{mode} rejected by name: {byName.FailureMessage}");

                var asNumber = ((int)mode).ToString(CultureInfo.InvariantCulture);
                var byNumber = Validate(new SecuritySettings { AuthCookieSameSite = asNumber });
                Assert.That(byNumber.Failed, Is.False, $"SameSiteMode.{mode} rejected as '{asNumber}': {byNumber.FailureMessage}");
            }
        });
    }

    [Test]
    public void Can_Validate_Default_SecuritySettings()
    {
        ValidateOptionsResult result = Validate(new SecuritySettings());

        Assert.That(result.Failed, Is.False, result.FailureMessage);
    }

    [Test]
    public void Cannot_Validate_BackOfficeHost_With_A_Path()
    {
        ValidateOptionsResult result = Validate(new SecuritySettings { BackOfficeHost = new Uri("https://backoffice.example.com/somewhere") });

        Assert.That(result.Failed, Is.True);
        Assert.That(result.FailureMessage, Does.Contain(nameof(SecuritySettings.BackOfficeHost)));
    }

    private static ValidateOptionsResult Validate(SecuritySettings settings)
        => new SecuritySettingsValidator().Validate(null, settings);
}
