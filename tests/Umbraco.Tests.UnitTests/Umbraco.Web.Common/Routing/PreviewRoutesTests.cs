// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Preview;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;
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

        Assert.AreEqual(1, endpoints.DataSources.Count);
        var route = endpoints.DataSources.First();
        Assert.AreEqual(2, route.Endpoints.Count);

        var endpoint0 = (RouteEndpoint)route.Endpoints[0];
        Assert.AreEqual($"{routes.GetPreviewHubRoute()}/negotiate", endpoint0.RoutePattern.RawText);
        var endpoint1 = (RouteEndpoint)route.Endpoints[1];
        Assert.AreEqual($"{routes.GetPreviewHubRoute()}", endpoint1.RoutePattern.RawText);
    }

    private PreviewRoutes GetRoutes(RuntimeLevel level)
        => new PreviewRoutes(Mock.Of<IRuntimeState>(x => x.Level == level));
}
