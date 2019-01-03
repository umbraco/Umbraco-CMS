using Umbraco.Core.Components;

namespace Umbraco.Web.Components
{
    public sealed class BackOfficeUserAuditEventsComposer : ICoreComposer
    {
        public void Compose(Composition composition)
        {
            composition.Components().Append<BackOfficeUserAuditEventsComponent>();
        }
    }
}
