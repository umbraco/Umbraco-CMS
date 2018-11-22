using LightInject;
using Umbraco.Core.Composing;
using Umbraco.Web.HealthCheck.NotificationMethods;

namespace Umbraco.Web.HealthCheck
{
    internal class HealthCheckNotificationMethodCollectionBuilder : LazyCollectionBuilderBase<HealthCheckNotificationMethodCollectionBuilder, HealthCheckNotificationMethodCollection, IHealthCheckNotificationMethod>
    {
        public HealthCheckNotificationMethodCollectionBuilder(IServiceContainer container)
            : base(container)
        { }

        protected override HealthCheckNotificationMethodCollectionBuilder This => this;
    }
}
