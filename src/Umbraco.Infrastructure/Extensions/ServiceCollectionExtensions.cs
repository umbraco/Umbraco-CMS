using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Infrastructure.BackgroundJobs;

namespace Umbraco.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds a recurring background job with an implementation type of
    ///     <typeparamref name="TJob" /> to the specified <see cref="IServiceCollection" />.
    /// </summary>
    public static void AddRecurringBackgroundJob<TJob>(
        this IServiceCollection services)
        where TJob : class, IRecurringBackgroundJob =>
        services.AddSingleton<IRecurringBackgroundJob, TJob>();

    /// <summary>
    ///     Adds a recurring background job with an implementation type of
    ///     <typeparamref name="TJob" /> using the factory <paramref name="implementationFactory"/>
    ///     to the specified <see cref="IServiceCollection" />.
    /// </summary>
    public static void AddRecurringBackgroundJob<TJob>(
        this IServiceCollection services,
        Func<IServiceProvider, TJob> implementationFactory)
        where TJob : class, IRecurringBackgroundJob =>
        services.AddSingleton<IRecurringBackgroundJob, TJob>(implementationFactory);

}

