using Umbraco.Core;
using Umbraco.Core.Components;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Components
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
