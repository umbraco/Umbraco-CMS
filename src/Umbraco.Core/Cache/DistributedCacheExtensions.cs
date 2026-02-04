// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Extensions;

/// <summary>
/// Extension methods for <see cref="DistributedCache" />.
/// </summary>
public static class DistributedCacheExtensions
{
    #region PublicAccessCacheRefresher

    /// <summary>
    ///     Refreshes all public access entries in the distributed cache.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    public static void RefreshPublicAccess(this DistributedCache dc)
        => dc.RefreshAll(PublicAccessCacheRefresher.UniqueId);

    #endregion

    #region UserCacheRefresher

    /// <summary>
    ///     Removes the specified users from the distributed cache.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    /// <param name="users">The users to remove from cache.</param>
    public static void RemoveUserCache(this DistributedCache dc, IEnumerable<IUser> users)
    {
        IEnumerable<UserCacheRefresher.JsonPayload> payloads = users.Select(x => new UserCacheRefresher.JsonPayload()
        {
            Id = x.Id,
            Key = x.Key,
        });

        dc.RefreshByPayload(UserCacheRefresher.UniqueId, payloads);
    }

    /// <summary>
    ///     Refreshes the specified users in the distributed cache.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    /// <param name="users">The users to refresh in cache.</param>
    public static void RefreshUserCache(this DistributedCache dc, IEnumerable<IUser> users)
    {
        IEnumerable<UserCacheRefresher.JsonPayload> payloads = users.Select(x => new UserCacheRefresher.JsonPayload()
        {
            Id = x.Id,
            Key = x.Key,
        });

        dc.RefreshByPayload(UserCacheRefresher.UniqueId, payloads);
    }

    /// <summary>
    ///     Refreshes all users in the distributed cache.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    public static void RefreshAllUserCache(this DistributedCache dc)
        => dc.RefreshAll(UserCacheRefresher.UniqueId);

    #endregion

    #region UserGroupCacheRefresher

    /// <summary>
    ///     Removes a user group from the distributed cache by user identifier.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    /// <param name="userId">The user identifier.</param>
    public static void RemoveUserGroupCache(this DistributedCache dc, int userId)
        => dc.Remove(UserGroupCacheRefresher.UniqueId, userId);

    /// <summary>
    ///     Removes the specified user groups from the distributed cache.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    /// <param name="userGroups">The user groups to remove from cache.</param>
    public static void RemoveUserGroupCache(this DistributedCache dc, IEnumerable<IUserGroup> userGroups)
        => dc.Remove(UserGroupCacheRefresher.UniqueId, userGroups.Select(x => x.Id).Distinct().ToArray());

    /// <summary>
    ///     Refreshes a user group in the distributed cache by user identifier.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    /// <param name="userId">The user identifier.</param>
    public static void RefreshUserGroupCache(this DistributedCache dc, int userId)
        => dc.Refresh(UserGroupCacheRefresher.UniqueId, userId);

    /// <summary>
    ///     Refreshes the specified user groups in the distributed cache.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    /// <param name="userGroups">The user groups to refresh in cache.</param>
    public static void RefreshUserGroupCache(this DistributedCache dc, IEnumerable<IUserGroup> userGroups)
        => dc.Refresh(UserGroupCacheRefresher.UniqueId, userGroups.Select(x => x.Id).Distinct().ToArray());

    /// <summary>
    ///     Refreshes all user groups in the distributed cache.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    public static void RefreshAllUserGroupCache(this DistributedCache dc)
        => dc.RefreshAll(UserGroupCacheRefresher.UniqueId);

    #endregion

    #region TemplateCacheRefresher

    /// <summary>
    ///     Refreshes a template in the distributed cache.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    /// <param name="templateId">The template identifier.</param>
    public static void RefreshTemplateCache(this DistributedCache dc, int templateId)
        => dc.Refresh(TemplateCacheRefresher.UniqueId, templateId);

    /// <summary>
    ///     Refreshes the specified templates in the distributed cache.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    /// <param name="templates">The templates to refresh in cache.</param>
    public static void RefreshTemplateCache(this DistributedCache dc, IEnumerable<ITemplate> templates)
        => dc.Refresh(TemplateCacheRefresher.UniqueId, templates.Select(x => x.Id).Distinct().ToArray());

