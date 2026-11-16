using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Search.Core.Cache;

namespace Umbraco.Cms.Search.Core.NotificationHandlers;

/// <summary>
/// Provides shared helpers for notification handlers that react to cache refresher notifications by triggering indexing work.
/// </summary>
internal abstract class IndexingNotificationHandlerBase
{
    private readonly ICoreScopeProvider _coreScopeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="IndexingNotificationHandlerBase"/> class.
    /// </summary>
    /// <param name="coreScopeProvider">The scope provider used to defer actions until the ambient scope completes.</param>
    protected IndexingNotificationHandlerBase(ICoreScopeProvider coreScopeProvider)
        => _coreScopeProvider = coreScopeProvider;

    /// <summary>
    /// Extracts the single payload array carried by a cache refresher notification.
    /// </summary>
    /// <typeparam name="T">The payload item type.</typeparam>
    /// <param name="notification">The cache refresher notification.</param>
    /// <param name="origin">The server origin the notification was raised from.</param>
    /// <returns>The notification's payload items.</returns>
    protected T[] GetNotificationPayloads<T>(CacheRefresherNotification notification, out string origin)
    {
        if (notification.MessageType != MessageType.RefreshByPayload
            || notification.MessageObject is not ContentCacheRefresherNotificationPayload<T>[] payloads)
        {
            throw new NotSupportedException($"Expected a cache refresher notification payload type.");
        }

        if (payloads.Length is not 1)
        {
            throw new InvalidOperationException("Expected exactly one cache refresher notification payload.");
        }

        origin = payloads[0].Origin;
        return payloads[0].Payloads;
    }

    /// <summary>
    /// Runs the given action once the ambient scope completes, or immediately if there is no ambient scope.
    /// </summary>
    /// <param name="action">The action to run.</param>
    protected void ExecuteDeferred(Action action)
    {
        var deferredActions = DeferredActions.Get(_coreScopeProvider);
        if (deferredActions != null)
        {
            deferredActions.Add(action);
        }
        else
        {
            action.Invoke();
        }
    }
}
