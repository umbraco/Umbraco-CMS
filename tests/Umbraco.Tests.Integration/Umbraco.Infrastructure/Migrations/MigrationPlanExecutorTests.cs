// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Migrations;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewEmptyPerTest)]
internal sealed class MigrationPlanExecutorTests : UmbracoIntegrationTest
{
    private CountingDatabaseCacheRebuilder _databaseCacheRebuilder;

    [SetUp]
    public void CreateDatabaseCacheRebuilder() => _databaseCacheRebuilder = new CountingDatabaseCacheRebuilder();

    [Test]
    public async Task Can_Rebuild_Cache_For_Plan_Requiring_A_Rebuild()
    {
        MigrationPlanExecutor executor = CreateExecutor();

        await ExecutePlanAsync<RebuildCacheMigration>(executor, "first");

        Assert.That(_databaseCacheRebuilder.RebuildCount, Is.EqualTo(1));
    }

    [Test]
    public async Task Cannot_Rebuild_Cache_For_Plan_Not_Requiring_A_Rebuild()
    {
        MigrationPlanExecutor executor = CreateExecutor();

        await ExecutePlanAsync<NoopMigration>(executor, "first");

        Assert.That(_databaseCacheRebuilder.RebuildCount, Is.EqualTo(0));
    }

    // The executor is registered as a singleton and executes the Umbraco plan followed by one plan per package with
    // pending migrations, so a rebuild requested by one plan must not leak into the plans that follow it
    // (https://github.com/umbraco/Umbraco-CMS/discussions/23531).
    [Test]
    public async Task Cannot_Rebuild_Cache_For_Subsequent_Plan_Not_Requiring_A_Rebuild()
    {
        MigrationPlanExecutor executor = CreateExecutor();

        await ExecutePlanAsync<RebuildCacheMigration>(executor, "first");
        await ExecutePlanAsync<NoopMigration>(executor, "second");
        await ExecutePlanAsync<NoopMigration>(executor, "third");

        Assert.That(_databaseCacheRebuilder.RebuildCount, Is.EqualTo(1));
    }

    [Test]
    public async Task Can_Rebuild_Cache_Once_Per_Plan_Requiring_A_Rebuild()
    {
        MigrationPlanExecutor executor = CreateExecutor();

        await ExecutePlanAsync<RebuildCacheMigration>(executor, "first");
        await ExecutePlanAsync<RebuildCacheMigration>(executor, "second");

        Assert.That(_databaseCacheRebuilder.RebuildCount, Is.EqualTo(2));
    }

    // A rebuild depends on infrastructure that migrations part way through the plan put in place, so a plan that
    // stopped before reaching it should not attempt one (it will fail, and mask the migration failure, #23612).
    [Test]
    public async Task Cannot_Rebuild_Cache_For_Plan_That_Did_Not_Complete()
    {
        MigrationPlanExecutor executor = CreateExecutor();

        MigrationPlan plan = new MigrationPlan("failing")
            .From(string.Empty)
            .To<RebuildCacheMigration>("rebuild-requested")
            .To<FailingMigration>("done");

        ExecutedMigrationPlan result = await executor.ExecutePlanAsync(plan, string.Empty);

        Assert.Multiple(() =>
        {
            Assert.That(result.Successful, Is.False);
            Assert.That(_databaseCacheRebuilder.RebuildCount, Is.EqualTo(0));
        });
    }

    private static async Task ExecutePlanAsync<TMigration>(MigrationPlanExecutor executor, string name)
        where TMigration : AsyncMigrationBase
    {
        MigrationPlan plan = new MigrationPlan(name)
            .From(string.Empty)
            .To<TMigration>("done");

        ExecutedMigrationPlan result = await executor.ExecutePlanAsync(plan, string.Empty);

        // Asserted so a plan that errors can't leave the rebuild count coincidentally matching the expectation.
        Assert.That(result.Successful, Is.True, result.Exception?.ToString());
    }

    private MigrationPlanExecutor CreateExecutor() => new(
        GetRequiredService<ICoreScopeProvider>(),
        ScopeAccessor,
        LoggerFactory,
        GetRequiredService<IMigrationBuilder>(),
        GetRequiredService<IUmbracoDatabaseFactory>(),
        _databaseCacheRebuilder,
        GetRequiredService<DistributedCache>(),

        // Not the real IKeyValueService: this fixture runs against an empty database, so persisting the plan state
        // in MigrationContext.Complete() would fail on the missing umbracoLock table and every plan would report
        // itself unsuccessful.
        Mock.Of<IKeyValueService>(),
        GetRequiredService<IServiceScopeFactory>(),
        AppCaches.NoCache,
        GetRequiredService<IPublishedContentTypeFactory>());

    public class RebuildCacheMigration : AsyncMigrationBase
    {
        public RebuildCacheMigration(IMigrationContext context)
            : base(context)
        {
        }

        protected override Task MigrateAsync()
        {
            RebuildCache = true;

            return Task.CompletedTask;
        }
    }

    public class NoopMigration : AsyncMigrationBase
    {
        public NoopMigration(IMigrationContext context)
            : base(context)
        {
        }

        protected override Task MigrateAsync() => Task.CompletedTask;
    }

    public class FailingMigration : AsyncMigrationBase
    {
        public FailingMigration(IMigrationContext context)
            : base(context)
        {
        }

        protected override Task MigrateAsync() => throw new InvalidOperationException("Migration failed.");
    }

    private sealed class CountingDatabaseCacheRebuilder : IDatabaseCacheRebuilder
    {
        public int RebuildCount { get; private set; }

        public Task<Attempt<DatabaseCacheRebuildResult>> RebuildAsync(bool useBackgroundThread)
        {
            RebuildCount++;

            return Task.FromResult(Attempt.Succeed(DatabaseCacheRebuildResult.Success));
        }

        public Task RebuildDatabaseCacheIfSerializerChangedAsync() => throw new NotSupportedException();
    }
}
