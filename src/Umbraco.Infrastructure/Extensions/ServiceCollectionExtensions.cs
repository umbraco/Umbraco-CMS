using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Infrastructure.BackgroundJobs;

namespace Umbraco.Extensions;

/// <summary>
/// Contains extension methods for <see cref="IServiceCollection"/> to configure and register Umbraco-related services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds a recurring background job with an implementation type of
    ///     <typeparamref name="TJob" /> to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the recurring background job to.</param>
    public static void AddRecurringBackgroundJob<TJob>(
        this IServiceCollection services)
        where TJob : class, IRecurringBackgroundJob =>
        services.AddSingleton<IRecurringBackgroundJob, TJob>();

    /// <summary>
    ///     Adds a recurring background job with an implementation type of
    ///     <typeparamref name="TJob" /> using the factory <paramref name="implementationFactory"/>
    ///     to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the recurring background job to.</param>
    /// <param name="implementationFactory">A factory function to create an instance of <typeparamref name="TJob" /> using the provided <see cref="IServiceProvider" />.</param>
    public static void AddRecurringBackgroundJob<TJob>(
        this IServiceCollection services,
        Func<IServiceProvider, TJob> implementationFactory)
        where TJob : class, IRecurringBackgroundJob =>
        services.AddSingleton<IRecurringBackgroundJob, TJob>(implementationFactory);

}

