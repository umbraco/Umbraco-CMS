using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Umbraco.Cms.Persistence.EFCore.Scoping;
using Umbraco.Cms.Persistence.EFCore.Sqlite;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Persistence.EFCore.DbContext;

/// <summary>
/// A context sharing the Umbraco connection must be able to run queries that the database provider
/// translates to its own connection-level functions and collations, exactly as on a connection EF Core opened itself.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Console)]
internal sealed class SharedConnectionProviderFunctionsTests : UmbracoIntegrationTest
{
    // Text ordering ("10.25" < "100" < "9.5") differs from numeric ordering, so a query that silently
    // falls back to comparing the stored text gives a visibly different result.
    private static readonly decimal[] _amounts = [100m, 9.5m, 10.25m];

    private IEFCoreScopeProvider<AmountDbContext> EfCoreScopeProvider =>
        GetRequiredService<IEFCoreScopeProvider<AmountDbContext>>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        // Integration tests don't run composers, so register the provider's services as a site would.
        new EFCoreSqliteComposer().Compose(builder);

        builder.Services.AddUmbracoDbContext<AmountDbContext>(
            (serviceProvider, options, _, _) => options.UseUmbracoDatabaseProvider(serviceProvider),
            shareUmbracoConnection: true);
    }

    [Test]
    public async Task Shared_Connection_Supports_Ordering_By_Provider_Translated_Types()
    {
        using IEfCoreScope<AmountDbContext> scope = EfCoreScopeProvider.CreateScope();

        List<decimal> ordered = await scope.ExecuteWithContextAsync(async db =>
        {
            await SeedAsync(db);
            return await db.Amounts.OrderBy(x => x.Amount).Select(x => x.Amount).ToListAsync();
        });

        scope.Complete();

        Assert.That(ordered, Is.EqualTo(new[] { 9.5m, 10.25m, 100m }));
    }

    [Test]
    public async Task Shared_Connection_Supports_Comparing_Provider_Translated_Types()
    {
        using IEfCoreScope<AmountDbContext> scope = EfCoreScopeProvider.CreateScope();

        List<decimal> filtered = await scope.ExecuteWithContextAsync(async db =>
        {
            await SeedAsync(db);
            return await db.Amounts.Where(x => x.Amount > 10m).Select(x => x.Amount).ToListAsync();
        });

        scope.Complete();

        Assert.That(filtered, Is.EquivalentTo(new[] { 10.25m, 100m }));
    }

    private static async Task SeedAsync(AmountDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync(db.Database.GenerateCreateScript());
        db.Amounts.AddRange(_amounts.Select(amount => new AmountEntity { Amount = amount }));
        await db.SaveChangesAsync();
    }

    internal sealed class AmountDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public AmountDbContext(DbContextOptions<AmountDbContext> options)
            : base(options)
        {
        }

        public DbSet<AmountEntity> Amounts => Set<AmountEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<AmountEntity>(entity =>
            {
                entity.ToTable("testSharedConnectionAmount");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Amount).HasPrecision(18, 4);
            });
    }

    internal sealed class AmountEntity
    {
        public int Id { get; set; }

        public decimal Amount { get; set; }
    }
}
