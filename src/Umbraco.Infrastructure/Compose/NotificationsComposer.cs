// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Infrastructure.Services.Notifications;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Compose
{
    public sealed class NotificationsComposer : ComponentComposer<NotificationsComponent>, ICoreComposer
    {
        public override void Compose(IUmbracoBuilder builder)
        {
            base.Compose(builder);

            builder.Services.AddUnique<NotificationsComponent.Notifier>();

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
                .AddNotificationHandler<ContentUnpublishedNotification, UserNotificationsHandler>();

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
                .AddNotificationHandler<ContentCopiedNotification, ImageCropperPropertyEditor>()
                .AddNotificationHandler<ContentDeletedNotification, ImageCropperPropertyEditor>()
                .AddNotificationHandler<MediaDeletedNotification, ImageCropperPropertyEditor>()
                .AddNotificationHandler<MediaSavingNotification, ImageCropperPropertyEditor>();

            // add notification handlers for redirect tracking
            builder
                .AddNotificationHandler<ContentPublishingNotification, RedirectTrackingHandler>()
                .AddNotificationHandler<ContentPublishedNotification, RedirectTrackingHandler>()
                .AddNotificationHandler<ContentMovingNotification, RedirectTrackingHandler>()
                .AddNotificationHandler<ContentMovedNotification, RedirectTrackingHandler>();
        }
    }
}
