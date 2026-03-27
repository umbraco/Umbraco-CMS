// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Tests.UnitTests.AutoFixture;
using Umbraco.Cms.Web.Common.AspNetCore;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Website;

[TestFixture]
public class AspNetCoreHostingEnvironmentTests
{
    [InlineAutoMoqData("~/Scripts", "/Scripts", null)]
    [InlineAutoMoqData("/Scripts", "/Scripts", null)]
    [InlineAutoMoqData("../Scripts", "/Scripts", typeof(InvalidOperationException))]
    public void IOHelper_ResolveUrl(string input, string expected, Type expectedExceptionType, AspNetCoreHostingEnvironment sut)
    {
        if (expectedExceptionType != null)
        {
            Assert.Throws(expectedExceptionType, () => sut.ToAbsolute(input));
        }
        else
        {
            var result = sut.ToAbsolute(input);
            Assert.AreEqual(expected, result);
        }
    }

    [Test]
    public void EnsurePathIsApplicationRootPrefixed()
    {
        // Assert
        Assert.AreEqual("~/Views/Template.cshtml", PathUtility.EnsurePathIsApplicationRootPrefixed("Views/Template.cshtml"));
        Assert.AreEqual("~/Views/Template.cshtml", PathUtility.EnsurePathIsApplicationRootPrefixed("/Views/Template.cshtml"));
        Assert.AreEqual("~/Views/Template.cshtml", PathUtility.EnsurePathIsApplicationRootPrefixed("~/Views/Template.cshtml"));
    }

    [AutoMoqData]
    [Test]
    public void EnsureApplicationMainUrl(AspNetCoreHostingEnvironment sut)
    {
        var url = new Uri("http://localhost:5000");
        sut.EnsureApplicationMainUrl(url);
        Assert.AreEqual(sut.ApplicationMainUrl, url);
    }

    /// <summary>
    /// Creates an AspNetCoreHostingEnvironment with UmbracoApplicationUrl = null,
    /// simulating the default configuration where no explicit URL is configured.
    /// This is the scenario where host header poisoning is possible.
    /// </summary>
    private static AspNetCoreHostingEnvironment CreateWithDefaultConfig()
    {
        var hostingSettings = new HostingSettings();
        var webRoutingSettings = new WebRoutingSettings(); // UmbracoApplicationUrl defaults to null

        var hostingSettingsMonitor = Mock.Of<IOptionsMonitor<HostingSettings>>(
            m => m.CurrentValue == hostingSettings);
        var webRoutingSettingsMonitor = Mock.Of<IOptionsMonitor<WebRoutingSettings>>(
            m => m.CurrentValue == webRoutingSettings);

        var webHostEnvironment = new Mock<IWebHostEnvironment>();
        webHostEnvironment.Setup(e => e.ContentRootPath).Returns(Path.GetTempPath());
        webHostEnvironment.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());
        webHostEnvironment.Setup(e => e.ApplicationName).Returns("TestApp");

        return new AspNetCoreHostingEnvironment(
            hostingSettingsMonitor,
            webRoutingSettingsMonitor,
            webHostEnvironment.Object);
    }

    [Test]
    public void EnsureApplicationMainUrl_LocksAfterFirstUrl()
    {
        var sut = CreateWithDefaultConfig();

        var legitimateUrl = new Uri("https://legit-site.com");
        var attackerUrl = new Uri("https://evil.com");

        // Step 1: Normal traffic sets the URL
        sut.EnsureApplicationMainUrl(legitimateUrl);
        Assert.AreEqual(legitimateUrl, sut.ApplicationMainUrl, "Initial legitimate URL should be set");

        // Step 2: Attacker sends request with forged Host header — must be ignored
        sut.EnsureApplicationMainUrl(attackerUrl);
        Assert.AreEqual(legitimateUrl, sut.ApplicationMainUrl, "Attacker URL must not overwrite the legitimate URL");

        // Step 3: Legitimate traffic continues — URL remains stable
        sut.EnsureApplicationMainUrl(legitimateUrl);
        Assert.AreEqual(legitimateUrl, sut.ApplicationMainUrl, "Legitimate URL is retained");
    }

    [Test]
    public void EnsureApplicationMainUrl_IgnoresSubsequentUrls()
    {
        var sut = CreateWithDefaultConfig();

        var legitimateUrl = new Uri("https://legit-site.com");

        sut.EnsureApplicationMainUrl(legitimateUrl);
        sut.EnsureApplicationMainUrl(new Uri("https://evil1.com"));
        sut.EnsureApplicationMainUrl(new Uri("https://evil2.com"));

        Assert.AreEqual(legitimateUrl, sut.ApplicationMainUrl, "First URL is locked, all subsequent URLs are ignored");
    }

    [Test]
    public void EnsureApplicationMainUrl_NullDoesNotLock()
    {
        var sut = CreateWithDefaultConfig();

        sut.EnsureApplicationMainUrl(null);

        var url = new Uri("https://legit-site.com");
        sut.EnsureApplicationMainUrl(url);
        Assert.AreEqual(url, sut.ApplicationMainUrl);
    }

    [Test]
    public void EnsureApplicationMainUrl_WithExplicitConfig_IgnoresHostHeader()
    {
        // When UmbracoApplicationUrl IS configured, poisoning should be impossible
        var webRoutingSettings = new WebRoutingSettings
        {
            UmbracoApplicationUrl = "https://configured-site.com",
        };

        var hostingSettingsMonitor = Mock.Of<IOptionsMonitor<HostingSettings>>(
            m => m.CurrentValue == new HostingSettings());
        var webRoutingSettingsMonitor = Mock.Of<IOptionsMonitor<WebRoutingSettings>>(
            m => m.CurrentValue == webRoutingSettings);

        var webHostEnvironment = new Mock<IWebHostEnvironment>();
        webHostEnvironment.Setup(e => e.ContentRootPath).Returns(Path.GetTempPath());
        webHostEnvironment.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());
        webHostEnvironment.Setup(e => e.ApplicationName).Returns("TestApp");

        var sut = new AspNetCoreHostingEnvironment(
            hostingSettingsMonitor,
            webRoutingSettingsMonitor,
            webHostEnvironment.Object);

        Assert.AreEqual(new Uri("https://configured-site.com"), sut.ApplicationMainUrl);

        // Attempt poisoning
        sut.EnsureApplicationMainUrl(new Uri("https://evil.com"));

        // Should remain configured value
        Assert.AreEqual(
            new Uri("https://configured-site.com"),
            sut.ApplicationMainUrl,
            "Explicit config prevents host header poisoning");
    }
}
