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

    [Test]
    public void EnsureApplicationMainUrl()
    {
        var sut = CreateWithDefaultConfig(ApplicationUrlDetection.FirstRequest);
        var url = new Uri("http://localhost:5000");
        sut.EnsureApplicationMainUrl(url);
        Assert.AreEqual(sut.ApplicationMainUrl, url);
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
        var sut = CreateWithDefaultConfig(ApplicationUrlDetection.FirstRequest);

        var legitimateUrl = new Uri("https://legit-site.com");

        sut.EnsureApplicationMainUrl(legitimateUrl);
        sut.EnsureApplicationMainUrl(new Uri("https://evil1.com"));
        sut.EnsureApplicationMainUrl(new Uri("https://evil2.com"));

        Assert.AreEqual(legitimateUrl, sut.ApplicationMainUrl, "First URL is locked, all subsequent URLs are ignored");
    }

    [Test]
    public void EnsureApplicationMainUrl_NullDoesNotLock()
    {
        var sut = CreateWithDefaultConfig(ApplicationUrlDetection.FirstRequest);

        sut.EnsureApplicationMainUrl(null);

        var url = new Uri("https://legit-site.com");
        sut.EnsureApplicationMainUrl(url);
        Assert.AreEqual(url, sut.ApplicationMainUrl);
    }

    [TestCase("http://localhost:5000", "https://legit-site.com")]
    [TestCase("https://127.0.0.1", "http://legit-site.com")]
    [TestCase("http://[::1]:8080", "https://legit-site.com")]
    public void EnsureApplicationMainUrl_FirstRequest_ReplacesLoopbackUrl(string loopback, string replacement)
    {
        var sut = CreateWithDefaultConfig(ApplicationUrlDetection.FirstRequest);

        sut.EnsureApplicationMainUrl(new Uri(loopback));
        sut.EnsureApplicationMainUrl(new Uri(replacement));

        Assert.AreEqual(new Uri(replacement), sut.ApplicationMainUrl, "A loopback URL is never useful as the public URL and is replaced");
    }

    [Test]
    public void EnsureApplicationMainUrl_FirstRequest_LoopbackDoesNotReplaceLoopback()
    {
        var sut = CreateWithDefaultConfig(ApplicationUrlDetection.FirstRequest);

        var first = new Uri("http://localhost:5000");
        sut.EnsureApplicationMainUrl(first);
        sut.EnsureApplicationMainUrl(new Uri("http://127.0.0.1:5000"));

        Assert.AreEqual(first, sut.ApplicationMainUrl);
    }

    [Test]
    public void EnsureApplicationMainUrl_FirstRequest_LocksAfterLeavingLoopback()
    {
        var sut = CreateWithDefaultConfig(ApplicationUrlDetection.FirstRequest);

        var legitimateUrl = new Uri("https://legit-site.com");
        sut.EnsureApplicationMainUrl(new Uri("http://localhost:5000"));
        sut.EnsureApplicationMainUrl(legitimateUrl);
        sut.EnsureApplicationMainUrl(new Uri("https://non-configured-site.com"));

        Assert.AreEqual(legitimateUrl, sut.ApplicationMainUrl, "Once a non-loopback URL is set, other hosts are ignored");
    }

    [TestCase("http://legit-site.com", "https://legit-site.com")]
    [TestCase("http://legit-site.com:5000", "https://legit-site.com:5001")]
    [TestCase("http://legit-site.com/site", "https://legit-site.com/site")]
    [TestCase("http://LEGIT-SITE.com", "https://legit-site.com")]
    [TestCase("http://localhost:5000", "https://localhost:5001")]
    [TestCase("http://legit-site.com/Site", "https://legit-site.com/site")]
    public void EnsureApplicationMainUrl_FirstRequest_UpgradesSameHostToHttps(string http, string https)
    {
        var sut = CreateWithDefaultConfig(ApplicationUrlDetection.FirstRequest);

        sut.EnsureApplicationMainUrl(new Uri(http));
        sut.EnsureApplicationMainUrl(new Uri(https));

        Assert.AreEqual(new Uri(https), sut.ApplicationMainUrl, "HTTPS for the same host and path replaces HTTP");
    }

    [TestCase("http://legit-site.com", "https://non-configured-site.com")]
    [TestCase("http://legit-site.com/site", "https://legit-site.com/other")]
    [TestCase("http://legit-site.com", "http://non-configured-site.com")]
    public void EnsureApplicationMainUrl_FirstRequest_HttpsForAnotherHostOrPathDoesNotUnlock(string http, string other)
    {
        var sut = CreateWithDefaultConfig(ApplicationUrlDetection.FirstRequest);

        var locked = new Uri(http);
        sut.EnsureApplicationMainUrl(locked);
        sut.EnsureApplicationMainUrl(new Uri(other));

        Assert.AreEqual(locked, sut.ApplicationMainUrl, "The upgrade path must not let a request change the host or path");
    }

    [Test]
    public void EnsureApplicationMainUrl_FirstRequest_DoesNotDowngradeToHttp()
    {
        var sut = CreateWithDefaultConfig(ApplicationUrlDetection.FirstRequest);

        var https = new Uri("https://legit-site.com");
        sut.EnsureApplicationMainUrl(https);
        sut.EnsureApplicationMainUrl(new Uri("http://legit-site.com"));

        Assert.AreEqual(https, sut.ApplicationMainUrl);
    }

    [Test]
    public void EnsureApplicationMainUrl_NoneMode_NeverSetsUrl()
    {
        var sut = CreateWithDefaultConfig(ApplicationUrlDetection.None);

        sut.EnsureApplicationMainUrl(new Uri("https://legit-site.com"));

        Assert.IsNull(sut.ApplicationMainUrl);
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
        Assert.AreEqual(new Uri("https://configured-site.com"), sut.ApplicationMainUrl);
    }

    [Test]
    public void EnsureApplicationMainUrl_EveryRequest_OverwritesOnNewUrl()
    {
        var sut = CreateWithDefaultConfig(ApplicationUrlDetection.EveryRequest);

        var url1 = new Uri("https://site-a.com");
        var url2 = new Uri("https://site-b.com");

        sut.EnsureApplicationMainUrl(url1);
        Assert.AreEqual(url1, sut.ApplicationMainUrl);

        sut.EnsureApplicationMainUrl(url2);
        Assert.AreEqual(url2, sut.ApplicationMainUrl, "New URL should overwrite in EveryRequest mode");
    }

    [Test]
    public void EnsureApplicationMainUrl_EveryRequest_SameUrlNoOp()
    {
        var sut = CreateWithDefaultConfig(ApplicationUrlDetection.EveryRequest);

        var url = new Uri("https://site-a.com");
        sut.EnsureApplicationMainUrl(url);
        sut.EnsureApplicationMainUrl(url);

        Assert.AreEqual(url, sut.ApplicationMainUrl, "Repeated same URL is a no-op");
    }

    [TestCase("https://site-a.com", "http://site-a.com")]
    [TestCase("https://site-a.com", "http://site-b.com")]
    public void EnsureApplicationMainUrl_EveryRequest_DoesNotDowngradeToHttp(string https, string http)
    {
        var sut = CreateWithDefaultConfig(ApplicationUrlDetection.EveryRequest);

        sut.EnsureApplicationMainUrl(new Uri(https));
        sut.EnsureApplicationMainUrl(new Uri(http));

        Assert.AreEqual(new Uri(https), sut.ApplicationMainUrl, "An HTTPS URL is never replaced by an HTTP one");
    }

    [Test]
    public void EnsureApplicationMainUrl_EveryRequest_StillSwitchesHostWhenUpgradingToHttps()
    {
        var sut = CreateWithDefaultConfig(ApplicationUrlDetection.EveryRequest);

        var upgraded = new Uri("https://site-b.com");
        sut.EnsureApplicationMainUrl(new Uri("http://site-a.com"));
        sut.EnsureApplicationMainUrl(upgraded);

        Assert.AreEqual(upgraded, sut.ApplicationMainUrl, "EveryRequest keeps switching hosts; only downgrades are refused");
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

        Assert.AreEqual(new Uri("https://configured-site.com"), sut.ApplicationMainUrl);

        // Attempt to overwrite via auto-detection
        sut.EnsureApplicationMainUrl(new Uri("https://non-configured-site.com"));

        Assert.AreEqual(
            new Uri("https://configured-site.com"),
            sut.ApplicationMainUrl,
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

        Assert.AreEqual(new Uri("https://configured-site.com"), sut.ApplicationMainUrl);

        // Attempt override.
        sut.EnsureApplicationMainUrl(new Uri("https://non-configured-site.com"));

        // Should remain configured value.
        Assert.AreEqual(
            new Uri("https://configured-site.com"),
            sut.ApplicationMainUrl,
            "Explicit config prevents host header poisoning");
    }
}
