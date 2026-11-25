using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Packaging;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Runtime;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class RuntimeStateTests : UmbracoIntegrationTest
{
    private const string UnknownState = "{00000000-0000-0000-0000-000000000001}";

    private IRuntimeState RuntimeState => Services.GetRequiredService<IRuntimeState>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        var migrations = builder.PackageMigrationPlans();
        migrations.Clear();
        migrations.Add<TestMigrationPlan>();
    }

    [Test]
    public void GivenPackageMigrationsExist_WhenLatestStateIsRegistered_ThenLevelIsRun()
    {
        // Add the final state to the keyvalue storage
        var keyValueService = Services.GetRequiredService<IKeyValueService>();
        keyValueService.SetValue(
            Constants.Conventions.Migrations.KeyValuePrefix + TestMigrationPlan.TestMigrationPlanName,
            TestMigrationPlan.TestMigrationFinalState.ToString());

        RuntimeState.DetermineRuntimeLevel();

        Assert.AreEqual(RuntimeLevel.Run, RuntimeState.Level);
        Assert.AreEqual(RuntimeLevelReason.Run, RuntimeState.Reason);
    }

    [Test]
    public void GivenPackageMigrationsExist_WhenUnattendedMigrations_ThenLevelIsUpgrading()
    {
        RuntimeState.DetermineRuntimeLevel();

        Assert.AreEqual(RuntimeLevel.Upgrading, RuntimeState.Level);
        Assert.AreEqual(RuntimeLevelReason.UpgradePackageMigrations, RuntimeState.Reason);
    }

    [Test]
    public void GivenPackageMigrationsExist_WhenNotUnattendedMigrations_ThenLevelIsRun()
    {
        var unattendedOptions = Services.GetRequiredService<IOptions<UnattendedSettings>>();
        unattendedOptions.Value.PackageMigrationsUnattended = false;

        RuntimeState.DetermineRuntimeLevel();

        Assert.AreEqual(RuntimeLevel.Run, RuntimeState.Level);
        Assert.AreEqual(RuntimeLevelReason.Run, RuntimeState.Reason);
    }

    [TestCase(MigrationBehavior.None, null, false)]
    [TestCase(MigrationBehavior.None, UnknownState, true)]
    [TestCase(MigrationBehavior.IgnoreUnknown, null, false)]
    [TestCase(MigrationBehavior.IgnoreUnknown, UnknownState, false)]
    [TestCase(MigrationBehavior.Upgrade, null, true)]
    [TestCase(MigrationBehavior.Upgrade, UnknownState, true)]
    [TestCase(MigrationBehavior.RollingUpgrade, null, true)]
    [TestCase(MigrationBehavior.RollingUpgrade, UnknownState, false)]
    public void GivenPackageNotAtFinalState_ThenMigrationBehaviorDecidesWhetherItIsPending(MigrationBehavior migrationBehavior, string? packageState, bool isPending)
    {
        SetPackageState(packageState);
        SetMigrationBehavior(migrationBehavior);

        RuntimeState.DetermineRuntimeLevel();

        Assert.AreEqual(isPending ? RuntimeLevel.Upgrading : RuntimeLevel.Run, RuntimeState.Level);
        Assert.AreEqual(isPending ? RuntimeLevelReason.UpgradePackageMigrations : RuntimeLevelReason.Run, RuntimeState.Reason);
        CollectionAssert.AreEqual(isPending ? new[] { TestMigrationPlan.TestMigrationPlanName } : Array.Empty<string>(), GetPendingPackageMigrations());
    }

    [TestCase(MigrationBehavior.None, true, false)]
    [TestCase(MigrationBehavior.None, false, true)]
    [TestCase(MigrationBehavior.IgnoreUnknown, true, false)]
    [TestCase(MigrationBehavior.IgnoreUnknown, false, false)]
    [TestCase(MigrationBehavior.Upgrade, true, true)]
    [TestCase(MigrationBehavior.Upgrade, false, true)]
    [TestCase(MigrationBehavior.RollingUpgrade, true, true)]
    [TestCase(MigrationBehavior.RollingUpgrade, false, false)]
    public void GivenCorePlanNotAtFinalState_ThenMigrationBehaviorDecidesWhetherUpgradeIsRequired(MigrationBehavior migrationBehavior, bool coreStateIsKnown, bool upgradeIsRequired)
    {
        SetPackageState(TestMigrationPlan.TestMigrationFinalState.ToString());
        SetCoreState(coreStateIsKnown);
        SetMigrationBehavior(migrationBehavior);

        RuntimeState.DetermineRuntimeLevel();

        Assert.AreEqual(upgradeIsRequired ? RuntimeLevelReason.UpgradeMigrations : RuntimeLevelReason.Run, RuntimeState.Reason);
    }

    [Test]
    public void GivenSkippedMigration_WhenRuntimeLevelIsDeterminedRepeatedly_ThenItIsLoggedOnlyWhenTheStateChanges()
    {
        SetPackageState(UnknownState);
        var logger = new Mock<ILogger<global::Umbraco.Cms.Infrastructure.Runtime.RuntimeState>>();
        IRuntimeState runtimeState = CreateRuntimeState(MigrationBehavior.RollingUpgrade, logger.Object);

        runtimeState.DetermineRuntimeLevel();
        runtimeState.DetermineRuntimeLevel();
        VerifyWarnings(logger, Times.Once());

        SetPackageState("{00000000-0000-0000-0000-000000000002}");
        runtimeState.DetermineRuntimeLevel();
        VerifyWarnings(logger, Times.Exactly(2));
    }

    private global::Umbraco.Cms.Infrastructure.Runtime.RuntimeState CreateRuntimeState(MigrationBehavior migrationBehavior, ILogger<global::Umbraco.Cms.Infrastructure.Runtime.RuntimeState> logger)
        => new global::Umbraco.Cms.Infrastructure.Runtime.RuntimeState(
            Services.GetRequiredService<IOptions<GlobalSettings>>(),
            Services.GetRequiredService<IOptions<UnattendedSettings>>(),
            Options.Create(new RuntimeSettings { MigrationBehavior = migrationBehavior }),
            Services.GetRequiredService<IUmbracoVersion>(),
            Services.GetRequiredService<IUmbracoDatabaseFactory>(),
            logger,
            Services.GetRequiredService<PendingPackageMigrations>(),
            Services.GetRequiredService<IConflictingRouteService>(),
            Services.GetServices<IDatabaseProviderMetadata>(),
            Services.GetRequiredService<IRuntimeModeValidationService>(),
            Services.GetRequiredService<IDatabaseAvailabilityCheck>());

    private static void VerifyWarnings(Mock<ILogger<global::Umbraco.Cms.Infrastructure.Runtime.RuntimeState>> logger, Times times)
        => logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);

    private void SetMigrationBehavior(MigrationBehavior migrationBehavior)
        => Services.GetRequiredService<IOptions<RuntimeSettings>>().Value.MigrationBehavior = migrationBehavior;

    private void SetPackageState(string? state)
    {
        if (state is not null)
        {
            Services.GetRequiredService<IKeyValueService>().SetValue(
                Constants.Conventions.Migrations.KeyValuePrefix + TestMigrationPlan.TestMigrationPlanName,
                state);
        }
    }

    // The initial state is the lowest the Umbraco plan can upgrade from: known, but behind the final state.
    private void SetCoreState(bool known)
        => Services.GetRequiredService<IKeyValueService>().SetValue(
            Constants.Conventions.Migrations.UmbracoUpgradePlanKey,
            known ? new UmbracoPlan(Services.GetRequiredService<IUmbracoVersion>()).InitialState : UnknownState);

    private IReadOnlyList<string> GetPendingPackageMigrations()
        => (IReadOnlyList<string>)RuntimeState.StartupState[global::Umbraco.Cms.Infrastructure.Runtime.RuntimeState.PendingPackageMigrationsStateKey];

    private class TestMigrationPlan : PackageMigrationPlan
    {
        public const string TestMigrationPlanName = "Test";

        public TestMigrationPlan() : base(TestMigrationPlanName)
        {
        }

        public static Guid TestMigrationFinalState => new("BB02C392-4007-4A6C-A550-28BA2FF7E43D");

        protected override void DefinePlan() => To<TestMigration>(TestMigrationFinalState);
    }

    private sealed class TestMigration : AsyncPackageMigrationBase
    {
        public TestMigration(
            IPackagingService packagingService,
            IMediaService mediaService,
            MediaFileManager mediaFileManager,
            MediaUrlGeneratorCollection mediaUrlGenerators,
            IShortStringHelper shortStringHelper,
            IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
            IMigrationContext context,
            IOptions<PackageMigrationSettings> options)
            : base(packagingService, mediaService, mediaFileManager, mediaUrlGenerators, shortStringHelper, contentTypeBaseServiceProvider, context, options)
        { }

        protected override Task MigrateAsync()
        {
            ImportPackage.FromEmbeddedResource<TestMigration>().Do();

            return Task.CompletedTask;
        }
    }
}
