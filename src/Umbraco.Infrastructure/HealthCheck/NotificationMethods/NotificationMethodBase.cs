using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.HealthCheck;
using Umbraco.Core.HealthCheck.Checks;
using Umbraco.Infrastructure.HealthCheck;

namespace Umbraco.Web.HealthCheck.NotificationMethods
{
    public abstract class NotificationMethodBase : IHealthCheckNotificationMethod
    {
        protected NotificationMethodBase(IOptions<HealthChecksSettings> healthCheckSettings)
        {
            var type = GetType();
            var attribute = type.GetCustomAttribute<HealthCheckNotificationMethodAttribute>();
            if (attribute == null)
            {
                Enabled = false;
                return;
            }

            var notificationMethods = healthCheckSettings.Value.Notification.NotificationMethods;
            if(!notificationMethods.TryGetValue(attribute.Alias, out var notificationMethod))
            {
                Enabled = false;
                return;
            }

            Enabled = notificationMethod.Enabled;
            FailureOnly = notificationMethod.FailureOnly;
            Verbosity = notificationMethod.Verbosity;
            Settings = notificationMethod.Settings;
        }

        public bool Enabled {  get; protected set; }

        public bool FailureOnly { get; protected set; }

        public HealthCheckNotificationVerbosity Verbosity { get; protected set; }

        public IReadOnlyDictionary<string, INotificationMethodSettings> Settings { get; }

        protected bool ShouldSend(HealthCheckResults results)
        {
            return Enabled && (!FailureOnly || !results.AllChecksSuccessful);
        }

        public abstract Task SendAsync(HealthCheckResults results);
    }
}
