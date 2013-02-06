using System;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// Extension methods for DistrubutedCache
    /// </summary>
    public static class DistributedCacheExtensions
    {
        /// <summary>
        /// Refreshes the cache amongst servers for a template
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="templateId"></param>
        public static void RefreshTemplateCache(this DistributedCache dc, int templateId)
        {
            dc.Refresh(new Guid(DistributedCache.TemplateRefresherId), templateId);
        }

        /// <summary>
        /// Removes the cache amongst servers for a template
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="templateId"></param>
        public static void RemoveTemplateCache(this DistributedCache dc, int templateId)
        {
            dc.Remove(new Guid(DistributedCache.TemplateRefresherId), templateId);
        }

        /// <summary>
        /// Refreshes the cache amongst servers for all pages
        /// </summary>
        /// <param name="dc"></param>
        public static void RefreshAllPageCache(this DistributedCache dc)
        {
            dc.RefreshAll(new Guid(DistributedCache.PageCacheRefresherId));
        }

        /// <summary>
        /// Refreshes the cache amongst servers for a page
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="pageId"></param>
        public static void RefreshPageCache(this DistributedCache dc, int pageId)
        {
            dc.Refresh(new Guid(DistributedCache.PageCacheRefresherId), pageId);
        }

        /// <summary>
        /// Removes the cache amongst servers for a page
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="pageId"></param>
        public static void RemovePageCache(this DistributedCache dc, int pageId)
        {
            dc.Remove(new Guid(DistributedCache.PageCacheRefresherId), pageId);
        }

        /// <summary>
        /// Refreshes the cache amongst servers for a member
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="memberId"></param>
        public static void RefreshMemberCache(this DistributedCache dc, int memberId)
        {
            dc.Refresh(new Guid(DistributedCache.MemberCacheRefresherId), memberId);
        }

        /// <summary>
        /// Removes the cache amongst servers for a member
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="memberId"></param>
        public static void RemoveMemberCache(this DistributedCache dc, int memberId)
        {
            dc.Remove(new Guid(DistributedCache.MemberCacheRefresherId), memberId);
        }

        /// <summary>
        /// Refreshes the cache amongst servers for a media item
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="mediaId"></param>
        public static void RefreshMediaCache(this DistributedCache dc, int mediaId)
        {
            dc.Refresh(new Guid(DistributedCache.MediaCacheRefresherId), mediaId);
        }

        /// <summary>
        /// Removes the cache amongst servers for a media item
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="mediaId"></param>
        public static void RemoveMediaCache(this DistributedCache dc, int mediaId)
        {
            dc.Remove(new Guid(DistributedCache.MediaCacheRefresherId), mediaId);
        }

        /// <summary>
        /// Refreshes the cache amongst servers for a macro item
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="macroId"></param>
        public static void RefreshMacroCache(this DistributedCache dc, int macroId)
        {
            dc.Refresh(new Guid(DistributedCache.MacroCacheRefresherId), macroId);
        }

        /// <summary>
        /// Removes the cache amongst servers for a macro item
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="macroId"></param>
        public static void RemoveMacroCache(this DistributedCache dc, int macroId)
        {
            dc.Remove(new Guid(DistributedCache.MacroCacheRefresherId), macroId);
        }
    }
}