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
using Umbraco.Extensions;
using static Umbraco.Cms.Core.Constants.Web.Routing;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Routing;

[TestFixture]
public class PreviewRoutesTests
{
    [TestCase(RuntimeLevel.BootFailed)]
    [TestCase(RuntimeLevel.Unknown)]
    [TestCase(RuntimeLevel.Boot)]
    public void RuntimeState_No_Routes(RuntimeLevel level)
    {
        var routes = GetRoutes(level);
        var endpoints = new TestRouteBuilder();
        routes.CreateRoutes(endpoints);

        Assert.AreEqual(0, endpoints.DataSources.Count);
    }

    [TestCase(RuntimeLevel.Run)]
    [TestCase(RuntimeLevel.Upgrade)]
    [TestCase(RuntimeLevel.Install)]
    public void RuntimeState_All_Routes(RuntimeLevel level)
    {
        var routes = GetRoutes(level);
        var endpoints = new TestRouteBuilder();
        routes.CreateRoutes(endpoints);

        Assert.AreEqual(2, endpoints.DataSources.Count);
        var route = endpoints.DataSources.First();
        Assert.AreEqual(2, route.Endpoints.Count);

        var endpoint0 = (RouteEndpoint)route.Endpoints[0];
        Assert.AreEqual($"{routes.GetPreviewHubRoute()}/negotiate", endpoint0.RoutePattern.RawText);
        var endpoint1 = (RouteEndpoint)route.Endpoints[1];
        Assert.AreEqual($"{routes.GetPreviewHubRoute()}", endpoint1.RoutePattern.RawText);

        var endpoint3 = (RouteEndpoint)endpoints.DataSources.Last().Endpoints[0];
        var previewControllerName = ControllerExtensions.GetControllerName<PreviewController>();
        Assert.AreEqual(
            $"umbraco/{previewControllerName.ToLowerInvariant()}/{{action}}/{{id?}}",
            endpoint3.RoutePattern.RawText);
        Assert.AreEqual(Constants.Web.Mvc.BackOfficeArea, endpoint3.RoutePattern.Defaults["area"]);
        Assert.AreEqual("Index", endpoint3.RoutePattern.Defaults[ActionToken]);
        Assert.AreEqual(previewControllerName, endpoint3.RoutePattern.Defaults[ControllerToken]);
        Assert.AreEqual(
            endpoint3.RoutePattern.Defaults["area"],
            typeof(PreviewController).GetCustomAttribute<AreaAttribute>(false).RouteValue);
    }

    private PreviewRoutes GetRoutes(RuntimeLevel level)
    {
        var globalSettings = new GlobalSettings();
        var routes = new PreviewRoutes(
            Options.Create(globalSettings),
            Mock.Of<IHostingEnvironment>(x =>
                x.ToAbsolute(It.IsAny<string>()) == "/umbraco" && x.ApplicationVirtualPath == string.Empty),
            Mock.Of<IRuntimeState>(x => x.Level == level));
        return routes;
    }
}
