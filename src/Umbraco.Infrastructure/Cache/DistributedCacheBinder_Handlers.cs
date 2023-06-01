// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache;

[Obsolete("These handlers are rewritten into separate IDistributedCacheHandler implmentations. This class will be removed in Umbraco 13.")]
public class DistributedCacheBinder :
    INotificationHandler<DictionaryItemDeletedNotification>,
    INotificationHandler<DictionaryItemSavedNotification>,
    INotificationHandler<LanguageSavedNotification>,
    INotificationHandler<LanguageDeletedNotification>,
    INotificationHandler<MemberSavedNotification>,
    INotificationHandler<MemberDeletedNotification>,
    INotificationHandler<PublicAccessEntrySavedNotification>,
    INotificationHandler<PublicAccessEntryDeletedNotification>,
    INotificationHandler<UserSavedNotification>,
    INotificationHandler<UserDeletedNotification>,
    INotificationHandler<UserGroupWithUsersSavedNotification>,
    INotificationHandler<UserGroupDeletedNotification>,
    INotificationHandler<MemberGroupDeletedNotification>,
    INotificationHandler<MemberGroupSavedNotification>,
    INotificationHandler<TemplateDeletedNotification>,
    INotificationHandler<TemplateSavedNotification>,
    INotificationHandler<DataTypeDeletedNotification>,
    INotificationHandler<DataTypeSavedNotification>,
    INotificationHandler<RelationTypeDeletedNotification>,
    INotificationHandler<RelationTypeSavedNotification>,
    INotificationHandler<DomainDeletedNotification>,
    INotificationHandler<DomainSavedNotification>,
    INotificationHandler<MacroSavedNotification>,
    INotificationHandler<MacroDeletedNotification>,
    INotificationHandler<MediaTreeChangeNotification>,
    INotificationHandler<ContentTypeChangedNotification>,
    INotificationHandler<MediaTypeChangedNotification>,
    INotificationHandler<MemberTypeChangedNotification>,
    INotificationHandler<ContentTreeChangeNotification>
{
    private readonly DistributedCache _distributedCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="DistributedCacheBinder" /> class.
    /// </summary>
    /// <param name="distributedCache">The distributed cache.</param>
    public DistributedCacheBinder(DistributedCache distributedCache)
        => _distributedCache = distributedCache;

    #region PublicAccessService

    public void Handle(PublicAccessEntrySavedNotification notification)
        => _distributedCache.RefreshPublicAccess();

    public void Handle(PublicAccessEntryDeletedNotification notification)
        => _distributedCache.RefreshPublicAccess();

    #endregion

    #region ContentService

    public void Handle(ContentTreeChangeNotification notification)
        => _distributedCache.RefreshContentCache(notification.Changes);

    #endregion

    #region LocalizationService

    public void Handle(DictionaryItemSavedNotification notification)
    {
        foreach (IDictionaryItem entity in notification.SavedEntities)
        {
            _distributedCache.RefreshDictionaryCache(entity.Id);
        }
    }

    public void Handle(DictionaryItemDeletedNotification notification)
    {
        foreach (IDictionaryItem entity in notification.DeletedEntities)
        {
            _distributedCache.RemoveDictionaryCache(entity.Id);
        }
    }

    #endregion

    #region DataTypeService

    public void Handle(DataTypeSavedNotification notification)
    {
        _distributedCache.RefreshDataTypeCache(notification.SavedEntities);
        _distributedCache.RefreshValueEditorCache(notification.SavedEntities);
    }

    public void Handle(DataTypeDeletedNotification notification)
    {
        _distributedCache.RemoveDataTypeCache(notification.DeletedEntities);
        _distributedCache.RefreshValueEditorCache(notification.DeletedEntities);
    }

    #endregion

    #region DomainService

    public void Handle(DomainSavedNotification notification)
        => _distributedCache.RefreshDomainCache(notification.SavedEntities);

    public void Handle(DomainDeletedNotification notification)
        => _distributedCache.RemoveDomainCache(notification.DeletedEntities);

    #endregion

    #region LocalizationService / Language

    public void Handle(LanguageDeletedNotification notification)
        => _distributedCache.RemoveLanguageCache(notification.DeletedEntities);

    public void Handle(LanguageSavedNotification notification)
        => _distributedCache.RefreshLanguageCache(notification.SavedEntities);

    #endregion

    #region ContentTypeService, MediaTypeService, MemberTypeService

    public void Handle(ContentTypeChangedNotification notification)
        => _distributedCache.RefreshContentTypeCache(notification.Changes);

    public void Handle(MediaTypeChangedNotification notification)
        => _distributedCache.RefreshContentTypeCache(notification.Changes);

    public void Handle(MemberTypeChangedNotification notification)
        => _distributedCache.RefreshContentTypeCache(notification.Changes);

    #endregion

    #region UserService

    public void Handle(UserSavedNotification notification)
    {
        foreach (IUser entity in notification.SavedEntities)
        {
            _distributedCache.RefreshUserCache(entity.Id);
        }
    }

    public void Handle(UserDeletedNotification notification)
    {
        foreach (IUser entity in notification.DeletedEntities)
        {
            _distributedCache.RemoveUserCache(entity.Id);
        }
    }

    public void Handle(UserGroupWithUsersSavedNotification notification)
    {
        foreach (UserGroupWithUsers entity in notification.SavedEntities)
        {
            _distributedCache.RefreshUserGroupCache(entity.UserGroup.Id);
        }
    }

    public void Handle(UserGroupDeletedNotification notification)
    {
        foreach (IUserGroup entity in notification.DeletedEntities)
        {
            _distributedCache.RemoveUserGroupCache(entity.Id);
        }
    }

    #endregion

    #region FileService

    public void Handle(TemplateDeletedNotification notification)
    {
        foreach (ITemplate entity in notification.DeletedEntities)
        {
            _distributedCache.RemoveTemplateCache(entity.Id);
        }
    }

    public void Handle(TemplateSavedNotification notification)
    {
        foreach (ITemplate entity in notification.SavedEntities)
        {
            _distributedCache.RefreshTemplateCache(entity.Id);
        }
    }

    #endregion

    #region MacroService

    public void Handle(MacroDeletedNotification notification)
        => _distributedCache.RemoveMacroCache(notification.DeletedEntities);

    public void Handle(MacroSavedNotification notification)
        => _distributedCache.RefreshMacroCache(notification.SavedEntities);

    #endregion

    #region MediaService

    public void Handle(MediaTreeChangeNotification notification)
        => _distributedCache.RefreshMediaCache(notification.Changes);

    #endregion

    #region MemberService

    public void Handle(MemberDeletedNotification notification)
        => _distributedCache.RemoveMemberCache(notification.DeletedEntities);

    public void Handle(MemberSavedNotification notification)
        => _distributedCache.RefreshMemberCache(notification.SavedEntities);

    #endregion

    #region MemberGroupService

    public void Handle(MemberGroupDeletedNotification notification)
    {
        foreach (IMemberGroup entity in notification.DeletedEntities)
        {
            _distributedCache.RemoveMemberGroupCache(entity.Id);
        }
    }

    public void Handle(MemberGroupSavedNotification notification)
    {
        foreach (IMemberGroup entity in notification.SavedEntities)
        {
            _distributedCache.RemoveMemberGroupCache(entity.Id);
        }
    }

    #endregion

    #region RelationTypeService

    public void Handle(RelationTypeSavedNotification notification)
    {
        foreach (IRelationType entity in notification.SavedEntities)
        {
            _distributedCache.RefreshRelationTypeCache(entity.Id);
        }
    }

    public void Handle(RelationTypeDeletedNotification notification)
    {
        foreach (IRelationType entity in notification.DeletedEntities)
        {
            _distributedCache.RemoveRelationTypeCache(entity.Id);
        }
    }

    #endregion
}
