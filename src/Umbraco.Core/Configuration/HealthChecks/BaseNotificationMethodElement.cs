using System.Configuration;

namespace Umbraco.Core.Configuration.HealthChecks
{
    public abstract class BaseNotificationMethodElement : ConfigurationElement
    {
        private const string VERBOSITY_KEY = "verbosity";

        [ConfigurationProperty(VERBOSITY_KEY, IsRequired = true)]
        public HealthCheckNotificationVerbosity Verbosity
        {
            get
            {
                return ((HealthCheckNotificationVerbosity)(base[VERBOSITY_KEY]));
            }
        }
    }
}
