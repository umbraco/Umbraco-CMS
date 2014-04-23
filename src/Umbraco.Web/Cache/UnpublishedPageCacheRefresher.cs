using System;
using System.Web.Script.Serialization;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using System.Linq;
using Umbraco.Core.Sync;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// A cache refresher used for non-published content, this is primarily to notify Examine indexes to update
    /// </summary>
    public sealed class UnpublishedPageCacheRefresher : TypedCacheRefresherBase<UnpublishedPageCacheRefresher, IContent>, IJsonCacheRefresher
    {

        //NOTE: There is no functionality for this cache refresher, it is here simply to emit events on each server for which examine
        // binds to. We could put the Examine index functionality in here but we've kept it all in the ExamineEvents class so that all of 
        // the logic is in one place. In the future we may put the examine logic in a cache refresher instead (that would make sense) but we'd
        // want to get this done before making more cache refreshers: 
        // http://issues.umbraco.org/issue/U4-2633

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

        /// <summary>
        /// Implement the IJsonCacheRefresher so that we can bulk delete the cache based on multiple IDs for when the recycle bin is emptied
        /// </summary>
        /// <param name="jsonPayload"></param>
        public void Refresh(string jsonPayload)
        {
            OnCacheUpdated(Instance, new CacheRefresherEventArgs(jsonPayload, MessageType.RefreshByJson));
        }
        
    }
}