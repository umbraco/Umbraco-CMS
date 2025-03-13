// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Extensions;

/// <summary>
/// Extension methods for <see cref="DistributedCache" />.
/// </summary>
public static class DistributedCacheExtensions
{
    #region PublicAccessCacheRefresher

    public static void RefreshPublicAccess(this DistributedCache dc)
        => dc.RefreshAll(PublicAccessCacheRefresher.UniqueId);

    #endregion

    #region UserCacheRefresher

    public static void RemoveUserCache(this DistributedCache dc, int userId)
        => dc.Remove(UserCacheRefresher.UniqueId, userId);

    public static void RemoveUserCache(this DistributedCache dc, IEnumerable<IUser> users)
        => dc.Remove(UserCacheRefresher.UniqueId, users.Select(x => x.Id).Distinct().ToArray());

    public static void RefreshUserCache(this DistributedCache dc, int userId)
        => dc.Refresh(UserCacheRefresher.UniqueId, userId);

    public static void RefreshUserCache(this DistributedCache dc, IEnumerable<IUser> users)
        => dc.Refresh(UserCacheRefresher.UniqueId, users.Select(x => x.Id).Distinct().ToArray());

    public static void RefreshAllUserCache(this DistributedCache dc)
        => dc.RefreshAll(UserCacheRefresher.UniqueId);

    #endregion

    #region UserGroupCacheRefresher

    public static void RemoveUserGroupCache(this DistributedCache dc, int userId)
        => dc.Remove(UserGroupCacheRefresher.UniqueId, userId);

    public static void RemoveUserGroupCache(this DistributedCache dc, IEnumerable<IUserGroup> userGroups)
        => dc.Remove(UserGroupCacheRefresher.UniqueId, userGroups.Select(x => x.Id).Distinct().ToArray());

    public static void RefreshUserGroupCache(this DistributedCache dc, int userId)
        => dc.Refresh(UserGroupCacheRefresher.UniqueId, userId);

    public static void RefreshUserGroupCache(this DistributedCache dc, IEnumerable<IUserGroup> userGroups)
        => dc.Refresh(UserGroupCacheRefresher.UniqueId, userGroups.Select(x => x.Id).Distinct().ToArray());

    public static void RefreshAllUserGroupCache(this DistributedCache dc)
        => dc.RefreshAll(UserGroupCacheRefresher.UniqueId);

    #endregion

    #region TemplateCacheRefresher

    public static void RefreshTemplateCache(this DistributedCache dc, int templateId)
        => dc.Refresh(TemplateCacheRefresher.UniqueId, templateId);

    public static void RefreshTemplateCache(this DistributedCache dc, IEnumerable<ITemplate> templates)
        => dc.Refresh(TemplateCacheRefresher.UniqueId, templates.Select(x => x.Id).Distinct().ToArray());

    public static void RemoveTemplateCache(this DistributedCache dc, int templateId)
        => dc.Remove(TemplateCacheRefresher.UniqueId, templateId);

    public static void RemoveTemplateCache(this DistributedCache dc, IEnumerable<ITemplate> templates)
        => dc.Remove(TemplateCacheRefresher.UniqueId, templates.Select(x => x.Id).Distinct().ToArray());

    #endregion

    #region DictionaryCacheRefresher

    public static void RefreshDictionaryCache(this DistributedCache dc, int dictionaryItemId)
        => dc.Refresh(DictionaryCacheRefresher.UniqueId, dictionaryItemId);

    public static void RefreshDictionaryCache(this DistributedCache dc, IEnumerable<IDictionaryItem> dictionaryItems)
        => dc.Refresh(DictionaryCacheRefresher.UniqueId, dictionaryItems.Select(x => x.Id).Distinct().ToArray());

    public static void RemoveDictionaryCache(this DistributedCache dc, int dictionaryItemId)
        => dc.Remove(DictionaryCacheRefresher.UniqueId, dictionaryItemId);

    public static void RemoveDictionaryCache(this DistributedCache dc, IEnumerable<IDictionaryItem> dictionaryItems)
        => dc.Remove(DictionaryCacheRefresher.UniqueId, dictionaryItems.Select(x => x.Id).Distinct().ToArray());

    #endregion

    #region DataTypeCacheRefresher

    [Obsolete("Use the overload accepting IEnumerable instead. This overload will be removed in Umbraco 13.")]
    public static void RefreshDataTypeCache(this DistributedCache dc, IDataType dataType)
        => dc.RefreshDataTypeCache(dataType.Yield());

