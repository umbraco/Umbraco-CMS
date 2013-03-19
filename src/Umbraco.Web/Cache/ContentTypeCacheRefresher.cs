using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Caching;

namespace Umbraco.Web.Cache
{

    /// <summary>
    /// A cache refresher to ensure content type cache is updated when members change
    /// </summary>
    /// <remarks>
    /// This is not intended to be used directly in your code
    /// </remarks>
    public sealed class ContentTypeCacheRefresher : ICacheRefresher<IContentType>, ICacheRefresher<IMediaType>
    {
        public Guid UniqueIdentifier
        {
            get { return new Guid(DistributedCache.ContentTypeCacheRefresherId); }
        }
        public string Name
        {
            get { return "ContentTypeCacheRefresher"; }
        }

        public void RefreshAll()
        {
            //all property type cache
            ApplicationContext.Current.ApplicationCache.ClearCacheByKeySearch(CacheKeys.PropertyTypeCacheKey);
            //all content type property cache
            ApplicationContext.Current.ApplicationCache.ClearCacheByKeySearch(CacheKeys.ContentTypePropertiesCacheKey);     
            //all content type cache
            ApplicationContext.Current.ApplicationCache.ClearCacheByKeySearch(CacheKeys.ContentTypeCacheKey);
            //clear static object cache
            global::umbraco.cms.businesslogic.ContentType.RemoveAllDataTypeCache();
        }

        public void Refresh(int id)
        {
            ClearContentTypeCache(id);
        }

        public void Remove(int id)
        {
            ClearContentTypeCache(id);
        }

        public void Refresh(Guid id)
        {
        }

        public void Refresh(IContentType instance)
        {
            ClearContentTypeCache(instance);
        }

        public void Remove(IContentType instance)
        {
            ClearContentTypeCache(instance);
        }

        public void Refresh(IMediaType instance)
        {
            ClearContentTypeCache(instance);
        }

        public void Remove(IMediaType instance)
        {
            ClearContentTypeCache(instance);
        }

        /// <summary>
        /// This clears out all cache associated with a content type
        /// </summary>
        /// <param name="contentTypes"></param>
        /// <remarks>
        /// The cache that is required to be cleared when a content type is updated is as follows:
        /// - ApplicationCache (keys to clear):
        /// -- CacheKeys.PropertyTypeCacheKey + propertyType.Id (each property type assigned)
        /// -- CacheKeys.ContentTypePropertiesCacheKey + contentType.Id
        /// - ContentType.RemoveFromDataTypeCache (clears static object/dictionary cache)
        /// - InMemoryCacheProvider.Current.Clear();
        /// - RuntimeCacheProvider.Current.Clear(); 
        ///         
        /// TODO: Needs to update any content items that this effects for the xml cache... 
        ///       it is only handled in the ContentTypeControlNew.ascx, not by business logic/events. - The xml cache needs to be updated 
        ///       when the doc type alias changes or when a property type is removed, the ContentService.RePublishAll should be executed anytime either of these happens.
        /// </remarks>
        private static void ClearContentTypeCache(params IContentTypeBase[] contentTypes)
        {
            var needsContentRefresh = false;
            
            contentTypes.ForEach(contentType =>
                {
                    //clear the cache for each item
                    ClearContentTypeCache(contentType);

                    //here we need to check if the alias of the content type changed or if one of the properties was removed.                    
                    var dirty = contentType as IRememberBeingDirty;
                    if (dirty == null) return;
                    if (dirty.WasPropertyDirty("Alias") || dirty.WasPropertyDirty("HasPropertyTypeBeenRemoved"))
                    {
                        needsContentRefresh = true;
                    }
                });

            //need to refresh the xml content cache if required
            if (needsContentRefresh)
            {
                var pageRefresher = CacheRefreshersResolver.Current.GetById(new Guid(DistributedCache.PageCacheRefresherId));
                pageRefresher.RefreshAll();
            }

            //clear the cache providers if there were any content types to clear
            if (contentTypes.Any())
            {
                InMemoryCacheProvider.Current.Clear();
                RuntimeCacheProvider.Current.Clear();   
            }
        }

        /// <summary>
        /// Clears cache for an individual IContentTypeBase object
        /// </summary>
        /// <param name="contentType"></param>
        /// <remarks>
        /// See notes for the other overloaded ClearContentTypeCache for 
        /// full details on clearing cache.
        /// </remarks>
        /// <returns>
        /// Return true if the alias of the content type changed
        /// </returns>
        private static void ClearContentTypeCache(IContentTypeBase contentType)
        {
            //clears the cache for each property type associated with the content type
            foreach (var p in contentType.PropertyTypes)
            {
                ApplicationContext.Current.ApplicationCache.ClearCacheItem(CacheKeys.PropertyTypeCacheKey + p.Id);
            }
            //clears the cache associated with the Content type itself
            ApplicationContext.Current.ApplicationCache.ClearCacheItem(string.Format("{0}{1}", CacheKeys.ContentTypeCacheKey, contentType.Id));
            //clears the cache associated with the content type properties collection
            ApplicationContext.Current.ApplicationCache.ClearCacheItem(CacheKeys.ContentTypePropertiesCacheKey + contentType.Id);
            
            //clears the dictionary object cache of the legacy ContentType
            global::umbraco.cms.businesslogic.ContentType.RemoveFromDataTypeCache(contentType.Alias);

            //need to recursively clear the cache for each child content type
            // performance related to http://issues.umbraco.org/issue/U4-1714
            var dtos = ApplicationContext.Current.DatabaseContext.Database.Fetch<ContentType2ContentTypeDto>("WHERE parentContentTypeId = @Id", new { Id = contentType.Id });
            foreach (var dto in dtos)
            {
                ClearContentTypeCache(dto.ChildId);
            }            
        }

        /// <summary>
        /// Clears the cache for any content type with the specified Ids
        /// </summary>
        /// <param name="ids"></param>
        private static void ClearContentTypeCache(params int[] ids)
        {
            ClearContentTypeCache(
                ids.Select(
                    x =>
                    ApplicationContext.Current.Services.ContentTypeService.GetContentType(x) as IContentTypeBase)
                   .WhereNotNull()
                   .ToArray());
        }

        
    }
}
