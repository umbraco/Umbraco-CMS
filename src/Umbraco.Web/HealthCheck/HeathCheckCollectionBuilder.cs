using LightInject;
using Umbraco.Core.Composing;

namespace Umbraco.Web.HealthCheck
{
    public class HealthCheckCollectionBuilder : LazyCollectionBuilderBase<HealthCheckCollectionBuilder, HealthCheckCollection, HealthCheck>
    {
        public HealthCheckCollectionBuilder(IServiceContainer container)
            : base(container)
        { }

        protected override HealthCheckCollectionBuilder This => this;

        // note: in v7 they were per-request, not sure why?
        // the collection is injected into the controller & there's only 1 controller per request anyways
        protected override ILifetime CollectionLifetime => null; // transient!
    }
}
