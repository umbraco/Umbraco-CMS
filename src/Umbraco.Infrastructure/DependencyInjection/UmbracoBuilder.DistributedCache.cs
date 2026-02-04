using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Factories;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Sync;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.DependencyInjection;

/// <summary>
///     Provides extension methods to the <see cref="IUmbracoBuilder" /> class.
/// </summary>
public static partial class UmbracoBuilderExtensions
{
    /// <summary>
    ///     Adds distributed cache support
    /// </summary>
    /// <remarks>
    ///     This is still required for websites that are not load balancing because this ensures that sites hosted
    ///     with managed hosts like IIS/etc... work correctly when AppDomains are running in parallel.
    /// </remarks>
    public static IUmbracoBuilder AddDistributedCache(this IUmbracoBuilder builder)
    {
        // Idempotency check using a private marker class
        if (builder.Services.Any(s => s.ServiceType == typeof(DistributedCacheMarker)))
        {
            return builder;
        }

        builder.Services.AddSingleton<DistributedCacheMarker>();
        builder.Services.AddSingleton<LastSyncedFileManager>();
        builder.Services.AddSingleton<ISyncBootStateAccessor, SyncBootStateAccessor>();
        builder.SetServerMessenger(factory => new BatchedDatabaseServerMessenger(
            factory.GetRequiredService<IMainDom>(),
            factory.GetRequiredService<CacheRefresherCollection>(),
            factory.GetRequiredService<ILogger<BatchedDatabaseServerMessenger>>(),
            factory.GetRequiredService<ISyncBootStateAccessor>(),
            factory.GetRequiredService<IHostingEnvironment>(),
            factory.GetRequiredService<ICacheInstructionService>(),
            factory.GetRequiredService<IJsonSerializer>(),
            factory.GetRequiredService<IRequestCache>(),
            factory.GetRequiredService<ILastSyncedManager>(),
            factory.GetRequiredService<IOptionsMonitor<GlobalSettings>>(),
            factory.GetRequiredService<IMachineInfoFactory>()));
        builder.AddNotificationHandler<UmbracoApplicationStartingNotification, DatabaseServerMessengerNotificationHandler>();
        builder.AddNotificationHandler<UmbracoRequestEndNotification, DatabaseServerMessengerNotificationHandler>();
        return builder;
    }

    /// <summary>
    ///     Sets the server registrar.
    /// </summary>
    /// <typeparam name="T">The type of the server registrar.</typeparam>
    /// <param name="builder">The builder.</param>
    public static IUmbracoBuilder SetServerRegistrar<T>(this IUmbracoBuilder builder)
        where T : class, IServerRoleAccessor
    {
        builder.Services.AddUnique<IServerRoleAccessor, T>();
        return builder;
    }

    /// <summary>
    ///     Sets the server registrar.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="factory">A function creating a server registrar.</param>
    public static IUmbracoBuilder SetServerRegistrar(
        this IUmbracoBuilder builder,
        Func<IServiceProvider, IServerRoleAccessor> factory)
    {
        builder.Services.AddUnique(factory);
        return builder;
    }

    /// <summary>
    ///     Sets the server registrar.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="registrar">A server registrar.</param>
    public static IUmbracoBuilder SetServerRegistrar(this IUmbracoBuilder builder, IServerRoleAccessor registrar)
    {
        builder.Services.AddUnique(registrar);
        return builder;
    }

    /// <summary>
    ///     Sets the server messenger.
    /// </summary>
    /// <typeparam name="T">The type of the server registrar.</typeparam>
    /// <param name="builder">The builder.</param>
    public static IUmbracoBuilder SetServerMessenger<T>(this IUmbracoBuilder builder)
        where T : class, IServerMessenger
    {
        builder.Services.AddUnique<IServerMessenger, T>();
        return builder;
    }

    /// <summary>
    ///     Sets the server messenger.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="factory">A function creating a server messenger.</param>
    public static IUmbracoBuilder SetServerMessenger(
        this IUmbracoBuilder builder,
        Func<IServiceProvider, IServerMessenger> factory)
    {
        builder.Services.AddUnique(factory);
        return builder;
    }

    /// <summary>
    ///     Sets the server messenger.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="registrar">A server messenger.</param>
    public static IUmbracoBuilder SetServerMessenger(this IUmbracoBuilder builder, IServerMessenger registrar)
    {
        builder.Services.AddUnique(registrar);
        return builder;
    }

    /// <summary>
    /// Marker class to ensure AddDistributedCache is only called once.
    /// </summary>
    private sealed class DistributedCacheMarker
    {
    }
}
