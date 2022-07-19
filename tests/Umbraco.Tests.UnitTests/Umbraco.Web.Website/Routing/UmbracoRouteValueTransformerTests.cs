using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Cms.Web.Website.Routing;
using Umbraco.Extensions;
using static Umbraco.Cms.Core.Constants.Web.Routing;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Website.Routing;

[TestFixture]
public class UmbracoRouteValueTransformerTests
{
    private IOptions<GlobalSettings> GetGlobalSettings() => Options.Create(new GlobalSettings());

    private UmbracoRouteValueTransformer GetTransformerWithRunState(
        IUmbracoContextAccessor ctx,
        IRoutableDocumentFilter filter = null,
        IPublishedRouter router = null,
        IUmbracoRouteValuesFactory routeValuesFactory = null)
        => GetTransformer(ctx, Mock.Of<IRuntimeState>(x => x.Level == RuntimeLevel.Run), filter, router, routeValuesFactory);

    private UmbracoRouteValueTransformer GetTransformer(
        IUmbracoContextAccessor ctx,
        IRuntimeState state,
        IRoutableDocumentFilter filter = null,
        IPublishedRouter router = null,
        IUmbracoRouteValuesFactory routeValuesFactory = null)
    {
        var publicAccessRequestHandler = new Mock<IPublicAccessRequestHandler>();
        publicAccessRequestHandler.Setup(x =>
                x.RewriteForPublishedContentAccessAsync(It.IsAny<HttpContext>(), It.IsAny<UmbracoRouteValues>()))
            .Returns((HttpContext ctx, UmbracoRouteValues routeVals) => Task.FromResult(routeVals));

        var transformer = new UmbracoRouteValueTransformer(
            new NullLogger<UmbracoRouteValueTransformer>(),
            ctx,
            router ?? Mock.Of<IPublishedRouter>(),
            GetGlobalSettings(),
            TestHelper.GetHostingEnvironment(),
            state,
            routeValuesFactory ?? Mock.Of<IUmbracoRouteValuesFactory>(),
            filter ?? Mock.Of<IRoutableDocumentFilter>(x => x.IsDocumentRequest(It.IsAny<string>()) == true),
            Mock.Of<IDataProtectionProvider>(),
            Mock.Of<IControllerActionSearcher>(),
            Mock.Of<IEventAggregator>(),
            publicAccessRequestHandler.Object);

        return transformer;
    }

    private IUmbracoContext GetUmbracoContext(bool hasContent)
    {
        var publishedContent = Mock.Of<IPublishedContentCache>(x => x.HasContent() == hasContent);
        var uri = new Uri("http://example.com");

        var umbracoContext = Mock.Of<IUmbracoContext>(x =>
            x.Content == publishedContent
            && x.OriginalRequestUrl == uri
            && x.CleanedUmbracoUrl == uri);

        return umbracoContext;
    }

    private UmbracoRouteValues GetRouteValues(IPublishedRequest request)
        => new(
            request,
            new ControllerActionDescriptor
            {
                ControllerTypeInfo = typeof(TestController).GetTypeInfo(),
                ControllerName = ControllerExtensions.GetControllerName<TestController>(),
            });

    private IUmbracoRouteValuesFactory GetRouteValuesFactory(IPublishedRequest request)
        => Mock.Of<IUmbracoRouteValuesFactory>(x =>
            x.CreateAsync(It.IsAny<HttpContext>(), It.IsAny<IPublishedRequest>()) ==
            Task.FromResult(GetRouteValues(request)));

    private IPublishedRouter GetRouter(IPublishedRequest request)
        => Mock.Of<IPublishedRouter>(x =>
            x.RouteRequestAsync(It.IsAny<IPublishedRequestBuilder>(), It.IsAny<RouteRequestOptions>()) ==
            Task.FromResult(request));

    [Test]
    public async Task Null_When_Runtime_Level_Not_Run()
    {
        var transformer = GetTransformer(
            Mock.Of<IUmbracoContextAccessor>(),
            Mock.Of<IRuntimeState>());

        var result = await transformer.TransformAsync(new DefaultHttpContext(), new RouteValueDictionary());
        Assert.IsNull(result);
    }

    [Test]
    public async Task Null_When_No_Umbraco_Context()
    {
        var transformer = GetTransformerWithRunState(
            Mock.Of<IUmbracoContextAccessor>());

        var result = await transformer.TransformAsync(new DefaultHttpContext(), new RouteValueDictionary());
        Assert.IsNull(result);
    }

    [Test]
    public async Task Null_When_Not_Document_Request()
    {
        var umbracoContext = Mock.Of<IUmbracoContext>();
        var transformer = GetTransformerWithRunState(
            Mock.Of<IUmbracoContextAccessor>(x => x.TryGetUmbracoContext(out umbracoContext)),
            Mock.Of<IRoutableDocumentFilter>(x => x.IsDocumentRequest(It.IsAny<string>()) == false));

        var result = await transformer.TransformAsync(new DefaultHttpContext(), new RouteValueDictionary());
        Assert.IsNull(result);
    }

