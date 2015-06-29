using System;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using umbraco;
using umbraco.cms.businesslogic.web;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// Extension methods for <see cref="DistributedCache"/>
    /// </summary>
    internal static class DistributedCacheExtensions
    {
        #region Public access

        public static void RefreshPublicAccess(this DistributedCache dc)
        {
            dc.RefreshAll(DistributedCache.PublicAccessCacheRefresherGuid);
        }

        #endregion

        #region Application tree cache

        public static void RefreshAllApplicationTreeCache(this DistributedCache dc)
        {
            dc.RefreshAll(DistributedCache.ApplicationTreeCacheRefresherGuid);
        }

        #endregion

        #region Application cache

        public static void RefreshAllApplicationCache(this DistributedCache dc)
        {
            dc.RefreshAll(DistributedCache.ApplicationCacheRefresherGuid);
        }

        #endregion

        #region User type cache

        public static void RemoveUserTypeCache(this DistributedCache dc, int userTypeId)
        {
            dc.Remove(DistributedCache.UserTypeCacheRefresherGuid, userTypeId);
        }

        public static void RefreshUserTypeCache(this DistributedCache dc, int userTypeId)
        {
            dc.Refresh(DistributedCache.UserTypeCacheRefresherGuid, userTypeId);
        }

        public static void RefreshAllUserTypeCache(this DistributedCache dc)
        {
            dc.RefreshAll(DistributedCache.UserTypeCacheRefresherGuid);
        }

        #endregion

        #region User cache

        public static void RemoveUserCache(this DistributedCache dc, int userId)
        {
            dc.Remove(DistributedCache.UserCacheRefresherGuid, userId);
        }

        public static void RefreshUserCache(this DistributedCache dc, int userId)
        {
            dc.Refresh(DistributedCache.UserCacheRefresherGuid, userId);
        }

        public static void RefreshAllUserCache(this DistributedCache dc)
        {
            dc.RefreshAll(DistributedCache.UserCacheRefresherGuid);
        } 

        #endregion

        #region User permissions cache

        public static void RemoveUserPermissionsCache(this DistributedCache dc, int userId)
        {
            dc.Remove(DistributedCache.UserPermissionsCacheRefresherGuid, userId);
        }

        public static void RefreshUserPermissionsCache(this DistributedCache dc, int userId)
        {
            dc.Refresh(DistributedCache.UserPermissionsCacheRefresherGuid, userId);
        }

        public static void RefreshAllUserPermissionsCache(this DistributedCache dc)
        {
            dc.RefreshAll(DistributedCache.UserPermissionsCacheRefresherGuid);
        }

        #endregion

        #region Template cache

        public static void RefreshTemplateCache(this DistributedCache dc, int templateId)
        {
            dc.Refresh(DistributedCache.TemplateRefresherGuid, templateId);
        }

        public static void RemoveTemplateCache(this DistributedCache dc, int templateId)
        {
            dc.Remove(DistributedCache.TemplateRefresherGuid, templateId);
        } 

        #endregion

        #region Dictionary cache

        public static void RefreshDictionaryCache(this DistributedCache dc, int dictionaryItemId)
        {
            dc.Refresh(DistributedCache.DictionaryCacheRefresherGuid, dictionaryItemId);
        }

        public static void RemoveDictionaryCache(this DistributedCache dc, int dictionaryItemId)
        {
            dc.Remove(DistributedCache.DictionaryCacheRefresherGuid, dictionaryItemId);
        }

        #endregion
        
        #region Data type cache     

        public static void RefreshDataTypeCache(this DistributedCache dc, IDataTypeDefinition dataType)
        {
            if (dataType == null) return;
            dc.RefreshByJson(DistributedCache.DataTypeCacheRefresherGuid, DataTypeCacheRefresher.SerializeToJsonPayload(dataType));
        }

        public static void RemoveDataTypeCache(this DistributedCache dc, IDataTypeDefinition dataType)
        {
            if (dataType == null) return;
            dc.RefreshByJson(DistributedCache.DataTypeCacheRefresherGuid, DataTypeCacheRefresher.SerializeToJsonPayload(dataType));
        }

        #endregion

        #region Page cache

        public static void RefreshAllPageCache(this DistributedCache dc)
        {
            dc.RefreshAll(DistributedCache.PageCacheRefresherGuid);
        }

        public static void RefreshPageCache(this DistributedCache dc, int documentId)
        {
            dc.Refresh(DistributedCache.PageCacheRefresherGuid, documentId);
        }

        public static void RefreshPageCache(this DistributedCache dc, params IContent[] content)
        {
            dc.Refresh(DistributedCache.PageCacheRefresherGuid, x => x.Id, content);
        }

        public static void RemovePageCache(this DistributedCache dc, params IContent[] content)
        {
            dc.Remove(DistributedCache.PageCacheRefresherGuid, x => x.Id, content);
        }

        public static void RemovePageCache(this DistributedCache dc, int documentId)
        {
            dc.Remove(DistributedCache.PageCacheRefresherGuid, documentId);
        }

        public static void RefreshUnpublishedPageCache(this DistributedCache dc, params IContent[] content)
        {
            dc.Refresh(DistributedCache.UnpublishedPageCacheRefresherGuid, x => x.Id, content);
        }

        public static void RemoveUnpublishedPageCache(this DistributedCache dc, params IContent[] content)
        {
            dc.Remove(DistributedCache.UnpublishedPageCacheRefresherGuid, x => x.Id, content);
        }

        public static void RemoveUnpublishedCachePermanently(this DistributedCache dc, params int[] contentIds)
        {
            dc.RefreshByJson(DistributedCache.UnpublishedPageCacheRefresherGuid, UnpublishedPageCacheRefresher.SerializeToJsonPayloadForPermanentDeletion(contentIds));
        }

        #endregion

        #region Member cache

        public static void RefreshMemberCache(this DistributedCache dc, params IMember[] members)
        {
            dc.Refresh(DistributedCache.MemberCacheRefresherGuid, x => x.Id, members);
        }

        public static void RemoveMemberCache(this DistributedCache dc, params IMember[] members)
        {
            dc.Remove(DistributedCache.MemberCacheRefresherGuid, x => x.Id, members);
        } 

        [Obsolete("Use the RefreshMemberCache with strongly typed IMember objects instead")]
        public static void RefreshMemberCache(this DistributedCache dc, int memberId)
        {
            dc.Refresh(DistributedCache.MemberCacheRefresherGuid, memberId);
        }

        [Obsolete("Use the RemoveMemberCache with strongly typed IMember objects instead")]
        public static void RemoveMemberCache(this DistributedCache dc, int memberId)
        {
            dc.Remove(DistributedCache.MemberCacheRefresherGuid, memberId);
        } 

        #endregion

        #region Member group cache

        public static void RefreshMemberGroupCache(this DistributedCache dc, int memberGroupId)
        {
            dc.Refresh(DistributedCache.MemberGroupCacheRefresherGuid, memberGroupId);
        }

        public static void RemoveMemberGroupCache(this DistributedCache dc, int memberGroupId)
        {
            dc.Remove(DistributedCache.MemberGroupCacheRefresherGuid, memberGroupId);
        }

        #endregion

        #region Media Cache
        
        public static void RefreshMediaCache(this DistributedCache dc, params IMedia[] media)
        {
            dc.RefreshByJson(DistributedCache.MediaCacheRefresherGuid, MediaCacheRefresher.SerializeToJsonPayload(MediaCacheRefresher.OperationType.Saved, media));
        }

        public static void RefreshMediaCacheAfterMoving(this DistributedCache dc, params MoveEventInfo<IMedia>[] media)
        {
            dc.RefreshByJson(DistributedCache.MediaCacheRefresherGuid, MediaCacheRefresher.SerializeToJsonPayloadForMoving(MediaCacheRefresher.OperationType.Saved, media));
        }

        // clearing by Id will never work for load balanced scenarios for media since we require a Path
        // to clear all of the cache but the media item will be removed before the other servers can
        // look it up. Only here for legacy purposes.
        [Obsolete("Ensure to clear with other RemoveMediaCache overload")]
        public static void RemoveMediaCache(this DistributedCache dc, int mediaId)
        {
            dc.Remove(new Guid(DistributedCache.MediaCacheRefresherId), mediaId);
        }

        public static void RemoveMediaCacheAfterRecycling(this DistributedCache dc, params MoveEventInfo<IMedia>[] media)
        {
            dc.RefreshByJson(DistributedCache.MediaCacheRefresherGuid, MediaCacheRefresher.SerializeToJsonPayloadForMoving(MediaCacheRefresher.OperationType.Trashed, media));
        }

        public static void RemoveMediaCachePermanently(this DistributedCache dc, params int[] mediaIds)
        {
            dc.RefreshByJson(DistributedCache.MediaCacheRefresherGuid, MediaCacheRefresher.SerializeToJsonPayloadForPermanentDeletion(mediaIds));
        }

        #endregion

        #region Macro Cache

        public static void ClearAllMacroCacheOnCurrentServer(this DistributedCache dc)
        {
            // NOTE: The 'false' ensure that it will only refresh on the current server, not post to all servers
            dc.RefreshAll(DistributedCache.MacroCacheRefresherGuid, false);
        }

        public static void RefreshMacroCache(this DistributedCache dc, IMacro macro)
        {
            if (macro == null) return;
            dc.RefreshByJson(DistributedCache.MacroCacheRefresherGuid, MacroCacheRefresher.SerializeToJsonPayload(macro));
        }

        public static void RemoveMacroCache(this DistributedCache dc, IMacro macro)
        {
            if (macro == null) return;
            dc.RefreshByJson(DistributedCache.MacroCacheRefresherGuid, MacroCacheRefresher.SerializeToJsonPayload(macro));
        }

        public static void RefreshMacroCache(this DistributedCache dc, global::umbraco.cms.businesslogic.macro.Macro macro)
        {
            if (macro == null) return;
            dc.RefreshByJson(DistributedCache.MacroCacheRefresherGuid, MacroCacheRefresher.SerializeToJsonPayload(macro));
        }
        
        public static void RemoveMacroCache(this DistributedCache dc, global::umbraco.cms.businesslogic.macro.Macro macro)
        {
            if (macro == null) return;
            dc.RefreshByJson(DistributedCache.MacroCacheRefresherGuid, MacroCacheRefresher.SerializeToJsonPayload(macro));
        }

        public static void RemoveMacroCache(this DistributedCache dc, macro macro)
        {
            if (macro == null || macro.Model == null) return;
            dc.RefreshByJson(DistributedCache.MacroCacheRefresherGuid, MacroCacheRefresher.SerializeToJsonPayload(macro));
        } 

        #endregion

        #region Document type cache

        public static void RefreshContentTypeCache(this DistributedCache dc, IContentType contentType)
        {
            if (contentType == null) return;
            dc.RefreshByJson(DistributedCache.ContentTypeCacheRefresherGuid, ContentTypeCacheRefresher.SerializeToJsonPayload(false, contentType));
        }

        public static void RemoveContentTypeCache(this DistributedCache dc, IContentType contentType)
        {
            if (contentType == null) return;
            dc.RefreshByJson(DistributedCache.ContentTypeCacheRefresherGuid, ContentTypeCacheRefresher.SerializeToJsonPayload(true, contentType));
        }

        #endregion

        #region Media type cache

        public static void RefreshMediaTypeCache(this DistributedCache dc, IMediaType mediaType)
        {
            if (mediaType == null) return;
            dc.RefreshByJson(DistributedCache.ContentTypeCacheRefresherGuid, ContentTypeCacheRefresher.SerializeToJsonPayload(false, mediaType));
        }

        public static void RemoveMediaTypeCache(this DistributedCache dc, IMediaType mediaType)
        {
            if (mediaType == null) return;
            dc.RefreshByJson(DistributedCache.ContentTypeCacheRefresherGuid, ContentTypeCacheRefresher.SerializeToJsonPayload(true, mediaType));
        }

        #endregion

        #region Media type cache

        public static void RefreshMemberTypeCache(this DistributedCache dc, IMemberType memberType)
        {
            if (memberType == null) return;
            dc.RefreshByJson(DistributedCache.ContentTypeCacheRefresherGuid, ContentTypeCacheRefresher.SerializeToJsonPayload(false, memberType));
        }

        public static void RemoveMemberTypeCache(this DistributedCache dc, IMemberType memberType)
        {
            if (memberType == null) return;
            dc.RefreshByJson(DistributedCache.ContentTypeCacheRefresherGuid, ContentTypeCacheRefresher.SerializeToJsonPayload(true, memberType));
        }

        #endregion

        #region Stylesheet Cache

        public static void RefreshStylesheetPropertyCache(this DistributedCache dc, global::umbraco.cms.businesslogic.web.StylesheetProperty styleSheetProperty)
        {
            if (styleSheetProperty == null) return;
            dc.Refresh(DistributedCache.StylesheetPropertyCacheRefresherGuid, styleSheetProperty.Id);
        }

        public static void RemoveStylesheetPropertyCache(this DistributedCache dc, global::umbraco.cms.businesslogic.web.StylesheetProperty styleSheetProperty)
        {
            if (styleSheetProperty == null) return;
            dc.Remove(DistributedCache.StylesheetPropertyCacheRefresherGuid, styleSheetProperty.Id);
        }

        public static void RefreshStylesheetCache(this DistributedCache dc, StyleSheet styleSheet)
        {
            if (styleSheet == null) return;
            dc.Refresh(DistributedCache.StylesheetCacheRefresherGuid, styleSheet.Id);
        }

        public static void RemoveStylesheetCache(this DistributedCache dc, StyleSheet styleSheet)
        {
            if (styleSheet == null) return;
            dc.Remove(DistributedCache.StylesheetCacheRefresherGuid, styleSheet.Id);
        }

        public static void RefreshStylesheetCache(this DistributedCache dc, Umbraco.Core.Models.Stylesheet styleSheet)
        {
            if (styleSheet == null) return;
            dc.Refresh(DistributedCache.StylesheetCacheRefresherGuid, styleSheet.Id);
        }

        public static void RemoveStylesheetCache(this DistributedCache dc, Umbraco.Core.Models.Stylesheet styleSheet)
        {
            if (styleSheet == null) return;
            dc.Remove(DistributedCache.StylesheetCacheRefresherGuid, styleSheet.Id);
        }

        #endregion

        #region Domain Cache

        public static void RefreshDomainCache(this DistributedCache dc, IDomain domain)
        {
            if (domain == null) return;
            dc.Refresh(DistributedCache.DomainCacheRefresherGuid, domain.Id);
        }

        public static void RemoveDomainCache(this DistributedCache dc, IDomain domain)
        {
            if (domain == null) return;
            dc.Remove(DistributedCache.DomainCacheRefresherGuid, domain.Id);
        }

        #endregion

        #region Language Cache

        public static void RefreshLanguageCache(this DistributedCache dc, ILanguage language)
        {
            if (language == null) return;
            dc.Refresh(DistributedCache.LanguageCacheRefresherGuid, language.Id);
        }

        public static void RemoveLanguageCache(this DistributedCache dc, ILanguage language)
        {
            if (language == null) return;
            dc.Remove(DistributedCache.LanguageCacheRefresherGuid, language.Id);
        }

        public static void RefreshLanguageCache(this DistributedCache dc, global::umbraco.cms.businesslogic.language.Language language)
        {
            if (language == null) return;
            dc.Refresh(DistributedCache.LanguageCacheRefresherGuid, language.id);
        }

        public static void RemoveLanguageCache(this DistributedCache dc, global::umbraco.cms.businesslogic.language.Language language)
        {
            if (language == null) return;
            dc.Remove(DistributedCache.LanguageCacheRefresherGuid, language.id);
        }

        #endregion

        #region Xslt Cache

        public static void ClearXsltCacheOnCurrentServer(this DistributedCache dc)
        {
            if (UmbracoConfig.For.UmbracoSettings().Content.UmbracoLibraryCacheDuration <= 0) return;
            ApplicationContext.Current.ApplicationCache.ClearCacheObjectTypes("MS.Internal.Xml.XPath.XPathSelectionIterator");
        }

        #endregion
    }
}