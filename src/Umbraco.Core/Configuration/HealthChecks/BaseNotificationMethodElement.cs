using System.Configuration;

namespace Umbraco.Core.Configuration.HealthChecks
{
    public abstract class BaseNotificationMethodElement : ConfigurationElement
    {
        private const string VERBOSITY_KEY = "verbosity";
        private const string FAILUREONLY_KEY = "failureOnly";

        [ConfigurationProperty(VERBOSITY_KEY, IsRequired = true)]
        public HealthCheckNotificationVerbosity Verbosity
        {
            get
            {
                return ((HealthCheckNotificationVerbosity)(base[VERBOSITY_KEY]));
            }
        }

        [ConfigurationProperty(FAILUREONLY_KEY, IsRequired = true)]
        public bool FailureOnly
        {
            get
            {
                return ((bool)(base[FAILUREONLY_KEY]));
            }
        }
    }
}
