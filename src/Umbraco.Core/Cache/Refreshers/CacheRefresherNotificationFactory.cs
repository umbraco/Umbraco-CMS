using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Sync;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     A <see cref="ICacheRefresherNotificationFactory" /> that uses ActivatorUtilities to create the
///     <see cref="CacheRefresherNotification" /> instances
/// </summary>
public sealed class CacheRefresherNotificationFactory : ICacheRefresherNotificationFactory
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CacheRefresherNotificationFactory" /> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public CacheRefresherNotificationFactory(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    /// <summary>
    ///     Create a <see cref="CacheRefresherNotification" /> using ActivatorUtilities
    /// </summary>
    /// <typeparam name="TNotification">The <see cref="CacheRefresherNotification" /> to create</typeparam>
    public TNotification Create<TNotification>(object msgObject, MessageType type)
        where TNotification : CacheRefresherNotification
        => _serviceProvider.CreateInstance<TNotification>(msgObject, type);
}