    public static void RefreshDataTypeCache(this DistributedCache dc, IEnumerable<IDataType> dataTypes)
        => dc.RefreshByPayload(DataTypeCacheRefresher.UniqueId, dataTypes.DistinctBy(x => (x.Id, x.Key)).Select(x => new DataTypeCacheRefresher.JsonPayload(x.Id, x.Key, false)));

    [Obsolete("Use the overload accepting IEnumerable instead. This overload will be removed in Umbraco 13.")]
    public static void RemoveDataTypeCache(this DistributedCache dc, IDataType dataType)
        => dc.RemoveDataTypeCache(dataType.Yield());

    public static void RemoveDataTypeCache(this DistributedCache dc, IEnumerable<IDataType> dataTypes)
        => dc.RefreshByPayload(DataTypeCacheRefresher.UniqueId, dataTypes.DistinctBy(x => (x.Id, x.Key)).Select(x => new DataTypeCacheRefresher.JsonPayload(x.Id, x.Key, true)));

    #endregion

    #region ValueEditorCacheRefresher

    public static void RefreshValueEditorCache(this DistributedCache dc, IEnumerable<IDataType> dataTypes)
        => dc.RefreshByPayload(ValueEditorCacheRefresher.UniqueId, dataTypes.DistinctBy(x => (x.Id, x.Key)).Select(x => new DataTypeCacheRefresher.JsonPayload(x.Id, x.Key, false)));

    #endregion

    #region ContentCacheRefresher

    public static void RefreshAllContentCache(this DistributedCache dc)
    {
        ContentCacheRefresher.JsonPayload[] payloads = new[]
        {
            new ContentCacheRefresher.JsonPayload()
            {
                ChangeTypes = TreeChangeTypes.RefreshAll
            }
        };

        // note: refresh all content cache does refresh content types too
        dc.RefreshByPayload(ContentCacheRefresher.UniqueId, payloads);
    }

    [Obsolete("Use the overload accepting IEnumerable instead to avoid allocating arrays. This overload will be removed in Umbraco 13.")]
    public static void RefreshContentCache(this DistributedCache dc, TreeChange<IContent>[] changes)
        => dc.RefreshContentCache(changes.AsEnumerable());

    public static void RefreshContentCache(this DistributedCache dc, IEnumerable<TreeChange<IContent>> changes)
    {
        IEnumerable<ContentCacheRefresher.JsonPayload> payloads = changes.Select(x => new ContentCacheRefresher.JsonPayload()
        {
            Id = x.Item.Id,
            Key = x.Item.Key,
            ChangeTypes = x.ChangeTypes,
            Blueprint = x.Item.Blueprint,
            PublishedCultures = x.PublishedCultures?.ToArray(),
            UnpublishedCultures = x.UnpublishedCultures?.ToArray()
        });

        dc.RefreshByPayload(ContentCacheRefresher.UniqueId, payloads);
    }

    #endregion

    #region MemberCacheRefresher

    [Obsolete("Use the overload accepting IEnumerable instead to avoid allocating arrays. This overload will be removed in Umbraco 13.")]
    public static void RefreshMemberCache(this DistributedCache dc, params IMember[] members)
        => dc.RefreshMemberCache(members.AsEnumerable());

    public static void RefreshMemberCache(this DistributedCache dc, IEnumerable<IMember> members)
        => dc.RefreshByPayload(MemberCacheRefresher.UniqueId, members.DistinctBy(x => (x.Id, x.Username)).Select(x => new MemberCacheRefresher.JsonPayload(x.Id, x.Username, false)));


    [Obsolete("Use the overload accepting IEnumerable instead to avoid allocating arrays. This overload will be removed in Umbraco 13.")]
    public static void RemoveMemberCache(this DistributedCache dc, params IMember[] members)
        => dc.RemoveMemberCache(members.AsEnumerable());

    public static void RemoveMemberCache(this DistributedCache dc, IEnumerable<IMember> members)
        => dc.RefreshByPayload(MemberCacheRefresher.UniqueId, members.DistinctBy(x => (x.Id, x.Username)).Select(x => new MemberCacheRefresher.JsonPayload(x.Id, x.Username, true)));

    #endregion

    #region MemberGroupCacheRefresher

    public static void RefreshMemberGroupCache(this DistributedCache dc, int memberGroupId)
        => dc.Refresh(MemberGroupCacheRefresher.UniqueId, memberGroupId);

    public static void RefreshMemberGroupCache(this DistributedCache dc, IEnumerable<IMemberGroup> memberGroups)
        => dc.Refresh(MemberGroupCacheRefresher.UniqueId, memberGroups.Select(x => x.Id).Distinct().ToArray());

