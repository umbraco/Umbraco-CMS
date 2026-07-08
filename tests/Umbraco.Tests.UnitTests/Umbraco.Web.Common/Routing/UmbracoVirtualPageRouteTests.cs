// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.Routing;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Routing;

[TestFixture]
public class UmbracoVirtualPageRouteTests
{
    [Test]
    public async Task SetupVirtualPageRoute_Sets_Route_Values_For_Unnamed_Attribute_Routed_Virtual_Page()
    {
        var content = Mock.Of<IPublishedContent>();
        var controller = new TestVirtualPageController(content);
        ControllerActionDescriptor actionDescriptor = CreateActionDescriptor<TestVirtualPageController>();

        // An unnamed, attribute-routed virtual page endpoint - invisible to the name-based lookup, so before
        // the pattern-based fallback (#14165) the surface POST never re-established the virtual page here.
        RouteEndpoint endpoint = CreateEndpoint("products/{sku}", new EndpointMetadataCollection(actionDescriptor));
        var dataSource = new DefaultEndpointDataSource(endpoint);

        IPublishedRequest publishedRequest = Mock.Of<IPublishedRequest>();
        UmbracoVirtualPageRoute route = CreateRoute(dataSource, publishedRequest);

        HttpContext httpContext = CreateHttpContext("/products/ABC123", controller);

        await route.SetupVirtualPageRoute(httpContext);

        UmbracoRouteValues? routeValues = httpContext.Features.Get<UmbracoRouteValues>();
        Assert.Multiple(() =>
        {
            Assert.IsNotNull(routeValues);
            Assert.AreSame(publishedRequest, routeValues!.PublishedRequest);
            Assert.AreSame(actionDescriptor, routeValues.ControllerActionDescriptor);
        });
    }

    [Test]
    public async Task SetupVirtualPageRoute_Does_Nothing_When_No_Virtual_Page_Endpoint_Matches()
    {
        // A controller endpoint that matches the path but is not a virtual page.
        RouteEndpoint endpoint = CreateEndpoint(
            "products/{sku}",
            new EndpointMetadataCollection(CreateActionDescriptor<TestNonVirtualController>()));
        var dataSource = new DefaultEndpointDataSource(endpoint);

        UmbracoVirtualPageRoute route = CreateRoute(dataSource, Mock.Of<IPublishedRequest>());

        HttpContext httpContext = CreateHttpContext("/products/ABC123");

        await route.SetupVirtualPageRoute(httpContext);

        Assert.IsNull(httpContext.Features.Get<UmbracoRouteValues>());
    }

    [Test]
    public async Task SetupVirtualPageRoute_Does_Nothing_When_The_Request_Already_Routed_To_Content()
    {
        // A virtual page endpoint that WOULD match the path if the lookup ran.
        RouteEndpoint endpoint = CreateEndpoint(
            "products/{sku}",
            new EndpointMetadataCollection(CreateActionDescriptor<TestVirtualPageController>()));
        var dataSource = new DefaultEndpointDataSource(endpoint);

        // The request already routed to a real, non-404 content item (i.e. a normal content page).
        var routedRequest = Mock.Of<IPublishedRequest>(r =>
            r.PublishedContent == Mock.Of<IPublishedContent>() && r.ResponseStatusCode == 200);
        var routedContext = Mock.Of<IUmbracoContext>(c => c.PublishedRequest == routedRequest);

        UmbracoVirtualPageRoute route = CreateRoute(dataSource, Mock.Of<IPublishedRequest>(), routedContext);

        HttpContext httpContext = CreateHttpContext("/products/ABC123", new TestVirtualPageController(Mock.Of<IPublishedContent>()));

        await route.SetupVirtualPageRoute(httpContext);

        Assert.IsNull(httpContext.Features.Get<UmbracoRouteValues>());
    }

    [Test]
    public void FindContent_Uses_The_Virtual_Page_Controller()
    {
        var content = Mock.Of<IPublishedContent>();
        RouteEndpoint endpoint = CreateEndpoint("products/{sku}");
        UmbracoVirtualPageRoute route = CreateRoute(new DefaultEndpointDataSource(endpoint), Mock.Of<IPublishedRequest>());

        IPublishedContent? result = route.FindContent(endpoint, CreateActionExecutingContext(new TestVirtualPageController(content)));

        Assert.AreSame(content, result);
    }

