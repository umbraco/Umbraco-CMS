using System.Collections.Generic;
using Umbraco.Core.HealthCheck;

namespace Umbraco.Core.Configuration.Models
{
    public class HealthCheckNotificationMethodSettings
    {
        public bool Enabled { get; set; } = false;

        public HealthCheckNotificationVerbosity Verbosity { get; set; } = HealthCheckNotificationVerbosity.Summary;

        public bool FailureOnly { get; set; } = false;

        public IDictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();
    }
}
