using LightInject;
using Umbraco.Core.DependencyInjection;

namespace Umbraco.Web.HealthCheck
{
    public class HealthCheckCollectionBuilder : LazyCollectionBuilderBase<HealthCheckCollectionBuilder, HealthCheckCollection, HealthCheck>
    {
        public HealthCheckCollectionBuilder(IServiceContainer container)
            : base(container)
        { }

        protected override HealthCheckCollectionBuilder This => this;

        protected override ILifetime CollectionLifetime => null; // transient!
    }
}
