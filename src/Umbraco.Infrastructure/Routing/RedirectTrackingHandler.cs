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
    public void Handle(ContentMovedNotification notification) => Handle(notification.Yield());

    /// <summary>
    /// Handles a batch of <see cref="ContentMovedNotification"/> by creating redirect entries for the old URLs of the moved content.
    /// </summary>
    /// <param name="notifications">The notifications containing details about the moved content items.</param>
    public void Handle(IEnumerable<ContentMovedNotification> notifications) => CreateRedirectsForOldRoutes(notifications);

    /// <summary>
    /// Handles the content moved notification by creating redirects for old routes when content is moved.
    /// </summary>
    /// <param name="notification">The notification containing information about the moved content.</param>
    public void Handle(ContentMovingNotification notification) => Handle(notification.Yield());

    /// <summary>
    /// Handles a batch of <see cref="ContentMovingNotification"/> by storing the old routes of the content being moved.
    /// </summary>
    /// <param name="notifications">The notifications containing information about the content being moved.</param>
    public void Handle(IEnumerable<ContentMovingNotification> notifications) =>
        StoreOldRoutes(notifications, notification => notification.MoveInfoCollection.Select(m => m.Entity), isMove: true);

    /// <summary>
    /// Handles a <see cref="ContentPublishedNotification"/> to track and manage redirects when content is published.
    /// </summary>
    /// <param name="notification">The notification containing information about the published content.</param>
    public void Handle(ContentPublishedNotification notification) => Handle(notification.Yield());

    /// <summary>
    /// Handles a batch of <see cref="ContentPublishedNotification"/> by creating redirect entries for the old URLs of the published content.
    /// </summary>
    /// <param name="notifications">The notifications containing information about the published content.</param>
    public void Handle(IEnumerable<ContentPublishedNotification> notifications) => CreateRedirectsForOldRoutes(notifications);

    /// <summary>
    /// Handles the content moved notification to create redirects for old routes when content is moved.
    /// </summary>
    /// <param name="notification">The content moved notification.</param>
    public void Handle(ContentPublishingNotification notification) => Handle(notification.Yield());

    /// <summary>
    /// Handles a batch of <see cref="ContentPublishingNotification"/> by storing the old routes of the content being published.
    /// </summary>
    /// <param name="notifications">The notifications containing information about the content being published.</param>
    public void Handle(IEnumerable<ContentPublishingNotification> notifications) =>
        StoreOldRoutes(notifications, notification => notification.PublishedEntities, isMove: false);

    private void StoreOldRoutes<TNotification>(IEnumerable<TNotification> notifications, Func<TNotification, IEnumerable<IContent>> getEntities, bool isMove)
        where TNotification : IStatefulNotification
    {
        // Don't let the notification handlers kick in if redirect tracking is turned off in the config.
        if (_webRoutingSettings.CurrentValue.DisableRedirectUrlTracking)
        {
            return;
        }

        // Share the old routes within the batch, so content already captured by an ancestor's traversal is skipped.
        Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)>? oldRoutes = null;
        foreach (TNotification notification in notifications)
        {
            oldRoutes ??= GetOldRoutes(notification) ?? [];
            notification.State[NotificationStateKey] = oldRoutes;

            foreach (IContent entity in getEntities(notification))
            {
                _redirectTracker.StoreOldRoute(entity, oldRoutes, isMove);
            }
        }
    }

    private void CreateRedirectsForOldRoutes(IEnumerable<IStatefulNotification> notifications)
    {
        // Don't let the notification handlers kick in if redirect tracking is turned off in the config.
        if (_webRoutingSettings.CurrentValue.DisableRedirectUrlTracking)
        {
            return;
        }

        // Notifications can share the same old routes (when stored as a batch), so only create the redirects once.
        IEnumerable<Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)>> storedOldRoutes = notifications
            .Select(GetOldRoutes)
            .OfType<Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)>>()
            .Distinct();

        var oldRoutes = new Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)>();
        foreach (Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)> notificationOldRoutes in storedOldRoutes)
        {
            foreach (KeyValuePair<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)> oldRoute in notificationOldRoutes)
            {
                // Keep the first stored route, which is the one before any of the changes in this batch.
                oldRoutes.TryAdd(oldRoute.Key, oldRoute.Value);
            }
        }

        _redirectTracker.CreateRedirects(oldRoutes);
    }

    private static Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)>? GetOldRoutes(IStatefulNotification notification)
        => notification.State.TryGetValue(NotificationStateKey, out var value)
            ? value as Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)>
            : null;
}
