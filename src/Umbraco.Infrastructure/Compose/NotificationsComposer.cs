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
                .AddNotificationHandler<SavedNotification<IContent>, UserNotificationsHandler>()
                .AddNotificationHandler<SortedNotification<IContent>, UserNotificationsHandler>()
                .AddNotificationHandler<PublishedNotification<IContent>, UserNotificationsHandler>()
                .AddNotificationHandler<MovedNotification<IContent>, UserNotificationsHandler>()
                .AddNotificationHandler<MovedToRecycleBinNotification<IContent>, UserNotificationsHandler>()
                .AddNotificationHandler<CopiedNotification<IContent>, UserNotificationsHandler>()
                .AddNotificationHandler<RolledBackNotification<IContent>, UserNotificationsHandler>()
                .AddNotificationHandler<SentToPublishNotification<IContent>, UserNotificationsHandler>()
                .AddNotificationHandler<UnpublishedNotification<IContent>, UserNotificationsHandler>()
                .AddNotificationHandler<AssignedUserGroupPermissionsNotification, UserNotificationsHandler>()
                .AddNotificationHandler<SavedNotification<PublicAccessEntry>, UserNotificationsHandler>();

            // add handlers for building content relations
            builder
                .AddNotificationHandler<CopiedNotification<IContent>, RelateOnCopyNotifcationHandler>()
                .AddNotificationHandler<MovedNotification<IContent>, RelateOnTrashNotificationHandler>()
                .AddNotificationHandler<MovedToRecycleBinNotification<IContent>, RelateOnTrashNotificationHandler>()
                .AddNotificationHandler<MovedNotification<IMedia>, RelateOnTrashNotificationHandler>()
                .AddNotificationHandler<MovedToRecycleBinNotification<IMedia>, RelateOnTrashNotificationHandler>();

            // add notification handlers for property editors
            builder
                .AddNotificationHandler<SavingNotification<IContent>, BlockEditorPropertyHandler>()
                .AddNotificationHandler<CopyingNotification<IContent>, BlockEditorPropertyHandler>()
                .AddNotificationHandler<SavingNotification<IContent>, NestedContentPropertyHandler>()
                .AddNotificationHandler<CopyingNotification<IContent>, NestedContentPropertyHandler>()
                .AddNotificationHandler<CopiedNotification<IContent>, FileUploadPropertyEditor>()
                .AddNotificationHandler<DeletedNotification<IContent>, FileUploadPropertyEditor>()
                .AddNotificationHandler<DeletedNotification<IMedia>, FileUploadPropertyEditor>()
                .AddNotificationHandler<SavingNotification<IMedia>, FileUploadPropertyEditor>()
                .AddNotificationHandler<DeletedNotification<IMember>, FileUploadPropertyEditor>()
                .AddNotificationHandler<CopiedNotification<IContent>, ImageCropperPropertyEditor>()
                .AddNotificationHandler<DeletedNotification<IContent>, ImageCropperPropertyEditor>()
                .AddNotificationHandler<DeletedNotification<IMedia>, ImageCropperPropertyEditor>()
                .AddNotificationHandler<SavingNotification<IMedia>, ImageCropperPropertyEditor>()
                .AddNotificationHandler<DeletedNotification<IMember>, ImageCropperPropertyEditor>();

            // add notification handlers for redirect tracking
            builder
                .AddNotificationHandler<PublishingNotification<IContent>, RedirectTrackingHandler>()
                .AddNotificationHandler<PublishedNotification<IContent>, RedirectTrackingHandler>()
                .AddNotificationHandler<MovingNotification<IContent>, RedirectTrackingHandler>()
                .AddNotificationHandler<MovedNotification<IContent>, RedirectTrackingHandler>();

            // add notification handlers for auditing
            builder
                .AddNotificationHandler<SavedNotification<IMember>, AuditNotificationsHandler>()
                .AddNotificationHandler<DeletedNotification<IMember>, AuditNotificationsHandler>()
                .AddNotificationHandler<AssignedMemberRolesNotification, AuditNotificationsHandler>()
                .AddNotificationHandler<RemovedMemberRolesNotification, AuditNotificationsHandler>()
                .AddNotificationHandler<ExportedMemberNotification, AuditNotificationsHandler>()
                .AddNotificationHandler<SavedNotification<IUser>, AuditNotificationsHandler>()
                .AddNotificationHandler<DeletedNotification<IUser>, AuditNotificationsHandler>()
                .AddNotificationHandler<SavedNotification<UserGroupWithUsers>, AuditNotificationsHandler>()
                .AddNotificationHandler<AssignedUserGroupPermissionsNotification, AuditNotificationsHandler>();

            // add notifications handlers for distributed cache
            builder
                .AddNotificationHandler<SavedNotification<IMember>, DistributedCacheHandler>()
                .AddNotificationHandler<DeletedNotification<IMember>, DistributedCacheHandler>()
                .AddNotificationHandler<SavedNotification<IUser>, DistributedCacheHandler>()
                .AddNotificationHandler<DeletedNotification<IUser>, DistributedCacheHandler>()
                .AddNotificationHandler<SavedNotification<IUserGroup>, DistributedCacheHandler>()
                .AddNotificationHandler<DeletedNotification<IUserGroup>, DistributedCacheHandler>()
                .AddNotificationHandler<SavedNotification<PublicAccessEntry>, DistributedCacheHandler>()
                .AddNotificationHandler<DeletedNotification<PublicAccessEntry>, DistributedCacheHandler>();
        }
    }
}
