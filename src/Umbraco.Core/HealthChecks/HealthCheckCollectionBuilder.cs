using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.HealthChecks;

/// <summary>
///     Builds a collection of <see cref="HealthCheck" /> instances for dependency injection.
/// </summary>
public class
    HealthCheckCollectionBuilder : LazyCollectionBuilderBase<HealthCheckCollectionBuilder, HealthCheckCollection,
        HealthCheck>
{
    /// <inheritdoc />
    protected override HealthCheckCollectionBuilder This => this;

    // note: in v7 they were per-request, not sure why?
    // the collection is injected into the controller & there's only 1 controller per request anyways
    /// <inheritdoc />
    protected override ServiceLifetime CollectionLifetime => ServiceLifetime.Transient; // transient!
}
