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
    /// Adds distributed cache support to the specified <see cref="IUmbracoBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IUmbracoBuilder"/> to which distributed cache support will be added.</param>
    /// <returns>The <see cref="IUmbracoBuilder"/> instance with distributed cache support enabled.</returns>
    /// <remarks>
    /// This is still required for websites that are not load balancing because it ensures that sites hosted
    /// with managed hosts like IIS and similar environments work correctly when AppDomains are running in parallel.
    /// </remarks>
    public static IUmbracoBuilder AddDistributedCache(this IUmbracoBuilder builder)
    {
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
    ///     Registers the specified <typeparamref name="T"/> as the implementation of <see cref="IServerRoleAccessor"/> in the dependency injection container.
    /// </summary>
    /// <typeparam name="T">The type of the server registrar to register. Must implement <see cref="IServerRoleAccessor"/>.</typeparam>
    /// <param name="builder">The Umbraco builder to configure.</param>
    /// <returns>The same <paramref name="builder"/> instance, to allow for method chaining.</returns>
    public static IUmbracoBuilder SetServerRegistrar<T>(this IUmbracoBuilder builder)
        where T : class, IServerRoleAccessor
    {
        builder.Services.AddUnique<IServerRoleAccessor, T>();
        return builder;
    }

    /// <summary>
    ///     Configures the <see cref="IServerRoleAccessor"/> implementation used by the Umbraco builder.
    /// </summary>
    /// <param name="builder">The <see cref="IUmbracoBuilder"/> to configure.</param>
    /// <param name="factory">A factory function that creates an <see cref="IServerRoleAccessor"/> instance using the provided <see cref="IServiceProvider"/>.</param>
    /// <returns>The configured <see cref="IUmbracoBuilder"/> instance.</returns>
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
    /// <returns>The builder.</returns>
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
    ///     Configures the <see cref="IServerMessenger"/> implementation used by the application by registering a factory method.
    /// </summary>
    /// <param name="builder">The <see cref="IUmbracoBuilder"/> used to configure services.</param>
    /// <param name="factory">A factory function that creates an <see cref="IServerMessenger"/> instance using the provided <see cref="IServiceProvider"/>.</param>
    /// <returns>The <see cref="IUmbracoBuilder"/> instance for chaining.</returns>
    public static IUmbracoBuilder SetServerMessenger(
        this IUmbracoBuilder builder,
        Func<IServiceProvider, IServerMessenger> factory)
    {
        builder.Services.AddUnique(factory);
        return builder;
    }

    /// <summary>
    ///     Registers the specified <see cref="IServerMessenger"/> implementation with the Umbraco builder.
    /// </summary>
    /// <param name="builder">The <see cref="IUmbracoBuilder"/> used to configure services.</param>
    /// <param name="registrar">The <see cref="IServerMessenger"/> implementation to register.</param>
    /// <returns>The <see cref="IUmbracoBuilder"/> instance for chaining.</returns>
    public static IUmbracoBuilder SetServerMessenger(this IUmbracoBuilder builder, IServerMessenger registrar)
    {
        builder.Services.AddUnique(registrar);
        return builder;
    }
}
