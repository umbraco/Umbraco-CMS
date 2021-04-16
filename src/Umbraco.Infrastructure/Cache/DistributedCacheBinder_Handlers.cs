// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Services.Implement;
using Umbraco.Cms.Core.Services.Notifications;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache
{
    /// <summary>
    /// Default <see cref="IDistributedCacheBinder"/> implementation.
    /// </summary>
    public partial class DistributedCacheBinder :
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
        INotificationHandler<MemberTypeChangedNotification>
    {
        private List<Action> _unbinders;

        private void Bind(Action binder, Action unbinder)
        {
            // bind now
            binder();

            // and register unbinder for later, if needed
            _unbinders?.Add(unbinder);
        }

        /// <inheritdoc />
        public void UnbindEvents()
        {
            if (_unbinders == null)
                throw new NotSupportedException();
            foreach (var unbinder in _unbinders)
                unbinder();
            _unbinders = null;
        }

        /// <inheritdoc />
        public void BindEvents(bool supportUnbinding = false)
        {
            if (supportUnbinding)
                _unbinders = new List<Action>();

            _logger.LogInformation("Initializing Umbraco internal event handlers for cache refreshing.");

            // bind to content events
            Bind(() => ContentService.TreeChanged += ContentService_TreeChanged,// handles all content changes
                () => ContentService.TreeChanged -= ContentService_TreeChanged);

            // TreeChanged should also deal with this
            //Bind(() => ContentService.SavedBlueprint += ContentService_SavedBlueprint,
            //    () => ContentService.SavedBlueprint -= ContentService_SavedBlueprint);
            //Bind(() => ContentService.DeletedBlueprint += ContentService_DeletedBlueprint,
            //    () => ContentService.DeletedBlueprint -= ContentService_DeletedBlueprint);
        }

        #region PublicAccessService

        public void Handle(PublicAccessEntrySavedNotification notification)
        {
            _distributedCache.RefreshPublicAccess();
        }

        public void Handle(PublicAccessEntryDeletedNotification notification)
        {
            _distributedCache.RefreshPublicAccess();

        }

        #endregion

        #region ContentService

        private void ContentService_TreeChanged(IContentService sender, TreeChange<IContent>.EventArgs args)
        {
            _distributedCache.RefreshContentCache(args.Changes.ToArray());
        }

        //private void ContentService_SavedBlueprint(IContentService sender, SaveEventArgs<IContent> e)
        //{
        //    _distributedCache.RefreshUnpublishedPageCache(e.SavedEntities.ToArray());
        //}

        //private void ContentService_DeletedBlueprint(IContentService sender, DeleteEventArgs<IContent> e)
        //{
        //    _distributedCache.RemoveUnpublishedPageCache(e.DeletedEntities.ToArray());
        //}

        #endregion

        #region LocalizationService / Dictionary
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
            foreach (IDataType entity in notification.SavedEntities)
            {
                _distributedCache.RefreshDataTypeCache(entity);
            }
        }

        public void Handle(DataTypeDeletedNotification notification)
        {
            foreach (IDataType entity in notification.DeletedEntities)
            {
                _distributedCache.RemoveDataTypeCache(entity);
            }
        }

        #endregion

        #region DomainService

        public void Handle(DomainSavedNotification notification)
        {
            foreach (IDomain entity in notification.SavedEntities)
            {
                _distributedCache.RefreshDomainCache(entity);
            }
        }

        public void Handle(DomainDeletedNotification notification)
        {
            foreach (IDomain entity in notification.DeletedEntities)
            {
                _distributedCache.RemoveDomainCache(entity);
            }
        }

        #endregion

        #region LocalizationService / Language

        /// <summary>
        /// Fires when a language is deleted
        /// </summary>
        /// <param name="notification"></param>
        public void Handle(LanguageDeletedNotification notification)
        {
            foreach (ILanguage entity in notification.DeletedEntities)
            {
                _distributedCache.RemoveLanguageCache(entity);
            }
        }

        /// <summary>
        /// Fires when a language is saved
        /// </summary>
        /// <param name="notification"></param>
        public void Handle(LanguageSavedNotification notification)
        {
            foreach (ILanguage entity in notification.SavedEntities)
            {
                _distributedCache.RefreshLanguageCache(entity);
            }
        }

        #endregion

        #region Content|Media|MemberTypeService

        public void Handle(ContentTypeChangedNotification notification) =>
            _distributedCache.RefreshContentTypeCache(notification.Changes.ToArray());

        public void Handle(MediaTypeChangedNotification notification) =>
            _distributedCache.RefreshContentTypeCache(notification.Changes.ToArray());

        public void Handle(MemberTypeChangedNotification notification) =>
            _distributedCache.RefreshContentTypeCache(notification.Changes.ToArray());

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

        /// <summary>
        /// Removes cache for template
        /// </summary>
        /// <param name="notification"></param>
        public void Handle(TemplateDeletedNotification notification)
        {
            foreach (ITemplate entity in notification.DeletedEntities)
            {
                _distributedCache.RemoveTemplateCache(entity.Id);
            }
        }

        /// <summary>
        /// Refresh cache for template
        /// </summary>
        /// <param name="notification"></param>
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
        {
            foreach (IMacro entity in notification.DeletedEntities)
            {
                _distributedCache.RemoveMacroCache(entity);
            }
        }

        public void Handle(MacroSavedNotification notification)
        {
            foreach (IMacro entity in notification.SavedEntities)
            {
                _distributedCache.RefreshMacroCache(entity);
            }
        }

        #endregion

        #region MediaService

        public void Handle(MediaTreeChangeNotification notification)
        {
            _distributedCache.RefreshMediaCache(notification.Changes.ToArray());
        }

        #endregion

        #region MemberService

        public void Handle(MemberDeletedNotification notification)
        {
            _distributedCache.RemoveMemberCache(notification.DeletedEntities.ToArray());
        }

        public void Handle(MemberSavedNotification notification)
        {
            _distributedCache.RefreshMemberCache(notification.SavedEntities.ToArray());
        }

        #endregion

        #region MemberGroupService

        /// <summary>
        /// Fires when a member group is deleted
        /// </summary>
        /// <param name="notification"></param>
        public void Handle(MemberGroupDeletedNotification notification)
        {
            foreach (IMemberGroup entity in notification.DeletedEntities)
            {
                _distributedCache.RemoveMemberGroupCache(entity.Id);
            }
        }

        /// <summary>
        /// Fires when a member group is saved
        /// </summary>
        /// <param name="notification"></param>
        public void Handle(MemberGroupSavedNotification notification)
        {
            foreach (IMemberGroup entity in notification.SavedEntities)
            {
                _distributedCache.RemoveMemberGroupCache(entity.Id);
            }
        }

        #endregion

        #region RelationType

        public void Handle(RelationTypeSavedNotification notification)
        {
            DistributedCache dc = _distributedCache;
            foreach (IRelationType entity in notification.SavedEntities)
            {
                dc.RefreshRelationTypeCache(entity.Id);
            }
        }

        public void Handle(RelationTypeDeletedNotification notification)
        {
            DistributedCache dc = _distributedCache;
            foreach (IRelationType entity in notification.DeletedEntities)
            {
                dc.RemoveRelationTypeCache(entity.Id);
            }
        }

        #endregion
    }
}
