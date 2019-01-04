namespace Umbraco.Core.Components
{
    public sealed class AuditEventsComposer : ICoreComposer
    {
        public void Compose(Composition composition)
        {
            composition.Components().Append<AuditEventsComponent>();
        }
    }
}
