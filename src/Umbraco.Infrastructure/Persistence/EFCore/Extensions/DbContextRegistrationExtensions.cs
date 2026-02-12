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

    public static void AddDbContextRegistrar(this IUmbracoBuilder builder, IDbContextServiceRegistrar registrar)
    {
        DbContextRegistration registration = builder.Services.GetDbContextRegistration();
        registration.AddRegistrar(builder.Services, registrar);
    }
}
