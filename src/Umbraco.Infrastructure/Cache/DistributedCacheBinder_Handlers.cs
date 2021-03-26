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
using Umbraco.Cms.Infrastructure.Services.Notifications;
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
        INotificationHandler<LanguageDeletedNotification>
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

            // bind to user and user group events
            Bind(() => UserService.SavedUserGroup += UserService_SavedUserGroup,
                () => UserService.SavedUserGroup -= UserService_SavedUserGroup);
            Bind(() => UserService.DeletedUserGroup += UserService_DeletedUserGroup,
                () => UserService.DeletedUserGroup -= UserService_DeletedUserGroup);
            Bind(() => UserService.SavedUser += UserService_SavedUser,
                () => UserService.SavedUser -= UserService_SavedUser);
            Bind(() => UserService.DeletedUser += UserService_DeletedUser,
                () => UserService.DeletedUser -= UserService_DeletedUser);

            // bind to data type events
            Bind(() => DataTypeService.Deleted += DataTypeService_Deleted,
                () => DataTypeService.Deleted -= DataTypeService_Deleted);
            Bind(() => DataTypeService.Saved += DataTypeService_Saved,
                () => DataTypeService.Saved -= DataTypeService_Saved);

            // bind to stylesheet events
            Bind(() => FileService.SavedStylesheet += FileService_SavedStylesheet,
                () => FileService.SavedStylesheet -= FileService_SavedStylesheet);
            Bind(() => FileService.DeletedStylesheet += FileService_DeletedStylesheet,
                () => FileService.DeletedStylesheet -= FileService_DeletedStylesheet);

            // bind to domain events
            Bind(() => DomainService.Saved += DomainService_Saved,
                () => DomainService.Saved -= DomainService_Saved);
            Bind(() => DomainService.Deleted += DomainService_Deleted,
                () => DomainService.Deleted -= DomainService_Deleted);

            // bind to content type events
            Bind(() => ContentTypeService.Changed += ContentTypeService_Changed,
                () => ContentTypeService.Changed -= ContentTypeService_Changed);
            Bind(() => MediaTypeService.Changed += MediaTypeService_Changed,
                () => MediaTypeService.Changed -= MediaTypeService_Changed);
            Bind(() => MemberTypeService.Changed += MemberTypeService_Changed,
                () => MemberTypeService.Changed -= MemberTypeService_Changed);

            // bind to template events
            Bind(() => FileService.SavedTemplate += FileService_SavedTemplate,
                () => FileService.SavedTemplate -= FileService_SavedTemplate);
            Bind(() => FileService.DeletedTemplate += FileService_DeletedTemplate,
                () => FileService.DeletedTemplate -= FileService_DeletedTemplate);

            // bind to macro events
            Bind(() => MacroService.Saved += MacroService_Saved,
                () => MacroService.Saved -= MacroService_Saved);
            Bind(() => MacroService.Deleted += MacroService_Deleted,
                () => MacroService.Deleted -= MacroService_Deleted);

            // bind to member events
            Bind(() => MemberService.Saved += MemberService_Saved,
                () => MemberService.Saved -= MemberService_Saved);
            Bind(() => MemberService.Deleted += MemberService_Deleted,
                () => MemberService.Deleted -= MemberService_Deleted);
            Bind(() => MemberGroupService.Saved += MemberGroupService_Saved,
                () => MemberGroupService.Saved -= MemberGroupService_Saved);
            Bind(() => MemberGroupService.Deleted += MemberGroupService_Deleted,
                () => MemberGroupService.Deleted -= MemberGroupService_Deleted);

            // bind to media events - handles all media changes
            Bind(() => MediaService.TreeChanged += MediaService_TreeChanged,
                () => MediaService.TreeChanged -= MediaService_TreeChanged);

            // bind to content events
            Bind(() => ContentService.TreeChanged += ContentService_TreeChanged,// handles all content changes
                () => ContentService.TreeChanged -= ContentService_TreeChanged);

            // TreeChanged should also deal with this
            //Bind(() => ContentService.SavedBlueprint += ContentService_SavedBlueprint,
            //    () => ContentService.SavedBlueprint -= ContentService_SavedBlueprint);
            //Bind(() => ContentService.DeletedBlueprint += ContentService_DeletedBlueprint,
            //    () => ContentService.DeletedBlueprint -= ContentService_DeletedBlueprint);

            // bind to public access events
            Bind(() => PublicAccessService.Saved += PublicAccessService_Saved,
                () => PublicAccessService.Saved -= PublicAccessService_Saved);
            Bind(() => PublicAccessService.Deleted += PublicAccessService_Deleted,
                () => PublicAccessService.Deleted -= PublicAccessService_Deleted);

            // bind to relation type events
            Bind(() => RelationService.SavedRelationType += RelationService_SavedRelationType,
                () => RelationService.SavedRelationType -= RelationService_SavedRelationType);
            Bind(() => RelationService.DeletedRelationType += RelationService_DeletedRelationType,
                () => RelationService.DeletedRelationType -= RelationService_DeletedRelationType);
        }

        #region PublicAccessService

        private void PublicAccessService_Saved(IPublicAccessService sender, SaveEventArgs<PublicAccessEntry> e)
        {
            _distributedCache.RefreshPublicAccess();
        }

        private void PublicAccessService_Deleted(IPublicAccessService sender, DeleteEventArgs<PublicAccessEntry> e)
        {
            _distributedCache.RefreshPublicAccess();
        }

        #endregion

        #region ContentService

        /// <summary>
        /// Handles cache refreshing for when content is copied
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// When an entity is copied new permissions may be assigned to it based on it's parent, if that is the
        /// case then we need to clear all user permissions cache.
        /// </remarks>
        private void ContentService_Copied(IContentService sender, CopyEventArgs<IContent> e)
        {
        }

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

        private void DataTypeService_Saved(IDataTypeService sender, SaveEventArgs<IDataType> e)
        {
            foreach (var entity in e.SavedEntities)
                _distributedCache.RefreshDataTypeCache(entity);
        }

        private void DataTypeService_Deleted(IDataTypeService sender, DeleteEventArgs<IDataType> e)
        {
            foreach (var entity in e.DeletedEntities)
                _distributedCache.RemoveDataTypeCache(entity);
        }

        #endregion

        #region DomainService

        private void DomainService_Saved(IDomainService sender, SaveEventArgs<IDomain> e)
        {
            foreach (var entity in e.SavedEntities)
                _distributedCache.RefreshDomainCache(entity);
        }

        private void DomainService_Deleted(IDomainService sender, DeleteEventArgs<IDomain> e)
        {
            foreach (var entity in e.DeletedEntities)
                _distributedCache.RemoveDomainCache(entity);
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

        private void ContentTypeService_Changed(IContentTypeService sender, ContentTypeChange<IContentType>.EventArgs args)
        {
            _distributedCache.RefreshContentTypeCache(args.Changes.ToArray());
        }

        private void MediaTypeService_Changed(IMediaTypeService sender, ContentTypeChange<IMediaType>.EventArgs args)
        {
            _distributedCache.RefreshContentTypeCache(args.Changes.ToArray());
        }

        private void MemberTypeService_Changed(IMemberTypeService sender, ContentTypeChange<IMemberType>.EventArgs args)
        {
            _distributedCache.RefreshContentTypeCache(args.Changes.ToArray());
        }

        // TODO: our weird events handling wants this for now
        private void ContentTypeService_Saved(IContentTypeService sender, SaveEventArgs<IContentType> args) { }
        private void MediaTypeService_Saved(IMediaTypeService sender, SaveEventArgs<IMediaType> args) { }
        private void MemberTypeService_Saved(IMemberTypeService sender, SaveEventArgs<IMemberType> args) { }
        private void ContentTypeService_Deleted(IContentTypeService sender, DeleteEventArgs<IContentType> args) { }
        private void MediaTypeService_Deleted(IMediaTypeService sender, DeleteEventArgs<IMediaType> args) { }
        private void MemberTypeService_Deleted(IMemberTypeService sender, DeleteEventArgs<IMemberType> args) { }

        #endregion

        #region UserService

        private void UserService_SavedUser(IUserService sender, SaveEventArgs<IUser> e)
        {
            foreach (var entity in e.SavedEntities)
                _distributedCache.RefreshUserCache(entity.Id);
        }

        private void UserService_DeletedUser(IUserService sender, DeleteEventArgs<IUser> e)
        {
            foreach (var entity in e.DeletedEntities)
                _distributedCache.RemoveUserCache(entity.Id);
        }

        private void UserService_SavedUserGroup(IUserService sender, SaveEventArgs<UserGroupWithUsers> e)
        {
            foreach (var entity in e.SavedEntities)
                _distributedCache.RefreshUserGroupCache(entity.UserGroup.Id);
        }

        private void UserService_DeletedUserGroup(IUserService sender, DeleteEventArgs<IUserGroup> e)
        {

            foreach (var entity in e.DeletedEntities)
                _distributedCache.RemoveUserGroupCache(entity.Id);
        }

        #endregion

        #region FileService

        /// <summary>
        /// Removes cache for template
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileService_DeletedTemplate(IFileService sender, DeleteEventArgs<ITemplate> e)
        {
            foreach (var entity in e.DeletedEntities)
                _distributedCache.RemoveTemplateCache(entity.Id);
        }

        /// <summary>
        /// Refresh cache for template
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileService_SavedTemplate(IFileService sender, SaveEventArgs<ITemplate> e)
        {
            foreach (var entity in e.SavedEntities)
                _distributedCache.RefreshTemplateCache(entity.Id);
        }

        // TODO: our weird events handling wants this for now
        private void FileService_DeletedStylesheet(IFileService sender, DeleteEventArgs<IStylesheet> e) { }
        private void FileService_SavedStylesheet(IFileService sender, SaveEventArgs<IStylesheet> e) { }

        #endregion

        #region MacroService

        private void MacroService_Deleted(IMacroService sender, DeleteEventArgs<IMacro> e)
        {
            foreach (var entity in e.DeletedEntities)
                _distributedCache.RemoveMacroCache(entity);
        }

        private void MacroService_Saved(IMacroService sender, SaveEventArgs<IMacro> e)
        {
            foreach (var entity in e.SavedEntities)
                _distributedCache.RefreshMacroCache(entity);
        }

        #endregion

        #region MediaService

        private void MediaService_TreeChanged(IMediaService sender, TreeChange<IMedia>.EventArgs args)
        {
            _distributedCache.RefreshMediaCache(args.Changes.ToArray());
        }

        #endregion

        #region MemberService

        private void MemberService_Deleted(IMemberService sender, DeleteEventArgs<IMember> e)
        {
            _distributedCache.RemoveMemberCache(e.DeletedEntities.ToArray());
        }

        private void MemberService_Saved(IMemberService sender, SaveEventArgs<IMember> e)
        {
            _distributedCache.RefreshMemberCache(e.SavedEntities.ToArray());
        }

        #endregion

        #region MemberGroupService

        private void MemberGroupService_Deleted(IMemberGroupService sender, DeleteEventArgs<IMemberGroup> e)
        {
            foreach (var m in e.DeletedEntities.ToArray())
            {
                _distributedCache.RemoveMemberGroupCache(m.Id);
            }
        }

        private void MemberGroupService_Saved(IMemberGroupService sender, SaveEventArgs<IMemberGroup> e)
        {
            foreach (var m in e.SavedEntities.ToArray())
            {
                _distributedCache.RemoveMemberGroupCache(m.Id);
            }
        }

        #endregion

        #region RelationType

        private void RelationService_SavedRelationType(IRelationService sender, SaveEventArgs<IRelationType> args)
        {
            var dc = _distributedCache;
            foreach (var e in args.SavedEntities)
                dc.RefreshRelationTypeCache(e.Id);
        }

        private void RelationService_DeletedRelationType(IRelationService sender, DeleteEventArgs<IRelationType> args)
        {
            var dc = _distributedCache;
            foreach (var e in args.DeletedEntities)
                dc.RemoveRelationTypeCache(e.Id);
        }

        #endregion
    }
}
