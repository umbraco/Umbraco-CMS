using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;

namespace Umbraco.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IServiceCollection"/> to work with <see cref="DbContextRegistration"/>.
/// </summary>
public static class DbContextRegistrationExtensions
{
    /// <summary>
    /// Gets or creates the <see cref="DbContextRegistration"/> singleton from the service collection.
    /// When creating a new instance, the configured database provider name is read from
    /// <see cref="IConfiguration"/> to filter registrars via <see cref="IDbContextServiceRegistrar.CanHandle"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The <see cref="DbContextRegistration"/> instance.</returns>
    public static DbContextRegistration GetDbContextRegistration(this IServiceCollection services)
    {
        ServiceDescriptor? dbContextDescriptor = services.FirstOrDefault(
            descriptor => descriptor.ServiceType == typeof(DbContextRegistration));

        if (dbContextDescriptor?.ImplementationInstance is DbContextRegistration registration)
        {
            return registration;
        }

        var configuration = services.FirstOrDefault(
            descriptor => descriptor.ServiceType == typeof(IConfiguration))?.ImplementationInstance as IConfiguration;

        string? providerName = null;
        configuration?.GetUmbracoConnectionString(out providerName);

        registration = new DbContextRegistration(providerName);
        services.AddSingleton(registration);
        return registration;
    }

    /// <summary>
    /// Adds a <see cref="IDbContextServiceRegistrar"/> to the builder, which will be replayed
    /// against all registered DbContext types. The registrar's <see cref="IDbContextServiceRegistrar.CanHandle"/>
    /// method is checked against the configured database provider name.
    /// </summary>
    /// <typeparam name="TRegistrar">
    /// The type of <see cref="IDbContextServiceRegistrar"/> to add. Must have a parameterless constructor.
    /// </typeparam>
    /// <param name="builder">The Umbraco builder.</param>
    public static void AddDbContextRegistrar<TRegistrar>(this IUmbracoBuilder builder)
        where TRegistrar : IDbContextServiceRegistrar, new()
        => AddDbContextRegistrar(builder, new TRegistrar());

    /// <summary>
    /// Adds a <see cref="IDbContextServiceRegistrar"/> instance to the builder, which will be replayed
    /// against all registered DbContext types. The registrar's <see cref="IDbContextServiceRegistrar.CanHandle"/>
    /// method is checked against the configured database provider name.
    /// </summary>
    /// <param name="builder">The Umbraco builder.</param>
    /// <param name="registrar">The registrar instance to add.</param>
    public static void AddDbContextRegistrar(this IUmbracoBuilder builder, IDbContextServiceRegistrar registrar)
    {
        DbContextRegistration registration = builder.Services.GetDbContextRegistration();
        registration.AddRegistrar(builder.Services, registrar);
    }

    /// <summary>
    /// Registers a provider-specific EF Core model customizer that will be applied during
    /// <see cref="Microsoft.EntityFrameworkCore.DbContext" /> model creation.
    /// </summary>
    /// <typeparam name="TCustomizer">
    /// The type of <see cref="IEFCoreModelCustomizer"/> to register.
    /// </typeparam>
    /// <param name="builder">The Umbraco builder.</param>
    public static IUmbracoBuilder AddEFCoreModelCustomizer<TCustomizer>(this IUmbracoBuilder builder)
        where TCustomizer : class, IEFCoreModelCustomizer
    {
        builder.Services.AddSingleton<IEFCoreModelCustomizer, TCustomizer>();
        return builder;
    }
}
