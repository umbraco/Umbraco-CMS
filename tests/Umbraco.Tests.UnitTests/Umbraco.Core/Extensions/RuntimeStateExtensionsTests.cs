// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

[TestFixture]
public class RuntimeStateExtensionsTests
{
    [TestCase(RuntimeLevel.Run, RuntimeLevelReason.UpgradeMigrations)]
    [TestCase(RuntimeLevel.Run, RuntimeLevelReason.UpgradePackageMigrations)]
    [TestCase(RuntimeLevel.Upgrading, RuntimeLevelReason.UpgradeMigrations)]
    [TestCase(RuntimeLevel.Upgrading, RuntimeLevelReason.UpgradePackageMigrations)]
    public void RunUnattendedBootLogic_WhenLevelAndReasonIndicateUnattendedUpgrade_ReturnsTrue(
        RuntimeLevel level, RuntimeLevelReason reason)
    {
        var state = MockState(level, reason);
        Assert.That(state.RunUnattendedBootLogic(), Is.True);
    }

    [TestCase(RuntimeLevel.Run, RuntimeLevelReason.Run)]
    [TestCase(RuntimeLevel.Upgrading, RuntimeLevelReason.Run)]
    [TestCase(RuntimeLevel.Upgrade, RuntimeLevelReason.UpgradeMigrations)]
    [TestCase(RuntimeLevel.BootFailed, RuntimeLevelReason.UpgradeMigrations)]
    public void RunUnattendedBootLogic_WhenLevelOrReasonDoNotIndicateUnattendedUpgrade_ReturnsFalse(
        RuntimeLevel level, RuntimeLevelReason reason)
    {
        var state = MockState(level, reason);
        Assert.That(state.RunUnattendedBootLogic(), Is.False);
    }

    [TestCase(RuntimeLevel.Upgrading)]
    [TestCase(RuntimeLevel.Run)]
    public void UmbracoCanBoot_WhenLevelIsAboveBootFailed_ReturnsTrue(RuntimeLevel level)
    {
        var state = MockState(level);
        Assert.That(state.UmbracoCanBoot(), Is.True);
    }

    public void UmbracoCanBoot_WhenLevelIsBootFailed_ReturnsFalse()
    {
        var state = MockState(RuntimeLevel.BootFailed);
        Assert.That(state.UmbracoCanBoot(), Is.False);
    }

    // Unknown = 0 > BootFailed = -1, so UmbracoCanBoot returns true for Unknown.
    [TestCase(RuntimeLevel.Unknown)]
    [TestCase(RuntimeLevel.Boot)]
    public void UmbracoCanBoot_WhenLevelIsAboveBootFailedButBelowRun_ReturnsTrue(RuntimeLevel level)
    {
        var state = MockState(level);
        Assert.That(state.UmbracoCanBoot(), Is.True);
    }

    [TestCase(RuntimeLevel.Upgrading)]
    [TestCase(RuntimeLevel.Run)]
    [TestCase(RuntimeLevel.Boot)]
    public void EnableInstaller_WhenLevelIsNeitherInstallNorUpgrade_ReturnsFalse(RuntimeLevel level)
    {
        var state = MockState(level);
        Assert.That(state.EnableInstaller(), Is.False);
    }

    [TestCase(RuntimeLevel.Install)]
    [TestCase(RuntimeLevel.Upgrade)]
    public void EnableInstaller_WhenLevelIsInstallOrUpgrade_ReturnsTrue(RuntimeLevel level)
    {
        var state = MockState(level);
        Assert.That(state.EnableInstaller(), Is.True);
    }

    private static IRuntimeState MockState(RuntimeLevel level, RuntimeLevelReason reason = RuntimeLevelReason.Unknown)
        => Mock.Of<IRuntimeState>(s => s.Level == level && s.Reason == reason);
}
