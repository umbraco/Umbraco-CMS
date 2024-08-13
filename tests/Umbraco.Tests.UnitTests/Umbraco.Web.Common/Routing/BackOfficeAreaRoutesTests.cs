// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Security;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;
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

        Assert.AreEqual(2, endpoints.DataSources.Count);
        var route = endpoints.DataSources.First();
        Assert.AreEqual(2, route.Endpoints.Count);

        AssertMinimalBackOfficeRoutes(route);
    }

    private void AssertMinimalBackOfficeRoutes(EndpointDataSource route)
    {
        var endpoint1 = (RouteEndpoint)route.Endpoints[0];
        Assert.AreEqual("umbraco/{action}/{id?}", endpoint1.RoutePattern.RawText);
        Assert.AreEqual("Index", endpoint1.RoutePattern.Defaults[ActionToken]);
        Assert.AreEqual(ControllerExtensions.GetControllerName<BackOfficeDefaultController>(), endpoint1.RoutePattern.Defaults[ControllerToken]);
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
