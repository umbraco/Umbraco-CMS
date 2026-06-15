// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Security;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;
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

        Assert.That(endpoints.DataSources.Count, Is.EqualTo(0));
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

        Assert.That(endpoints.DataSources, Has.Count.EqualTo(2));
        var route = endpoints.DataSources.First();
        Assert.That(route.Endpoints, Has.Count.EqualTo(2));

        AssertMinimalBackOfficeRoutes(route);
    }

    private void AssertMinimalBackOfficeRoutes(EndpointDataSource route)
    {
        var endpoint1 = (RouteEndpoint)route.Endpoints[0];
        Assert.That(endpoint1.RoutePattern.RawText, Is.EqualTo("umbraco/{action}/{id?}"));
        Assert.That(endpoint1.RoutePattern.Defaults[ActionToken], Is.EqualTo("Index"));
        Assert.That(endpoint1.RoutePattern.Defaults[ControllerToken], Is.EqualTo(ControllerExtensions.GetControllerName<BackOfficeDefaultController>()));
    }

    private BackOfficeAreaRoutes GetBackOfficeAreaRoutes(RuntimeLevel level)
        => new BackOfficeAreaRoutes(
            Mock.Of<IRuntimeState>(x => x.Level == level),
            Options.Create(new SignalRSettings()));
}
