using System.Collections.Generic;
using System.Configuration;
using Umbraco.Core.Configuration.HealthChecks;

namespace Umbraco.Core.Configuration.Legacy
{
    public class HealthChecksSettings : IHealthChecksSettings
    {
        private HealthChecksSection _healthChecksSection;

        private HealthChecksSection HealthChecksSection
        {
            get
            {
                if (_healthChecksSection is null)
                {
                    _healthChecksSection = ConfigurationManager.GetSection("umbracoConfiguration/HealthChecks") as HealthChecksSection;
                }
                return _healthChecksSection;
            }
        }

        public IEnumerable<IDisabledHealthCheck> DisabledChecks => HealthChecksSection.DisabledChecks;
        public IHealthCheckNotificationSettings NotificationSettings => HealthChecksSection.NotificationSettings;
    }
}
