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
        private const string ContextKey1 = "RedirectTrackingEventHandler.1";
        private const string ContextKey2 = "RedirectTrackingEventHandler.2";
        //private const string ContextKey3 = "RedirectTrackingEventHandler.3";
        private const string ContextKey4 = "RedirectTrackingEventHandler.4";

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

        ///// <summary>
        ///// Tracks a documents URLs during publishing in the current request
        ///// </summary>
        //private static Dictionary<int, Tuple<Guid, string>> OldRoutes
        //{
        //    get
        //    {
        //        var oldRoutes = RequestCache.GetCacheItem<Dictionary<int, Tuple<Guid, string>>>(
        //            ContextKey3, 
        //            () => new Dictionary<int, Tuple<Guid, string>>());
        //        return oldRoutes;
        //    }
        //}

        private class PrePublishedContentContext
        {
            public static PrePublishedContentContext Empty
            {
                get { return new PrePublishedContentContext(null, null, null, null); }
            }
            /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
            public PrePublishedContentContext(IContent entity, string urlSegment, ContextualPublishedContentCache contentCache, Func<IEnumerable<IPublishedContent>> descendentsDelegate)
            {
                if (entity == null) throw new ArgumentNullException("entity");
                if (contentCache == null) throw new ArgumentNullException("contentCache");
                if (descendentsDelegate == null) throw new ArgumentNullException("descendentsDelegate");
                if (string.IsNullOrWhiteSpace(urlSegment)) throw new ArgumentException("Value cannot be null or whitespace.", "urlSegment");
                Entity = entity;
                UrlSegment = urlSegment;
                ContentCache = contentCache;
                DescendentsDelegate = descendentsDelegate;
            }

            public IContent Entity { get; set; }
            public string UrlSegment { get; set; }
            public Func<IEnumerable<IPublishedContent>> DescendentsDelegate { get; set; }
            public ContextualPublishedContentCache ContentCache { get; set; }
        }

        /// <summary>
        /// Tracks the current doc's entity, url segment and delegate to retrieve it's old descendents during publishing in the current request
        /// </summary>
        private static PrePublishedContentContext PrePublishedContent
        {
            get
            {
                //return the item in the cache - otherwise initialize it to an empty instance
                return RequestCache.GetCacheItem<PrePublishedContentContext>(ContextKey4, () => PrePublishedContentContext.Empty);
            }
            set
            {
                //clear it
                RequestCache.ClearCacheItem(ContextKey4);
                //force it into the cache
                RequestCache.GetCacheItem<PrePublishedContentContext>(ContextKey4, () => value);
            }
        }

        //private static Func<IEnumerable<IPublishedContent>> DescendentsOrSelfDelegate
        //{
        //    get
        //    {
        //        //return the item in the cache - otherwise initialize it to an empty string
        //        return RequestCache.GetCacheItem<Func<IEnumerable<IPublishedContent>>>(ContextKey4, () => (() => Enumerable.Empty<IPublishedContent>()));
        //    }
        //    set
        //    {
        //        //clear it
        //        RequestCache.ClearCacheItem(ContextKey4);
        //        //force it into the cache
        //        RequestCache.GetCacheItem<Func<IEnumerable<IPublishedContent>>>(ContextKey4, () => value);
        //    }
        //}

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

            foreach (var entity in args.PublishedEntities)
            {                
                var entityContent = contentCache.GetById(entity.Id);
                if (entityContent == null) continue;

                PrePublishedContent = new PrePublishedContentContext(entity, entity.GetUrlSegment(), contentCache, () => entityContent.Descendants());

                //if (Moving)
                //{
                //    var entityContent = contentCache.GetById(entity.Id);
                //    if (entityContent == null) continue;
                //    foreach (var x in entityContent.Descendants())
                //    {
                //        var route = contentCache.GetRouteById(x.Id);
                //        if (IsNotRoute(route)) continue;
                //        var wk = UnwrapToKey(x);
                //        if (wk == null) continue;

                //        OldRoutes[x.Id] = Tuple.Create(wk.Key, route);
                //    }
                //}
            }

            LockedEvents = true; // we only want to see the "first batch"
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
            //This should only ever occur on the Master server when in load balancing since this will fire on all
            // servers taking part in load balancing
            var serverRole = ApplicationContext.Current.GetCurrentServerRole();
            if (serverRole == ServerRole.Master || serverRole == ServerRole.Single)
            {
                //copy local
                var prePublishedContext = PrePublishedContent;

                //cannot continue if this is empty
                if (prePublishedContext.Entity == null) return;

                //cannot continue if there is no published cache
                var contentCache = GetPublishedCache();
                if (contentCache == null) return;
                
                //get the entity id out of the event args to compare with the id stored during publishing
                if (cacheRefresherEventArgs.MessageType != MessageType.RefreshById || cacheRefresherEventArgs.MessageType != MessageType.RefreshByInstance) return;

                var refreshedEntityId = cacheRefresherEventArgs.MessageType == MessageType.RefreshByInstance
                    ? ((IContent)cacheRefresherEventArgs.MessageObject).Id
                    : (int) cacheRefresherEventArgs.MessageObject;

                //if it's not the id that we're targeting, don't continue
                if (refreshedEntityId != prePublishedContext.Entity.Id) return;

                //cannot continue if the entity is not found
                var entityContent = contentCache.GetById(prePublishedContext.Entity.Id);                
                if (entityContent == null) return;

                //now we can check if the segment has changed
                var newSegment = prePublishedContext.Entity.GetUrlSegment();
                try
                {
                    if (Moving || newSegment != prePublishedContext.UrlSegment)
                    {
                        //it's changed!

                        // assuming we cannot have 'CacheUpdated' for only part of the infos else we'd need
                        // to set a flag in 'Published' to indicate which entities have been refreshed ok
                        CreateRedirect(prePublishedContext.Entity.Id, prePublishedContext.Entity.Key, prePublishedContext.UrlSegment);

                        //iterate the old descendents and get their old routes
                        foreach (var x in prePublishedContext.DescendentsDelegate())
                        {
                            //get the old route from the old contextual cache
                            var oldRoute = prePublishedContext.ContentCache.GetRouteById(x.Id);
                            if (IsNotRoute(oldRoute)) continue;
                            var wk = UnwrapToKey(x);
                            if (wk == null) continue;

                            CreateRedirect(wk.Id, wk.Key, oldRoute);
                        }
                    }
                }
                finally
                {
                    //set all refs to null
                    prePublishedContext.Entity = null;
                    prePublishedContext.ContentCache = null;
                    prePublishedContext.DescendentsDelegate = null;
                }
            }
        }

        private static void ContentService_Published(IPublishingStrategy sender, PublishEventArgs<IContent> e)
        {
            // look note in CacheUpdated
            // we might want to set a flag on the entities we are seeing here
        }

        private static void ContentService_Moving(IContentService sender, MoveEventArgs<IContent> e)
        {
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
            // err/- if collision or anomaly or ...
            return route == null || route.StartsWith("err/");
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
