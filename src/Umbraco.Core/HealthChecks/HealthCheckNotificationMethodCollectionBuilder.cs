using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.HealthChecks.NotificationMethods;

namespace Umbraco.Cms.Core.HealthChecks;

public class HealthCheckNotificationMethodCollectionBuilder : LazyCollectionBuilderBase<
    HealthCheckNotificationMethodCollectionBuilder, HealthCheckNotificationMethodCollection,
    IHealthCheckNotificationMethod>
{
    protected override HealthCheckNotificationMethodCollectionBuilder This => this;
}
