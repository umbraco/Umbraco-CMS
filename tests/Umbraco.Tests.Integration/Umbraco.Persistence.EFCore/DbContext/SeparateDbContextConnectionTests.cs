using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Persistence.EFCore.Scoping;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Persistence.EFCore.DbContext;

/// <summary>
/// Tests for https://github.com/umbraco/Umbraco-CMS/issues/22131
/// AddUmbracoDbContext with shareUmbracoConnection: false should use its own connection.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Console)]
public class SeparateDbContextConnectionTests : UmbracoIntegrationTest
{
    private IEFCoreScopeProvider<SeparateDbContext> EfCoreScopeProvider =>
        GetRequiredService<IEFCoreScopeProvider<SeparateDbContext>>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.Services.AddUmbracoDbContext<SeparateDbContext>(
            (_, options, _, _) =>
            {
                options.UseSqlite("Data Source=separate-test.db");
            },
            shareUmbracoConnection: false);
    }

    /// <summary>
    /// Verifies that a DbContext registered with shareUmbracoConnection: false
    /// uses its own configured connection string, not the Umbraco main database connection.
    /// Before the fix, EFCoreScope.InitializeDatabase() unconditionally called
    /// SetDbConnection(transaction?.Connection) which forced the NPoco connection
    /// onto the DbContext, overriding the configured connection string.
    /// </summary>
    [Test]
    public async Task Scope_Uses_Own_Connection_When_ShareUmbracoConnection_Is_False()
    {
        using IEfCoreScope<SeparateDbContext> scope = EfCoreScopeProvider.CreateScope();

        await scope.ExecuteWithContextAsync<Task>(async db =>
        {
            var connectionString = db.Database.GetConnectionString();
            Assert.That(connectionString, Does.Contain("separate-test.db"),
                "The DbContext should use its own configured connection string, " +
                "not the Umbraco main database connection.");
        });

        scope.Complete();
    }

    /// <summary>
    /// Verifies that a DbContext registered with shareUmbracoConnection: false
    /// can create and query its own tables independently of the Umbraco database.
    /// </summary>
    [Test]
    public async Task Scope_Can_Create_And_Query_Own_Tables()
    {
        using IEfCoreScope<SeparateDbContext> scope = EfCoreScopeProvider.CreateScope();

        await scope.ExecuteWithContextAsync<Task>(async db =>
        {
            await db.Database.ExecuteSqlRawAsync(
                "DROP TABLE IF EXISTS TestItems");
            await db.Database.ExecuteSqlRawAsync(
                "CREATE TABLE TestItems (Id INTEGER PRIMARY KEY, Name TEXT)");
            await db.Database.ExecuteSqlRawAsync(
                "INSERT INTO TestItems (Id, Name) VALUES (1, 'test')");
            var name = await db.Database.ExecuteScalarAsync<string>(
                "SELECT Name FROM TestItems WHERE Id = 1");
            Assert.That(name, Is.EqualTo("test"));
        });

        scope.Complete();
    }

    internal class SeparateDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public SeparateDbContext(DbContextOptions<SeparateDbContext> options)
            : base(options)
        {
        }
    }
}
