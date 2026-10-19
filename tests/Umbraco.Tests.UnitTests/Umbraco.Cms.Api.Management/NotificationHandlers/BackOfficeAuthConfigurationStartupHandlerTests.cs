// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.NotificationHandlers;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.NotificationHandlers;

[TestFixture]
public class BackOfficeAuthConfigurationStartupHandlerTests
{
    // Substrings that identify each warning. Scenarios below are arranged so only the warning under
    // test can fire, so these only need to tell the three apart.
    private const string CrossSiteWarning = "cross-site request forgery";
    private const string NotSecureWarning = "SameSite=None requires the Secure attribute";
    private const string RemovedSettingsWarning = "no longer supported and are being ignored";

    private List<string> _warnings = null!;
    private Mock<ILogger<BackOfficeAuthConfigurationStartupHandler>> _logger = null!;

    [SetUp]
    public void SetUp()
    {
        _warnings = [];
        _logger = new Mock<ILogger<BackOfficeAuthConfigurationStartupHandler>>();
        _logger
            .Setup(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
            .Callback(new InvocationAction(invocation => _warnings.Add(invocation.Arguments[2].ToString()!)));
    }

    // SameSite=None lets the auth cookie ride cross-site requests, and the Management API has no
    // antiforgery tokens behind it. Staging covers environments that are neither Development nor
    // Production; the numeric spelling pins that the value is resolved through the shared parser
    // rather than compared as a string.
    [TestCase("Staging", "None")]
    [TestCase("Production", "0")]
    public void Can_Warn_When_SameSite_Is_None_Outside_Development(string environmentName, string configured)
    {
        Handle(environmentName: environmentName, sameSite: configured);

        AssertSingleWarningContaining(CrossSiteWarning);
    }

    // Serving the back office from a separate dev server is exactly what the setting is for.
    [Test]
    public void Cannot_Warn_When_SameSite_Is_None_In_Development()
    {
        Handle(environmentName: "Development", sameSite: "None");

        AssertNoWarnings();
    }

    [Test]
    public void Cannot_Warn_When_SameSite_Is_Not_None()
    {
        Handle(sameSite: "Strict");

        AssertNoWarnings();
    }

    // Browsers reject a SameSite=None cookie that is not Secure, so sign-in fails outright with nothing
    // logged server-side. Asserted in Development, where the cross-site warning is suppressed, so this
    // warning is the only one that can fire - and Development is where the combination is most likely.
    [Test]
    public void Can_Warn_When_SameSite_Is_None_And_UseHttps_Is_Disabled()
    {
        Handle(environmentName: "Development", sameSite: "None", useHttps: false);

        AssertSingleWarningContaining(NotSecureWarning);
    }

    [Test]
    public void Cannot_Warn_When_SameSite_Is_None_And_UseHttps_Is_Enabled()
    {
        Handle(environmentName: "Development", sameSite: "None", useHttps: true);

        AssertNoWarnings();
    }

    // HTTP is only a problem in combination with SameSite=None.
    [Test]
    public void Cannot_Warn_When_UseHttps_Is_Disabled_But_SameSite_Is_Not_None()
    {
        Handle(environmentName: "Development", sameSite: "Strict", useHttps: false);

        AssertNoWarnings();
    }

    [Test]
    public void Can_Warn_Separately_When_SameSite_Is_None_Outside_Development_And_UseHttps_Is_Disabled()
    {
        Handle(environmentName: "Production", sameSite: "None", useHttps: false);

        Assert.That(_warnings, Has.Count.EqualTo(2));
        Assert.That(_warnings.Any(x => x.Contains(CrossSiteWarning)), Is.True);
        Assert.That(_warnings.Any(x => x.Contains(NotSecureWarning)), Is.True);
    }

    // These keys were removed by the cookie-auth work. They bind to nothing, so without a warning the
    // only symptom is the back office redirecting to a path that no longer exists.
    [TestCase("Umbraco:CMS:Security:AuthorizeCallbackPathName")]
    [TestCase("Umbraco:CMS:Security:AuthorizeCallbackLogoutPathName")]
    [TestCase("Umbraco:CMS:Security:AuthorizeCallbackErrorPathName")]
    [TestCase("Umbraco:CMS:Security:BackOfficeTokenCookie:SameSite")]
    [TestCase("Umbraco:CMS:Security:BackOfficeTokenCookie:SiteName")]
    public void Can_Warn_When_Removed_Setting_Is_Configured(string key)
    {
        Handle(environmentName: "Development", configuration: new Dictionary<string, string?> { [key] = "/oauth_complete" });

        AssertSingleWarningContaining(RemovedSettingsWarning);
    }

    [Test]
    public void Can_Report_Every_Removed_Setting_In_A_Single_Warning()
    {
        Handle(
            environmentName: "Development",
            configuration: new Dictionary<string, string?>
            {
                ["Umbraco:CMS:Security:AuthorizeCallbackPathName"] = "/oauth_complete",
                ["Umbraco:CMS:Security:AuthorizeCallbackLogoutPathName"] = "/logout",
                ["Umbraco:CMS:Security:BackOfficeTokenCookie:SameSite"] = "None",
            });

        AssertSingleWarningContaining(RemovedSettingsWarning);
        Assert.That(_warnings[0], Does.Contain("AuthorizeCallbackPathName"));
        Assert.That(_warnings[0], Does.Contain("AuthorizeCallbackLogoutPathName"));
        Assert.That(_warnings[0], Does.Contain("BackOfficeTokenCookie"));
        Assert.That(_warnings[0], Does.Not.Contain("AuthorizeCallbackErrorPathName"));
    }

    // The replacement settings must not trip the removed-settings check.
    [Test]
    public void Cannot_Warn_When_Only_Supported_Settings_Are_Configured()
    {
        Handle(
            environmentName: "Development",
            configuration: new Dictionary<string, string?>
            {
                ["Umbraco:CMS:Security:CallbackPathName"] = "/umbraco",
                ["Umbraco:CMS:Security:AuthCookieName"] = "UMB_UCONTEXT",
                ["Umbraco:CMS:Security:AuthCookieSameSite"] = "Strict",
            });

        AssertNoWarnings();
    }

    private void Handle(
        string environmentName = "Production",
        string sameSite = "Strict",
        bool useHttps = true,
        Dictionary<string, string?>? configuration = null)
    {
        IConfiguration builtConfiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(configuration ?? [])
            .Build();

        var sut = new BackOfficeAuthConfigurationStartupHandler(
            Options.Create(new SecuritySettings { AuthCookieSameSite = sameSite }),
            Options.Create(new GlobalSettings { UseHttps = useHttps }),
            Mock.Of<IHostEnvironment>(x => x.EnvironmentName == environmentName),
            builtConfiguration,
            _logger.Object);

        sut.Handle(new UmbracoApplicationStartingNotification(RuntimeLevel.Run, false));
    }

    private void AssertSingleWarningContaining(string expected)
    {
        Assert.That(
            _warnings,
            Has.Count.EqualTo(1),
            $"Expected exactly one warning. Got: {string.Join(" | ", _warnings)}");
        Assert.That(_warnings[0], Does.Contain(expected));
    }

    private void AssertNoWarnings()
        => Assert.That(_warnings, Is.Empty, $"Expected no warnings. Got: {string.Join(" | ", _warnings)}");
}