    [Test]
    public async Task NoContentController_Values_When_No_Content()
    {
        var umbracoContext = GetUmbracoContext(false);

        var transformer = GetTransformerWithRunState(
            Mock.Of<IUmbracoContextAccessor>(x => x.TryGetUmbracoContext(out umbracoContext)));

        var result = await transformer.TransformAsync(new DefaultHttpContext(), new RouteValueDictionary());
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(ControllerExtensions.GetControllerName<RenderNoContentController>(), result[ControllerToken]);
        Assert.AreEqual(nameof(RenderNoContentController.Index), result[ActionToken]);
    }

    [Test]
    public async Task Assigns_PublishedRequest_To_UmbracoContext()
    {
        var umbracoContext = GetUmbracoContext(true);
        var request = Mock.Of<IPublishedRequest>();

        var transformer = GetTransformerWithRunState(
            Mock.Of<IUmbracoContextAccessor>(x => x.TryGetUmbracoContext(out umbracoContext)),
            router: GetRouter(request),
            routeValuesFactory: GetRouteValuesFactory(request));

        await transformer.TransformAsync(new DefaultHttpContext(), new RouteValueDictionary());
        Assert.AreEqual(request, umbracoContext.PublishedRequest);
    }

    [Test]
    public async Task Null_When_No_Content_On_PublishedRequest()
    {
        var umbracoContext = GetUmbracoContext(true);
        var request = Mock.Of<IPublishedRequest>(x => x.PublishedContent == null);

        var transformer = GetTransformerWithRunState(
            Mock.Of<IUmbracoContextAccessor>(x => x.TryGetUmbracoContext(out umbracoContext)),
            router: GetRouter(request),
            routeValuesFactory: GetRouteValuesFactory(request));

        var httpContext = new DefaultHttpContext();
        var result = await transformer.TransformAsync(httpContext, new RouteValueDictionary());
        Assert.IsNull(result);

        var routeVals = httpContext.Features.Get<UmbracoRouteValues>();
        Assert.AreEqual(routeVals.PublishedRequest.GetRouteResult(), UmbracoRouteResult.NotFound);
    }

    [Test]
    public async Task Assigns_UmbracoRouteValues_To_HttpContext_Feature()
    {
        var umbracoContext = GetUmbracoContext(true);
        var request = Mock.Of<IPublishedRequest>(x => x.PublishedContent == Mock.Of<IPublishedContent>());

        var transformer = GetTransformerWithRunState(
            Mock.Of<IUmbracoContextAccessor>(x => x.TryGetUmbracoContext(out umbracoContext)),
            router: GetRouter(request),
            routeValuesFactory: GetRouteValuesFactory(request));

        var httpContext = new DefaultHttpContext();
        await transformer.TransformAsync(httpContext, new RouteValueDictionary());

        var routeVals = httpContext.Features.Get<UmbracoRouteValues>();
        Assert.IsNotNull(routeVals);
        Assert.AreEqual(routeVals.PublishedRequest, umbracoContext.PublishedRequest);
    }

    [Test]
    public async Task Assigns_Values_To_RouteValueDictionary_When_Content()
    {
        var umbracoContext = GetUmbracoContext(true);
        var request = Mock.Of<IPublishedRequest>(x => x.PublishedContent == Mock.Of<IPublishedContent>());
        var routeValues = GetRouteValues(request);

        var transformer = GetTransformerWithRunState(
            Mock.Of<IUmbracoContextAccessor>(x => x.TryGetUmbracoContext(out umbracoContext)),
            router: GetRouter(request),
            routeValuesFactory: GetRouteValuesFactory(request));

        var result = await transformer.TransformAsync(new DefaultHttpContext(), new RouteValueDictionary());

        Assert.AreEqual(routeValues.ControllerName, result[ControllerToken]);
        Assert.AreEqual(routeValues.ActionName, result[ActionToken]);
    }

    [Test]
    public async Task Returns_Null_RouteValueDictionary_When_No_Content()
    {
        var umbracoContext = GetUmbracoContext(true);
        var request = Mock.Of<IPublishedRequest>(x => x.PublishedContent == null);
        GetRouteValues(request);

        var transformer = GetTransformerWithRunState(
            Mock.Of<IUmbracoContextAccessor>(x => x.TryGetUmbracoContext(out umbracoContext)),
            router: GetRouter(request),
            routeValuesFactory: GetRouteValuesFactory(request));

        var result = await transformer.TransformAsync(new DefaultHttpContext(), new RouteValueDictionary());

        Assert.IsNull(result);
    }

    private class TestController : RenderController
    {
        public TestController(ILogger<TestController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoContextAccessor umbracoContextAccessor)
            : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
        }
    }
}
