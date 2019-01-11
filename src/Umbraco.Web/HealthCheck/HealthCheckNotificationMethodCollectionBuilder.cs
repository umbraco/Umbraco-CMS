using Umbraco.Core.Composing;
using Umbraco.Web.HealthCheck.NotificationMethods;

namespace Umbraco.Web.HealthCheck
{
    internal class HealthCheckNotificationMethodCollectionBuilder : LazyCollectionBuilderBase<HealthCheckNotificationMethodCollectionBuilder, HealthCheckNotificationMethodCollection, IHealthCheckNotificationMethod>
    {
        protected override HealthCheckNotificationMethodCollectionBuilder This => this;
    }
}
