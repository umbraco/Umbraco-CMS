using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Core.Sync;
using Umbraco.Web.Cache;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Implements an Application Event Handler for managing redirect urls tracking.
    /// </summary>
    /// <remarks>
    /// <para>when content is renamed or moved, we want to create a permanent 301 redirect from it's old url</para>
    /// <para>not managing domains because we don't know how to do it - changing domains => must create a higher level strategy using rewriting rules probably</para>
    /// <para>recycle bin = moving to and from does nothing: to = the node is gone, where would we redirect? from = same</para>
    /// </remarks>
    public class RedirectTrackingEventHandler : ApplicationEventHandler
    {
        private const string ContextKey1 = "Umbraco.Web.Routing.RedirectTrackingEventHandler.1";
        private const string ContextKey2 = "Umbraco.Web.Routing.RedirectTrackingEventHandler.2";
        private const string ContextKey3 = "Umbraco.Web.Routing.RedirectTrackingEventHandler.3";

        /// <inheritdoc />
        protected override void ApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            if (UmbracoConfig.For.UmbracoSettings().WebRouting.DisableRedirectUrlTracking)
            {
                ContentFinderResolver.Current.RemoveType<ContentFinderByRedirectUrl>();
            }
            else
            {
                // if any of these dlls are loaded we don't want to run our finder
                var dlls = new[]
                {
                    "InfoCaster.Umbraco.UrlTracker",
                    "SEOChecker",
                    "Simple301",
                    "Terabyte.Umbraco.Modules.PermanentRedirect",
                    "CMUmbracoTools",
                    "PWUrlRedirect"
                };

                // assuming all assemblies have been loaded already
                // check if any of them matches one of the above dlls
                var found = AppDomain.CurrentDomain.GetAssemblies()
                    .Select(x => x.FullName.Split(',')[0])
                    .Any(x => dlls.Contains(x));
                if (found)
                    ContentFinderResolver.Current.RemoveType<ContentFinderByRedirectUrl>();
            }
        }

        /// <inheritdoc />
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            // don't let the event handlers kick in if Redirect Tracking is turned off in the config
            if (UmbracoConfig.For.UmbracoSettings().WebRouting.DisableRedirectUrlTracking) return;

            // events are weird
            // on 'published' we 'could' get the old or the new route depending on event handlers order
            // so it is not reliable. getting the old route in 'publishing' to be sure and storing in http
            // context. then for the same reason, we have to process these old items only when the cache
            // is ready
            // when moving, the moved node is also published, which is causing all sorts of troubles with
            // descendants, so when moving, we lock events so that neither 'published' nor 'publishing'
            // are processed more than once
            //
            // this is all verrrry weird but it seems to work

            ContentService.Publishing += ContentService_Publishing;
            ContentService.Published += ContentService_Published;
            ContentService.Moving += ContentService_Moving;
            ContentService.Moved += ContentService_Moved;
            PageCacheRefresher.CacheUpdated += PageCacheRefresher_CacheUpdated;

            // kill all redirects once a content is deleted
            //ContentService.Deleted += ContentService_Deleted;
            // BUT, doing it here would prevent content deletion due to FK
            // so the rows are actually deleted by the ContentRepository (see GetDeleteClauses)

            // rolled back items have to be published, so publishing will take care of that
        }

        /// <summary>
        /// Tracks a documents URLs during publishing in the current request
        /// </summary>
        private static Dictionary<int, Tuple<Guid, string>> OldRoutes
        {
            get
            {
                var oldRoutes = RequestCache.GetCacheItem<Dictionary<int, Tuple<Guid, string>>>(
                    ContextKey3,
                    () => new Dictionary<int, Tuple<Guid, string>>());
                return oldRoutes;
            }
        }

        private static bool LockedEvents
        {
            get
            {
                return Moving && RequestCache.GetCacheItem(ContextKey2) != null;
            }
            set
            {
                if (Moving && value)
                {
                    //this forces true into the cache
                    RequestCache.GetCacheItem(ContextKey2, () => true);
                }
                else
                {
                    RequestCache.ClearCacheItem(ContextKey2);
                }
            }
        }

        private static bool Moving
        {
            get { return RequestCache.GetCacheItem(ContextKey1) != null; }
            set
            {
                if (value)
                {
                    //this forces true into the cache
                    RequestCache.GetCacheItem(ContextKey1, () => true);
                }
                else
                {
                    RequestCache.ClearCacheItem(ContextKey1);
                    RequestCache.ClearCacheItem(ContextKey2);
                }
            }
        }

        /// <summary>
        /// Before the items are published, we need to get it's current URL before it changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void ContentService_Publishing(IPublishingStrategy sender, PublishEventArgs<IContent> args)
        {
            if (LockedEvents) return;

            var contentCache = GetPublishedCache();
            if (contentCache == null) return;

            // prepare entities
            var entities = PrepareEntities(args.PublishedEntities);

            foreach (var entity in entities)
            {
                // for each entity, we want to save the 'old route' of any impacted entity.
                //
                // previously, we'd save the routes of all descendants - super safe but has an
                // impact on perfs - assuming that the descendant routes will NOT change if the
                // entity's segment does not change (else... outside of the scope of the simple,
                // built -in, tracker) then we can compare the entity's old and new segments
                // and avoid processing the descendants

                var process = true;
                if (Moving == false) // always process descendants when moving
                {
                    // SD: in 7.5.0 we re-lookup the entity that is published, which gets its
                    // current state in the DB, which we use to get the 'old' segment. In the
                    // future this will certainly cause some problems, to fix this we'd need to
                    // change the IUrlSegmentProvider to support being able to determine if a
                    // segment is going to change for an entity. See notes in IUrlSegmentProvider.

                    var oldEntity = ApplicationContext.Current.Services.ContentService.GetById(entity.Id);
                    if (oldEntity == null) continue;
                    var oldSegment = oldEntity.GetUrlSegment();
                    var newSegment = entity.GetUrlSegment();
                    process = oldSegment != newSegment;
                }

                // skip if no segment change
                if (process == false) continue;

                // else save routes for all descendants
                var entityContent = contentCache.GetById(entity.Id);
                if (entityContent == null) continue;

                foreach (var x in entityContent.DescendantsOrSelf())
                {
                    if (IsNotRoute(x.Url)) continue;
                    var wk = UnwrapToKey(x);
                    if (wk == null) continue;

                    OldRoutes[x.Id] = Tuple.Create(wk.Key, x.Url);
                }
            }

            LockedEvents = true; // we only want to see the "first batch"
        }

        private static IEnumerable<IContent> PrepareEntities(IEnumerable<IContent> eventEntities)
        {
            // prepare entities
            // - exclude entities without an identity (new entities)
            // - exclude duplicates (in case publishing a parent and its children)

            var entities = new List<IContent>();
            foreach (var e in eventEntities.Where(x => x.HasIdentity).OrderBy(x => x.Level))
            {
                var pathIds = e.Path.Split(',').Select(int.Parse);
                if (entities.Any(x => pathIds.Contains(x.Id))) continue;
                entities.Add(e);
            }
            return entities;
        }

        private static IPublishedContentWithKey UnwrapToKey(IPublishedContent content)
        {
            if (content == null) return null;
            var withKey = content as IPublishedContentWithKey;
            if (withKey != null) return withKey;

            var extended = content as PublishedContentExtended;
            while (extended != null)
                extended = (content = extended.Unwrap()) as PublishedContentExtended;

            withKey = content as IPublishedContentWithKey;
            return withKey;
        }

        /// <summary>
        /// Executed when the cache updates, which means we can know what the new URL is for a given document
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="cacheRefresherEventArgs"></param>
        private void PageCacheRefresher_CacheUpdated(PageCacheRefresher sender, CacheRefresherEventArgs cacheRefresherEventArgs)
        {
            // only on master / single, not on replicas!
            if (IsReplicaServer) return;

            // simply getting OldRoutes will register it in the request cache,
            // so whatever we do with it, try/finally it to ensure it's cleared

            try
            {
                foreach (var oldRoute in OldRoutes)
                {
                    // assuming we cannot have 'CacheUpdated' for only part of the infos else we'd need
                    // to set a flag in 'Published' to indicate which entities have been refreshed ok
                    CreateRedirect(oldRoute.Key, oldRoute.Value.Item1, oldRoute.Value.Item2);
                }
            }
            finally
            {
                OldRoutes.Clear();
                RequestCache.ClearCacheItem(ContextKey3);
            }
        }

        private static void ContentService_Published(IPublishingStrategy sender, PublishEventArgs<IContent> e)
        {
            // look note in CacheUpdated
            // we might want to set a flag on the entities we are seeing here
        }

        private static void ContentService_Moving(IContentService sender, MoveEventArgs<IContent> e)
        {
            //TODO: Use the new e.EventState to track state between Moving/Moved events!
            Moving = true;
        }

        private static void ContentService_Moved(IContentService sender, MoveEventArgs<IContent> e)
        {
            Moving = false;
            LockedEvents = false;
        }

        private static void CreateRedirect(int contentId, Guid contentKey, string oldRoute)
        {

            var contentCache = GetPublishedCache();
            if (contentCache == null) return;

            var newRoute = contentCache.GetRouteById(contentId);
            if (IsNotRoute(newRoute) || oldRoute == newRoute) return;
            var redirectUrlService = ApplicationContext.Current.Services.RedirectUrlService;
            redirectUrlService.Register(oldRoute, contentKey);
        }

        private static bool IsNotRoute(string route)
        {
            // null if content not found
            return route == null;
        }

        // gets a value indicating whether server is 'replica'
        private static bool IsReplicaServer
        {
            get
            {
                var serverRole = ApplicationContext.Current.GetCurrentServerRole();
                return serverRole != ServerRole.Master && serverRole != ServerRole.Single;
            }
        }

        /// <summary>
        /// Gets the current request cache to persist the values between handlers
        /// </summary>
        private static ContextualPublishedContentCache GetPublishedCache()
        {
            return UmbracoContext.Current == null ? null : UmbracoContext.Current.ContentCache;
        }

        /// <summary>
        /// Gets the current request cache to persist the values between handlers
        /// </summary>
        private static ICacheProvider RequestCache
        {
            get { return ApplicationContext.Current.ApplicationCache.RequestCache; }
        }
    }
}
