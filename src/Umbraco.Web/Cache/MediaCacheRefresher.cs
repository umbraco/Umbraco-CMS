using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Script.Serialization;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Models;

using Umbraco.Core.Persistence.Repositories;
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
        internal static JsonPayload[] DeserializeFromJsonPayload(string json)
        {
            var serializer = new JavaScriptSerializer();
            var jsonObject = serializer.Deserialize<JsonPayload[]>(json);
            return jsonObject;
        }

        /// <summary>
        /// Creates the custom Json payload used to refresh cache amongst the servers
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="media"></param>
        /// <returns></returns>
        internal static string SerializeToJsonPayload(OperationType operation, params IMedia[] media)
        {
            var serializer = new JavaScriptSerializer();
            var items = media.Select(x => FromMedia(x, operation)).ToArray();
            var json = serializer.Serialize(items);
            return json;
        }

        internal static string SerializeToJsonPayloadForMoving(OperationType operation, MoveEventInfo<IMedia>[] media)
        {
            var serializer = new JavaScriptSerializer();
            var items = media.Select(x => new JsonPayload
            {
                Id = x.Entity.Id,
                Operation = operation,
                Path = x.OriginalPath
            }).ToArray();
            var json = serializer.Serialize(items);
            return json;
        }

        internal static string SerializeToJsonPayloadForPermanentDeletion(params int[] mediaIds)
        {
            var serializer = new JavaScriptSerializer();
            var items = mediaIds.Select(x => new JsonPayload
            {
                Id = x,
                Operation = OperationType.Deleted
            }).ToArray();
            var json = serializer.Serialize(items);
            return json;
        }

        /// <summary>
        /// Converts a macro to a jsonPayload object
        /// </summary>
        /// <param name="media"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        internal static JsonPayload FromMedia(IMedia media, OperationType operation)
        {
            if (media == null) return null;

            var payload = new JsonPayload
            {
                Id = media.Id,
                Path = media.Path,
                Operation = operation
            };
            return payload;
        }

        #endregion

        #region Sub classes

        internal enum OperationType
        {
            Saved,
            Trashed,
            Deleted
        }

        internal class JsonPayload
        {
            public string Path { get; set; }
            public int Id { get; set; }
            public OperationType Operation { get; set; }
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
            ClearCache(FromMedia(ApplicationContext.Current.Services.MediaService.GetById(id), OperationType.Saved));
            base.Refresh(id);
        }

        public override void Remove(int id)
        {            
            ClearCache(FromMedia(ApplicationContext.Current.Services.MediaService.GetById(id),
                //NOTE: we'll just default to trashed for this one.    
                OperationType.Trashed));
            base.Remove(id);
        }
        
        private static void ClearCache(params JsonPayload[] payloads)
        {
            if (payloads == null) return;

            ApplicationContext.Current.ApplicationCache.ClearPartialViewCache();

            payloads.ForEach(payload =>
                {

                    //if there's no path, then just use id (this will occur on permanent deletion like emptying recycle bin)
                    if (payload.Path.IsNullOrWhiteSpace())
                    {
                        ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheByKeySearch(
                            string.Format("{0}_{1}", CacheKeys.MediaCacheKey, payload.Id));
                    }
                    else
                    {
                        foreach (var idPart in payload.Path.Split(','))
                        {
                            int idPartAsInt;
                            if (int.TryParse(idPart, out idPartAsInt))
                            {
                                ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheItem(
                                    RepositoryBase.GetCacheIdKey<IMedia>(idPartAsInt));
                            }

                            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheByKeySearch(
                                string.Format("{0}_{1}_True", CacheKeys.MediaCacheKey, idPart));

                            // Also clear calls that only query this specific item!
                            if (idPart == payload.Id.ToString(CultureInfo.InvariantCulture))
                                ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheByKeySearch(
                                    string.Format("{0}_{1}", CacheKeys.MediaCacheKey, payload.Id));

                        }   
                    }                    
                });

            
        }
    }
}