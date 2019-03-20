﻿using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Core.Sync;
using Umbraco.Web.Cache;
using Current = Umbraco.Web.Composing.Current;

namespace Umbraco.Web.Routing
{
    /// Implements an Application Event Handler for managing redirect urls tracking.
    /// <para>when content is renamed or moved, we want to create a permanent 301 redirect from it's old url</para>
    /// <para>
    ///     not managing domains because we don't know how to do it - changing domains => must create a higher level
    ///     strategy using rewriting rules probably
    /// </para>
    /// <para>recycle bin = moving to and from does nothing: to = the node is gone, where would we redirect? from = same</para>
    public sealed class RedirectTrackingComponent : IComponent
    {
        private const string ContextKey1 = "Umbraco.Web.Redirects.RedirectTrackingEventHandler.1";
        private const string ContextKey2 = "Umbraco.Web.Redirects.RedirectTrackingEventHandler.2";
        private const string ContextKey3 = "Umbraco.Web.Redirects.RedirectTrackingEventHandler.3";

        private readonly IUmbracoSettingsSection _umbracoSettings;

        public RedirectTrackingComponent(IUmbracoSettingsSection umbracoSettings)
        {
            _umbracoSettings = umbracoSettings;
        }

        private static Dictionary<ContentIdAndCulture, ContentKeyAndOldRoute> OldRoutes
        {
            get
            {
                var oldRoutes = (Dictionary<ContentIdAndCulture, ContentKeyAndOldRoute>) Current.UmbracoContext.HttpContext.Items[ContextKey3];
                if (oldRoutes == null)
                    Current.UmbracoContext.HttpContext.Items[ContextKey3] = oldRoutes = new Dictionary<ContentIdAndCulture, ContentKeyAndOldRoute>();
                return oldRoutes;
            }
        }

        private static bool HasOldRoutes
        {
            get
            {
                if (Current.UmbracoContext == null) return false;
                if (Current.UmbracoContext.HttpContext == null) return false;
                if (Current.UmbracoContext.HttpContext.Items[ContextKey3] == null) return false;
                return true;
            }
        }

        private static bool LockedEvents
        {
            get => Moving && Current.UmbracoContext.HttpContext.Items[ContextKey2] != null;
            set
            {
                if (Moving && value)
                    Current.UmbracoContext.HttpContext.Items[ContextKey2] = true;
                else
                    Current.UmbracoContext.HttpContext.Items.Remove(ContextKey2);
            }
        }

        private static bool Moving
        {
            get => Current.UmbracoContext.HttpContext.Items[ContextKey1] != null;
            set
            {
                if (value)
                    Current.UmbracoContext.HttpContext.Items[ContextKey1] = true;
                else
                {
                    Current.UmbracoContext.HttpContext.Items.Remove(ContextKey1);
                    Current.UmbracoContext.HttpContext.Items.Remove(ContextKey2);
                }
            }
        }

        public void Initialize()
        {
            // don't let the event handlers kick in if Redirect Tracking is turned off in the config
            if (_umbracoSettings.WebRouting.DisableRedirectUrlTracking) return;

            // events are weird
            // on 'published' we 'could' get the old or the new route depending on event handlers order
            // so it is not reliable. getting the old route in 'publishing' to be sure and storing in http
            // context. then for the same reason, we have to process these old items only when the cache
            // is ready
            // when moving, the moved node is also published, which is causing all sorts of troubles with
            // descendants, so when moving, we lock events so that neither 'published' nor 'publishing'
            // are processed more than once
            // we cannot rely only on ContentCacheRefresher because when CacheUpdated triggers the old
            // route is gone
            //
            // this is all very weird but it seems to work

            ContentService.Publishing += ContentService_Publishing;
            ContentService.Published += ContentService_Published;
            ContentService.Moving += ContentService_Moving;
            ContentService.Moved += ContentService_Moved;

            ContentCacheRefresher.CacheUpdated += ContentCacheRefresher_CacheUpdated;

            // kill all redirects once a content is deleted
            //ContentService.Deleted += ContentService_Deleted;
            // BUT, doing it here would prevent content deletion due to FK
            // so the rows are actually deleted by the ContentRepository (see GetDeleteClauses)

            // rolled back items have to be published, so publishing will take care of that
        }

