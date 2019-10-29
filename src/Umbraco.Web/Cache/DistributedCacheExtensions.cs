﻿using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Services.Changes;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// Extension methods for <see cref="DistributedCache"/>.
    /// </summary>
    internal static class DistributedCacheExtensions
    {
        #region PublicAccessCache

        public static void RefreshPublicAccess(this DistributedCache dc)
        {
            dc.RefreshAll(PublicAccessCacheRefresher.UniqueId);
        }

        #endregion

        #region User cache

        public static void RemoveUserCache(this DistributedCache dc, int userId)
        {
            dc.Remove(UserCacheRefresher.UniqueId, userId);
        }

        public static void RefreshUserCache(this DistributedCache dc, int userId)
        {
            dc.Refresh(UserCacheRefresher.UniqueId, userId);
        }

        public static void RefreshAllUserCache(this DistributedCache dc)
        {
            dc.RefreshAll(UserCacheRefresher.UniqueId);
        }

        #endregion

        #region User group cache

        public static void RemoveUserGroupCache(this DistributedCache dc, int userId)
        {
            dc.Remove(UserGroupCacheRefresher.UniqueId, userId);
        }

        public static void RefreshUserGroupCache(this DistributedCache dc, int userId)
        {
            dc.Refresh(UserGroupCacheRefresher.UniqueId, userId);
        }

        public static void RefreshAllUserGroupCache(this DistributedCache dc)
        {
            dc.RefreshAll(UserGroupCacheRefresher.UniqueId);
        }

        #endregion

        #region TemplateCache

        public static void RefreshTemplateCache(this DistributedCache dc, int templateId)
        {
            dc.Refresh(TemplateCacheRefresher.UniqueId, templateId);
        }

        public static void RemoveTemplateCache(this DistributedCache dc, int templateId)
        {
            dc.Remove(TemplateCacheRefresher.UniqueId, templateId);
        }

        #endregion

        #region DictionaryCache

        public static void RefreshDictionaryCache(this DistributedCache dc, int dictionaryItemId)
        {
            dc.Refresh(DictionaryCacheRefresher.UniqueId, dictionaryItemId);
        }

        public static void RemoveDictionaryCache(this DistributedCache dc, int dictionaryItemId)
        {
            dc.Remove(DictionaryCacheRefresher.UniqueId, dictionaryItemId);
        }

        #endregion

        #region DataTypeCache

        public static void RefreshDataTypeCache(this DistributedCache dc, IDataType dataType)
        {
            if (dataType == null) return;
            var payloads = new[] { new DataTypeCacheRefresher.JsonPayload(dataType.Id, dataType.Key, false) };
            dc.RefreshByPayload(DataTypeCacheRefresher.UniqueId, payloads);
        }

        public static void RemoveDataTypeCache(this DistributedCache dc, IDataType dataType)
        {
            if (dataType == null) return;
            var payloads = new[] { new DataTypeCacheRefresher.JsonPayload(dataType.Id, dataType.Key, true) };
            dc.RefreshByPayload(DataTypeCacheRefresher.UniqueId, payloads);
        }

        #endregion

        #region ContentCache

        public static void RefreshAllContentCache(this DistributedCache dc)
        {
            var payloads = new[] { new ContentCacheRefresher.JsonPayload(0, null, TreeChangeTypes.RefreshAll) };

            // note: refresh all content cache does refresh content types too
            dc.RefreshByPayload(ContentCacheRefresher.UniqueId, payloads);
        }

        public static void RefreshContentCache(this DistributedCache dc, TreeChange<IContent>[] changes)
        {
            if (changes.Length == 0) return;

            var payloads = changes
                .Select(x => new ContentCacheRefresher.JsonPayload(x.Item.Id, x.Item.Key, x.ChangeTypes));

            dc.RefreshByPayload(ContentCacheRefresher.UniqueId, payloads);
        }

        #endregion

        #region MemberCache

        public static void RefreshMemberCache(this DistributedCache dc, params IMember[] members)
        {
            dc.Refresh(MemberCacheRefresher.UniqueId, x => x.Id, members);
        }

        public static void RemoveMemberCache(this DistributedCache dc, params IMember[] members)
        {
            dc.Remove(MemberCacheRefresher.UniqueId, x => x.Id, members);
        }


        #endregion

        #region MemberGroupCache

        public static void RefreshMemberGroupCache(this DistributedCache dc, int memberGroupId)
        {
            dc.Refresh(MemberGroupCacheRefresher.UniqueId, memberGroupId);
        }

        public static void RemoveMemberGroupCache(this DistributedCache dc, int memberGroupId)
        {
            dc.Remove(MemberGroupCacheRefresher.UniqueId, memberGroupId);
        }

        #endregion

        #region MediaCache

        public static void RefreshAllMediaCache(this DistributedCache dc)
        {
            var payloads = new[] { new MediaCacheRefresher.JsonPayload(0, null, TreeChangeTypes.RefreshAll) };

            // note: refresh all media cache does refresh content types too
            dc.RefreshByPayload(MediaCacheRefresher.UniqueId, payloads);
        }

        public static void RefreshMediaCache(this DistributedCache dc, TreeChange<IMedia>[] changes)
        {
            if (changes.Length == 0) return;

            var payloads = changes
                .Select(x => new MediaCacheRefresher.JsonPayload(x.Item.Id, x.Item.Key, x.ChangeTypes));

            dc.RefreshByPayload(MediaCacheRefresher.UniqueId, payloads);
        }

        #endregion

        #region Published Snapshot

        public static void RefreshAllPublishedSnapshot(this DistributedCache dc)
        {
            // note: refresh all content & media caches does refresh content types too
            dc.RefreshAllContentCache();
            dc.RefreshAllMediaCache();
            dc.RefreshAllDomainCache();
        }

        #endregion

        #region MacroCache

        public static void RefreshMacroCache(this DistributedCache dc, IMacro macro)
        {
            if (macro == null) return;
            dc.RefreshByJson(MacroCacheRefresher.UniqueId, MacroCacheRefresher.Serialize(macro));
        }

        public static void RemoveMacroCache(this DistributedCache dc, IMacro macro)
        {
            if (macro == null) return;
            dc.RefreshByJson(MacroCacheRefresher.UniqueId, MacroCacheRefresher.Serialize(macro));
        }

        #endregion

        #region Content/Media/Member type cache

        public static void RefreshContentTypeCache(this DistributedCache dc, ContentTypeChange<IContentType>[] changes)
        {
            if (changes.Length == 0) return;

            var payloads = changes
                .Select(x => new ContentTypeCacheRefresher.JsonPayload(typeof (IContentType).Name, x.Item.Id, x.ChangeTypes));

            dc.RefreshByPayload(ContentTypeCacheRefresher.UniqueId, payloads);
        }

        public static void RefreshContentTypeCache(this DistributedCache dc, ContentTypeChange<IMediaType>[] changes)
        {
            if (changes.Length == 0) return;

            var payloads = changes
                .Select(x => new ContentTypeCacheRefresher.JsonPayload(typeof(IMediaType).Name, x.Item.Id, x.ChangeTypes));

            dc.RefreshByPayload(ContentTypeCacheRefresher.UniqueId, payloads);
        }

        public static void RefreshContentTypeCache(this DistributedCache dc, ContentTypeChange<IMemberType>[] changes)
        {
            if (changes.Length == 0) return;

            var payloads = changes
                .Select(x => new ContentTypeCacheRefresher.JsonPayload(typeof(IMemberType).Name, x.Item.Id, x.ChangeTypes));

            dc.RefreshByPayload(ContentTypeCacheRefresher.UniqueId, payloads);
        }

        #endregion

        #region Domain Cache

        public static void RefreshDomainCache(this DistributedCache dc, IDomain domain)
        {
            if (domain == null) return;
            var payloads = new[] { new DomainCacheRefresher.JsonPayload(domain.Id, DomainChangeTypes.Refresh) };
            dc.RefreshByPayload(DomainCacheRefresher.UniqueId, payloads);
        }

        public static void RemoveDomainCache(this DistributedCache dc, IDomain domain)
        {
            if (domain == null) return;
            var payloads = new[] { new DomainCacheRefresher.JsonPayload(domain.Id, DomainChangeTypes.Remove) };
            dc.RefreshByPayload(DomainCacheRefresher.UniqueId, payloads);
        }

        public static void RefreshAllDomainCache(this DistributedCache dc)
        {
            var payloads = new[] { new DomainCacheRefresher.JsonPayload(0, DomainChangeTypes.RefreshAll) };
            dc.RefreshByPayload(DomainCacheRefresher.UniqueId, payloads);
        }

        #endregion

        #region Language Cache

        public static void RefreshLanguageCache(this DistributedCache dc, ILanguage language)
        {
            if (language == null) return;

            var payload = new LanguageCacheRefresher.JsonPayload(language.Id, language.IsoCode,
                language.WasPropertyDirty(nameof(ILanguage.IsoCode))
                    ? LanguageCacheRefresher.JsonPayload.LanguageChangeType.ChangeCulture
                    : LanguageCacheRefresher.JsonPayload.LanguageChangeType.Update);

            dc.RefreshByPayload(LanguageCacheRefresher.UniqueId, new[] { payload });
        }

        public static void RemoveLanguageCache(this DistributedCache dc, ILanguage language)
        {
            if (language == null) return;

            var payload = new LanguageCacheRefresher.JsonPayload(language.Id, language.IsoCode, LanguageCacheRefresher.JsonPayload.LanguageChangeType.Remove);
            dc.RefreshByPayload(LanguageCacheRefresher.UniqueId, new[] { payload });
        }

        #endregion

        #region Relation type cache

        public static void RefreshRelationTypeCache(this DistributedCache dc, int id)
        {
            dc.Refresh(RelationTypeCacheRefresher.UniqueId, id);
        }

        public static void RemoveRelationTypeCache(this DistributedCache dc, int id)
        {
            dc.Remove(RelationTypeCacheRefresher.UniqueId, id);
        }

        #endregion
    }
}
