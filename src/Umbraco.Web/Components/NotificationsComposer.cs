using Umbraco.Core;
using Umbraco.Core.Components;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Components
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public sealed class NotificationsComposer : ICoreComposer
    {
        public void Compose(Composition composition)
        {
            composition.Components().Append<NotificationsComponent>();
            composition.RegisterUnique<NotificationsComponent.Notifier>();
        }
    }
}
