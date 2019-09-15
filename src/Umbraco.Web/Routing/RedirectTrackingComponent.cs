using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Changes;
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

        private readonly IUmbracoSettingsSection _umbracoSettings;

        public RedirectTrackingComponent(IUmbracoSettingsSection umbracoSettings)
        {
            _umbracoSettings = umbracoSettings;
        }

        private static Dictionary<ContentIdAndCulture, ContentKeyAndOldRoute> OldRoutes
        {
            get
            {
                var oldRoutes = (Dictionary<ContentIdAndCulture, ContentKeyAndOldRoute>) Current.UmbracoContext.HttpContext.Items[ContextKey1];
                if (oldRoutes == null)
                    Current.UmbracoContext.HttpContext.Items[ContextKey1] = oldRoutes = new Dictionary<ContentIdAndCulture, ContentKeyAndOldRoute>();
                return oldRoutes;
            }
        }

        private static bool HasOldRoutes
        {
            get
            {
                if (Current.UmbracoContext == null) return false;
                if (Current.UmbracoContext.HttpContext == null) return false;
                if (Current.UmbracoContext.HttpContext.Items[ContextKey1] == null) return false;
                return true;
            }
        }

        public void Initialize()
        {
            // don't let the event handlers kick in if Redirect Tracking is turned off in the config
            if (_umbracoSettings.WebRouting.DisableRedirectUrlTracking) return;

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
        { }

        private static void ContentService_Publishing(IContentService sender, PublishEventArgs<IContent> args)
        {
            foreach (var entity in args.PublishedEntities)
            {
                StoreOldRoute(entity);
            }
        }

        private void ContentService_Published(IContentService sender, ContentPublishedEventArgs args)
        {
            CreateRedirects(args.PublishedEntities.Select(c => c.Id).ToArray());            
        }

        private static void ContentService_Moving(IContentService sender, MoveEventArgs<IContent> args)
        {
            foreach (var info in args.MoveInfoCollection)
            {
                StoreOldRoute(info.Entity);
            }
        }

        private static void ContentService_Moved(IContentService sender, MoveEventArgs<IContent> args)
        {
            CreateRedirects(args.MoveInfoCollection.Select(i => i.Entity.Id).ToArray());
        }

        private static void StoreOldRoute(IContent entity)
        {
            var contentCache = Current.UmbracoContext.Content;
            var entityContent = contentCache.GetById(entity.Id);
            if (entityContent == null) return;

            // get the default affected cultures by going up the tree until we find the first culture variant entity (default to no cultures) 
            var defaultCultures = entityContent.AncestorsOrSelf()?.FirstOrDefault(a => a.Cultures.Any())?.Cultures.Keys.ToArray()
                ?? new[] { (string)null };
            foreach (var x in entityContent.DescendantsOrSelf())
            {
                // if this entity defines specific cultures, use those instead of the default ones
                var cultures = x.Cultures.Any() ? x.Cultures.Keys : defaultCultures;

                foreach (var culture in cultures)
                {
                    var route = contentCache.GetRouteById(x.Id, culture);
                    if (IsNotRoute(route)) return;
                    OldRoutes[new ContentIdAndCulture(x.Id, culture)] = new ContentKeyAndOldRoute(x.Key, route);
                }
            }
        }

        private static void CreateRedirects(IEnumerable<int> contentIds)
        {
            if (!HasOldRoutes)
                return;

            // manage routes
            var removeKeys = new List<ContentIdAndCulture>();

            foreach (var oldRoute in OldRoutes)
            {
                if (contentIds.Contains(oldRoute.Key.ContentId))
                {
                    CreateRedirect(oldRoute.Key.ContentId, oldRoute.Key.Culture, oldRoute.Value.ContentKey, oldRoute.Value.OldRoute);
                    removeKeys.Add(oldRoute.Key);
                }
            }

            foreach (var k in removeKeys)
            {
                OldRoutes.Remove(k);
            }
        }

        private static void CreateRedirect(int contentId, string culture, Guid contentKey, string oldRoute)
        {
            var contentCache = Current.UmbracoContext.Content;
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
