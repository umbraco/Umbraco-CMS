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

        Assert.That(endpoints.DataSources.Count, Is.EqualTo(0));
    }

    [TestCase(RuntimeLevel.Run)]
    [TestCase(RuntimeLevel.Upgrade)]
    [TestCase(RuntimeLevel.Install)]
    public void RuntimeState_All_Routes(RuntimeLevel level)
    {
        var routes = GetRoutes(level);
        var endpoints = new TestRouteBuilder();
        routes.CreateRoutes(endpoints);

        Assert.That(endpoints.DataSources, Has.Count.EqualTo(1));
        var route = endpoints.DataSources.First();
        Assert.That(route.Endpoints, Has.Count.EqualTo(2));

        var endpoint0 = (RouteEndpoint)route.Endpoints[0];
        Assert.That(endpoint0.RoutePattern.RawText, Is.EqualTo($"{routes.GetPreviewHubRoute()}/negotiate"));
        var endpoint1 = (RouteEndpoint)route.Endpoints[1];
        Assert.That(endpoint1.RoutePattern.RawText, Is.EqualTo($"{routes.GetPreviewHubRoute()}"));
    }

    private PreviewRoutes GetRoutes(RuntimeLevel level)
        => new PreviewRoutes(
            Mock.Of<IRuntimeState>(x => x.Level == level),
            Options.Create(new SignalRSettings()));
}
