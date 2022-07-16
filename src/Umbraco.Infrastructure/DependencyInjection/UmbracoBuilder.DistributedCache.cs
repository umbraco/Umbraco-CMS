using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
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
        builder.Services.AddSingleton<LastSyncedFileManager>();
        builder.Services.AddSingleton<ISyncBootStateAccessor, SyncBootStateAccessor>();
        builder.SetServerMessenger<BatchedDatabaseServerMessenger>();
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
}
