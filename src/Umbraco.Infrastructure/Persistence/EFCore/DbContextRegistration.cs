using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Umbraco.Cms.Infrastructure.Persistence.EFCore;

/// <summary>
/// Coordinates registration of provider-specific services for EF Core DbContext types.
/// </summary>
/// <remarks>
/// <para>
/// This class handles the bidirectional registration problem: <c>AddUmbracoDbContext&lt;T&gt;</c> runs
/// before Composers (which register provider-specific services), but providers need to register
/// services that are generic on the DbContext type.
/// </para>
/// <para>
/// When a DbContext type is registered via <see cref="RegisterDbContextType{T}"/>, any already-added
/// registrars are immediately invoked. When a registrar is added via <see cref="AddRegistrar"/>,
/// it is replayed against all previously registered DbContext types. This ensures services are
/// registered regardless of ordering.
/// </para>
/// </remarks>
public class DbContextRegistration
{
    private readonly List<Action<IServiceCollection, IDbContextServiceRegistrar>> _contextTypeActions = new();
    private readonly List<IDbContextServiceRegistrar> _registrars = new();

    /// <summary>
    /// Registers a DbContext type and invokes all existing registrars for it.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="DbContext"/>.</typeparam>
    /// <param name="services">The service collection.</param>
    public void RegisterDbContextType<T>(IServiceCollection services)
        where T : DbContext
    {
        // Store a replay action for future registrars (captures <T> in the closure)
        _contextTypeActions.Add((serviceCollection, registrar) => registrar.RegisterServices<T>(serviceCollection));

        // Execute for already-registered registrars
        foreach (IDbContextServiceRegistrar registrar in _registrars)
        {
            registrar.RegisterServices<T>(services);
        }
    }

    /// <summary>
    /// Adds a registrar and replays it against all previously registered DbContext types.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="registrar">The registrar to add.</param>
    public void AddRegistrar(IServiceCollection services, IDbContextServiceRegistrar registrar)
    {
        _registrars.Add(registrar);

        // Replay for already-registered context types
        foreach (Action<IServiceCollection, IDbContextServiceRegistrar> action in _contextTypeActions)
        {
            action(services, registrar);
        }
    }
}