    public static void RemoveMemberGroupCache(this DistributedCache dc, int memberGroupId)
        => dc.Remove(MemberGroupCacheRefresher.UniqueId, memberGroupId);

    public static void RemoveMemberGroupCache(this DistributedCache dc, IEnumerable<IMemberGroup> memberGroups)
        => dc.Remove(MemberGroupCacheRefresher.UniqueId, memberGroups.Select(x => x.Id).Distinct().ToArray());

    #endregion

    #region MediaCacheRefresher

    public static void RefreshAllMediaCache(this DistributedCache dc)
        // note: refresh all media cache does refresh content types too
        => dc.RefreshByPayload(MediaCacheRefresher.UniqueId, new MediaCacheRefresher.JsonPayload(0, null, TreeChangeTypes.RefreshAll).Yield());

    [Obsolete("Use the overload accepting IEnumerable instead to avoid allocating arrays. This overload will be removed in Umbraco 13.")]
    public static void RefreshMediaCache(this DistributedCache dc, TreeChange<IMedia>[] changes)
        => dc.RefreshMediaCache(changes.AsEnumerable());

    public static void RefreshMediaCache(this DistributedCache dc, IEnumerable<TreeChange<IMedia>> changes)
        => dc.RefreshByPayload(MediaCacheRefresher.UniqueId, changes.DistinctBy(x => (x.Item.Id, x.Item.Key, x.ChangeTypes)).Select(x => new MediaCacheRefresher.JsonPayload(x.Item.Id, x.Item.Key, x.ChangeTypes)));

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

    #region MacroCacheRefresher

    [Obsolete("Use the overload accepting IEnumerable instead. This overload will be removed in Umbraco 13.")]
    public static void RefreshMacroCache(this DistributedCache dc, IMacro macro)
        => dc.RefreshMacroCache(macro.Yield());

    public static void RefreshMacroCache(this DistributedCache dc, IEnumerable<IMacro> macros)
        => dc.RefreshByPayload(MacroCacheRefresher.UniqueId, macros.DistinctBy(x => (x.Id, x.Alias)).Select(x => new MacroCacheRefresher.JsonPayload(x.Id, x.Alias)));

    [Obsolete("Use the overload accepting IEnumerable instead. This overload will be removed in Umbraco 13.")]
    public static void RemoveMacroCache(this DistributedCache dc, IMacro macro)
        => dc.RemoveMacroCache(macro.Yield());

    public static void RemoveMacroCache(this DistributedCache dc, IEnumerable<IMacro> macros)
        => dc.RefreshByPayload(MacroCacheRefresher.UniqueId, macros.DistinctBy(x => (x.Id, x.Alias)).Select(x => new MacroCacheRefresher.JsonPayload(x.Id, x.Alias)));

    #endregion

    #region ContentTypeCacheRefresher

    [Obsolete("Use the overload accepting IEnumerable instead to avoid allocating arrays. This overload will be removed in Umbraco 13.")]
    public static void RefreshContentTypeCache(this DistributedCache dc, ContentTypeChange<IContentType>[] changes)
        => dc.RefreshContentTypeCache(changes.AsEnumerable());

    public static void RefreshContentTypeCache(this DistributedCache dc, IEnumerable<ContentTypeChange<IContentType>> changes)
        => dc.RefreshByPayload(ContentTypeCacheRefresher.UniqueId, changes.DistinctBy(x => (x.Item.Id, x.ChangeTypes)).Select(x => new ContentTypeCacheRefresher.JsonPayload(typeof(IContentType).Name, x.Item.Id, x.ChangeTypes)));

    [Obsolete("Use the overload accepting IEnumerable instead to avoid allocating arrays. This overload will be removed in Umbraco 13.")]
    public static void RefreshContentTypeCache(this DistributedCache dc, ContentTypeChange<IMediaType>[] changes)
        => dc.RefreshContentTypeCache(changes.AsEnumerable());

    public static void RefreshContentTypeCache(this DistributedCache dc, IEnumerable<ContentTypeChange<IMediaType>> changes)
        => dc.RefreshByPayload(ContentTypeCacheRefresher.UniqueId, changes.DistinctBy(x => (x.Item.Id, x.ChangeTypes)).Select(x => new ContentTypeCacheRefresher.JsonPayload(typeof(IMediaType).Name, x.Item.Id, x.ChangeTypes)));

    [Obsolete("Use the overload accepting IEnumerable instead to avoid allocating arrays. This overload will be removed in Umbraco 13.")]
    public static void RefreshContentTypeCache(this DistributedCache dc, ContentTypeChange<IMemberType>[] changes)
        => dc.RefreshContentTypeCache(changes.AsEnumerable());

