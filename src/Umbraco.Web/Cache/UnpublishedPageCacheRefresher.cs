using System;
using System.Web.Script.Serialization;
using umbraco;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using System.Linq;
using Newtonsoft.Json;
using umbraco.cms.businesslogic.web;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Sync;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// A cache refresher used for non-published content, this is primarily to notify Examine indexes to update and to refresh the RuntimeCacheRefresher
    /// </summary>
    public sealed class UnpublishedPageCacheRefresher : TypedCacheRefresherBase<UnpublishedPageCacheRefresher, IContent>, IJsonCacheRefresher
    {
        protected override UnpublishedPageCacheRefresher Instance
        {
            get { return this; }
        }

        public override Guid UniqueIdentifier
        {
            get { return new Guid(DistributedCache.UnpublishedPageCacheRefresherId); }
        }

        public override string Name
        {
            get { return "Unpublished Page Refresher"; }
        }

        #region Static helpers

        /// <summary>
        /// Converts the json to a JsonPayload object
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        internal static JsonPayload[] DeserializeFromJsonPayload(string json)
        {
            var jsonObject = JsonConvert.DeserializeObject<JsonPayload[]>(json);
            return jsonObject;
        }


        internal static string SerializeToJsonPayloadForPermanentDeletion(params int[] contentIds)
        {
            var items = contentIds.Select(x => new JsonPayload
            {
                Id = x,
                Operation = OperationType.Deleted
            }).ToArray();
            var json = JsonConvert.SerializeObject(items);
            return json;
        }
        internal static string SerializeToJsonPayload(OperationType operationType, params IContent[] contents)
        {
            var items = contents.Select(x => new JsonPayload
            {
                Id = x.Id,
                Key = x.Key,
                Operation = operationType,
                IsBlueprint = x.IsBlueprint
            }).ToArray();
            var json = JsonConvert.SerializeObject(items);
            return json;
        }

        #endregion

        #region Sub classes

        internal enum OperationType
        {
            Deleted,
            Refresh
        }

        internal class JsonPayload
        {
            public int Id { get; set; }
            public Guid? Key { get; set; }
            public OperationType Operation { get; set; }
            public bool? IsBlueprint { get; set; }
        }

        #endregion

        public override void RefreshAll()
        {
            ApplicationContext.Current.Services.IdkMap.ClearCache();
            ClearAllIsolatedCacheByEntityType<IContent>();
            ClearAllIsolatedCacheByEntityType<PublicAccessEntry>();
            DistributedCache.Instance.ClearDomainCacheOnCurrentServer();
            content.Instance.ClearPreviewXmlContent();
            base.RefreshAll();
        }

        public override void Refresh(int id)
        {
            ClearRepositoryCacheItemById(id);
            ClearAllIsolatedCacheByEntityType<PublicAccessEntry>();
            content.Instance.UpdateSortOrder(id);
            Document d = null;
            try
            {
                d = new Document(id);
            }
            catch (Exception){ } // not a document, cannot continue
            if (d != null)
            {
                content.Instance.UpdateDocumentCache(d);
                content.Instance.UpdatePreviewXmlContent(d);
                DistributedCache.Instance.ClearDomainCacheOnCurrentServer();
            }
            base.Refresh(id);
        }

        public override void Remove(int id)
        {
            ApplicationContext.Current.Services.IdkMap.ClearCache(id);
            ClearRepositoryCacheItemById(id);
            ClearAllIsolatedCacheByEntityType<PublicAccessEntry>();
            DistributedCache.Instance.ClearDomainCacheOnCurrentServer();
            content.Instance.ClearPreviewXmlContent(id);
            base.Remove(id);
        }


        public override void Refresh(IContent instance)
        {
            ClearRepositoryCacheItemById(instance.Id);
            ClearAllIsolatedCacheByEntityType<PublicAccessEntry>();
            if (!instance.IsBlueprint)
            {
                content.Instance.UpdateSortOrder(instance);
                var d = new Document(instance);
                content.Instance.UpdateDocumentCache(d);
                content.Instance.UpdatePreviewXmlContent(d);
                DistributedCache.Instance.ClearDomainCacheOnCurrentServer();
            }
            base.Refresh(instance);
        }

        public override void Remove(IContent instance)
        {
            ApplicationContext.Current.Services.IdkMap.ClearCache(instance.Id);
            ClearRepositoryCacheItemById(instance.Id);
            if (!instance.IsBlueprint)
            {
                ClearAllIsolatedCacheByEntityType<PublicAccessEntry>();
                DistributedCache.Instance.ClearDomainCacheOnCurrentServer();
                content.Instance.ClearPreviewXmlContent(instance.Id);
            }
            base.Remove(instance);
        }

        /// <summary>
        /// Implement the IJsonCacheRefresher so that we can bulk delete the cache based on multiple IDs for when the recycle bin is emptied
        /// </summary>
        /// <param name="jsonPayload"></param>
        public void Refresh(string jsonPayload)
        {
            ClearAllIsolatedCacheByEntityType<PublicAccessEntry>();

            foreach (var payload in DeserializeFromJsonPayload(jsonPayload))
            {
                ClearRepositoryCacheItemById(payload.Id);
                ClearRepositoryCacheItemById(payload.Key);
                ClearAllIsolatedCacheByEntityType<PublicAccessEntry>();

                if (payload.Operation == OperationType.Deleted)
                {
                    ApplicationContext.Current.Services.IdkMap.ClearCache(payload.Id);
                    content.Instance.ClearPreviewXmlContent(payload.Id);
                    base.Remove(payload.Id);
                }

                if (payload.Operation == OperationType.Refresh)
                {                    
                    if (!payload.IsBlueprint.HasValue)
                    {
                        // if this is null (since it was added later) then we need to try/catch since we don't know what it is
                        Document d = null;
                        try
                        {
                            d = new Document(payload.Id);
                        }
                        catch (Exception) { } // not a document, cannot continue
                        if (d != null)
                        {
                            content.Instance.UpdateSortOrder(payload.Id);
                            content.Instance.UpdateDocumentCache(d);
                            content.Instance.UpdatePreviewXmlContent(d);
                        }
                    }
                    else if (!payload.IsBlueprint.Value)
                    {
                        // if it's not a blueprint, then need to refresh the published cache 
                        var d = new Document(payload.Id);
                        content.Instance.UpdateSortOrder(payload.Id);                        
                        content.Instance.UpdateDocumentCache(d);
                        content.Instance.UpdatePreviewXmlContent(d);
                    }

                    base.Refresh(payload.Id);
                }
            }

            DistributedCache.Instance.ClearDomainCacheOnCurrentServer();

            OnCacheUpdated(Instance, new CacheRefresherEventArgs(jsonPayload, MessageType.RefreshByJson));
        }

        private void ClearRepositoryCacheItemById(int id)
        {
            var contentCache = ApplicationContext.Current.ApplicationCache.IsolatedRuntimeCache.GetCache<IContent>();
            if (contentCache)
            {
                contentCache.Result.ClearCacheItem(RepositoryBase.GetCacheIdKey<IContent>(id));
            }
        }

        private void ClearRepositoryCacheItemById(Guid? key)
        {
            var contentCache = ApplicationContext.Current.ApplicationCache.IsolatedRuntimeCache.GetCache<IContent>();
            if (contentCache)
            {
                contentCache.Result.ClearCacheItem(RepositoryBase.GetCacheIdKey<IContent>(key));
            }
        }
    }
}
