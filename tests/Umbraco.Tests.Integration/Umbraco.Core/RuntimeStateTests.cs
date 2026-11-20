using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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

    [TestCase(null)]
    [TestCase(UnknownState)]
    public void GivenPackageMigrationsExist_WhenMigrationBehaviorIsNone_ThenLevelIsRunAndNothingIsPending(string? packageState)
    {
        SetPackageState(packageState);
        SetMigrationBehavior(MigrationBehavior.None);

        RuntimeState.DetermineRuntimeLevel();

        Assert.AreEqual(RuntimeLevel.Run, RuntimeState.Level);
        Assert.AreEqual(RuntimeLevelReason.Run, RuntimeState.Reason);
        Assert.IsEmpty(GetPendingPackageMigrations());
    }

    [Test]
    public void GivenPackageAtUnknownState_WhenMigrationBehaviorIsUpgrade_ThenLevelIsUpgrading()
    {
        SetPackageState(UnknownState);

        RuntimeState.DetermineRuntimeLevel();

        Assert.AreEqual(RuntimeLevel.Upgrading, RuntimeState.Level);
        Assert.AreEqual(RuntimeLevelReason.UpgradePackageMigrations, RuntimeState.Reason);
        CollectionAssert.AreEqual(new[] { TestMigrationPlan.TestMigrationPlanName }, GetPendingPackageMigrations());
    }

    [TestCase(true)]
    [TestCase(false)]
    public void GivenCorePlanNotAtFinalState_WhenMigrationBehaviorIsNone_ThenLevelIsRun(bool coreStateIsKnown)
    {
        SetPackageState(TestMigrationPlan.TestMigrationFinalState.ToString());
        SetCoreState(coreStateIsKnown);
        SetMigrationBehavior(MigrationBehavior.None);

        RuntimeState.DetermineRuntimeLevel();

        Assert.AreEqual(RuntimeLevel.Run, RuntimeState.Level);
        Assert.AreEqual(RuntimeLevelReason.Run, RuntimeState.Reason);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void GivenCorePlanNotAtFinalState_WhenMigrationBehaviorIsUpgrade_ThenUpgradeIsRequired(bool coreStateIsKnown)
    {
        SetPackageState(TestMigrationPlan.TestMigrationFinalState.ToString());
        SetCoreState(coreStateIsKnown);

        RuntimeState.DetermineRuntimeLevel();

        Assert.AreEqual(RuntimeLevelReason.UpgradeMigrations, RuntimeState.Reason);
    }

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
