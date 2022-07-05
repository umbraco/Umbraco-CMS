// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Routing
{
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
        private readonly ILogger<RedirectTrackingHandler> _logger;
        private readonly IOptionsMonitor<WebRoutingSettings> _webRoutingSettings;
        private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
        private readonly IRedirectUrlService _redirectUrlService;
        private readonly IVariationContextAccessor _variationContextAccessor;

        private const string NotificationStateKey = "Umbraco.Cms.Core.Routing.RedirectTrackingHandler";

        public RedirectTrackingHandler(
            ILogger<RedirectTrackingHandler> logger,
            IOptionsMonitor<WebRoutingSettings> webRoutingSettings,
            IPublishedSnapshotAccessor publishedSnapshotAccessor,
            IRedirectUrlService redirectUrlService,
            IVariationContextAccessor variationContextAccessor)
        {
            _logger = logger;
            _webRoutingSettings = webRoutingSettings;
            _publishedSnapshotAccessor = publishedSnapshotAccessor;
            _redirectUrlService = redirectUrlService;
            _variationContextAccessor = variationContextAccessor;
        }

        public void Handle(ContentPublishingNotification notification) => StoreOldRoutes(notification.PublishedEntities, notification);

        public void Handle(ContentPublishedNotification notification) => CreateRedirectsForOldRoutes(notification);

        public void Handle(ContentMovingNotification notification) => StoreOldRoutes(notification.MoveInfoCollection.Select(m => m.Entity), notification);

        public void Handle(ContentMovedNotification notification) => CreateRedirectsForOldRoutes(notification);

        private void StoreOldRoutes(IEnumerable<IContent> entities, IStatefulNotification notification)
        {
            // don't let the notification handlers kick in if Redirect Tracking is turned off in the config
            if (_webRoutingSettings.CurrentValue.DisableRedirectUrlTracking)
                return;

            var oldRoutes = GetOldRoutes(notification);
            foreach (var entity in entities)
            {
                StoreOldRoute(entity, oldRoutes);
            }
        }

        private void CreateRedirectsForOldRoutes(IStatefulNotification notification)
        {
            // don't let the notification handlers kick in if Redirect Tracking is turned off in the config
            if (_webRoutingSettings.CurrentValue.DisableRedirectUrlTracking)
                return;

            var oldRoutes = GetOldRoutes(notification);
            CreateRedirects(oldRoutes);
        }

        private OldRoutesDictionary GetOldRoutes(IStatefulNotification notification)
        {
            if (notification.State.ContainsKey(NotificationStateKey) == false)
            {
                notification.State[NotificationStateKey] = new OldRoutesDictionary();
            }

            return (OldRoutesDictionary?)notification.State[NotificationStateKey] ?? new OldRoutesDictionary();
        }

        private void StoreOldRoute(IContent entity, OldRoutesDictionary oldRoutes)
        {
            if (!_publishedSnapshotAccessor.TryGetPublishedSnapshot(out var publishedSnapshot))
            {
                return;
            }

            IPublishedContentCache? contentCache = publishedSnapshot?.Content;
            IPublishedContent? entityContent = contentCache?.GetById(entity.Id);
            if (entityContent is null)
            {
                return;
            }

            // get the default affected cultures by going up the tree until we find the first culture variant entity (default to no cultures)
            var defaultCultures = entityContent.AncestorsOrSelf()?.FirstOrDefault(a => a.Cultures.Any())?.Cultures.Keys.ToArray()
                ?? Array.Empty<string>();

            foreach (IPublishedContent publishedContent in entityContent.DescendantsOrSelf(_variationContextAccessor))
            {
                // if this entity defines specific cultures, use those instead of the default ones
                IEnumerable<string> cultures = publishedContent.Cultures.Any() ? publishedContent.Cultures.Keys : defaultCultures;

                foreach (var culture in cultures)
                {
                    var route = contentCache?.GetRouteById(publishedContent.Id, culture);
                    if (IsNotRoute(route))
                    {
                        continue;
                    }

                    oldRoutes[new ContentIdAndCulture(publishedContent.Id, culture)] = new ContentKeyAndOldRoute(publishedContent.Key, route!);
                }
            }
        }

        private void CreateRedirects(OldRoutesDictionary oldRoutes)
        {
            if (!_publishedSnapshotAccessor.TryGetPublishedSnapshot(out var publishedSnapshot))
            {
                return;
            }

            var contentCache = publishedSnapshot?.Content;

            if (contentCache == null)
            {
                _logger.LogWarning("Could not track redirects because there is no current published snapshot available.");
                return;
            }

            foreach (KeyValuePair<ContentIdAndCulture, ContentKeyAndOldRoute> oldRoute in oldRoutes)
            {
                var newRoute = contentCache.GetRouteById(oldRoute.Key.ContentId, oldRoute.Key.Culture);
                if (IsNotRoute(newRoute) || oldRoute.Value.OldRoute == newRoute)
                {
                    continue;
                }

                _redirectUrlService.Register(oldRoute.Value.OldRoute, oldRoute.Value.ContentKey, oldRoute.Key.Culture);
            }
        }

        private static bool IsNotRoute(string? route)
        {
            // null if content not found
            // err/- if collision or anomaly or ...
            return route == null || route.StartsWith("err/");
        }

        private class ContentIdAndCulture : Tuple<int, string>
        {
            public ContentIdAndCulture(int contentId, string culture) : base(contentId, culture)
            {
            }

            public int ContentId => Item1;
            public string Culture => Item2;
        }

        private class ContentKeyAndOldRoute : Tuple<Guid, string>
        {
            public ContentKeyAndOldRoute(Guid contentKey, string oldRoute) : base(contentKey, oldRoute)
            {
            }

            public Guid ContentKey => Item1;
            public string OldRoute => Item2;
        }

        private class OldRoutesDictionary : Dictionary<ContentIdAndCulture, ContentKeyAndOldRoute>
        {

        }
    }
}
