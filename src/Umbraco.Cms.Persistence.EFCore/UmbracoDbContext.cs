using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Persistence.EFCore.Migrations;

namespace Umbraco.Cms.Persistence.EFCore;

/// <summary>
/// </summary>
/// <remarks>
/// To autogenerate migrations use the following commands
/// and insure the 'src/Umbraco.Web.UI/appsettings.json' have a connection string set with the right provider.
///
/// Create a migration for each provider.
/// <code>dotnet ef migrations add %Name% -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.SqlServer -- --provider SqlServer</code>
///
/// <code>dotnet ef migrations add %Name% -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.Sqlite -- --provider Sqlite</code>
///
/// Remove the last migration for each provider.
/// <code>dotnet ef migrations remove -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.SqlServer -- --provider SqlServer</code>
///
/// <code>dotnet ef migrations remove -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.Sqlite -- --provider Sqlite</code>
///
/// To find documentation about this way of working with the context see
/// https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/providers?tabs=dotnet-core-cli#using-one-context-type
/// </remarks>
public class UmbracoDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UmbracoDbContext"/> class.
    /// </summary>
    /// <param name="options"></param>
    public UmbracoDbContext(DbContextOptions<UmbracoDbContext> options)
        : base(ConfigureOptions(options, out IOptionsMonitor<ConnectionStrings>? connectionStringsOptionsMonitor))
    {
        connectionStringsOptionsMonitor.OnChange(c =>
        {
            ILogger<UmbracoDbContext> logger = StaticServiceProvider.Instance.GetRequiredService<ILogger<UmbracoDbContext>>();
            logger.LogWarning("Connection string changed, disposing context");
            Dispose();
        });
    }

    private static DbContextOptions<UmbracoDbContext> ConfigureOptions(DbContextOptions<UmbracoDbContext> options, out IOptionsMonitor<ConnectionStrings> connectionStringsOptionsMonitor)
    {
        connectionStringsOptionsMonitor = StaticServiceProvider.Instance.GetRequiredService<IOptionsMonitor<ConnectionStrings>>();

        ConnectionStrings connectionStrings = connectionStringsOptionsMonitor.CurrentValue;

        if (string.IsNullOrWhiteSpace(connectionStrings.ConnectionString))
        {
            ILogger<UmbracoDbContext> logger = StaticServiceProvider.Instance.GetRequiredService<ILogger<UmbracoDbContext>>();
            logger.LogCritical("No connection string was found, cannot setup Umbraco EF Core context");
        }

        IEnumerable<IMigrationProviderSetup> migrationProviders = StaticServiceProvider.Instance.GetServices<IMigrationProviderSetup>();
        IMigrationProviderSetup? migrationProvider = migrationProviders.FirstOrDefault(x => x.ProviderName.CompareProviderNames(connectionStrings.ProviderName));

        if (migrationProvider == null && connectionStrings.ProviderName != null)
        {
            throw new InvalidOperationException($"No migration provider found for provider name {connectionStrings.ProviderName}");
        }

        var optionsBuilder = new DbContextOptionsBuilder<UmbracoDbContext>(options);
        migrationProvider?.Setup(optionsBuilder, connectionStrings.ConnectionString);
        return optionsBuilder.Options;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (IMutableEntityType entity in modelBuilder.Model.GetEntityTypes())
        {
            entity.SetTableName(Core.Constants.DatabaseSchema.TableNamePrefix + entity.GetTableName());
        }
    }
}
