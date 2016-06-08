using System;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Services.Changes;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web.Cache
{
    public sealed class ContentCacheRefresher : PayloadCacheRefresherBase<ContentCacheRefresher, ContentCacheRefresher.JsonPayload>
    {
        private readonly IFacadeService _facadeService;

        public ContentCacheRefresher(CacheHelper cacheHelper, IFacadeService facadeService)
            : base(cacheHelper)
        {
            _facadeService = facadeService;
        }

        #region Define

        protected override ContentCacheRefresher Instance => this;

        public static readonly Guid UniqueId = Guid.Parse("900A4FBE-DF3C-41E6-BB77-BE896CD158EA");

        public override Guid RefresherUniqueId => UniqueId;

        public override string Name => "ContentCacheRefresher";

        #endregion

        #region Refresher

        public override void Refresh(JsonPayload[] payloads)
        {
            var runtimeCache = ApplicationContext.Current.ApplicationCache.RuntimeCache;
            runtimeCache.ClearCacheObjectTypes<PublicAccessEntry>();
            foreach (var payload in payloads)
            {
                // remove that one
                runtimeCache.ClearCacheItem(RepositoryBase.GetCacheIdKey<IContent>(payload.Id));

                // remove those that are in the branch
                if (payload.ChangeTypes.HasTypesAny(TreeChangeTypes.RefreshBranch | TreeChangeTypes.Remove))
                {
                    var pathid = "," + payload.Id + ",";
                    runtimeCache.ClearCacheObjectTypes<IContent>((k, v) => v.Path.Contains(pathid));
                }
            }

            // note: must do what's above FIRST else the repositories still have the old cached
            // content and when the PublishedCachesService is notified of changes it does not see
            // the new content...

            bool draftChanged, publishedChanged;
            _facadeService.Notify(payloads, out draftChanged, out publishedChanged);

            if (payloads.Any(x => x.ChangeTypes.HasType(TreeChangeTypes.RefreshAll)) || publishedChanged)
            {
                // when a public version changes
                ApplicationContext.Current.ApplicationCache.ClearPartialViewCache();
                MacroCacheRefresher.ClearMacroContentCache(CacheHelper); // just the content
                ClearXsltCache();

                ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheByKeySearch(CacheKeys.IdToKeyCacheKey);
                ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheByKeySearch(CacheKeys.KeyToIdCacheKey);
            }

            base.Refresh(payloads);
        }

        // these events should never trigger
        // everything should be PAYLOAD/JSON

        public override void RefreshAll()
        {
            throw new NotSupportedException();
        }

        public override void Refresh(int id)
        {
            throw new NotSupportedException();
        }

        public override void Refresh(Guid id)
        {
            throw new NotSupportedException();
        }

        public override void Remove(int id)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region Json

        public class JsonPayload
        {
            public JsonPayload(int id, TreeChangeTypes changeTypes)
            {
                Id = id;
                ChangeTypes = changeTypes;
            }

            public int Id { get; }

            public TreeChangeTypes ChangeTypes { get; }
        }

        #endregion

        #region Events

        #endregion

        #region Indirect

        public static void RefreshContentTypes(CacheHelper cacheHelper)
        {
            // we could try to have a mechanism to notify the PublishedCachesService
            // and figure out whether published items were modified or not... keep it
            // simple for now, just clear the whole thing

            cacheHelper.ClearPartialViewCache();
            MacroCacheRefresher.ClearMacroContentCache(cacheHelper); // just the content
            ClearXsltCache();

            cacheHelper.IsolatedRuntimeCache.ClearCache<PublicAccessEntry>();
            cacheHelper.IsolatedRuntimeCache.ClearCache<IContent>();
        }

        #endregion

        #region Helpers

        private static void ClearXsltCache()
        {
            // todo: document where this is coming from
            if (UmbracoConfig.For.UmbracoSettings().Content.UmbracoLibraryCacheDuration <= 0) return;
            ApplicationContext.Current.ApplicationCache.ClearCacheObjectTypes("MS.Internal.Xml.XPath.XPathSelectionIterator");
        }

        #endregion
    }
}
