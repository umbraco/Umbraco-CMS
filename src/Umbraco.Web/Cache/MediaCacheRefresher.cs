using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using umbraco.interfaces;
using System.Linq;

namespace Umbraco.Web.Cache
{

    /// <summary>
    /// A cache refresher to ensure media cache is updated
    /// </summary>
    /// <remarks>
    /// This is not intended to be used directly in your code and it should be sealed but due to legacy code we cannot seal it.
    /// </remarks>
    public class MediaCacheRefresher : JsonCacheRefresherBase<MediaCacheRefresher>
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
        /// <param name="media"></param>
        /// <returns></returns>
        internal static string SerializeToJsonPayload(params IMedia[] media)
        {
            var serializer = new JavaScriptSerializer();
            var items = media.Select(FromMedia).ToArray();
            var json = serializer.Serialize(items);
            return json;
        }

        /// <summary>
        /// Converts a macro to a jsonPayload object
        /// </summary>
        /// <param name="media"></param>
        /// <returns></returns>
        private static JsonPayload FromMedia(IMedia media)
        {
            if (media == null) return null;

            var payload = new JsonPayload
            {
                Id = media.Id,
                Path = media.Path
            };
            return payload;
        }

        #endregion

        #region Sub classes

        private class JsonPayload
        {
            public string Path { get; set; }
            public int Id { get; set; }
        }

        #endregion

        protected override MediaCacheRefresher Instance
        {
            get { return this; }
        }

        public override Guid UniqueIdentifier
        {
            get { return new Guid(DistributedCache.MediaCacheRefresherId); }
        }

        public override string Name
        {
            get { return "Clears Media Cache from umbraco.library"; }
        }

        public override void Refresh(string jsonPayload)
        {
            ClearCache(DeserializeFromJsonPayload(jsonPayload));
            base.Refresh(jsonPayload);
        }

        public override void Refresh(int id)
        {
            ClearCache(FromMedia(ApplicationContext.Current.Services.MediaService.GetById(id)));
            base.Refresh(id);
        }

        public override void Remove(int id)
        {
            ClearCache(FromMedia(ApplicationContext.Current.Services.MediaService.GetById(id)));
            base.Remove(id);
        }
        
        private static void ClearCache(params JsonPayload[] payloads)
        {
            if (payloads == null) return;

            ApplicationContext.Current.ApplicationCache.ClearPartialViewCache();

            payloads.ForEach(payload =>
                {
                    foreach (var idPart in payload.Path.Split(','))
                    {
                        ApplicationContext.Current.ApplicationCache.ClearCacheByKeySearch(
                            string.Format("{0}_{1}_True", CacheKeys.MediaCacheKey, idPart));

                        // Also clear calls that only query this specific item!
                        if (idPart == payload.Id.ToString())
                            ApplicationContext.Current.ApplicationCache.ClearCacheByKeySearch(
                                string.Format("{0}_{1}", CacheKeys.MediaCacheKey, payload.Id));

                    }
                });

            
        }
    }
}