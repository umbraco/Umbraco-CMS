using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.HealthChecks.NotificationMethods;

namespace Umbraco.Cms.Core.HealthChecks;

/// <summary>
///     Builds a collection of <see cref="IHealthCheckNotificationMethod" /> instances for dependency injection.
/// </summary>
public class HealthCheckNotificationMethodCollectionBuilder : LazyCollectionBuilderBase<
    HealthCheckNotificationMethodCollectionBuilder, HealthCheckNotificationMethodCollection,
    IHealthCheckNotificationMethod>
{
    /// <inheritdoc />
    protected override HealthCheckNotificationMethodCollectionBuilder This => this;
}
