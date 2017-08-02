using Umbraco.Core.Configuration.HealthChecks;

namespace Umbraco.Web.HealthCheck.NotificationMethods
{
    public abstract class NotificationMethodBase
    {
        protected NotificationMethodBase(bool enabled, bool failureOnly, HealthCheckNotificationVerbosity verbosity)
        {
            Enabled = enabled;
            FailureOnly = failureOnly;
            Verbosity = verbosity;
        }

        public bool Enabled {  get; protected set; }

        public bool FailureOnly { get; protected set; }

        public HealthCheckNotificationVerbosity Verbosity { get; protected set; }

        protected bool ShouldSend(HealthCheckResults results)
        {
            if (Enabled == false)
            {
                return false;
            }

            if (FailureOnly && results.AllChecksSuccessful)
            {
                return false;
            }

            return true;
        }
    }
}
