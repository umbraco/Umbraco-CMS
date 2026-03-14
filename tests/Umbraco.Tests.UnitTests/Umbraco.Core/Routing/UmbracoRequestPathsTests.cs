using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Tests.Common.Testing;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Routing;

    /// <summary>
    /// Contains unit tests for verifying the behavior and correctness of the <see cref="UmbracoRequestPaths"/> class in the Umbraco CMS routing system.
    /// These tests ensure that request path handling and related logic function as expected.
    /// </summary>
[TestFixture]
public class UmbracoRequestPathsTests
{
    private IWebHostEnvironment _hostEnvironment;
    private UmbracoRequestPathsOptions _umbracoRequestPathsOptions;

    /// <summary>
    /// Sets up the test environment for UmbracoRequestPathsTests.
    /// </summary>
    [OneTimeSetUp]
    public void Setup()
    {
        _hostEnvironment = Mock.Of<IWebHostEnvironment>();
        _umbracoRequestPathsOptions = new UmbracoRequestPathsOptions();
    }

    private IHostingEnvironment CreateHostingEnvironment(string virtualPath = "")
    {
        var hostingSettings = new HostingSettings { ApplicationVirtualPath = virtualPath };
        var webRoutingSettings = new WebRoutingSettings();
        var mockedOptionsMonitorOfHostingSettings = Mock.Of<IOptionsMonitor<HostingSettings>>(x => x.CurrentValue == hostingSettings);
        var mockedOptionsMonitorOfWebRoutingSettings = Mock.Of<IOptionsMonitor<WebRoutingSettings>>(x => x.CurrentValue == webRoutingSettings);

        return new TestHostingEnvironment(
            mockedOptionsMonitorOfHostingSettings,
            mockedOptionsMonitorOfWebRoutingSettings,
            _hostEnvironment);
    }

    /// <summary>
    /// Unit test for <see cref="UmbracoRequestPaths.IsClientSideRequest"/> that verifies whether a given URL path is correctly identified as a client-side request (such as static assets).
    /// </summary>
    /// <param name="url">The URL path to evaluate for client-side request status.</param>
    /// <param name="assert">The expected boolean result; <c>true</c> if the URL should be considered a client-side request, otherwise <c>false</c>.</param>
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
        var umbracoRequestPaths = new UmbracoRequestPaths(hostingEnvironment, Options.Create(_umbracoRequestPathsOptions));

        var uri = new Uri("http://test.com" + url);
        var result = umbracoRequestPaths.IsClientSideRequest(uri.AbsolutePath);
        Assert.AreEqual(assert, result);
    }

    /// <summary>
    /// Tests that an invalid client-side request path returns false.
    /// </summary>
    [Test]
    public void Is_Client_Side_Request_InvalidPath_ReturnFalse()
    {
        var hostingEnvironment = CreateHostingEnvironment();
        var umbracoRequestPaths = new UmbracoRequestPaths(hostingEnvironment, Options.Create(_umbracoRequestPathsOptions));

        // This URL is invalid. Default to false when the extension cannot be determined
        var uri = new Uri("http://test.com/installing-modules+foobar+\"yipee\"");
        var result = umbracoRequestPaths.IsClientSideRequest(uri.AbsolutePath);
        Assert.AreEqual(false, result);
    }

    /// <summary>
    /// Verifies whether a given input URL and virtual path are identified as a back office request by <see cref="UmbracoRequestPaths.IsBackOfficeRequest"/>.
    /// </summary>
    /// <param name="input">The input URL to evaluate.</param>
    /// <param name="virtualPath">The virtual path of the hosting environment (e.g., application root or subdirectory).</param>
    /// <param name="expected">True if the request should be recognized as a back office request; otherwise, false.</param>
    /// <remarks>
    /// This test uses various URL and virtual path combinations to ensure correct identification of Umbraco back office requests.
    /// </remarks>
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
    [TestCase("http://www.domain.com/Umbraco/management/api/", "", true)]
    [TestCase("http://www.domain.com/Umbraco/management/api", "", false)]
    [TestCase("http://www.domain.com/umbraco/management/api/v1.0/my/controller/action/", "", true)]
    public void Is_Back_Office_Request(string input, string virtualPath, bool expected)
    {
        var source = new Uri(input);
        var hostingEnvironment = CreateHostingEnvironment(virtualPath);
        var umbracoRequestPaths = new UmbracoRequestPaths(hostingEnvironment, Options.Create(_umbracoRequestPathsOptions));
        Assert.AreEqual(expected, umbracoRequestPaths.IsBackOfficeRequest(source.AbsolutePath));
    }

    /// <summary>
    /// Verifies that the <see cref="UmbracoRequestPaths.IsBackOfficeRequest"/> method correctly determines whether a request is considered a back office request
    /// based on different input URL paths and custom <see cref="UmbracoRequestPathsOptions"/> configuration.
    /// </summary>
    /// <param name="input">The input URL string to evaluate.</param>
    /// <param name="expected">True if the request should be identified as a back office request; otherwise, false.</param>
    [TestCase("http://www.domain.com/some/path", false)]
    [TestCase("http://www.domain.com/umbraco/surface/blah", false)]
    [TestCase("http://www.domain.com/umbraco/api/blah", false)]
    [TestCase("http://www.domain.com/umbraco/management/api/v1.0/my/controller/action/", true)]
    public void Force_Back_Office_Request_With_Request_Paths_Options(string input, bool expected)
    {
        var source = new Uri(input);
        var hostingEnvironment = CreateHostingEnvironment();
        var umbracoRequestPathsOptions = new UmbracoRequestPathsOptions
        {
            IsBackOfficeRequest = _ => true
        };
        var umbracoRequestPaths = new UmbracoRequestPaths(hostingEnvironment, Options.Create(umbracoRequestPathsOptions));
        Assert.AreEqual(expected, umbracoRequestPaths.IsBackOfficeRequest(source.AbsolutePath));
    }
}
