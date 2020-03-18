using Umbraco.Core.Composing;
using Umbraco.Web.HealthCheck.NotificationMethods;

namespace Umbraco.Web.HealthCheck
{
    public class HealthCheckNotificationMethodCollectionBuilder : LazyCollectionBuilderBase<HealthCheckNotificationMethodCollectionBuilder, HealthCheckNotificationMethodCollection, IHealthCheckNotificationMethod>
    {
        protected override HealthCheckNotificationMethodCollectionBuilder This => this;
    }
}
