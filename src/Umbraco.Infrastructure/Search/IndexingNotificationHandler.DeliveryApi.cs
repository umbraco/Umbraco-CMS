using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Infrastructure.Search;

internal sealed class DeliveryApiContentIndexingNotificationHandler :
    INotificationHandler<ContentCacheRefresherNotification>, INotificationHandler<ContentTypeCacheRefresherNotification>
{
    private readonly IDeliveryApiIndexingHandler _deliveryApiIndexingHandler;

    public DeliveryApiContentIndexingNotificationHandler(IDeliveryApiIndexingHandler deliveryApiIndexingHandler)
        => _deliveryApiIndexingHandler = deliveryApiIndexingHandler;

    public void Handle(ContentCacheRefresherNotification notification)
    {
        if (NotificationHandlingIsDisabled())
        {
            return;
        }

        ContentCacheRefresher.JsonPayload[] payloads = GetNotificationPayloads<ContentCacheRefresher.JsonPayload>(notification);

        var changesById = payloads
            .Select(payload => new KeyValuePair<int, TreeChangeTypes>(payload.Id, payload.ChangeTypes))
            .ToList();

        _deliveryApiIndexingHandler.HandleContentChanges(changesById);
    }

    public void Handle(ContentTypeCacheRefresherNotification notification)
    {
        if (NotificationHandlingIsDisabled())
        {
            return;
        }

        ContentTypeCacheRefresher.JsonPayload[] payloads = GetNotificationPayloads<ContentTypeCacheRefresher.JsonPayload>(notification);

        var contentTypeChangesById = payloads
            .Where(payload => payload.ItemType == nameof(IContentType))
            .Select(payload => new KeyValuePair<int, ContentTypeChangeTypes>(payload.Id, payload.ChangeTypes))
            .ToList();
        _deliveryApiIndexingHandler.HandleContentTypeChanges(contentTypeChangesById);
    }

    private bool NotificationHandlingIsDisabled()
    {
        if (_deliveryApiIndexingHandler.Enabled == false)
        {
            return true;
        }

        if (Suspendable.ExamineEvents.CanIndex == false)
        {
            return true;
        }

        return false;
    }

    private T[] GetNotificationPayloads<T>(CacheRefresherNotification notification)
    {
        if (notification.MessageType != MessageType.RefreshByPayload || notification.MessageObject is not T[] payloads)
        {
            throw new NotSupportedException();
        }

        return payloads;
    }
}
