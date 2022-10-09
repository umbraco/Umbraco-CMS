using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.HealthChecks;

public class
    HealthCheckCollectionBuilder : LazyCollectionBuilderBase<HealthCheckCollectionBuilder, HealthCheckCollection,
        HealthCheck>
{
    protected override HealthCheckCollectionBuilder This => this;

    // note: in v7 they were per-request, not sure why?
    // the collection is injected into the controller & there's only 1 controller per request anyways
    protected override ServiceLifetime CollectionLifetime => ServiceLifetime.Transient; // transient!
}
