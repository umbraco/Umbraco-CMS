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
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.ActionsResults;
using Umbraco.Cms.Web.Common.Controllers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Controllers;

[TestFixture]
public class MaintenanceModeActionFilterAttributeTests
{
    private const string CustomUpgradingViewPath = "~/Views/CustomUpgrading.cshtml";

    [Test]
    public void OnActionExecuting_WhenUpgradingAndMaintenanceEnabled_SetsMaintenanceResultWithUpgradingViewPath()
    {
        var settings = new GlobalSettings
        {
            ShowMaintenancePageWhenInUpgradeState = true,
            UpgradingViewPath = CustomUpgradingViewPath,
        };
        var context = CreateContext();

        InvokeFilter(RuntimeLevel.Upgrading, settings, context);

        Assert.That(context.Result, Is.InstanceOf<MaintenanceResult>());
    }

    [Test]
    public void OnActionExecuting_WhenUpgradingAndMaintenanceDisabled_DoesNotSetResult()
    {
        var settings = new GlobalSettings { ShowMaintenancePageWhenInUpgradeState = false };
        var context = CreateContext();

        InvokeFilter(RuntimeLevel.Upgrading, settings, context);

        Assert.That(context.Result, Is.Null);
    }

    [Test]
    public void OnActionExecuting_WhenUpgradeAndMaintenanceEnabled_SetsMaintenanceResult()
    {
        var settings = new GlobalSettings { ShowMaintenancePageWhenInUpgradeState = true };
        var context = CreateContext();

        InvokeFilter(RuntimeLevel.Upgrade, settings, context);

        Assert.That(context.Result, Is.InstanceOf<MaintenanceResult>());
    }

    [Test]
    public void OnActionExecuting_WhenUpgradeAndMaintenanceDisabled_DoesNotSetResult()
    {
        var settings = new GlobalSettings { ShowMaintenancePageWhenInUpgradeState = false };
        var context = CreateContext();

        InvokeFilter(RuntimeLevel.Upgrade, settings, context);

        Assert.That(context.Result, Is.Null);
    }

    [TestCase(RuntimeLevel.Run)]
    [TestCase(RuntimeLevel.Install)]
    [TestCase(RuntimeLevel.Boot)]
    [TestCase(RuntimeLevel.UpgradeFailed)]
    public void OnActionExecuting_WhenLevelIsNotUpgradeOrUpgrading_DoesNotSetResult(RuntimeLevel level)
    {
        var settings = new GlobalSettings { ShowMaintenancePageWhenInUpgradeState = true };
        var context = CreateContext();

        InvokeFilter(level, settings, context);

        Assert.That(context.Result, Is.Null);
    }

    private static void InvokeFilter(RuntimeLevel level, GlobalSettings settings, ActionExecutingContext context)
    {
        var attribute = new MaintenanceModeActionFilterAttribute();
        var services = new ServiceCollection()
            .AddSingleton(Mock.Of<IRuntimeState>(s => s.Level == level))
            .AddSingleton(Mock.Of<IOptionsMonitor<GlobalSettings>>(m => m.CurrentValue == settings))
            .BuildServiceProvider();

        var filter = (IActionFilter)attribute.CreateInstance(services);
        filter.OnActionExecuting(context);
    }

    private static ActionExecutingContext CreateContext()
    {
        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor());

        return new ActionExecutingContext(
            actionContext,
            [],
            new Dictionary<string, object?>(),
            controller: null!);
    }
}