    /// <summary>
    ///     Removes a template from the distributed cache.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    /// <param name="templateId">The template identifier.</param>
    public static void RemoveTemplateCache(this DistributedCache dc, int templateId)
        => dc.Remove(TemplateCacheRefresher.UniqueId, templateId);

    /// <summary>
    ///     Removes the specified templates from the distributed cache.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    /// <param name="templates">The templates to remove from cache.</param>
    public static void RemoveTemplateCache(this DistributedCache dc, IEnumerable<ITemplate> templates)
        => dc.Remove(TemplateCacheRefresher.UniqueId, templates.Select(x => x.Id).Distinct().ToArray());

    #endregion

    #region DictionaryCacheRefresher

    /// <summary>
    ///     Refreshes a dictionary item in the distributed cache.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    /// <param name="dictionaryItemId">The dictionary item identifier.</param>
    public static void RefreshDictionaryCache(this DistributedCache dc, int dictionaryItemId)
        => dc.Refresh(DictionaryCacheRefresher.UniqueId, dictionaryItemId);

    /// <summary>
    ///     Refreshes the specified dictionary items in the distributed cache.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    /// <param name="dictionaryItems">The dictionary items to refresh in cache.</param>
    public static void RefreshDictionaryCache(this DistributedCache dc, IEnumerable<IDictionaryItem> dictionaryItems)
        => dc.Refresh(DictionaryCacheRefresher.UniqueId, dictionaryItems.Select(x => x.Id).Distinct().ToArray());

    /// <summary>
    ///     Removes a dictionary item from the distributed cache.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    /// <param name="dictionaryItemId">The dictionary item identifier.</param>
    public static void RemoveDictionaryCache(this DistributedCache dc, int dictionaryItemId)
        => dc.Remove(DictionaryCacheRefresher.UniqueId, dictionaryItemId);

    /// <summary>
    ///     Removes the specified dictionary items from the distributed cache.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    /// <param name="dictionaryItems">The dictionary items to remove from cache.</param>
    public static void RemoveDictionaryCache(this DistributedCache dc, IEnumerable<IDictionaryItem> dictionaryItems)
        => dc.Remove(DictionaryCacheRefresher.UniqueId, dictionaryItems.Select(x => x.Id).Distinct().ToArray());

    #endregion

    #region DataTypeCacheRefresher

    /// <summary>
    ///     Refreshes the specified data types in the distributed cache.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    /// <param name="dataTypes">The data types to refresh in cache.</param>
    public static void RefreshDataTypeCache(this DistributedCache dc, IEnumerable<IDataType> dataTypes)
        => dc.RefreshByPayload(DataTypeCacheRefresher.UniqueId, dataTypes.DistinctBy(x => (x.Id, x.Key)).Select(x => new DataTypeCacheRefresher.JsonPayload(x.Id, x.Key, false)));

    /// <summary>
    ///     Removes the specified data types from the distributed cache.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    /// <param name="dataTypes">The data types to remove from cache.</param>
    public static void RemoveDataTypeCache(this DistributedCache dc, IEnumerable<IDataType> dataTypes)
        => dc.RefreshByPayload(DataTypeCacheRefresher.UniqueId, dataTypes.DistinctBy(x => (x.Id, x.Key)).Select(x => new DataTypeCacheRefresher.JsonPayload(x.Id, x.Key, true)));

    #endregion

    #region ValueEditorCacheRefresher

    /// <summary>
    ///     Refreshes the value editor cache for the specified data types.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    /// <param name="dataTypes">The data types whose value editors should be refreshed.</param>
    public static void RefreshValueEditorCache(this DistributedCache dc, IEnumerable<IDataType> dataTypes)
        => dc.RefreshByPayload(ValueEditorCacheRefresher.UniqueId, dataTypes.DistinctBy(x => (x.Id, x.Key)).Select(x => new DataTypeCacheRefresher.JsonPayload(x.Id, x.Key, false)));

    #endregion

    #region ContentCacheRefresher

    /// <summary>
    ///     Refreshes all content in the distributed cache.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
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

    /// <summary>
    ///     Refreshes the content cache for the specified content changes.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    /// <param name="changes">The content changes to refresh.</param>
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

