using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services.Notifications;

namespace Umbraco.Cms.Infrastructure.PublishedCache.Compose
{
    public sealed class NotificationsComposer : ICoreComposer
    {
        public void Compose(IUmbracoBuilder builder) =>
            builder
                .AddNotificationHandler<LanguageSavedNotification, PublishedSnapshotServiceEventHandler>()
                .AddNotificationHandler<ContentDeletingNotification, PublishedSnapshotServiceEventHandler>()
                .AddNotificationHandler<MediaDeletingNotification, PublishedSnapshotServiceEventHandler>()
                .AddNotificationHandler<MemberDeletingNotification, PublishedSnapshotServiceEventHandler>()
                .AddNotificationHandler<ContentEmptyingRecycleBinNotification, PublishedSnapshotServiceEventHandler>()
                .AddNotificationHandler<MediaEmptyingRecycleBinNotification, PublishedSnapshotServiceEventHandler>()
                .AddNotificationHandler<ContentRefreshNotification, PublishedSnapshotServiceEventHandler>()
                .AddNotificationHandler<MediaRefreshNotification, PublishedSnapshotServiceEventHandler>()
                .AddNotificationHandler<MemberRefreshNotification, PublishedSnapshotServiceEventHandler>()
            ;
    }
}
