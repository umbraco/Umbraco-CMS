// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.ActionsResults;
using Umbraco.Cms.Web.Common.Controllers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Controllers;

[TestFixture]
public class MaintenanceModeActionFilterAttributeTests
{
    private const string CustomUpgradingViewPath = "~/Views/CustomUpgrading.cshtml";

    [Test]
    public void OnActionExecuting_MvcController_WhenUpgradingAndMaintenanceEnabled_SetsMaintenanceResultWithUpgradingViewPath()
    {
        var settings = new GlobalSettings
        {
            ShowMaintenancePageWhenInUpgradeState = true,
            UpgradingViewPath = CustomUpgradingViewPath,
        };
        var context = CreateMvcControllerContext();

        InvokeFilter(RuntimeLevel.Upgrading, settings, context);

        Assert.That(context.Result, Is.InstanceOf<MaintenanceResult>());
    }

    [Test]
    public void OnActionExecuting_MvcController_WhenUpgradingAndMaintenanceDisabled_DoesNotSetResult()
    {
        var settings = new GlobalSettings { ShowMaintenancePageWhenInUpgradeState = false };
        var context = CreateMvcControllerContext();

        InvokeFilter(RuntimeLevel.Upgrading, settings, context);

        Assert.That(context.Result, Is.Null);
    }

    [Test]
    public void OnActionExecuting_MvcController_WhenUpgradeAndMaintenanceEnabled_SetsMaintenanceResult()
    {
        var settings = new GlobalSettings { ShowMaintenancePageWhenInUpgradeState = true };
        var context = CreateMvcControllerContext();

        InvokeFilter(RuntimeLevel.Upgrade, settings, context);

        Assert.That(context.Result, Is.InstanceOf<MaintenanceResult>());
    }

    [Test]
    public void OnActionExecuting_MvcController_WhenUpgradeAndMaintenanceDisabled_DoesNotSetResult()
    {
        var settings = new GlobalSettings { ShowMaintenancePageWhenInUpgradeState = false };
        var context = CreateMvcControllerContext();

        InvokeFilter(RuntimeLevel.Upgrade, settings, context);

        Assert.That(context.Result, Is.Null);
    }

    [TestCase(RuntimeLevel.Run)]
    [TestCase(RuntimeLevel.Install)]
    [TestCase(RuntimeLevel.Boot)]
    public void OnActionExecuting_MvcController_WhenLevelIsNotUpgradeOrUpgrading_DoesNotSetResult(RuntimeLevel level)
    {
        var settings = new GlobalSettings { ShowMaintenancePageWhenInUpgradeState = true };
        var context = CreateMvcControllerContext();

        InvokeFilter(level, settings, context);

        Assert.That(context.Result, Is.Null);
    }

