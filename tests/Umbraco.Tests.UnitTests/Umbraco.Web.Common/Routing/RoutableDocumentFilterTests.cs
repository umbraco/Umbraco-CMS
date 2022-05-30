using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Web.Common.Routing;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Routing;

[TestFixture]
public class RoutableDocumentFilterTests
{
    private IOptions<GlobalSettings> GetGlobalSettings() => Options.Create(new GlobalSettings());

    private IOptions<WebRoutingSettings> GetWebRoutingSettings() => Options.Create(new WebRoutingSettings());

    private IHostingEnvironment GetHostingEnvironment()
    {
        var hostingEnv = new Mock<IHostingEnvironment>();
        hostingEnv.Setup(x => x.ToAbsolute(It.IsAny<string>())).Returns((string virtualPath) =>
            virtualPath.TrimStart(Constants.CharArrays.TildeForwardSlash));
        return hostingEnv.Object;
    }

    [TestCase("/umbraco/editContent.aspx")]
    [TestCase("/install/default.aspx")]
    [TestCase("/install/")]
    [TestCase("/install")]
    [TestCase("/install/?installStep=asdf")]
    [TestCase("/install/test.aspx")]
    public void Is_Reserved_Path_Or_Url(string url)
    {
        var routableDocFilter = new RoutableDocumentFilter(
            GetGlobalSettings(),
            GetWebRoutingSettings(),
            GetHostingEnvironment(),
            new DefaultEndpointDataSource());

        // Will be false if it is a reserved path
        Assert.IsFalse(routableDocFilter.IsDocumentRequest(url));
    }

    [TestCase("/base/somebasehandler")]
    [TestCase("/")]
    [TestCase("/home")]
    [TestCase("/umbraco-test")]
    [TestCase("/install-test")]
    public void Is_Not_Reserved_Path_Or_Url(string url)
    {
        var routableDocFilter = new RoutableDocumentFilter(
            GetGlobalSettings(),
            GetWebRoutingSettings(),
            GetHostingEnvironment(),
            new DefaultEndpointDataSource());

        // Will be true if it's not reserved
        Assert.IsTrue(routableDocFilter.IsDocumentRequest(url));
    }

    [TestCase("/Do/Not/match", false)]
    [TestCase("/Umbraco/RenderMvcs", false)]
    [TestCase("/Umbraco/RenderMvc", true)]
    [TestCase("/umbraco/RenderMvc/Index", true)]
    [TestCase("/Umbraco/RenderMvc/Index/1234", true)]
    [TestCase("/Umbraco/RenderMvc/Index/1234/", true)]
    [TestCase("/Umbraco/RenderMvc/Index/1234/9876", false)]
    [TestCase("/api", true)]
    [TestCase("/api/WebApiTest", true)]
    [TestCase("/Api/WebApiTest/1234", true)]
    [TestCase("/api/WebApiTest/Index/1234", false)]
    public void Is_Reserved_By_Route(string url, bool isReserved)
    {
        var globalSettings = new GlobalSettings { ReservedPaths = string.Empty, ReservedUrls = string.Empty };
        var routingSettings = new WebRoutingSettings { TryMatchingEndpointsForAllPages = true };

        var endpoint1 = CreateEndpoint(
            "Umbraco/RenderMvc/{action?}/{id?}",
            new { controller = "RenderMvc" });

        var endpoint2 = CreateEndpoint(
            "api/{controller?}/{id?}",
            new { action = "Index" },
            1);

        var endpointDataSource = new DefaultEndpointDataSource(endpoint1, endpoint2);

        var routableDocFilter = new RoutableDocumentFilter(
            new TestOptionsSnapshot<GlobalSettings>(globalSettings),
            Options.Create(routingSettings),
            GetHostingEnvironment(),
            endpointDataSource);

        Assert.AreEqual(
            !isReserved, // not reserved if it's a document request
            routableDocFilter.IsDocumentRequest(url));
    }

    [TestCase("/umbraco", true)]
    [TestCase("/umbraco/", true)]
    [TestCase("/umbraco/Default", true)]
    [TestCase("/umbraco/default/", true)]
    [TestCase("/umbraco/default/123", true)]
    [TestCase("/umbraco/default/blah/123", false)]
    public void Is_Reserved_By_Default_Back_Office_Route(string url, bool isReserved)
    {
        var globalSettings = new GlobalSettings { ReservedPaths = string.Empty, ReservedUrls = string.Empty };
        var routingSettings = new WebRoutingSettings { TryMatchingEndpointsForAllPages = true };

        var endpoint1 = CreateEndpoint(
            "umbraco/{action?}/{id?}",
            new { controller = "BackOffice" });

        var endpointDataSource = new DefaultEndpointDataSource(endpoint1);

        var routableDocFilter = new RoutableDocumentFilter(
            new TestOptionsSnapshot<GlobalSettings>(globalSettings),
            Options.Create(routingSettings),
            GetHostingEnvironment(),
            endpointDataSource);

        Assert.AreEqual(
            !isReserved, // not reserved if it's a document request
            routableDocFilter.IsDocumentRequest(url));
    }

    // borrowed from https://github.com/dotnet/aspnetcore/blob/19559e73da2b6d335b864ed2855dd8a0c7a207a0/src/Mvc/Mvc.Core/test/Routing/ControllerLinkGeneratorExtensionsTest.cs#L171
    private RouteEndpoint CreateEndpoint(
        string template,
        object defaults = null,
        int order = 0) => new(
        httpContext => Task.CompletedTask,
        RoutePatternFactory.Parse(template, defaults, null),
        order,
        new EndpointMetadataCollection(Array.Empty<object>()),
        null);
}
