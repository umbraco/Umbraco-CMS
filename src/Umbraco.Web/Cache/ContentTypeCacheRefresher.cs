using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Caching;

namespace Umbraco.Web.Cache
{
    public class ContentTypeCacheRefresher : ICacheRefresher<IContentType>, ICacheRefresher<IMediaType>
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

        private static void ClearContentTypeCache(params IContentTypeBase[] contentTypes)
        {
            contentTypes.ForEach(contentType =>
                {
                    //clears the cache for each property type associated with the content type
                    foreach (var p in contentType.PropertyTypes)
                    {
                        ApplicationContext.Current.ApplicationCache.ClearCacheItem(CacheKeys.PropertyTypeCacheKey + p.Id);
                    }
                    //clears the cache for all tabs
                    var regexMatch = string.Format("{0}_{1}_{2}", CacheKeys.PropertyTypeTabCacheKey, @"\d+", contentType.Id);
                    ApplicationContext.Current.ApplicationCache.ClearCacheByKeyExpression(regexMatch);
                    //clears the cache associated with the content type properties collection
                    ApplicationContext.Current.ApplicationCache.ClearCacheItem(CacheKeys.ContentTypePropertiesCacheKey + contentType.Id);
                    //clears the dictionary object cache of the legacy ContentType
                    global::umbraco.cms.businesslogic.ContentType.RemoveFromDataTypeCache(contentType.Alias);
                });
            if (contentTypes.Any())
            {
                InMemoryCacheProvider.Current.Clear();
                RuntimeCacheProvider.Current.Clear();   
            }            
        }

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
