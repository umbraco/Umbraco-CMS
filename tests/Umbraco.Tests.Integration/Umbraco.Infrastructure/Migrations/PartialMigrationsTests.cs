using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Migrations.Notifications;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Migrations;

// These tests depend on the key-value table, so we need a schema to run these tests.
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
[TestFixture]
internal sealed class PartialMigrationsTests : UmbracoIntegrationTest
{
    public const string TableName = "testTable";
    public const string ColumnName = "testColumn";

    private IMigrationPlanExecutor MigrationPlanExecutor => GetRequiredService<IMigrationPlanExecutor>();

    private IKeyValueService KeyValueService => GetRequiredService<IKeyValueService>();

    [TearDown]
    public void ResetMigration()
    {
        ErrorMigration.ShouldExplode = true;
        UmbracoPlanExecutedTestNotificationHandler.HandleNotification = null;
    }

    protected override void ConfigureTestServices(IServiceCollection services)
        => services.AddNotificationHandler<UmbracoPlanExecutedNotification, UmbracoPlanExecutedTestNotificationHandler>();

    [Test]
    public async Task CanRerunPartiallyCompletedMigration()
    {
        var plan = new MigrationPlan("test")
            .From(string.Empty)
            .To<CreateTableMigration>("a")
            .To<ErrorMigration>("b")
            .To<AddColumnMigration>("c");

        var upgrader = new Upgrader(plan);

        var result = await upgrader.ExecuteAsync(MigrationPlanExecutor, ScopeProvider, KeyValueService).ConfigureAwait(false);

        Assert.Multiple(() =>
        {
            Assert.That(result.Successful, Is.False);
            Assert.That(result.InitialState, Is.EqualTo(string.Empty));
            Assert.That(result.FinalState, Is.EqualTo("a"));
            Assert.That(result.CompletedTransitions, Has.Count.EqualTo(1));
            Assert.That(result.Exception, Is.Not.Null);

            // Ensure that the partial success is saved in the keyvalue service so next plan execution starts correctly.
            using var scope = ScopeProvider.CreateScope(autoComplete: true);
            Assert.That(KeyValueService.GetValue(upgrader.StateValueKey), Is.EqualTo("a"));
            // Ensure that the changes from the first migration is persisted
            Assert.That(scope.Database.HasTable(TableName), Is.True);
            // But that the final migration wasn't run
            Assert.That(ColumnExists(TableName, ColumnName, scope), Is.False);
        });

        // Now let's simulate that someone came along and fixed the broken migration and we'll now try and rerun
        ErrorMigration.ShouldExplode = false;
        upgrader = new Upgrader(plan);
        result = await upgrader.ExecuteAsync(MigrationPlanExecutor, ScopeProvider, KeyValueService).ConfigureAwait(false);

        Assert.Multiple(() =>
        {
            Assert.That(result.InitialState, Is.EqualTo("a"));
            Assert.That(result.Successful, Is.True);
            Assert.That(result.Exception, Is.Null);
            Assert.That(result.CompletedTransitions, Has.Count.EqualTo(2));
            Assert.That(result.FinalState, Is.EqualTo("c"));

            // Ensure that everything got updated in the database.
            using var scope = ScopeProvider.CreateScope(autoComplete: true);
            Assert.That(KeyValueService.GetValue(upgrader.StateValueKey), Is.EqualTo("c"));
            Assert.That(scope.Database.HasTable(TableName), Is.True);
            Assert.That(ColumnExists(TableName, ColumnName, scope), Is.True);
        });
    }

    [Test]
    public async Task CanRunMigrationTwice()
    {
        Upgrader? upgrader = new(new SimpleMigrationPlan());
        Upgrader? upgrader2 = new(new SimpleMigrationPlan());
        var result = await upgrader.ExecuteAsync(MigrationPlanExecutor, ScopeProvider, KeyValueService).ConfigureAwait(false);
        var result2 = await upgrader2.ExecuteAsync(MigrationPlanExecutor, ScopeProvider, KeyValueService).ConfigureAwait(false);

        Assert.Multiple(() =>
        {
            Assert.That(result.Successful, Is.True);
            Assert.That(result.InitialState, Is.EqualTo("SimpleMigrationPlan_InitialState"));
            Assert.That(result.FinalState, Is.EqualTo("SimpleMigrationStep"));
            Assert.That(result.CompletedTransitions, Has.Count.EqualTo(1));
            Assert.That(result.Exception, Is.Null);
            Assert.That(result2.Successful, Is.True);
            Assert.That(result2.Exception, Is.Null);
        });
    }

    [Test]
    public async Task StateIsOnlySavedIfAMigrationSucceeds()
    {
        var plan = new MigrationPlan("test")
            .From(string.Empty)
            .To<ErrorMigration>("a")
            .To<CreateTableMigration>("b");

        var upgrader = new Upgrader(plan);
        var result = await upgrader.ExecuteAsync(MigrationPlanExecutor, ScopeProvider, KeyValueService).ConfigureAwait(false);

        Assert.Multiple(() =>
        {
            Assert.That(result.Successful, Is.False);
            Assert.That(result.Exception, Is.Not.Null);
            Assert.That(result.Exception, Is.InstanceOf<PanicException>());
            Assert.That(result.CompletedTransitions, Is.Empty);
            Assert.That(result.InitialState, Is.EqualTo(string.Empty));
            Assert.That(result.FinalState, Is.EqualTo(string.Empty));

            using var scope = ScopeProvider.CreateCoreScope();
            Assert.That(KeyValueService.GetValue(upgrader.StateValueKey), Is.Null);
        });
    }

