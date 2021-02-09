using System.Collections.Generic;
using Umbraco.Core.Composing;
using Umbraco.Core.HealthChecks.NotificationMethods;

namespace Umbraco.Core.HealthChecks
{
    public class HealthCheckNotificationMethodCollection : BuilderCollectionBase<IHealthCheckNotificationMethod>
    {
        public HealthCheckNotificationMethodCollection(IEnumerable<IHealthCheckNotificationMethod> items)
            : base(items)
        { }
    }
}
