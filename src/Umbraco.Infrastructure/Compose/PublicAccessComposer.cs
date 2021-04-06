using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Services.Notifications;

namespace Umbraco.Cms.Core.Compose
{
    /// <summary>
    /// Used to ensure that the public access data file is kept up to date properly
    /// </summary>
    public sealed class PublicAccessComposer : ICoreComposer
    {
        public void Compose(IUmbracoBuilder builder) =>
            builder
                .AddNotificationHandler<MemberGroupSavedNotification, PublicAccessHandler>()
                .AddNotificationHandler<MemberGroupDeletedNotification, PublicAccessHandler>();
    }
}
