using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Infrastructure.BackgroundJobs;

namespace Umbraco.Extensions;

/// <summary>
/// Contains extension methods for <see cref="IServiceCollection"/> to configure and register Umbraco-related services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds a recurring background job with an implementation type of <typeparamref name="TJob" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the recurring background job to.</param>
    public static void AddRecurringBackgroundJob<TJob>(
        this IServiceCollection services)
        where TJob : class, IRecurringBackgroundJob
        => services.AddSingleton<IRecurringBackgroundJob, TJob>();

    /// <summary>
    /// Adds a recurring background job with an implementation type of <typeparamref name="TJob" /> using the factory <paramref name="implementationFactory" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the recurring background job to.</param>
    /// <param name="implementationFactory">A factory function to create an instance of <typeparamref name="TJob" /> using the provided <see cref="IServiceProvider" />.</param>
    public static void AddRecurringBackgroundJob<TJob>(
        this IServiceCollection services,
        Func<IServiceProvider, TJob> implementationFactory)
        where TJob : class, IRecurringBackgroundJob
        => services.AddSingleton<IRecurringBackgroundJob, TJob>(implementationFactory);

    /// <summary>
    /// Adds a triggerable recurring background job with an implementation type of <typeparamref name="TJob" /> and registers an <see cref="IRecurringBackgroundJobTrigger{TJob}" /> for it.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the recurring background job to.</param>
    public static void AddTriggerableRecurringBackgroundJob<TJob>(
        this IServiceCollection services)
        where TJob : class, ITriggerableRecurringBackgroundJob
    {
        services.AddRecurringBackgroundJob<TJob>();
        services.AddSingleton<IRecurringBackgroundJobTrigger<TJob>, RecurringBackgroundJobTrigger<TJob>>();
    }

    /// <summary>
    /// Adds a triggerable recurring background job with an implementation type of <typeparamref name="TJob" /> using the factory <paramref name="implementationFactory" /> and registers an <see cref="IRecurringBackgroundJobTrigger{TJob}" /> for it.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the recurring background job to.</param>
    /// <param name="implementationFactory">A factory function to create an instance of <typeparamref name="TJob" /> using the provided <see cref="IServiceProvider" />.</param>
    public static void AddTriggerableRecurringBackgroundJob<TJob>(
        this IServiceCollection services,
        Func<IServiceProvider, TJob> implementationFactory)
        where TJob : class, ITriggerableRecurringBackgroundJob
    {
        services.AddRecurringBackgroundJob(implementationFactory);
        services.AddSingleton<IRecurringBackgroundJobTrigger<TJob>, RecurringBackgroundJobTrigger<TJob>>();
    }
}