    [Test]
    public void FindContent_Prefers_The_Custom_Route_Delegate_Over_The_Controller()
    {
        var delegateContent = Mock.Of<IPublishedContent>();
        RouteEndpoint endpoint = CreateEndpoint(
            "products/{sku}",
            new EndpointMetadataCollection(new CustomRouteContentFinderDelegate(_ => delegateContent)));
        UmbracoVirtualPageRoute route = CreateRoute(new DefaultEndpointDataSource(endpoint), Mock.Of<IPublishedRequest>());

        // The controller would return its own content, but the delegate takes precedence.
        IPublishedContent? result = route.FindContent(endpoint, CreateActionExecutingContext(new TestVirtualPageController(Mock.Of<IPublishedContent>())));

        Assert.AreSame(delegateContent, result);
    }

    [Test]
    public void FindContent_Returns_Null_When_Not_A_Virtual_Page()
    {
        RouteEndpoint endpoint = CreateEndpoint("products/{sku}");
        UmbracoVirtualPageRoute route = CreateRoute(new DefaultEndpointDataSource(endpoint), Mock.Of<IPublishedRequest>());

        IPublishedContent? result = route.FindContent(endpoint, CreateActionExecutingContext(new TestNonVirtualController()));

        Assert.IsNull(result);
    }

    [Test]
    public void FindContent_Populates_ActionArguments_From_The_Route_Values()
    {
        // A FindContent that resolves the item from ActionArguments (the documented pattern) must work when
        // the context is built for a surface POST, where there is no MVC model binding to populate them (#14165).
        var content = Mock.Of<IPublishedContent>();
        var controller = new ActionArgumentReadingController(content);
        RouteEndpoint endpoint = CreateEndpoint("products/{sku}");
        var actionDescriptor = new ControllerActionDescriptor
        {
            ControllerTypeInfo = typeof(ActionArgumentReadingController).GetTypeInfo(),
            MethodInfo = typeof(ActionArgumentReadingController).GetMethod(nameof(ActionArgumentReadingController.Index))!,
            ControllerName = nameof(ActionArgumentReadingController),
            ActionName = nameof(ActionArgumentReadingController.Index),
        };
        var routeValues = new RouteValueDictionary { ["sku"] = "ABC123" };

        UmbracoVirtualPageRoute route = CreateRoute(new DefaultEndpointDataSource(endpoint), Mock.Of<IPublishedRequest>());

        IPublishedContent? result = route.FindContent(endpoint, new DefaultHttpContext(), routeValues, actionDescriptor, controller);

        Assert.AreSame(content, result);
    }

    private static UmbracoVirtualPageRoute CreateRoute(
        EndpointDataSource dataSource,
        IPublishedRequest publishedRequest,
        IUmbracoContext? routedContext = null)
    {
        var publishedRouter = new Mock<IPublishedRouter>();
        publishedRouter
            .Setup(x => x.CreateRequestAsync(It.IsAny<Uri>()))
            .ReturnsAsync(Mock.Of<IPublishedRequestBuilder>());
        publishedRouter
            .Setup(x => x.RouteRequestAsync(It.IsAny<IPublishedRequestBuilder>(), It.IsAny<RouteRequestOptions>()))
            .ReturnsAsync(publishedRequest);

        IUmbracoContext? umbracoContext = routedContext;
        var umbracoContextAccessor = new Mock<IUmbracoContextAccessor>();
        umbracoContextAccessor
            .Setup(x => x.TryGetUmbracoContext(out umbracoContext))
            .Returns(routedContext is not null);

        return new UmbracoVirtualPageRoute(
            dataSource,
            Mock.Of<LinkParser>(),
            new UriUtility(Mock.Of<IHostingEnvironment>()),
            publishedRouter.Object,
            umbracoContextAccessor.Object);
    }

    private static HttpContext CreateHttpContext(string path, object? controller = null)
    {
        var services = new ServiceCollection();
        if (controller is not null)
        {
            services.AddSingleton(controller.GetType(), controller);
        }

        var httpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("localhost");
        httpContext.Request.Path = path;
        return httpContext;
    }

    private static ActionExecutingContext CreateActionExecutingContext(object controller) =>
        new(
            new ActionContext(new DefaultHttpContext(), new RouteData(), new ControllerActionDescriptor()),
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            controller);

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

    private class TestVirtualPageController : IVirtualPageController
    {
        private readonly IPublishedContent? _content;

        public TestVirtualPageController(IPublishedContent? content) => _content = content;

        public IPublishedContent? FindContent(ActionExecutingContext actionExecutingContext) => _content;
    }

    private class TestNonVirtualController
    {
    }

    private class ActionArgumentReadingController : IVirtualPageController
    {
        private readonly IPublishedContent _content;

        public ActionArgumentReadingController(IPublishedContent content) => _content = content;

        public IActionResult Index(string sku) => new EmptyResult();

        public IPublishedContent? FindContent(ActionExecutingContext actionExecutingContext)
            => actionExecutingContext.ActionArguments.TryGetValue("sku", out var sku) && sku is "ABC123"
                ? _content
                : null;
    }
}
