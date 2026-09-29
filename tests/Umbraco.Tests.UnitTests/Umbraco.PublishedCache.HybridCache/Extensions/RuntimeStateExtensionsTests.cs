// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HybridCache.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.PublishedCache.HybridCache.Extensions;

[TestFixture]
public class RuntimeStateExtensionsTests
{
    /// <summary>
    /// At or below <see cref="RuntimeLevel.Install"/> the front-end cannot serve content, so seeding is
    /// always skipped regardless of the maintenance-page setting.
    /// </summary>
    [TestCase(RuntimeLevel.BootFailed, true)]
    [TestCase(RuntimeLevel.BootFailed, false)]
    [TestCase(RuntimeLevel.Unknown, true)]
    [TestCase(RuntimeLevel.Unknown, false)]
    [TestCase(RuntimeLevel.Boot, true)]
    [TestCase(RuntimeLevel.Boot, false)]
    [TestCase(RuntimeLevel.Install, true)]
    [TestCase(RuntimeLevel.Install, false)]
    public void ShouldSkipStartupSeeding_WhenLevelIsInstallOrBelow_ReturnsTrue(
        RuntimeLevel level, bool showMaintenancePage)
    {
        var state = MockState(level);
        var globalSettings = new GlobalSettings { ShowMaintenancePageWhenInUpgradeState = showMaintenancePage };
        Assert.That(state.ShouldSkipStartupSeeding(globalSettings), Is.True);
    }

    /// <summary>
    /// During <see cref="RuntimeLevel.Upgrade"/> the result follows the maintenance-page setting: shown means
    /// the front-end is blocked, so seeding is skipped; hidden means content is served, so seeding proceeds.
    /// </summary>
    [TestCase(true, true)]
    [TestCase(false, false)]
    public void ShouldSkipStartupSeeding_WhenLevelIsUpgrade_FollowsMaintenancePageSetting(
        bool showMaintenancePage, bool expected)
    {
        var state = MockState(RuntimeLevel.Upgrade);
        var globalSettings = new GlobalSettings { ShowMaintenancePageWhenInUpgradeState = showMaintenancePage };
        Assert.That(state.ShouldSkipStartupSeeding(globalSettings), Is.EqualTo(expected));
    }

    /// <summary>
    /// <see cref="RuntimeLevel.Upgrading"/> (background upgrade, server already serving) and
    /// <see cref="RuntimeLevel.Run"/> are content-serving states, so seeding proceeds even when the
    /// maintenance page is configured to show during upgrades.
    /// </summary>
    [TestCase(RuntimeLevel.Upgrading)]
    [TestCase(RuntimeLevel.Run)]
    public void ShouldSkipStartupSeeding_WhenLevelIsUpgradingOrRun_ReturnsFalse(RuntimeLevel level)
    {
        var state = MockState(level);
        var globalSettings = new GlobalSettings { ShowMaintenancePageWhenInUpgradeState = true };
        Assert.That(state.ShouldSkipStartupSeeding(globalSettings), Is.False);
    }

    private static IRuntimeState MockState(RuntimeLevel level)
        => Mock.Of<IRuntimeState>(s => s.Level == level);
}