        public void Terminate()
        { }

        private static void ContentCacheRefresher_CacheUpdated(ContentCacheRefresher sender, CacheRefresherEventArgs args)
        {
            // that event is a distributed even that triggers on all nodes
            // BUT it should totally NOT run on nodes other that the one that handled the other events
            // and besides, it cannot run on a background thread!
            if (!HasOldRoutes)
                return;

            // sanity checks
            if (args.MessageType != MessageType.RefreshByPayload)
            {
                throw new InvalidOperationException("ContentCacheRefresher MessageType should be ByPayload.");
            }

            if (args.MessageObject == null)
            {
                return;
            }

            if (!(args.MessageObject is ContentCacheRefresher.JsonPayload[]))
            {
                throw new InvalidOperationException("ContentCacheRefresher MessageObject should be JsonPayload[].");
            }

            // manage routes
            var removeKeys = new List<ContentIdAndCulture>();

            foreach (var oldRoute in OldRoutes)
            {
                // assuming we cannot have 'CacheUpdated' for only part of the infos else we'd need
                // to set a flag in 'Published' to indicate which entities have been refreshed ok
                CreateRedirect(oldRoute.Key.ContentId, oldRoute.Key.Culture, oldRoute.Value.ContentKey, oldRoute.Value.OldRoute);
                removeKeys.Add(oldRoute.Key);
            }

            foreach (var k in removeKeys)
            {
                OldRoutes.Remove(k);
            }
        }

        private static void ContentService_Publishing(IContentService sender, PublishEventArgs<IContent> args)
        {
            if (LockedEvents) return;

            var contentCache = Current.UmbracoContext.ContentCache;
            foreach (var entity in args.PublishedEntities)
            {
                var entityContent = contentCache.GetById(entity.Id);
                if (entityContent == null) continue;

                // get the default affected cultures by going up the tree until we find the first culture variant entity (default to no cultures) 
                var defaultCultures = entityContent.AncestorsOrSelf()?.FirstOrDefault(a => a.Cultures.Any())?.Cultures.Select(c => c.Key).ToArray()
                    ?? new[] {(string) null};
                foreach (var x in entityContent.DescendantsOrSelf())
                {
                    // if this entity defines specific cultures, use those instead of the default ones
                    var cultures = x.Cultures.Any() ? x.Cultures.Select(c => c.Key) : defaultCultures;

                    foreach (var culture in cultures)
                    {
                        var route = contentCache.GetRouteById(x.Id, culture);
                        if (IsNotRoute(route)) return;
                        OldRoutes[new ContentIdAndCulture(x.Id, culture)] = new ContentKeyAndOldRoute(x.Key, route);
                    }
                }
            }

            LockedEvents = true; // we only want to see the "first batch"
        }

        private static void ContentService_Published(IContentService sender, PublishEventArgs<IContent> e)
        {
            // look note in CacheUpdated
            // we might want to set a flag on the entities we are seeing here
        }

        private static void ContentService_Moving(IContentService sender, MoveEventArgs<IContent> e)
        {
            // TODO: Use the new e.EventState to track state between Moving/Moved events!
            Moving = true;
        }

        private static void ContentService_Moved(IContentService sender, MoveEventArgs<IContent> e)
        {
            Moving = false;
            LockedEvents = false;
        }

        private static void CreateRedirect(int contentId, string culture, Guid contentKey, string oldRoute)
        {
            var contentCache = Current.UmbracoContext.ContentCache;
            var newRoute = contentCache.GetRouteById(contentId, culture);
            if (IsNotRoute(newRoute) || oldRoute == newRoute) return;
            var redirectUrlService = Current.Services.RedirectUrlService;
            redirectUrlService.Register(oldRoute, contentKey, culture);
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
    }
}
