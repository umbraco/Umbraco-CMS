using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Compose
{
    public sealed class NotificationsComposer : ComponentComposer<NotificationsComponent>, ICoreComposer
    {
        public override void Compose(IUmbracoBuilder builder)
        {
            base.Compose(builder);

            builder.Services.AddUnique<NotificationsComponent.Notifier>();
        }
    }
}
