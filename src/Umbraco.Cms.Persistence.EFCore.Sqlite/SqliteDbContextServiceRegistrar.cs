using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Locking;

namespace Umbraco.Cms.Persistence.EFCore.Sqlite;

/// <summary>
/// Registers SQLite-specific services for a given <see cref="DbContext"/> type.
/// </summary>
public class SqliteDbContextServiceRegistrar : IDbContextServiceRegistrar
{
    /// <inheritdoc />
    public bool CanHandle(string providerName)
        => string.Equals(providerName, Constants.ProviderNames.SQLite, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public void RegisterServices<TContext>(IServiceCollection services)
        where TContext : DbContext
        => services.AddSingleton<IDistributedLockingMechanism, SqliteEFCoreDistributedLockingMechanism<TContext>>();
}
