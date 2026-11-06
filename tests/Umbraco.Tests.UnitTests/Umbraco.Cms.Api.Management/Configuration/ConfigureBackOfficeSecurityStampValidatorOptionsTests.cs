using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Configuration;
using Umbraco.Cms.Api.Management.Security;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Configuration;

[TestFixture]
public class ConfigureBackOfficeSecurityStampValidatorOptionsTests
{
    [Test]
    public void Can_Validate_Security_Stamp_On_Every_Request_When_Concurrent_Logins_Are_Disallowed()
    {
        BackOfficeSecurityStampValidatorOptions options = Configure(new SecuritySettings { AllowConcurrentLogins = false });

        Assert.That(options.ValidationInterval, Is.EqualTo(TimeSpan.Zero));
    }

    // Signing out renews the security stamp; the validation interval bounds how long a copy of the signed out cookie
    // keeps working when the stamp is not validated on every request.
    [Test]
    public void Can_Validate_Security_Stamp_Within_A_Short_Interval_When_Concurrent_Logins_Are_Allowed()
    {
        BackOfficeSecurityStampValidatorOptions options = Configure(new SecuritySettings { AllowConcurrentLogins = true });

        Assert.That(options.ValidationInterval, Is.EqualTo(TimeSpan.FromSeconds(30)));
    }

    [Test]
    public void Can_Preserve_Customized_Interval_When_Concurrent_Logins_Are_Allowed()
    {
        var customInterval = TimeSpan.FromMinutes(5);

        BackOfficeSecurityStampValidatorOptions options = Configure(
            new SecuritySettings { AllowConcurrentLogins = true },
            new BackOfficeSecurityStampValidatorOptions { ValidationInterval = customInterval });

        Assert.That(options.ValidationInterval, Is.EqualTo(customInterval));
    }

    [TestCase(true, false, 0)]
    [TestCase(false, true, 30)]
    public void Can_Apply_User_Specific_Concurrent_Logins_Setting(bool allowConcurrentLogins, bool userAllowConcurrentLogins, int expectedIntervalSeconds)
    {
        BackOfficeSecurityStampValidatorOptions options = Configure(new SecuritySettings
        {
            AllowConcurrentLogins = allowConcurrentLogins,
            UserAllowConcurrentLogins = userAllowConcurrentLogins,
        });

        Assert.That(options.ValidationInterval, Is.EqualTo(TimeSpan.FromSeconds(expectedIntervalSeconds)));
    }

    private static BackOfficeSecurityStampValidatorOptions Configure(
        SecuritySettings securitySettings,
        BackOfficeSecurityStampValidatorOptions? options = null)
    {
        options ??= new BackOfficeSecurityStampValidatorOptions();
        new ConfigureBackOfficeSecurityStampValidatorOptions(Options.Create(securitySettings), TimeProvider.System).Configure(options);
        return options;
    }
}
