// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Install;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Runtime;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Install;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class UnattendedPackageMigrationTests : UmbracoIntegrationTest
{
    // Exercises the real MigrationPlanExecutor, which catches a failing migration step and returns a
    // result with Successful = false rather than throwing. This test locks in that this silently-failed
    // package migration is converted into a BootFailed state (and not left stuck on Upgrading), and that
    // the original migration exception is surfaced - the full chain the unit tests mock away.
    [Test]
    public async Task HandleAsync_WhenRealPackageMigrationThrows_SetsBootFailedAndSurfacesTheException()
    {
        var plan = new ThrowingPackageMigrationPlan();
        var planCollection = new PackageMigrationPlanCollection(() => new PackageMigrationPlan[] { plan });

        var packageMigrationRunner = new PackageMigrationRunner(
            GetRequiredService<IProfilingLogger>(),
            GetRequiredService<ICoreScopeProvider>(),
            new PendingPackageMigrations(NullLogger<PendingPackageMigrations>.Instance, planCollection),
            planCollection,
            GetRequiredService<IMigrationPlanExecutor>(), // the real executor that swallows the failure
            GetRequiredService<IKeyValueService>(),
            GetRequiredService<IEventAggregator>(),
            NullLogger<PackageMigrationRunner>.Instance);

        Exception? capturedError = null;
        var runtimeState = new Mock<IRuntimeState>();
        runtimeState.SetupGet(x => x.Level).Returns(RuntimeLevel.Upgrading);
        runtimeState.SetupGet(x => x.Reason).Returns(RuntimeLevelReason.UpgradePackageMigrations);
        runtimeState.SetupGet(x => x.StartupState).Returns(new Dictionary<string, object>
        {
            [RuntimeState.PendingPackageMigrationsStateKey] = (IReadOnlyList<string>)new[] { ThrowingPackageMigrationPlan.PlanName },
        });
        runtimeState
            .Setup(x => x.Configure(RuntimeLevel.BootFailed, It.IsAny<RuntimeLevelReason>(), It.IsAny<Exception?>()))
            .Callback<RuntimeLevel, RuntimeLevelReason, Exception?>((_, _, exception) => capturedError = exception);

        var upgrader = new UnattendedUpgrader(
            GetRequiredService<IProfilingLogger>(),
            GetRequiredService<IUmbracoVersion>(),
            GetRequiredService<DatabaseBuilder>(),
            runtimeState.Object,
            packageMigrationRunner,
            Options.Create(new UnattendedSettings()),
            GetRequiredService<DistributedCache>(),
            NullLogger<UnattendedUpgrader>.Instance);

        var notification = new RuntimeUnattendedUpgradeNotification();

        // The failure must not propagate as a thrown exception - it has to be reported as a boot failure.
        await upgrader.HandleAsync(notification, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(notification.UnattendedUpgradeResult, Is.EqualTo(RuntimeUnattendedUpgradeNotification.UpgradeResult.HasErrors));
            runtimeState.Verify(
                x => x.Configure(RuntimeLevel.BootFailed, RuntimeLevelReason.BootFailedOnException, It.IsNotNull<Exception>()),
                Times.Once);
            Assert.That(capturedError, Is.Not.Null);
            Assert.That(capturedError!.ToString(), Does.Contain(ThrowingMigration.ExceptionMessage));
        });
    }
}

internal sealed class ThrowingPackageMigrationPlan : PackageMigrationPlan
{
    public const string PlanName = "IntegrationThrowingPackage";

    public ThrowingPackageMigrationPlan()
        : base(PlanName)
    {
    }

    protected override void DefinePlan() => To<ThrowingMigration>("throw");
}

internal sealed class ThrowingMigration : AsyncMigrationBase
{
    public const string ExceptionMessage = "Integration test package migration deliberately threw.";

    public ThrowingMigration(IMigrationContext context)
        : base(context)
    {
    }

    protected override async Task MigrateAsync() => throw new InvalidOperationException(ExceptionMessage);
}
