// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.Extensions;
using Umbraco.Cms.Web.Common.Routing;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Extensions;

[TestFixture]
public class EndpointDataSourceExtensionsTests
{
    [Test]
    public void GetEndpointByPath_Returns_Null_When_There_Are_No_Endpoints()
    {
        var dataSource = new DefaultEndpointDataSource(Array.Empty<Endpoint>());

        Endpoint? result = dataSource.GetEndpointByPath(Mock.Of<LinkParser>(), new PathString("/shop/index"), out RouteValueDictionary? routeValues);

        Assert.Multiple(() =>
        {
            Assert.IsNull(result);
            Assert.IsNull(routeValues);
        });
    }

    [Test]
    public void GetEndpointByPath_Ignores_Endpoints_Without_A_Route_Name()
    {
        // An unnamed endpoint whose pattern would match the path. This is the case the name-based lookup
        // cannot handle, which is why GetEndpointByRoutePattern exists for attribute-routed virtual pages (#14165).
        RouteEndpoint unnamed = CreateEndpoint("shop/{action}");
        var dataSource = new DefaultEndpointDataSource(unnamed);

        // The parser would match if it were ever consulted; it must not be consulted for an unnamed endpoint.
        var linkParser = new Mock<LinkParser>();
        linkParser
            .Setup(x => x.ParsePathByAddress<string>(It.IsAny<string>(), It.IsAny<PathString>()))
            .Returns(new RouteValueDictionary());

        Endpoint? result = dataSource.GetEndpointByPath(linkParser.Object, new PathString("/shop/index"), out RouteValueDictionary? routeValues);

        Assert.Multiple(() =>
        {
            Assert.IsNull(result);
            Assert.IsNull(routeValues);
        });
        linkParser.Verify(x => x.ParsePathByAddress<string>(It.IsAny<string>(), It.IsAny<PathString>()), Times.Never);
    }

    [Test]
    public void GetEndpointByPath_Returns_Named_Endpoint_And_Its_Route_Values()
    {
        RouteEndpoint named = CreateEndpoint("shop/{action}", new EndpointMetadataCollection(new RouteNameMetadata("shop")));
        var dataSource = new DefaultEndpointDataSource(named);

        var parsedValues = new RouteValueDictionary { ["action"] = "index" };
        var linkParser = new Mock<LinkParser>();
        linkParser
            .Setup(x => x.ParsePathByAddress<string>("shop", It.IsAny<PathString>()))
            .Returns(parsedValues);

        Endpoint? result = dataSource.GetEndpointByPath(linkParser.Object, new PathString("/shop/index"), out RouteValueDictionary? routeValues);

        Assert.Multiple(() =>
        {
            Assert.AreSame(named, result);
            Assert.AreSame(parsedValues, routeValues);
        });
    }

    [Test]
    public void GetEndpointByPath_Returns_Null_When_The_Parser_Does_Not_Match_The_Path()
    {
        RouteEndpoint named = CreateEndpoint("shop/{action}", new EndpointMetadataCollection(new RouteNameMetadata("shop")));
        var dataSource = new DefaultEndpointDataSource(named);

        // The endpoint is named but the path does not parse against it, so the parser returns null.
        var linkParser = new Mock<LinkParser>();
        linkParser
            .Setup(x => x.ParsePathByAddress<string>(It.IsAny<string>(), It.IsAny<PathString>()))
            .Returns((RouteValueDictionary?)null);

        Endpoint? result = dataSource.GetEndpointByPath(linkParser.Object, new PathString("/something/else"), out RouteValueDictionary? routeValues);

        Assert.Multiple(() =>
        {
            Assert.IsNull(result);
            Assert.IsNull(routeValues);
        });
    }

    [Test]
    public void GetEndpointByRoutePattern_Finds_Unnamed_Attribute_Routed_Virtual_Page_Endpoint()
    {
        // The catch-all is first and matches every path, but is not a virtual page (no descriptor).
        RouteEndpoint catchAll = CreateEndpoint("{**umbracoSlug}");
        RouteEndpoint virtualPage = CreateEndpoint(
            "products/{sku}",
            new EndpointMetadataCollection(CreateActionDescriptor<TestVirtualPageController>()));

        var dataSource = new DefaultEndpointDataSource(catchAll, virtualPage);

        Endpoint? result = dataSource.GetEndpointByRoutePattern(
            new PathString("/products/ABC123"),
            UmbracoVirtualPageRoute.IsVirtualPageEndpoint,
            out RouteValueDictionary? routeValues);

        Assert.Multiple(() =>
        {
            Assert.AreSame(virtualPage, result);
            Assert.IsNotNull(routeValues);
            Assert.AreEqual("ABC123", routeValues!["sku"]);
        });
    }

    [Test]
    public void GetEndpointByRoutePattern_Finds_Custom_Route_With_Content_Finder_Delegate()
    {
        RouteEndpoint customRoute = CreateEndpoint(
            "things/{id}",
            new EndpointMetadataCollection(
                CreateActionDescriptor<TestNonVirtualController>(),
                new CustomRouteContentFinderDelegate(_ => Mock.Of<IPublishedContent>())));

        var dataSource = new DefaultEndpointDataSource(customRoute);

        Endpoint? result = dataSource.GetEndpointByRoutePattern(
            new PathString("/things/42"),
            UmbracoVirtualPageRoute.IsVirtualPageEndpoint,
            out _);

        Assert.AreSame(customRoute, result);
    }

    [Test]
    public void GetEndpointByRoutePattern_Returns_Null_When_No_Virtual_Page_Endpoint_Matches()
    {
        // A controller endpoint that matches the path but is neither IVirtualPageController nor a custom route.
        RouteEndpoint nonVirtual = CreateEndpoint(
            "products/{sku}",
            new EndpointMetadataCollection(CreateActionDescriptor<TestNonVirtualController>()));

        var dataSource = new DefaultEndpointDataSource(nonVirtual);

        Endpoint? result = dataSource.GetEndpointByRoutePattern(
            new PathString("/products/ABC123"),
            UmbracoVirtualPageRoute.IsVirtualPageEndpoint,
            out RouteValueDictionary? routeValues);

        Assert.Multiple(() =>
        {
            Assert.IsNull(result);
            Assert.IsNull(routeValues);
        });
    }

    [Test]
    public void IsVirtualPageEndpoint_Is_False_For_Endpoint_Without_Controller_Action_Descriptor()
        => Assert.IsFalse(UmbracoVirtualPageRoute.IsVirtualPageEndpoint(CreateEndpoint("{**umbracoSlug}")));

    private static RouteEndpoint CreateEndpoint(string template, EndpointMetadataCollection? metadata = null) =>
        new(
            httpContext => Task.CompletedTask,
            RoutePatternFactory.Parse(template),
            order: 0,
            metadata ?? new EndpointMetadataCollection(Array.Empty<object>()),
            displayName: template);

    private static ControllerActionDescriptor CreateActionDescriptor<TController>() =>
        new()
        {
            ControllerTypeInfo = typeof(TController).GetTypeInfo(),
            ControllerName = typeof(TController).Name,
            ActionName = "Index",
        };

    private class TestVirtualPageController : Controller, IVirtualPageController
    {
        public IPublishedContent? FindContent(ActionExecutingContext actionExecutingContext) => null;
    }

    private class TestNonVirtualController : Controller
    {
    }
}
