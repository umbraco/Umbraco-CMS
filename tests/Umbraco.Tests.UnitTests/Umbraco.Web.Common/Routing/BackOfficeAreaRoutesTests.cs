// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.BackOffice.Routing;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Extensions;
using static Umbraco.Cms.Core.Constants.Web.Routing;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Routing;

[TestFixture]
public class BackOfficeAreaRoutesTests
{
    [TestCase(RuntimeLevel.BootFailed)]
    [TestCase(RuntimeLevel.Unknown)]
    [TestCase(RuntimeLevel.Boot)]
    public void RuntimeState_No_Routes(RuntimeLevel level)
    {
        var routes = GetBackOfficeAreaRoutes(level);
        var endpoints = new TestRouteBuilder();
        routes.CreateRoutes(endpoints);

        Assert.AreEqual(0, endpoints.DataSources.Count);
    }

    [Test]
    [TestCase(RuntimeLevel.Run)]
    [TestCase(RuntimeLevel.Upgrade)]
    [TestCase(RuntimeLevel.Install)]
    public void RuntimeState_All_Routes(RuntimeLevel level)
    {
        var routes = GetBackOfficeAreaRoutes(level);
        var endpoints = new TestRouteBuilder();
        routes.CreateRoutes(endpoints);

        Assert.AreEqual(1, endpoints.DataSources.Count);
        var route = endpoints.DataSources.First();
        Assert.AreEqual(3, route.Endpoints.Count);

        AssertMinimalBackOfficeRoutes(route);

        var endpoint4 = (RouteEndpoint)route.Endpoints[2];
        var apiControllerName = ControllerExtensions.GetControllerName<Testing1Controller>();
        Assert.AreEqual(
            $"umbraco/backoffice/api/{apiControllerName.ToLowerInvariant()}/{{action}}/{{id?}}",
            endpoint4.RoutePattern.RawText);
        Assert.IsFalse(endpoint4.RoutePattern.Defaults.ContainsKey(AreaToken));
        Assert.IsFalse(endpoint4.RoutePattern.Defaults.ContainsKey(ActionToken));
        Assert.AreEqual(apiControllerName, endpoint4.RoutePattern.Defaults[ControllerToken]);
    }

    private void AssertMinimalBackOfficeRoutes(EndpointDataSource route)
    {
        var endpoint1 = (RouteEndpoint)route.Endpoints[0];
        Assert.AreEqual("umbraco/{action}/{id?}", endpoint1.RoutePattern.RawText);
        Assert.AreEqual(Constants.Web.Mvc.BackOfficeArea, endpoint1.RoutePattern.Defaults[AreaToken]);
        Assert.AreEqual("Default", endpoint1.RoutePattern.Defaults[ActionToken]);
        Assert.AreEqual(
            ControllerExtensions.GetControllerName<BackOfficeController>(),
            endpoint1.RoutePattern.Defaults[ControllerToken]);
        Assert.AreEqual(
            endpoint1.RoutePattern.Defaults[AreaToken],
            typeof(BackOfficeController).GetCustomAttribute<AreaAttribute>(false).RouteValue);

        var endpoint2 = (RouteEndpoint)route.Endpoints[1];
        var controllerName = ControllerExtensions.GetControllerName<AuthenticationController>();
        Assert.AreEqual(
            $"umbraco/backoffice/{Constants.Web.Mvc.BackOfficeApiArea.ToLowerInvariant()}/{controllerName.ToLowerInvariant()}/{{action}}/{{id?}}",
            endpoint2.RoutePattern.RawText);
        Assert.AreEqual(Constants.Web.Mvc.BackOfficeApiArea, endpoint2.RoutePattern.Defaults[AreaToken]);
        Assert.IsFalse(endpoint2.RoutePattern.Defaults.ContainsKey(ActionToken));
        Assert.AreEqual(controllerName, endpoint2.RoutePattern.Defaults[ControllerToken]);
        Assert.AreEqual(
            endpoint1.RoutePattern.Defaults[AreaToken],
            typeof(BackOfficeController).GetCustomAttribute<AreaAttribute>(false).RouteValue);
    }

    private BackOfficeAreaRoutes GetBackOfficeAreaRoutes(RuntimeLevel level)
    {
        var globalSettings = new GlobalSettings();
        var routes = new BackOfficeAreaRoutes(
            Options.Create(globalSettings),
            Mock.Of<IHostingEnvironment>(x =>
                x.ToAbsolute(It.IsAny<string>()) == "/umbraco" && x.ApplicationVirtualPath == string.Empty),
            Mock.Of<IRuntimeState>(x => x.Level == level),
            new UmbracoApiControllerTypeCollection(() => new[] { typeof(Testing1Controller) }));

        return routes;
    }

    [IsBackOffice]
    private class Testing1Controller : UmbracoApiController
    {
    }
}
