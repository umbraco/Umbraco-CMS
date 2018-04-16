using System.Collections.Generic;
using Umbraco.Core.Composing;
using Umbraco.Web.HealthCheck.NotificationMethods;

namespace Umbraco.Web.HealthCheck
{
    public class HealthCheckNotificationMethodCollection : BuilderCollectionBase<IHealthCheckNotificationMethod>
    {
        public HealthCheckNotificationMethodCollection(IEnumerable<IHealthCheckNotificationMethod> items)
            : base(items)
        { }
    }
}
