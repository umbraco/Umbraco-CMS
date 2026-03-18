// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Routing;

/// <summary>
/// Handles notifications to manage tracking of redirect URLs when content is renamed or moved.
/// When content is renamed or moved, this handler creates a permanent 301 redirect from its old URL to the new one.
/// </summary>
/// <remarks>
/// Domain changes are not managed by this handler; changing domains requires a higher-level strategy, such as using URL rewriting rules.
/// Moving content to or from the recycle bin does not create redirects, as the node is either removed or restored without a meaningful previous URL.
/// </remarks>
public sealed class RedirectTrackingHandler :
    INotificationHandler<ContentPublishingNotification>,
    INotificationHandler<ContentPublishedNotification>,
    INotificationHandler<ContentMovingNotification>,
    INotificationHandler<ContentMovedNotification>
{
    private const string NotificationStateKey = "Umbraco.Cms.Core.Routing.RedirectTrackingHandler";

    private readonly IOptionsMonitor<WebRoutingSettings> _webRoutingSettings;
    private readonly IRedirectTracker _redirectTracker;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedirectTrackingHandler"/> class.
    /// </summary>
    /// <param name="webRoutingSettings">An <see cref="IOptionsMonitor{T}"/> for <see cref="WebRoutingSettings"/> that provides access to the current web routing configuration.</param>
    /// <param name="redirectTracker">An <see cref="IRedirectTracker"/> instance used to track and manage redirects.</param>
    public RedirectTrackingHandler(
        IOptionsMonitor<WebRoutingSettings> webRoutingSettings,
        IRedirectTracker redirectTracker)
    {
        _webRoutingSettings = webRoutingSettings;
        _redirectTracker = redirectTracker;
    }

    /// <summary>
    /// Handles a <see cref="ContentMovedNotification"/> by creating redirect entries for the old URLs of the moved content.
    /// This ensures that requests to previous URLs are redirected to the new locations.
    /// </summary>
    /// <param name="notification">The notification containing details about the moved content items.</param>
    public void Handle(ContentMovedNotification notification) => CreateRedirectsForOldRoutes(notification);

    /// <summary>
    /// Handles the content moved notification by creating redirects for old routes when content is moved.
    /// </summary>
    /// <param name="notification">The notification containing information about the moved content.</param>
    public void Handle(ContentMovingNotification notification) =>
        StoreOldRoutes(notification.MoveInfoCollection.Select(m => m.Entity), notification);

    /// <summary>
    /// Handles a <see cref="ContentPublishedNotification"/> to track and manage redirects when content is published.
    /// </summary>
    /// <param name="notification">The notification containing information about the published content.</param>
    public void Handle(ContentPublishedNotification notification) => CreateRedirectsForOldRoutes(notification);

    /// <summary>
    /// Handles the content moved notification to create redirects for old routes when content is moved.
    /// </summary>
    /// <param name="notification">The content moved notification.</param>
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