    [Test]
    public async Task ScopesAreCreatedIfNecessary()
    {
        // The migrations have assert to ensure scopes
        var plan = new MigrationPlan("test")
            .From(string.Empty)
            .To<AsserScopeScopedTestMigration>("a")
            .To<AssertScopeUnscopedTestMigration>("b");

        var upgrader = new Upgrader(plan);
        var result = await upgrader.ExecuteAsync(MigrationPlanExecutor, ScopeProvider, KeyValueService).ConfigureAwait(false);

        Assert.That(result.Successful, Is.True);
        Assert.That(result.CompletedTransitions, Has.Count.EqualTo(2));
        Assert.That(result.FinalState, Is.EqualTo("b"));
    }

    [Test]
    public async Task PackageMigrationPlan_ResumesFromSavedState_WhenNewStepIsAdded()
    {
        // Run a package migration plan with 2 steps.
        var plan1 = new TwoStepTestPackageMigrationPlan();
        var upgrader1 = new Upgrader(plan1);
        var result1 = await upgrader1.ExecuteAsync(MigrationPlanExecutor, ScopeProvider, KeyValueService);

        Assert.Multiple(() =>
        {
            Assert.That(result1.Successful, Is.True);
            Assert.That(result1.InitialState, Is.EqualTo(string.Empty));
            Assert.That(result1.FinalState, Is.EqualTo("b"));
            Assert.That(result1.CompletedTransitions, Has.Count.EqualTo(2));
        });

        // Run an extended plan with 3 steps under the same plan name.
        // Only the new step should execute.
        var plan2 = new ThreeStepTestPackageMigrationPlan();
        var upgrader2 = new Upgrader(plan2);
        var result2 = await upgrader2.ExecuteAsync(MigrationPlanExecutor, ScopeProvider, KeyValueService);

        Assert.Multiple(() =>
        {
            Assert.That(result2.Successful, Is.True);
            Assert.That(result2.InitialState, Is.EqualTo("b"));
            Assert.That(result2.FinalState, Is.EqualTo("c"));
            Assert.That(result2.CompletedTransitions, Has.Count.EqualTo(1));
        });
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task UmbracoPlanExecutedNotificationIsAlwaysPublished(bool shouldSucceed)
    {
        var notificationPublished = false;
        ErrorMigration.ShouldExplode = shouldSucceed is false;

        UmbracoPlanExecutedTestNotificationHandler.HandleNotification += notification =>
        {
            notificationPublished = true;
            Assert.Multiple(() =>
            {
                var executedPlan = notification.ExecutedPlan;

                if (shouldSucceed)
                {
                    Assert.That(executedPlan.Successful, Is.True);
                    Assert.That(executedPlan.Exception, Is.Null);
                    Assert.That(executedPlan.FinalState, Is.EqualTo("c"));
                    Assert.That(executedPlan.CompletedTransitions, Has.Count.EqualTo(3));
                }
                else
                {
                    Assert.That(executedPlan.Successful, Is.False);
                    Assert.That(executedPlan.Exception, Is.Not.Null);
                    Assert.That(executedPlan.Exception, Is.InstanceOf<PanicException>());
                    Assert.That(executedPlan.FinalState, Is.EqualTo("a"));
                    Assert.That(executedPlan.CompletedTransitions, Has.Count.EqualTo(1));
                }
            });
        };

        // We have to use the DatabaseBuilder otherwise the notification isn't published
        var databaseBuilder = GetRequiredService<DatabaseBuilder>();
        var plan = new TestUmbracoPlan(null!);
        await databaseBuilder.UpgradeSchemaAndDataAsync(plan).ConfigureAwait(false);

        Assert.That(notificationPublished, Is.True);
    }

    private class TwoStepTestPackageMigrationPlan : PackageMigrationPlan
    {
        public const string TestPlanName = "TestPackagePlan";

        public TwoStepTestPackageMigrationPlan()
            : base(TestPlanName)
        {
        }

        protected override void DefinePlan()
        {
            To<NoOpMigration>("a");
            To<NoOpMigration>("b");
        }
    }

    private class ThreeStepTestPackageMigrationPlan : PackageMigrationPlan
    {
        public ThreeStepTestPackageMigrationPlan()
            : base(TwoStepTestPackageMigrationPlan.TestPlanName)
        {
        }

        protected override void DefinePlan()
        {
            To<NoOpMigration>("a");
            To<NoOpMigration>("b");
            To<NoOpMigration>("c");
        }
    }

    private class NoOpMigration : AsyncMigrationBase
    {
        public NoOpMigration(IMigrationContext context) : base(context)
        {
        }

        protected override Task MigrateAsync()
        {
            return Task.CompletedTask;
        }
    }

    private bool ColumnExists(string tableName, string columnName, IScope scope) =>
        scope.Database.SqlContext.SqlSyntax.GetColumnsInSchema(scope.Database)
            .Any(x => x.TableName.Equals(tableName) && x.ColumnName.Equals(columnName));
}


// This is just some basic migrations to test the migration plans...
internal class ErrorMigration : AsyncMigrationBase
{
    // Used to determine if an exception should be thrown, used to test re-running migrations
    public static bool ShouldExplode { get; set; } = true;

