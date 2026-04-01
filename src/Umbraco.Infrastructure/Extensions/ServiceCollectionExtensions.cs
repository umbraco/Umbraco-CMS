using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Infrastructure.BackgroundJobs;

namespace Umbraco.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds a recurring background job with an implementation type of <typeparamref name="TJob" />.
    /// </summary>
    /// <typeparam name="TJob">The type of the job.</typeparam>
    /// <param name="services">The services.</param>
    public static void AddRecurringBackgroundJob<TJob>(this IServiceCollection services)
        where TJob : class, IRecurringBackgroundJob
        => services.AddSingleton<IRecurringBackgroundJob, TJob>();

    /// <summary>
    /// Adds a recurring background job with an implementation type of <typeparamref name="TJob" /> using the factory <paramref name="implementationFactory" />.
    /// </summary>
    /// <typeparam name="TJob">The type of the job.</typeparam>
    /// <param name="services">The services.</param>
    /// <param name="implementationFactory">The implementation factory.</param>
    public static void AddRecurringBackgroundJob<TJob>(
        this IServiceCollection services,
        Func<IServiceProvider, TJob> implementationFactory)
        where TJob : class, IRecurringBackgroundJob
        => services.AddSingleton<IRecurringBackgroundJob, TJob>(implementationFactory);
}
