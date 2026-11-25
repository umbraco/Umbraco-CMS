// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Extensions;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Extensions;

[TestFixture]
public class MigrationBehaviorExtensionsTests
{
    private const string UnknownState = "{00000000-0000-0000-0000-000000000001}";

    [TestCase(MigrationBehavior.Upgrade)]
    [TestCase(MigrationBehavior.RollingUpgrade)]
    public void Upgrade_Migrates_Known_State_Behind_Final(MigrationBehavior behavior)
    {
        var plan = new UmbracoPremigrationPlan();
        var logger = new Mock<ILogger>();

        Assert.That(behavior.ShouldMigrate(plan, KnownStateBehindFinal(plan), logger.Object), Is.True);
        logger.VerifyNoOtherCalls();
    }

    [TestCase(MigrationBehavior.Upgrade)]
    [TestCase(MigrationBehavior.RollingUpgrade)]
    public void Upgrade_Migrates_Missing_State(MigrationBehavior behavior)
        => Assert.That(behavior.ShouldMigrate(new UmbracoPremigrationPlan(), null, Mock.Of<ILogger>()), Is.True);

    [TestCase(MigrationBehavior.None)]
    [TestCase(MigrationBehavior.IgnoreUnknown)]
    public void Without_Upgrade_Skips_Known_State_With_Warning(MigrationBehavior behavior)
    {
        var plan = new UmbracoPremigrationPlan();
        var logger = new Mock<ILogger>();

        Assert.That(behavior.ShouldMigrate(plan, KnownStateBehindFinal(plan), logger.Object), Is.False);
        VerifyLoggedOnce(logger, LogLevel.Warning);
    }

    [TestCase(MigrationBehavior.None)]
    [TestCase(MigrationBehavior.IgnoreUnknown)]
    public void Without_Upgrade_Skips_Missing_State_With_Warning(MigrationBehavior behavior)
    {
        var logger = new Mock<ILogger>();

        Assert.That(behavior.ShouldMigrate(new UmbracoPremigrationPlan(), null, logger.Object), Is.False);
        VerifyLoggedOnce(logger, LogLevel.Warning);
    }

    [TestCase(MigrationBehavior.None)]
    [TestCase(MigrationBehavior.Upgrade)]
    public void Without_IgnoreUnknown_Migrates_Unknown_State_So_The_Boot_Fails(MigrationBehavior behavior)
    {
        var logger = new Mock<ILogger>();

        Assert.That(behavior.ShouldMigrate(new UmbracoPremigrationPlan(), UnknownState, logger.Object), Is.True);
        logger.VerifyNoOtherCalls();
    }

    [TestCase(MigrationBehavior.IgnoreUnknown)]
    [TestCase(MigrationBehavior.RollingUpgrade)]
    public void IgnoreUnknown_Skips_Unknown_State_With_Warning(MigrationBehavior behavior)
    {
        var logger = new Mock<ILogger>();

        Assert.That(behavior.ShouldMigrate(new UmbracoPremigrationPlan(), UnknownState, logger.Object), Is.False);
        VerifyLoggedOnce(logger, LogLevel.Warning);
    }

    [TestCase(MigrationBehavior.None)]
    [TestCase(MigrationBehavior.RollingUpgrade)]
    public void Without_Logger_Decides_Without_Logging(MigrationBehavior behavior)
    {
        var plan = new UmbracoPremigrationPlan();

        Assert.Multiple(() =>
        {
            Assert.That(behavior.ShouldMigrate(plan, KnownStateBehindFinal(plan), null), Is.EqualTo(behavior.HasFlag(MigrationBehavior.Upgrade)));
            Assert.That(behavior.ShouldMigrate(plan, UnknownState, null), Is.EqualTo(behavior.HasFlag(MigrationBehavior.IgnoreUnknown) is false));
        });
    }

    [TestCase(MigrationBehavior.None)]
    [TestCase(MigrationBehavior.IgnoreUnknown)]
    public void Without_Upgrade_Skips_Plan_Ignoring_Current_State_With_Warning(MigrationBehavior behavior)
    {
        var logger = new Mock<ILogger>();

        Assert.That(behavior.ShouldMigrate(new IgnoreCurrentStatePlan(), UnknownState, logger.Object), Is.False);
        VerifyLoggedOnce(logger, LogLevel.Warning);
    }

    [TestCase(MigrationBehavior.Upgrade)]
    [TestCase(MigrationBehavior.RollingUpgrade)]
    public void Upgrade_Migrates_Plan_Ignoring_Current_State(MigrationBehavior behavior)
        => Assert.That(behavior.ShouldMigrate(new IgnoreCurrentStatePlan(), UnknownState, Mock.Of<ILogger>()), Is.True);

    private static string KnownStateBehindFinal(MigrationPlan plan)
        => plan.Transitions[plan.InitialState]!.TargetState;

    private static void VerifyLoggedOnce(Mock<ILogger> logger, LogLevel level)
        => logger.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

    private sealed class IgnoreCurrentStatePlan : MigrationPlan
    {
        public IgnoreCurrentStatePlan()
            : base("IgnoreCurrentStatePlan")
            => From(string.Empty).To("{00000000-0000-0000-0000-000000000002}");

        public override bool IgnoreCurrentState => true;
    }
}
