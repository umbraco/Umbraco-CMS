// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

/// <summary>
/// Contains unit tests for the <see cref="RuntimeStateExtensions"/> class, which provides extension methods related to runtime state.
/// </summary>
[TestFixture]
public class RuntimeStateExtensionsTests
{
    /// <summary>
    /// Verifies that <see cref="RuntimeStateExtensions.RunUnattendedBootLogic"/> returns <c>true</c> when the specified
    /// <paramref name="level"/> and <paramref name="reason"/> indicate an unattended upgrade scenario.
    /// This is tested for multiple combinations of <see cref="RuntimeLevel"/> and <see cref="RuntimeLevelReason"/> using test cases.
    /// </summary>
    /// <param name="level">The runtime level to test.</param>
    /// <param name="reason">The runtime level reason to test.</param>
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

    /// <summary>
    /// Verifies that <see cref="RuntimeStateExtensions.RunUnattendedBootLogic"/> returns <c>false</c>
    /// when the provided <paramref name="level"/> and <paramref name="reason"/> do not indicate an unattended upgrade scenario.
    /// </summary>
    /// <param name="level">The <see cref="RuntimeLevel"/> value to test.</param>
    /// <param name="reason">The <see cref="RuntimeLevelReason"/> value to test.</param>
    /// <remarks>
    /// This test ensures that unattended boot logic is not triggered for unrelated runtime levels or reasons.
    /// </remarks>
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

    /// <summary>
    /// Verifies that <see cref="UmbracoCanBoot"/> returns <c>true</c> when the specified <paramref name="level"/> is above <see cref="RuntimeLevel.BootFailed"/>.
    /// </summary>
    /// <param name="level">The <see cref="RuntimeLevel"/> value to test.</param>
    [TestCase(RuntimeLevel.Upgrading)]
    [TestCase(RuntimeLevel.Run)]
    public void UmbracoCanBoot_WhenLevelIsAboveBootFailed_ReturnsTrue(RuntimeLevel level)
    {
        var state = MockState(level);
        Assert.That(state.UmbracoCanBoot(), Is.True);
    }

    /// <summary>
    /// Tests that UmbracoCanBoot returns false when the runtime level is BootFailed.
    /// </summary>
    public void UmbracoCanBoot_WhenLevelIsBootFailed_ReturnsFalse()
    {
        var state = MockState(RuntimeLevel.BootFailed);
        Assert.That(state.UmbracoCanBoot(), Is.False);
    }

    // Unknown = 0 > BootFailed = -1, so UmbracoCanBoot returns true for Unknown.
    /// <summary>
    /// Verifies that <see cref="UmbracoCanBoot"/> returns <c>true</c> for runtime levels that are greater than <see cref="RuntimeLevel.BootFailed"/> but less than <see cref="RuntimeLevel.Run"/>.
    /// This ensures that Umbraco can boot in intermediate states such as <see cref="RuntimeLevel.Unknown"/> and <see cref="RuntimeLevel.Boot"/>.
    /// </summary>
    /// <param name="level">The <see cref="RuntimeLevel"/> value to test.</param>
    [TestCase(RuntimeLevel.Unknown)]
    [TestCase(RuntimeLevel.Boot)]
    public void UmbracoCanBoot_WhenLevelIsAboveBootFailedButBelowRun_ReturnsTrue(RuntimeLevel level)
    {
        var state = MockState(level);
        Assert.That(state.UmbracoCanBoot(), Is.True);
    }

    /// <summary>
    /// Verifies that <see cref="RuntimeStateExtensions.EnableInstaller"/> returns <c>false</c>
    /// when the <paramref name="level"/> is neither <c>Install</c> nor <c>Upgrade</c>.
    /// This test is executed for the <c>Upgrading</c>, <c>Run</c>, and <c>Boot</c> runtime levels.
    /// </summary>
    /// <param name="level">The <see cref="RuntimeLevel"/> value to test.</param>
    [TestCase(RuntimeLevel.Upgrading)]
    [TestCase(RuntimeLevel.Run)]
    [TestCase(RuntimeLevel.Boot)]
    public void EnableInstaller_WhenLevelIsNeitherInstallNorUpgrade_ReturnsFalse(RuntimeLevel level)
    {
        var state = MockState(level);
        Assert.That(state.EnableInstaller(), Is.False);
    }

    /// <summary>
    /// Tests that EnableInstaller returns true when the runtime level is Install or Upgrade.
    /// </summary>
    /// <param name="level">The runtime level to test.</param>
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
