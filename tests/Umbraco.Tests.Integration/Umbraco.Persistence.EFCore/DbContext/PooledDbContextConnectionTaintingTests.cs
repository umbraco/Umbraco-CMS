using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Persistence.EFCore.Scoping;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Persistence.EFCore.DbContext;

/// <summary>
/// Tests for https://github.com/umbraco/Umbraco-CMS/issues/22124
/// EFCoreScope.SetDbConnection taints pooled DbContexts with ProfiledDbConnection, causing NRE on reuse.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Console)]
public class PooledDbContextConnectionTaintingTests : UmbracoIntegrationTest
{
    private IEFCoreScopeProvider<PooledTestDbContext> EfCoreScopeProvider =>
        GetRequiredService<IEFCoreScopeProvider<PooledTestDbContext>>();

    private IDbContextFactory<PooledTestDbContext> DbContextFactory =>
        GetRequiredService<IDbContextFactory<PooledTestDbContext>>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.Services.AddUmbracoDbContext<PooledTestDbContext>(
            (serviceProvider, options, connectionString, providerName) =>
            {
                if (providerName is "Npgsql2" or "Npgsql")
                {
                    options.UseNpgsql(connectionString!);
                    return;
                }

                options.UseUmbracoDatabaseProvider(serviceProvider);
            });
    }

    /// <summary>
    /// Verifies that a pooled DbContext obtained from IDbContextFactory has a valid connection string
    /// after an EFCore scope has used and disposed a context from the same pool.
    ///
    /// This reproduces the bug where EFCoreScope.InitializeDatabase() calls SetDbConnection()
    /// with the NPoco scope's ProfiledDbConnection, tainting the pooled context. When the NPoco
    /// scope disposes, the ProfiledDbConnection's inner connection is set to null. A subsequent
    /// CreateDbContext() call returns the tainted context, and accessing ConnectionString causes
    /// a NullReferenceException.
    /// </summary>
    [Test]
    public async Task Factory_Created_DbContext_Has_Valid_ConnectionString_After_Scope_Disposes()
    {
        // Step 1: Use an EFCore scope to trigger InitializeDatabase() which calls
        // SetDbConnection(transaction?.Connection) on the pooled DbContext.
        // This sets a ProfiledDbConnection (from the NPoco scope) on the EF context.
        using (IEfCoreScope<PooledTestDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async db =>
            {
                // Execute something to ensure the context is fully initialized
                // and SetDbConnection has been called.
                _ = db.Database.GetConnectionString();
            });
            scope.Complete();
        }

        // Step 2: At this point the scope has disposed:
        // - The EF context was returned to the pool (still holding a reference to the ProfiledDbConnection)
        // - The NPoco inner scope was disposed, which disposed the ProfiledDbConnection
        // - The ProfiledDbConnection's inner connection field is now null

        // Step 3: Get a context from the factory. If pooling is active, this may return
        // the same context instance that was tainted with the stale ProfiledDbConnection.
        using PooledTestDbContext context = DbContextFactory.CreateDbContext();

        // Step 4: The context should be usable. Before the fix, this would throw
        // NullReferenceException at ProfiledDbConnection.get_ConnectionString() because
        // the pooled context retained a stale ProfiledDbConnection whose inner connection was null.
        Assert.DoesNotThrowAsync(async () =>
        {
            await context.Database.CanConnectAsync();
        });
    }

    /// <summary>
    /// Simulates the exact scenario from the issue: using IEFCoreScopeProvider for normal data
    /// access, then using IDbContextFactory for migrations. The migration-like operation should
    /// not fail with a NullReferenceException.
    /// </summary>
    [Test]
    public async Task Can_Use_Factory_DbContext_For_Migrations_After_Scope_Usage()
    {
        // Step 1: Normal data access via scope (triggers SetDbConnection)
        using (IEfCoreScope<PooledTestDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async db =>
            {
                // Simulate normal data access
                await db.Database.ExecuteSqlRawAsync("SELECT 1");
            });
            scope.Complete();
        }

        // Step 2: Migration-like operation via factory (the scenario that fails)
        using PooledTestDbContext context = DbContextFactory.CreateDbContext();

        // This is what fails in the reported issue:
        // context.Database.GetPendingMigrationsAsync() eventually calls
        // ProfiledDbConnection.ConnectionString which NREs.
        Assert.DoesNotThrowAsync(async () =>
        {
            // GetPendingMigrationsAsync accesses ConnectionString internally
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        });
    }

    internal class PooledTestDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public PooledTestDbContext(DbContextOptions<PooledTestDbContext> options)
            : base(options)
        {
        }
    }
}
