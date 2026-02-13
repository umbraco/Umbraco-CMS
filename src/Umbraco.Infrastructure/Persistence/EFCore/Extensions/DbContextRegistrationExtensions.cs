using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;

namespace Umbraco.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IServiceCollection"/> to work with <see cref="DbContextRegistration"/>.
/// </summary>
public static class DBContextRegistrationExtensions
{
    /// <summary>
    /// Gets or creates the <see cref="DbContextRegistration"/> singleton from the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The <see cref="DbContextRegistration"/> instance.</returns>
    public static DbContextRegistration GetDbContextRegistration(this IServiceCollection services)
    {
        ServiceDescriptor? descriptor = services.FirstOrDefault(
            descriptor => descriptor.ServiceType == typeof(DbContextRegistration));

        if (descriptor?.ImplementationInstance is DbContextRegistration registration)
        {
            return registration;
        }

        registration = new DbContextRegistration();
        services.AddSingleton(registration);
        return registration;
    }

    /// <summary>
    /// Adds a <see cref="IDbContextServiceRegistrar"/> to the builder, which will be replayed
    /// against all registered DbContext types.
    /// </summary>
    /// <typeparam name="TRegistrar">
    /// The type of <see cref="IDbContextServiceRegistrar"/> to add. Must have a parameterless constructor.
    /// </typeparam>
    /// <param name="builder">The Umbraco builder.</param>
    public static void AddDbContextRegistrar<TRegistrar>(this IUmbracoBuilder builder)
        where TRegistrar : IDbContextServiceRegistrar, new()
    {
        DbContextRegistration registration = builder.Services.GetDbContextRegistration();
        registration.AddRegistrar(builder.Services, new TRegistrar());
    }

    /// <summary>
    /// Adds a <see cref="IDbContextServiceRegistrar"/> instance to the builder, which will be replayed
    /// against all registered DbContext types.
    /// </summary>
    /// <param name="builder">The Umbraco builder.</param>
    /// <param name="registrar">The registrar instance to add.</param>
    public static void AddDbContextRegistrar(this IUmbracoBuilder builder, IDbContextServiceRegistrar registrar)
    {
        DbContextRegistration registration = builder.Services.GetDbContextRegistration();
        registration.AddRegistrar(builder.Services, registrar);
    }
}
