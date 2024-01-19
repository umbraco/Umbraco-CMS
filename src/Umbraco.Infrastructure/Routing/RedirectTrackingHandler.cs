// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Routing;

/// Implements a notification handler for managing redirect URLs tracking.
/// <para>when content is renamed or moved, we want to create a permanent 301 redirect from it's old URL</para>
/// <para>
///     not managing domains because we don't know how to do it - changing domains => must create a higher level
///     strategy using rewriting rules probably
/// </para>
/// <para>recycle bin = moving to and from does nothing: to = the node is gone, where would we redirect? from = same</para>
public sealed class RedirectTrackingHandler :
    INotificationHandler<ContentPublishingNotification>,
    INotificationHandler<ContentPublishedNotification>,
    INotificationHandler<ContentMovingNotification>,
    INotificationHandler<ContentMovedNotification>
{
    private const string NotificationStateKey = "Umbraco.Cms.Core.Routing.RedirectTrackingHandler";

    private readonly IOptionsMonitor<WebRoutingSettings> _webRoutingSettings;
    private readonly IRedirectTracker _redirectTracker;

    public RedirectTrackingHandler(
        IOptionsMonitor<WebRoutingSettings> webRoutingSettings,
        IRedirectTracker redirectTracker)
    {
        _webRoutingSettings = webRoutingSettings;
        _redirectTracker = redirectTracker;
    }

    public void Handle(ContentMovedNotification notification) => CreateRedirectsForOldRoutes(notification);

    public void Handle(ContentMovingNotification notification) =>
        StoreOldRoutes(notification.MoveInfoCollection.Select(m => m.Entity), notification);

    public void Handle(ContentPublishedNotification notification) => CreateRedirectsForOldRoutes(notification);

    public void Handle(ContentPublishingNotification notification) =>
        StoreOldRoutes(notification.PublishedEntities, notification);

    private void StoreOldRoutes(IEnumerable<IContent> entities, IStatefulNotification notification)
    {
        // Don't let the notification handlers kick in if redirect tracking is turned off in the config.
        if (_webRoutingSettings.CurrentValue.DisableRedirectUrlTracking)
        {
            return;
        }

        Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)> oldRoutes = GetOldRoutes(notification);
        foreach (IContent entity in entities)
        {
            _redirectTracker.StoreOldRoute(entity, oldRoutes);
        }
    }

    private void CreateRedirectsForOldRoutes(IStatefulNotification notification)
    {
        // Don't let the notification handlers kick in if redirect tracking is turned off in the config.
        if (_webRoutingSettings.CurrentValue.DisableRedirectUrlTracking)
        {
            return;
        }

        Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)> oldRoutes = GetOldRoutes(notification);
        _redirectTracker.CreateRedirects(oldRoutes);
    }

    private Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)> GetOldRoutes(IStatefulNotification notification)
    {
        if (notification.State.ContainsKey(NotificationStateKey) == false)
        {
            notification.State[NotificationStateKey] = new Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)>();
        }

        return (Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)>?)notification.State[NotificationStateKey]!;
    }
}
