using Umbraco.Core;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Compose
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
