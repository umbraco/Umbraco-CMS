// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Website.Routing;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Website.Routing;

[TestFixture]
public class EagerMatcherPolicyTests
{
    [Test]
    public async Task ApplyAsync_WhenRunButNotReadyToServe_ReRoutesToMaintenanceRenderEndpoint()
    {
        // The runtime has flipped to Run after an unattended upgrade, but post-migration initialization
        // is not finished (not ready to serve). The front-end request must be re-routed to the render
        // endpoint so the maintenance page is shown, instead of routing against not-yet-initialized services.
        Endpoint renderEndpoint = CreateRenderEndpoint();
        EagerMatcherPolicy sut = CreateSut(RuntimeLevel.Run, isReadyToServe: false, renderEndpoint);
        CandidateSet candidates = CreateDynamicCandidateSet(out _);

        await sut.ApplyAsync(new DefaultHttpContext(), candidates);

        Assert.That(candidates[0].Endpoint, Is.SameAs(renderEndpoint));
    }

    [Test]
    public async Task ApplyAsync_WhenRunAndReadyToServe_DoesNotReRoute()
    {
        // Normal running state: the dynamic content route must be left intact (no false-positive gating).
        Endpoint renderEndpoint = CreateRenderEndpoint();
        EagerMatcherPolicy sut = CreateSut(RuntimeLevel.Run, isReadyToServe: true, renderEndpoint);
        CandidateSet candidates = CreateDynamicCandidateSet(out Endpoint dynamicEndpoint);

        await sut.ApplyAsync(new DefaultHttpContext(), candidates);

        Assert.That(candidates[0].Endpoint, Is.SameAs(dynamicEndpoint));
    }

    private static EagerMatcherPolicy CreateSut(RuntimeLevel level, bool isReadyToServe, Endpoint renderEndpoint)
    {
        var settings = new GlobalSettings { ShowMaintenancePageWhenInUpgradeState = true };
        return new EagerMatcherPolicy(
            Mock.Of<IRuntimeState>(s => s.Level == level),
            new DefaultEndpointDataSource(renderEndpoint),
            umbracoRequestPaths: null!, // only used on the install branch, which these tests don't exercise
            Mock.Of<IOptionsMonitor<GlobalSettings>>(m => m.CurrentValue == settings),
            umbracoContextAccessor: null!,
            publishedRouter: null!,
            Mock.Of<IRuntimeStartupReadiness>(r => r.IsReadyToServe == isReadyToServe));
    }

    private static Endpoint CreateRenderEndpoint()
    {
        var descriptor = new ControllerActionDescriptor
        {
            ControllerTypeInfo = typeof(RenderController).GetTypeInfo(),
            ActionName = nameof(RenderController.Index),
        };
        return new RouteEndpoint(
            _ => Task.CompletedTask,
            RoutePatternFactory.Parse("/"),
            order: 0,
            new EndpointMetadataCollection(descriptor),
            "render");
    }

    private static CandidateSet CreateDynamicCandidateSet(out Endpoint dynamicEndpoint)
    {
        dynamicEndpoint = new RouteEndpoint(
            _ => Task.CompletedTask,
            RoutePatternFactory.Parse("{**slug}"),
            order: 0,
            new EndpointMetadataCollection(new FakeDynamicEndpointMetadata()),
            "dynamic");

        return new CandidateSet(
            new[] { dynamicEndpoint },
            new[] { new RouteValueDictionary() },
            new[] { 0 });
    }

    private sealed class FakeDynamicEndpointMetadata : IDynamicEndpointMetadata
    {
        public bool IsDynamic => true;
    }
}
