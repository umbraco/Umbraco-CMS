using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Tests.Common.Testing;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Routing;

[TestFixture]
public class UmbracoRequestPathsTests
{
    [OneTimeSetUp]
    public void Setup()
    {
        _hostEnvironment = Mock.Of<IWebHostEnvironment>();
        _globalSettings = new GlobalSettings();
    }

    private IWebHostEnvironment _hostEnvironment;
    private GlobalSettings _globalSettings;

    private IHostingEnvironment CreateHostingEnvironment(string virtualPath = "")
    {
        var hostingSettings = new HostingSettings { ApplicationVirtualPath = virtualPath };
        var webRoutingSettings = new WebRoutingSettings();
        var mockedOptionsMonitorOfHostingSettings =
            Mock.Of<IOptionsMonitor<HostingSettings>>(x => x.CurrentValue == hostingSettings);
        var mockedOptionsMonitorOfWebRoutingSettings =
            Mock.Of<IOptionsMonitor<WebRoutingSettings>>(x => x.CurrentValue == webRoutingSettings);

        return new TestHostingEnvironment(
            mockedOptionsMonitorOfHostingSettings,
            mockedOptionsMonitorOfWebRoutingSettings,
            _hostEnvironment);
    }

    [TestCase("/favicon.ico", true)]
    [TestCase("/umbraco_client/Tree/treeIcons.css", true)]
    [TestCase("/umbraco_client/Tree/Themes/umbraco/style.css?cdv=37", true)]
    [TestCase("/base/somebasehandler", false)]
    [TestCase("/", false)]
    [TestCase("/home.aspx", true)] // has ext, assume client side
    [TestCase("http://www.domain.com/Umbraco/test/test.aspx", true)] // has ext, assume client side
    [TestCase("http://www.domain.com/umbraco/test/test.js", true)]
    public void Is_Client_Side_Request(string url, bool assert)
    {
        var hostingEnvironment = CreateHostingEnvironment();
        var umbracoRequestPaths = new UmbracoRequestPaths(Options.Create(_globalSettings), hostingEnvironment);

        var uri = new Uri("http://test.com" + url);
        var result = umbracoRequestPaths.IsClientSideRequest(uri.AbsolutePath);
        Assert.AreEqual(assert, result);
    }

    [Test]
    public void Is_Client_Side_Request_InvalidPath_ReturnFalse()
    {
        var hostingEnvironment = CreateHostingEnvironment();
        var umbracoRequestPaths = new UmbracoRequestPaths(Options.Create(_globalSettings), hostingEnvironment);

        // This URL is invalid. Default to false when the extension cannot be determined
        var uri = new Uri("http://test.com/installing-modules+foobar+\"yipee\"");
        var result = umbracoRequestPaths.IsClientSideRequest(uri.AbsolutePath);
        Assert.AreEqual(false, result);
    }

    [TestCase("http://www.domain.com/umbraco/preview/frame?id=1234", "", true)]
    [TestCase("http://www.domain.com/umbraco", "", true)]
    [TestCase("http://www.domain.com/Umbraco/", "", true)]
    [TestCase("http://www.domain.com/umbraco/default.aspx", "", true)]
    [TestCase("http://www.domain.com/umbraco/test/test", "", false)]
    [TestCase("http://www.domain.com/umbraco/test/test/test", "", false)]
    [TestCase("http://www.domain.com/umbrac", "", false)]
    [TestCase("http://www.domain.com/test", "", false)]
    [TestCase("http://www.domain.com/test/umbraco", "", false)]
    [TestCase("http://www.domain.com/Umbraco/Backoffice/blah", "", true)]
    [TestCase("http://www.domain.com/Umbraco/anything", "", true)]
    [TestCase("http://www.domain.com/Umbraco/anything/", "", true)]
    [TestCase("http://www.domain.com/Umbraco/surface/blah", "", false)]
    [TestCase("http://www.domain.com/umbraco/api/blah", "", false)]
    [TestCase("http://www.domain.com/myvdir/umbraco/api/blah", "myvdir", false)]
    [TestCase("http://www.domain.com/MyVdir/umbraco/api/blah", "/myvdir", false)]
    [TestCase("http://www.domain.com/MyVdir/Umbraco/", "myvdir", true)]
    public void Is_Back_Office_Request(string input, string virtualPath, bool expected)
    {
        var source = new Uri(input);
        var hostingEnvironment = CreateHostingEnvironment(virtualPath);
        var umbracoRequestPaths = new UmbracoRequestPaths(Options.Create(_globalSettings), hostingEnvironment);
        Assert.AreEqual(expected, umbracoRequestPaths.IsBackOfficeRequest(source.AbsolutePath));
    }

    [TestCase("http://www.domain.com/install", true)]
    [TestCase("http://www.domain.com/Install/", true)]
    [TestCase("http://www.domain.com/install/default.aspx", true)]
    [TestCase("http://www.domain.com/install/test/test", true)]
    [TestCase("http://www.domain.com/Install/test/test.aspx", true)]
    [TestCase("http://www.domain.com/install/test/test.js", true)]
    [TestCase("http://www.domain.com/instal", false)]
    [TestCase("http://www.domain.com/umbraco", false)]
    [TestCase("http://www.domain.com/umbraco/umbraco", false)]
    public void Is_Installer_Request(string input, bool expected)
    {
        var source = new Uri(input);
        var hostingEnvironment = CreateHostingEnvironment();
        var umbracoRequestPaths = new UmbracoRequestPaths(Options.Create(_globalSettings), hostingEnvironment);
        Assert.AreEqual(expected, umbracoRequestPaths.IsInstallerRequest(source.AbsolutePath));
    }
}
