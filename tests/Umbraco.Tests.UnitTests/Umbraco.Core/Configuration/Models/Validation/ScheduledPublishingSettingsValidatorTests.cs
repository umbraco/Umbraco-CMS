// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Configuration.Models.Validation;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Configuration.Models.Validation;

[TestFixture]
public class ScheduledPublishingSettingsValidatorTests
{
    [Test]
    public void Can_Validate_Default_Configuration()
    {
        var result = Validate(new ScheduledPublishingSettings());
        Assert.True(result.Succeeded);
    }

    [Test]
    public void Cannot_Validate_Zero_Or_Negative_Period()
    {
        var result = Validate(new ScheduledPublishingSettings { Period = TimeSpan.Zero });
        Assert.False(result.Succeeded);
    }

    [Test]
    public void Can_Validate_Any_Positive_Period_When_Not_Aligned()
    {
        // 7 seconds does not divide evenly into an hour, but that's only enforced when aligned.
        var result = Validate(new ScheduledPublishingSettings
        {
            Period = TimeSpan.FromSeconds(7),
            AlignToClock = false,
        });
        Assert.True(result.Succeeded);
    }

    [TestCase(1)]
    [TestCase(10)]
    [TestCase(12)]
    [TestCase(15)]
    [TestCase(20)]
    [TestCase(30)]
    [TestCase(60)]
    [TestCase(3600)]
    public void Can_Validate_Clean_Divisor_Of_An_Hour_When_Aligned(int seconds)
    {
        var result = Validate(new ScheduledPublishingSettings
        {
            Period = TimeSpan.FromSeconds(seconds),
            AlignToClock = true,
        });
        Assert.True(result.Succeeded);
    }

    [TestCase(7)]
    [TestCase(11)]
    [TestCase(70)]
    [TestCase(7200)]
    public void Cannot_Validate_Non_Divisor_Of_An_Hour_When_Aligned(int seconds)
    {
        var result = Validate(new ScheduledPublishingSettings
        {
            Period = TimeSpan.FromSeconds(seconds),
            AlignToClock = true,
        });
        Assert.False(result.Succeeded);
    }

    [Test]
    public void Cannot_Validate_Sub_Second_Period_When_Aligned()
    {
        var result = Validate(new ScheduledPublishingSettings
        {
            Period = TimeSpan.FromMilliseconds(500),
            AlignToClock = true,
        });
        Assert.False(result.Succeeded);
    }

    private static Microsoft.Extensions.Options.ValidateOptionsResult Validate(ScheduledPublishingSettings options)
        => new ScheduledPublishingSettingsValidator().Validate("settings", options);
}
