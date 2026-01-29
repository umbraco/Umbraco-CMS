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
/// The Entity Framework Core database context for Umbraco CMS.
/// </summary>
/// <remarks>
/// To autogenerate migrations use the following commands
/// and insure the 'src/Umbraco.Web.UI/appsettings.json' have a connection string set with the right provider.
///
/// Create a migration for each provider.
/// <code>dotnet ef migrations add %Name% -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.SqlServer -c UmbracoDbContext</code>
///
/// <code>dotnet ef migrations add %Name% -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.Sqlite -c UmbracoDbContext</code>
///
/// Remove the last migration for each provider.
/// <code>dotnet ef migrations remove -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.SqlServer</code>
///
/// <code>dotnet ef migrations remove -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.Sqlite</code>
///
/// To find documentation about this way of working with the context see
/// https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/providers?tabs=dotnet-core-cli#using-one-context-type
/// </remarks>
public class UmbracoDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UmbracoDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used by the DbContext.</param>
    public UmbracoDbContext(DbContextOptions<UmbracoDbContext> options)
        : base(ConfigureOptions(options))
    { }

    private static DbContextOptions<UmbracoDbContext> ConfigureOptions(DbContextOptions<UmbracoDbContext> options)
    {
        var extensions = options.Extensions.FirstOrDefault() as Microsoft.EntityFrameworkCore.Infrastructure.CoreOptionsExtension;
        IServiceProvider? serviceProvider = extensions?.ApplicationServiceProvider;
        serviceProvider ??= StaticServiceProvider.Instance;
        if (serviceProvider == null)
        {
            // If the service provider is null, we cannot resolve the connection string or migration provider.
            throw new InvalidOperationException("The service provider is not configured. Ensure that UmbracoDbContext is registered correctly.");
        }

        IOptionsMonitor<ConnectionStrings>? connectionStringsOptionsMonitor = serviceProvider?.GetRequiredService<IOptionsMonitor<ConnectionStrings>>();

        ConnectionStrings? connectionStrings = connectionStringsOptionsMonitor?.CurrentValue;

        if (string.IsNullOrWhiteSpace(connectionStrings?.ConnectionString))
        {
            ILogger<UmbracoDbContext>? logger = serviceProvider?.GetRequiredService<ILogger<UmbracoDbContext>>();
            logger?.LogCritical("No connection string was found, cannot setup Umbraco EF Core context");

            // we're throwing an exception here to make it abundantly clear that one should never utilize (or have a
            // dependency on) the DbContext before the connection string has been initialized by the installer.
            throw new InvalidOperationException("No connection string was found, cannot setup Umbraco EF Core context");
        }

        IEnumerable<IMigrationProviderSetup>? migrationProviders = serviceProvider?.GetServices<IMigrationProviderSetup>();
        IMigrationProviderSetup? migrationProvider = migrationProviders?.FirstOrDefault(x => x.ProviderName.CompareProviderNames(connectionStrings.ProviderName));

        if (migrationProvider == null && connectionStrings.ProviderName != null)
        {
            throw new InvalidOperationException($"No migration provider found for provider name {connectionStrings.ProviderName}");
        }

        var optionsBuilder = new DbContextOptionsBuilder<UmbracoDbContext>(options);
        migrationProvider?.Setup(optionsBuilder, connectionStrings.ConnectionString);
        return optionsBuilder.Options;
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (IMutableEntityType entity in modelBuilder.Model.GetEntityTypes())
        {
            // Prefix OpenIddict tables with "umbraco", we only want to do this for OpenIddict because our own tables are not consistent.
            if (entity.ClrType.Assembly.FullName!.StartsWith("OpenIddict"))
            {
                entity.SetTableName(Core.Constants.DatabaseSchema.TableNamePrefix + entity.GetTableName());
            }
        }
    }
}
