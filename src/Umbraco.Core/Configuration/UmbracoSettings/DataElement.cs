using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class DataElement : UmbracoConfigurationElement, IDataSection
    {
        [ConfigurationProperty("SQLRetryPolicy")]
        internal InnerTextConfigurationElement<SQLRetryPolicyBehaviour> SQLRetryPolicy
        {
            get { return GetOptionalTextElement("SQLRetryPolicy", SQLRetryPolicyBehaviour.Default); }
        }

        public SQLRetryPolicyBehaviour SQLRetryPolicyBehaviour
        {
            get { return SQLRetryPolicy; }
        }
    }
}