    /// <summary>
    ///     Refreshes the specified members in the distributed cache.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    /// <param name="members">The members to refresh in cache.</param>
    [Obsolete("Please use the overload taking all parameters. Scheduled for removal in Umbraco 18.")]
    public static void RefreshMemberCache(this DistributedCache dc, IEnumerable<IMember> members)
        => dc.RefreshMemberCache(members, new Dictionary<string, object?>());

    /// <summary>
    ///     Refreshes the specified members in the distributed cache.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    /// <param name="members">The members to refresh in cache.</param>
    /// <param name="state">The notification state.</param>
    public static void RefreshMemberCache(this DistributedCache dc, IEnumerable<IMember> members, IDictionary<string, object?> state)
        => dc.RefreshByPayload(
            MemberCacheRefresher.UniqueId,
            GetPayloads(members, state, false));

    /// <summary>
    ///     Removes the specified members from the distributed cache.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    /// <param name="members">The members to remove from cache.</param>
    [Obsolete("Please use the overload taking all parameters. Scheduled for removal in Umbraco 18.")]
    public static void RemoveMemberCache(this DistributedCache dc, IEnumerable<IMember> members)
        => dc.RemoveMemberCache(members, new Dictionary<string, object?>());

    /// <summary>
    ///     Removes the specified members from the distributed cache.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    /// <param name="members">The members to remove from cache.</param>
    /// <param name="state">The notification state.</param>
    public static void RemoveMemberCache(this DistributedCache dc, IEnumerable<IMember> members, IDictionary<string, object?> state)
        => dc.RefreshByPayload(
            MemberCacheRefresher.UniqueId,
            GetPayloads(members, state, true));

    /// <summary>
    ///     Gets the JSON payloads for member cache refresh operations.
    /// </summary>
    /// <param name="members">The members to create payloads for.</param>
    /// <param name="state">The notification state dictionary.</param>
    /// <param name="removed">Whether the members were removed.</param>
    /// <returns>An enumerable of JSON payloads for the member cache refresher.</returns>
    /// <remarks>Internal for unit test.</remarks>
    internal static IEnumerable<MemberCacheRefresher.JsonPayload> GetPayloads(IEnumerable<IMember> members, IDictionary<string, object?> state, bool removed)
        => members
            .DistinctBy(x => (x.Id, x.Username))
            .Select(x => new MemberCacheRefresher.JsonPayload(x.Id, x.Username, removed)
            {
                PreviousUsername = GetPreviousUsername(x, state)
            });

    private static string? GetPreviousUsername(IMember x, IDictionary<string, object?> state)
    {
        if (state.TryGetValue(MemberSavedNotification.PreviousUsernameStateKey, out object? previousUserNames) is false)
        {
            return null;
        }

        if (previousUserNames is not IDictionary<Guid, string> previousUserNamesDictionary)
        {
            return null;
        }

        return previousUserNamesDictionary.TryGetValue(x.Key, out string? previousUsername)
            ? previousUsername
            : null;
    }

    #endregion

    #region MemberGroupCacheRefresher

    /// <summary>
    ///     Refreshes a member group in the distributed cache.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    /// <param name="memberGroupId">The member group identifier.</param>
    public static void RefreshMemberGroupCache(this DistributedCache dc, int memberGroupId)
        => dc.Refresh(MemberGroupCacheRefresher.UniqueId, memberGroupId);

    /// <summary>
    ///     Refreshes the specified member groups in the distributed cache.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    /// <param name="memberGroups">The member groups to refresh in cache.</param>
    public static void RefreshMemberGroupCache(this DistributedCache dc, IEnumerable<IMemberGroup> memberGroups)
        => dc.Refresh(MemberGroupCacheRefresher.UniqueId, memberGroups.Select(x => x.Id).Distinct().ToArray());

    /// <summary>
    ///     Removes a member group from the distributed cache.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    /// <param name="memberGroupId">The member group identifier.</param>
    public static void RemoveMemberGroupCache(this DistributedCache dc, int memberGroupId)
        => dc.Remove(MemberGroupCacheRefresher.UniqueId, memberGroupId);

