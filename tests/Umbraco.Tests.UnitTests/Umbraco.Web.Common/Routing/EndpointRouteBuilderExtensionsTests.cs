// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Extensions;
using static Umbraco.Cms.Core.Constants.Web.Routing;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Routing;

[TestFixture]
public class EndpointRouteBuilderExtensionsTests
{
    [TestCase("umbraco", Constants.Web.Mvc.BackOfficeApiArea, "test", null, true)]
    [TestCase("umbraco", Constants.Web.Mvc.BackOfficeApiArea, "test", "GetStuff", true)]
    [TestCase("umbraco", Constants.Web.Mvc.BackOfficeApiArea, null, null, true)]
    [TestCase("umbraco", Constants.Web.Mvc.BackOfficeApiArea, null, "GetStuff", true)]
    [TestCase("umbraco", Constants.Web.Mvc.BackOfficeApiArea, "test", null, false)]
    [TestCase("umbraco", Constants.Web.Mvc.BackOfficeApiArea, "test", "GetStuff", false)]
    [TestCase("umbraco", Constants.Web.Mvc.BackOfficeApiArea, null, null, false)]
    [TestCase("umbraco", Constants.Web.Mvc.BackOfficeApiArea, null, "GetStuff", false)]
    [TestCase("umbraco", null, "test", null, true)]
    [TestCase("umbraco", null, "test", "GetStuff", true)]
    [TestCase("umbraco", null, null, null, true)]
    [TestCase("umbraco", null, null, "GetStuff", true)]
    [TestCase("umbraco", null, "test", null, false)]
    [TestCase("umbraco", null, "test", "GetStuff", false)]
    [TestCase("umbraco", null, null, null, false)]
    [TestCase("umbraco", null, null, "GetStuff", false)]
    public void MapUmbracoRoute(string umbracoPath, string area, string prefix, string defaultAction, bool includeControllerName)
    {
        var endpoints = new TestRouteBuilder();
        endpoints.MapUmbracoRoute<Testing1Controller>(umbracoPath, area, prefix, defaultAction, includeControllerName);

        var route = endpoints.DataSources.First();
        var endpoint = (RouteEndpoint)route.Endpoints[0];

        var controllerName = ControllerExtensions.GetControllerName<Testing1Controller>();
        var controllerNamePattern = controllerName.ToLowerInvariant();

        if (includeControllerName)
        {
            if (prefix.IsNullOrWhiteSpace())
            {
                Assert.That(
                    endpoint.RoutePattern.RawText, Is.EqualTo($"{umbracoPath}/{controllerNamePattern}/{{action}}/{{id?}}"));
            }
            else
            {
                Assert.That(
                    endpoint.RoutePattern.RawText, Is.EqualTo($"{umbracoPath}/{prefix}/{controllerNamePattern}/{{action}}/{{id?}}"));
            }
        }
        else
        {
            if (prefix.IsNullOrWhiteSpace())
            {
                Assert.That(endpoint.RoutePattern.RawText, Is.EqualTo($"{umbracoPath}/{{action}}/{{id?}}"));
            }
            else
            {
                Assert.That(endpoint.RoutePattern.RawText, Is.EqualTo($"{umbracoPath}/{prefix}/{{action}}/{{id?}}"));
            }
        }

        if (!area.IsNullOrWhiteSpace())
        {
            Assert.That(endpoint.RoutePattern.Defaults[AreaToken], Is.EqualTo(area));
        }

        if (!defaultAction.IsNullOrWhiteSpace())
        {
            Assert.That(endpoint.RoutePattern.Defaults["action"], Is.EqualTo(defaultAction));
        }

        Assert.That(endpoint.RoutePattern.Defaults[ControllerToken], Is.EqualTo(controllerName));
    }

    private class Testing1Controller : ControllerBase
    {
    }
}
