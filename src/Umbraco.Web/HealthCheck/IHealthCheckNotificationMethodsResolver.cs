using System.Collections.Generic;
using Umbraco.Web.HealthCheck.NotificationMethods;

namespace Umbraco.Web.HealthCheck
{
    public interface IHealthCheckNotificationMethodsResolver
    {
        IEnumerable<IHealthCheckNotificatationMethod> NotificationMethods { get; }
    }
}