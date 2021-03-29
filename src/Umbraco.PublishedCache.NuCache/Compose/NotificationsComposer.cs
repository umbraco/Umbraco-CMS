using Umbraco.Cms.Core.Compose;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Services.Notifications;

namespace Umbraco.Cms.Infrastructure.PublishedCache.Compose
{
    public sealed class NotificationsComposer : ComponentComposer<NotificationsComponent>, ICoreComposer
    {
        public override void Compose(IUmbracoBuilder builder)
        {
            base.Compose(builder);

            builder.AddNotificationHandler<LanguageSavedNotification, PublishedSnapshotServiceEventHandler>();
        }
    }
}
