using System;
using System.Web.Script.Serialization;
using umbraco;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using System.Linq;
using Umbraco.Core.Persistence.Caching;
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
            RuntimeCacheProvider.Current.Clear(typeof(IContent));
            base.RefreshAll();
        }

        public override void Refresh(int id)
        {
            RuntimeCacheProvider.Current.Delete(typeof(IContent), id);
            content.Instance.UpdateSortOrder(id);
            base.Refresh(id);
        }

        public override void Remove(int id)
        {
            RuntimeCacheProvider.Current.Delete(typeof(IContent), id);
            base.Remove(id);
        }


        public override void Refresh(IContent instance)
        {
            RuntimeCacheProvider.Current.Delete(typeof(IContent), instance.Id);
            content.Instance.UpdateSortOrder(instance);
            base.Refresh(instance);
        }

        public override void Remove(IContent instance)
        {
            RuntimeCacheProvider.Current.Delete(typeof(IContent), instance.Id);
            base.Remove(instance);
        }

        /// <summary>
        /// Implement the IJsonCacheRefresher so that we can bulk delete the cache based on multiple IDs for when the recycle bin is emptied
        /// </summary>
        /// <param name="jsonPayload"></param>
        public void Refresh(string jsonPayload)
        {
            foreach (var payload in DeserializeFromJsonPayload(jsonPayload))
            {
                RuntimeCacheProvider.Current.Delete(typeof(IContent), payload.Id);
                content.Instance.UpdateSortOrder(payload.Id);
            }

            OnCacheUpdated(Instance, new CacheRefresherEventArgs(jsonPayload, MessageType.RefreshByJson));
        }
        
    }
}