    public ErrorMigration(IMigrationContext context) : base(context)
    {
    }

    protected override Task MigrateAsync()
    {
        if (ShouldExplode)
        {
            throw new PanicException();
        }

        return Task.CompletedTask;
    }
}

internal class CreateTableMigration : AsyncMigrationBase
{
    public CreateTableMigration(IMigrationContext context) : base(context)
    {
    }

    protected override Task MigrateAsync()
    {
        Create.Table<TestDto>().Do();
        return Task.CompletedTask;
    }
}

internal class AddColumnMigration : AsyncMigrationBase
{
    public AddColumnMigration(IMigrationContext context) : base(context)
    {
    }

    protected override async Task MigrateAsync() => Create
        .Column(PartialMigrationsTests.ColumnName)
        .OnTable(PartialMigrationsTests.TableName)
        .AsString()
        .Do();
}

internal class AssertScopeUnscopedTestMigration : UnscopedAsyncMigrationBase
{
    private readonly IScopeProvider _scopeProvider;
    private readonly IScopeAccessor _scopeAccessor;

    public AssertScopeUnscopedTestMigration(
        IMigrationContext context,
        IScopeProvider scopeProvider,
        IScopeAccessor scopeAccessor) : base(context)
    {
        _scopeProvider = scopeProvider;
        _scopeAccessor = scopeAccessor;
    }

    protected override Task MigrateAsync()
    {
        // Since this is a scopeless migration both ambient scope and the parent scope should be null
        Assert.That(_scopeAccessor.AmbientScope, Is.Null);

        using var scope = _scopeProvider.CreateScope();
        Assert.That(((Scope)scope).ParentScope, Is.Null);

        Context.Complete();

        return Task.CompletedTask;
    }
}

internal class AsserScopeScopedTestMigration : AsyncMigrationBase
{
    private readonly IScopeProvider _scopeProvider;
    private readonly IScopeAccessor _scopeAccessor;

    public AsserScopeScopedTestMigration(
        IMigrationContext context,
        IScopeProvider scopeProvider,
        IScopeAccessor scopeAccessor) : base(context)
    {
        _scopeProvider = scopeProvider;
        _scopeAccessor = scopeAccessor;
    }

    protected override Task MigrateAsync()
    {
        Assert.That(_scopeAccessor.AmbientScope, Is.Not.Null);

        using var scope = _scopeProvider.CreateScope();

        Assert.That(((Scope)scope).ParentScope, Is.Not.Null);

        return Task.CompletedTask;
    }
}

[TableName(PartialMigrationsTests.TableName)]
[PrimaryKey("id", AutoIncrement = true)]
internal class TestDto
{
    [Column("id")]
    [PrimaryKeyColumn(Name = "PK_testTable")]
    public int Id { get; set; }
}

internal class UmbracoPlanExecutedTestNotificationHandler : INotificationHandler<UmbracoPlanExecutedNotification>
{
    public static Action<UmbracoPlanExecutedNotification>? HandleNotification { get; set; }

    public void Handle(UmbracoPlanExecutedNotification notification) => HandleNotification?.Invoke(notification);
}


/// <summary>
/// This is a fake UmbracoPlan used for testing of the DatabaseBuilder, this overrides everything to be of type
/// UmbracoPlan but behave like a normal migration plan.
/// </summary>
internal class TestUmbracoPlan : UmbracoPlan
{
    public TestUmbracoPlan(IUmbracoVersion umbracoVersion) : base(umbracoVersion)
    {
    }

    public override string InitialState => string.Empty;

    public override bool IgnoreCurrentState => true;

    protected override void DefinePlan()
    {
        From(InitialState);
        To<CreateTableMigration>("a");
        To<ErrorMigration>("b");
        To<AddColumnMigration>("c");
    }
}


internal class SimpleMigrationPlan : MigrationPlan
{
    public SimpleMigrationPlan()
        : base("SimpleMigrationPlan") => DefinePlan();

    public override string InitialState => "SimpleMigrationPlan_InitialState";

    private void DefinePlan()
    {
        MigrationPlan plan = From(InitialState)
            .To<SimpleMigrationStep>(nameof(SimpleMigrationStep));
    }
}

internal class SimpleMigrationStep : AsyncMigrationBase
{
    private readonly ILogger<SimpleMigrationStep> _logger;

    public SimpleMigrationStep(
        IMigrationContext context,
        ILogger<SimpleMigrationStep> logger)
        : base(context) => _logger = logger;

    protected override Task MigrateAsync()
    {
        _logger.LogDebug("Here be migration");
        return Task.CompletedTask;
    }
}
