using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Infrastructure.Search;

internal sealed class DeliveryApiContentIndexingNotificationHandler :
    INotificationHandler<ContentCacheRefresherNotification>,
    INotificationHandler<ContentTypeCacheRefresherNotification>,
    INotificationHandler<PublicAccessCacheRefresherNotification>
{
    private readonly IDeliveryApiIndexingHandler _deliveryApiIndexingHandler;
    private readonly ILogger<DeliveryApiContentIndexingNotificationHandler> _logger;
    private readonly DeliveryApiSettings _deliveryApiSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeliveryApiContentIndexingNotificationHandler"/> class, which handles notifications related to content indexing for the Delivery API.
    /// </summary>
    /// <param name="deliveryApiIndexingHandler">An instance responsible for handling Delivery API content indexing operations.</param>
    /// <param name="logger">The logger used for logging events and errors related to content indexing notifications.</param>
    /// <param name="deliveryApiSettings">The monitor providing access to current Delivery API settings.</param>
    public DeliveryApiContentIndexingNotificationHandler(
        IDeliveryApiIndexingHandler deliveryApiIndexingHandler,
        ILogger<DeliveryApiContentIndexingNotificationHandler> logger,
        IOptionsMonitor<DeliveryApiSettings> deliveryApiSettings)
    {
        _deliveryApiIndexingHandler = deliveryApiIndexingHandler;
        _logger = logger;
        _deliveryApiSettings = deliveryApiSettings.CurrentValue;
    }

    /// <summary>
    /// Handles a <see cref="ContentCacheRefresherNotification"/> by extracting content change information
    /// and delegating the changes to the delivery API indexing handler for processing.
    /// </summary>
    /// <param name="notification">The notification containing information about content cache changes.</param>
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

    /// <summary>
    /// Handles a <see cref="ContentCacheRefresherNotification"/> by processing its payloads and forwarding content change information
    /// to the delivery API indexing handler for further processing.
    /// </summary>
    /// <param name="notification">The content cache refresher notification containing information about content changes.</param>
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

    /// <summary>
    /// Handles a <see cref="PublicAccessCacheRefresherNotification"/> by extracting content change payloads and passing them to the delivery API indexing handler for processing.
    /// </summary>
    /// <param name="notification">The notification containing information about content cache changes.</param>
    public void Handle(PublicAccessCacheRefresherNotification notification)
        => _deliveryApiIndexingHandler.HandlePublicAccessChanges();

    private bool NotificationHandlingIsDisabled()
    {
        if (_deliveryApiSettings.Enabled is false)
        {
            // using debug logging here since this happens on every content cache refresh and we don't want to flood the log
            _logger.LogDebug("Delivery API index notification handling is suspended while the Delivery API is disabled.");
            return true;
        }

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
