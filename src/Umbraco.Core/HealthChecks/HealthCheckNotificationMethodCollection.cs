using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.HealthChecks.NotificationMethods;

namespace Umbraco.Cms.Core.HealthChecks
{
    public class HealthCheckNotificationMethodCollection : BuilderCollectionBase<IHealthCheckNotificationMethod>
    {
        public HealthCheckNotificationMethodCollection(IEnumerable<IHealthCheckNotificationMethod> items)
            : base(items)
        { }
    }
}
