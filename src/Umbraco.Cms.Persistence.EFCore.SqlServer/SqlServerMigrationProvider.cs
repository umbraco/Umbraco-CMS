using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Persistence.EFCore.Migrations;
using Umbraco.Extensions;

namespace Umbraco.Cms.Persistence.EFCore.SqlServer;

/// <summary>
/// Provides SQL Server-specific migration functionality for Umbraco's EF Core database context.
/// </summary>
public class SqlServerMigrationProvider : IMigrationProvider
{
    private readonly IDbContextFactory<UmbracoDbContext> _dbContextFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerMigrationProvider"/> class.
    /// </summary>
    /// <param name="dbContextFactory">The factory for creating database context instances.</param>
    public SqlServerMigrationProvider(IDbContextFactory<UmbracoDbContext> dbContextFactory) => _dbContextFactory = dbContextFactory;

    /// <inheritdoc />
    public string ProviderName => Constants.ProviderNames.SQLServer;

    /// <inheritdoc />
    public async Task MigrateAsync(EFCoreMigration migration)
    {
        UmbracoDbContext context = await _dbContextFactory.CreateDbContextAsync();
        await context.MigrateDatabaseAsync(GetMigrationType(migration));
    }

    /// <inheritdoc />
    public async Task MigrateAllAsync()
    {
        UmbracoDbContext context = await _dbContextFactory.CreateDbContextAsync();
        await context.Database.MigrateAsync();
    }

    private static Type GetMigrationType(EFCoreMigration migration) =>
        migration switch
        {
            EFCoreMigration.InitialCreate => typeof(Migrations.InitialCreate),
            EFCoreMigration.AddOpenIddict => typeof(Migrations.AddOpenIddict),
            EFCoreMigration.UpdateOpenIddictToV5 => typeof(Migrations.UpdateOpenIddictToV5),
            EFCoreMigration.UpdateOpenIddictToV7 => typeof(Migrations.UpdateOpenIddictToV7),
            _ => throw new ArgumentOutOfRangeException(nameof(migration), $@"Not expected migration value: {migration}")
        };
}
