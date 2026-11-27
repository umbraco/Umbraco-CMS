// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Install;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Install;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class PremigrationUpgraderTests : UmbracoIntegrationTest
{
    private const string UnknownState = "{00000000-0000-0000-0000-000000000001}";

    private IKeyValueService KeyValueService => GetRequiredService<IKeyValueService>();

    private IRuntimeState RuntimeState => GetRequiredService<IRuntimeState>();

    [TestCase(MigrationBehavior.None)]
    [TestCase(MigrationBehavior.IgnoreUnknown)]
    public async Task HandleAsync_WhenMigrationBehaviorExcludesUpgrade_DoesNotRunPremigrationsBehindFinalState(MigrationBehavior migrationBehavior)
    {
        var plan = new UmbracoPremigrationPlan();
        var state = plan.Transitions[plan.InitialState]!.TargetState;
        Assert.That(state, Is.Not.EqualTo(plan.FinalState));

        await AssertPremigrationsNotRunAsync(migrationBehavior, state);
    }

    [TestCase(MigrationBehavior.IgnoreUnknown)]
    [TestCase(MigrationBehavior.RollingUpgrade)]
    public async Task HandleAsync_WhenMigrationBehaviorIgnoresUnknown_DoesNotRunPremigrationsAtUnknownState(MigrationBehavior migrationBehavior)
        => await AssertPremigrationsNotRunAsync(migrationBehavior, UnknownState);

    [TestCase(MigrationBehavior.None)]
    [TestCase(MigrationBehavior.Upgrade)]
    public async Task HandleAsync_WhenMigrationBehaviorDoesNotIgnoreUnknown_FailsBootAtUnknownState(MigrationBehavior migrationBehavior)
    {
        KeyValueService.SetValue(Constants.Conventions.Migrations.UmbracoUpgradePlanPremigrationsKey, UnknownState);

        await CreateUpgrader(migrationBehavior).HandleAsync(new RuntimePremigrationsUpgradeNotification(), CancellationToken.None);

        Assert.That(RuntimeState.Level, Is.EqualTo(RuntimeLevel.BootFailed));
    }

    private async Task AssertPremigrationsNotRunAsync(MigrationBehavior migrationBehavior, string state)
    {
        KeyValueService.SetValue(Constants.Conventions.Migrations.UmbracoUpgradePlanPremigrationsKey, state);

        var notification = new RuntimePremigrationsUpgradeNotification();
        await CreateUpgrader(migrationBehavior).HandleAsync(notification, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(notification.UpgradeResult, Is.EqualTo(RuntimePremigrationsUpgradeNotification.PremigrationUpgradeResult.NotRequired));
            Assert.That(RuntimeState.Level, Is.Not.EqualTo(RuntimeLevel.BootFailed));
            Assert.That(KeyValueService.GetValue(Constants.Conventions.Migrations.UmbracoUpgradePlanPremigrationsKey), Is.EqualTo(state));
        });
    }

    private PremigrationUpgrader CreateUpgrader(MigrationBehavior migrationBehavior)
        => new(
            GetRequiredService<IProfilingLogger>(),
            GetRequiredService<DatabaseBuilder>(),
            RuntimeState,
            GetRequiredService<IUmbracoDatabaseFactory>(),
            KeyValueService,
            Options.Create(new RuntimeSettings { MigrationBehavior = migrationBehavior }),
            GetRequiredService<ILogger<PremigrationUpgrader>>());
}
