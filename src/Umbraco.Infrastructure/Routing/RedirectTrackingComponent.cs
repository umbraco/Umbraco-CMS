using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Services.Implement;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Routing
{
    /// Implements an Application Event Handler for managing redirect URLs tracking.
    /// <para>when content is renamed or moved, we want to create a permanent 301 redirect from it's old URL</para>
    /// <para>
    ///     not managing domains because we don't know how to do it - changing domains => must create a higher level
    ///     strategy using rewriting rules probably
    /// </para>
    /// <para>recycle bin = moving to and from does nothing: to = the node is gone, where would we redirect? from = same</para>
    public sealed class RedirectTrackingComponent : IComponent
    {
        private const string _eventStateKey = "Umbraco.Web.Redirects.RedirectTrackingEventHandler";

        private readonly IOptionsMonitor<WebRoutingSettings> _webRoutingSettings;
        private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
        private readonly IRedirectUrlService _redirectUrlService;
        private readonly IVariationContextAccessor _variationContextAccessor;

        public RedirectTrackingComponent(IOptionsMonitor<WebRoutingSettings> webRoutingSettings, IPublishedSnapshotAccessor publishedSnapshotAccessor, IRedirectUrlService redirectUrlService, IVariationContextAccessor variationContextAccessor)
        {
            _webRoutingSettings = webRoutingSettings;
            _publishedSnapshotAccessor = publishedSnapshotAccessor;
            _redirectUrlService = redirectUrlService;
            _variationContextAccessor = variationContextAccessor;
        }

        public void Initialize()
        {
            ContentService.Publishing += ContentService_Publishing;
            ContentService.Published += ContentService_Published;
            ContentService.Moving += ContentService_Moving;
            ContentService.Moved += ContentService_Moved;

            // kill all redirects once a content is deleted
            //ContentService.Deleted += ContentService_Deleted;
            // BUT, doing it here would prevent content deletion due to FK
            // so the rows are actually deleted by the ContentRepository (see GetDeleteClauses)

            // rolled back items have to be published, so publishing will take care of that
        }

        public void Terminate()
        {
            ContentService.Publishing -= ContentService_Publishing;
            ContentService.Published -= ContentService_Published;
            ContentService.Moving -= ContentService_Moving;
            ContentService.Moved -= ContentService_Moved;
        }

        private void ContentService_Publishing(IContentService sender, PublishEventArgs<IContent> args)
        {
            // don't let the event handlers kick in if Redirect Tracking is turned off in the config
            if (_webRoutingSettings.CurrentValue.DisableRedirectUrlTracking) return;

            var oldRoutes = GetOldRoutes(args.EventState);
            foreach (var entity in args.PublishedEntities)
            {
                StoreOldRoute(entity, oldRoutes);
            }
        }

        private void ContentService_Published(IContentService sender, ContentPublishedEventArgs args)
        {
            // don't let the event handlers kick in if Redirect Tracking is turned off in the config
            if (_webRoutingSettings.CurrentValue.DisableRedirectUrlTracking) return;

            var oldRoutes = GetOldRoutes(args.EventState);
            CreateRedirects(oldRoutes);
        }

        private void ContentService_Moving(IContentService sender, MoveEventArgs<IContent> args)
        {
            // don't let the event handlers kick in if Redirect Tracking is turned off in the config
            if (_webRoutingSettings.CurrentValue.DisableRedirectUrlTracking) return;

            var oldRoutes = GetOldRoutes(args.EventState);
            foreach (var info in args.MoveInfoCollection)
            {
                StoreOldRoute(info.Entity, oldRoutes);
            }
        }

        private void ContentService_Moved(IContentService sender, MoveEventArgs<IContent> args)
        {
            // don't let the event handlers kick in if Redirect Tracking is turned off in the config
            if (_webRoutingSettings.CurrentValue.DisableRedirectUrlTracking) return;

            var oldRoutes = GetOldRoutes(args.EventState);
            CreateRedirects(oldRoutes);
        }

        private OldRoutesDictionary GetOldRoutes(IDictionary<string, object> eventState)
        {
            if (! eventState.ContainsKey(_eventStateKey))
            {
                eventState[_eventStateKey] = new OldRoutesDictionary();
            }

            return eventState[_eventStateKey] as OldRoutesDictionary;
        }

        private void StoreOldRoute(IContent entity, OldRoutesDictionary oldRoutes)
        {
            var contentCache = _publishedSnapshotAccessor.PublishedSnapshot.Content;
            var entityContent = contentCache.GetById(entity.Id);
            if (entityContent == null) return;

            // get the default affected cultures by going up the tree until we find the first culture variant entity (default to no cultures)
            var defaultCultures = entityContent.AncestorsOrSelf()?.FirstOrDefault(a => a.Cultures.Any())?.Cultures.Keys.ToArray()
                ?? new[] { (string)null };
            foreach (var x in entityContent.DescendantsOrSelf(_variationContextAccessor))
            {
                // if this entity defines specific cultures, use those instead of the default ones
                var cultures = x.Cultures.Any() ? x.Cultures.Keys : defaultCultures;

                foreach (var culture in cultures)
                {
                    var route = contentCache.GetRouteById(x.Id, culture);
                    if (IsNotRoute(route)) continue;
                    oldRoutes[new ContentIdAndCulture(x.Id, culture)] = new ContentKeyAndOldRoute(x.Key, route);
                }
            }
        }

        private void CreateRedirects(OldRoutesDictionary oldRoutes)
        {
            var contentCache = _publishedSnapshotAccessor.PublishedSnapshot.Content;

            foreach (var oldRoute in oldRoutes)
            {
                var newRoute = contentCache.GetRouteById(oldRoute.Key.ContentId, oldRoute.Key.Culture);
                if (IsNotRoute(newRoute) || oldRoute.Value.OldRoute == newRoute) continue;
                _redirectUrlService.Register(oldRoute.Value.OldRoute, oldRoute.Value.ContentKey, oldRoute.Key.Culture);
            }
        }

        private static bool IsNotRoute(string route)
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
        { }
    }
}
