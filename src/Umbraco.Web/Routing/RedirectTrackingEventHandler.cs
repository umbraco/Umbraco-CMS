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
using Umbraco.Web.Cache;

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
        private const string ContextKey1 = "Umbraco.Web.Redirects.RedirectTrackingEventHandler.1";
        private const string ContextKey2 = "Umbraco.Web.Redirects.RedirectTrackingEventHandler.2";
        private const string ContextKey3 = "Umbraco.Web.Redirects.RedirectTrackingEventHandler.3";

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

        private static Dictionary<int, Tuple<Guid, string>> OldRoutes
        {
            get
            {
                if (UmbracoContext.Current == null)
                    return null;
                var oldRoutes = (Dictionary<int, Tuple<Guid, string>>) UmbracoContext.Current.HttpContext.Items[ContextKey3];
                if (oldRoutes == null)
                    UmbracoContext.Current.HttpContext.Items[ContextKey3] = oldRoutes = new Dictionary<int, Tuple<Guid, string>>();
                return oldRoutes;
            }
        }

        private static bool LockedEvents
        {
            get { return Moving && UmbracoContext.Current.HttpContext.Items[ContextKey2] != null; }
            set
            {
                if (Moving && value)
                    UmbracoContext.Current.HttpContext.Items[ContextKey2] = true;
                else
                    UmbracoContext.Current.HttpContext.Items.Remove(ContextKey2);
            }
        }

        private static bool Moving
        {
            get { return UmbracoContext.Current.HttpContext.Items[ContextKey1] != null; }
            set
            {
                if (value)
                    UmbracoContext.Current.HttpContext.Items[ContextKey1] = true;
                else
                {
                    UmbracoContext.Current.HttpContext.Items.Remove(ContextKey1);
                    UmbracoContext.Current.HttpContext.Items.Remove(ContextKey2);
                }
            }
        }

        private static void ContentService_Publishing(IPublishingStrategy sender, PublishEventArgs<IContent> args)
        {
            if (LockedEvents) return;

            var contentCache = UmbracoContext.Current.ContentCache;
            foreach (var entity in args.PublishedEntities)
            {
                var entityContent = contentCache.GetById(entity.Id);
                if (entityContent == null) continue;
                foreach (var x in entityContent.DescendantsOrSelf())
                {
                    var route = contentCache.GetRouteById(x.Id);
                    if (IsNotRoute(route)) continue;
                    var wk = UnwrapToKey(x);
                    if (wk == null) continue;
                    OldRoutes[x.Id] = Tuple.Create(wk.Key, route);
                }
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

        private void PageCacheRefresher_CacheUpdated(PageCacheRefresher sender, CacheRefresherEventArgs cacheRefresherEventArgs)
        {
            if (OldRoutes == null)
                return;

            var removeKeys = new List<int>();
            
            foreach (var oldRoute in OldRoutes)
            {
                // assuming we cannot have 'CacheUpdated' for only part of the infos else we'd need
                // to set a flag in 'Published' to indicate which entities have been refreshed ok
                CreateRedirect(oldRoute.Key, oldRoute.Value.Item1, oldRoute.Value.Item2);
                removeKeys.Add(oldRoute.Key);
            }

            foreach (var k in removeKeys)
                OldRoutes.Remove(k);
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
            var contentCache = UmbracoContext.Current.ContentCache;
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
    }
}
