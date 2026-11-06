using Microsoft.Extensions.DependencyInjection;
using Moq;
using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_10_0;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Migrations.Upgrade.V17_10_0;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class AddRelationsLockTests : UmbracoIntegrationTest
{
    private IMigrationBuilder MigrationBuilder => GetRequiredService<IMigrationBuilder>();

    private IUmbracoDatabaseFactory UmbracoDatabaseFactory => GetRequiredService<IUmbracoDatabaseFactory>();

    private IServiceScopeFactory ServiceScopeFactory => GetRequiredService<IServiceScopeFactory>();

    private DistributedCache DistributedCache => GetRequiredService<DistributedCache>();

    private IDatabaseCacheRebuilder DatabaseCacheRebuilder => GetRequiredService<IDatabaseCacheRebuilder>();

    private IPublishedContentTypeFactory PublishedContentTypeFactory => GetRequiredService<IPublishedContentTypeFactory>();

    private IMigrationPlanExecutor MigrationPlanExecutor => new MigrationPlanExecutor(
        ScopeProvider,
        ScopeAccessor,
        LoggerFactory,
        MigrationBuilder,
        UmbracoDatabaseFactory,
        DatabaseCacheRebuilder,
        DistributedCache,
        Mock.Of<IKeyValueService>(),
        ServiceScopeFactory,
        AppCaches.NoCache,
        PublishedContentTypeFactory);

    [Test]
    public void New_Installs_Include_The_Lock()
        => Assert.That(CountRelationsLocks(), Is.EqualTo(1));

    [Test]
    public async Task Can_Add_The_Lock_When_Missing()
    {
        using (IUmbracoDatabase db = UmbracoDatabaseFactory.CreateDatabase())
        {
            db.Delete<LockDto>(Constants.Locks.Relations);
        }

        Assert.That(CountRelationsLocks(), Is.Zero, "Arrange failed: the lock should have been removed.");

        ExecutedMigrationPlan result = await ExecuteMigration();

        Assert.Multiple(() =>
        {
            Assert.That(result.Successful, Is.True);
            Assert.That(CountRelationsLocks(), Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Can_Run_When_The_Lock_Already_Exists()
    {
        ExecutedMigrationPlan result = await ExecuteMigration();

        Assert.Multiple(() =>
        {
            Assert.That(result.Successful, Is.True);
            Assert.That(CountRelationsLocks(), Is.EqualTo(1));
        });
    }

    private async Task<ExecutedMigrationPlan> ExecuteMigration()
    {
        var upgrader = new Upgrader(
            new MigrationPlan("AddRelationsLockTest")
                .From(string.Empty)
                .To<AddRelationsLock>("done"));

        return await upgrader.ExecuteAsync(MigrationPlanExecutor, ScopeProvider, Mock.Of<IKeyValueService>());
    }

    private int CountRelationsLocks()
    {
        using IUmbracoDatabase db = UmbracoDatabaseFactory.CreateDatabase();
        Sql<ISqlContext> sql = db.SqlContext.Sql()
            .SelectCount()
            .From<LockDto>()
            .Where<LockDto>(x => x.Id == Constants.Locks.Relations);
        return db.ExecuteScalar<int>(sql);
    }
}
