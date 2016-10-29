﻿using System;
using System.Web.Script.Serialization;
using umbraco;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using System.Linq;

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
            var serializer = new JavaScriptSerializer();
            var jsonObject = serializer.Deserialize<JsonPayload[]>(json);
            return jsonObject;
        }

      
        internal static string SerializeToJsonPayloadForPermanentDeletion(params int[] contentIds)
        {
            var serializer = new JavaScriptSerializer();
            var items = contentIds.Select(x => new JsonPayload
            {
                Id = x,
                Operation = OperationType.Deleted
            }).ToArray();
            var json = serializer.Serialize(items);
            return json;
        }

        #endregion

        #region Sub classes

        internal enum OperationType
        {            
            Deleted
        }

        internal class JsonPayload
        {            
            public int Id { get; set; }
            public OperationType Operation { get; set; }
        }

        #endregion

        public override void RefreshAll()
        {
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheByKeySearch(CacheKeys.IdToKeyCacheKey);
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheByKeySearch(CacheKeys.KeyToIdCacheKey);
            ClearAllIsolatedCacheByEntityType<IContent>();
            ClearAllIsolatedCacheByEntityType<PublicAccessEntry>();
            DistributedCache.Instance.ClearDomainCacheOnCurrentServer();
            base.RefreshAll();
        }

        public override void Refresh(int id)
        {
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheByKeySearch(CacheKeys.IdToKeyCacheKey);
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheByKeySearch(CacheKeys.KeyToIdCacheKey);
            ClearRepositoryCacheItemById(id);
            ClearAllIsolatedCacheByEntityType<PublicAccessEntry>();
            content.Instance.UpdateSortOrder(id);
            DistributedCache.Instance.ClearDomainCacheOnCurrentServer();
            base.Refresh(id);
        }

        public override void Remove(int id)
        {
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheByKeySearch(CacheKeys.IdToKeyCacheKey);
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheByKeySearch(CacheKeys.KeyToIdCacheKey);
            ClearRepositoryCacheItemById(id);
            ClearAllIsolatedCacheByEntityType<PublicAccessEntry>();
            DistributedCache.Instance.ClearDomainCacheOnCurrentServer();
            base.Remove(id);
        }


        public override void Refresh(IContent instance)
        {
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheByKeySearch(CacheKeys.IdToKeyCacheKey);
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheByKeySearch(CacheKeys.KeyToIdCacheKey);
            ClearRepositoryCacheItemById(instance.Id);
            ClearAllIsolatedCacheByEntityType<PublicAccessEntry>();
            content.Instance.UpdateSortOrder(instance);
            DistributedCache.Instance.ClearDomainCacheOnCurrentServer();
            base.Refresh(instance);
        }

        public override void Remove(IContent instance)
        {
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheByKeySearch(CacheKeys.IdToKeyCacheKey);
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheByKeySearch(CacheKeys.KeyToIdCacheKey);
            ClearRepositoryCacheItemById(instance.Id);
            ClearAllIsolatedCacheByEntityType<PublicAccessEntry>();
            DistributedCache.Instance.ClearDomainCacheOnCurrentServer();
            base.Remove(instance);
        }

        /// <summary>
        /// Implement the IJsonCacheRefresher so that we can bulk delete the cache based on multiple IDs for when the recycle bin is emptied
        /// </summary>
        /// <param name="jsonPayload"></param>
        public void Refresh(string jsonPayload)
        {
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheByKeySearch(CacheKeys.IdToKeyCacheKey);
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheByKeySearch(CacheKeys.KeyToIdCacheKey);
            ClearAllIsolatedCacheByEntityType<PublicAccessEntry>();

            foreach (var payload in DeserializeFromJsonPayload(jsonPayload))
            {
                ClearRepositoryCacheItemById(payload.Id);
                content.Instance.UpdateSortOrder(payload.Id);
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
        
    }
}