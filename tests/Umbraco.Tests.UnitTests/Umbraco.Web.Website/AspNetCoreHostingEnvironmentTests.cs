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
            Assert.That(result, Is.EqualTo(expected));
        }
    }

    [Test]
    public void EnsurePathIsApplicationRootPrefixed()
    {
        // Assert
        Assert.That(PathUtility.EnsurePathIsApplicationRootPrefixed("Views/Template.cshtml"), Is.EqualTo("~/Views/Template.cshtml"));
        Assert.That(PathUtility.EnsurePathIsApplicationRootPrefixed("/Views/Template.cshtml"), Is.EqualTo("~/Views/Template.cshtml"));
        Assert.That(PathUtility.EnsurePathIsApplicationRootPrefixed("~/Views/Template.cshtml"), Is.EqualTo("~/Views/Template.cshtml"));
    }

    [Test]
    public void EnsureApplicationMainUrl()
    {
        var sut = CreateWithDefaultConfig(ApplicationUrlDetection.FirstRequest);
        var url = new Uri("http://localhost:5000");
        sut.EnsureApplicationMainUrl(url);
        Assert.That(url, Is.EqualTo(sut.ApplicationMainUrl));
    }

    /// <summary>
    /// Creates an AspNetCoreHostingEnvironment with UmbracoApplicationUrl = null,
    /// simulating the default configuration where no explicit URL is configured.
    /// </summary>
    private static AspNetCoreHostingEnvironment CreateWithDefaultConfig(
        ApplicationUrlDetection detection = ApplicationUrlDetection.FirstRequest)
    {
        var hostingSettings = new HostingSettings();
        var webRoutingSettings = new WebRoutingSettings
        {
            ApplicationUrlDetection = detection,
        };

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
        var sut = CreateWithDefaultConfig(ApplicationUrlDetection.FirstRequest);

        var legitimateUrl = new Uri("https://legit-site.com");
        var attackerUrl = new Uri("https://non-configured-site.com");

        // Step 1: Normal traffic sets the URL
        sut.EnsureApplicationMainUrl(legitimateUrl);
        Assert.That(sut.ApplicationMainUrl, Is.EqualTo(legitimateUrl), "Initial legitimate URL should be set");

        // Step 2: Attacker sends request with forged Host header — must be ignored
        sut.EnsureApplicationMainUrl(attackerUrl);
        Assert.That(sut.ApplicationMainUrl, Is.EqualTo(legitimateUrl), "Attacker URL must not overwrite the legitimate URL");

        // Step 3: Legitimate traffic continues — URL remains stable
        sut.EnsureApplicationMainUrl(legitimateUrl);
        Assert.That(sut.ApplicationMainUrl, Is.EqualTo(legitimateUrl), "Legitimate URL is retained");
    }

    [Test]
    public void EnsureApplicationMainUrl_IgnoresSubsequentUrls()
    {
        var sut = CreateWithDefaultConfig(ApplicationUrlDetection.FirstRequest);

        var legitimateUrl = new Uri("https://legit-site.com");

        sut.EnsureApplicationMainUrl(legitimateUrl);
        sut.EnsureApplicationMainUrl(new Uri("https://evil1.com"));
        sut.EnsureApplicationMainUrl(new Uri("https://evil2.com"));

        Assert.That(sut.ApplicationMainUrl, Is.EqualTo(legitimateUrl), "First URL is locked, all subsequent URLs are ignored");
    }

    [Test]
    public void EnsureApplicationMainUrl_NullDoesNotLock()
    {
        var sut = CreateWithDefaultConfig(ApplicationUrlDetection.FirstRequest);

        sut.EnsureApplicationMainUrl(null);

        var url = new Uri("https://legit-site.com");
        sut.EnsureApplicationMainUrl(url);
        Assert.That(sut.ApplicationMainUrl, Is.EqualTo(url));
    }

    [Test]
    public void EnsureApplicationMainUrl_NoneMode_NeverSetsUrl()
    {
        var sut = CreateWithDefaultConfig(ApplicationUrlDetection.None);

        sut.EnsureApplicationMainUrl(new Uri("https://legit-site.com"));

        Assert.That(sut.ApplicationMainUrl, Is.Null);
    }

    [Test]
    public void EnsureApplicationMainUrl_NoneMode_ExplicitConfigStillWorks()
    {
        var webRoutingSettings = new WebRoutingSettings
        {
            UmbracoApplicationUrl = "https://configured-site.com",
            ApplicationUrlDetection = ApplicationUrlDetection.None,
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

        // Explicit config is set in the constructor, not via auto-detection
        Assert.That(sut.ApplicationMainUrl, Is.EqualTo(new Uri("https://configured-site.com")));
    }

    [Test]
    public void EnsureApplicationMainUrl_EveryRequest_OverwritesOnNewUrl()
    {
        var sut = CreateWithDefaultConfig(ApplicationUrlDetection.EveryRequest);

        var url1 = new Uri("https://site-a.com");
        var url2 = new Uri("https://site-b.com");

        sut.EnsureApplicationMainUrl(url1);
        Assert.That(sut.ApplicationMainUrl, Is.EqualTo(url1));

        sut.EnsureApplicationMainUrl(url2);
        Assert.That(sut.ApplicationMainUrl, Is.EqualTo(url2), "New URL should overwrite in EveryRequest mode");
    }

    [Test]
    public void EnsureApplicationMainUrl_EveryRequest_SameUrlNoOp()
    {
        var sut = CreateWithDefaultConfig(ApplicationUrlDetection.EveryRequest);

        var url = new Uri("https://site-a.com");
        sut.EnsureApplicationMainUrl(url);
        sut.EnsureApplicationMainUrl(url);

        Assert.That(sut.ApplicationMainUrl, Is.EqualTo(url), "Repeated same URL is a no-op");
    }

    [Test]
    public void EnsureApplicationMainUrl_EveryRequest_ExplicitConfigTakesPrecedence()
    {
        var webRoutingSettings = new WebRoutingSettings
        {
            UmbracoApplicationUrl = "https://configured-site.com",
            ApplicationUrlDetection = ApplicationUrlDetection.EveryRequest,
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

        Assert.That(sut.ApplicationMainUrl, Is.EqualTo(new Uri("https://configured-site.com")));

        // Attempt to overwrite via auto-detection
        sut.EnsureApplicationMainUrl(new Uri("https://non-configured-site.com"));

        Assert.That(
            sut.ApplicationMainUrl, Is.EqualTo(new Uri("https://configured-site.com")),
            "Explicit config prevents auto-detection overwrite");
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

        Assert.That(sut.ApplicationMainUrl, Is.EqualTo(new Uri("https://configured-site.com")));

        // Attempt override.
        sut.EnsureApplicationMainUrl(new Uri("https://non-configured-site.com"));

        // Should remain configured value.
        Assert.That(
            sut.ApplicationMainUrl, Is.EqualTo(new Uri("https://configured-site.com")),
            "Explicit config prevents host header poisoning");
    }
}
