using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Locking;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.EFCore.Extensions;

/// <summary>
/// Provides extension methods for registering EF Core services with Umbraco.
/// </summary>
public static class UmbracoEFCoreServiceCollectionExtensions
{
    /// <summary>
    /// Delegate for configuring EF Core options with provider information.
    /// </summary>
    /// <param name="options">The DbContext options builder.</param>
    /// <param name="providerName">The database provider name.</param>
    /// <param name="connectionString">The connection string.</param>
    public delegate void DefaultEFCoreOptionsAction(DbContextOptionsBuilder options, string? providerName, string? connectionString);

    /// <summary>
    /// Adds a EFCore DbContext with all the services needed to integrate with Umbraco scopes.
    /// </summary>
    public static IServiceCollection AddUmbracoDbContext<T>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder, string?, string?, IServiceProvider?>? optionsAction = null)
        where T : DbContext
    {
        return AddUmbracoDbContext<T>(services, (IServiceProvider provider, DbContextOptionsBuilder optionsBuilder, string? providerName, string? connectionString) =>
        {
            ConnectionStrings connectionStrings = GetConnectionStringAndProviderName(provider);
            optionsAction?.Invoke(optionsBuilder, connectionStrings.ConnectionString, connectionStrings.ProviderName, provider);
        });
    }

    /// <summary>
    /// Adds a EFCore DbContext with all the services needed to integrate with Umbraco scopes.
    /// </summary>
    public static IServiceCollection AddUmbracoDbContext<T>(
        this IServiceCollection services,
        Action<IServiceProvider, DbContextOptionsBuilder, string?, string?>? optionsAction = null)
        where T : DbContext
    {
        optionsAction ??= (sp, optionsBuilder, connectionString, providerName) => { };


        services.AddPooledDbContextFactory<T>((provider, optionsBuilder) => SetupDbContext(optionsAction, provider, optionsBuilder));
        services.AddTransient(services => services.GetRequiredService<IDbContextFactory<T>>().CreateDbContext());

        services.AddUnique<IAmbientEFCoreScopeStack<T>, AmbientEFCoreScopeStack<T>>();
        services.AddUnique<IEFCoreScopeAccessor<T>, EFCoreScopeAccessor<T>>();
        services.AddUnique<IEFCoreScopeProvider<T>, EFCoreScopeProvider<T>>();
        services.AddSingleton<IDistributedLockingMechanism, SqliteEFCoreDistributedLockingMechanism<T>>();
        services.AddSingleton<IDistributedLockingMechanism, SqlServerEFCoreDistributedLockingMechanism<T>>();

        return services;
    }

    /// <summary>
    /// Sets the database provider. I.E UseSqlite or UseSqlServer based on the provider name.
    /// </summary>
    /// <param name="builder">The DbContext options builder.</param>
    /// <param name="providerName">The database provider name.</param>
    /// <param name="connectionString">The connection string.</param>
    /// <exception cref="InvalidDataException">Thrown when the provider is not supported.</exception>
    /// <remarks>
    /// Only supports the databases normally supported in Umbraco.
    /// </remarks>
    public static void UseDatabaseProvider(this DbContextOptionsBuilder builder, string providerName, string connectionString)
    {
        switch (providerName)
        {
            case Constants.ProviderNames.SQLServer:
                builder.UseSqlServer(connectionString);
                break;
            case Constants.ProviderNames.SQLLite:
            case "Microsoft.Data.SQLite":
                builder.UseSqlite(connectionString);
                break;
            default:
                throw new InvalidDataException($"The provider {providerName} is not supported. Manually add the add the UseXXX statement to the options. I.E UseNpgsql()");
        }
    }

    /// <summary>
    /// Sets the database provider to use based on the Umbraco connection string.
    /// </summary>
    /// <param name="builder">The DbContext options builder.</param>
    /// <param name="serviceProvider">The service provider to resolve connection string settings from.</param>
    public static void UseUmbracoDatabaseProvider(this DbContextOptionsBuilder builder, IServiceProvider serviceProvider)
    {
        ConnectionStrings connectionStrings = serviceProvider.GetRequiredService<IOptionsMonitor<ConnectionStrings>>().CurrentValue;

        // Replace data directory
        string? dataDirectory = AppDomain.CurrentDomain.GetData(Core.Constants.System.DataDirectoryName)?.ToString();
        if (string.IsNullOrEmpty(dataDirectory) is false)
        {
            connectionStrings.ConnectionString = connectionStrings.ConnectionString?.Replace(Core.Constants.System.DataDirectoryPlaceholder, dataDirectory);
        }

        if (string.IsNullOrEmpty(connectionStrings.ProviderName))
        {
            Log.Warning("No database provider was set. ProviderName is null");
            return;
        }

        if (string.IsNullOrEmpty(connectionStrings.ConnectionString))
        {
            Log.Warning("No database provider was set. Connection string is null");
            return;
        }

        builder.UseDatabaseProvider(connectionStrings.ProviderName, connectionStrings.ConnectionString);
    }

    private static void SetupDbContext(Action<IServiceProvider, DbContextOptionsBuilder, string?, string?>? optionsAction, IServiceProvider provider, DbContextOptionsBuilder builder)
    {
        ConnectionStrings connectionStrings = GetConnectionStringAndProviderName(provider);

        optionsAction?.Invoke(provider, builder, connectionStrings.ConnectionString, connectionStrings.ProviderName);
    }

    private static ConnectionStrings GetConnectionStringAndProviderName(IServiceProvider serviceProvider)
    {
        ConnectionStrings connectionStrings = serviceProvider.GetRequiredService<IOptionsMonitor<ConnectionStrings>>().CurrentValue;

        // Replace data directory
        string? dataDirectory = AppDomain.CurrentDomain.GetData(Core.Constants.System.DataDirectoryName)?.ToString();
        if (string.IsNullOrEmpty(dataDirectory) is false)
        {
            connectionStrings.ConnectionString = connectionStrings.ConnectionString?.Replace(Core.Constants.System.DataDirectoryPlaceholder, dataDirectory);
        }

        return connectionStrings;
    }
}
