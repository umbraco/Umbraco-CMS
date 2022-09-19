using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds a service of type <typeparamref name="TService" /> with an implementation type of
    ///     <typeparamref name="TImplementing" /> to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <remarks>
    ///     Removes all previous registrations for the type <typeparamref name="TService" />.
    /// </remarks>
    public static void AddUnique<TService, TImplementing>(
        this IServiceCollection services)
        where TService : class
        where TImplementing : class, TService =>
        AddUnique<TService, TImplementing>(services, ServiceLifetime.Singleton);

    /// <summary>
    ///     Adds a service of type <typeparamref name="TService" /> with an implementation type of
    ///     <typeparamref name="TImplementing" /> to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <remarks>
    ///     Removes all previous registrations for the type <typeparamref name="TService" />.
    /// </remarks>
    public static void AddUnique<TService, TImplementing>(
        this IServiceCollection services,
        ServiceLifetime lifetime)
        where TService : class
        where TImplementing : class, TService
    {
        services.RemoveAll<TService>();
        services.Add(ServiceDescriptor.Describe(typeof(TService), typeof(TImplementing), lifetime));
    }

    /// <summary>
    ///     Adds services of types <typeparamref name="TService1" /> &amp; <typeparamref name="TService2" /> with a shared
    ///     implementation type of <typeparamref name="TImplementing" /> to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <remarks>
    ///     Removes all previous registrations for the types <typeparamref name="TService1" /> &amp;
    ///     <typeparamref name="TService2" />.
    /// </remarks>
    public static void AddMultipleUnique<TService1, TService2, TImplementing>(
        this IServiceCollection services)
        where TService1 : class
        where TService2 : class
        where TImplementing : class, TService1, TService2
        => services.AddMultipleUnique<TService1, TService2, TImplementing>(ServiceLifetime.Singleton);

    /// <summary>
    /// Adds services of types <typeparamref name="TService1"/> &amp; <typeparamref name="TService2"/> with a shared implementation type of <typeparamref name="TImplementing"/> to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>
    /// Removes all previous registrations for the types <typeparamref name="TService1"/> &amp; <typeparamref name="TService2"/>.
    /// </remarks>
    public static void AddMultipleUnique<TService1, TService2, TImplementing>(
        this IServiceCollection services,
        ServiceLifetime lifetime)
        where TService1 : class
        where TService2 : class
        where TImplementing : class, TService1, TService2
    {
        services.AddUnique<TService1, TImplementing>(lifetime);
        services.AddUnique<TService2>(factory => (TImplementing)factory.GetRequiredService<TService1>(), lifetime);
    }

    // TODO(V11): Remove this function.
    [Obsolete("This method is functionally equivalent to AddSingleton<TImplementing>() please use that instead.")]
    public static void AddUnique<TImplementing>(this IServiceCollection services)
        where TImplementing : class
    {
        services.RemoveAll<TImplementing>();
        services.AddSingleton<TImplementing>();
    }

    /// <summary>
    ///     Adds a service of type <typeparamref name="TService" /> with an implementation factory method to the specified
    ///     <see cref="IServiceCollection" />.
    /// </summary>
    /// <remarks>
    ///     Removes all previous registrations for the type <typeparamref name="TService" />.
    /// </remarks>
    public static void AddUnique<TService>(
        this IServiceCollection services,
        Func<IServiceProvider, TService> factory)
        where TService : class
        => services.AddUnique(factory, ServiceLifetime.Singleton);

    /// <summary>
    /// Adds a service of type <typeparamref name="TService"/> with an implementation factory method to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>
    /// Removes all previous registrations for the type <typeparamref name="TService"/>.
    /// </remarks>
    public static void AddUnique<TService>(
        this IServiceCollection services,
        Func<IServiceProvider, TService> factory,
        ServiceLifetime lifetime)
        where TService : class
    {
        services.RemoveAll<TService>();
        services.Add(ServiceDescriptor.Describe(typeof(TService), factory, lifetime));
    }

    /// <summary>
    ///     Adds a singleton service of the type specified by <paramref name="serviceType" /> to the specified
    ///     <see cref="IServiceCollection" />.
    /// </summary>
    /// <remarks>
    ///     Removes all previous registrations for the type specified by <paramref name="serviceType" />.
    /// </remarks>
    public static void AddUnique(this IServiceCollection services, Type serviceType, object instance)
    {
        services.RemoveAll(serviceType);
        services.AddSingleton(serviceType, instance);
    }

    /// <summary>
    ///     Adds a singleton service of type <typeparamref name="TService" /> to the specified
    ///     <see cref="IServiceCollection" />.
    /// </summary>
    /// <remarks>
    ///     Removes all previous registrations for the type type <typeparamref name="TService" />.
    /// </remarks>
    public static void AddUnique<TService>(this IServiceCollection services, TService instance)
        where TService : class
    {
        services.RemoveAll<TService>();
        services.AddSingleton(instance);
    }

    internal static IServiceCollection AddLazySupport(this IServiceCollection services)
    {
        services.Replace(ServiceDescriptor.Transient(typeof(Lazy<>), typeof(LazyResolve<>)));
        return services;
    }
}
