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
                .AddNotificationHandler<SavedNotification<IContent>, UserNotificationsHandler>()
                .AddNotificationHandler<SortedNotification<IContent>, UserNotificationsHandler>()
                .AddNotificationHandler<PublishedNotification<IContent>, UserNotificationsHandler>()
                .AddNotificationHandler<MovedNotification<IContent>, UserNotificationsHandler>()
                .AddNotificationHandler<MovedToRecycleBinNotification<IContent>, UserNotificationsHandler>()
                .AddNotificationHandler<CopiedNotification<IContent>, UserNotificationsHandler>()
                .AddNotificationHandler<RolledBackNotification<IContent>, UserNotificationsHandler>()
                .AddNotificationHandler<SentToPublishNotification<IContent>, UserNotificationsHandler>()
                .AddNotificationHandler<UnpublishedNotification<IContent>, UserNotificationsHandler>();

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
                .AddNotificationHandler<CopiedNotification<IContent>, ImageCropperPropertyEditor>()
                .AddNotificationHandler<DeletedNotification<IContent>, ImageCropperPropertyEditor>()
                .AddNotificationHandler<DeletedNotification<IMedia>, ImageCropperPropertyEditor>()
                .AddNotificationHandler<SavingNotification<IMedia>, ImageCropperPropertyEditor>();

            // add notification handlers for redirect tracking
            builder
                .AddNotificationHandler<PublishingNotification<IContent>, RedirectTrackingHandler>()
                .AddNotificationHandler<PublishedNotification<IContent>, RedirectTrackingHandler>()
                .AddNotificationHandler<MovingNotification<IContent>, RedirectTrackingHandler>()
                .AddNotificationHandler<MovedNotification<IContent>, RedirectTrackingHandler>();
        }
    }
}