    public static void RefreshContentTypeCache(this DistributedCache dc, IEnumerable<ContentTypeChange<IMemberType>> changes)
        => dc.RefreshByPayload(ContentTypeCacheRefresher.UniqueId, changes.DistinctBy(x => (x.Item.Id, x.ChangeTypes)).Select(x => new ContentTypeCacheRefresher.JsonPayload(typeof(IMemberType).Name, x.Item.Id, x.ChangeTypes)));

    #endregion

    #region DomainCacheRefresher

    [Obsolete("Use the overload accepting IEnumerable instead. This overload will be removed in Umbraco 13.")]
    public static void RefreshDomainCache(this DistributedCache dc, IDomain domain)
        => dc.RefreshDomainCache(domain.Yield());

    public static void RefreshDomainCache(this DistributedCache dc, IEnumerable<IDomain> domains)
        => dc.RefreshByPayload(DomainCacheRefresher.UniqueId, domains.DistinctBy(x => x.Id).Select(x => new DomainCacheRefresher.JsonPayload(x.Id, DomainChangeTypes.Refresh)));

    [Obsolete("Use the overload accepting IEnumerable instead. This overload will be removed in Umbraco 13.")]
    public static void RemoveDomainCache(this DistributedCache dc, IDomain domain)
        => dc.RemoveDomainCache(domain.Yield());

    public static void RemoveDomainCache(this DistributedCache dc, IEnumerable<IDomain> domains)
        => dc.RefreshByPayload(DomainCacheRefresher.UniqueId, domains.DistinctBy(x => x.Id).Select(x => new DomainCacheRefresher.JsonPayload(x.Id, DomainChangeTypes.Remove)));

    public static void RefreshAllDomainCache(this DistributedCache dc)
        => dc.RefreshByPayload(DomainCacheRefresher.UniqueId, new DomainCacheRefresher.JsonPayload(0, DomainChangeTypes.RefreshAll).Yield());

    #endregion

    #region LanguageCacheRefresher

    [Obsolete("Use the overload accepting IEnumerable instead. This overload will be removed in Umbraco 13.")]
    public static void RefreshLanguageCache(this DistributedCache dc, ILanguage language)
        => dc.RefreshLanguageCache(language.Yield());

    public static void RefreshLanguageCache(this DistributedCache dc, IEnumerable<ILanguage> languages)
        => dc.RefreshByPayload(LanguageCacheRefresher.UniqueId, languages.DistinctBy(x => (x.Id, x.IsoCode)).Select(x => new LanguageCacheRefresher.JsonPayload(
            x.Id,
            x.IsoCode,
            x.WasPropertyDirty(nameof(ILanguage.IsoCode))
            ? LanguageCacheRefresher.JsonPayload.LanguageChangeType.ChangeCulture
            : LanguageCacheRefresher.JsonPayload.LanguageChangeType.Update)));

    [Obsolete("Use the overload accepting IEnumerable instead. This overload will be removed in Umbraco 13.")]
    public static void RemoveLanguageCache(this DistributedCache dc, ILanguage language)
        => dc.RemoveLanguageCache(language.Yield());

    public static void RemoveLanguageCache(this DistributedCache dc, IEnumerable<ILanguage> languages)
        => dc.RefreshByPayload(LanguageCacheRefresher.UniqueId, languages.DistinctBy(x => (x.Id, x.IsoCode)).Select(x => new LanguageCacheRefresher.JsonPayload(x.Id, x.IsoCode, LanguageCacheRefresher.JsonPayload.LanguageChangeType.Remove)));

    #endregion

    #region RelationTypeCacheRefresher

    public static void RefreshRelationTypeCache(this DistributedCache dc, int id)
        => dc.Refresh(RelationTypeCacheRefresher.UniqueId, id);

    public static void RefreshRelationTypeCache(this DistributedCache dc, IEnumerable<IRelationType> relationTypes)
        => dc.Refresh(RelationTypeCacheRefresher.UniqueId, relationTypes.Select(x => x.Id).Distinct().ToArray());

    public static void RemoveRelationTypeCache(this DistributedCache dc, int id)
        => dc.Remove(RelationTypeCacheRefresher.UniqueId, id);

    public static void RemoveRelationTypeCache(this DistributedCache dc, IEnumerable<IRelationType> relationTypes)
        => dc.Remove(RelationTypeCacheRefresher.UniqueId, relationTypes.Select(x => x.Id).Distinct().ToArray());

    #endregion
}
