using System;
using System.Web.Script.Serialization;
using Umbraco.Core;
using Umbraco.Core.Cache;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;


namespace Umbraco.Web.Cache
{
    /// <summary>
    /// A cache refresher to ensure member cache is updated when members change
    /// </summary>    
    public sealed class DataTypeCacheRefresher : JsonCacheRefresherBase<DataTypeCacheRefresher>
    {

        #region Static helpers
        
        /// <summary>
        /// Converts the json to a JsonPayload object
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        private static JsonPayload[] DeserializeFromJsonPayload(string json)
        {
            var serializer = new JavaScriptSerializer();
            var jsonObject = serializer.Deserialize<JsonPayload[]>(json);
            return jsonObject;
        }

        /// <summary>
        /// Creates the custom Json payload used to refresh cache amongst the servers
        /// </summary>
        /// <param name="dataTypes"></param>
        /// <returns></returns>
        internal static string SerializeToJsonPayload(params global::umbraco.cms.businesslogic.datatype.DataTypeDefinition[] dataTypes)
        {
            var serializer = new JavaScriptSerializer();
            var items = dataTypes.Select(FromDataTypeDefinition).ToArray();
            var json = serializer.Serialize(items);
            return json;
        }

        /// <summary>
        /// Creates the custom Json payload used to refresh cache amongst the servers
        /// </summary>
        /// <param name="dataTypes"></param>
        /// <returns></returns>
        internal static string SerializeToJsonPayload(params IDataTypeDefinition[] dataTypes)
        {
            var serializer = new JavaScriptSerializer();
            var items = dataTypes.Select(FromDataTypeDefinition).ToArray();
            var json = serializer.Serialize(items);
            return json;
        }

        /// <summary>
        /// Converts a macro to a jsonPayload object
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        private static JsonPayload FromDataTypeDefinition(global::umbraco.cms.businesslogic.datatype.DataTypeDefinition dataType)
        {
            var payload = new JsonPayload
            {
                UniqueId = dataType.UniqueId,
                Id = dataType.Id
            };
            return payload;
        }

        /// <summary>
        /// Converts a macro to a jsonPayload object
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        private static JsonPayload FromDataTypeDefinition(IDataTypeDefinition dataType)
        {
            var payload = new JsonPayload
            {
                UniqueId = dataType.Key,
                Id = dataType.Id
            };
            return payload;
        }
        
        #endregion

        #region Sub classes

        private class JsonPayload
        {
            public Guid UniqueId { get; set; }
            public int Id { get; set; }
        }

        #endregion

        protected override DataTypeCacheRefresher Instance
        {
            get { return this; }
        }

        public override Guid UniqueIdentifier
        {
            get { return new Guid(DistributedCache.DataTypeCacheRefresherId); }
        }

        public override string Name
        {
            get { return "Clears data type cache"; }
        }

        public override void Refresh(string jsonPayload)
        {
            var payloads = DeserializeFromJsonPayload(jsonPayload);

            //we need to clear the ContentType runtime cache since that is what caches the
            // db data type to store the value against and anytime a datatype changes, this also might change
            // we basically need to clear all sorts of runtime caches here because so many things depend upon a data type
            
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheObjectTypes<IContent>();
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheObjectTypes<IContentType>();
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheObjectTypes<IMedia>();
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheObjectTypes<IMediaType>();
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheObjectTypes<IMember>();
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheObjectTypes<IMemberType>();

            payloads.ForEach(payload =>
            {
                //clear both the Id and Unique Id cache since we cache both in the legacy classes :(
                ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheByKeySearch(
                    string.Format("{0}{1}", CacheKeys.DataTypeCacheKey, payload.Id));                
                ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheByKeySearch(
                    string.Format("{0}{1}", CacheKeys.DataTypeCacheKey, payload.UniqueId));

                //clears the prevalue cache
                ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheByKeySearch(
                    string.Format("{0}{1}", CacheKeys.DataTypePreValuesCacheKey, payload.Id));

                PublishedContentType.ClearDataType(payload.Id);
            });

            base.Refresh(jsonPayload);
        }
    }
}