    /// <summary>
    ///     Removes the specified member groups from the distributed cache.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    /// <param name="memberGroups">The member groups to remove from cache.</param>
    public static void RemoveMemberGroupCache(this DistributedCache dc, IEnumerable<IMemberGroup> memberGroups)
        => dc.Remove(MemberGroupCacheRefresher.UniqueId, memberGroups.Select(x => x.Id).Distinct().ToArray());

    #endregion

    #region MediaCacheRefresher

    /// <summary>
    ///     Refreshes all media in the distributed cache.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    public static void RefreshAllMediaCache(this DistributedCache dc)
        // note: refresh all media cache does refresh content types too
        => dc.RefreshByPayload(MediaCacheRefresher.UniqueId, new MediaCacheRefresher.JsonPayload(0, null, TreeChangeTypes.RefreshAll).Yield());

    /// <summary>
    ///     Refreshes the media cache for the specified media changes.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    /// <param name="changes">The media changes to refresh.</param>
    public static void RefreshMediaCache(this DistributedCache dc, IEnumerable<TreeChange<IMedia>> changes)
        => dc.RefreshByPayload(MediaCacheRefresher.UniqueId, changes.DistinctBy(x => (x.Item.Id, x.Item.Key, x.ChangeTypes)).Select(x => new MediaCacheRefresher.JsonPayload(x.Item.Id, x.Item.Key, x.ChangeTypes)));

    #endregion

    #region ElementCacheRefresher

    public static void RefreshAllElementCache(this DistributedCache dc)
        // note: refresh all element cache does refresh content types too
        => dc.RefreshByPayload(ElementCacheRefresher.UniqueId, new ElementCacheRefresher.JsonPayload(0, Guid.Empty, TreeChangeTypes.RefreshAll).Yield());


    public static void RefreshElementCache(this DistributedCache dc, IEnumerable<TreeChange<IElement>> changes)
        => dc.RefreshByPayload(ElementCacheRefresher.UniqueId, changes.DistinctBy(x => (x.Item.Id, x.Item.Key, x.ChangeTypes)).Select(x => new ElementCacheRefresher.JsonPayload(x.Item.Id, x.Item.Key, x.ChangeTypes)));

    #endregion

    #region Published Snapshot

    /// <summary>
    ///     Refreshes all published snapshots including content, media, and domain caches.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    public static void RefreshAllPublishedSnapshot(this DistributedCache dc)
    {
        // note: refresh all content & media caches does refresh content types too
        dc.RefreshAllContentCache();
        dc.RefreshAllMediaCache();
        dc.RefreshAllElementCache();
        dc.RefreshAllDomainCache();
    }

    #endregion

    #region ContentTypeCacheRefresher

    /// <summary>
    ///     Refreshes the content type cache for the specified content type changes.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    /// <param name="changes">The content type changes to refresh.</param>
    public static void RefreshContentTypeCache(this DistributedCache dc, IEnumerable<ContentTypeChange<IContentType>> changes)
        => dc.RefreshByPayload(ContentTypeCacheRefresher.UniqueId, changes.DistinctBy(x => (x.Item.Id, x.ChangeTypes)).Select(x => new ContentTypeCacheRefresher.JsonPayload(typeof(IContentType).Name, x.Item.Id, x.ChangeTypes)));

    /// <summary>
    ///     Refreshes the content type cache for the specified media type changes.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    /// <param name="changes">The media type changes to refresh.</param>
    public static void RefreshContentTypeCache(this DistributedCache dc, IEnumerable<ContentTypeChange<IMediaType>> changes)
        => dc.RefreshByPayload(ContentTypeCacheRefresher.UniqueId, changes.DistinctBy(x => (x.Item.Id, x.ChangeTypes)).Select(x => new ContentTypeCacheRefresher.JsonPayload(typeof(IMediaType).Name, x.Item.Id, x.ChangeTypes)));

    /// <summary>
    ///     Refreshes the content type cache for the specified member type changes.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    /// <param name="changes">The member type changes to refresh.</param>
    public static void RefreshContentTypeCache(this DistributedCache dc, IEnumerable<ContentTypeChange<IMemberType>> changes)
        => dc.RefreshByPayload(ContentTypeCacheRefresher.UniqueId, changes.DistinctBy(x => (x.Item.Id, x.ChangeTypes)).Select(x => new ContentTypeCacheRefresher.JsonPayload(typeof(IMemberType).Name, x.Item.Id, x.ChangeTypes)));

