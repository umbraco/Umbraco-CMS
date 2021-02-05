using Umbraco.Core.Composing;
using Umbraco.Core.HealthChecks.NotificationMethods;

namespace Umbraco.Core.HealthChecks
{
    public class HealthCheckNotificationMethodCollectionBuilder : LazyCollectionBuilderBase<HealthCheckNotificationMethodCollectionBuilder, HealthCheckNotificationMethodCollection, IHealthCheckNotificationMethod>
    {
        protected override HealthCheckNotificationMethodCollectionBuilder This => this;
    }
}
