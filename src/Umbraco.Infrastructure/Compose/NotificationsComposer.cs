using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Compose
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public sealed class NotificationsComposer : ComponentComposer<NotificationsComponent>, ICoreComposer
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            composition.RegisterUnique<NotificationsComponent.Notifier>();
        }
    }
}
