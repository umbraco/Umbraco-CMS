using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Umbraco.Cms.Infrastructure.Persistence.EFCore;

/// <summary>
/// Registers provider-specific services for a given <see cref="DbContext"/> type.
/// </summary>
/// <remarks>
/// Implementations are provided by database provider packages (e.g., SQLite, SQL Server)
/// to register services such as distributed locking mechanisms that are specific to
/// both the provider and the <see cref="DbContext"/> type.
/// </remarks>
public interface IDbContextServiceRegistrar
{
    /// <summary>
    /// Determines whether this registrar should register services for the specified database provider.
    /// </summary>
    /// <param name="providerName">The database provider name from the connection string configuration.</param>
    /// <returns><c>true</c> if this registrar should handle the specified provider; otherwise, <c>false</c>.</returns>
    bool CanHandle(string providerName);

    /// <summary>
    /// Registers provider-specific services for the specified <see cref="DbContext"/> type.
    /// </summary>
    /// <typeparam name="TContext">The type of <see cref="DbContext"/>.</typeparam>
    /// <param name="services">The service collection to register services with.</param>
    void RegisterServices<TContext>(IServiceCollection services)
        where TContext : DbContext;
}
