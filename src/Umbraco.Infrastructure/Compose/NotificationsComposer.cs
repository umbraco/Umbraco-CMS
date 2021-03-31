// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Infrastructure.Services.Notifications;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Compose
{
    public sealed class NotificationsComposer : ICoreComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            // add handlers for sending user notifications (i.e. emails)
            builder.Services.AddUnique<UserNotificationsHandler.Notifier>();
            builder
                .AddNotificationHandler<ContentSavedNotification, UserNotificationsHandler>()
                .AddNotificationHandler<ContentSortedNotification, UserNotificationsHandler>()
                .AddNotificationHandler<ContentPublishedNotification, UserNotificationsHandler>()
                .AddNotificationHandler<ContentMovedNotification, UserNotificationsHandler>()
                .AddNotificationHandler<ContentMovedToRecycleBinNotification, UserNotificationsHandler>()
                .AddNotificationHandler<ContentCopiedNotification, UserNotificationsHandler>()
                .AddNotificationHandler<ContentRolledBackNotification, UserNotificationsHandler>()
                .AddNotificationHandler<ContentSentToPublishNotification, UserNotificationsHandler>()
                .AddNotificationHandler<ContentUnpublishedNotification, UserNotificationsHandler>()
                .AddNotificationHandler<AssignedUserGroupPermissionsNotification, UserNotificationsHandler>()
                .AddNotificationHandler<PublicAccessEntrySavedNotification, UserNotificationsHandler>();

            // add handlers for building content relations
            builder
                .AddNotificationHandler<ContentCopiedNotification, RelateOnCopyNotificationHandler>()
                .AddNotificationHandler<ContentMovedNotification, RelateOnTrashNotificationHandler>()
                .AddNotificationHandler<ContentMovedToRecycleBinNotification, RelateOnTrashNotificationHandler>()
                .AddNotificationHandler<MediaMovedNotification, RelateOnTrashNotificationHandler>()
                .AddNotificationHandler<MediaMovedToRecycleBinNotification, RelateOnTrashNotificationHandler>();

            // add notification handlers for property editors
            builder
                .AddNotificationHandler<ContentSavingNotification, BlockEditorPropertyHandler>()
                .AddNotificationHandler<ContentCopyingNotification, BlockEditorPropertyHandler>()
                .AddNotificationHandler<ContentSavingNotification, NestedContentPropertyHandler>()
                .AddNotificationHandler<ContentCopyingNotification, NestedContentPropertyHandler>()
                .AddNotificationHandler<ContentCopiedNotification, FileUploadPropertyEditor>()
                .AddNotificationHandler<ContentDeletedNotification, FileUploadPropertyEditor>()
                .AddNotificationHandler<MediaDeletedNotification, FileUploadPropertyEditor>()
                .AddNotificationHandler<MediaSavingNotification, FileUploadPropertyEditor>()
                .AddNotificationHandler<MemberDeletedNotification, FileUploadPropertyEditor>()
                .AddNotificationHandler<ContentCopiedNotification, ImageCropperPropertyEditor>()
                .AddNotificationHandler<ContentDeletedNotification, ImageCropperPropertyEditor>()
                .AddNotificationHandler<MediaDeletedNotification, ImageCropperPropertyEditor>()
                .AddNotificationHandler<MediaSavingNotification, ImageCropperPropertyEditor>()
                .AddNotificationHandler<MemberDeletedNotification, ImageCropperPropertyEditor>();

            // add notification handlers for redirect tracking
            builder
                .AddNotificationHandler<ContentPublishingNotification, RedirectTrackingHandler>()
                .AddNotificationHandler<ContentPublishedNotification, RedirectTrackingHandler>()
                .AddNotificationHandler<ContentMovingNotification, RedirectTrackingHandler>()
                .AddNotificationHandler<ContentMovedNotification, RedirectTrackingHandler>();

            // Add notification handlers for DistributedCache
            builder
                .AddNotificationHandler<DictionaryItemDeletedNotification, DistributedCacheBinder>()
                .AddNotificationHandler<DictionaryItemSavedNotification, DistributedCacheBinder>()
                .AddNotificationHandler<LanguageSavedNotification, DistributedCacheBinder>()
                .AddNotificationHandler<LanguageDeletedNotification, DistributedCacheBinder>()
                .AddNotificationHandler<MemberSavedNotification, DistributedCacheBinder>()
                .AddNotificationHandler<MemberDeletedNotification, DistributedCacheBinder>()
                .AddNotificationHandler<PublicAccessEntrySavedNotification, DistributedCacheBinder>()
                .AddNotificationHandler<PublicAccessEntryDeletedNotification, DistributedCacheBinder>()
                .AddNotificationHandler<UserSavedNotification, DistributedCacheBinder>()
                .AddNotificationHandler<UserDeletedNotification, DistributedCacheBinder>()
                .AddNotificationHandler<UserGroupWithUsersSavedNotification, DistributedCacheBinder>()
                .AddNotificationHandler<UserGroupDeletedNotification, DistributedCacheBinder>()
                .AddNotificationHandler<MemberGroupDeletedNotification, DistributedCacheBinder>()
                .AddNotificationHandler<MemberGroupSavedNotification, DistributedCacheBinder>()
                .AddNotificationHandler<DataTypeDeletedNotification, DistributedCacheBinder>()
                .AddNotificationHandler<DataTypeSavedNotification, DistributedCacheBinder>()
                .AddNotificationHandler<TemplateDeletedNotification, DistributedCacheBinder>()
                .AddNotificationHandler<TemplateSavedNotification, DistributedCacheBinder>();

            // add notification handlers for auditing
            builder
                .AddNotificationHandler<MemberSavedNotification, AuditNotificationsHandler>()
                .AddNotificationHandler<MemberDeletedNotification, AuditNotificationsHandler>()
                .AddNotificationHandler<AssignedMemberRolesNotification, AuditNotificationsHandler>()
                .AddNotificationHandler<RemovedMemberRolesNotification, AuditNotificationsHandler>()
                .AddNotificationHandler<ExportedMemberNotification, AuditNotificationsHandler>()
                .AddNotificationHandler<UserSavedNotification, AuditNotificationsHandler>()
                .AddNotificationHandler<UserDeletedNotification, AuditNotificationsHandler>()
                .AddNotificationHandler<UserGroupWithUsersSavedNotification, AuditNotificationsHandler>()
                .AddNotificationHandler<AssignedUserGroupPermissionsNotification, AuditNotificationsHandler>();
        }
    }
}
