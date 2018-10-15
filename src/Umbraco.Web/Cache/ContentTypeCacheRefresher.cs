﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Services;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.PublishedCache.XmlPublishedCache;

namespace Umbraco.Web.Cache
{

    /// <summary>
    /// A cache refresher to ensure content type cache is updated when content types change - this is applicable to content, media and member types
    /// </summary>
    /// <remarks>
    /// This is not intended to be used directly in your code
    /// </remarks>
    public sealed class ContentTypeCacheRefresher : JsonCacheRefresherBase<ContentTypeCacheRefresher>
    {

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

        /// <summary>
        /// Converts a content type to a jsonPayload object
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="isDeleted">if the item was deleted</param>
        /// <returns></returns>
        internal static JsonPayload FromContentType(IContentTypeBase contentType, bool isDeleted = false)
        {
            var contentTypeService = (ContentTypeService) ApplicationContext.Current.Services.ContentTypeService;
            var payload = new JsonPayload
            {
                Alias = contentType.Alias,
                Id = contentType.Id,
                PropertyTypeIds = contentType.PropertyTypes.Select(x => x.Id).ToArray(),
                //either IContentType or IMediaType or IMemberType
                Type = (contentType is IContentType)
                    ? typeof(IContentType).Name
                    : (contentType is IMediaType)
                        ? typeof(IMediaType).Name
                        : typeof(IMemberType).Name,
                DescendantPayloads = contentTypeService.GetDescendants(contentType).Select(x => FromContentType(x)).ToArray(),
                WasDeleted = isDeleted,
                PropertyRemoved = contentType.WasPropertyDirty("HasPropertyTypeBeenRemoved"),
                AliasChanged = contentType.WasPropertyDirty("Alias"),
                PropertyTypeAliasChanged = contentType.PropertyTypes.Any(x => x.WasPropertyDirty("Alias")),
                IsNew = contentType.WasPropertyDirty("HasIdentity")
            };


            return payload;
        }

        /// <summary>
        /// Creates the custom Json payload used to refresh cache amongst the servers
        /// </summary>
        /// <param name="isDeleted">specify false if this is an update, otherwise true if it is a deletion</param>
        /// <param name="contentTypes"></param>
        /// <returns></returns>
        internal static string SerializeToJsonPayload(bool isDeleted, params IContentTypeBase[] contentTypes)
        {
            var items = contentTypes.Select(x => FromContentType(x, isDeleted)).ToArray();
            var json = JsonConvert.SerializeObject(items);
            return json;
        }

        #endregion

        #region Sub classes

        internal class JsonPayload
        {
            public JsonPayload()
            {
                WasDeleted = false;
                IsNew = false;
            }
            public string Alias { get; set; }
            public int Id { get; set; }
            public int[] PropertyTypeIds { get; set; }
            public string Type { get; set; }
            public bool AliasChanged { get; set; }
            public bool PropertyRemoved { get; set; }
            public bool PropertyTypeAliasChanged { get; set; }
            public JsonPayload[] DescendantPayloads { get; set; }
            public bool WasDeleted { get; set; }
            public bool IsNew { get; set; }
        }

        #endregion

        protected override ContentTypeCacheRefresher Instance
        {
            get { return this; }
        }

        public override Guid UniqueIdentifier
        {
            get { return new Guid(DistributedCache.ContentTypeCacheRefresherId); }
        }
        public override string Name
        {
            get { return "ContentTypeCacheRefresher"; }
        }

        public override void RefreshAll()
        {
            ClearAllIsolatedCacheByEntityType<IContent>();
            ClearAllIsolatedCacheByEntityType<IContentType>();
            ClearAllIsolatedCacheByEntityType<IMedia>();
            ClearAllIsolatedCacheByEntityType<IMediaType>();
            ClearAllIsolatedCacheByEntityType<IMember>();
            ClearAllIsolatedCacheByEntityType<IMemberType>();
            
            //all content type property cache
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheByKeySearch(CacheKeys.ContentTypePropertiesCacheKey);     
            //all content type cache
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheByKeySearch(CacheKeys.ContentTypeCacheKey);
            //clear static object cache
            global::umbraco.cms.businesslogic.ContentType.RemoveAllDataTypeCache();

            PublishedContentType.ClearAll();

            base.RefreshAll();
        }

        public override void Refresh(int id)
        {
            ClearContentTypeCache(false, id);
            base.Refresh(id);
        }

        public override void Remove(int id)
        {
            ClearContentTypeCache(true, id);
            base.Remove(id);
        }

        /// <summary>
        /// Refreshes the cache using the custom jsonPayload provided
        /// </summary>
        /// <param name="jsonPayload"></param>
        public override void Refresh(string jsonPayload)
        {
            var payload = DeserializeFromJsonPayload(jsonPayload);
            ClearContentTypeCache(payload);
            base.Refresh(jsonPayload);
        }

