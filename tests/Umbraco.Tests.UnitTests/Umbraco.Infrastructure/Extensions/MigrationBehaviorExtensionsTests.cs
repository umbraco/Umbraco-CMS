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

    [TestCase(null)]
    [TestCase(UnknownState)]
    public void Upgrade_Migrates_Any_State_Behind_Final(string? state)
    {
        var logger = new Mock<ILogger>();

        var result = MigrationBehavior.Upgrade.ShouldMigrate(new UmbracoPremigrationPlan(), state, logger.Object);

        Assert.That(result, Is.True);
        logger.VerifyNoOtherCalls();
    }

    [Test]
    public void Upgrade_Migrates_Known_State_Behind_Final()
    {
        var plan = new UmbracoPremigrationPlan();

        Assert.That(MigrationBehavior.Upgrade.ShouldMigrate(plan, KnownStateBehindFinal(plan), Mock.Of<ILogger>()), Is.True);
    }

    [Test]
    public void None_Skips_Known_State_With_Warning()
    {
        var plan = new UmbracoPremigrationPlan();
        var logger = new Mock<ILogger>();

        var result = MigrationBehavior.None.ShouldMigrate(plan, KnownStateBehindFinal(plan), logger.Object);

        Assert.That(result, Is.False);
        VerifyLoggedOnce(logger, LogLevel.Warning);
    }

    [Test]
    public void None_Skips_Missing_State_With_Warning()
    {
        var logger = new Mock<ILogger>();

        var result = MigrationBehavior.None.ShouldMigrate(new UmbracoPremigrationPlan(), null, logger.Object);

        Assert.That(result, Is.False);
        VerifyLoggedOnce(logger, LogLevel.Warning);
    }

    [Test]
    public void None_Skips_Unknown_State_With_Error()
    {
        var logger = new Mock<ILogger>();

        var result = MigrationBehavior.None.ShouldMigrate(new UmbracoPremigrationPlan(), UnknownState, logger.Object);

        Assert.That(result, Is.False);
        VerifyLoggedOnce(logger, LogLevel.Error);
    }

    [Test]
    public void None_Skips_Unknown_State_With_Warning_When_Plan_Ignores_Current_State()
    {
        var logger = new Mock<ILogger>();

        var result = MigrationBehavior.None.ShouldMigrate(new IgnoreCurrentStatePlan(), UnknownState, logger.Object);

        Assert.That(result, Is.False);
        VerifyLoggedOnce(logger, LogLevel.Warning);
    }

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
