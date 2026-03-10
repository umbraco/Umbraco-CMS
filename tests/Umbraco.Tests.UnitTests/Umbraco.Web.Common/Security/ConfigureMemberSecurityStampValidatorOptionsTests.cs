using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Security;

[TestFixture]
public class ConfigureMemberSecurityStampValidatorOptionsTests
{
    private static readonly TimeSpan _defaultInterval = new SecurityStampValidatorOptions().ValidationInterval;
    private static readonly TimeSpan _expectedMemberInterval = TimeSpan.FromSeconds(30);

    [Test]
    public void Sets_30_Second_Interval_When_AllowConcurrentLogins_Is_False()
    {
        var securitySettings = new SecuritySettings { AllowConcurrentLogins = false };
        var sut = new ConfigureMemberSecurityStampValidatorOptions(Options.Create(securitySettings));
        var options = new MemberSecurityStampValidatorOptions();

        sut.Configure(options);

        Assert.That(options.ValidationInterval, Is.EqualTo(_expectedMemberInterval));
    }

    [Test]
    public void Preserves_Default_Interval_When_AllowConcurrentLogins_Is_True()
    {
        var securitySettings = new SecuritySettings { AllowConcurrentLogins = true };
        var sut = new ConfigureMemberSecurityStampValidatorOptions(Options.Create(securitySettings));
        var options = new MemberSecurityStampValidatorOptions();

        sut.Configure(options);

        Assert.That(options.ValidationInterval, Is.EqualTo(_defaultInterval));
    }

    [Test]
    public void Preserves_Custom_Interval_When_AllowConcurrentLogins_Is_False()
    {
        // If a developer has already customized the interval, don't override it.
        var customInterval = TimeSpan.FromMinutes(5);
        var securitySettings = new SecuritySettings { AllowConcurrentLogins = false };
        var sut = new ConfigureMemberSecurityStampValidatorOptions(Options.Create(securitySettings));
        var options = new MemberSecurityStampValidatorOptions { ValidationInterval = customInterval };

        sut.Configure(options);

        Assert.That(options.ValidationInterval, Is.EqualTo(customInterval));
    }

    [Test]
    public void Preserves_Zero_Interval_When_Explicitly_Set_By_Developer()
    {
        // A developer may intentionally set TimeSpan.Zero for per-request validation.
        // This should not be overridden to 30 seconds.
        var securitySettings = new SecuritySettings { AllowConcurrentLogins = false };
        var sut = new ConfigureMemberSecurityStampValidatorOptions(Options.Create(securitySettings));
        var options = new MemberSecurityStampValidatorOptions { ValidationInterval = TimeSpan.Zero };

        sut.Configure(options);

        Assert.That(options.ValidationInterval, Is.EqualTo(TimeSpan.Zero));
    }

    [Test]
    public void Configures_OnRefreshingPrincipal_Callback()
    {
        var securitySettings = new SecuritySettings();
        var sut = new ConfigureMemberSecurityStampValidatorOptions(Options.Create(securitySettings));
        var options = new MemberSecurityStampValidatorOptions();

        sut.Configure(options);

        Assert.That(options.OnRefreshingPrincipal, Is.Not.Null);
    }

    [Test]
    public void MemberAllowConcurrentLogins_False_Overrides_Global_True()
    {
        var securitySettings = new SecuritySettings
        {
            AllowConcurrentLogins = true,
            MemberAllowConcurrentLogins = false,
        };
        var sut = new ConfigureMemberSecurityStampValidatorOptions(Options.Create(securitySettings));
        var options = new MemberSecurityStampValidatorOptions();

        sut.Configure(options);

        Assert.That(options.ValidationInterval, Is.EqualTo(_expectedMemberInterval));
    }

    [Test]
    public void MemberAllowConcurrentLogins_True_Overrides_Global_False()
    {
        var securitySettings = new SecuritySettings
        {
            AllowConcurrentLogins = false,
            MemberAllowConcurrentLogins = true,
        };
        var sut = new ConfigureMemberSecurityStampValidatorOptions(Options.Create(securitySettings));
        var options = new MemberSecurityStampValidatorOptions();

        sut.Configure(options);

        Assert.That(options.ValidationInterval, Is.EqualTo(_defaultInterval));
    }

    [Test]
    public void MemberAllowConcurrentLogins_Null_Falls_Back_To_Global()
    {
        var securitySettings = new SecuritySettings
        {
            AllowConcurrentLogins = false,
            MemberAllowConcurrentLogins = null,
        };
        var sut = new ConfigureMemberSecurityStampValidatorOptions(Options.Create(securitySettings));
        var options = new MemberSecurityStampValidatorOptions();

        sut.Configure(options);

        Assert.That(options.ValidationInterval, Is.EqualTo(_expectedMemberInterval));
    }
}
