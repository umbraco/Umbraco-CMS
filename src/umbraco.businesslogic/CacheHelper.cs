//using System;
//using System.Web.Caching;

//namespace umbraco.BusinessLogic
//{
//    internal class CacheHelper
//    {
//        public delegate TT GetCacheItemDelegate<TT>();
//        public static TT GetCacheItem<TT>(string cacheKey, object syncLock,
//            CacheItemPriority priority, CacheItemRemovedCallback refreshAction,
//            CacheDependency cacheDependency, TimeSpan timeout, GetCacheItemDelegate<TT> getCacheItem)
//        {
//            object result = System.Web.HttpRuntime.Cache.Get(cacheKey);
//            if (result == null)
//            {
//                lock (syncLock)
//                {
//                    result = System.Web.HttpRuntime.Cache.Get(cacheKey);
//                    if (result == null)
//                    {
//                        result = getCacheItem();
//                        System.Web.HttpRuntime.Cache.Add(cacheKey, result, cacheDependency,
//                            DateTime.Now.Add(timeout), TimeSpan.Zero, priority, refreshAction);
//                    }
//                }
//            }
//            return (TT)result;
//        }

//    }
//}
