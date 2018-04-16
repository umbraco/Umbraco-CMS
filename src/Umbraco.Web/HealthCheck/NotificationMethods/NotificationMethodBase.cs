using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.HealthChecks;

namespace Umbraco.Web.HealthCheck.NotificationMethods
{
    public abstract class NotificationMethodBase : IHealthCheckNotificationMethod
    {
        protected NotificationMethodBase()
        {
            var type = GetType();
            var attribute = type.GetCustomAttribute<HealthCheckNotificationMethodAttribute>();
            if (attribute == null)
            {
                Enabled = false;
                return;
            }

            var healthCheckConfig = UmbracoConfig.For.HealthCheck();
            var notificationMethods = healthCheckConfig.NotificationSettings.NotificationMethods;
            var notificationMethod = notificationMethods[attribute.Alias];
            if (notificationMethod == null)
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

        public abstract Task SendAsync(HealthCheckResults results, CancellationToken token);
    }
}