    [Test]
    public void OnActionExecuting_ApiController_WhenUpgradingAndMaintenanceEnabled_SetsProblemDetailsResult()
    {
        var settings = new GlobalSettings { ShowMaintenancePageWhenInUpgradeState = true };
        var context = CreateApiControllerContext();

        InvokeFilter(RuntimeLevel.Upgrading, settings, context);

        Assert.Multiple(() =>
        {
            Assert.That(context.Result, Is.InstanceOf<ObjectResult>());
            var result = (ObjectResult)context.Result!;
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status503ServiceUnavailable));
            Assert.That(result.Value, Is.InstanceOf<ProblemDetails>());
            Assert.That(((ProblemDetails)result.Value!).Status, Is.EqualTo(StatusCodes.Status503ServiceUnavailable));
        });
    }

    [Test]
    public void OnActionExecuting_ApiController_WhenUpgradeAndMaintenanceEnabled_DoesNotSetResult()
    {
        // During an attended upgrade (Upgrade), API controllers are NOT blocked — the operator
        // needs API access to log in and trigger the upgrade from the backoffice.
        var settings = new GlobalSettings { ShowMaintenancePageWhenInUpgradeState = true };
        var context = CreateApiControllerContext();

        InvokeFilter(RuntimeLevel.Upgrade, settings, context);

        Assert.That(context.Result, Is.Null);
    }

    [Test]
    public void OnActionExecuting_ApiController_WhenUpgradingAndMaintenanceDisabled_DoesNotSetResult()
    {
        var settings = new GlobalSettings { ShowMaintenancePageWhenInUpgradeState = false };
        var context = CreateApiControllerContext();

        InvokeFilter(RuntimeLevel.Upgrading, settings, context);

        Assert.That(context.Result, Is.Null);
    }

    [TestCase(RuntimeLevel.Run)]
    [TestCase(RuntimeLevel.Install)]
    [TestCase(RuntimeLevel.Boot)]
    [TestCase(RuntimeLevel.Upgrade)]
    public void OnActionExecuting_ApiController_WhenLevelIsNotUpgrading_DoesNotSetResult(RuntimeLevel level)
    {
        var settings = new GlobalSettings { ShowMaintenancePageWhenInUpgradeState = true };
        var context = CreateApiControllerContext();

        InvokeFilter(level, settings, context);

        Assert.That(context.Result, Is.Null);
    }

    [Test]
    public void OnActionExecuting_WhenSkipAttributePresent_DoesNotSetResult()
    {
        // SkipMaintenanceModeFilterAttribute bypasses blocking even during Upgrading.
        var settings = new GlobalSettings { ShowMaintenancePageWhenInUpgradeState = true };
        var context = CreateContext(endpointMetadata: [new ApiControllerAttribute(), new SkipMaintenanceModeFilterAttribute()]);

        InvokeFilter(RuntimeLevel.Upgrading, settings, context);

        Assert.That(context.Result, Is.Null);
    }

    [Test]
    public void OnActionExecuting_MvcController_WhenRunButNotReadyAndMaintenanceEnabled_SetsMaintenanceResult()
    {
        // After a background unattended upgrade the runtime is at Run before per-server caches are seeded.
        // The front-end must be blocked in that window so no request poisons a not-yet-seeded cache (#22581).
        var settings = new GlobalSettings { ShowMaintenancePageWhenInUpgradeState = true };
        var context = CreateMvcControllerContext();

        InvokeFilter(RuntimeLevel.Run, settings, context, isReady: false);

        Assert.That(context.Result, Is.InstanceOf<MaintenanceResult>());
    }

    [Test]
    public void OnActionExecuting_MvcController_WhenRunButNotReadyAndMaintenanceDisabled_DoesNotSetResult()
    {
        var settings = new GlobalSettings { ShowMaintenancePageWhenInUpgradeState = false };
        var context = CreateMvcControllerContext();

        InvokeFilter(RuntimeLevel.Run, settings, context, isReady: false);

        Assert.That(context.Result, Is.Null);
    }

    [Test]
    public void OnActionExecuting_MvcController_WhenRunAndReady_DoesNotSetResult()
    {
        var settings = new GlobalSettings { ShowMaintenancePageWhenInUpgradeState = true };
        var context = CreateMvcControllerContext();

        InvokeFilter(RuntimeLevel.Run, settings, context, isReady: true);

        Assert.That(context.Result, Is.Null);
    }

    [Test]
    public void OnActionExecuting_ApiController_WhenRunButNotReadyAndMaintenanceEnabled_SetsProblemDetailsResult()
    {
        var settings = new GlobalSettings { ShowMaintenancePageWhenInUpgradeState = true };
        var context = CreateApiControllerContext();

        InvokeFilter(RuntimeLevel.Run, settings, context, isReady: false);

        Assert.Multiple(() =>
        {
            Assert.That(context.Result, Is.InstanceOf<ObjectResult>());
            var result = (ObjectResult)context.Result!;
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status503ServiceUnavailable));
        });
    }

    private static void InvokeFilter(RuntimeLevel level, GlobalSettings settings, ActionExecutingContext context, bool isReady = true)
    {
        var attribute = new MaintenanceModeActionFilterAttribute();
        var services = new ServiceCollection()
            .AddSingleton(Mock.Of<IRuntimeState>(s => s.Level == level))
            .AddSingleton(Mock.Of<IOptionsMonitor<GlobalSettings>>(m => m.CurrentValue == settings))
            .AddSingleton(Mock.Of<IContentRoutingReadiness>(r => r.IsReady == isReady))
            .BuildServiceProvider();

        var filter = (IActionFilter)attribute.CreateInstance(services);
        filter.OnActionExecuting(context);
    }

    private static ActionExecutingContext CreateMvcControllerContext()
        => CreateContext(endpointMetadata: []);

    private static ActionExecutingContext CreateApiControllerContext()
        => CreateContext(endpointMetadata: [new ApiControllerAttribute()]);

    private static ActionExecutingContext CreateContext(IList<object> endpointMetadata)
    {
        var actionDescriptor = new ActionDescriptor
        {
            EndpointMetadata = new List<object>(endpointMetadata),
        };

        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            actionDescriptor);

        return new ActionExecutingContext(
            actionContext,
            [],
            new Dictionary<string, object?>(),
            controller: null!);
    }
}