    #endregion

    #region DomainCacheRefresher

    /// <summary>
    ///     Refreshes the specified domains in the distributed cache.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    /// <param name="domains">The domains to refresh in cache.</param>
    public static void RefreshDomainCache(this DistributedCache dc, IEnumerable<IDomain> domains)
        => dc.RefreshByPayload(DomainCacheRefresher.UniqueId, domains.DistinctBy(x => x.Id).Select(x => new DomainCacheRefresher.JsonPayload(x.Id, DomainChangeTypes.Refresh)));

    /// <summary>
    ///     Removes the specified domains from the distributed cache.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    /// <param name="domains">The domains to remove from cache.</param>
    public static void RemoveDomainCache(this DistributedCache dc, IEnumerable<IDomain> domains)
        => dc.RefreshByPayload(DomainCacheRefresher.UniqueId, domains.DistinctBy(x => x.Id).Select(x => new DomainCacheRefresher.JsonPayload(x.Id, DomainChangeTypes.Remove)));

    /// <summary>
    ///     Refreshes all domains in the distributed cache.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    public static void RefreshAllDomainCache(this DistributedCache dc)
        => dc.RefreshByPayload(DomainCacheRefresher.UniqueId, new DomainCacheRefresher.JsonPayload(0, DomainChangeTypes.RefreshAll).Yield());

    #endregion

    #region LanguageCacheRefresher

    /// <summary>
    ///     Refreshes the specified languages in the distributed cache.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    /// <param name="languages">The languages to refresh in cache.</param>
    public static void RefreshLanguageCache(this DistributedCache dc, IEnumerable<ILanguage> languages)
        => dc.RefreshByPayload(LanguageCacheRefresher.UniqueId, languages.DistinctBy(x => (x.Id, x.IsoCode)).Select(x => new LanguageCacheRefresher.JsonPayload(
            x.Id,
            x.IsoCode,
            x.WasPropertyDirty(nameof(ILanguage.IsoCode))
            ? LanguageCacheRefresher.JsonPayload.LanguageChangeType.ChangeCulture
            : LanguageCacheRefresher.JsonPayload.LanguageChangeType.Update)));

    /// <summary>
    ///     Removes the specified languages from the distributed cache.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    /// <param name="languages">The languages to remove from cache.</param>
    public static void RemoveLanguageCache(this DistributedCache dc, IEnumerable<ILanguage> languages)
        => dc.RefreshByPayload(LanguageCacheRefresher.UniqueId, languages.DistinctBy(x => (x.Id, x.IsoCode)).Select(x => new LanguageCacheRefresher.JsonPayload(x.Id, x.IsoCode, LanguageCacheRefresher.JsonPayload.LanguageChangeType.Remove)));

    #endregion

    #region RelationTypeCacheRefresher

    /// <summary>
    ///     Refreshes a relation type in the distributed cache.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    /// <param name="id">The relation type identifier.</param>
    public static void RefreshRelationTypeCache(this DistributedCache dc, int id)
        => dc.Refresh(RelationTypeCacheRefresher.UniqueId, id);

    /// <summary>
    ///     Refreshes the specified relation types in the distributed cache.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    /// <param name="relationTypes">The relation types to refresh in cache.</param>
    public static void RefreshRelationTypeCache(this DistributedCache dc, IEnumerable<IRelationType> relationTypes)
        => dc.Refresh(RelationTypeCacheRefresher.UniqueId, relationTypes.Select(x => x.Id).Distinct().ToArray());

    /// <summary>
    ///     Removes a relation type from the distributed cache.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    /// <param name="id">The relation type identifier.</param>
    public static void RemoveRelationTypeCache(this DistributedCache dc, int id)
        => dc.Remove(RelationTypeCacheRefresher.UniqueId, id);

    /// <summary>
    ///     Removes the specified relation types from the distributed cache.
    /// </summary>
    /// <param name="dc">The distributed cache.</param>
    /// <param name="relationTypes">The relation types to remove from cache.</param>
    public static void RemoveRelationTypeCache(this DistributedCache dc, IEnumerable<IRelationType> relationTypes)
        => dc.Remove(RelationTypeCacheRefresher.UniqueId, relationTypes.Select(x => x.Id).Distinct().ToArray());

    #endregion
}
