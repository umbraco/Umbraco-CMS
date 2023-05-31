// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Install;
using Umbraco.Extensions;
using static Umbraco.Cms.Core.Constants.Web.Routing;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Routing;

[TestFixture]
public class InstallAreaRoutesTests
{
    [TestCase(RuntimeLevel.BootFailed)]
    [TestCase(RuntimeLevel.Unknown)]
    [TestCase(RuntimeLevel.Boot)]
    public void RuntimeState_No_Routes(RuntimeLevel level)
    {
        var routes = GetInstallAreaRoutes(level);
        var endpoints = new TestRouteBuilder();
        routes.CreateRoutes(endpoints);

        Assert.AreEqual(0, endpoints.DataSources.Count);
    }

    [TestCase(RuntimeLevel.Install)]
    [TestCase(RuntimeLevel.Upgrade)]
    public void RuntimeState_Install(RuntimeLevel level)
    {
        var routes = GetInstallAreaRoutes(level);
        var endpoints = new TestRouteBuilder();
        routes.CreateRoutes(endpoints);

        Assert.AreEqual(1, endpoints.DataSources.Count);
        var route = endpoints.DataSources.First();
        Assert.AreEqual(2, route.Endpoints.Count);

        var endpoint1 = (RouteEndpoint)route.Endpoints[0];
        Assert.AreEqual("install/api/{action}/{id?}", endpoint1.RoutePattern.RawText);
        Assert.AreEqual(Constants.Web.Mvc.InstallArea, endpoint1.RoutePattern.Defaults[AreaToken]);
        Assert.AreEqual("Index", endpoint1.RoutePattern.Defaults[ActionToken]);
        Assert.AreEqual(
            ControllerExtensions.GetControllerName<InstallApiController>(),
            endpoint1.RoutePattern.Defaults[ControllerToken]);
        Assert.AreEqual(
            endpoint1.RoutePattern.Defaults[AreaToken],
            typeof(InstallApiController).GetCustomAttribute<AreaAttribute>(false).RouteValue);

        var endpoint2 = (RouteEndpoint)route.Endpoints[1];
        Assert.AreEqual("install/{action}/{id?}", endpoint2.RoutePattern.RawText);
        Assert.AreEqual(Constants.Web.Mvc.InstallArea, endpoint2.RoutePattern.Defaults[AreaToken]);
        Assert.AreEqual("Index", endpoint2.RoutePattern.Defaults[ActionToken]);
        Assert.AreEqual(
            ControllerExtensions.GetControllerName<InstallController>(),
            endpoint2.RoutePattern.Defaults[ControllerToken]);
        Assert.AreEqual(
            endpoint2.RoutePattern.Defaults[AreaToken],
            typeof(InstallController).GetCustomAttribute<AreaAttribute>(false).RouteValue);

        var dataSource = endpoints.DataSources.Last();
        Assert.AreEqual(2, dataSource.Endpoints.Count);

        Assert.AreEqual("Route: install/api/{action}/{id?}", dataSource.Endpoints[0].ToString());
        Assert.AreEqual("Route: install/{action}/{id?}", dataSource.Endpoints[1].ToString());
    }

    [Test]
    public void RuntimeState_Run()
    {
        var routes = GetInstallAreaRoutes(RuntimeLevel.Run);
        var endpoints = new TestRouteBuilder();
        routes.CreateRoutes(endpoints);

        Assert.AreEqual(1, endpoints.DataSources.Count);
        var route = endpoints.DataSources.First();
        Assert.AreEqual(2, route.Endpoints.Count);

        var endpoint = (RouteEndpoint)route.Endpoints[0];
        Assert.AreEqual("install/api/{action}/{id?}", endpoint.RoutePattern.RawText);
        
        endpoint = (RouteEndpoint)route.Endpoints[1];
        Assert.AreEqual("install/{action}/{id?}", endpoint.RoutePattern.RawText);
    }

    private InstallAreaRoutes GetInstallAreaRoutes(RuntimeLevel level) =>
        new(
            Mock.Of<IRuntimeState>(x => x.Level == level),
            Mock.Of<IHostingEnvironment>(x =>
                x.ToAbsolute(It.IsAny<string>()) == "/install" && x.ApplicationVirtualPath == string.Empty),
            Mock.Of<LinkGenerator>());
}