        /// <summary>
        /// This clears out all cache associated with a content type
        /// </summary>
        /// <param name="payloads"></param>
        /// <remarks>
        /// The cache that is required to be cleared when a content type is updated is as follows:
        /// - ApplicationCache (keys to clear):
        /// -- CacheKeys.PropertyTypeCacheKey + propertyType.Id (each property type assigned)
        /// -- CacheKeys.ContentTypePropertiesCacheKey + contentType.Id
        /// - ContentType.RemoveFromDataTypeCache (clears static object/dictionary cache)
        /// - InMemoryCacheProvider.Current.Clear();
        /// - RoutesCache.Clear();        
        /// </remarks>
        private void ClearContentTypeCache(JsonPayload[] payloads)
        {
            var needsContentRefresh = false;

            foreach (var payload in payloads)
            {
                ApplicationContext.Current.Services.IdkMap.ClearCache(payload.Id);

                //clear the cache for each item
                ClearContentTypeCache(payload);

                //we only need to do this for IContentType NOT for IMediaType, we don't want to refresh the whole cache.
                //if the item was deleted or the alias changed or property removed then we need to refresh the content.
                //and, don't refresh the cache if it is new.
                if (payload.Type == typeof(IContentType).Name
                    && payload.IsNew == false
                    && (payload.WasDeleted || payload.AliasChanged || payload.PropertyRemoved || payload.PropertyTypeAliasChanged))
                {
                    needsContentRefresh = true;
                }
            }

            //need to refresh the xml content cache if required
            if (needsContentRefresh)
            {
                var pageRefresher = CacheRefreshersResolver.Current.GetById(new Guid(DistributedCache.PageCacheRefresherId));
                pageRefresher.RefreshAll();
            }

            //clear the cache providers if there were any content types to clear
            if (payloads.Any())
            {
                if (payloads.Any(x => x.Type == typeof (IContentType).Name))
                {
                    ClearAllIsolatedCacheByEntityType<IContent>();
                    ClearAllIsolatedCacheByEntityType<IContentType>();
                }
                if (payloads.Any(x => x.Type == typeof(IMediaType).Name))
                {
                    ClearAllIsolatedCacheByEntityType<IMedia>();
                    ClearAllIsolatedCacheByEntityType<IMediaType>();
                }
                if (payloads.Any(x => x.Type == typeof(IMemberType).Name))
                {
                    ClearAllIsolatedCacheByEntityType<IMember>();
                    ClearAllIsolatedCacheByEntityType<IMemberType>();
                }
                

                //we only need to do this for IContentType NOT for IMediaType, we don't want to refresh the whole routes
                //cache if only a media type has changed.
                //we don't want to update the routes cache if all of the content types here are new.
                if (payloads.Any(x => x.Type ==  typeof(IContentType).Name) 
                    && payloads.All(x => x.IsNew) == false) //if they are all new then don't proceed
                {
                    // SD: we need to clear the routes cache here!             
                    //
                    // zpqrtbnk: no, not here, in fact the caches should subsribe to refresh events else we
                    // are creating a nasty dependency - but keep it like that for the time being while
                    // SD is cleaning cache refreshers up.
                    if (UmbracoContext.Current != null)
                    {
                        var contentCache = UmbracoContext.Current.ContentCache.InnerCache as PublishedContentCache;
                        if (contentCache != null)
                            contentCache.RoutesCache.Clear();
                    }
                }                
            }
        }

        /// <summary>
        /// Clears cache for an individual IContentTypeBase object
        /// </summary>
        /// <param name="payload"></param>
        /// <remarks>
        /// See notes for the other overloaded ClearContentTypeCache for 
        /// full details on clearing cache.
        /// </remarks>
        /// <returns>
        /// Return true if the alias of the content type changed
        /// </returns>
        private static void ClearContentTypeCache(JsonPayload payload)
        {
            //clears the cache associated with the Content type itself
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheItem(string.Format("{0}{1}", CacheKeys.ContentTypeCacheKey, payload.Id));
            //clears the cache associated with the content type properties collection
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheItem(CacheKeys.ContentTypePropertiesCacheKey + payload.Id);

            //clears the dictionary object cache of the legacy ContentType
            global::umbraco.cms.businesslogic.ContentType.RemoveFromDataTypeCache(payload.Alias);

            PublishedContentType.ClearContentType(payload.Id);

            //need to recursively clear the cache for each child content type
            foreach (var descendant in payload.DescendantPayloads)
            {
                ClearContentTypeCache(descendant);
            }            
        }

        /// <summary>
        /// Clears the cache for any content type with the specified Ids
        /// </summary>
        /// <param name="isDeleted">true if the entity was deleted, false if it is just an update</param>
        /// <param name="ids"></param>
        private void ClearContentTypeCache(bool isDeleted, params int[] ids)
        {
            ClearContentTypeCache(
                ids.Select(
                    x =>
                    ApplicationContext.Current.Services.ContentTypeService.GetContentType(x) as IContentTypeBase)
                   .WhereNotNull()
                   .Select(x => FromContentType(x, isDeleted))
                   .ToArray());
        }

    }
}
